using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.CA.Business
{
	public class B3XMessageWrapper : IB3Header
	{
		public B3XMessageWrapper(JobDeclaration declaration)
		{
			Argument.NotNull(declaration, "declaration");
			this.declaration = declaration;
		}

		#region IB3Header Members

		ZString IB3Header.BatchNumber => EDIMessage.UniqueBatchNumberPlaceHolder;

		ZString IB3Header.B3TypeCode => BGMPaymentCode;

		internal const string BGMPaymentCode = "X";

		ZString IB3Header.PaymentCode => declaration.JE_PaymentMethod;

		ZString IB3Header.CBSAOffice
		{
			get { return declaration.JE_CustomsOffice.TrimStart('0'); }
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
			get { return declaration.TransactionNumber.UniqueIdentifier; }
		}

		ZString IB3Header.BusinessNumber => IB3HeaderHelper.GetBusinessNumber(declaration.Importer);

		ZString IB3Header.GSTNumber => IB3HeaderHelper.GetGSTNumber(declaration.Importer);

		ZString IB3Header.TransportMode => declaration.Lookups.CBSATransportTypeList.GetDescriptionFromCode(declaration.JE_TransportMode);

		ZString IB3Header.CarrierCodeAtImportation => declaration.JE_CarrierCode;

		IEnumerable<IB3BRelease> IB3Header.B3BInputReleases
		{
			get
			{
				var result = (from CargoControlNumber ccn in declaration.OriginalJobCargoControlNumbers where !ccn.CY_CargoControlNumber.IsEmpty select new B3BRelease(ccn.CY_CargoControlNumber, ccn.CY_DateOfRelease)).ToArray();
				if (!declaration.JE_EntryAuthorisationDate.IsEmpty)
				{
					if (!result.Any())
					{
						result = new[] { new B3BRelease() { CargoControlNumber = "2CSA1" } };
					}
					result[0].DateOfRelease = declaration.JE_EntryAuthorisationDate;
				}
				return result;
			}
		}

		ZDecimal IB3Header.TotalValueForDuty
		{
			get
			{
				if (!totalValueForDuty.HasValue)
				{
					totalValueForDuty = declaration.B2AsClaimedForInvoices.Sum(x => x.InvoiceLines.Cast<JobComInvoiceLine>().Sum(y => y.CA_CustomsValue));
				}
				return totalValueForDuty.Value;
			}
		}
		ZDecimal? totalValueForDuty;

		IEnumerable<IB3SubHeader> IB3Header.PositiveB3SubHeaders
		{
			get
			{
				if (positiveB3SubHeaders == null)
				{
					positiveB3SubHeaders = from JobComInvoiceHeader header in declaration.B2AsClaimedForInvoices
										   where SendSubHeader(header)
										   orderby header.JZ_InvoiceNumber
										   select (IB3SubHeader)new B3XSubHeader(header, this);
				}
				return positiveB3SubHeaders;
			}
		}
		IEnumerable<IB3SubHeader> positiveB3SubHeaders;

		bool SendSubHeader(JobComInvoiceHeader claimedHeader)
		{
			var result = true;

			if (declaration.B2AsAccountedForInvoices.OfType<JobComInvoiceHeader>().FirstOrDefault(x => x.JZ_InvoiceNumber == claimedHeader.JZ_InvoiceNumber) is JobComInvoiceHeader accountedHeader)
			{
				result = !(claimedHeader.JZ_RN_NKDefaultOrigin == accountedHeader.JZ_RN_NKDefaultOrigin
					&& claimedHeader.JZ_RW_NKOriginState == accountedHeader.JZ_RW_NKOriginState
					&& claimedHeader.CA_RN_NKExport == accountedHeader.CA_RN_NKExport
					&& claimedHeader.CA_USStateOfExport == accountedHeader.CA_USStateOfExport
					&& claimedHeader.CA_TreatmentCode == accountedHeader.CA_TreatmentCode
					&& claimedHeader.JZ_ValuationDateOverride == accountedHeader.JZ_ValuationDateOverride
					&& claimedHeader.JZ_RX_NKInvoice_Currency == accountedHeader.JZ_RX_NKInvoice_Currency
					&& claimedHeader.CA_TimeLimit == accountedHeader.CA_TimeLimit
					&& claimedHeader.CA_TimeLimitCode == accountedHeader.CA_TimeLimitCode
					&& claimedHeader.CA_TradeZone == accountedHeader.CA_TradeZone);
			}

			return result;
		}

		IEnumerable<IClassificationLine1> IB3Header.PositiveClassificationLines
		{
			get
			{
				if (positiveClassificationLines == null)
				{
					positiveClassificationLines = declaration.B2AsClaimedForInvoices.SelectMany(x => x.InvoiceLines.Cast<JobComInvoiceLine>().Select(y => new B3XClassificationLine1(y, false)));
				}
				return positiveClassificationLines;
			}
		}
		IEnumerable<IClassificationLine1> positiveClassificationLines;

		IEnumerable<IB3SubHeader> IB3Header.NegativeB3SubHeaders
		{
			get
			{
				if (negativeB3SubHeaders == null)
				{
					negativeB3SubHeaders = from JobComInvoiceHeader header in declaration.B2AsAccountedForInvoices
										   orderby header.JZ_InvoiceNumber
										   select (IB3SubHeader)new B3XSubHeader(header, this);
				}
				return negativeB3SubHeaders;
			}
		}
		IEnumerable<IB3SubHeader> negativeB3SubHeaders;

		IEnumerable<IClassificationLine1> IB3Header.NegativeClassificationLines
		{
			get
			{
				if (negativeClassificationLines == null)
				{
					negativeClassificationLines = declaration.B2AsAccountedForInvoices.SelectMany(x => x.InvoiceLines.Cast<JobComInvoiceLine>().Select(y => new B3XClassificationLine1(y, true)));
				}
				return negativeClassificationLines;
			}
		}
		IEnumerable<IClassificationLine1> negativeClassificationLines;

		bool IB3Header.IsCalculationsDone
		{
			get { return ((IB3Header)this).PositiveClassificationLines.All(line => line.HasGSTDetails) && ((IB3Header)this).NegativeClassificationLines.All(line => line.HasGSTDetails); }
		}

		bool IB3Header.SumPosAndNeg => true;

		ZString IB3Header.B3Comments
		{
			get { return ZString.Empty; }
		}

		IDocAddress IB3Header.Importer
		{
			get { return declaration.Importer.MainAddress; }
		}

		ZString IB3Header.AccountSecurityCode
		{
			get { return declaration.TransactionNumber.AccountSecurityCode; }
		}

		ITotalAmounts IB3Header.PositiveTotalAmounts
		{
			get
			{
				var positiveAmounts = ((IB3Header)this).PositiveClassificationLines.GetTotalAmounts(MessageConstants.B3RecordIdentifiers.Positive, declaration.CA_AnySightDepositAmount);
				var negativeAmounts = ((IB3Header)this).NegativeClassificationLines.GetTotalAmounts(MessageConstants.B3RecordIdentifiers.Negative, declaration.CA_AnySightDepositAmount);

				return new TotalAmounts
				{
					Deposit = positiveAmounts.Deposit + negativeAmounts.Deposit,
					TotalExciseTax = positiveAmounts.TotalExciseTax + negativeAmounts.TotalExciseTax,
					TotalSIMAAssessment = positiveAmounts.TotalSIMAAssessment + negativeAmounts.TotalSIMAAssessment,
					TotalGST = positiveAmounts.TotalGST + negativeAmounts.TotalGST,
					TotalCustomsDuty = positiveAmounts.TotalCustomsDuty + negativeAmounts.TotalCustomsDuty
				};
			}
		}

		ITotalAmounts IB3Header.NegativeTotalAmounts
		{
			get
			{
				return new TotalAmounts
				{
					Deposit = 0,
					TotalCustomsDuty = 0,
					TotalExciseTax = 0,
					TotalGST = 0,
					TotalSIMAAssessment = 0
				};
			}
		}

		void IB3Header.RefreshCachedValues()
		{
			positiveB3SubHeaders = null;
			positiveClassificationLines = null;
			negativeB3SubHeaders = null;
			negativeClassificationLines = null;
			totalValueForDuty = null;
		}

		#endregion

		#region ICAEDIFACTMessageAttachee Members

		public BusinessObjectFactory Factory
		{
			get { return declaration.Factory; }
		}

		public void AddMessage(Enterprise.Messaging.Business.EDIMessage message)
		{
			declaration.Messages.Add(message);
		}

		public Enterprise.Messaging.Business.EDIMessageCollection Messages
		{
			get { return declaration.Messages; }
		}

		public ZString MessageStatus
		{
			get => declaration.JE_MessageStatus;
			set => declaration.JE_MessageStatus = value;
		}

		public ZString JobStatus
		{
			get => declaration.JE_EntryStatus;
			set => declaration.JE_EntryStatus = value;
		}

		public bool HasChanges
		{
			get { return declaration.HasChanges; }
		}

		public ZString JobIdentification
		{
			get { return declaration.JE_DeclarationReference; }
		}

		public BusinessObject TopLevelBusinessObject
		{
			get { return declaration; }
		}

		public ZDateTime ReleaseDate
		{
			get { return declaration.JE_EntryAuthorisationDate; }
			set { declaration.JE_EntryAuthorisationDate = value; }
		}

		bool ICAEDIFACTMessageAttachee.IsCancelled
		{
			get { return declaration.IsCancelled; }
		}

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage
		{
			get { return declaration.RefreshValidationBeforeSendMessage; }
		}

		ZString IB3Header.MessageType => declaration.JE_MessageType;

		#endregion

		#region Implementation

		#region B3XSubHeader

		internal class B3XSubHeader : IB3SubHeader
		{
			internal B3XSubHeader(JobComInvoiceHeader header, IB3Header b3Header)
			{
				this.header = header;
				this.b3Header = b3Header;
			}

			#region IB3SubHeader Members

			public ZInt B3SubHeaderNumber
			{
				get
				{
					return -1;
				}
			}

			ZString IB3SubHeader.InvoiceNumber
			{
				get
				{
					return B3SubHeaderNumber.ToString();
				}
			}

			ZDecimal IB3SubHeader.FreightCharges
			{
				get
				{
					return ZDecimal.Zero;
				}
			}

			IDocAddress IB3SubHeader.Vendor
			{
				get
				{
					var mainVendor = header.JobDeclaration.NotifyParty;
					return mainVendor?.MainAddress as IDocAddress ?? new DocAddressWrapper("VARIOUS");
				}
			}

			IDocAddress IB3SubHeader.Exporter
			{
				get { return ((IEDIInvoiceOGD)header).Exporter; }
			}

			ZDateTime IB3SubHeader.DateOfDirectShipment
			{
				get { return !header.JZ_ValuationDateOverride.IsEmpty ? header.JZ_ValuationDateOverride : header.JobDeclaration.OriginalLodgedB3ReleaseDateForB3X; }
			}

			ZString IB3SubHeader.CountryOfOrigin
			{
				get
				{
					return header.JZ_RW_NKOriginState.IsEmpty ? header.JZ_RN_NKDefaultOrigin : Get3AlphasUSState(header.JZ_RW_NKOriginState);
				}
			}

			ZString IB3SubHeader.PlaceOfExport
			{
				get { return header.CA_USStateOfExport.IsEmpty ? header.CA_RN_NKExport : Get3AlphasUSState(header.CA_USStateOfExport); }
			}

			ZString IB3SubHeader.USPortOfExit
			{
				get { return ZString.Empty; }
			}

			ZString IB3SubHeader.TariffTreatmentCode
			{
				get { return header.CA_TreatmentCode; }
			}

			ZString IB3SubHeader.TimeLimitUnit
			{
				get { return header.JobDeclaration.CA_AmendmentTo == (ZString)AmendmentToList.Codes.B2 ? (ZString)"Y" : ZString.Empty; }
			}

			ZInt IB3SubHeader.B3TimeLimits
			{
				get { return header.JobDeclaration.CA_AmendmentTo == (ZString)AmendmentToList.Codes.B2 ? (ZInt)2 : ZInt.Zero; }
			}

			ZString IB3SubHeader.CurrencyCode
			{
				get { return header.JZ_RX_NKInvoice_Currency; }
			}

			ZDecimal IB3SubHeader.ExchangeRate
			{
				get { return header.EffectiveExchangeRateForInvoiceCurr; }
			}

			IB3Header IB3SubHeader.B3Header
			{
				get { return this.b3Header; }
			}

			#endregion

			readonly IB3Header b3Header;
			readonly JobComInvoiceHeader header;
			internal IEnumerable<JobComInvoiceLine> Lines { get; private set; }

			VendorStateAndZipStruct IB3SubHeader.VendorStateAndZip
			{
				get
				{
					var vendor = ((IB3SubHeader)this).Vendor;
					var state = vendor.E2_State;

					if (vendor.CountryCode == Core.Constants.CountryCodes.UnitedStates)
					{
						state = Get3AlphasUSState(vendor.E2_State);
					}

					return new VendorStateAndZipStruct(state, vendor.E2_Postcode);
				}
			}

			public ZString TradeZone
			{
				get
				{
					return header.CA_TradeZone;
				}
			}
			ZString Get3AlphasUSState(ZString state)
			{
				return state.IsEmpty ? state : new ZString("U" + state.TrimStart());
			}
		}

		#endregion

		#region B3XClassificationLine1
		internal class B3XClassificationLine1 : IClassificationLine1
		{
			internal B3XClassificationLine1(JobComInvoiceLine line, ZBool isAccountedForLine)
			{
				this.line = line;
				this.isAccountedForLine = isAccountedForLine;
				this.header = line.InvoiceHeader;
			}
			readonly JobComInvoiceLine line;
			readonly ZBool isAccountedForLine;
			readonly JobComInvoiceHeader header;

			BusinessObjectFactory IClassificationLine1.Factory => line.Factory;

			ZShort IClassificationLine1.B3LineNumber => -1;

			ZShort IClassificationLine1.SequenceNumber => -1;

			ZString IClassificationLine1.RecordIdentifier => isAccountedForLine ? MessageConstants.B3RecordIdentifiers.Negative : MessageConstants.B3RecordIdentifiers.Positive;

			ZInt IClassificationLine1.B3SubHeaderNumber
			{
				get
				{
					var subHeaderNo = ZInt.Zero;
					ZInt.TryParse(header.JZ_InvoiceNumber, out subHeaderNo);
					return subHeaderNo;
				}
			}

			ZInt IClassificationLine1.B3SubHeaderNumberForLVX => ZInt.Zero;

			ZString IClassificationLine1.ClassificationNumber => line.JI_Tariff;

			ZString IClassificationLine1.ValueForDutyCode => line.CA_ValueForDutyCode;

			ZString IClassificationLine1.TariffCode => line.CA_99TariffCode;

			ZDecimal IClassificationLine1.ValueForCurrency => line.CA_CustomsValue;

			ZDecimal IClassificationLine1.ValueForDuty => line.CA_CustomsValue;

			ZDecimal IClassificationLine1.ValueForTax => line.CA_ValueForTax;

			ZString IClassificationLine1.AuthorityNumber => line.CA_AuthorityNumber;

			ZString IClassificationLine1.TRSNumber => ZString.Empty;

			ZString[] IClassificationLine1.PartNumberDescriptions => Array.Empty<ZString>();

			IEnumerable<IInvoiceCrossReference> IClassificationLine1.InvoiceCrossReferences => Enumerable.Empty<IInvoiceCrossReference>();

			ZDecimal IClassificationLine1.CustomsQuantity => line.JI_CustomsQuantity;

			ZString IClassificationLine1.CustomsUnitQty => line.JI_CustomsUnitQty;

			ZDecimal IClassificationLine1.InvoiceQuantity => line.JI_InvoiceQuantity;

			ZString IClassificationLine1.InvoiceUQ => line.JI_InvoiceUQ;

			ZDecimal IClassificationLine1.CountOfInvoice => ZDecimal.Zero;

			Money IClassificationLine1.TotalLinePrice => line.JI_LinePriceMoney;

			Money IClassificationLine1.CustomsValue => new Money(line.JI_CustomsValue, header.LocalCurrency);

			Money IClassificationLine1.FOB => line.JI_FOB;

			ZDecimal IClassificationLine1.SalesTaxAmount => GetTotalAmountFromDutiesAndTaxes(DutyAndTaxTypes.Codes.CPT);

			ZDecimal IClassificationLine1.CTAAmount => GetTotalAmountFromDutiesAndTaxes(DutyAndTaxTypes.Codes.CTA);

			(ZDecimal Amount, RefCurrency Currency) IClassificationLine1.DeductionChargeAmountAndCurrency
			{
				get
				{
					if (!deductionChargeAmountAndCurrency.HasValue)
					{
						deductionChargeAmountAndCurrency = (0, null);
						var deductionChargesOfLine = line.Charges.Where(x => x.J7_ChargeType == CustomsChargeTypeList.Codes.DeductionCharge);
						if (deductionChargesOfLine.Any())
						{
							var totalAmount = deductionChargesOfLine.Sum(x => x.J7_Amount);
							var currency = deductionChargesOfLine.First().Currency;
							deductionChargeAmountAndCurrency = (totalAmount, currency);
						}
						else
						{
							var deductionChargesOfHeader = line.InvoiceHeader.Charges.Where(x => x.J7_ChargeType == CustomsChargeTypeList.Codes.DeductionCharge);
							if (deductionChargesOfHeader.Any())
							{
								var totalAmount = deductionChargesOfHeader.Sum(x => x.J7_Amount);
								var currency = deductionChargesOfHeader.First().Currency;
								deductionChargeAmountAndCurrency = (totalAmount, currency);
							}
						}
					}
					return deductionChargeAmountAndCurrency.Value;
				}
			}
			(ZDecimal Amount, RefCurrency Currency)? deductionChargeAmountAndCurrency;

			ZString IClassificationLine1.CustomsDutyCode => line.DutyAndTaxManager.GetCode(DutyAndTaxTypes.Codes.CustomsDuty);

			ZDecimal IClassificationLine1.SurtaxAmount => GetTotalAmountFromDutiesAndTaxes(DutyAndTaxTypes.Codes.SUR);

			ZDecimal IClassificationLine1.SurtaxQuantity => GetTotalQuantityFromDutiesAndTaxes(DutyAndTaxTypes.Codes.SUR);

			ZString IClassificationLine1.SurtaxUnitOfMeasure => line.DutyAndTaxManager.GetUnitOfMeasure(DutyAndTaxTypes.Codes.SUR, getSpecifiedTypeForSIMA: true);

			ZString IClassificationLine1.SurtaxStatementCode => CusEntryLine.GetStatementCodeViaExemptCode(line.DutyAndTaxManager.GetExemptCode(DutyAndTaxTypes.Codes.SUR, getSpecifiedTypeForSIMA: true));

			ZString IClassificationLine1.SurtaxCode => line.DutyAndTaxManager.GetCode(DutyAndTaxTypes.Codes.SUR, getSpecifiedTypeForSIMA: true);

			ZBool IClassificationLine1.SurtaxIsOverride => line.DutyAndTaxManager.GetIsOverride(DutyAndTaxTypes.Codes.SUR, getSpecifiedTypeForSIMA: true);

			ZBool IClassificationLine1.HasSurtax => line.DutiesAndTaxes.Any(x => x.C1_TaxType == DutyAndTaxTypes.Codes.SUR);

			ZDecimal IClassificationLine1.ADDAmount => GetTotalAmountFromDutiesAndTaxes(DutyAndTaxTypes.Codes.ADD);

			ZDecimal IClassificationLine1.ADDQuantity => GetTotalQuantityFromDutiesAndTaxes(DutyAndTaxTypes.Codes.ADD);

			ZString IClassificationLine1.ADDUnitOfMeasure => line.DutyAndTaxManager.GetUnitOfMeasure(DutyAndTaxTypes.Codes.ADD, getSpecifiedTypeForSIMA: true);

			ZString IClassificationLine1.ADDCode => line.DutyAndTaxManager.GetCode(DutyAndTaxTypes.Codes.ADD, getSpecifiedTypeForSIMA: true);

			ZBool IClassificationLine1.ADDIsOverride => line.DutyAndTaxManager.GetIsOverride(DutyAndTaxTypes.Codes.ADD, getSpecifiedTypeForSIMA: true);

			ZBool IClassificationLine1.HasADD => line.DutiesAndTaxes.Any(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ADD);

			ZDecimal IClassificationLine1.CVDAmount => GetTotalAmountFromDutiesAndTaxes(DutyAndTaxTypes.Codes.CVD);

			ZDecimal IClassificationLine1.CVDQuantity => GetTotalQuantityFromDutiesAndTaxes(DutyAndTaxTypes.Codes.CVD);

			ZString IClassificationLine1.CVDUnitOfMeasure => line.DutyAndTaxManager.GetUnitOfMeasure(DutyAndTaxTypes.Codes.CVD, getSpecifiedTypeForSIMA: true);

			ZString IClassificationLine1.CVDCode => line.DutyAndTaxManager.GetCode(DutyAndTaxTypes.Codes.CVD, getSpecifiedTypeForSIMA: true);

			ZBool IClassificationLine1.CVDIsOverride => line.DutyAndTaxManager.GetIsOverride(DutyAndTaxTypes.Codes.CVD, getSpecifiedTypeForSIMA: true);

			ZBool IClassificationLine1.HasCVD => line.DutiesAndTaxes.Any(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CVD);

			ZDecimal IClassificationLine1.SafeguardAmount => GetTotalAmountFromDutiesAndTaxes(DutyAndTaxTypes.Codes.SAF);

			ZString IClassificationLine1.SafeguardCode => line.DutyAndTaxManager.GetCode(DutyAndTaxTypes.Codes.SAF);

			ZString IClassificationLine1.SafeguardStatementCode => CusEntryLine.GetStatementCodeViaExemptCode(line.DutyAndTaxManager.GetExemptCode(DutyAndTaxTypes.Codes.SAF));

			ZBool IClassificationLine1.SafeguardIsOverride => line.DutyAndTaxManager.GetIsOverride(DutyAndTaxTypes.Codes.SAF);

			ZBool IClassificationLine1.HasSafeguard => line.DutiesAndTaxes.Any(x => x.C1_TaxType == DutyAndTaxTypes.Codes.SAF);

			ZString IClassificationLine1.SIMACode => line.DutyAndTaxManager.GetExemptCode(DutyAndTaxTypes.Codes.SIMADuty);

			ZString IClassificationLine1.SIMAStatementCode => CusEntryLine.GetStatementCodeViaExemptCode(((IClassificationLine1)this).SIMACode);

			ZDecimal IClassificationLine1.SIMAAssessment => line.DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.SIMADuty);

			ZDecimal IClassificationLine1.ExciseDutyAmount => line.DutyAndTaxManager.Duties.Where(x => x.IsEXDDuty).Sum(x => x.C1_Amount);

			ZString IClassificationLine1.ExciseExemptionCode => line.DutyAndTaxManager.GetExemptCode(DutyAndTaxTypes.Codes.ExciseTax);

			ZString IClassificationLine1.ExciseCode => line.DutyAndTaxManager.GetCode(DutyAndTaxTypes.Codes.ExciseTax);

			ZDecimal IClassificationLine1.ExciseTaxRate => line.DutyAndTaxManager.GetRate(DutyAndTaxTypes.Codes.ExciseTax);

			public ZDecimal ExciseTaxRateToPrint => ((IClassificationLine1)this).ExciseTaxAmount == ZDecimal.Zero ? ZDecimal.Zero : ((IClassificationLine1)this).ExciseTaxRate;

			ZString IClassificationLine1.ExciseTaxRateType => line.DutyAndTaxManager.GetRateType(DutyAndTaxTypes.Codes.ExciseTax);

			ZDecimal IClassificationLine1.ExciseTaxAmount => line.DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.ExciseTax);

			bool IClassificationLine1.IsDummyExciseTaxRate => false;

			ZBool IClassificationLine1.HasExcise => line.DutiesAndTaxes.Any(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax);

			ZString IClassificationLine1.GSTCode => line.DutyAndTaxManager.GetCode(DutyAndTaxTypes.Codes.GST);

			ZString IClassificationLine1.GSTExemptionCode => line.DutyAndTaxManager.GetExemptCode(DutyAndTaxTypes.Codes.GST);

			ZDecimal IClassificationLine1.RateOfGST => line.DutyAndTaxManager.GetRate(DutyAndTaxTypes.Codes.GST);

			ZString IClassificationLine1.GSTRateType => line.DutyAndTaxManager.GetRateType(DutyAndTaxTypes.Codes.GST);

			ZDecimal IClassificationLine1.GSTAmount => line.DutyAndTaxManager.GetAmount(DutyAndTaxTypes.Codes.GST);

			bool IClassificationLine1.HasGSTDetails => line.DutyAndTaxManager.GSTaxes.Any();

			ZDecimal IClassificationLine1.CUDAmount => ZDecimal.Zero;

			IEnumerable<IClassificationLine2> IClassificationLine1.ClassificationLines => ClassificationLines;

			BusinessObject IClassificationLine1.RelevantLine => line;

			IEnumerable<IClassificationLine2> ClassificationLines
			{
				get { return B3XClassificationLine2.GetClassificationLines(this.line); }
			}

			public ZInt CountOfConsolidatedLines => ZInt.Zero;

			public MessageSubTypes MessageSubType => MessageSubTypes.Create;

			ZDecimal GetTotalAmountFromDutiesAndTaxes(params ZString[] taxTypes)
			{
				return GetTotalValueFromDutiesAndTaxes((x) => x.C1_Amount, taxTypes);
			}

			ZDecimal GetTotalQuantityFromDutiesAndTaxes(params ZString[] taxTypes)
			{
				return GetTotalValueFromDutiesAndTaxes((x) => x.Quantity, taxTypes);
			}

			ZDecimal GetTotalValueFromDutiesAndTaxes(Func<DutyAndTax, ZDecimal> getValue, params ZString[] taxTypes)
			{
				return line.DutiesAndTaxes.Where(x => taxTypes.Contains(x.C1_TaxType)).Sum(x => getValue(x));
			}
		}
		#endregion

		#region ClassificationLine2

		class B3XClassificationLine2 : IClassificationLine2
		{
			B3XClassificationLine2() { }

			public static IEnumerable<IClassificationLine2> GetClassificationLines(JobComInvoiceLine line)
			{
				var classLines = new List<B3XClassificationLine2>();
				if (line.DutyAndTaxManager.Duties.Any())
				{
					var lineNo = line.CA_OriginalLineNo;
					foreach (var tax in line.DutyAndTaxManager.Duties.OrderBy(a => a, new DutyAndTaxComparer()))
					{
						var customsUnits = line.Tariff != null ? line.Tariff.TariffUnits : line.JI_CustomsUnitQty;
						var b3xClassLine2 = new B3XClassificationLine2 { B3LineNumber = -1 };

						b3xClassLine2.UnitOfMeasureCode = tax.C1_UnitOfMeasure.IsEmpty ? customsUnits : tax.C1_UnitOfMeasure;
						b3xClassLine2.CustomsDutyRate = tax.C1_Rate;
						b3xClassLine2.CustomsDutyRateType = tax.C1_RateType;
						b3xClassLine2.PreviousTransactionNumber = line.InvoiceHeader.JobDeclaration?.CA_OriginalTransactionNo ?? ZString.Empty;
						b3xClassLine2.PreviousLineNumber = JobComInvoiceLine.GetEffectiveOriginalLineNo(lineNo);
						b3xClassLine2.CustomsDutyAmount = line.DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.CustomsDuty);

						var customsQuantity = customsUnits.IsEmpty ? ZDecimal.Zero : line.JI_CustomsQuantity;
						b3xClassLine2.ClassificationLineQuantity += tax.Quantity.IsEmpty ? customsQuantity : tax.Quantity;

						if (line.InvoiceHeader.EffectiveGrossWeight != ZWeight.Empty)
						{
							b3xClassLine2.WeightInKGM = GetB3XGrossWeight(line.InvoiceHeader.EffectiveGrossWeight);
						}
						if (!line.JI_InvoiceUQ.IsEmpty)
						{
							b3xClassLine2.InvoiceUnitOfMeasureCode = line.JI_InvoiceUQ;
						}

						b3xClassLine2.ClassificationLineInvoiceQuantity = line.JI_InvoiceQuantity;
						classLines.Add(b3xClassLine2);
					}
				}
				return classLines.Cast<IClassificationLine2>();
			}

			static ZDecimal GetB3XGrossWeight(ZWeight weight)
			{
				var safeValue = weight.InKilogramsSafe;
				if (safeValue >= 0 && safeValue <= 1)
				{
					return 1;
				}
				else
				{
					return safeValue.Round(0);
				}
			}

			#region Implementation of IClassificationLine2

			public ZInt B3LineNumber { get; private set; }
			public ZString UnitOfMeasureCode { get; private set; }
			public ZDecimal ClassificationLineQuantity { get; private set; }
			public ZDecimal WeightInKGM { get; private set; }
			public ZDecimal CustomsDutyRate { get; private set; }
			public ZString CustomsDutyRateType { get; private set; }
			public ZDecimal CustomsDutyAmount { get; private set; }
			public ZString PreviousTransactionNumber { get; private set; }
			public ZInt PreviousLineNumber { get; private set; }
			public ZString InvoiceUnitOfMeasureCode { get; private set; }
			public ZDecimal ClassificationLineInvoiceQuantity { get; private set; }

			#endregion
		}

		#endregion

		readonly JobDeclaration declaration;
		#endregion
	}
}
