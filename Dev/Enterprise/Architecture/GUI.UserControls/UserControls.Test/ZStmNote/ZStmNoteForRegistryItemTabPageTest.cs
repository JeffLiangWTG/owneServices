using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZStmNoteForRegistryItemTabPageTest : ZTabPageControlTest
	{
		public void TestSetDataBindingCoretoStmData()
		{
			var factory = new BusinessObjectFactory();
			var stmData = factory.NewWithValidTestData<StmData>();
			factory.Save();
			using (var tabPage = new ZStmNoteForRegistryItemTabPage())
			{
				tabPage.SetDataBindingCoreForTest(stmData, "");
				AssertEquals(stmData.PK, tabPage.BusinessEntityForTest.PK);
			}
		}
	}
}
