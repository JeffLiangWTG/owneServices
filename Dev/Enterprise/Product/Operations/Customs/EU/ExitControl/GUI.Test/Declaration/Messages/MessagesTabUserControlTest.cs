using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	sealed class MessagesTabUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(EDIMessageCollection), userControl.BindingSource.DataSourceType);
		}

		public void TestIsTestMessageColumn()
		{
			var messagesGrid = (ZGrid)userControl.Controls.Find("MessagesGrid", true).First();
			AssertCollectionContains("EM_IsTestMessage", messagesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new MessagesTabUserControl();
		}
		MessagesTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
