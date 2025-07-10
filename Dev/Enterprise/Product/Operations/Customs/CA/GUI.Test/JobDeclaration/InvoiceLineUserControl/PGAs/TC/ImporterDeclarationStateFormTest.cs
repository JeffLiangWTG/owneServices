using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(ImporterDeclarationStateForm))]
	sealed class ImporterDeclarationStateFormTest : ZFormBasherTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1199:Do Not Use Unnecessary Resource String In Unit Tests", Justification = "WI: WI00900330. Used to test translatability of a form, thus must use a MultilingualString.")]
		protected override System.Windows.Forms.Form GetFormToBashCore() => new ImporterDeclarationStateForm(ResString.GetMultilingualString("4aa011e7-6e28-40b7-bdf3-91ede999c16b", "AAA"));
	}
}
