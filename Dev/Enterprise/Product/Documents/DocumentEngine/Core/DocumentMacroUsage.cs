using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ResourceStrings.Business;

namespace Enterprise.DocumentEngine
{
	public abstract class DocumentMacroUsage : NonPersistentBusinessObject, IDocBuilderUsage, IObsoleteValidation
	{
		protected DocumentMacroUsage()
		{ }

		protected DocumentMacroUsage(string macro, string templateName)
		{
			this.Macro = macro;
			this.TemplateName = templateName;
		}

		public ZString Macro { get; set; }
		public ZString TemplateName { get; set; }

		public abstract ZString AllDocumentNames
		{
			get;
		}
	}
}
