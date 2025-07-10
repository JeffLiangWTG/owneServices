using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.BatchProcessor;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSDEC;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using CUSDECMessage = Enterprise.Edifact.CA.D99B.Messages.CUSDEC.CUSDECMessage;
using SegmentGroup10 = Enterprise.Edifact.CA.D99B.Messages.CUSDEC.SegmentGroup10;
using SegmentGroup11 = Enterprise.Edifact.CA.D99B.Messages.CUSDEC.SegmentGroup11;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	class B3AsLodgedDocumentWrapper : IB3Header
	{
		internal B3AsLodgedDocumentWrapper(B3Message b3Message)
		{
			Argument.NotNull(b3Message, "b3Message");
			if (!b3Message.IsTransmitMessage)
			{
				throw new ArgumentException("B3 transmit message is supported only", nameof(b3Message));
			}

			if (!(b3Message.EM_LinkedObject is CusEntryHeader))
			{
				throw new ArgumentException("CusEntryHeader is supported only as linked object for B3 message");
			}

			linkedObject = (IEDIFACTMessageAttachee)b3Message.EM_LinkedObject;
			interchange = b3Message.Interchange;
			Argument.NotNull(interchange, "interchange");
			message = (CUSDECMessage)b3Message.GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet());
			Argument.NotNull(message, "message", "Supported EDIFACT message type is D99B CUSDEC");
			bgm = message.BGM.Count > 0 ? message.BGM[0] : new BGMSegment();
		}

		CusEntryHeader EntryHeader
		{
			get { return (CusEntryHeader)linkedObject; }
		}

		ZString IB3Header.MessageType => EntryHeader.Declaration?.JE_MessageType ?? ZString.Empty;

		#region Implementation of IEDIFACTMessageAttachee

		BusinessObjectFactory IEDIMessageCollectionProvider.Factory
		{
			get { return linkedObject.Factory; }
		}

		void IEDIFACTMessageAttachee.AddMessage(Enterprise.Messaging.Business.EDIMessage ediMessage)
		{
			linkedObject.AddMessage(ediMessage);
		}

		Enterprise.Messaging.Business.EDIMessageCollection IEDIMessageCollectionProvider.Messages
		{
			get { return linkedObject.Messages; }
		}

		ZString IEDIFACTMessageAttachee.MessageStatus
		{
			get { return linkedObject.MessageStatus; }
			set { linkedObject.MessageStatus = value; }
		}

		ZString IEDIFACTMessageAttachee.JobStatus
		{
			get { return linkedObject.JobStatus; }
			set { linkedObject.JobStatus = value; }
		}

		bool IEDIFACTMessageAttachee.HasChanges
		{
			get { return linkedObject.HasChanges; }
		}

		ZString IEDIFACTMessageAttachee.JobIdentification
		{
			get { return linkedObject.JobIdentification; }
		}

		BusinessObject IEDIFACTMessageAttachee.TopLevelBusinessObject
		{
			get { return linkedObject.TopLevelBusinessObject; }
		}

		bool ICAEDIFACTMessageAttachee.IsCancelled
		{
			get { return EntryHeader.Declaration.IsCancelled; }
		}

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage
		{
			get { return EntryHeader.Declaration.RefreshValidationBeforeSendMessage; }
		}

		#endregion

		#region Implementation of IB3Header

		void IB3Header.RefreshCachedValues()
		{
		}

		ZString IB3Header.BatchNumber
		{
			get { return bgm.DocumentMessageIdentification.DocumentMessageNumber; }
		}

		ZString IB3Header.B3TypeCode
		{
			get { return bgm.DocumentMessageName.DocumentName; }
		}

		ZString IB3Header.PaymentCode
		{
			get { return (from CSTSegment cst in message.CST select cst.CustomsIdentityCodes1.CustomsCodeIdentification).FirstOrDefault(); }
		}

		ZString IB3Header.CBSAOffice
		{
			get { return D99BMessageUtilities.GetLocation(message.LOC, LocationFunctionCodeQualifierList.CustomsOfficeOfEntry); }
		}

		ZString IB3Header.PortOfUnlading
		{
			get { return D99BMessageUtilities.GetLocation(message.LOC, LocationFunctionCodeQualifierList.PlacePortOfDischarge); }
		}

		ZString IB3Header.WarehouseNumber
		{
			get { return D99BMessageUtilities.GetLocation(message.LOC, LocationFunctionCodeQualifierList.Warehouse); }
		}

		ZString IB3Header.TransactionNumber
		{
			get { return D99BMessageUtilities.GetReference(message.Group1, ReferenceFunctionCodeQualifierList.TransactionReferenceNumber); }
		}

		ZString IB3Header.BusinessNumber
		{
			get { return D99BMessageUtilities.GetReference(message.Group1, ReferenceFunctionCodeQualifierList.NationalGovernmentBusinessIdentificationNumber); }
		}

		ZString IB3Header.GSTNumber
		{
			get { return D99BMessageUtilities.GetReference(message.Group1, ReferenceFunctionCodeQualifierList.GovernmentAgencyReferenceNumber); }
		}

		ZString IB3Header.TransportMode
		{
			get { return D99BMessageUtilities.GetTransportMode(message.Group4, TransportStageCodeQualifierList.AtBorder); }
		}

		ZString IB3Header.CarrierCodeAtImportation
		{
			get { return D99BMessageUtilities.GetCarrierCode(message.Group4, TransportStageCodeQualifierList.AtBorder); }
		}

		IEnumerable<IB3BRelease> IB3Header.B3BInputReleases
		{
			get
			{
				if (message.Group5.Count > 0)
				{
					var group5 = message.Group5[0];
					var result = (from DOCSegment doc in group5.DOC
								  where doc.DocumentMessageName.DocumentNameCode == DocumentNameCodeList.CargoManifest
								  select (IB3BRelease)new B3BRelease { CargoControlNumber = doc.DocumentMessageDetails.DocumentMessageNumber }).ToArray();

					var first = (B3BRelease)result[0];
					first.DateOfRelease = D99BMessageUtilities.GetDate(group5.DTM, DateTimePeriodFunctionCodeQualifierList.ReleaseDateCustoms);
					return result;
				}
				return Array.Empty<IB3BRelease>();
			}
		}

		ZDecimal IB3Header.TotalValueForDuty
		{
			get
			{
				return (from SegmentGroup8 group8 in message.Group8
						select D99BMessageUtilities.GetAmount(group8.MOA, MonetaryAmountTypeCodeQualifierList.DeclaredTotalCustomsValue, 0)).FirstOrDefault();
			}
		}

		IEnumerable<IB3SubHeader> IB3Header.PositiveB3SubHeaders
		{
			get { return from SegmentGroup10 group10 in message.Group10 select (IB3SubHeader)new B3SubHeader(group10, EntryHeader, this); }
		}

		IEnumerable<IClassificationLine1> IB3Header.PositiveClassificationLines
		{
			get { return from SegmentGroup30 group30 in message.Group30 select (IClassificationLine1)new ClassificationLine1(group30, EntryHeader); }
		}

		IEnumerable<IB3SubHeader> IB3Header.NegativeB3SubHeaders => Enumerable.Empty<IB3SubHeader>();

		IEnumerable<IClassificationLine1> IB3Header.NegativeClassificationLines => Enumerable.Empty<IClassificationLine1>();

		ITotalAmounts IB3Header.PositiveTotalAmounts
		{
			get { return new TotalAmounts(message, true); }
		}

		ITotalAmounts IB3Header.NegativeTotalAmounts
		{
			get { return new TotalAmounts(message, false); }
		}

		bool IB3Header.IsCalculationsDone
		{
			get { return true; }
		}

		bool IB3Header.SumPosAndNeg => false;

		ZString IB3Header.B3Comments
		{
			get { return ZString.Empty; }
		}

		ZDateTime IB3Header.ReleaseDate
		{
			get { return ((IB3Header)this).B3BInputReleases.FirstOrDefault()?.DateOfRelease ?? ZDateTime.Empty; }
		}

		IDocAddress IB3Header.Importer
		{
			get
			{
				OrgAddress result = null;
				var businessNumber = ((IB3Header)this).BusinessNumber;
				if (!businessNumber.IsEmpty)
				{
					var loader = new OrgHeader.Loader(((IEDIFACTMessageAttachee)this).Factory);
					var orgs = loader.LoadDBOrganisations(Core.Constants.CountryCodes.Canada, OrgCusCode.CACodeTypes.BusinessNumberForImportExport, businessNumber)
								   ?? loader.LoadDBOrganisations(Core.Constants.CountryCodes.Canada, OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial, businessNumber);
					if (orgs.Length == 1)
					{
						result = orgs[0].MainAddress;
					}
					else if (orgs.Length > 1)
					{
						var importer = EntryHeader.Declaration.ImporterOfRecordAddress.HasRealOrganisation ? EntryHeader.Declaration.ImporterOfRecordAddress.Organisation : EntryHeader.Declaration.Importer;
						if (importer != null)
						{
							result = orgs.FirstOrDefault(x => x.PK == importer.PK)?.MainAddress;
						}
					}
				}
				return result;
			}
		}

		ZString IB3Header.AccountSecurityCode
		{
			get { return BatchProcessorUtilities.GetAccountSecurityCode(interchange.EI_InterchangeText); }
		}

		#endregion

		#region B3SubHeader

		class B3SubHeader : IB3SubHeader
		{
			internal B3SubHeader(SegmentGroup10 group10, CusEntryHeader entryHeader, IB3Header b3Header)
			{
				this.group10 = group10;
				this.b3Header = b3Header;
				this.entryHeader = entryHeader;
				group14 = group10.Group14.Count > 0 ? group10.Group14[0] : new SegmentGroup14();
				group15 = group14.Group15.Count > 0 ? group14.Group15[0] : new SegmentGroup15();
				group18 = group10.Group18.Count > 0 ? group10.Group18[0] : new SegmentGroup18();

				pat = (from PATSegment patSegment in group18.PAT
					   where patSegment.PaymentTermsTypeCodeQualifier == PaymentTermsTypeCodeQualifierList.Basic
					   select patSegment).FirstOrDefault() ?? new PATSegment();
			}

			#region Implementation of IB3SubHeader

			public ZInt B3SubHeaderNumber
			{
				get
				{
					return (from DMSSegment dms in group10.DMS
							select ZInt.ParseEmptyAsZero(dms.DocumentMessageIdentification.DocumentMessageNumber)).FirstOrDefault();
				}
			}

			ZDecimal IB3SubHeader.FreightCharges
			{
				get
				{
					return (from SegmentGroup11 group11 in group10.Group11
							select D99BMessageUtilities.GetAmount(group11.MOA, MonetaryAmountTypeCodeQualifierList.FreightCharge, 0)).FirstOrDefault();
				}
			}

			IDocAddress GetVendor(CusEntryHeader entryHeader, ZInt b3SubHeaderNumber)
			{
				foreach (JobComInvoiceLine invoiceLine in entryHeader.InvoiceLines)
				{
					if (invoiceLine.CA_B3SubHeaderNumber == b3SubHeaderNumber)
					{
						return ((IEDIInvoiceOGD)invoiceLine.InvoiceHeader)?.Vendor;
					}
				}

				return null;
			}

			VendorStateAndZipStruct IB3SubHeader.VendorStateAndZip
			{
				get { throw new NotImplementedException(); }
			}

			IDocAddress IB3SubHeader.Vendor
			{
				get
				{
					var asLodgedVendor = D99BMessageUtilities.GetAddress(group14.NAD, PartyFunctionCodeQualifierList.Seller);
					var currentDataVendor = GetVendor(entryHeader, B3SubHeaderNumber);
					return currentDataVendor != null &&
						asLodgedVendor.E2_CompanyName.Trim() == currentDataVendor.E2_CompanyName.Trim() &&
						(asLodgedVendor.E2_State.IsEmpty || asLodgedVendor.E2_State.Trim() == currentDataVendor.E2_State.Trim()) &&
						(asLodgedVendor.E2_Postcode.IsEmpty || asLodgedVendor.E2_Postcode.Trim() == currentDataVendor.E2_Postcode.Trim())
							? currentDataVendor : asLodgedVendor;
				}
			}

			IDocAddress IB3SubHeader.Exporter
			{
				get { return null; }
			}

			ZDateTime IB3SubHeader.DateOfDirectShipment
			{
				get { return D99BMessageUtilities.GetDate(group15.DTM, DateTimePeriodFunctionCodeQualifierList.ExportationDate); }
			}

			ZString IB3SubHeader.CountryOfOrigin
			{
				get { return D99BMessageUtilities.GetLocation(group15.LOC, LocationFunctionCodeQualifierList.CountryOfOrigin); }
			}

			ZString IB3SubHeader.PlaceOfExport
			{
				get { return D99BMessageUtilities.GetRelatedLocationOne(group15.LOC, LocationFunctionCodeQualifierList.CountryOfOrigin); }
			}

			ZString IB3SubHeader.USPortOfExit
			{
				get { return D99BMessageUtilities.GetRelatedLocationTwo(group15.LOC, LocationFunctionCodeQualifierList.CountryOfOrigin); }
			}

			ZString IB3SubHeader.TariffTreatmentCode
			{
				get { return pat.PaymentTerms_X.PaymentTermsDescription1; }
			}

			ZString IB3SubHeader.TimeLimitUnit
			{
				get { return pat.TermsTimeInformation_X.PeriodTypeCode.ToString(); }
			}

			ZInt IB3SubHeader.B3TimeLimits
			{
				get { return ZInt.ParseEmptyAsZero(pat.TermsTimeInformation_X.PeriodCountQuantity); }
			}

			ZString IB3SubHeader.CurrencyCode
			{
				get { return D99BMessageUtilities.GetCurrency(group18.MOA, MonetaryAmountTypeCodeQualifierList.AmountReferenceCurrency); }
			}

			IB3Header IB3SubHeader.B3Header
			{
				get { return this.b3Header; }
			}

			ZString IB3SubHeader.InvoiceNumber
			{
				get
				{
					foreach (JobComInvoiceLine invoiceLine in entryHeader.InvoiceLines)
					{
						if (invoiceLine.CA_B3SubHeaderNumber == B3SubHeaderNumber)
						{
							return invoiceLine.InvoiceHeader?.JZ_InvoiceNumber ?? ZString.Empty;
						}
					}

					return ZString.Empty;
				}
			}

			ZDecimal IB3SubHeader.ExchangeRate
			{
				get
				{
					foreach (JobComInvoiceLine invoiceLine in entryHeader.InvoiceLines)
					{
						if (invoiceLine.CA_B3SubHeaderNumber == B3SubHeaderNumber)
						{
							return invoiceLine.InvoiceHeader?.EffectiveExchangeRateForInvoiceCurr ?? ZDecimal.Zero;
						}
					}

					return ZDecimal.Zero;
				}
			}

			ZString IB3SubHeader.TradeZone
			{
				get
				{
					return ZString.Empty;
				}
			}

			#endregion

			readonly IB3Header b3Header;
			readonly CusEntryHeader entryHeader;
			readonly SegmentGroup10 group10;
			readonly SegmentGroup14 group14;
			readonly SegmentGroup15 group15;
			readonly SegmentGroup18 group18;
			readonly PATSegment pat;
		}

		#endregion

		#region ClassificationLine1

		class ClassificationLine1 : IClassificationLine1
		{
			public ClassificationLine1(SegmentGroup30 group30, CusEntryHeader entryHeader)
			{
				this.group30 = group30;
				this.entryHeader = entryHeader;
				cst = group30.CST.Count > 0 ? group30.CST[0] : new CSTSegment();
			}

			#region Implementation of IClassificationLine1

			public BusinessObjectFactory Factory => entryHeader.Factory;

			public ZShort B3LineNumber
			{
				get
				{
					ZShort lineNo = 0;
					ZShort.TryParse(cst.GoodsItemNumber, out lineNo);
					return lineNo;
				}
			}

			public ZShort SequenceNumber => ZShort.Zero;

			CusEntryLine EntryLine
			{
				get { return entryLine ?? (entryLine = (CusEntryLine)entryHeader.MergedLines.FindByLineNumber(B3LineNumber)); }
			}
			CusEntryLine entryLine;

			T GetValueFromEntryLine<T>(Func<IClassificationLine1, T> getValue) where T : IZType
			{
				var result = default(T);
				if (EntryLine is IClassificationLine1 classificationLine)
				{
					result = getValue(classificationLine);
				}
				return result;
			}

			ZString IClassificationLine1.RecordIdentifier
			{
				get { return cst.CustomsIdentityCodes1.CustomsCodeIdentification; }
			}

			ZInt IClassificationLine1.B3SubHeaderNumber
			{
				get { return ZInt.ParseEmptyAsZero(cst.CustomsIdentityCodes2.CustomsCodeIdentification); }
			}

			ZInt IClassificationLine1.B3SubHeaderNumberForLVX
			{
				get { return ZInt.Zero; }
			}

			ZString IClassificationLine1.ClassificationNumber
			{
				get { return cst.CustomsIdentityCodes3.CustomsCodeIdentification; }
			}

			ZString IClassificationLine1.ValueForDutyCode
			{
				get { return cst.CustomsIdentityCodes4.CustomsCodeIdentification; }
			}

			ZString IClassificationLine1.TariffCode
			{
				get { return cst.CustomsIdentityCodes5.CustomsCodeIdentification; }
			}

			ZDecimal IClassificationLine1.ValueForCurrency
			{
				get { return D99BMessageUtilities.GetAmount(group30.Group33, MonetaryAmountTypeCodeQualifierList.CustomsValue); }
			}

			ZDecimal IClassificationLine1.ValueForDuty
			{
				get { return D99BMessageUtilities.GetAmount(group30.Group33, MonetaryAmountTypeCodeQualifierList.DeclaredTotalCustomsValue); }
			}

			ZDecimal IClassificationLine1.ValueForTax
			{
				get { return D99BMessageUtilities.GetAmount(group30.Group33, MonetaryAmountTypeCodeQualifierList.TaxableAmount); }
			}

			ZString IClassificationLine1.AuthorityNumber
			{
				get { return D99BMessageUtilities.GetReference(group30.Group35, ReferenceFunctionCodeQualifierList.CustomsDecisionRequestNumber); }
			}

			ZString IClassificationLine1.TRSNumber
			{
				get { return D99BMessageUtilities.GetReference(group30.Group35, ReferenceFunctionCodeQualifierList.CustomsValuationDecisionNumber); }
			}

			ZString[] IClassificationLine1.PartNumberDescriptions
			{
				get
				{
					ZString result = D99BMessageUtilities.GetGINAsString(group30.Group35, ObjectIdentificationCodeQualifierList.PartNumber);
					if (result.IsEmpty && EntryLine != null)
					{
						result = Res.GetString("f642fdd2-b5bf-40ae-b604-2ec281e95033", "{0} (*)", entryLine.Description);
					}
					return new ZString[] { result };
				}
			}

			IEnumerable<IInvoiceCrossReference> IClassificationLine1.InvoiceCrossReferences
			{
				get { return Array.Empty<IInvoiceCrossReference>(); }
			}

			ZDecimal IClassificationLine1.CustomsQuantity => GetValueFromEntryLine((x) => x.CustomsQuantity);

			ZString IClassificationLine1.CustomsUnitQty => GetValueFromEntryLine((x) => x.CustomsUnitQty);

			ZDecimal IClassificationLine1.InvoiceQuantity => GetValueFromEntryLine((x) => x.InvoiceQuantity);

			ZString IClassificationLine1.InvoiceUQ => GetValueFromEntryLine((x) => x.InvoiceUQ);

			ZDecimal IClassificationLine1.CountOfInvoice => ZDecimal.Zero;

			Money IClassificationLine1.TotalLinePrice => EntryLine?.TotalLinePrice;

			Money IClassificationLine1.CustomsValue => EntryLine?.CustomsValue;

			Money IClassificationLine1.FOB => EntryLine?.FOB;

			ZDecimal IClassificationLine1.SalesTaxAmount => GetValueFromEntryLine((x) => x.SalesTaxAmount);

			ZDecimal IClassificationLine1.CTAAmount => GetValueFromEntryLine((x) => x.CTAAmount);

			(ZDecimal Amount, RefCurrency Currency) IClassificationLine1.DeductionChargeAmountAndCurrency
			{
				get
				{
					(ZDecimal Amount, RefCurrency Currency) result = (0, null);
					if (EntryLine is IClassificationLine1 classificationLine)
					{
						result = classificationLine.DeductionChargeAmountAndCurrency;
					}
					return result;
				}
			}

			ZString IClassificationLine1.CustomsDutyCode => GetValueFromEntryLine((x) => x.CustomsDutyCode);

			ZDecimal IClassificationLine1.SurtaxAmount => GetValueFromEntryLine((x) => x.SurtaxAmount);

			ZDecimal IClassificationLine1.SurtaxQuantity => GetValueFromEntryLine((x) => x.SurtaxQuantity);

			ZString IClassificationLine1.SurtaxStatementCode => GetValueFromEntryLine((x) => x.SurtaxStatementCode);

			ZString IClassificationLine1.SurtaxUnitOfMeasure => GetValueFromEntryLine((x) => x.SurtaxUnitOfMeasure);

			ZString IClassificationLine1.SurtaxCode => GetValueFromEntryLine((x) => x.SurtaxCode);

			ZBool IClassificationLine1.SurtaxIsOverride => GetValueFromEntryLine((x) => x.SurtaxIsOverride);

			ZBool IClassificationLine1.HasSurtax => GetValueFromEntryLine((x) => x.HasSurtax);

			ZDecimal IClassificationLine1.ADDAmount => GetValueFromEntryLine((x) => x.ADDAmount);

			ZDecimal IClassificationLine1.ADDQuantity => GetValueFromEntryLine((x) => x.ADDQuantity);

			ZString IClassificationLine1.ADDUnitOfMeasure => GetValueFromEntryLine((x) => x.ADDUnitOfMeasure);

			ZString IClassificationLine1.ADDCode => GetValueFromEntryLine((x) => x.ADDCode);

			ZBool IClassificationLine1.ADDIsOverride => GetValueFromEntryLine((x) => x.ADDIsOverride);

			ZBool IClassificationLine1.HasADD => GetValueFromEntryLine((x) => x.HasADD);

			ZDecimal IClassificationLine1.CVDAmount => GetValueFromEntryLine((x) => x.CVDAmount);

			ZDecimal IClassificationLine1.CVDQuantity => GetValueFromEntryLine((x) => x.CVDQuantity);

			ZString IClassificationLine1.CVDUnitOfMeasure => GetValueFromEntryLine((x) => x.CVDUnitOfMeasure);

			ZString IClassificationLine1.CVDCode => GetValueFromEntryLine((x) => x.CVDCode);

			ZBool IClassificationLine1.CVDIsOverride => GetValueFromEntryLine((x) => x.CVDIsOverride);

			ZBool IClassificationLine1.HasCVD => GetValueFromEntryLine((x) => x.HasCVD);

			ZDecimal IClassificationLine1.SafeguardAmount => GetValueFromEntryLine((x) => x.SafeguardAmount);

			ZString IClassificationLine1.SafeguardCode => GetValueFromEntryLine((x) => x.SafeguardCode);

			ZString IClassificationLine1.SafeguardStatementCode => GetValueFromEntryLine((x) => x.SafeguardStatementCode);

			ZBool IClassificationLine1.SafeguardIsOverride => GetValueFromEntryLine((x) => x.SafeguardIsOverride);

			ZBool IClassificationLine1.HasSafeguard => GetValueFromEntryLine((x) => x.HasSafeguard);

			ZString IClassificationLine1.SIMACode
			{
				get
				{
					return D99BMessageUtilities.GetDutyOrTaxRateOrExemptCode(
						group30.Group41,
						DutyTaxFeeFunctionQualifierList.IndividualDutyTaxOrFeeCustomsItem,
						DutyTaxFeeTypeNameCodeList.AntiDumpingDuty);
				}
			}

			ZString IClassificationLine1.SIMAStatementCode => CusEntryLine.GetStatementCodeViaExemptCode(((IClassificationLine1)this).SIMACode);

			ZDecimal IClassificationLine1.SIMAAssessment
			{
				get { return D99BMessageUtilities.GetAmount(group30.Group41, MonetaryAmountTypeCodeQualifierList.DeductionsCustoms); }
			}

			ZDecimal IClassificationLine1.ExciseDutyAmount
			{
				get { return GetValueFromEntryLine(x => x.ExciseDutyAmount); }
			}

			ParsedRateCodeDetails ExciseDetails
			{
				get
				{
					return exciseDetails ?? (exciseDetails = new ParsedRateCodeDetails(
				  D99BMessageUtilities.GetDutyOrTaxRateOrExemptCode(
					  group30.Group41, DutyTaxFeeFunctionQualifierList.IndividualDutyTaxOrFeeCustomsItem, DutyTaxFeeTypeNameCodeList.ExciseDuty),
					  D99BMessageUtilities.GetAmount(group30.Group41, MonetaryAmountTypeCodeQualifierList.DutyTaxOrFeeAmount)
				  ));
				}
			}
			ParsedRateCodeDetails exciseDetails;

			ZString IClassificationLine1.ExciseExemptionCode
			{
				get { return ExciseDetails.ExecemptionCode; }
			}

			ZString IClassificationLine1.ExciseCode
			{
				get { return GetValueFromEntryLine((x) => x.ExciseCode); }
			}

			ZDecimal IClassificationLine1.ExciseTaxRate
			{
				get { return ExciseDetails.Rate; }
			}

			ZDecimal IClassificationLine1.ExciseTaxRateToPrint
			{
				get { return ExciseDetails.Rate; }
			}

			ZString IClassificationLine1.ExciseTaxRateType
			{
				get { return ExciseDetails.RateType; }
			}

			ZDecimal IClassificationLine1.ExciseTaxAmount
			{
				get { return ExciseDetails.Amount; }
			}

			bool IClassificationLine1.IsDummyExciseTaxRate
			{
				get { return false; }
			}

			ZBool IClassificationLine1.HasExcise
			{
				get { return GetValueFromEntryLine((x) => x.HasExcise); }
			}

			ParsedRateCodeDetails GSTDetails
			{
				get
				{
					return gSTDetails ?? (gSTDetails = new ParsedRateCodeDetails(
						D99BMessageUtilities.GetDutyOrTaxRateOrExemptCode(
						group30.Group41,
						DutyTaxFeeFunctionQualifierList.Tax,
						DutyTaxFeeTypeNameCodeList.ValueAddedTax),
						D99BMessageUtilities.GetAmount(group30.Group41, MonetaryAmountTypeCodeQualifierList.Vat1stValue)
						));
				}
			}
			ParsedRateCodeDetails gSTDetails;

			ZString IClassificationLine1.GSTExemptionCode
			{
				get { return GSTDetails.ExecemptionCode; }
			}

			ZString IClassificationLine1.GSTCode
			{
				get { return GSTDetails.Code; }
			}

			ZDecimal IClassificationLine1.RateOfGST
			{
				get { return GSTDetails.Rate; }
			}

			ZString IClassificationLine1.GSTRateType
			{
				get { return GSTDetails.RateType; }
			}

			ZDecimal IClassificationLine1.GSTAmount
			{
				get { return GSTDetails.Amount; }
			}

			bool IClassificationLine1.HasGSTDetails
			{
				get { return true; }
			}

			ZDecimal IClassificationLine1.CUDAmount => ZDecimal.Zero;

			IEnumerable<IClassificationLine2> IClassificationLine1.ClassificationLines
			{
				get { return from SegmentGroup44 group44 in group30.Group44 select (IClassificationLine2)new ClassificationLine2(group44); }
			}

			ZInt IClassificationLine1.CountOfConsolidatedLines
			{
				get { return ZInt.Zero; }
			}

			BusinessObject IClassificationLine1.RelevantLine => EntryLine?.RandomLine;

			public MessageSubTypes MessageSubType => MessageSubTypes.Create;

			#endregion

			#region ClassificationLine2

			class ClassificationLine2 : IClassificationLine2
			{
				public ClassificationLine2(SegmentGroup44 group44)
				{
					this.group44 = group44;
					group47 = group44.Group47.Count > 0 ? group44.Group47[0] : new SegmentGroup47();
				}

				#region Implementation of IClassificationLine2

				ZInt IClassificationLine2.B3LineNumber
				{
					get
					{
						return (from GIRSegment gir in group44.GIR
								where gir.SetIdentificationQualifier == SetIdentificationQualifierList.Product
								select ZInt.ParseEmptyAsZero(gir.IdentificationNumber1.ObjectIdentifier)).FirstOrDefault();
					}
				}

				ZString IClassificationLine2.UnitOfMeasureCode
				{
					get { return D99BMessageUtilities.GetUnitOfMeasure(group44.MEA, MeasurementAttributeCodeList._1stSpecifiedTariffQuantity); }
				}

				ZDecimal IClassificationLine2.ClassificationLineQuantity
				{
					get { return D99BMessageUtilities.GetQuantity(group44.MEA, MeasurementAttributeCodeList._1stSpecifiedTariffQuantity); }
				}

				ZString IClassificationLine2.InvoiceUnitOfMeasureCode
				{
					get { return ZString.Empty; }
				}

				ZDecimal IClassificationLine2.ClassificationLineInvoiceQuantity
				{
					get { return ZDecimal.Zero; }
				}

				ZDecimal IClassificationLine2.WeightInKGM
				{
					get { return D99BMessageUtilities.GetQuantity(group44.MEA, MeasurementAttributeCodeList.LineItemMeasurement, 0); }
				}

				ParsedRateCodeDetails DutyRateDetails
				{
					get
					{
						return dutyRateDetails ?? (dutyRateDetails = new ParsedRateCodeDetails(
							(from TAXSegment tax in group47.TAX
							 where tax.DutyTaxFeeFunctionQualifier == DutyTaxFeeFunctionQualifierList.CustomsDuty
							 select tax.DutyTaxFeeAssessmentBasis).FirstOrDefault(),
							D99BMessageUtilities.GetAmount(group47.MOA, MonetaryAmountTypeCodeQualifierList.StandardDuty)
							));
					}
				}
				ParsedRateCodeDetails dutyRateDetails;

				ZDecimal IClassificationLine2.CustomsDutyRate
				{
					get { return DutyRateDetails.Rate; }
				}

				ZString IClassificationLine2.CustomsDutyRateType
				{
					get { return DutyRateDetails.RateType; }
				}

				ZDecimal IClassificationLine2.CustomsDutyAmount
				{
					get { return DutyRateDetails.Amount; }
				}

				ZString IClassificationLine2.PreviousTransactionNumber
				{
					get { return ZString.Empty; }
				}

				ZInt IClassificationLine2.PreviousLineNumber
				{
					get { return ZInt.Zero; }
				}

				#endregion

				readonly SegmentGroup44 group44;
				readonly SegmentGroup47 group47;
			}

			#endregion

			readonly CSTSegment cst;
			readonly SegmentGroup30 group30;
			readonly CusEntryHeader entryHeader;
		}

		#endregion

		#region TotalAmounts

		class TotalAmounts : ITotalAmounts
		{
			public TotalAmounts(CUSDECMessage message, bool isPositive)
			{
				dutyTaxFeeTypeName = isPositive ? "K90" : "K92";
				groups = from SegmentGroup49 group49 in message.Group49
						 where (from TAXSegment tax in group49.TAX
								where tax.DutyTaxFeeType.DutyTaxFeeTypeName == dutyTaxFeeTypeName
								select tax).Any()
						 select group49;
			}

			#region Implementation of ITotalAmounts

			ZDecimal ITotalAmounts.Deposit
			{
				get { return 0.0m; }
			}

			ZDecimal ITotalAmounts.TotalCustomsDuty
			{
				get { return D99BMessageUtilities.GetAmount(groups, MonetaryAmountTypeCodeQualifierList.StandardDuty); }
			}

			ZDecimal ITotalAmounts.TotalSIMAAssessment
			{
				get { return D99BMessageUtilities.GetAmount(groups, MonetaryAmountTypeCodeQualifierList.OtherValuationChargesCustoms); }
			}

			ZDecimal ITotalAmounts.TotalExciseTax
			{
				get { return D99BMessageUtilities.GetAmount(groups, MonetaryAmountTypeCodeQualifierList.AdditionalRoyaltiesCustoms); }
			}

			ZDecimal ITotalAmounts.TotalGST
			{
				get { return D99BMessageUtilities.GetAmount(groups, MonetaryAmountTypeCodeQualifierList.Vat1stValue); }
			}

			ZDecimal ITotalAmounts.TotalAllDutyAndTaxes
			{
				get { return D99BMessageUtilities.GetAmount(groups, MonetaryAmountTypeCodeQualifierList.MessageTotalDutyTaxFeeAmount); }
			}

			#endregion

			readonly string dutyTaxFeeTypeName;
			readonly IEnumerable<SegmentGroup49> groups;
		}

		#endregion

		readonly CUSDECMessage message;
		readonly BGMSegment bgm;
		readonly IEDIFACTMessageAttachee linkedObject;
		readonly EDIInterchange interchange;
	}
}
