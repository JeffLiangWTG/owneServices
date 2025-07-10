using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AUOrgImpAddInfo))]
	sealed class AUOrgImpAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new AUOrgImpAddInfo(Factory);
		}

		public void TestIsDutyDeferred()
		{
			var impAddInfo = new AUOrgImpAddInfo(Factory);
			impAddInfo.ZO_IsDutyDeferred = true;
			Assert(impAddInfo.IsDutyDeferred);
			impAddInfo.IsDutyDeferred = false;
			Assert(!impAddInfo.ZO_IsDutyDeferred);
		}
	}
}
