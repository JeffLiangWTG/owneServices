using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(SupplementaryCode))]
	public class SupplementaryCodeTest : Customs.Business.Testing.CusCodeDataTest<SupplementaryCode>
	{
		protected override IEnumerable<SupplementaryCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
			var loader = new Customs.Business.BaseSupplementaryCode.Loader(factory);
			var supplementaryCode = loader.LoadOrCreate<SupplementaryCode, NctsCommonCargoDesc>(goodsItem, 1);
			supplementaryCode.CY_Code = SupplementaryCode.CodeDataType;
			yield return supplementaryCode;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var code = factory.NewWithValidTestData<SupplementaryCode>();
			code.CY_Code = SupplementaryCode.CodeDataType;
			return code;
		}
	}
}
