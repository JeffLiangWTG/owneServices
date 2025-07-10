using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business
{
	public static class Constants
	{
		public static class Numbers
		{
			public const string One = "1";
		}

		public static class LanguageType
		{
			public const string English = "EN";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Event reference")]
		public static class SendingMessageEventList
		{
			public const string Reference = "Submitting when Message status is sent";
			public const string Resubmit = "Resubmitting when Entry status is ";
			public const string RES = "RES";
			public const string RFN = "RFN";
		}

		public static class RepresentationTypeList
		{
			public const string Self = "1";
			public const string Direct = "2";
			public const string Indirect = "3";
		}

		public static class ExportOperationSecurity
		{
			public const string NonSecurity = "0";
			public const string Security = "2";
		}

		public static class RefCusCodeListTypes
		{
			public const string IrelandQualifierType = "QUA";
			public const string SpecificCircumstanceIndicatorType = "CL296";
			public const string RevenueErrorType = IE.Messaging.UniversalReferenceConstants.RefCusCodeListTypes.RevenueErrorType;
		}

		public static class TransportDocumentCodes
		{
			public const string _N235 = "N235";
			public const string _N271 = "N271";
			public const string _N703 = "N703";
			public const string _N704 = "N704";
			public const string _N705 = "N705";
			public const string _N710 = "N710";
			public const string _N714 = "N714";
			public const string _N720 = "N720";
			public const string _N722 = "N722";
			public const string _N730 = "N730";
			public const string _N740 = "N740";
			public const string _N741 = "N741";
			public const string _N750 = "N750";
			public const string _N760 = "N760";
			public const string _N785 = "N785";
			public const string _N787 = "N787";
			public const string _N952 = "N952";
			public const string _N955 = "N955";
			public static readonly ImmutableArray<ZString> DocumentTypesForBR1110Validation = new ZString[] { _N703, _N740, _N741 }.ToImmutableArray();
		}

		public static class AdditionalReferenceCodes
		{
			public const string EstimatedTimeOfDeparture = "1D23";
			public const string RoRoUnaccompaniedTrailerRegistrationNumber = "1D95";
			public const string RoRoShipID = "1D94";
			public const string AuthorisationForSpecialProcedureOtherThanTransit = "00100";
			public const string _1A01 = "1A01";
			public const string _1A05 = "1A05";
			public const string _1A06 = "1A06";
			public const string Prefix_6 = "6";

			public static bool IsEstimatedDeparture(ZString code) => code.EqualsIgnoringCase(EstimatedTimeOfDeparture);
			public static bool IsEstimatedDeparture<TAddInfo>(TAddInfo info) where TAddInfo : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo
				=> IsEstimatedDeparture(info.CSI_Code);
		}

		public static class AdditionalReferenceNumberCodes
		{
			public const string NAI = "NAI";
		}

		public static class CusEntryLineFeeRefundDutyMethodOfCalculation
		{
			public const string ConfirmedRelease = "RFR";
			public const string ConfirmedAmendment = "RFA";
			public const string DifferenceForRefunds = "RFD";
			public const string ToBeRepaid = "RFP";

			public static readonly ImmutableArray<string> All = ImmutableArray.Create(
				ConfirmedRelease, ConfirmedAmendment, DifferenceForRefunds, ToBeRepaid
			);
		}

		public static class ExportAdditionalInformationCodes
		{
			public const string SingleContract = "30500";
		}

		public static class ProcedureCodesForBR2041
		{
			public const string _4051 = "4051";
			public const string _4053 = "4053";
			public const string _4054 = "4054";
			public const string _4071 = "4071";
			public const string _5151 = "5151";
			public const string _5153 = "5153";
			public const string _5154 = "5154";
			public const string _5171 = "5171";
			public const string _5353 = "5353";
			public const string _7151 = "7151";
			public const string _7153 = "7153";
			public const string _7171 = "7171";
		}

		public static class PreviousDocumentTypeCodes
		{
			public const string MRN = "MRN";
			public const string SAD = "SAD";
		}
		public static class SupportingDocumentReferenceReferenceNumbers
		{
			public const string IEPOSTPONED = "IEPOSTPONED";
		}

		public static class SupportingDocumentCodes
		{
			public const string _1A01 = "1A01";
			public const string _1A02 = "1A02";
			public const string _1A03 = "1A03";
			public const string _1A04 = "1A04";
			public const string _1A05 = "1A05";
			public const string _1A06 = "1A06";
			public const string _1D02 = "1D02";
			public const string _1D03 = "1D03";
			public const string _1D04 = "1D04";
			public const string _1D24 = "1D24";
			public const string _1D96 = "1D96";
			public const string _1N09 = "1N09";
			public const string _1N21 = "1N21";
			public const string _1N22 = "1N22";
			public const string _1N99 = "1N99";
			public const string _1Q75 = "1Q75";
			public const string _1Q99 = "1Q99";
			public const string _C057 = "C057";
			public const string _C079 = "C079";
			public const string _C082 = "C082";
			public const string _C085 = "C085";
			public const string _C100 = "C100";
			public const string _C501 = "C501";
			public const string _C502 = "C502";
			public const string _C503 = "C503";
			public const string _C512 = "C512";
			public const string AuthorisationInwardProcessingProcedure = "C601";
			public const string _C640 = "C640";
			public const string _C644 = "C644";
			public const string _C678 = "C678";
			public const string _C990 = "C990";
			public const string _D005 = "D005";
			public const string _D008 = "D008";
			public const string _L100 = "L100";
			public const string _N018 = "N018";
			public const string _N325 = "N325";
			public const string _N380 = "N380";
			public const string _N740 = "N740";
			public const string _N741 = "N741";
			public const string _N853 = "N853";
			public const string _N864 = "N864";
			public const string _N865 = "N865";
			public const string _N935 = "N935";
			public const string _N954 = "N954";
			public const string _N990 = "N990";
			public const string _U059 = "U059";
			public const string _U110 = "U110";
			public const string _U111 = "U111";
			public const string _U112 = "U112";
			public const string _U116 = "U116";
			public const string _U117 = "U117";
			public const string _U118 = "U118";
			public const string _U162 = "U162";
			public const string _U163 = "U163";
			public const string _U164 = "U164";
			public const string _U165 = "U165";
			public const string _U166 = "U166";
			public const string _U167 = "U167";
			public const string _U168 = "U168";
			public const string _U169 = "U169";
			public const string _U170 = "U170";
			public const string _U171 = "U171";
			public const string _Y120 = "Y120";
			public const string _Y123 = "Y123";
			public const string _Y124 = "Y124";
			public const string _Y125 = "Y125";
			public const string _Y951 = "Y951";
			public const string _Y986 = "Y986";

			public static Dictionary<string, ZString[]> BR20239SpecificCode => new Dictionary<string, ZString[]>()
			{
				{ _C644, new ZString[] { "COI" } },
				{ _L100, new ZString[] { "EXP", "IMP" } },
				{ _C085, new ZString[] { "CHEDPP" } },
				{ _C678, new ZString[] { "CHEDD" } },
				{ _N853, new ZString[] { "CHEDP", "CVEDP" } },
				{ _C640, new ZString[] { "CHEDA" } },
			};

			public static readonly ImmutableArray<ZString> PartiallyStatusBR20329 = new ZString[] { "V", "R" }.ToImmutableArray();

			public static Dictionary<string, ZString[]> LicenceTypeBR20329 => new Dictionary<string, ZString[]>()
			{
				{ "IMP", new ZString[] { "ICUH", "IDST", "IPEA", "IPEO", "IPED", "IPEX" } },
				{ "EXP", new ZString[] { "ECUH", "EHCF", "EHCP", "EHCO", "EPEA", "EPEO", "EPEX" } },
			};

			public static readonly ImmutableArray<ZString> InvoiceDocumentTypesForExport = new ZString[] { _D005, _D008, _N325, _N380, _N864, _N935, _1N09, _1N21, _1N22, _1N99 }.ToImmutableArray();

			public static bool IsEstimatedDestination(ZString code) => code.EqualsIgnoringCase(_1D24);
			public static bool IsEstimatedDestination<TSupportInfo>(TSupportInfo info) where TSupportInfo : EU.Business.Declaration.MultiLineAddInfos.SupportingDocument
				=> IsEstimatedDestination(info.CSI_Code);

			public static readonly ImmutableHashSet<ZString> CodesWithoutVat = new ZString[] { _1A01, _1A02, _1A03, _1A04 }.ToImmutableHashSet();
			public static readonly ImmutableArray<ZString> H7InvalidCodes = new ZString[] { "C019", "C516", "C601", "C990", "1D02", "1D03", "1D04", "N990", "1Q75" }.ToImmutableArray();
			public static readonly ImmutableArray<ZString> H7InvalidCodesV1Only = new ZString[] { "C644", "C640", "C678", "N853", "C100" }.ToImmutableArray();
			public static readonly ImmutableArray<ZString> H7NonC08MandatoryCodes = new ZString[] { "D005", "D008", "N325", "N380", "N864", "N935" }.ToImmutableArray();
			public static readonly ImmutableArray<ZString> H7ExclusiveCodes = new ZString[] { "U164", "U165", "U166", "U167" }.ToImmutableArray();
		}

		public static class AdditionalInformationCodes
		{
			public const string N9001 = "N9001";
			public const string _00100 = "00100";
			public const string _00200 = "00200";
			public const string _00400 = "00400";
			public const string _00500 = "00500";
			public const string _1D94 = "1D94";
			public const string _1D95 = "1D95";
		}

		public static class DateTimeFormat
		{
			public const string ShortDateTime = IE.Messaging.Constants.DateTimeFormat.ShortDateTime;
			public const string AdditionalReferenceDateTime = "yyyyMMddHHmm";
			public const string StandardDate = "dd/MM/yyyy";
		}

		public static class ValidationMessage
		{
			public static MultilingualString AdditionalInfo1D23FullTypeIsRequired => ResString.GetMultilingualString("8D4B3B7B-FFBE-46BD-8B35-92F17A01A8C9", "An Additional Document with Kind = Additional Reference (REF) and Full Type = '1D23' is required.");

			public static string SupportingDocumentMustContainAnInvoiceDocumentWithInvoiceNumber
			{
				get
				{
					if (supportingDocumentMustContainAnInvoiceDocumentWithInvoiceNumber == null)
					{
						supportingDocumentMustContainAnInvoiceDocumentWithInvoiceNumber = ResString.GetMultilingualString("8282DB8C-82EE-45A8-B713-CEBC7AF0BC96", "Supporting Documents must contain at least one of the following codes: {0}, followed by the value of an invoice number.", string.Join(", ", Constants.SupportingDocumentCodes.InvoiceDocumentTypesForExport));
					}
					return supportingDocumentMustContainAnInvoiceDocumentWithInvoiceNumber;
				}
			}
			[ThreadStatic]
			static MultilingualString supportingDocumentMustContainAnInvoiceDocumentWithInvoiceNumber;
		}

		public static class TypeOfLocation
		{
			public const string DesignatedLocation = "A";
		}

		public static class LocationQualifier
		{
			public const string UNLoco = "U";
		}

		public static class IDType
		{
			public const string V = "V";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Remarks string")]
		public static class RemarksTypeList
		{
			public const string RefundApplicationRequired = "Refund Application Required";
			public const string InsufficientFund = "Insufficient Fund";
		}

		public static class PreferenceCode
		{
			public const string Code1 = "1";
			public const string Code2 = "2";
			public const string Code3 = "3";
			public const string Code4 = "4";
			public const string Code5 = "5";
		}

		public static class AdditionalDeclarationTypeList
		{
			public const string C = "C";
			public const string F = "F";
			public const string X = "X";
			public const string Y = "Y";
			public const string Z = "Z";
		}

		public static class RepresentativeStatus
		{
			public const string SameAsDeclarant = "3";
			public const string DifferentFromDeclarant = "2";
		}

		public static class ImportExtendedDeclarationTypeList
		{
			public const string I2 = "I2";
		}

		public static class TariffCodes
		{
			public static readonly ImmutableArray<ZString> H7InvalidTariffCodes = new ZString[] { "3303001000", "3303009000" }.ToImmutableArray();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings - URLs")]
		public static class CustomsExciseReport
		{
			static readonly string dateFormat = "yyyyMMdd";
			public static string UDR => "/transactions/payer-unpaids-report";
			public static string BAL => "/transactions/balance";
			public static string PSR(ZDate periodStart)
			{
				return string.Format("/transactions/periods/{0}/payer-summary-report", periodStart.ToString(dateFormat));
			}

			public static string PCT(ZDate periodStart)
			{
				return string.Format("/transactions/periods/{0}/payer-combined-taxes-report", periodStart.ToString(dateFormat));
			}

			public static string PTT(ZDate periodStart)
			{
				return string.Format("/transactions/periods/{0}/payer-tax-types-report", periodStart.ToString(dateFormat));
			}

			public static string PCI(ZDate periodStart)
			{
				return string.Format("/transactions/periods/{0}/importer-combined-taxes-report", periodStart.ToString(dateFormat));
			}

			public static string DSR(ZDate day)
			{
				return string.Format("/transactions/daily/{0}/payer-summary-report", day.ToString(dateFormat));
			}

			public static string DCT(ZDate day)
			{
				return string.Format("/transactions/daily/{0}/payer-combined-taxes-report", day.ToString(dateFormat));
			}

			public static string DTT(ZDate day)
			{
				return string.Format("/transactions/daily/{0}/payer-tax-types-report", day.ToString(dateFormat));
			}
		}
	}
}
