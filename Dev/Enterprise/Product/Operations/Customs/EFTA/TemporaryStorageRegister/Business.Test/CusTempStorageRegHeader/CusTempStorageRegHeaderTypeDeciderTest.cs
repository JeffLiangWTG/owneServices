using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(CusTempStorageRegHeaderTypeDecider))]
sealed class CusTempStorageRegHeaderTypeDeciderTest : TestCaseWithFactory
{
	public void TestGetTypeForLoad_Default()
	{
		var header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_AppCode = "A1";
		header.SRH_Reference = "REF";
		Factory.Save();

		AssertEquals("Type", typeof(CusTempStorageRegHeader), new BusinessObjectFactory().Load<CusTempStorageRegHeader>(header.PK).GetType());
	}

	public void TestGetTypeForLoad_SUM()
	{
		var header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_AppCode = "SUM";
		header.SRH_Reference = "REF";
		Factory.Save();

		AssertEquals("Type", ObjectFactory.GetType<Integration.Customs.DE.ICusTempStorageRegHeader>(), new BusinessObjectFactory().Load<CusTempStorageRegHeader>(header.PK).GetType());
	}

	public void TestGetTypeForLoad_IST()
	{
		var header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_AppCode = "IST";
		header.SRH_Reference = "REF";
		Factory.Save();

		AssertEquals("Type", ObjectFactory.GetType<Integration.Customs.FR.ICusTempStorageRegHeader>(), new BusinessObjectFactory().Load<CusTempStorageRegHeader>(header.PK).GetType());
	}

	public void TestGetTypeForLoad_ADT()
	{
		var header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_AppCode = "ADT";
		header.SRH_Reference = "REF";
		Factory.Save();

		AssertEquals("Type", ObjectFactory.GetType<Integration.Customs.ES.ICusTempStorageRegHeader>(), new BusinessObjectFactory().Load<CusTempStorageRegHeader>(header.PK).GetType());
	}

	public void TestGetTypeForBinding()
	{
		AssertEquals("Type", typeof(CusTempStorageRegHeader), Decider.GetTypeForBinding());
	}

	public void TestGetTypeForNew()
	{
		AssertEquals("Type", typeof(CusTempStorageRegHeader), Decider.GetTypeForNew());
	}

	CusTempStorageRegHeaderTypeDecider Decider => decider ??= new CusTempStorageRegHeaderTypeDecider();
	CusTempStorageRegHeaderTypeDecider decider;
}
