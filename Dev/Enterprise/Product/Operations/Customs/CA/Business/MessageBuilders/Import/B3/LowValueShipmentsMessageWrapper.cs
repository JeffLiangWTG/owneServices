using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Customs.CA.Business.MessageBuilders.B3ImportMessageWrapper;
namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	public class LowValueShipmentsMessageWrapper : IB3HeaderWithScheduledMessageSupport
	{
		public LowValueShipmentsMessageWrapper(CusEntryHeader entryHeader, MessageSubTypes messageSubType = MessageSubTypes.Create)
		{
			b3Wrapper = new B3ImportMessageWrapper(entryHeader, true);
			declaration = entryHeader.Declaration;
			this.messageSubType = messageSubType;
		}

		#region ICAEDIFACTMessageAttachee Members

		BusinessObjectFactory Enterprise.Messaging.Business.IEDIMessageCollectionProvider.Factory
		{
			get { return b3Wrapper.Factory; }
		}

		void IEDIFACTMessageAttachee.AddMessage(Enterprise.Messaging.Business.EDIMessage message)
		{
			b3Wrapper.AddMessage(message);
		}

		Enterprise.Messaging.Business.EDIMessageCollection Enterprise.Messaging.Business.IEDIMessageCollectionProvider.Messages
		{
			get { return b3Wrapper.Messages; }
		}

		ZString IEDIFACTMessageAttachee.MessageStatus
		{
			get { return b3Wrapper.MessageStatus; }
			set { b3Wrapper.MessageStatus = value; }
		}

		ZString IEDIFACTMessageAttachee.JobStatus
		{
			get { return b3Wrapper.JobStatus; }
			set { b3Wrapper.JobStatus = value; }
		}

		bool IEDIFACTMessageAttachee.HasChanges
		{
			get { return b3Wrapper.HasChanges; }
		}

		ZString IEDIFACTMessageAttachee.JobIdentification
		{
			get { return b3Wrapper.JobIdentification; }
		}

		BusinessObject IEDIFACTMessageAttachee.TopLevelBusinessObject
		{
			get { return b3Wrapper.TopLevelBusinessObject; }
		}

		bool ICAEDIFACTMessageAttachee.IsCancelled
		{
			get { return declaration.IsCancelled; }
		}

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage
		{
			get { return declaration.RefreshValidationBeforeSendMessage; }
		}

		#endregion

		#region IB3Header Members

		ZString IB3Header.BatchNumber
		{
			get { return b3Wrapper.BatchNumber; }
		}

		ZString IB3Header.B3TypeCode
		{
			get { return B3EntryTypeList.Codes.LowValueShipments; }
		}

		ZString IB3Header.PaymentCode
		{
			get
			{
				if (declaration.IsImporterDirectPayment)
				{
					return "I";
				}
				else if (declaration.IsGSTDirectPayment)
				{
					return "G";
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		ZString IB3Header.CBSAOffice
		{
			get { return b3Wrapper.CBSAOffice; }
		}

		ZString IB3Header.PortOfUnlading
		{
			get { return ZString.Empty; }
		}

		ZString IB3Header.WarehouseNumber
		{
			get { return ZString.Empty; }
		}

		ZString IB3Header.TransactionNumber
		{
			get { return b3Wrapper.TransactionNumber; }
		}

		ZString IB3Header.BusinessNumber
		{
			get
			{
				var result = ZString.Empty;
				switch (declaration.JE_MessageSubType)
				{
					case LowValueShipmentsTypes.Codes.TotalConsolidation:
						result = GetBrokerBusinessNumberForLVS(declaration);
						break;
					case LowValueShipmentsTypes.Codes.ConsolidationByImporter:
						result = b3Wrapper.BusinessNumber;
						if (result.IsEmpty)
						{
							result = GetBrokerBusinessNumberForLVS(declaration);
						}

						break;
				}
				return result;
			}
		}

		ZString IB3Header.GSTNumber
		{
			get
			{
				switch (declaration.JE_MessageSubType)
				{
					case LowValueShipmentsTypes.Codes.ConsolidationByImporter:
						return IB3HeaderHelper.GetGSTNumber(declaration.Importer);
					default:
						return ZString.Empty;
				}
			}
		}

		ZString IB3Header.TransportMode
		{
			get { return IsMinimumDataRequired ? string.Empty : "2"; }
		}

		ZString IB3Header.CarrierCodeAtImportation
		{
			get { return b3Wrapper.CarrierCodeAtImportation; }
		}

		IEnumerable<IB3BRelease> IB3Header.B3BInputReleases
		{
			get
			{
				return declaration.JE_EntryAuthorisationDate.IsEmpty ? Array.Empty<IB3BRelease>()
					: new IB3BRelease[] { new B3BRelease { DateOfRelease = declaration.JE_EntryAuthorisationDate } };
			}
		}

		ZDecimal IB3Header.TotalValueForDuty
		{
			get { return b3Wrapper.TotalValueForDuty; }
		}

		IEnumerable<IB3SubHeader> IB3Header.PositiveB3SubHeaders
		{
			get { return from b3SubHeader in b3Wrapper.PositiveB3SubHeaders select (IB3SubHeader)new LVSSubHeader(b3SubHeader, IsMinimumDataRequired, this); }
		}

		public IEnumerable<IClassificationLine1> PositiveClassificationLines
		{
			get { return from line in b3Wrapper.PositiveClassificationLines select (IClassificationLine1)new LVSClassificationLine(line, declaration.JE_MessageSubType, messageSubType); }
		}

		IEnumerable<IB3SubHeader> IB3Header.NegativeB3SubHeaders => Enumerable.Empty<IB3SubHeader>();

		IEnumerable<IClassificationLine1> IB3Header.NegativeClassificationLines => Enumerable.Empty<IClassificationLine1>();

		public bool IsCalculationsDone
		{
			get { return !PositiveClassificationLines.Any() || PositiveClassificationLines.All(line => line.HasGSTDetails); }
		}

		bool IB3Header.SumPosAndNeg => false;

		ZString IB3Header.B3Comments
		{
			get { return b3Wrapper.B3Comments; }
		}

		ZDateTime IB3Header.ReleaseDate
		{
			get { return b3Wrapper.ReleaseDate; }
		}

		IDocAddress IB3Header.Importer
		{
			get
			{
				return declaration.JE_MessageSubType == LowValueShipmentsTypes.Codes.TotalConsolidation
					? new DocAddressWrapper(DummyDescription) : b3Wrapper.Importer;
			}
		}

		ZString IB3Header.AccountSecurityCode
		{
			get { return b3Wrapper.AccountSecurityCode; }
		}

		ITotalAmounts IB3Header.PositiveTotalAmounts
		{
			get { return b3Wrapper.PositiveTotalAmounts; }
		}

		ITotalAmounts IB3Header.NegativeTotalAmounts
		{
			get { return b3Wrapper.NegativeTotalAmounts; }
		}

		void IB3Header.RefreshCachedValues()
		{
			((IB3Header)b3Wrapper).RefreshCachedValues();
		}

		#region Implementation

		bool IsMinimumDataRequired
		{
			get { return ((IB3Header)this).TotalValueForDuty < MinimumDataRequiredAmountInCad; }
		}

		ZString IB3Header.MessageType => declaration.JE_MessageType;

		internal static ZString GetBrokerBusinessNumberForLVS(JobDeclaration declaration)
		{
			var result = ZString.Empty;
			var broker = declaration.CusAgent;
			if (broker != null)
			{
				if (broker.HomeBranch != null)
				{
					result = GetBusinessNumberForLVS(broker.HomeBranch.OrgProxy, broker.HomeBranch.Company.OrgProxy);
				}
			}
			else
			{
				result = GetBusinessNumberForLVS(GlbBranch.CurrentBranch.OrgProxy, GlbCompany.CurrentCompany.OrgProxy);
			}
			return result;
		}

		static ZString GetBusinessNumberForLVS(OrgHeader org, OrgHeader fallback)
		{
			var cusCodes = new ZString[] { OrgCusCode.CACodeTypes.BusinessNumberForLowValueShipments };
			var result = IB3HeaderHelper.GetCustomsRegNo(org, cusCodes);
			if (result.IsEmpty)
			{
				result = IB3HeaderHelper.GetCustomsRegNo(fallback, cusCodes);
			}

			return result;
		}

		#endregion

		#endregion

		#region LVSSubHeader

		internal class LVSSubHeader : IB3SubHeader
		{
			internal LVSSubHeader(IB3SubHeader b3SubHeader, bool isMinimumDataRequired, IB3Header b3Header)
			{
				var header = b3SubHeader as B3SubHeader;
				this.b3SubHeader = b3SubHeader;
				this.b3Header = b3Header;
				this.isMinimumDataRequired = isMinimumDataRequired;
				this.InvoiceHeader = header.InvoiceHeader;
			}

			#region IB3SubHeader Members

			ZInt IB3SubHeader.B3SubHeaderNumber
			{
				get { return b3SubHeader.B3SubHeaderNumber; }
			}

			ZString IB3SubHeader.InvoiceNumber
			{
				get { return ZString.Empty; }
			}

			ZDecimal IB3SubHeader.FreightCharges
			{
				get { return isMinimumDataRequired ? ZDecimal.Zero : 1; }
			}

			IDocAddress IB3SubHeader.Vendor
			{
				get
				{
					var vendor = new DocAddressWrapper(DummyDescription);
					if (!isMinimumDataRequired)
					{
						vendor.CountryCode = Core.Constants.CountryCodes.UnitedStates;
						vendor.E2_State = USStatesList.Codes.NewYork;
						vendor.E2_Postcode = DummyPostCode;
					}
					return vendor;
				}
			}

			IDocAddress IB3SubHeader.Exporter
			{
				get { return null; }
			}

			ZDateTime IB3SubHeader.DateOfDirectShipment
			{
				get { return ZDateTime.Empty; }
			}

			ZString IB3SubHeader.CountryOfOrigin
			{
				get { return GetContryOfOriginOrExport(b3SubHeader.CountryOfOrigin); }
			}

			ZString IB3SubHeader.PlaceOfExport
			{
				get { return GetContryOfOriginOrExport(((B3SubHeader)b3SubHeader).Lines.First().EffectiveCountryAndStateOfExport); }
			}

			ZString IB3SubHeader.USPortOfExit
			{
				get { return isMinimumDataRequired ? ZString.Empty : new ZString(DummyPortOfExit); }
			}

			ZString IB3SubHeader.TariffTreatmentCode
			{
				get { return b3SubHeader.TariffTreatmentCode; }
			}

			ZString IB3SubHeader.TimeLimitUnit
			{
				get { return b3SubHeader.TimeLimitUnit; }
			}

			ZInt IB3SubHeader.B3TimeLimits
			{
				get { return b3SubHeader.B3TimeLimits; }
			}

			ZString IB3SubHeader.CurrencyCode
			{
				get { return Core.Constants.CurrencyCodes.Canada; }
			}

			ZDecimal IB3SubHeader.ExchangeRate
			{
				get { return ZDecimal.Zero; }
			}

			VendorStateAndZipStruct IB3SubHeader.VendorStateAndZip
			{
				get { return !isMinimumDataRequired ? new VendorStateAndZipStruct("U" + USStatesList.Codes.NewYork, DummyPostCode) : new VendorStateAndZipStruct(); }
			}

			ZString IB3SubHeader.TradeZone
			{
				get { return b3SubHeader.TradeZone; }
			}

			IB3Header IB3SubHeader.B3Header
			{
				get { return this.b3Header; }
			}

			ZString GetContryOfOriginOrExport(string actualValue)
			{
				return TariffTreatmentCodes.IsCountryOfOriginAndExportRequiredForLVS(b3SubHeader.TariffTreatmentCode) ? actualValue : DummyCountry;
			}

			#endregion

			public JobComInvoiceHeader InvoiceHeader;
			readonly IB3SubHeader b3SubHeader;
			readonly IB3Header b3Header;
			readonly bool isMinimumDataRequired;
			const string DummyCountry = "UNY";
			const string DummyPostCode = "55555";
			const string DummyPortOfExit = "1001";
		}

		#endregion

		#region LVSClassificationLine

		class LVSClassificationLine : IClassificationLine1
		{
			internal LVSClassificationLine(IClassificationLine1 line, ZString consolidationType, MessageSubTypes messageSubType)
			{
				this.line = line;
				var entryLine = line as CusEntryLine;
				this.messageSubType = messageSubType;
				isOicOrRemission = consolidationType == LowValueShipmentsTypes.Codes.ConsolidationByImporter
					&& (!line.AuthorityNumber.IsEmpty || ((CusEntryLine)line).RandomLine.IsRemissionLine);
			}

			#region IClassificationLine1 Members

			BusinessObjectFactory IClassificationLine1.Factory => line.Factory;

			ZShort IClassificationLine1.B3LineNumber
			{
				get { return line.B3LineNumber; }
			}

			ZString IClassificationLine1.RecordIdentifier
			{
				get { return line.RecordIdentifier; }
			}

			ZInt IClassificationLine1.B3SubHeaderNumber
			{
				get { return line.B3SubHeaderNumber; }
			}

			ZInt IClassificationLine1.B3SubHeaderNumberForLVX
			{
				get { return line.B3SubHeaderNumberForLVX; }
			}

			ZString IClassificationLine1.ClassificationNumber
			{
				get { return isOicOrRemission ? line.ClassificationNumber : (ZString)DummyClassificationNumber; }
			}

			ZString IClassificationLine1.ValueForDutyCode
			{
				get { return ValueForDutyCodes.Codes.UnrelatedFirmsPaidPayableWithoutAdjustments; }
			}

			ZString IClassificationLine1.TariffCode
			{
				get { return isOicOrRemission ? line.TariffCode : ZString.Empty; }
			}

			ZDecimal IClassificationLine1.ValueForCurrency
			{
				get { return line.ValueForDuty; } //Value for currency should be always in CAD for LVS, so send VFD.
			}

			ZDecimal IClassificationLine1.ValueForDuty
			{
				get { return line.ValueForDuty; }
			}

			ZDecimal IClassificationLine1.ValueForTax
			{
				get { return line.ValueForTax; }
			}

			ZString IClassificationLine1.AuthorityNumber
			{
				get { return isOicOrRemission ? line.AuthorityNumber : ZString.Empty; }
			}

			ZString IClassificationLine1.TRSNumber
			{
				get { return ZString.Empty; }
			}

			ZString[] IClassificationLine1.PartNumberDescriptions
			{
				get { return isOicOrRemission ? line.PartNumberDescriptions : new ZString[] { DummyDescription }; }
			}

			IEnumerable<IInvoiceCrossReference> IClassificationLine1.InvoiceCrossReferences
			{
				get { return new[] { new LVSInvoiceCrossReference() }; }
			}

			ZDecimal IClassificationLine1.CustomsQuantity
			{
				get { return line.CustomsQuantity; }
			}

			ZString IClassificationLine1.CustomsUnitQty
			{
				get { return line.CustomsUnitQty; }
			}

			ZDecimal IClassificationLine1.InvoiceQuantity
			{
				get { return line.InvoiceQuantity; }
			}

			ZString IClassificationLine1.InvoiceUQ
			{
				get { return line.InvoiceUQ; }
			}

			ZDecimal IClassificationLine1.CountOfInvoice
			{
				get { return ZDecimal.Zero; }
			}

			Money IClassificationLine1.TotalLinePrice
			{
				get { return line.TotalLinePrice; }
			}

			Money IClassificationLine1.CustomsValue
			{
				get { return line.CustomsValue; }
			}

			Money IClassificationLine1.FOB
			{
				get { return line.FOB; }
			}

			ZDecimal IClassificationLine1.SalesTaxAmount
			{
				get { return line.SalesTaxAmount; }
			}

			ZDecimal IClassificationLine1.CTAAmount
			{
				get { return line.CTAAmount; }
			}

			(ZDecimal Amount, RefCurrency Currency) IClassificationLine1.DeductionChargeAmountAndCurrency
			{
				get { return line.DeductionChargeAmountAndCurrency; }
			}

			ZString IClassificationLine1.CustomsDutyCode
			{
				get { return line.CustomsDutyCode; }
			}

			ZDecimal IClassificationLine1.SurtaxAmount
			{
				get { return line.SurtaxAmount; }
			}

			ZDecimal IClassificationLine1.SurtaxQuantity
			{
				get { return line.SurtaxQuantity; }
			}

			ZString IClassificationLine1.SurtaxUnitOfMeasure
			{
				get { return line.SurtaxUnitOfMeasure; }
			}

			ZString IClassificationLine1.SurtaxStatementCode
			{
				get { return line.SurtaxStatementCode; }
			}

			ZString IClassificationLine1.SurtaxCode
			{
				get { return line.SurtaxCode; }
			}

			ZBool IClassificationLine1.SurtaxIsOverride
			{
				get { return line.SurtaxIsOverride; }
			}

			ZBool IClassificationLine1.HasSurtax
			{
				get { return line.HasSurtax; }
			}

			ZDecimal IClassificationLine1.ADDAmount
			{
				get { return line.ADDAmount; }
			}

			ZDecimal IClassificationLine1.ADDQuantity
			{
				get { return line.ADDQuantity; }
			}

			ZString IClassificationLine1.ADDUnitOfMeasure
			{
				get { return line.ADDUnitOfMeasure; }
			}

			ZString IClassificationLine1.ADDCode
			{
				get { return line.ADDCode; }
			}

			ZBool IClassificationLine1.ADDIsOverride
			{
				get { return line.ADDIsOverride; }
			}

			ZBool IClassificationLine1.HasADD
			{
				get { return line.HasADD; }
			}

			ZDecimal IClassificationLine1.CVDAmount
			{
				get { return line.CVDAmount; }
			}

			ZDecimal IClassificationLine1.CVDQuantity
			{
				get { return line.CVDQuantity; }
			}

			ZString IClassificationLine1.CVDUnitOfMeasure
			{
				get { return line.CVDUnitOfMeasure; }
			}

			ZString IClassificationLine1.CVDCode
			{
				get { return line.CVDCode; }
			}

			ZBool IClassificationLine1.CVDIsOverride
			{
				get { return line.CVDIsOverride; }
			}

			ZBool IClassificationLine1.HasCVD
			{
				get { return line.HasCVD; }
			}

			ZDecimal IClassificationLine1.SafeguardAmount
			{
				get { return line.SafeguardAmount; }
			}

			ZString IClassificationLine1.SafeguardCode
			{
				get { return line.SafeguardCode; }
			}

			ZString IClassificationLine1.SafeguardStatementCode
			{
				get { return line.SafeguardStatementCode; }
			}

			ZBool IClassificationLine1.SafeguardIsOverride
			{
				get { return line.SafeguardIsOverride; }
			}

			ZBool IClassificationLine1.HasSafeguard
			{
				get { return line.HasSafeguard; }
			}

			ZString IClassificationLine1.SIMACode
			{
				get { return line.SIMACode; }
			}

			ZString IClassificationLine1.SIMAStatementCode
			{
				get { return line.SIMAStatementCode; }
			}

			ZDecimal IClassificationLine1.SIMAAssessment
			{
				get { return line.SIMAAssessment; }
			}

			ZDecimal IClassificationLine1.ExciseDutyAmount
			{
				get { return line.ExciseDutyAmount; }
			}

			ZString IClassificationLine1.ExciseExemptionCode
			{
				get { return isOicOrRemission ? line.ExciseExemptionCode : ZString.Empty; }
			}

			ZString IClassificationLine1.ExciseCode
			{
				get { return line.ExciseCode; }
			}

			ZDecimal ExciseTaxRateCore
			{
				get { return isOicOrRemission ? line.ExciseTaxRate : 44.0; }
			}

			bool IClassificationLine1.IsDummyExciseTaxRate
			{
				get { return !isOicOrRemission; }
			}

			ZBool IClassificationLine1.HasExcise
			{
				get { return line.HasExcise; }
			}

			ZDecimal IClassificationLine1.ExciseTaxRate
			{
				get { return ExciseTaxRateCore; }
			}

			ZDecimal IClassificationLine1.ExciseTaxRateToPrint
			{
				get { return isOicOrRemission && line.ExciseTaxAmount == ZDecimal.Zero ? ZDecimal.Zero : ExciseTaxRateCore; }
			}

			ZString IClassificationLine1.ExciseTaxRateType
			{
				get { return isOicOrRemission ? line.ExciseTaxRateType : (ZString)RateTypes.Codes.AdValorem; }
			}

			ZDecimal IClassificationLine1.ExciseTaxAmount
			{
				get { return line.ExciseTaxAmount; }
			}

			ZString IClassificationLine1.GSTExemptionCode
			{
				get { return isOicOrRemission ? line.GSTExemptionCode : ZString.Empty; }
			}

			ZString IClassificationLine1.GSTCode
			{
				get { return isOicOrRemission ? line.GSTCode : ZString.Empty; }
			}

			ZDecimal IClassificationLine1.RateOfGST
			{
				get { return isOicOrRemission ? line.RateOfGST : 39.0; }
			}

			ZString IClassificationLine1.GSTRateType
			{
				get { return isOicOrRemission ? line.GSTRateType : (ZString)RateTypes.Codes.AdValorem; }
			}

			ZDecimal IClassificationLine1.GSTAmount
			{
				get { return line.GSTAmount; }
			}

			bool IClassificationLine1.HasGSTDetails
			{
				get { return line.HasGSTDetails; }
			}

			ZDecimal IClassificationLine1.CUDAmount => ZDecimal.Zero;

			IEnumerable<IClassificationLine2> IClassificationLine1.ClassificationLines
			{
				get { return from line2 in line.ClassificationLines select (IClassificationLine2)new LVSClassificationLine2(line2, this, isOicOrRemission); }
			}

			ZInt IClassificationLine1.CountOfConsolidatedLines
			{
				get { return line.CountOfConsolidatedLines; }
			}

			BusinessObject IClassificationLine1.RelevantLine => line.RelevantLine;

			public ZShort SequenceNumber => line.SequenceNumber;

			MessageSubTypes IClassificationLine1.MessageSubType => messageSubType;

			#endregion

			#region LVSInvoiceCrossReference

			class LVSInvoiceCrossReference : IInvoiceCrossReference
			{
				#region IInvoiceCrossReference Members

				ZInt IInvoiceCrossReference.InvoiceLineNumber
				{
					get { return 1; }
				}

				ZInt IInvoiceCrossReference.InvoicePageNumber
				{
					get { return 1; }
				}

				ZDecimal IInvoiceCrossReference.InvoiceValue
				{
					get { return 0.01; }
				}

				#endregion
			}

			#endregion

			#region LVSClassificationLine2

			class LVSClassificationLine2 : IClassificationLine2
			{
				internal LVSClassificationLine2(IClassificationLine2 line2, IClassificationLine1 line1, bool isOicOrRemissionOrCasualImportLine)
				{
					this.line2 = line2;
					this.line1 = line1;
					this.isOicOrRemissionOrCasualImportLine = isOicOrRemissionOrCasualImportLine;
				}

				#region IClassificationLine2 Members

				public ZInt B3LineNumber
				{
					get { return line2.B3LineNumber; }
				}

				public ZString UnitOfMeasureCode
				{
					get { return isOicOrRemissionOrCasualImportLine ? line2.UnitOfMeasureCode : (ZString)CustomsUnitOfMeasureList.Codes.Number; }
				}

				public ZDecimal ClassificationLineQuantity
				{
					get { return isOicOrRemissionOrCasualImportLine ? line2.ClassificationLineQuantity : (ZDecimal)line1.CountOfConsolidatedLines; }
				}

				ZString IClassificationLine2.InvoiceUnitOfMeasureCode
				{
					get { return ZString.Empty; }
				}

				ZDecimal IClassificationLine2.ClassificationLineInvoiceQuantity
				{
					get { return ZDecimal.Zero; }
				}

				public ZDecimal WeightInKGM
				{
					get { return line2.WeightInKGM; }
				}

				public ZDecimal CustomsDutyRate
				{
					get { return isOicOrRemissionOrCasualImportLine ? line2.CustomsDutyRate : 1.0; }
				}

				public ZString CustomsDutyRateType
				{
					get { return isOicOrRemissionOrCasualImportLine ? line2.CustomsDutyRateType : (ZString)RateTypes.Codes.AdValorem; }
				}

				public ZDecimal CustomsDutyAmount
				{
					get { return line2.CustomsDutyAmount; }
				}

				public ZString PreviousTransactionNumber
				{
					get { return ZString.Empty; }
				}

				public ZInt PreviousLineNumber
				{
					get { return ZInt.Zero; }
				}

				#endregion

				readonly IClassificationLine2 line2;
				readonly IClassificationLine1 line1;
				readonly bool isOicOrRemissionOrCasualImportLine;
			}

			#endregion

			readonly IClassificationLine1 line;
			readonly MessageSubTypes messageSubType;
			readonly bool isOicOrRemission;
			const string DummyClassificationNumber = "0000999900";
		}

		#endregion

		internal const string DummyDescription = "Various";
		const decimal MinimumDataRequiredAmountInCad = 2500;
		readonly B3ImportMessageWrapper b3Wrapper;
		readonly JobDeclaration declaration;
		readonly MessageSubTypes messageSubType;

		#region  IB3HeaderWithScheduledMessageSupport Members

		public void CancelAndDeactivateScheduledB3Message()
		{
			b3Wrapper.CancelAndDeactivateScheduledB3Message();
		}

		public void PopulateEntrySubmittedDateIfRequired(ZDateTime? scheduledTime)
		{
			b3Wrapper.PopulateEntrySubmittedDateIfRequired(scheduledTime);
		}

		#endregion
	}
}
