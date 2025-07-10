namespace Enterprise.Customs.IT.Business;

public static class SADConstants
{
	public static class RepresentationTypeList
	{
		public const string Self = "1";
		public const string Direct = "2";
		public const string Indirect = "3";
	}

	public static class CertificateFlag
	{
		public const string DER = "DER";
		public const string PAP = "PAP";
	}

	public static class CustomsFieldMaxLength
	{
		public const int MeansOfTransportCrossingBorderIdentity = 27;
		public const int PlaceOfUnloading = 17;
		public const int AuthorisationReferenceWithCin = 7;

		public static class Trader
		{
			public const int Name = 35;
			public const int Address = 35;
			public const int PostCode = 9;
			public const int City = 35;
		}

		public static class EntryHeader
		{
			public const int EntryLinesCountLimit = 40;
		}

		public static class EntryLine
		{
			public const int MarksAndNumbers = 42;
			public const int Notes = 500;
			public const int DescriptionForImportDeclarations = 140;
			public const int DescriptionForUCC6 = 512;
			public const int DescriptionForUcc6TransitionPeriod = 280;
			public const int CombinedNomenclatureCodeLength = 8;
		}

		public static class SupportingDocument
		{
			public const int ReferenceForUCC6 = 70;
			public const int ReferenceForImportOrTransitionPeriod = 35;
		}

		public static class EntryCustomsOffice
		{
			public const int Name = 30;
		}

		public static class Package
		{
			public const int MarksAndNos = 512;
			public const int MarksAndNosForExport = 42;
		}

		public static class AdditionalInfo
		{
			public const int DescriptionForExport = 70;
		}
	}

	public static class CustomsInterchangeType
	{
		public const string IdocR = "R";
		public const string IrispX = "X";
		public const string IdocT = "T";
		public const string Ivisto = "Q";
	}

	public static class MessageSubTypes
	{
		public const string IM = "IM";
		public const string NB = "NB";
		public const string ET = "ET";
		public const string NBE = "NBE";
	}
}
