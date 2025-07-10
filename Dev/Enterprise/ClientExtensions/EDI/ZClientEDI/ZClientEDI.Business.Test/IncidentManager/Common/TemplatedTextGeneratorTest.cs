using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public abstract class TemplatedTextGeneratorTest<GeneratorT, BizoT> : TestCaseWithFactory
		where GeneratorT : TemplatedTextGenerator<BizoT>
		where BizoT : BusinessObject
	{
		public void TestHeaderAndFooter()
		{
			string template = "<<Header>>\n<<HTMLLink>>\nSome text\n<<Footer>>";
			string result = TextGenerator.GenerateTemplatedText(template, "Some link", "Some header", "Some footer");
			AssertEquals("Template header and footer", "Some header\nSome link\nSome text\nSome footer", result);
		}

		public void TestSupportedMacros()
		{
			AssertEquals("Supported macros count", ExpectedSupportedMacros.Count, TextGenerator.SupportedMacros.Count);

			foreach (string macro in ExpectedSupportedMacros)
			{
				AssertCollectionContains(string.Format(CultureInfo.CurrentCulture, "Macro '{0}' should be supported", macro), macro, TextGenerator.SupportedMacros);
				Assert(string.Format(CultureInfo.CurrentCulture, "Macro '{0}' should be supported", macro), TextGenerator.CanSubstituteMacro(macro));
				AssertNotNull(string.Format(CultureInfo.CurrentCulture, "Macro '{0}' substitution should be not null", macro), TextGenerator.GetMacroSubstitute(macro));
			}
		}

		#region Implementation

		protected abstract List<string> ExpectedSupportedMacros { get; }

		protected GeneratorT TextGenerator
		{
			get { return textGenerator ?? (textGenerator = GetTextGeneratorForTest(BizO)); }
		}
		GeneratorT textGenerator;

		protected abstract GeneratorT GetTextGeneratorForTest(BizoT bizO);

		protected BizoT BizO
		{
			get { return bizO ?? (bizO = GetBusinessObjectForTest()); }
		}
		BizoT bizO;

		protected abstract BizoT GetBusinessObjectForTest();

		#endregion
	}
}