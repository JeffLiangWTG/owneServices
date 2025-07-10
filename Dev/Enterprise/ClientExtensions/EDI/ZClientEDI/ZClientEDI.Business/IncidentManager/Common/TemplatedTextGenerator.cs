using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public abstract class TemplatedTextGenerator<BizoT>
		where BizoT : BusinessObject
	{
		protected TemplatedTextGenerator(BizoT businessObject)
		{
			BizO = businessObject;
		}

		protected BizoT BizO { get; private set; }

		#region GenerateTemplatedText

		public string GenerateTemplatedText(string textTemplate)
		{
			return GenerateTemplatedText(textTemplate, null, null, null);
		}

		public string GenerateTemplatedText(string textTemplate, string htmlLink, string textHeader, string textFooter)
		{
			MatchCollection macroPoints = replacedValuesRegex.Matches(textTemplate);
			StringBuilder result = new StringBuilder(textTemplate);

			if (macroPoints != null)
			{
				foreach (Match macro in macroPoints)
				{
					if (CanSubstituteMacro(macro.Value))
					{
						result.Replace(macro.Value, GetMacroSubstitute(macro.Value));
					}

					if (htmlLink != null)
					{
						result.Replace(HtmlMacro, htmlLink);
					}
					if (textHeader != null)
					{
						result.Replace(HeaderMacro, textHeader);
					}
					if (textFooter != null)
					{
						result.Replace(FooterMacro, textFooter);
					}
				}
			}
			return result.ToString();
		}

		static readonly Regex replacedValuesRegex = new Regex(@"\<\<\w+\>\>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		const string HtmlMacro = "<<HTMLLink>>";
		const string HeaderMacro = "<<Header>>";
		const string FooterMacro = "<<Footer>>";

		#endregion

		#region Value Provider

		public bool CanSubstituteMacro(string macro)
		{
			return SupportedMacros.Contains(macro);
		}

		public string GetMacroSubstitute(string macro)
		{
			return CanSubstituteMacro(macro) ? GetMacroValueCore(macro) : null;
		}

		protected abstract string GetMacroValueCore(string macro);

		public List<string> SupportedMacros
		{
			get { return supportedMacros ?? (supportedMacros = GetSupportedMacrosList()); }
		}
		List<string> supportedMacros;

		protected abstract List<string> GetSupportedMacrosList();

		#endregion
	}
}


