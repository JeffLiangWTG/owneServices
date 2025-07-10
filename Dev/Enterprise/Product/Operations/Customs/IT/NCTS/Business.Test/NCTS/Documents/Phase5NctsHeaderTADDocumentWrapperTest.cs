using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(Phase5NctsHeaderTADDocumentWrapper))]
sealed class Phase5NctsHeaderTADDocumentWrapperTest : NonPersistentBusinessObjectTestCase
{
	public void TestLines()
	{
		AssertType<Phase5NctsTADItemWrapperCollection>(wrapper.Lines);
	}

	protected override BusinessObject GetNewBusinessObject() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		wrapper = Phase5NctsHeaderTADDocumentWrapper.New(nctsHeader, Factory);
	}

	NctsHeader nctsHeader;
	Phase5NctsHeaderTADDocumentWrapper wrapper;
}
