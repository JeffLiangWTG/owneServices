namespace Enterprise.Customs.BR.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant strings")]
	public static class Constants
	{
		public const string DataFormat = "dd/MM/yyyy";

		public static class BRTransportModes
		{
			public const string River = "RIV";
			public const string Lake = "LAK";
		}

		public static class EntryStatus
		{
			public const string Registered = "E10";
			public const string CanceledByExporter = "E80";
			public const string CanceledDueToExpiration = "E81";
			public const string CanceledByRFB = "E82";
			public const string CanceledByRFBByRequest = "E83";
			public const string NotSent = "NST";
		}

		public static class TariffTypes
		{
			public const string NCCA = "NCCA";
			public const string NALADIHS = "NAHS";
		}

		public static class Profile
		{
			public static class Types
			{
				public const string NCM = "NCM";
				public const string NCMTE = "NCMTE";
				public const string NVE = "NVE";
				public const string LPC = "LPC";
				public const string LPCT = "LPCT";
				public const string TTDTY = "TTDTY";
				public const string TTPIS = "TTPIS";
				public const string TTCOF = "TTCOF";
				public const string TTADD = "TTADD";
				public const string TTIPI = "TTIPI";
				public const string TTDTYTE = "TTDTYTE";
				public const string TTPISTE = "TTPISTE";
				public const string TTCOFTE = "TTCOFTE";
				public const string TTADDTE = "TTADDTE";
				public const string TTIPITE = "TTIPITE";
			}

			public static class AttributeNames
			{
				public const string LegalCode = "LegalCode";
				public const string Mandatory = "Mandatory";
				public const string Modality = "Modality";
				public const string Origin = "Origin";
				public const string Regime = "Regime";
				public const string TaxType = "TaxType";
			}

			public static class AnswerValues
			{
				public const string Yes = "S";
				public const string No = "N";
			}
		}

		public static class ProfileQuestion
		{
			public static class AttributeValues
			{
				public const string Duimp = "5";
				public const string Product = "7";
			}

			public static class AttributeNames
			{
				public const string Target = "objetivo";
			}
		}

		public static class CustomsOfficeCodes
		{
			public const string BoardingOffice = "BOA";
			public const string EntranceOffice = "ENT";
		}

		public static class RefCusMapType
		{
			public const string ModalTransport = "TRANS";
		}

		public static class CustomMsgAttributes
		{
			public const string MessageRequirement = "custom.MessageRequirement";
			public const string SubscriptionId = "custom.SubscriptionId";
			public const string VersionNumber = "custom.BR.VersionNumber";
			public const string ItemNumber = "custom.ItemNumber";
			public const string RootCNPJ = "custom.BR.CNPJ";
			public const string DisplayDisabled = "custom.BR.DisplayDisabled";
		}

		public static class SiscomexUsageFee
		{
			public const string Code = "5629";
		}

		public static class RateCodes
		{
			public const string ImportDuty = "0086";
			public const string Cofins = "5629";
			public const string PIS = "5602";
			public const string IPI = "1038";
			public const string Antidumping = "5529";
			public const string ICMSFCP = "FCP";
		}

		public static class RateTypes
		{
			public const string Cofins = "COF";
			public const string PIS = "PIS";
			public const string IPI = "IPI";
			public const string ICMS = "ICM";
			public const string Antidumping = "ADD";
			public const string OtherExpensesICMS = "EIC";
			public const string ICMSFCP = "FCP";
			public const string ImportDuty = "DTY";
		}

		public static class RatePreferenceType
		{
			public const string Normal = "NORMAL";
			public const string ExTariff = "EXTARIFF";
			public const string FreeTradeAgreement = "FTA";
			public const string ReductionMargin = "REDUCTION";
			public const string ReducedRate = "REDUCED";
		}

		public static class MethodOfCalculation
		{
			public const string Percentage = "%";
			public const string FiftyPercent = "50%";
		}

		public static class ProcedureCategories
		{
			public const string Duty = "DTY";
			public const string PisCofins = "PIS";
			public const string ICMS = "ICMS";
		}

		public static class FilterConstants
		{
			public static class Declaration
			{
				public const string ShipmentType = "Shipment Type";
				public const string ImporterSupplier = "Importer/Supplier";
				public const string EntryNumber = "Entry #";
			}

			public static class CommercialInvoice
			{
				public const string ShipmentType = "Shipment Type";
				public const string ImporterSupplier = "Importer / Supplier";
				public const string AttachedToDeclaration = "Attached to a Declaration";
			}

			public static class ForeignOperator
			{
				public const string Owner = "Owner";
				public const string Status = "Status";
				public const string MessageStatus = "Message Status";
				public const string AuthorityIdentifier = "Authority Identifier";
			}
		}

		public static class TariffAgreementTypes
		{
			public const string SGPC = "SGPC";
			public const string OMC = "OMC";
			public const string Aladi = "Aladi";
		}

		public static class RefCusCodeList
		{
			public static class Attributes
			{
				public const string Type = "Type";
				public const string LegalActInImportEntry = "LegalActInImportEntry";
				public const string TradeGroup = "TradeGroup";
				public const string Country = "Country";
				public const string AgreementCodeInImportEntry = "AgreementCodeInImportEntry";
			}
		}

		public static class EDIMessageStatusCodes
		{
			public const string Manual = "MAN";
		}

		public static class RiskChannelTypes
		{
			public const string Green = "VERDE";
			public const string Red = "VERMELHO";
			public const string Yellow = "AMARELO";
			public const string Gray = "CINZA";
			public const string Orange = "LARANJA";
		}

		public static class Situation
		{
			public const string Draft = "RASCUNHO";
			public const string Active = "ATIVADO";
			public const string Deactive = "DESATIVADO";
		}

		public static class ResponseCodes
		{
			public const int Update = 200;
			public const int Success = 201;
		}

		public static class ImporterRegistrationNumberTypes
		{
			public const string CNPJ = "CNPJ";
			public const string CPF = "CPF";
		}

		public static class CargoIdentificationType
		{
			public const string RUC = "RUC";
			public const string CE = "CE";
		}

		public static class OperationType
		{
			public const string Import = "I";
		}

		public static class DuimpAttributesStatus
		{
			public const string Mandatory = "M";
			public const string Optional = "O";
		}

		public static class LegalBasisType
		{
			public const string Normal = "Normal";
			public const string Optional = "Opcional";
		}

		public static class AttributeAnswerDataType
		{
			public const string DecimalNumber = "número real";
			public const string IntegerNumber = "número inteiro";
			public const string DynamicDomain = "domínio dinâmico";
		}
	}
}
