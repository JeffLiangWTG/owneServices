using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Moq.Protected;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public class CusEntryInstructionValidationTests : TestCaseWithFactory
	{
		public void TestGBCheckCEI_SubStyle()
		{
			var msgError = "The code you have selected is not in the list.";
			cei.CEI_SubStyle = "#";
			AssertHasMessageErrorContaining(cei.CEI_SubStyleInfo, msgError);
			cei.CEI_SubStyle = "";
			AssertNoMessageErrorContaining(cei.CEI_SubStyleInfo, msgError);
		}

		public void TestNoExceptionThrownWhenValidatingEMCSDeclaration()
		{
			var dec = Factory.New<EMCSJobDeclaration>();
			dec.Invoices.AddNew().InvoiceLines.AddNew();
			AssertNoExceptionThrown(() => dec.RunPreSaveValidation());
		}

		public void TestNIWarehouseValidation()
		{
			var belfast = new RefUNLOCO.Loader(Factory).Load("GBBEL");
			if (belfast.CountryStates == null || string.Compare(belfast.CountryStates.RW_RegionName, "NORTHERN IRELAND", true) != 0)
			{
				var ni = Factory.NewWithValidTestData<RefCountryStates>();
				ni.RW_RegionName = "NORTHERN IRELAND";
				belfast.RL_RW = ni.PK;
			}

			var london = new RefUNLOCO.Loader(Factory).Load("GBLON");
			if (london.CountryStates == null)
			{
				var gb = Factory.NewWithValidTestData<RefCountryStates>();
				gb.RW_RegionName = "ENGLAND";
				london.RL_RW = gb.PK;
			}
			Factory.Save();

			var warehouseBelfast = Factory.New<OrgHeader>();
			warehouseBelfast.OH_Code = "WAR1";
			warehouseBelfast.OH_FullName = "WAREHOUSE 1";
			warehouseBelfast.OH_RL_NKClosestPort = "GBBEL";
			warehouseBelfast.MainAddress.OA_Address1 = "1 TEST";
			warehouseBelfast.MainAddress.OA_RL_NKRelatedPortCode = "GBBEL";
			warehouseBelfast.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;

			var warehouseLondon = Factory.New<OrgHeader>();
			warehouseLondon.OH_Code = "WAR1";
			warehouseLondon.OH_FullName = "WAREHOUSE 1";
			warehouseLondon.OH_RL_NKClosestPort = "GBLON";
			warehouseLondon.MainAddress.OA_Address1 = "1 TEST";
			warehouseLondon.MainAddress.OA_RL_NKRelatedPortCode = "GBLON";
			warehouseLondon.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;

			var warehouseSydney = Factory.New<OrgHeader>();
			warehouseSydney.OH_Code = "WAR2";
			warehouseSydney.OH_FullName = "WAREHOUSE 2";
			warehouseSydney.OH_RL_NKClosestPort = "AUSYD";
			warehouseSydney.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			warehouseSydney.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var importer = Factory.New<OrgHeader>();
				importer.OH_Code = "WENDY";
				importer.OH_FullName = "WENDY THE BUILDER";
				importer.CompanyData.OB_IMUsedBondedWhs = true;
				importer.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
				var cusProcedureHelper = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "", "40", "10", "", "AB DESC", "IMP", intoWarehouse: true, outOfWarehouse: true, group: "JC");
				cusProcedureHelper.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.UnitedKingdom;

				TestWarehouseWithGoodBoolean(importer, cusProcedureHelper, out var entryInstruction, true, "ENT4010", out var message);

				entryInstruction.CEI_OA_Warehouse = warehouseBelfast.MainAddress.PK;
				AssertNoMessageErrors(entryInstruction.CEI_OA_WarehouseInfo);

				entryInstruction.CEI_OA_Warehouse = warehouseLondon.MainAddress.PK;
				AssertNoMessageErrors(entryInstruction.CEI_OA_WarehouseInfo);

				entryInstruction.CEI_OA_Warehouse = warehouseSydney.MainAddress.PK;
				AssertNoMessageErrors(entryInstruction.CEI_OA_WarehouseInfo);

				entryInstruction.CEI_OA_Warehouse2 = warehouseBelfast.MainAddress.PK;
				AssertNoMessageErrors(entryInstruction.CEI_OA_Warehouse2Info);

				entryInstruction.CEI_OA_Warehouse2 = warehouseLondon.MainAddress.PK;
				AssertNoMessageErrors(entryInstruction.CEI_OA_Warehouse2Info);

				entryInstruction.CEI_OA_Warehouse2 = warehouseSydney.MainAddress.PK;
				AssertNoMessageErrors(entryInstruction.CEI_OA_Warehouse2Info);
			}
		}

		void TestWarehouseWithGoodBoolean(OrgHeader importer, RefCusProcedure cusProcedureHelper, out CusEntryInstruction entryInstruction, bool areMultipleEntryInstructionsAllowed, string entryNumber, out string message)
		{
			var declarationMock = Factory.NewMoq<JobDeclaration>();
			declarationMock.Protected().Setup<bool>("GetIsWHSUniversalXMLActive").Returns(true);
			declarationMock.Setup(d => d.AreMultipleEntryInstructionsAllowed).Returns(areMultipleEntryInstructionsAllowed);
			declarationMock.Protected().Setup<bool>("SupportsBondedWarehousingCore").Returns(true);
			var declaration = declarationMock.Object;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			entryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_Style = cusProcedureHelper.ZZ6_Group;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			invoiceLine.JI_Procedure = cusProcedureHelper.ZZ6_ProcedureCode + cusProcedureHelper.ZZ6_PreviousProcedureCode;
			invoiceLine.JI_CEI = entryInstruction.PK;
			entry.CH_CEI_Instruction = entryInstruction.PK;
			entry.EntryNumber = entryNumber;
			entry.CH_WarehouseTransactionStatus = "SOS";
			Assert(entry.IsBondedWarehousingFieldValidationRequired);
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.SetUseBondedWarehouseAutomationForTesting(true);
			message = CusEntryHeader.BondedWarehouseAddressShouldBeInsideDeclarationRegionEU(importer == null ? ZString.Empty : importer.OH_Code, entryInstruction.EntryHeader.EntryHeaderDescriptiveMenuItemText, declaration.Country == null ? ZString.Empty : declaration.Country.RN_DescMultilingual);
		}

		protected override void SetUp()
		{
			base.SetUp();
			dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			cei = dec.CustomsEntryInstructions.AddNew();
		}

		protected JobDeclaration dec;
		protected CusEntryInstruction cei;
	}
}
