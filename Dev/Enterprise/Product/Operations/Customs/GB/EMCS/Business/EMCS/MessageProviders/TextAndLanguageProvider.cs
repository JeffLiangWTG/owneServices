using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Business
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

		public string Language => text.IsEmpty ? ZString.Empty : (language.IsEmpty ? (ZString)Core.SharedConstants.Languages.English : language).ToLower();
	}
}
