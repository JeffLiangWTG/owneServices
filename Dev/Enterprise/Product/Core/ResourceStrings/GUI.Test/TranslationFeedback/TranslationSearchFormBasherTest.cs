using System.Windows.Forms;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.GUI.Testing
{
	[TestedType(typeof(TranslationSearchForm))]
	sealed class TranslationSearchFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new TranslationSearchForm(new TranslationSearchCriteria());
		}
	}
}
