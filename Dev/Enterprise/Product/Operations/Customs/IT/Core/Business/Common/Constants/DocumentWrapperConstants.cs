namespace Enterprise.Customs.IT.Business;

public static class DocumentWrapperConstants
{
	public static class FullNames
	{
#pragma warning disable CW1161
		public const string SADH = "Enterprise.Customs.IT.Business.ITDocSADH, Enterprise.Customs.IT.Business";
		public const string ITSadAttachment = "Enterprise.Customs.IT.Business.ITDocSADH, Enterprise.Customs.IT.Business";
		public const string NctsHeaderAccompanyingDocument = "Enterprise.Customs.IT.NCTS.Business.NctsHeaderDocumentWrapper, Enterprise.Customs.IT.NCTS.Business";
		public const string ITTADAttachment = "Enterprise.Customs.IT.NCTS.Business.ITNctsHeaderAttachmentDocumentWrapper, Enterprise.Customs.IT.NCTS.Business";
		public const string NctsHeaderPhase5TransitAccompanyingDocument = "Enterprise.Customs.IT.NCTS.Business.Phase5NctsHeaderTADDocumentWrapper, Enterprise.Customs.IT.NCTS.Business";
#pragma warning restore CW1161
	}

	public static class MenuItemName
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant EAD")]
		public const string EAD = "Export Accompanying Doc (EAD) and ELOI";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant SADHC88 values")]
		public static class SADHC88
		{
			public const string SADH = "SADH C88";
			public const string CopyC = "SADH C88 Copy C";
			public const string Copy1 = "SADH C88 Copy 1";
			public const string Copy3 = "SADH C88 Copy 3";
			public const string Copy3A = "SADH C88 Copy 3A";
			public const string Copy3B = "SADH C88 Copy 3B";
			public const string Copy6 = "SADH C88 Copy 6";
			public const string Copy7 = "SADH C88 Copy 7";
			public const string Copy8 = "SADH C88 Copy 8";
			public const string Copy8R = "SADH C88 Copy 8R";
			public const string CopyI = "SADH C88 Copy I";
		}
	}
}
