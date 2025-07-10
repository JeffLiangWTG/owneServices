using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.DocumentEngine.GUI.Test
{
	public class CustomizeDocumentUrlHandlerTest : TestCaseWithFactory
	{
		public void TestHandleCore_WithValidSupportable()
		{
			var supportableId = ControllerIDs.GlbStaff;

			var dummyAccount = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			// An instance of the repository is needed, since the static class leaks memory due to the inability to get rid of it's private static lastRepository property
			var mockDisplayTool = new Mock<IDevTool>();
			mockDisplayTool.Setup(m => m.Show(It.IsAny<Form>()));

			var customizedHandler = new CustomizeDocumentsUrlHandler(mockDisplayTool.Object);

			var queryString = new QueryString();
			queryString.Add("TableCode", GlbStaffSchema.Constants.Prefix);
			queryString.Add("BusinessEntityPK", dummyAccount.PK.ToString());

			customizedHandler.Handle(queryString);

			AssertNoExceptionThrown(mockDisplayTool.VerifyAll);
		}

		public void TestHandleCore_WithNonSupportable_NotifiesUser()
		{
			var dummyBizO = Factory.NewWithValidTestData<GlbExternalPassword>();
			Factory.Save();

			var queryString = new QueryString();
			queryString.Add("TableCode", GlbExternalPasswordSchema.Constants.Prefix);
			queryString.Add("BusinessEntityPK", dummyBizO.PK.ToString());

			var customizedHandler = new CustomizeDocumentsUrlHandler();
			customizedHandler.Handle(queryString);

			AssertEquals(
				"A friendly message should notify the user of unsupported entities",
				"Error Tried to open document customization for an entity that does not support this functionality.",
				UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		public void TestHandleCore_WithNoBusinessObject_NotifiesUser()
		{
			var queryString = new QueryString();
			queryString.Add("TableCode", GlbStaffSchema.Constants.Prefix);
			queryString.Add("BusinessEntityPK", "1762653f-8680-4acd-a186-caa09d73c448"); // This is a randomly generated Guid

			var customizedHandler = new CustomizeDocumentsUrlHandler();
			customizedHandler.Handle(queryString);

			AssertEquals(
				"A friendly message should notify the user inexistant entity",
				"Error Tried to open document customization without providing an entity.",
				UnitTestUserNotification.Instance.LastMessage.ToString());
		}
	}
}
