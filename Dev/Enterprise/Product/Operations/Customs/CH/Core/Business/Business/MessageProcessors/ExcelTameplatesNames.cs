using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business
{
	public static class ExcelTemplatesNames
	{
		public static string EvvRefundDutiesTemplateName { get; } = (NoResString)"RBD (eVV)";
		public static string EvvRefundVATTemplateName { get; } = (NoResString)"RBV (eVV)";
		public static string EvvDutiesTemplateName { get; } = (NoResString)"Customs Duties (eVV)";
		public static string EvvVATTemplateName { get; } = (NoResString)"VAT (eVV)";
		public static string EvvValidationReportTemplateName { get; } = (NoResString)"Validation Report (eVV)";
	}
}

