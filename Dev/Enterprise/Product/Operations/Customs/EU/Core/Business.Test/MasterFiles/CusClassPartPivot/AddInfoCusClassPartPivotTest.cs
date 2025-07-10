using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.MasterFiles.Testing
{
	[TestedType(typeof(AddInfoCusClassPartPivot))]
	public class AddInfoCusClassPartPivotTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var part = Factory.New<CusClassPartPivot>();
			return new AddInfoCusClassPartPivot(part.CI_AddInfoInfo);
		}
	}
}
