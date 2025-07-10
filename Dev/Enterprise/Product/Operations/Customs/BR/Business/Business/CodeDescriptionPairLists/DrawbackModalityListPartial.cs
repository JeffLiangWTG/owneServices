namespace Enterprise.Customs.BR.Business
{
	public partial class DrawbackModalityList
	{
		public static bool IsSuspension(string code) => code == Codes.GenericSuspension || code == Codes.NonGenericSuspension;

		public static bool IsExemption(string code) => code == Codes.ExemptionWeb || code == Codes.ExemptionPaper;
	}
}
