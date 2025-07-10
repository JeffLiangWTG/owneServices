using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageJobHeaderDocManagerInfo))]
	public class CusTempStorageJobHeaderDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject() => Factory.New<CusTempStorageJobHeader>();

		public override BusinessObject GetPopulatedParentBusinessObject() => Factory.NewWithValidTestData<CusTempStorageJobHeader>();
	}
}
