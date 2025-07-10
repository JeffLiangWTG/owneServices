using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZStmNoteForRegistryItemUserControlTest : TransactionedTestCase
	{
		public void TestSetDataBindingToStmData()
		{
			var factory = new BusinessObjectFactory();
			var stmData = factory.NewWithValidTestData<StmData>();
			factory.Save();

				using (var userControl = new ZStmNoteForRegistryItemUserControl())
				{
					userControl.SetDataBinding(stmData, "");
					Assert(userControl.CurrentDataItem is StmData);
					AssertEquals(stmData.PK, ((StmData)userControl.CurrentDataItem).PK);
				}
		}
	}
}
