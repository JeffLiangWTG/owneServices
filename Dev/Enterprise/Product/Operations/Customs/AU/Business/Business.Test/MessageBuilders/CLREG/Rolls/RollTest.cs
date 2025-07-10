using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(Roll))]
	sealed class RollTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<Roll>
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<OrgHeader>();
			header.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			header.MainAddress.OA_Address1 = "Test Address 1";
			header.MainAddress.OA_Address2 = "Test address 2";
			header.MainAddress.OA_City = "London";
			header.MainAddress.OA_PostCode = "2000";
			header.MainAddress.OA_State = "NSW";
			var wrapper = new OrgHeaderWrapper(header);
			var parent = wrapper.CLREGInfoProvider;
			var result = factory.New<Roll>();
			result.B7_ParentID = parent.PK;
			result.B7_ParentTableCode = parent.TablePrefix;
			return result;
		}
	}
}
