using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.GUI.Testing
{
	[TestedType(typeof(ESH7AsycudaBillControlTemplate))]
	sealed class ESH7AsycudaBillControlTemplateTest : TestCaseWithFactory
	{
		public void TestComponents()
		{
			using (var control = new ESH7AsycudaBillControlTemplate())
			{
				control.Show();

				var documentationRequiredTextBoxControl = control.FindSingle<ZTextBox>("DocumentationRequiredTextBox");
				var g3LocalReferenceNumberTextBox = control.FindSingle<ZTextBox>("G3LocalReferenceNumberTextBox");
				var g3MovementReferenceNumberTextBox = control.FindSingle<ZTextBox>("G3MovementReferenceNumberTextBox");
				var h7MovementReferenceNumberTextBox = control.FindSingle<ZTextBox>("H7MovementReferenceNumberTextBox");

				CombineAssertions(() =>
				{
					AssertEquals("BindingMember", "DocumentationRequiredDescription", documentationRequiredTextBoxControl.GetBindingMember());
					AssertEquals("BindingMember", "G3LocalReferenceNumber", g3LocalReferenceNumberTextBox.GetBindingMember());
					AssertEquals("BindingMember", "G3MovementReferenceNumber", g3MovementReferenceNumberTextBox.GetBindingMember());
					AssertEquals("BindingMember", "H7MovementReferenceNumber", h7MovementReferenceNumberTextBox.GetBindingMember());
				});
			}
		}
	}
}
