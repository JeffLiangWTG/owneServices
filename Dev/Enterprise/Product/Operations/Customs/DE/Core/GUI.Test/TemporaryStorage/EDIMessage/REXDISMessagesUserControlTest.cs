using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class REXDISMessagesUserControlTest : TestCaseWithFactory
	{
		public void TestColumnLayoutContext()
		{
			using (REXDISMessagesUserControl control = new REXDISMessagesUserControl())
			{
				var messageGrid = control.FindSingleOrDefault<ZGrid>(x => x.Name == "MessagesGrid");
				AssertEquals("Column layout context should be REX", TemporaryStorageApplicationCodeList.Codes.REX, messageGrid.ColumnLayoutContext);
			}
		}
	}
}
