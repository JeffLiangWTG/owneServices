using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(CusTempStorageRegLineTypeDecider))]
sealed class CusTempStorageRegLineTypeDeciderTest : TestCaseWithFactory
{
	public void TestGetTypeForLoad_Default()
	{
		var header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_AppCode = "A1";
		header.SRH_Reference = "REF";
		var line = header.CusTempStorageRegLines.AddNew();
		line.SRL_LineNumber = 1;
		Factory.Save();

		AssertEquals("Type", typeof(CusTempStorageRegLine), new BusinessObjectFactory().Load<CusTempStorageRegLine>(line.PK).GetType());
	}

	public void TestGetTypeForLoad_SUM()
	{
		var header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_AppCode = "SUM";
		header.SRH_Reference = "REF";
		var line = header.CusTempStorageRegLines.AddNew();
		line.SRL_LineNumber = 1;
		Factory.Save();

		AssertEquals("Type", ObjectFactory.GetType<Integration.Customs.DE.ICusTempStorageRegLine>(), new BusinessObjectFactory().Load<CusTempStorageRegLine>(line.PK).GetType());
	}

	public void TestGetTypeForLoad_ADT()
	{
		var header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_AppCode = "ADT";
		header.SRH_Reference = "REF";
		var line = header.CusTempStorageRegLines.AddNew();
		line.SRL_LineNumber = 1;
		Factory.Save();

		AssertEquals("Type", ObjectFactory.GetType<Integration.Customs.ES.ICusTempStorageRegLine>(), new BusinessObjectFactory().Load<CusTempStorageRegLine>(line.PK).GetType());
	}

	public void TestGetTypeForLoad_IST()
	{
		var header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_AppCode = "IST";
		header.SRH_Reference = "REF";
		var line = header.CusTempStorageRegLines.AddNew();
		line.SRL_LineNumber = 1;
		Factory.Save();

		AssertEquals("Type", ObjectFactory.GetType<Integration.Customs.FR.ICusTempStorageRegLine>(), new BusinessObjectFactory().Load<CusTempStorageRegLine>(line.PK).GetType());
	}

	public void TestGetTypeForLoad_TSR()
	{
		var header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_AppCode = "TSR";
		header.SRH_Reference = "REF";
		var line = header.CusTempStorageRegLines.AddNew();
		line.SRL_LineNumber = 1;
		Factory.Save();

		AssertEquals("Type", ObjectFactory.GetType<Integration.Customs.IT.ICusTempStorageRegLine>(), new BusinessObjectFactory().Load<CusTempStorageRegLine>(line.PK).GetType());
	}

	public void TestGetTypeForBinding()
	{
		AssertEquals("Type", typeof(CusTempStorageRegLine), Decider.GetTypeForBinding());
	}

	public void TestGetTypeForNew()
	{
		AssertEquals("Type", typeof(CusTempStorageRegLine), Decider.GetTypeForNew());
	}

	public void TestGetHeaderType()
	{
		AssertEquals("Type", typeof(CusTempStorageRegHeader), Decider.GetHeaderType());
	}

	CusTempStorageRegLineTypeDecider Decider => decider ??= new CusTempStorageRegLineTypeDecider();
	CusTempStorageRegLineTypeDecider decider;
}
