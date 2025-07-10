using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	class CusClassificationFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestVisibleColumns()
		{
			var collection = new Customs.Business.BaseClassificationCollection<CusClassification>(Factory);
			CusClassificationFilterBusinessObject bizObj = new CusClassificationFilterBusinessObject();
			using (CusClassificationFilterControl control = new CusClassificationFilterControl(collection, bizObj))
			{
				control.SetDataBinding(collection, "");
				control.Show();
				AssertNull(control.FilteredGrid.Columns[CusClassification.Schema.CC_TariffNum]);
			}
		}
	}
}
