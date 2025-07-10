using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	public class AutoRatingGUIInteractorTest : TestCase
	{
		[GuiTest]
		public void TestAutoRatingGUIInteractorWillShowWaitCursorInsteadOfProgressForm()
		{
			using (var tempForm = new ZForm())
			{
				var interactor = new AutoRatingGUIInteractor(tempForm);
				var progressFormIsShown = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (form is ProgressForm)
					{
						progressFormIsShown = true;
					}
				});

				using (interactor.StartRatingSession())
				{
					Assert("ProgressForm should not be shown", !progressFormIsShown);
					AssertEquals("Cursor should be changed to Wait Cursor during autorating.", Cursors.WaitCursor, Cursor.Current);
				}
			}
		}
	}
}
