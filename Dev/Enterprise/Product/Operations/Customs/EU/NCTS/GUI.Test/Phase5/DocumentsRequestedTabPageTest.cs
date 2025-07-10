using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class DocumentsRequestedTabPageTest : TestCaseWithFactory
	{
		public void TestCaption()
		{
			AssertEquals("Documents Requested", documentsRequestedTabPage.Caption.Caption);
		}

		public void TestUserControlBindingMember()
		{
			AssertEquals(".", documentsRequestedTabPage.UserControlBindingMember);
		}

		public void TestCreateUserControl()
		{
			using (var userControl = documentsRequestedTabPage.CreateUserControl())
			{
				AssertType<RequestedDocumentsUserControl>(userControl);
			}
		}

		public void TestRequestedDocumentsGridReadOnly()
		{
			var header = Factory.New<NctsHeader>();
			header.RequestedDocuments.AddNew();
			using (var form = new ZForm(header))
			using (var userControl = documentsRequestedTabPage.CreateUserControl())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(header, "");
				form.Show();
				AssertEquals(true, userControl.FindSingle<ZGrid>("RequestedDocumentsGrid").ReadOnly);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			documentsRequestedTabPage = new DocumentsRequestedTabPage();
		}
		DocumentsRequestedTabPage documentsRequestedTabPage;
	}
}
