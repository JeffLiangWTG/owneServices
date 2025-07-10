using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class UcrAndBillTypeUserControlTest : TestCaseWithFactory
	{
		public void TestDataSourceType()
		{
			using (var control = new UcrAndBillTypeUserControl())
			{
				AssertEquals("DataSourceType", typeof(JobDeclaration), control.DataSourceType);
			}
		}

		public void TestComponents()
		{
			using (var control = new UcrAndBillTypeUserControl())
			{
				CombineAssertions(() =>
				{
					AssertType<ZTextBox>("JE_UCRTextBox must be ZTextBox", control.JE_UCRTextBox);
					AssertType<ZDropEdit>("BillTypeDropEdit must be ZDropEdit", control.BillTypeDropEdit);
				});
			}
		}
	}
}
