using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.ZArchitecture.GUI.ZDropEdit;

namespace Enterprise.Customs.AU.GUI.Testing
{
	[TestedType(typeof(AUCOLSLateLodgementReasonUserControl))]
	sealed class AUCOLSLateLodgementReasonUserControlTest : TestCaseWithFactory
	{
		public void TestLongTextFormBinding()
		{
			var quarantineColsHeader = Factory.New<QuarantineColsHeader>();
			quarantineColsHeader.QCH_LateLodgementReason = "Awaiting shipping details";
			using (var testForm = new ZForm(quarantineColsHeader))
			using (var userControl = new AUCOLSLateLodgementReasonUserControl())
			{
				testForm.Controls.Add(userControl);
				testForm.Show();
				userControl.SetDataBinding(quarantineColsHeader, nameof(QuarantineColsHeader.QCH_LateLodgementReason));
				var moreButton = userControl.FindSingle<ZButton>("MoreButton");
				moreButton.PerformClick();
				using (var popup = ZFormModaliser.ActiveForm)
				{
					var textbox = popup.FindSingle<ZTextBox>("LongTextTextBox");
					AssertEquals("Awaiting shipping details", textbox.Text);
				}
			}
		}

		public void TestLongTextTextBoxNotVisible()
		{
			using (var userControl = new AUCOLSLateLodgementReasonUserControl())
			{
				AssertNull("LongTextTextBox not visible", userControl.FindSingleOrDefault<ZTextBox.Bare>("LongTextTextBox"));
			}
		}

		public void TestReasonDropEdit() => CombineAssertions(() =>
		{
			using (var userControl = new AUCOLSLateLodgementReasonUserControl())
			{
				var dropEdit = userControl.ReasonDropEdit;
				AssertType<ZDropEdit>("Type", dropEdit);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, dropEdit.CharacterCasing);
				AssertEquals("ShowInDropDown", ShowInDropDownList.OnlyShowCode, dropEdit.ShowInDropDown);
				AssertEquals("ShowDescriptionBox", false, dropEdit.ShowDescriptionBox);
			}
		});
	}
}
