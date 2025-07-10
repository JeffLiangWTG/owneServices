using System.Linq;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(AssignLocationAndReferenceForm))]
	class AssignLocationAndReferenceFormTest : ZFormBasherTest
	{
		#region Overrides

		protected override Form GetFormToBashCore() => new AssignLocationAndReferenceForm();

		#endregion

		public void TestControlsMaxLength()
		{
			using (var assignLocationAndReferenceForm = new AssignLocationAndReferenceForm())
			{
				var locationTextBox = (ZTextBox)assignLocationAndReferenceForm.Controls.Find("LocationTextBox", true).Single();
				var referenceTextBox = (ZTextBox)assignLocationAndReferenceForm.Controls.Find("ReferenceTextBox", true).Single();

				CombineAssertions(() =>
				{
					AssertEquals("LocationTextBox max length is 35", 35, locationTextBox.MaxLength);
					AssertEquals("ReferenceTextBox max length is 50", 50, referenceTextBox.MaxLength);
				});
			}
		}

		public void TestControlsCaption()
		{
			using (var assignLocationAndReferenceForm = new AssignLocationAndReferenceForm())
			{
				var locationTextBox = (ZTextBox)assignLocationAndReferenceForm.Controls.Find("LocationTextBox", true).Single();
				var emptyLocationCheckBox = (ZCheckBox)assignLocationAndReferenceForm.Controls.Find("EmptyLocationCheckBox", true).Single();
				var referenceTextBox = (ZTextBox)assignLocationAndReferenceForm.Controls.Find("ReferenceTextBox", true).Single();
				var emptyReferenceCheckBox = (ZCheckBox)assignLocationAndReferenceForm.Controls.Find("EmptyReferenceCheckBox", true).Single();

				CombineAssertions(() =>
				{
					AssertEquals("LocationTextBox Caption", "Location of Goods", locationTextBox.CaptionResourceString.Caption);
					AssertEquals("EmptyLocationCheckBox Caption", "Leave Empty", emptyLocationCheckBox.CaptionResourceString.Caption);
					AssertEquals("ReferenceTextBox Caption", "Owner Reference", referenceTextBox.CaptionResourceString.Caption);
					AssertEquals("EmptyReferenceCheckBox Caption", "Leave Empty", emptyReferenceCheckBox.CaptionResourceString.Caption);
				});
			}
		}

		public void TestControlsReadOnly()
		{
			using (var assignLocationAndReferenceForm = new AssignLocationAndReferenceForm())
			{
				var locationTextBox = (ZTextBox)assignLocationAndReferenceForm.Controls.Find("LocationTextBox", true).Single();
				var emptyLocationCheckBox = (ZCheckBox)assignLocationAndReferenceForm.Controls.Find("EmptyLocationCheckBox", true).Single();
				var referenceTextBox = (ZTextBox)assignLocationAndReferenceForm.Controls.Find("ReferenceTextBox", true).Single();
				var emptyReferenceCheckBox = (ZCheckBox)assignLocationAndReferenceForm.Controls.Find("EmptyReferenceCheckBox", true).Single();

				CombineAssertions(() =>
				{
					emptyLocationCheckBox.Checked = false;
					emptyReferenceCheckBox.Checked = false;
					AssertEquals("LocationTextBox is not ReadOnly when emptyLocationCheckBox is not checked", false, locationTextBox.ReadOnly);
					AssertEquals("ReferenceTextBox is not ReadOnly when emptyReferenceCheckBox is not checked", false, referenceTextBox.ReadOnly);

					emptyLocationCheckBox.Checked = true;
					emptyReferenceCheckBox.Checked = true;
					AssertEquals("LocationTextBox is ReadOnly when emptyLocationCheckBox is checked", true, locationTextBox.ReadOnly);
					AssertEquals("ReferenceTextBox is ReadOnly when emptyReferenceCheckBox is checked", true, referenceTextBox.ReadOnly);
				});
			}
		}
	}
}
