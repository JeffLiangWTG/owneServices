using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.CN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.GUI.Testing
{
	[TestedType(typeof(EntryNumbersUpdateForm))]
	class EntryNumbersUpdateFormTest : ZFormBasherTest
	{
		public void TestControls()
		{
			using (var form = new EntryNumbersUpdateForm(new EntryNumbersBO(Factory.New<CusEntryHeader>())))
			{
				form.Show();
				Application.DoEvents();
				var preEntryNumberTextBox = form.Controls.Find("PreEntryNumberTextBox", true).First() as ZTextBox;
				var declarationUnifiedNumberTextBox = form.Controls.Find("DeclarationUnifiedNumberTextBox", true).First() as ZTextBox;
				var cIQNumberTextBox = form.Controls.Find("CIQNumberTextBox", true).First() as ZTextBox;
				var movementReferenceNumberTextBox = form.Controls.Find("MovementReferenceNumberTextBox", true).First() as ZTextBox;
				var movementReferenceNumberDateEdit = form.Controls.Find("MovementReferenceNumberDateEdit", true).First() as ZDateEdit;
				AssertNotNull(preEntryNumberTextBox);
				AssertNotNull(declarationUnifiedNumberTextBox);
				AssertNotNull(cIQNumberTextBox);
				AssertNotNull(movementReferenceNumberTextBox);
				AssertNotNull(movementReferenceNumberDateEdit);
			}
		}

		protected override Form GetFormToBashCore() => new EntryNumbersUpdateForm(new EntryNumbersBO(Factory.New<CusEntryHeader>()));
	}
}
