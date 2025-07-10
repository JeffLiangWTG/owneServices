using System.Web.UI;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.RemotePrinting.Server.Business;
using Enterprise.RemotePrinting.Server.Controls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.RemotePrinting.Server.Testing
{
	public class RequestLogsPageTest : ZIFramePageTest
	{
		public override void TestOKButtonClick()
		{
			var page = new RequestLogsPageForTest
			{
				ShouldNewValidDataSource = true
			};
			page.LoadOrCreateDataSource_Exposed();

			var registered = page.ZClientScript.IsClientScriptBlockRegistered(page.GetType(), "RequestLogsPage_ReloadParentPage");
			AssertEquals("RequestLogsPage_ReloadParentPage should not registered", false, registered);
			AssertNoExceptionThrown(() => page.HandleOkButtonClick_Exposed());

			registered = page.ZClientScript.IsClientScriptBlockRegistered(page.GetType(), "RequestLogsPage_ReloadParentPage");
			AssertEquals("RequestLogsPage_ReloadParentPage should registered", true, registered);
		}

		public override void TestGetNewDataSource()
		{
			var newDataSource = ((RequestLogsPageForTest)TestIFramePage).GetNewDataSource_Exposed();
			AssertType<ClientLogRetrievalInfo>("Should be ClientLogRetrievalInfo", newDataSource);

			var clientLogRetrievalInfo = newDataSource as ClientLogRetrievalInfo;
			var expectedFromDate = ZDateTime.Today.AddDays(-7);
			var expectedToDate = ZDateTime.Today;

			AssertEquals(expectedFromDate, clientLogRetrievalInfo.FromDate);
			AssertEquals(expectedToDate, clientLogRetrievalInfo.ToDate);
			AssertEquals(true, clientLogRetrievalInfo.Logs);
			AssertEquals(true, clientLogRetrievalInfo.ServiceLogs);
		}

		public override void TestOKFunctionArguments()
		{
			AssertNull("No OKFunctionArguments", ((RequestLogsPageForTest)TestIFramePage).OKFunctionArguments_Exposed);
		}

		public override void TestCancelFunctionArguments()
		{
			AssertNull("No CancelFunctionArguments", ((RequestLogsPageForTest)TestIFramePage).CancelFunctionArguments_Exposed);
		}

		protected override Control GetNewControl()
		{
			return new RequestLogsPageForTest();
		}

		#region UselessTests

		public override void TestButtonsContainer()
		{
			Assert(true);
		}

		public override void TestOKButton()
		{
			Assert(true);
		}

		public override void TestSaveDataSourceFactoryNoBubble()
		{
			Assert(true);
		}

		#endregion

		class RequestLogsPageForTest : RequestLogsPage
		{
			public BusinessObject GetNewDataSource_Exposed() => base.GetNewDataSource();

			public string[] OKFunctionArguments_Exposed => base.OKFunctionArguments;

			public string[] CancelFunctionArguments_Exposed => base.CancelFunctionArguments;

			public void HandleOkButtonClick_Exposed() => base.HandleOkButtonClick();

			public void LoadOrCreateDataSource_Exposed() => base.LoadOrCreateDataSource();

			protected override BusinessObject GetNewDataSource()
			{
				var data = base.GetNewDataSource();
				if (ShouldNewValidDataSource)
				{
					((ClientLogRetrievalInfo)data).EmailAddress = "jerry@test.com";
				}
				return data;
			}

			public bool ShouldNewValidDataSource { get; set; }
		}
	}
}
