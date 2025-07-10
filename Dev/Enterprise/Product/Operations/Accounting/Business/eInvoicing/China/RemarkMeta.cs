namespace Enterprise.Accounting.Business.eInvoicing.China
{
	public class RemarkMeta
	{
		public RemarkMeta() : this(null, null) { }

		public RemarkMeta(string originalCaptionAndValue, string originalValue, bool hasNoMacro = false)
		{
			OriginalCaptionAndValue = originalCaptionAndValue;
			OriginalValue = originalValue;
			HasNoMacro = hasNoMacro;
		}

		public string OriginalCaptionAndValue { get; set; }
		public string OriginalValue { get; set; }
		public bool HasNoMacro { get; set; }
	}
}
