using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using static Enterprise.Customs.ES.Business.ESConstants;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using EntrySubStyleList = Enterprise.Customs.ES.Business.Declaration.EntrySubStyleList;

namespace Enterprise.Customs.ES.Business.Testing;

public class UrlDeciderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			CusEntryHeader entryHeader = null;
			AssertExceptionThrown<ArgumentNullException>("Null entryHeader", () => new UrlDecider(entryHeader));
			AssertExceptionThrown<ArgumentNullException>("Null declaration", () => new UrlDecider(Factory.New<CusEntryHeader>()));

			var dec = Factory.New<JobDeclaration>();
			entryHeader = dec.CustomsEntryHeaders.AddNew();
			AssertExceptionThrown<ArgumentNullException>("Null entryInstruction", () => new UrlDecider(entryHeader));
		});
	}

	public void TestGetUrl_Import_PDI()
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/inwinvoc/es.aeat.advu.jdit.web.cons.DetalleVUAInt?operacion=3000&CABECERA_MRN=" + expectedMRN;

		var entryHeader = SetEntryHeader(MessageTypeList.Codes.Import, EntrySubStyleList.Codes.C);
		entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

		AssertGetUrl(entryHeader, expectedMRN, expectedUrl);
	}

	public void TestGetUrl_Import_NotPDI_Mainland()
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/inwinvoc/es.aeat.dit.adu.adip.inter.cnt.CImporInternet?CABECERA_MRN=" + expectedMRN;

		var entryHeader = SetEntryHeader(MessageTypeList.Codes.Import, EntrySubStyleList.Codes.B);

		AssertGetUrl(entryHeader, expectedMRN, expectedUrl);
	}

	public void TestGetUrl_Import_NotPDI_CanaryIsland()
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/inwinvoc/es.aeat.dit.adu.adip.inter.cnt.CImpInternetVexcan?CABECERA_MRN=" + expectedMRN;

		var testDataHelper = new ESUniversalReferenceTestDataHelper(Factory);
		var canaryIslandCode = "61";
		testDataHelper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, canaryIslandCode, "Test 61");

		var entryHeader = SetEntryHeader(MessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, canaryIslandCode);

		AssertGetUrl(entryHeader, expectedMRN, expectedUrl);
	}

	public void TestGetUrl_Import_T2L_NoPOUS()
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/L/inwinvoc/es.aeat.dit.adu.adtl.gestion.cnt.DetExpedicionT2L?retorno=es.aeat.dit.adu.adtl.gestion.qry.QDocsT2LInt&operacion=1001&clave=" + expectedMRN;

		var entryHeader = SetEntryHeader(MessageTypeList.Codes.Import, EntrySubStyleList.Codes.T2L);
		entryHeader.ZG_POUSVersion = POUSVersionCodes.NoPOUS;

		AssertGetUrl(entryHeader, expectedMRN, expectedUrl);
	}

	public void TestGetUrl_Import_T2L_POUS1()
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/L/inwinvoc/es.aeat.dit.adu.adtl.gestion.cnt.DetExpedicionT2L?retorno=es.aeat.dit.adu.adtl.gestion.qry.QDocsT2LInt&operacion=1001&clave=" + expectedMRN;

		var entryHeader = SetEntryHeader(MessageTypeList.Codes.Import, EntrySubStyleList.Codes.T2L);
		entryHeader.ZG_POUSVersion = POUSVersionCodes.POUS;

		AssertGetUrl(entryHeader, expectedMRN, expectedUrl);
	}

	public void TestGetUrl_Import_T2L_POUS2()
	{
		var expectedT2CMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADTL-JDIT/T2LDetalle?mrn=" + expectedT2CMRN;

		var entryHeader = SetEntryHeader(MessageTypeList.Codes.Import, EntrySubStyleList.Codes.T2L);
		entryHeader.ZG_POUSVersion = POUSVersionCodes.POUS2;

		CombineAssertions(() =>
		{
			AssertNullOrEmpty("No url was returned when no mrn", new UrlDecider(entryHeader).GetUrl());

			entryHeader.MovementReferenceNumber = "mrn";
			AssertNullOrEmpty("No url was returned when mrn but no t2c mrn", new UrlDecider(entryHeader).GetUrl());

			var newEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Spain.T2CMovementReferenceNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = expectedT2CMRN;
			newEntryNumber.CE_IssueDate = ZDateTime.Today;
			newEntryNumber.CE_EntryIsSystemGenerated = true;
			AssertEquals("The correct url has been launched", expectedUrl, new UrlDecider(entryHeader).GetUrl());
		});
	}

	public void TestGetUrl_Import_T2C()
	{
		var expectedT2CMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADTL-JDIT/JECDetalle?mrn=" + expectedT2CMRN;

		var entryHeader = SetEntryHeader(MessageTypeList.Codes.Import, EntrySubStyleList.Codes.T2C);

		CombineAssertions(() =>
		{
			AssertNullOrEmpty("No url was returned when no mrn", new UrlDecider(entryHeader).GetUrl());

			entryHeader.MovementReferenceNumber = "mrn";
			AssertNullOrEmpty("No url was returned when mrn but no t2c mrn", new UrlDecider(entryHeader).GetUrl());

			var newEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Spain.T2CMovementReferenceNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = expectedT2CMRN;
			newEntryNumber.CE_IssueDate = ZDateTime.Today;
			newEntryNumber.CE_EntryIsSystemGenerated = true;
			AssertEquals("The correct url has been launched", expectedUrl, new UrlDecider(entryHeader).GetUrl());
		});
	}

	public void TestGetUrl_ImportH1_DJPStatus()
	{
		var expectedMRNDJP = "JPBTEST00123456789";
		var declarationMRN = "22ES00999912345678";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADIP-JDIT/DetalleUltH1?mrn=" + expectedMRNDJP;

		var entryHeader = SetEntryHeader(MessageTypeList.Codes.Import, EntrySubStyleList.Codes.X);

		entryHeader.ZG_UCC6Version = UCC6VersionCodes.UCC6;
		AssertGetUrl(entryHeader, declarationMRN, expectedUrl, expectedMRNDJP);
	}

	public void TestGetUrl_ImportH1_StatusCanaryIsland()
	{
		var expectedMRN = "24ES00999930000JPB";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADIP-JDIT/DetalleXSH1?anyo=24&pais=ES&recinto=009999&numero=30000JPB";

		var testDataHelper = new ESUniversalReferenceTestDataHelper(Factory);
		var canaryIslandCode = "61";
		testDataHelper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, canaryIslandCode, "Test 61");

		var entryHeader = SetEntryHeader(MessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, canaryIslandCode);

		entryHeader.ZG_UCC6Version = UCC6VersionCodes.UCC6;
		AssertGetUrl(entryHeader, expectedMRN, expectedUrl);
	}

	public void TestGetUrl_ImportH1_Status()
	{
		var expectedMRN = "24ES00999930000JPB";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADIP-JDIT/DetalleSH1?anyo=24&pais=ES&recinto=009999&numero=30000JPB";

		var entryHeader = SetEntryHeader(MessageTypeList.Codes.Import, EntrySubStyleList.Codes.B);

		entryHeader.ZG_UCC6Version = UCC6VersionCodes.UCC6;
		AssertGetUrl(entryHeader, expectedMRN, expectedUrl);
	}

	public void TestGetUrl_Export()
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADEX-JDIT/AesDetalle?CLAVE=" + expectedMRN;

		var entryHeader = SetEntryHeader(MessageTypeList.Codes.Export, EntrySubStyleList.Codes.C);

		AssertGetUrl(entryHeader, expectedMRN, expectedUrl);
	}

	public void TestGetUrl_Export_EXS()
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/inwinvoc/es.aeat.dit.adu.adrx.inter.CtrInternet?operacion=1032&clave=" + expectedMRN;

		var entryHeader = SetEntryHeader(MessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS);

		AssertGetUrl(entryHeader, expectedMRN, expectedUrl);
	}

	public void TestGetUrl_Export_T2L()
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADTL-JDIT/T2LDetalle?mrn=" + expectedMRN;

		var entryHeader = SetEntryHeader(MessageTypeList.Codes.Export, EntrySubStyleList.Codes.T2L);

		AssertGetUrl(entryHeader, expectedMRN, expectedUrl);
	}

	public void TestGetUrl_Import_DJP()
	{
		var expectedMRNDJP = "AACCRRRRRRNNNNNNNN";
		var declarationMRN = "22ES00999912345678";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/inwinvoc/es.aeat.dit.adu.adip.pendencia.cnt.CDespUltInt?operacion=3010&CABECERA_MRN=" + expectedMRNDJP;

		var entryHeader = SetEntryHeader(MessageTypeList.Codes.Import, EntrySubStyleList.Codes.X);

		AssertGetUrl(entryHeader, declarationMRN, expectedUrl, expectedMRNDJP);
	}

	public void TestGetUrl_Import_H2()
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADDV-H2UC/CntAEATInternet?operacion=2250&CABECERA_MRN=" + expectedMRN;

		var entryHeader = SetEntryHeader(MessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2, customOffice: "ES009999");

		AssertGetUrl(entryHeader, expectedMRN, expectedUrl);
	}

	public void TestGetUrl_Import_H2_CanaryIsland()
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADDV-H2UC/CntATCInternet?operacion=2250&CABECERA_MRN=" + expectedMRN;

		var entryHeader = SetEntryHeader(MessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2, customOffice: "ES009998");
		AssertGetUrl(entryHeader, expectedMRN, expectedUrl);

		entryHeader.Declaration.JE_CustomsOffice = "ES003500";
		entryHeader.MovementReferenceNumber = ZString.Empty;
		AssertGetUrl(entryHeader, expectedMRN, expectedUrl);

		entryHeader.Declaration.JE_CustomsOffice = "ES003800";
		entryHeader.MovementReferenceNumber = ZString.Empty;
		AssertGetUrl(entryHeader, expectedMRN, expectedUrl);
	}

	void AssertGetUrl(CusEntryHeader entryHeader, string expectedMRN, string expectedUrl, string expectedMRNDJP = "")
	{
		CombineAssertions(() =>
		{
			AssertNullOrEmpty("No url was returned when no mrn", new UrlDecider(entryHeader).GetUrl());

			entryHeader.MovementReferenceNumber = expectedMRN;
			entryHeader.ZG_DJPMRN = expectedMRNDJP;
			AssertEquals("The correct url has been launched", expectedUrl, new UrlDecider(entryHeader).GetUrl());
		});
	}

	CusEntryHeader SetEntryHeader(ZString messageType, ZString entryInstructionSubStyle, string destinationState = "28", string entryInstructionStyle = "", string customOffice = "")
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = messageType;
		dec.ZG_DestinationState = destinationState;
		dec.JE_CustomsOffice = customOffice;
		var entryInstruction = dec.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = entryInstructionSubStyle;
		entryInstruction.CEI_Style = entryInstructionStyle;
		var entryHeader = dec.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		return entryHeader;
	}
}
