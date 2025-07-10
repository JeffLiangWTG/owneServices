using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(DefaultTSCustomsNumberViewStmNumsBusinessProvider))]
sealed class DefaultTSCustomsNumberViewStmNumsBusinessProviderTest : NonPersistentBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObject()
	{
		var premises = Factory.New<CusTempStorageRegPremises>();
		return new DefaultTSCustomsNumberViewStmNumsBusinessProvider(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, premises.PK);
	}
}
