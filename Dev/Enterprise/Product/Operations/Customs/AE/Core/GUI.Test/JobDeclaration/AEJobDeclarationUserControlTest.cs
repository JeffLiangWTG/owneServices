using System.Windows.Forms;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AE.GUI.Testing;

	[TestedType(typeof(AEJobDeclarationUserControl))]
	class AEJobDeclarationUserControlTest : BaseCustomsDeclarationUserControlAbstractTest<AEJobDeclarationUserControl, JobDeclaration>
	{
		public void TestMarksAndNumbersPopupRefreshesAndValidatesMarksAndNumbersShort()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageSubTypeList.Codes.Import;
			using (ZForm form = new ZForm(declaration))
			using (var control = new AEJobDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				declaration.JE_MarksAndNumbers = "Line1";
				AssertNoMessageErrors(declaration.JE_MarksAndNumbersShortInfo);
				var note = declaration.Notes.FindByDescription(ZArchitecture.Business.PredefinedNoteTypes.Instance.MarksAndNumbers.Description)[0];
				note.ST_NoteText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do";
				AssertEquals("Expected JE_MarksAndNumbers to match ST_NoteText, but they are different.", declaration.JE_MarksAndNumbers, "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do");
				AssertEquals("Expected JE_MarksAndNumbersShort to be a truncated (max 35 characters) version of JE_MarksAndNumbers", declaration.JE_MarksAndNumbersShort, "Lorem ipsum dolor sit amet, consect");
			}
		}
	}
