using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public abstract class ZPageConfirmationTest : TestCaseWithFactory
	{
		public abstract void TestHandleResponse();

		public void TestPage1()
		{
			AssertEquals(TestConfirmationPage, TestConfirmation.Page);
		}

		public abstract void TestIsRequired();

		public void TestUserReponseHolderID()
		{
			AssertEquals(GetExpectedUserReponseHolderID(), TestConfirmation.UserReponseHolderID);
		}

		protected abstract string GetExpectedUserReponseHolderID();

		public void TestConfirmationMessage()
		{
			AssertEquals(GetExpectedConfirmationMessage(), TestConfirmation.ConfirmationMessage);
		}

		protected abstract string GetExpectedConfirmationMessage();

		public void TestAlwaysRegisterConfirmationScript()
		{
			AssertEquals(GetAlwaysRegisterConfirmationScript(), TestConfirmation.AlwaysRegisterConfirmationScript);
		}

		protected virtual bool GetAlwaysRegisterConfirmationScript() => false;

		public void TestTitle()
		{
			AssertEquals(GetExpectedTitle(), TestConfirmation.Title);
		}

		protected abstract string GetExpectedTitle();

		public void TestConfirmationImageURL()
		{
			AssertEquals(GetExpectedConfirmationImageURL(), TestConfirmation.ConfirmationImageURL);
		}

		protected virtual string GetExpectedConfirmationImageURL() => string.Empty;

		public virtual void TestButtons()
		{
			var buttons = TestConfirmation.Buttons;
			AssertEquals(2, buttons.Count);
			AssertButton(buttons.FirstOrDefault(b => b.Caption == "Yes"));
			AssertButton(buttons.FirstOrDefault(b => b.Caption == "No"));
		}

		protected void AssertButton(ZPageConfirmation.ZPageConfirmationButton saveButton)
		{
			AssertNotNull(saveButton);
			AssertButtonOnClickScript(saveButton);
		}

		void AssertButtonOnClickScript(ZPageConfirmation.ZPageConfirmationButton button)
		{
			if (TestConfirmation.IsSaveConfirmation)
			{
				AssertEquals(button.OnClickScript, $"$(\\'{TestConfirmationPage.SaveButtonClientID}\\').click();");
			}
			else
			{
				AssertEquals(button.OnClickScript, $"__doPostBack();return true;");
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestConfirmationPage = new TestPageWithConfirmations();
			TestConfirmation = GetNewPageConfirmationForTesting();
		}

		protected abstract ZPageConfirmation GetNewPageConfirmationForTesting();

		protected TestPageWithConfirmations TestConfirmationPage;
		protected ZPageConfirmation TestConfirmation;

		protected class TestPageWithConfirmations : ZPage
		{
			protected override string GetSaveButtonClientID() => "SaveButtonID";

			protected override BusinessObject GetNewDataSource() => dataSource;

			public void SetDataSource(BusinessObject dataSource)
			{
				this.dataSource = dataSource;
				LoadOrCreateDataSource();
			}

			BusinessObject dataSource;
		}
	}
}
