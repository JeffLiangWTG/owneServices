using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class JobDeclarationPacksAndPiecesTest : TestCaseWithFactory
	{
		public void TestDeclarationMustBeSentPrimeEnclosure()
		{
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			testDec.JE_ForcePrimeEnclosure = false;
			Assert("!MustBeSentPrimeEnclosure", !testDec.DeclarationMustBeSentPrimeEnclosure);
			testDec.AddInfo.ZA_MergeBy_Hidden = "NON";
			testHeader2.AddInfo.ZA_ValuationBasis_Hidden = "RT";
			Assert("MustBeSentPrimeEnclosure", testDec.DeclarationMustBeSentPrimeEnclosure);
			testHeader1.JZ_Nature10PackCount = 7;
			testHeader2.JZ_Nature10PackCount = 7;
			Assert("!MustBeSentPrimeEnclosure", !testDec.DeclarationMustBeSentPrimeEnclosure);
			testHeader1.JZ_Nature10PackCount = 0;
			testHeader2.JZ_Nature10PackCount = 0;
			testHeader1.JZ_BondPackCount = 7;
			testHeader2.JZ_BondPackCount = 7;
			Assert("!MustBeSentPrimeEnclosure", !testDec.DeclarationMustBeSentPrimeEnclosure);
			testHeader1.JZ_Nature10PackCount = 0;
			testHeader2.JZ_Nature10PackCount = 0;
			testHeader1.JZ_BondPackCount = 0;
			Assert("MustBeSentPrimeEnclosure", testDec.DeclarationMustBeSentPrimeEnclosure);
			testHeader2.JZ_BondPackCount = 0;
			Assert("MustBeSentPrimeEnclosure", testDec.DeclarationMustBeSentPrimeEnclosure);
		}

		public void TestSetDefaultValueToPackage()
		{
			testDec.JE_ApplicationCode = JobMessageTypeList.Codes.Import;
			testDec.JE_OH_Importer = ZGuid.Empty;
			testDec.JE_OH_Supplier = ZGuid.Empty;
			testDec.Bills.RemoveAndDeleteAll();
			var bill = testDec.Bills.AddNew();
			bill.CU_BillType = "HB";
			bill.CU_BillNum = "1";
			testDec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_TotalNoOfPieces = 10;
			var decContainer = testDec.CusContainers.AddNew();
			decContainer.CO_ContainerNumber = "MSKU0045983";
			decContainer.CO_ContainerSize = "20";
			decContainer.CO_FCL_LCL_AIR = "LCL";

			Assert(bill.PackingGroups.Any());
			var packingGroup = bill.PackingGroups[0];
			Assert(packingGroup.Packages.Any());
			Assert(packingGroup.Packages[0].CW_OuterPacks == 10);
			AssertNotNull(packingGroup.CR_CO_Container);
		}

		public void TestDeclarationMustBeSentPrimeEnclosureForPieces()
		{
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			testDec.JE_ForcePrimeEnclosure = false;
			testDec.JE_TotalNoOfPacks = 0;
			testDec.JE_TotalNoOfPieces = 14;
			Assert("!MustBeSentPrimeEnclosure", !testDec.DeclarationMustBeSentPrimeEnclosure);
			testDec.AddInfo.ZA_MergeBy_Hidden = "NON";
			testHeader2.AddInfo.ZA_ValuationBasis_Hidden = "RT";
			Assert("MustBeSentPrimeEnclosure", testDec.DeclarationMustBeSentPrimeEnclosure);
			testHeader1.JZ_PiecesForRelease = 7;
			testHeader2.JZ_PiecesForRelease = 7;
			Assert("!MustBeSentPrimeEnclosure", !testDec.DeclarationMustBeSentPrimeEnclosure);
		}

		public void TestHasNature10Entry()
		{
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			testHeader1.JZ_Nature10PackCount = 7;
			testHeader1.JZ_BondPackCount = 0;
			testHeader2.JZ_Nature10PackCount = 7;
			testHeader2.JZ_BondPackCount = 0;
			Assert("HasNature10Entry", testDec.HasNature10Entry);
			testHeader1.JZ_Nature10PackCount = 0;
			testHeader1.JZ_BondPackCount = 7;
			testHeader2.JZ_Nature10PackCount = 0;
			testHeader2.JZ_BondPackCount = 7;
			Assert("!HasNature10Entry", !testDec.HasNature10Entry);
			testHeader1.JZ_Nature10PackCount = 0;
			testHeader1.JZ_BondPackCount = 7;
			testHeader2.JZ_Nature10PackCount = 7;
			testHeader2.JZ_BondPackCount = 0;
			Assert("HasNature10Entry", testDec.HasNature10Entry);
			testHeader1.JZ_Nature10PackCount = 0;
			testHeader1.JZ_BondPackCount = 0;
			testHeader2.JZ_Nature10PackCount = 0;
			testHeader2.JZ_BondPackCount = 0;
			Assert("HasNature10Entry", testDec.HasNature10Entry);
			testLine1.AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";
			Assert("HasNature10Entry", testDec.HasNature10Entry);
			testLine2.AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";
			Assert("!HasNature10Entry", !testDec.HasNature10Entry);
		}

		public void TestHasNature20Entry()
		{
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			testHeader1.JZ_Nature10PackCount = 0;
			testHeader1.JZ_BondPackCount = 7;
			testHeader2.JZ_Nature10PackCount = 0;
			testHeader2.JZ_BondPackCount = 7;
			Assert("HasNature20Entry", testDec.HasNature20Entry);
			testHeader1.JZ_Nature10PackCount = 7;
			testHeader1.JZ_BondPackCount = 0;
			testHeader2.JZ_Nature10PackCount = 7;
			testHeader2.JZ_BondPackCount = 0;
			Assert("!HasNature20Entry", !testDec.HasNature20Entry);
			testHeader1.JZ_Nature10PackCount = 7;
			testHeader1.JZ_BondPackCount = 0;
			testHeader2.JZ_Nature10PackCount = 0;
			testHeader2.JZ_BondPackCount = 7;
			Assert("HasNature20Entry", testDec.HasNature20Entry);

			testHeader1.JZ_Nature10PackCount = 0;
			testHeader1.JZ_BondPackCount = 0;
			testHeader2.JZ_Nature10PackCount = 0;
			testHeader2.JZ_BondPackCount = 0;

			Assert("!HasNature20Entry", !testDec.HasNature20Entry);
			testLine1.AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";
			Assert("HasNature20Entry", testDec.HasNature20Entry);
			testLine2.AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";
			Assert("HasNature20Entry", testDec.HasNature20Entry);
		}

		public void TestHasNature10And20Entries()
		{
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			testHeader1.JZ_Nature10PackCount = 7;
			testHeader1.JZ_BondPackCount = 0;
			testHeader2.JZ_Nature10PackCount = 7;
			testHeader2.JZ_BondPackCount = 0;
			Assert("!HasNature10And20Entries", !testDec.HasNature10And20Entries);
			testHeader1.JZ_Nature10PackCount = 0;
			testHeader1.JZ_BondPackCount = 7;
			testHeader2.JZ_Nature10PackCount = 0;
			testHeader2.JZ_BondPackCount = 7;
			Assert("!HasNature10And20Entries", !testDec.HasNature10And20Entries);
			testHeader1.JZ_Nature10PackCount = 0;
			testHeader1.JZ_BondPackCount = 7;
			testHeader2.JZ_Nature10PackCount = 7;
			testHeader2.JZ_BondPackCount = 0;
			Assert("HasNature10And20Entries", testDec.HasNature10And20Entries);
			testHeader1.JZ_Nature10PackCount = 0;
			testHeader1.JZ_BondPackCount = 0;
			testHeader2.JZ_Nature10PackCount = 0;
			testHeader2.JZ_BondPackCount = 0;
			Assert("!HasNature10And20Entries", !testDec.HasNature10And20Entries);
			testLine1.AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";
			Assert("HasNature10And20Entries", testDec.HasNature10And20Entries);
			testLine2.AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";
			Assert("!HasNature10And20Entries", !testDec.HasNature10And20Entries);
		}

		[ExpectNoExceptions()]
		public void TestJE_TransportMode_MAI()
		{
			testDec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Mail;
		}

		public void TestSavingAShipmentAndADeclarationTogether()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ForwardingShipment shipment = factory.New<ForwardingShipment>();
			JobDeclaration declaration = factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			factory.Save();
			Assert("UniqueConsignmentRefNotEmpty", shipment.JS_UniqueConsignRef != "");
			AssertEquals("DecReference", shipment.JS_UniqueConsignRef, declaration.JE_DeclarationReference);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();

			OrgHeader supplier = OrgHeader.LoadFromCode(Factory, "ABIGAS");
			OrgHeader importer = OrgHeader.LoadFromCode(Factory, "ABABEU");

			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.GSTCode, "90093519530");
			supplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.SupplierCode, "955411R");
			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, "0644736G");
			testDec = Factory.New<JobDeclaration>();

			testDec.JE_OH_Importer = importer.PK;
			testDec.JE_OH_Supplier = supplier.PK;
			testDec.JE_AddInfo = "MergeBy_Hidden=TRF*NumberOfEntryPrints_Hidden=1";
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testDec.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.AIR;
			testDec.JE_DateOfArrival = new ZDateTime(2004, 1, 10);
			testDec.JE_DateOfFirstArrival = new ZDateTime(2004, 1, 10);
			testDec.JE_DeclarationReference = "B00110381";
			testDec.JE_EntryStatus = CustomsEntryStatus.ClearCreate.Code;
			testDec.JE_ExportDate = new ZDateTime(2004, 1, 8);
			testDec.JE_ExportGoodsType = "OT";
			testDec.JE_GB = GlbBranch.CurrentBranch.PK;
			testDec.JE_HouseBill = "54151";
			testDec.JE_MessageSubType = "FRM";
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_RL_NKFinalDestination = "AUBNE";
			testDec.JE_RL_NKOrigin = "DEFRA";
			testDec.JE_RL_NKPortOfArrival = "AUBNE";
			testDec.JE_RL_NKPortOfFirstArrival = "AUBNE";
			testDec.JE_RL_NKPortOfLoading = "DEFRA";
			testDec.JE_TotalNoOfPacks = 14;
			testDec.JE_TotalNoOfPacksPackType = "CTN";
			testDec.JE_TransportMode = Core.Constants.TransportModes.Air;
			testDec.JE_VoyageFlightNo = "QF66";
			testDec.JE_OwnerRef = ".";

			testDec.JobComInvoiceGroupHeaders[0].JZ_GroupInvoice = true;
			testDec.JobComInvoiceGroupHeaders[0].JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testDec.JobComInvoiceGroupHeaders[0].JZ_InvoiceCurrExRate = 1.000000000m;
			testDec.JobComInvoiceGroupHeaders[0].JZ_InvoiceDate = new ZDateTime(2004, 1, 8);

			testHeader1 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testHeader1.JZ_AddInfo = "PackCountForNature10_Hidden=14*ValuationBasis_Hidden=UT*ORG=DE";
			testHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testHeader1.JZ_InvoiceAmount = 40000.0000m;
			testHeader1.JZ_InvoiceCurrExRate = 1.000000000m;
			testHeader1.JZ_InvoiceDate = new ZDateTime(2004, 1, 8);
			testHeader1.JZ_InvoiceNumber = "1";
			testHeader1.JZ_OH_Supplier = supplier.PK;
			testHeader1.JZ_RX_NKInvoice_Currency = "AUD";
			testHeader1.JZ_Weight = 40.000m;
			testHeader1.JZ_WeightUQ = "KG";

			testHeader2 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testHeader2.JZ_AddInfo = "ValuationBasis_Hidden=UT*ORG=DE";
			testHeader2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testHeader2.JZ_InvoiceAmount = 400.0000m;
			testHeader2.JZ_InvoiceCurrExRate = 1.000000000m;
			testHeader2.JZ_InvoiceDate = new ZDateTime(2004, 1, 8);
			testHeader2.JZ_InvoiceNumber = "2";
			testHeader2.JZ_OH_Supplier = supplier.PK;
			testHeader2.JZ_RX_NKInvoice_Currency = "AUD";
			testHeader2.JZ_Weight = 500.000m;
			testHeader2.JZ_WeightUQ = "KG";

			testLine1 = testHeader1.JobComInvoiceLines.AddNew();
			testLine1.JI_AddInfo = "ORG=DE";
			testLine1.JI_ConcessionOrder = "|";
			testLine1.JI_Description = "Spades and shovels";
			testLine1.JI_LineNo = (short)1;
			testLine1.JI_LinePrice = 40000.0000m;
			testLine1.JI_CountryOfOrigin = "DE";
			testLine1.JI_Tariff = "8201.10.00 01";
			testLine1.JI_WeightUQ = "KG";

			testLine2 = testHeader2.JobComInvoiceLines.AddNew();
			testLine2.JI_AddInfo = "ORG=DE";
			testLine2.JI_ConcessionOrder = "|";
			testLine2.JI_Description = "Spades and shovels";
			testLine2.JI_LineNo = (short)1;
			testLine2.JI_LinePrice = 400.0000m;
			testLine2.JI_CountryOfOrigin = "DE";
			testLine2.JI_Tariff = "8201.10.00 01";
			testLine2.JI_WeightUQ = "KG";

			OrgAddress warehouseAddress = Factory.New<OrgAddress>();
			OrgHeader warehouse = Factory.New<OrgHeader>();
			warehouse.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "123");
			warehouseAddress.OA_OH = warehouse.PK;

			testDec.WarehouseDocAddress.E2_OA_Address = warehouseAddress.PK;
		}

		protected JobDeclaration testDec;
		protected JobComInvoiceHeader testHeader1;
		protected JobComInvoiceHeader testHeader2;
		protected JobComInvoiceLine testLine1;
		protected JobComInvoiceLine testLine2;
		#endregion
	}
}
