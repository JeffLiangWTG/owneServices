using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class SumAMessagesUserControlTest : TestCaseWithFactory
	{
		public void TestColumnLayoutContext()
		{
			using (SumAMessagesUserControl control = new SumAMessagesUserControl())
			{
				var messageGrid = control.FindSingleOrDefault<ZGrid>(x => x.Name == "MessagesGrid");
				AssertEquals("Column layout context should be SUM", TemporaryStorageApplicationCodeList.Codes.SumA, messageGrid.ColumnLayoutContext);
			}
		}
	}
}
