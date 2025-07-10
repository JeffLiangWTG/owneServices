using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class TextAndLanguageProvider : ITextAndLanguage
	{
		public TextAndLanguageProvider(ZString text) : this(text, string.Empty)
		{
		}

		public TextAndLanguageProvider(ZString text, ZString language)
		{
			this.text = text;
			this.language = language;
		}
		readonly ZString text;
		readonly ZString language;

		public string Text => text;

		public string Language
		{
			get
			{
				var result = ZString.Empty;
				if (!text.IsEmpty)
				{
					result = (language.IsEmpty ? GlbBranch.CurrentBranch.OrgProxy?.OH_Language ?? GlbBranch.CurrentBranch.Language : language).ToLower();
				}
				return result;
			}
		}
	}
}
