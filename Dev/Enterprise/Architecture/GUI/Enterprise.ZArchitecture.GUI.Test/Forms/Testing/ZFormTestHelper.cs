using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZFormTestHelper : TestCaseWithFactory
	{
		public static void AssertCommandButtons(IPostingButtonsProvider postingButtons, bool applyEnabled, bool postEnabled, bool cancelEnabled, string applyText, string postText, string cancelText)
		{
			CombineAssertions(() =>
			{
				AssertEquals("New active status", applyEnabled, postingButtons.CommandButtonApply.Enabled);
				AssertEquals("Save active status", postEnabled, postingButtons.CommandButtonPost.Enabled);
				AssertEquals("Close active status", cancelEnabled, postingButtons.CommandButtonCancel.Enabled);

				AssertEquals(applyText, postingButtons.CommandButtonApply.Text);
				AssertEquals(postText, postingButtons.CommandButtonPost.Text);
				AssertEquals(cancelText, postingButtons.CommandButtonCancel.Text);
			});
		}
	}
}
