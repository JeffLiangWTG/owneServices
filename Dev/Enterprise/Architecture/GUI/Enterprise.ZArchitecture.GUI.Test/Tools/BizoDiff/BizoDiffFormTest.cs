using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class BizoDiffFormTest : TestCaseWithFactory
	{
		public void TestBizoDiffForm_WhenNoControllerShouldGetErrorMessage()
		{
			var obj = Factory.NewWithValidTestData<BizoDiffFormTestObjectWithCode>();
			Factory.Save();
			AssertNull("Precondition: No messages should be shown yet.", UnitTestUserNotification.Instance.LastMessage.Text);
			using (var form = new BizoDiffForm(obj))
			{
				AssertEquals("A message should be shown.", "Cannot find Module Id.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
		public void TestBizoDiffForm_NoErrorWhenModuleIdCanBeFound()
		{
			var obj = Factory.NewWithValidTestData<GlbCompany>();
			Factory.Save();
			AssertNull("Precondition: No messages should be shown yet.", UnitTestUserNotification.Instance.LastMessage.Text);
			using (var form = new BizoDiffForm(obj))
			{
				AssertNull("Precondition: No messages should be shown yet.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[CodeProperty("Code")]
		public class BizoDiffFormTestObjectWithCode : AutoDummyBizo
		{
			public BizoDiffFormTestObjectWithCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public string Code { get; set; }
		}
	}
}
