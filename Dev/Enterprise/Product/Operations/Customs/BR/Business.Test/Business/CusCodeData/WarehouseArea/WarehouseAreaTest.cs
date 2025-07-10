using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(WarehouseArea))]
	public class WarehouseAreaTest : Customs.Business.Testing.CusCodeDataTest<WarehouseArea>
	{
		public void TestSetDefaultValues()
		{
			var enclosure = Factory.New<WarehouseArea>();
			AssertEquals("CY_Type", Common.BR.CusCodeDataTypeList.Codes.WarehouseArea, enclosure.CY_Type);
		}

		public void TestValidation()
		{
			var attribute = Factory.New<WarehouseArea>();
			AssertType<WarehouseAreaValidation>(attribute.Validation);
		}

		WarehouseArea CreateNewWarehouseArea(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			return declaration.WarehouseAreas.AddNew("000001");
		}

		protected override IEnumerable<WarehouseArea> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return CreateNewWarehouseArea(factory);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => CreateNewWarehouseArea(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateNewWarehouseArea(factory);

		protected override BusinessObject GetNewBusinessObject() => CreateNewWarehouseArea(Factory);
	}
}
