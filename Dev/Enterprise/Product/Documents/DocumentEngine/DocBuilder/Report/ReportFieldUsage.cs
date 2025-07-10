using CargoWise.Types;

namespace Enterprise.DocumentEngine.DocBuilder
{
	public class ReportFieldUsage : DocumentMacroUsage
	{
		public ReportFieldUsage()
		{ }

		public ReportFieldUsage(string macro, string templateName)
			: base(macro, templateName)
		{ }

		public override ZString AllDocumentNames
		{
			get
			{
				return "";
			}
		}
	}
}
