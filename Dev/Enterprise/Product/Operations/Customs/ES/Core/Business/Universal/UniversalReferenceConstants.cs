namespace Enterprise.Customs.ES.Business
{
	public static class UniversalReferenceConstants
	{
		public static class ReservedRateFormulaValue
		{
			public const string RetailPriceCode = "PVP";
			public const string GlobalWarmingPotential = "PCA";
		}

		public static class RefCusCodeListTypes
		{
			public const string REACodeDescriptions = "REA";
			public const string CL142 = "CL142";
			public const string DC40A = "DC40A";
			public const string DC40W = "DC40W";
			public const string EXNOSEGU = "EXSEC";
		}

		public static class RefCusCodeListAttributeNames
		{
			public const string IsT2LPous = "IsT2LPous";
		}

		public static class CustomsOfficeCodeList
		{
			public const string Ceuta = "ES0055";
			public const string Melilla = "ES0056";
		}

		public static class EntryStatusCodeList
		{
			public const string Clear = "CLR";
			public const string PermanentClear = "CLP";
		}

		public static class RateCodeCodeList
		{
			public const string AYD = "AYD";
			public const string AYT = "AYT";
		}

		public static class RateTypeList
		{
			public const string REA = "REA";
		}

		public static class FeeMethodOfPayment
		{
			public const string Deferred = "DEF";
			public const string NonBillableTax = "NBL";
		}

		public static class PrimaryPreferenceCode
		{
			public const string REA = "085";
		}

		public static class SupportingDocumentType
		{
			public const string REARebate = "7003";
			public const string TransformedRPP = "7009";
			public const string FluorinatedGases = "7015";
			public const string AmountToBeGuaranteedAEAT = "7018";
			public const string AmountToBeGuaranteedATC = "7019";
			public const string VATAdditionsCode = "7002";
			public const string VATReductionCode = "9015";
			public const string T2L = "N825";
			public const string T2LF = "C620";
			public const string WHLOCAuthorisation = "5018";
			public const string CentralizedClearance = "C513";
			public const string EntryOfDataInDeclarantsRecords = "C514";
			public const string ComplYDoc = "1220";
		}

		public static class PreviousDocumentType
		{
			public const string C651 = "C651";
			public const string NMRN = "NMRN";
		}

		public static class SupportingDocumentSubType
		{
			public const string LIQ = "LIQ";
			public const string H1S = "H1S";
		}

		public static class DeclarationResponseCode
		{
			public const string Accepted = "962";
			public const string Rejected = "963";
		}

		public static class RefCusTariffType
		{
			public const string AIEM = "AIEM";
			public const string CANEX = "CANEX";
			public const string ESEXC = "ESEXC";
		}

		public static class RefCusTaxOrFee
		{
			public const string CanaryIslandVATPrefix = "IG";
			public const string VatPrefix = "IV";
		}

		public static class RefCusTaxOrFeeType
		{
			public const string VAT = "VAT";
		}

		public static class RefCusCodeList
		{
			public static class CustomsUq
			{
				public const string AlcoholHectolitres = "HG";
				public const string HectolitresPlate = "HP";
				public const string GigaJoules = "GJ";
				public const string ThousandItems = "MIL";
			}

			public static class PackageType
			{
				public const string Frame = "FR";
			}
		}

		public static class RefCusRateCode
		{
			public const string IGIC = "3IG";
			public const string AIEM = "3AI";
			public const string EquivalenceSurcharge = "B01";
			public const string RetailerSurcharge = "3RM";
			public const string NonRecycledPlasticFee = "1PL";
			public const string FluorinatedGases = "1CF";
		}

		public static class RefCusMapType
		{
			public const string RetailerFee = "ESREQ";
		}
	}
}
