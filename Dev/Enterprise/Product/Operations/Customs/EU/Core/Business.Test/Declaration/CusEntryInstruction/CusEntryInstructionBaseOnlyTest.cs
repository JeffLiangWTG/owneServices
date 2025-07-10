using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryInstruction))]
	sealed class CusEntryInstructionBaseOnlyTest : Customs.Business.Testing.CusEntryInstructionAbstractTest
	{
		public void TestIsIntoRegime()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(currentCountry, "IM", "45", "00", "F06", "Description", "IMP");
			procedure.ZZ6_IntoVATWarehouse = "Y";

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			AssertEquals("Precondition: returns false by default", false, instruction.IsIntoRegime);
			invoiceLine.JI_Procedure = "4500F06";
			AssertEquals("Should return true when ZZ6_IntoVATWarehouse is Y", true, instruction.IsIntoRegime);
			procedure.ZZ6_IntoVATWarehouse = "N";
			AssertEquals("Should return false when ZZ6_IntoVATWarehouse is N", false, instruction.IsIntoRegime);
		}

		public void TestApplyProcedureToInvoiceLines()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(currentCountry, "A", "C1", "11", "111", "CPC1 Desc", "IMP", group: "H1,H2");

			helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "Procedure Code");
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "C1", "CPC1 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var dec = Factory.New<JobDeclarationForTestOnly_RequestedProcedure>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(dec, true))
			{
				dec.JE_MessageType = "IMP";
				var instruction = dec.CustomsEntryInstructions.AddNew();
				var line = dec.Invoices.AddNew().InvoiceLines.AddNew();
				line.JI_CEI = instruction.PK;

				instruction.CEI_Style = "H1";
				instruction.CEI_Procedure = "XX";
				Assert("Invalid Code", line.JI_Procedure.IsEmpty);

				line.JI_Procedure = "XXABCD";
				instruction.CEI_Procedure = "C1";
				AssertEquals("First 2 Characters is set", "C1ABCD", line.JI_Procedure);
			}
		}

		public void TestIsRequestedProcedureValid()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(currentCountry, "A", "C1", "11", "111", "CPC1 Desc", "IMP", group: "H1,H2");

			helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "Procedure Code");
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "C1", "CPC1 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var dec = Factory.New<JobDeclarationForTestOnly_RequestedProcedure>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(dec, true))
			{
				dec.JE_MessageType = "IMP";
				var instruction = dec.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = "H1";
				instruction.CEI_Procedure = "C1";

				Assert("Valid Code", instruction.IsRequestedProcedureValid);

				instruction.CEI_Procedure = "XX";
				Assert("Invalid Code", !instruction.IsRequestedProcedureValid);
			}
		}

		public void TestCEI_Procedure_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<CusEntryInstruction>().CEI_ProcedureInfo, JobDeclaration.CaptionKeyImportUCC6);
			CombineAssertions(() =>
			{
				AssertEquals("ShortCaption", "Procedure", resourceStringDataAttribute.ShortCaption);
				AssertEquals("MediumCaption", "Req. Procedure", resourceStringDataAttribute.MediumCaption);
				AssertEquals("Caption", "Requested Procedure", resourceStringDataAttribute.Caption);
				AssertEquals("FullDescription", "[11 09 001 000] Requested Procedure", resourceStringDataAttribute.FullDescription);
			});
		}

		public void TestProcedureDescription()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(currentCountry, "A", "C1", "11", "111", "CPC1 Desc", "IMP", group: "H1,H2");

			helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "Procedure Code");
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "C1", "CPC1 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var dec = Factory.New<JobDeclarationForTestOnly_RequestedProcedure>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(dec, true))
			{
				dec.JE_MessageType = "IMP";
				var instruction = dec.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = "H1";
				instruction.CEI_Procedure = "C1";

				AssertEquals("ProcedureDescription", "CPC1 Desc", instruction.ProcedureDescription);
			}
		}

		public void TestProcedureDescription_Attribute()
		{
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<CusEntryInstruction>().ProcedureDescriptionInfo, JobDeclaration.CaptionKeyImportUCC6);
			CombineAssertions(() =>
			{
				AssertEquals("ShortCaption", "Procedure Desc.", resourceStringDataAttribute.ShortCaption);
				AssertEquals("MediumCaption", "Req. Procedure Description", resourceStringDataAttribute.MediumCaption);
				AssertEquals("Caption", "Requested Procedure Description", resourceStringDataAttribute.Caption);
			});

			AssertEquals("ReadOnly", true, Factory.New<CusEntryInstruction>().ProcedureDescriptionInfo.GetAttribute<ReadOnlyAttribute>().IsReadOnly);
		}

		public void TestIsSimplifiedEntryInstruction()
		{
			CombineAssertions(() =>
			{
				instruction.CEI_Style = ZString.Empty;
				AssertEquals("IsSimplifiedEntryInstruction for empty CEI_Style", expected: false, instruction.IsSimplifiedEntryInstruction);

				instruction.CEI_Style = "I1";
				AssertEquals("IsSimplifiedEntryInstruction for CEI_Style I1", expected: true, instruction.IsSimplifiedEntryInstruction);

				instruction.CEI_Style = "H1";
				AssertEquals("IsSimplifiedEntryInstruction for CEI_Style H1", expected: false, instruction.IsSimplifiedEntryInstruction);
			});
		}

		public void TestIsSimplifiedOrPreliminaryUnderCodeC()
		{
			CombineAssertions(() =>
			{
				instruction.CEI_SubStyle = ZString.Empty;
				AssertEquals("IsSimplifiedOrPreliminaryUnderCodeC is true only if SubStyle is C or F", false, instruction.IsSimplifiedOrPreliminaryUnderCodeC);

				instruction.CEI_SubStyle = EntrySubStyleList.Codes.SimplifiedDeclaration;
				AssertEquals("IsSimplifiedOrPreliminaryUnderCodeC is true only if SubStyle is C or F", true, instruction.IsSimplifiedOrPreliminaryUnderCodeC);

				instruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC;
				AssertEquals("IsSimplifiedOrPreliminaryUnderCodeC is true only if SubStyle is C or F", true, instruction.IsSimplifiedOrPreliminaryUnderCodeC);

				instruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;
				AssertEquals("IsSimplifiedOrPreliminaryUnderCodeC is true only if SubStyle is C or F", false, instruction.IsSimplifiedOrPreliminaryUnderCodeC);
			});
		}

		public void TestHasLoadedGoodsLocation()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default", false, instruction.HasLoadedGoodsLocation);

				_ = instruction.GoodsLocation;
				AssertEquals("Loaded", true, instruction.HasLoadedGoodsLocation);
			});
		}

		public void TestGoodsLocationDescription_Caption()
		{
			AssertEquals("Location of Goods", DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(instruction.GoodsLocationDescription)).Caption);
		}

		public void TestHasHeaderLevelPreviousDocuments()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.PreviousDocuments.AddNew();
			AssertEquals("No data at invoice or entry instruction", false, instruction.HasHeaderLevelPreviousDocuments());
			invoice.PreviousDocuments.AddNew();
			AssertEquals("Has on invoice", true, instruction.HasHeaderLevelPreviousDocuments());
			invoiceLine.JI_CEI = ZGuid.Empty;
			AssertEquals("No data at invoice or entry instruction", false, instruction.HasHeaderLevelPreviousDocuments());
			instruction.PreviousDocuments.AddNew();
			AssertEquals("Has on entry instruction", true, instruction.HasHeaderLevelPreviousDocuments());
		}

		public void TestGoodsLocation()
		{
			var goodsLocation = instruction.GoodsLocation;
			CombineAssertions(() =>
			{
				AssertEquals("CGL_ParentID", instruction.PK, goodsLocation.CGL_ParentID);
				AssertEquals("CGL_ParentTableCode", ZArchitecture.Schema.CusEntryInstructionSchema.Constants.Prefix, goodsLocation.CGL_ParentTableCode);
				AssertEquals("CGL_LocationUse", CusGoodsLocationUseList.Codes.EntryInstruction, goodsLocation.CGL_LocationUse);
				AssertSame("Cached", goodsLocation, instruction.GoodsLocation);
				AssertEquals("IsRegisteredEditableChildObject", true, instruction.IsRegisteredEditableChildObject(goodsLocation));
			});
		}

		public void TestGoodsLocationDescription()
		{
			CombineAssertions(() =>
			{
				AssertEquals("GoodsLocationDescription empty when there's no GoodsLocation", ZString.Empty, instruction.GoodsLocationDescription);
				AssertNull("GoodsLocation doesn't exist", Customs.Business.CusGoodsLocation.Load<CusGoodsLocation>(instruction, CusGoodsLocationUseList.Codes.EntryInstruction));

				var goodsLocation = instruction.GoodsLocation;
				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
				AssertEquals("GoodsLocationDescription when there's GoodsLocation", "Z", instruction.GoodsLocationDescription);
				AssertNotNull("GoodsLocation exists", Customs.Business.CusGoodsLocation.Load<CusGoodsLocation>(instruction, CusGoodsLocationUseList.Codes.EntryInstruction));
			});
		}

		public void TestGoodsLocationDescription_RegisterEditableChildObject()
		{
			CusGoodsLocation.New<CusGoodsLocation>(instruction, CusGoodsLocationUseList.Codes.Departure);
			_ = instruction.GoodsLocationDescription;
			AssertEquals("IsRegisteredEditableChildObject", true, instruction.IsRegisteredEditableChildObject(instruction.GoodsLocation));
		}

		public void TestGoodsLocationDescriptionInfo()
		{
			AssertEquals(nameof(CusEntryInstruction.GoodsLocationDescription), instruction.GoodsLocationDescriptionInfo.Name);
		}

		public void TestICusGoodsLocationProvider_ProviderKey()
		{
			AssertEquals("LVDECL", (instruction as ICusGoodsLocationProvider).ProviderKey);
		}

		public void TestICusGoodsLocationTypeSupporter()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				var declaration = Factory.New<JobDeclaration>();
				instruction = declaration.CustomsEntryInstructions.AddNew();
				AssertEquals("Enterprise.Customs.ES.Business.CusGoodsLocation", (instruction as ICusGoodsLocationTypeSupporter).GoodsLocationType.FullName);
			}
		}

		public void TestSetGoodsLocationReadOnly()
		{
			var goodsLocation = instruction.GoodsLocation;
			instruction.SetGoodsLocationReadOnly();
			AssertEquals("Goods Locations Set ReadOnly is false", false, goodsLocation.ReadOnly);
		}

		public void TestFromWarehouseTypeAndFromWarehouseCode()
		{
			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			var warehouseAddress = warehouse.MainAddress;
			var owner = Factory.NewWithValidTestData<OrgHeader>();

			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2;
			authorizationHeader.CPH_OH_PermitHolder = owner.PK;
			authorizationHeader.CPH_Number = "001";
			authorizationHeader.CPH_OA_AppliesTo = warehouseAddress.PK;

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_OA_Warehouse = warehouseAddress.PK;

			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2;
			cusAuthorizationUsage.AGC_Number = "001";
			cusAuthorizationUsage.AGC_OH_Owner = owner.PK;

			CombineAssertions("When does not have CCP", () =>
			{
				AssertEquals("FromWarehouseType", CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, entryInstruction.FromWarehouseType);
				AssertEquals("FromWarehouseCode", ZString.Empty, entryInstruction.FromWarehouseCode);
			});

			warehouseAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "U002", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			entryInstruction.Factory.ClearCachedValue<CusAuthorisationHeader[]>($"GetAuthorisationHeaders|{GlbCompany.CurrentCompany.GC_RN_NKCountryCode}|{ZDateTime.Today.ToISO8601ShortDateString()}|{authorizationHeader.CPH_OA_AppliesTo.ToStringKey()}|CW1_CW2_CWP");

			CombineAssertions("When has CCP", () =>
			{
				AssertEquals("FromWarehouseType", CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, entryInstruction.FromWarehouseType);
				AssertEquals("FromWarehouseCode", "U002", entryInstruction.FromWarehouseCode);
			});
		}

		public void TestToWarehouseTypeAndToWarehouseCode()
		{
			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			var warehouseAddress = warehouse.MainAddress;
			var owner = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_OA_Warehouse2 = warehouseAddress.PK;

			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			authorizationHeader.CPH_OH_PermitHolder = owner.PK;
			authorizationHeader.CPH_Number = "001";
			authorizationHeader.CPH_OA_AppliesTo = warehouseAddress.PK;

			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			cusAuthorizationUsage.AGC_Number = "001";
			cusAuthorizationUsage.AGC_OH_Owner = owner.PK;

			CombineAssertions("When does not have CCP", () =>
			{
				AssertEquals("FromWarehouseType", CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, entryInstruction.ToWarehouseType);
				AssertEquals("FromWarehouseCode", ZString.Empty, entryInstruction.ToWarehouseCode);
			});

			warehouseAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "U002", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			entryInstruction.Factory.ClearCachedValue<CusAuthorisationHeader[]>($"GetAuthorisationHeaders|{GlbCompany.CurrentCompany.GC_RN_NKCountryCode}|{ZDateTime.Today.ToISO8601ShortDateString()}|{authorizationHeader.CPH_OA_AppliesTo.ToStringKey()}|CW1_CW2_CWP");

			CombineAssertions("When has CCP", () =>
			{
				AssertEquals("FromWarehouseType", CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, entryInstruction.ToWarehouseType);
				AssertEquals("FromWarehouseCode", "U002", entryInstruction.ToWarehouseCode);
			});
		}

		public void TestAuthorizationUsagesCached()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			authorizationHeader.CPH_OH_PermitHolder = owner.PK;
			authorizationHeader.CPH_Number = "001";
			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			authorizationHeader.CPH_OA_AppliesTo = warehouse.MainAddress.PK;

			var authorizationUsage = instruction.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			authorizationUsage.AGC_OH_Owner = owner.PK;

			instruction.CEI_OA_Warehouse = warehouse.MainAddress.PK;
			instruction.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;

			var fromWarehouseAuthorizationUsages = instruction.FromWarehouseAuthorizationUsages;
			var toWarehouseAuthorizationUsages = instruction.ToWarehouseAuthorizationUsages;
			AssertSame("FromWarehouseAuthorizationUsages cached", fromWarehouseAuthorizationUsages, instruction.FromWarehouseAuthorizationUsages);
			AssertSame("ToWarehouseAuthorizationUsages cached", toWarehouseAuthorizationUsages, instruction.ToWarehouseAuthorizationUsages);

			authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2;
			Assert("FromWarehouseAuthorizationUsages recalculated", fromWarehouseAuthorizationUsages != instruction.FromWarehouseAuthorizationUsages);
			Assert("ToWarehouseAuthorizationUsages recalculated", toWarehouseAuthorizationUsages != instruction.ToWarehouseAuthorizationUsages);
		}

		public void TestGetAuthorizationUsages()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			instruction.CEI_OA_Warehouse = owner.MainAddress.PK;
			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			var authorizationUsage = instruction.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			authorizationUsage.AGC_OH_Owner = owner.PK;

			CombineAssertions("Warehouse OrgPK equals to Usage Owner", () =>
			{
				AssertEquals(1, instruction.FromWarehouseAuthorizationUsages.Count);
				AssertEquals(authorizationUsage.PK, instruction.FromWarehouseAuthorizationUsages[0].PK);
			});

			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			var permitHolder = Factory.NewWithValidTestData<OrgHeader>();
			authorizationHeader.CPH_OH_PermitHolder = permitHolder.PK;
			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			authorizationHeader.CPH_OA_AppliesTo = warehouse.MainAddress.PK;

			instruction.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;
			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			authorizationUsage = instruction.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			authorizationUsage.AGC_OH_Owner = permitHolder.PK;

			CombineAssertions("Permit Holder equals to Usage Owner", () =>
			{
				AssertEquals(1, instruction.ToWarehouseAuthorizationUsages.Count);
				AssertEquals(authorizationHeader.CPH_OH_PermitHolder, instruction.ToWarehouseAuthorizationUsages[0].AGC_OH_Owner);
			});
		}

		public void TestOldOwner()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			instruction.CEI_OA_Warehouse = owner.MainAddress.PK;

			AssertNull(instruction.OldOwner);

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "WENDY";
			importer.OH_FullName = "WENDY THE BUILDER";
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			importer.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;

			declaration.JE_OH_Importer = importer.PK;

			AssertNotNull(instruction.OldOwner);
			AssertEquals(importer.PK, instruction.OldOwner.PK);
		}

		public void TestWarehouseFor27()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			helper.CreateNewOrGetExistingDataGrouping(currentCountry);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, "AA", "BB", ZString.Empty, "description", "EXP", intoWarehouse: true);
			var procedure2 = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, "CC", "DD", ZString.Empty, "description2", "EXP", outOfWarehouse: true);

			Factory.Save();

			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			warehouse.OH_Code = "warehouse1";
			warehouse.CompanyData.OB_IMUsedBondedWhs = true;
			var warehouse2 = Factory.NewWithValidTestData<OrgHeader>();
			warehouse2.OH_Code = "warehouse2";
			warehouse2.CompanyData.OB_IMUsedBondedWhs = true;

			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var cusInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine1.JI_CEI = cusInstruction.PK;
			cusInstruction.CEI_OA_Warehouse = warehouse.MainAddress.PK;
			cusInstruction.CEI_OA_Warehouse2 = warehouse2.MainAddress.PK;

			AssertEquals(null, cusInstruction.WarehouseFor27);

			var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var cusInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine2.JI_CEI = cusInstruction2.PK;
			invoiceLine2.JI_Procedure = "AABB";
			cusInstruction2.CEI_OA_Warehouse = warehouse.MainAddress.PK;
			cusInstruction2.CEI_OA_Warehouse2 = warehouse2.MainAddress.PK;

			AssertEquals(warehouse2.MainAddress.PK, cusInstruction2.WarehouseFor27.PK);

			warehouse2.CompanyData.OB_IMUsedBondedWhs = false;
			AssertEquals(warehouse2.MainAddress.PK, cusInstruction2.WarehouseFor27.PK);

			var invoiceLine3 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var cusInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine3.JI_CEI = cusInstruction3.PK;
			invoiceLine3.JI_Procedure = "CCDD";
			cusInstruction3.CEI_OA_Warehouse = warehouse.MainAddress.PK;
			cusInstruction3.CEI_OA_Warehouse2 = warehouse2.MainAddress.PK;

			AssertEquals(warehouse.MainAddress.PK, cusInstruction3.WarehouseFor27.PK);

			warehouse.CompanyData.OB_IMUsedBondedWhs = false;
			AssertEquals(warehouse.MainAddress.PK, cusInstruction3.WarehouseFor27.PK);
		}

		public void TestIsCountryOfSupplySameForAllInvoiceLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cei = declaration.CustomsEntryInstructions.AddNew();
			Assert(cei.IsCountryOfSupplySameForAllInvoiceLines);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			Assert(cei.IsCountryOfSupplySameForAllInvoiceLines);
			invoiceLine1.JI_CEI = cei.PK;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = cei.PK;
			Assert(cei.IsCountryOfSupplySameForAllInvoiceLines);
			invoiceLine2.ZG_CountryOfSupply = Core.Constants.CountryCodes.Australia;
			AssertEquals(false, cei.IsCountryOfSupplySameForAllInvoiceLines);
		}

		public void TestAddInfoLookups()
		{
			AssertType<AddInfoCusEntryInstructionLookups>(instruction.AddInfoLookups);
		}

		public void TestValidation()
		{
			AssertType<CusEntryInstructionValidation>(instruction.Validation);
		}

		public void TestCEI_SubStyle()
		{
			instruction.CEI_SubStyle = "X";
			AssertEquals("X", instruction.CEI_SubStyle);
		}

		public void TestCEI_SubStyle_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var info = instruction.CEI_SubStyleInfo;

			CombineAssertions("ImportUCC6", () =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					AssertCaption(info, instruction.MultipleKeysToUse, "Sub Style", "Sub Style", "[11 02 001 000] Additional Declaration Type");
				}
			});

			CombineAssertions("Not UCC6", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
				{
					AssertCaption(info, instruction.MultipleKeysToUse, "Sub Style", "Sub Style", ZString.Empty);
				}
			});
		}

		void AssertCaption(ZPropertyInfo info, IReadOnlyList<string> keys, string expectedHumanReadableName, string expectedCaption, string expectedFullDescription)
		{
			AssertEquals("HumanReadableName", expectedHumanReadableName, info.HumanReadableName);
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, keys);
			AssertEquals("Caption", expectedCaption, captionResourceString.Caption);
			AssertEquals("FullDescription", expectedFullDescription, captionResourceString.FullDescription);
		}

		public void TestCEI_OH_Owner_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(
				instruction.CEI_OH_OwnerInfo,
				multipleResourceKey: JobDeclaration.CaptionKeyImportUCC6,
				caption: "Owner",
				fullDescription: "[3/8] Owner of Goods"
			);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(
				instruction.CEI_OH_OwnerInfo,
				multipleResourceKey: string.Empty,
				caption: "Owner",
				fullDescription: string.Empty
			);
		}

		public void TestCEI_TotalInnerPackages()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			AssertEquals("Inner Pack.", DataBoundResourceStrings.GetDataForProperty(entryInstruction.CEI_TotalInnerPackagesInfo).ShortCaption);
			AssertEquals("Total Inner Packages", DataBoundResourceStrings.GetDataForProperty(entryInstruction.CEI_TotalInnerPackagesInfo).Caption);
			AssertEquals("This field is used to communicate the number of inner packages to the Transit Warehouse only.", DataBoundResourceStrings.GetDataForProperty(entryInstruction.CEI_TotalInnerPackagesInfo).FullDescription);
		}

		public void TestAddInfoValidation()
		{
			AssertType<AddInfoCusEntryInstructionValidation>(instruction.AddInfoValidation);
		}

		public void TestJobDeclaration()
		{
			AssertType<JobDeclaration>(instruction.JobDeclaration);
		}

		public void TestLookups()
		{
			AssertType<CusEntryInstructionLookups>(instruction.Lookups);
		}

		public void TestIAddInfoManager_AddInfo()
		{
			AssertType<AddInfoCusEntryInstruction>(((IAddInfoManager)instruction).AddInfo);
		}

		public void TestInvoices()
		{
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = ZGuid.Empty;
			invoiceLine2.JI_CEI = ZGuid.Empty;
			AssertEquals("No invoices", 0, instruction.Invoices.Count());

			invoiceLine1.JI_CEI = instruction.PK;
			AssertEquals("One invoice", 1, instruction.Invoices.Count());
			AssertEquals("Invoice PK", invoiceHeader1.PK, instruction.Invoices.ElementAt(0).PK);

			invoiceLine2.JI_CEI = instruction.PK;
			AssertEquals("Two invoices", 2, instruction.Invoices.Count());
			AssertNotNull("Invoice 1", instruction.Invoices.SingleOrDefault(x => x.PK == invoiceHeader1.PK));
			AssertNotNull("Invoice 2", instruction.Invoices.SingleOrDefault(x => x.PK == invoiceHeader2.PK));

			var invoiceLine3 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine3.JI_CEI = instruction.PK;
			AssertEquals("Two invoices", 2, instruction.Invoices.Count());
			AssertNotNull("Invoice 1", instruction.Invoices.SingleOrDefault(x => x.PK == invoiceHeader1.PK));
			AssertNotNull("Invoice 2", instruction.Invoices.SingleOrDefault(x => x.PK == invoiceHeader2.PK));
		}

		public void TestOrgCusCodeTypeForWarehouse()
		{
			AssertEquals(OrgCusCode.CodeTypes.ControlledPremisesID, Factory.New<CusEntryInstructionForTest>().OrgCusCodeTypeForWarehouseExposed);
		}

		public void TestToWarehouseCode()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var warehouseAddress = orgHeader.Addresses.AddNew();
			var ccpCustomsCode = orgHeader.CustomsCodes.AddNew();
			ccpCustomsCode.OK_CustomsRegNo = "DBNSOS78901";
			ccpCustomsCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.Country.Code;
			ccpCustomsCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			ccpCustomsCode.OK_OA_PremisesAddress = warehouseAddress.PK;

			instruction.CEI_OA_Warehouse2 = warehouseAddress.PK;
			AssertEquals("DBNSOS78901", instruction.ToWarehouseCode);

			ccpCustomsCode.OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
			AssertEquals("", instruction.ToWarehouseCode);

			ccpCustomsCode.OK_CodeType = "XXX";
			AssertEquals("", instruction.ToWarehouseCode);
		}

		public void TestPreviousDocuments()
		{
			AssertType<PreviousDocumentCollection>(instruction.PreviousDocuments);
		}

		public void TestIPreviousDocumentsProvider()
		{
			var previousDocumentsProvider = instruction as IPreviousDocumentsProvider;
			AssertNotNull("EntryInstruction as IPreviousDocumentsProvider", previousDocumentsProvider);
			AssertSame("Previous Documents", instruction.PreviousDocuments, previousDocumentsProvider.PreviousDocuments);
		}

		public void TestSupportingDocumentsType()
		{
			AssertType<SupportingDocumentCollection>(instruction.SupportingDocuments);
		}

		public void TestAdditionalInfosType()
		{
			AssertType<AdditionalInfoCollection>(instruction.AdditionalInfos);
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var supportingInfoTypes = ((ICusSupportingInfoTypeSupporter)instruction).GetCusSupportingInfoTypes();

			CombineAssertions(() =>
			{
				AssertEquals("SupportingInfoTypes Count", 4, supportingInfoTypes.Count);
				AssertEquals("Contains PreviousDocument?", true, supportingInfoTypes.ContainsKey(Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument));
				AssertEquals("Contains SupportingDocument?", true, supportingInfoTypes.ContainsKey(Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument));
				AssertEquals("RequestedDocument", true, supportingInfoTypes.ContainsKey(CusSupportingInfoTypeList.Codes.InstructionRequestedDocument));
			});
		}

		public void TestICanBeImportOrExport()
		{
			var iore = instruction as ICanBeImportOrExport;

			CombineAssertions(() =>
			{
				AssertEquals("Level", UniversalReferenceConstants.RefCusCodeListLevelType.Both, iore.Level);

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("IsImport", true, iore.IsImport);
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("IsExport", true, iore.IsExport);

				AssertEquals("CountryCode", declaration.CountryCode, iore.TrueCountryCode);
				AssertEquals("Data Grouping", declaration.GetDefaultDataGroupingCode(), iore.DataGroupingCode);
			});
		}

		public void TestCusAuthorizationUsages()
		{
			AssertType<CusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction>>(instruction.CusAuthorizationUsages);
		}

		public void TestFiscalReferences()
		{
			CombineAssertions(() =>
			{
				var fiscalReferences = instruction.FiscalReferences;
				AssertEquals("IsRegisteredEditableChildObject", true, instruction.IsRegisteredEditableChildObject(fiscalReferences));
				AssertSame("Cached", fiscalReferences, instruction.FiscalReferences);
			});
		}

		public void TestDEDefaultingOfFiscalReferences()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;

				AssertEquals("Pre-Req: FiscalReferencesSupport is false for this test to correctly expect a lack of defaulting behavior", false, declaration.Configuration.InstructionConfiguration.FiscalReferencesSupport(declaration));

				var cpvParty = Factory.NewWithValidTestData<OrgHeader>();
				cpvParty.OH_Code = "CPV PARTY";
				cpvParty.MainAddress.Address1 = "CPV ADDRESS";
				var eori = cpvParty.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "11111111111", Core.Constants.CountryCodes.Germany);

				var importer = Factory.NewWithValidTestData<OrgHeader>();
				importer.OH_Code = "IMPORTER";
				declaration.JE_OH_Importer = importer.PK;

				OrgRelatedParty relation = Factory.New<OrgRelatedParty>();
				relation.PR_OH_RelatedParty = cpvParty.PK;
				relation.PR_OH_Parent = importer.PK;
				relation.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
				relation.PR_PartyType = RelatedPartyTypeList.Codes.CustomsPostponedVatAccounting;

				var euAddInfo = EUOrgImpAddInfo.Get(importer, Core.Constants.CountryCodes.Germany);
				euAddInfo.ZO_UseFr3FiscalRepresentation = true;

				Factory.Save();

				var cei1 = declaration.CustomsEntryInstructions.AddNew();
				AssertEquals("Whatever settings might be, the DE entry instruction should not contain any FR3 type of fiscal reference.", false, cei1.FiscalReferences.Cast<CusFiscalReference>().Any(f => f.CFR_Code == FiscalReferenceCodeList.Codes.FR3_TaxRepresentative));
			}
		}

		public void TestDelete_CusAuthorizationUsages()
		{
			CombineAssertions(() =>
			{
				var authorizationUsage1 = instruction.CusAuthorizationUsages.AddNew();
				var authorizationUsage2 = instruction.CusAuthorizationUsages.AddNew();
				instruction.Delete();
				AssertEquals("authorizationUsage deleted", true, authorizationUsage1.IsDeleted);
				AssertEquals("authorizationUsage deleted", true, authorizationUsage2.IsDeleted);
			});
		}

		public void TestDelete_DV1DetailsPivots()
		{
			CombineAssertions(() =>
			{
				var dv1DetailsPivots1 = instruction.DV1DetailsPivots.AddNew();
				var dv1DetailsPivots2 = instruction.DV1DetailsPivots.AddNew();
				instruction.Delete();
				AssertEquals("dv1DetailsPivots1 deleted", true, dv1DetailsPivots1.IsDeleted);
				AssertEquals("dv1DetailsPivots2 deleted", true, dv1DetailsPivots2.IsDeleted);
			});
		}

		public void TestDelete_FiscalReferences()
		{
			CombineAssertions(() =>
			{
				var fiscalReferences1 = instruction.FiscalReferences.AddNew();
				var fiscalReferences2 = instruction.FiscalReferences.AddNew();
				instruction.Delete();
				AssertEquals("fiscalReferences1 deleted", true, fiscalReferences1.IsDeleted);
				AssertEquals("fiscalReferences2 deleted", true, fiscalReferences2.IsDeleted);
			});
		}

		public void TestCusSupplyChainActorReferences()
		{
			CombineAssertions(() =>
			{
				var supplyChainActorReferences = instruction.CusSupplyChainActorReferences;
				AssertEquals("IsRegisteredEditableChildObject", true, instruction.IsRegisteredEditableChildObject(supplyChainActorReferences));
				AssertSame("Cached", supplyChainActorReferences, instruction.CusSupplyChainActorReferences);
				AssertType<CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>>(nameof(instruction.CusSupplyChainActorReferences), instruction.CusSupplyChainActorReferences);
			});
		}

		public void TestDelete_CusSupplyChainActorReferences()
		{
			CombineAssertions(() =>
			{
				var supplyChainActorReference1 = instruction.CusSupplyChainActorReferences.AddNew();
				var supplyChainActorReference2 = instruction.CusSupplyChainActorReferences.AddNew();
				instruction.Delete();
				AssertEquals("supplyChainActorReference1 deleted", true, supplyChainActorReference1.IsDeleted);
				AssertEquals("supplyChainActorReference2 deleted", true, supplyChainActorReference2.IsDeleted);
			});
		}

		public void TestZG_SealsCountCaption()
		{
			AssertEquals("Caption", "Seals Quantity", DataBoundResourceStrings.GetDataForProperty(instruction.ZG_SealsCountInfo).Caption);
		}

		public void TestSeals()
		{
			CombineAssertions(() =>
			{
				AssertType<SealNumberCollection>("Correct type", instruction.Seals);
				AssertEquals("IsRegisteredEditableChildObject", true, instruction.IsRegisteredEditableChildObject(instruction.Seals));
			});
		}

		public void TestGuarantees()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();

			AssertNotNull(nameof(entryInstruction.Guarantees), entryInstruction.Guarantees);
		}

		public void TestEmptyGuaranteesIfNecessary()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();

			jobDeclaration.JE_MessageType = "IMP";
			entryInstruction.Guarantees.AddNew();
			entryInstruction.Guarantees.AddNew();

			jobDeclaration.JE_MessageType = "EXP";
			AssertEquals("EntryInstruction.Guarantees Count", 0, entryInstruction.Guarantees.Count);
		}

		public void TestGetCusCodeDataTypes()
		{
			var supportedCusCodeDataTypes = ((ICusCodeDataTypeSupporter)instruction).GetCusCodeDataTypes();
			AssertEquals("Supported CusCodeDataTypes", 1, supportedCusCodeDataTypes.Count);
			AssertEquals("SealNumber Type", typeof(SealNumber), supportedCusCodeDataTypes["SNO"]);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			instruction = declaration.CustomsEntryInstructions.AddNew();
		}

		JobDeclaration declaration;
		CusEntryInstruction instruction;

		public void TestGetEffectiveSealNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertArrayEqualsByElements(nameof(CusEntryInstruction.GetEffectiveSealNumbers), System.Array.Empty<ZString>(), entryInstruction.GetEffectiveSealNumbers().ToArray());

			var cnt1 = AddNewDeclarationContainerWithSeals("A", "B");
			var cnt2 = AddNewDeclarationContainerWithSeals("C", "B");
			var cnt3 = AddNewDeclarationContainerWithSeals("D", "E");
			entryInstruction.Seals.AddNew().CY_Data = "";
			entryInstruction.Seals.AddNew().CY_Data = "F";
			entryInstruction.Seals.AddNew().CY_Data = "D";

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainer(cnt1).IsForInvoiceLine = true;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainer(cnt2).IsForInvoiceLine = true;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainer(cnt3).IsForInvoiceLine = true;

			AssertContainsExactElementsInAnyOrder(nameof(CusEntryInstruction.GetEffectiveSealNumbers), new ZString[] { "F", "D", "A", "B", "C", "E" }, entryInstruction.GetEffectiveSealNumbers().ToArray());

			CusContainer AddNewDeclarationContainerWithSeals(ZString seal, ZString secondSeal)
			{
				var container = declaration.CusContainers.AddNew();
				container.CO_Seal = seal;
				container.CO_SecondSeal = secondSeal;
				return container;
			}
		}

		public void TestRequestedDocuments()
		{
			AssertType<RequestedDocumentCollection>(instruction.RequestedDocuments);
		}

		public void TestRequestedDocuments_ReadOnly() => CombineAssertions(() =>
		{
			AssertEquals("Default", false, instruction.RequestedDocuments.ReadOnly);

			var instructionMock = Factory.NewMoq<CusEntryInstruction>();
			instructionMock
				.Protected()
				.Setup<ZBool>("RequestedDocumentsReadOnly")
				.Returns(true);
			AssertEquals("override", true, instructionMock.Object.RequestedDocuments.ReadOnly);
		});

		#region IUcc6ValueProvider

		public void TestIUcc6ValueProvider_IsUCC6()
		{
			IUcc6ValueProvider ucc6ValueProvider = instruction;

			AssertEquals("Non-UCC6", false, ucc6ValueProvider.IsUCC6);

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				AssertEquals("UCC6", true, ucc6ValueProvider.IsUCC6);
			}
		}

		public void TestIUcc6ValueProvider_IsExport()
		{
			IUcc6ValueProvider ucc6ValueProvider = instruction;
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertEquals("EXP - IsExport", true, ucc6ValueProvider.IsExport);

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertEquals("IMP - IsExport", false, ucc6ValueProvider.IsExport);
		}

		public void TestIUcc6ValueProvider_IsImport()
		{
			IUcc6ValueProvider ucc6ValueProvider = instruction;
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertEquals("EXP - IsImport", false, ucc6ValueProvider.IsImport);

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertEquals("IMP - IsImport", true, ucc6ValueProvider.IsImport);
		}

		#endregion

		public void TestCurrencyConverter()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			CombineAssertions(() =>
			{
				AssertType<CurrencyConverterWithDataProvider>("Type", entryInstruction.CurrencyConverter);
				AssertSame("Cached", entryInstruction.CurrencyConverter, entryInstruction.CurrencyConverter);
			});
		}

		public void TestAdditionalInfos()
		{
			var additionalInfoItems = instruction.AdditionalInfos;
			AssertNotNull(nameof(instruction.AdditionalInfos), additionalInfoItems);
			AssertEquals("Additional Info Count", 0, additionalInfoItems.Count);

			var additionalInfo = additionalInfoItems.AddNew();

			AssertEquals(nameof(additionalInfo.CSI_ParentID), instruction.PK, additionalInfo.CSI_ParentID);
			AssertEquals(nameof(additionalInfo.CSI_ParentTableCode), CusEntryInstructionSchema.Constants.Prefix, additionalInfo.CSI_ParentTableCode);
		}

		public void TestSupportingDocuments()
		{
			var supportingDocumentItems = instruction.SupportingDocuments;
			AssertNotNull(nameof(instruction.SupportingDocuments), supportingDocumentItems);
			AssertEquals("Supporting Document Count", 0, supportingDocumentItems.Count);

			var supportingDocument = supportingDocumentItems.AddNew();

			AssertEquals(nameof(supportingDocument.CSI_ParentID), instruction.PK, supportingDocument.CSI_ParentID);
			AssertEquals(nameof(supportingDocument.CSI_ParentTableCode), CusEntryInstructionSchema.Constants.Prefix, supportingDocument.CSI_ParentTableCode);
		}

		public void TestSupportingDocumentsValidationDecider()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertType<UCC6ImportSupportingDocumentValidationDecider>(((ISupportingDocumentsProviderWithValidationDecider)instruction).ValidationDecider);
			}
		}

		public void TestAdditionalInfosValidationDecider()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertType<UCC6ImportAdditionalInfoValidationDecider>(((IAdditionalInfosProviderWithValidationDecider)instruction).ValidationDecider);
			}
		}

		public void TestCusFiscalReferenceValidationDecider()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertType<UCC6ImportCusFiscalReferenceValidationDecider>(((ICusFiscalReferenceProviderWithValidationDecider)instruction).ValidationDecider);
			}
		}

		public void TestCusAuthorizationUsageValidationDecider()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertType<UCC6ImportCusAuthorizationUsageValidationDecider>(((ICusAuthorizationUsageProviderWithValidationDecider)instruction).ValidationDecider);
			}
		}

		public void TestPreviousDocumentValidationDecider()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertType<UCC6ImportPreviousDocumentValidationDecider>(((IPreviousDocumentsProviderWithValidationDecider)instruction).ValidationDecider);
			}
		}

		public void TestCusAuthorisationChangeCreatesSupportingDocs()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping("DE");
			helper.CreateNewOrGetExistingCusCodeType("DC44I", "Supporing Docs", "DE");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("AuthCode", "AuthCode", "DC44I", "DE");
			var doc1 = helper.CreateNewOrGetExistingCusCodeList("DE", "DC44I", "DEF", "Sup doc1 with AuthCode", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			var doc2 = helper.CreateNewOrGetExistingCusCodeList("DE", "DC44I", "EFG", "Sup doc2 with AuthCode", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList("DE", "DC44I", "ZZZ", "Sup doc with no AuthCode", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeListAttribute(doc1.PK, "AuthCode", "ABC");
			helper.CreateNewOrGetExistingCusCodeListAttribute(doc2.PK, "AuthCode", "ABC");
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var permitHolder = ZGuid.NewZGuid();
				var header1 = Factory.New<CusAuthorisationHeader>();
				header1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
				header1.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
				header1.CPH_IsActive = true;
				header1.CPH_Type = "ABC";
				header1.CPH_Number = "ABC1234567";
				header1.CPH_OH_PermitHolder = permitHolder;
				header1.CPH_StartDate = ZDate.Today;

				var header2 = Factory.New<CusAuthorisationHeader>();
				header2.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
				header2.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
				header2.CPH_IsActive = true;
				header2.CPH_Type = "XYZ";
				header2.CPH_Number = "XYZ1234567";
				header2.CPH_OH_PermitHolder = permitHolder;
				header2.CPH_StartDate = ZDate.Today;

				var dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = "IMP";
				var cei = dec.CustomsEntryInstructions.AddNew();
				var cei2 = dec.CustomsEntryInstructions.AddNew();
				var inv = dec.Invoices.AddNew();
				var line1 = inv.InvoiceLines.AddNew();
				var line2 = inv.InvoiceLines.AddNew();
				var line3 = inv.InvoiceLines.AddNew();
				var line4 = inv.InvoiceLines.AddNew();

				line1.JI_CEI = cei.PK;
				line2.JI_CEI = cei.PK;
				line3.JI_CEI = cei.PK;
				line4.JI_CEI = cei2.PK;

				var existingDoc = line2.SupportingDocuments.AddNew();
				existingDoc.CSI_Code = "DEF";
				existingDoc.CSI_ReferenceNumber = "EX9999999";

				var usage = cei.CusAuthorizationUsages.AddNew();

				usage.AGC_OH_Owner = permitHolder;
				usage.AGC_Code = "XYZ";

				CombineAssertions("No doc with AuthCode XYZ", () =>
				{
					AssertEquals("Line 1 not created", 0, line1.SupportingDocuments.Count);
					AssertEquals("Line 2 not changed", 1, line2.SupportingDocuments.Count);
					AssertEquals("Line 2 CSI_Code", "DEF", line2.SupportingDocuments[0].CSI_Code);
					AssertEquals("Line 2 CSI_Ref", "EX9999999", line2.SupportingDocuments[0].CSI_ReferenceNumber);
					AssertEquals("Line 3 not created", 0, line3.SupportingDocuments.Count);
					AssertEquals("Line 4 not created", 0, line4.SupportingDocuments.Count);
				});

				usage.AGC_Code = "ABC";

				CombineAssertions("Doc DEF with AuthCode ABC", () =>
				{
					AssertEquals("Line1 2 created", 2, line1.SupportingDocuments.Count);
					AssertNotNull("Line1 Item 1", line1.SupportingDocuments.Where(x => x.CSI_Code == "DEF" && x.CSI_ReferenceNumber == "ABC1234567").Single());
					AssertNotNull("Line1 Item 2", line1.SupportingDocuments.Where(x => x.CSI_Code == "EFG" && x.CSI_ReferenceNumber == "ABC1234567").Single());

					AssertEquals("Line2  1 added", 2, line2.SupportingDocuments.Count);
					AssertNotNull("Line2 Item 1", line2.SupportingDocuments.Where(x => x.CSI_Code == "DEF" && x.CSI_ReferenceNumber == "EX9999999").Single());
					AssertNotNull("Line2 Item 2", line2.SupportingDocuments.Where(x => x.CSI_Code == "EFG" && x.CSI_ReferenceNumber == "ABC1234567").Single());

					AssertEquals("Line 3 created", 2, line3.SupportingDocuments.Count);
					AssertEquals("Line 4 not created - wrong CEI", 0, line4.SupportingDocuments.Count);
				});
			}
		}

		public void TestGetSupportingDocTypesForAuthorisation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping("DE");
			helper.CreateNewOrGetExistingCusCodeType("DC44I", "Supporing Docs", "DE");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("AuthCode", "AuthCode", "DC44I", "DE");
			var doc1 = helper.CreateNewOrGetExistingCusCodeList("DE", "DC44I", "DEF", "Sup doc1 with AuthCode", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			var doc2 = helper.CreateNewOrGetExistingCusCodeList("DE", "DC44I", "EFG", "Sup doc2 with AuthCode", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList("DE", "DC44I", "ZZZ", "Sup doc with no AuthCode", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeListAttribute(doc1.PK, "AuthCode", "ABC");
			helper.CreateNewOrGetExistingCusCodeListAttribute(doc2.PK, "AuthCode", "ABC");
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = "IMP";
				var cei = dec.CustomsEntryInstructions.AddNew();

				var inv = dec.Invoices.AddNew();
				var line1 = inv.InvoiceLines.AddNew();
				line1.JI_CEI = cei.PK;

				var docCodes = cei.GetSupportingDocTypesForAuthorisation("XYZ");
				AssertEquals("No codes", 0, docCodes.Count);
				docCodes = cei.GetSupportingDocTypesForAuthorisation("ABC");
				AssertContainsExactElementsInAnyOrder("Codes loaded", new[] { "DEF", "EFG" }, docCodes);
			}
		}

		[TestDate(2023, 6, 28)]
		public void TestGetAuthorisationHeader()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var permitHolder = ZGuid.NewZGuid();
				var header = Factory.New<CusAuthorisationHeader>();
				header.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
				header.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
				header.CPH_IsActive = true;
				header.CPH_Type = "ABC";
				header.CPH_OH_PermitHolder = permitHolder;
				header.CPH_StartDate = ZDate.Today;

				var cei = Factory.New<CusEntryInstructionForTest>();
				var usage = cei.CusAuthorizationUsages.AddNew();
				usage.AGC_Code = "ABC";
				usage.AGC_OH_Owner = ZGuid.NewZGuid();

				var loaded = cei.GetCusAuthorizationHeader(usage, ZDateTime.Today);
				CombineAssertions(() =>
				{
					AssertNull("Incorrect Owner", loaded);
					usage.AGC_OH_Owner = permitHolder;

					loaded = cei.GetCusAuthorizationHeader(usage, ZDateTime.Today);
					AssertEquals("Found", header.PK, loaded.PK);
				});

				var header2 = Factory.New<CusAuthorisationHeader>();
				header2.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
				header2.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
				header2.CPH_IsActive = true;
				header2.CPH_Type = "ABC";
				header2.CPH_OH_PermitHolder = permitHolder;
				header2.CPH_StartDate = ZDate.Today.AddDays(-1);

				loaded = cei.GetCusAuthorizationHeader(usage, ZDateTime.Today);
				AssertNull("Cant decide when more than 1", loaded);
			}
		}

		public void TestOwnersOfGoodsCollection()
		{
			AssertType<OwnerOfGoodsCollection>(instruction.OwnerOfGoodsCollection);
		}

		public void TestISequenceNumberHeader()
		{
			var sequenceHeader = (ISequenceNumberHeader)instruction;
			AssertEquals(0, sequenceHeader.Lines.Count());
			instruction.OwnerOfGoodsCollection.AddNew();
			AssertEquals(1, sequenceHeader.Lines.Count());
		}

		public void TestPlaceOfUseOrProcessingCollection()
		{
			AssertType<PlaceOfUseOrProcessingCollection>(instruction.PlaceOfUseOrProcessingCollection);
		}

		public void TestZG_PeriodForDischargeCaption()
		{
			CombineAssertions(() =>
			{
				var resData = DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(instruction.ZG_PeriodForDischarge));
				AssertEquals("Period (Month)", resData.Caption);
				AssertEquals(string.Empty, resData.FullDescription);
				resData = DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(instruction.ZG_PeriodForDischarge), new[] { JobDeclaration.CaptionKeyImportUCC6 });
				AssertEquals("Period (Month)", resData.Caption);
				AssertEquals("[Annex A 4/17] Dates, Times, Periods and Places > Period for Discharge > Period (Month)", resData.FullDescription);
			});
		}

		public void TestZG_PeriodForDischargeAutoExtensionCaption()
		{
			var resData = DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(instruction.ZG_PeriodForDischargeAutoExtension));
			AssertEquals("Automatic Extension?", resData.Caption);
			AssertEquals(string.Empty, resData.FullDescription);
			resData = DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(instruction.ZG_PeriodForDischargeAutoExtension), new[] { JobDeclaration.CaptionKeyImportUCC6 });
			AssertEquals("Automatic Extension?", resData.Caption);
			AssertEquals("[Annex A 4/17] Dates, Times, Periods and Places > Period for Discharge > Automatic Extension?", resData.FullDescription);
		}

		public void TestZG_BillOfDischargeDeadlineCaption()
		{
			var resData = DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(instruction.ZG_BillOfDischargeDeadline));
			AssertEquals("Deadline", resData.Caption);
			AssertEquals(string.Empty, resData.FullDescription);
			resData = DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(instruction.ZG_BillOfDischargeDeadline), new[] { JobDeclaration.CaptionKeyImportUCC6 });
			AssertEquals("Deadline", resData.Caption);
			AssertEquals("[Annex A 4/18] Dates, Times, Periods and Places > Bill of Discharge > Deadline", resData.FullDescription);
		}

		public void TestZG_BillOfDischargeIsNecessaryCaption()
		{
			var resData = DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(instruction.ZG_BillOfDischargeIsNecessary));
			AssertEquals("Necessary?", resData.Caption);
			AssertEquals(string.Empty, resData.FullDescription);
			resData = DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(instruction.ZG_BillOfDischargeIsNecessary), new[] { JobDeclaration.CaptionKeyImportUCC6 });
			AssertEquals("Necessary?", resData.Caption);
			AssertEquals("[Annex A 4/18] Dates, Times, Periods and Places > Bill of Discharge > Necessary?", resData.FullDescription);
		}

		public void TestBillOfDischargeDetailsCaption()
		{
			var resData = DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(instruction.BillOfDischargeDetails));
			AssertEquals("Details", resData.Caption);
			AssertEquals(string.Empty, resData.FullDescription);
			resData = DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(instruction.BillOfDischargeDetails), new[] { JobDeclaration.CaptionKeyImportUCC6 });
			AssertEquals("Details", resData.Caption);
			AssertEquals("[Annex A 4/18] Dates, Times, Periods and Places > Bill of Discharge > Details", resData.FullDescription);
		}

		public void TestPeriodForDischargeDetailsCaption()
		{
			var resData = DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(instruction.PeriodForDischargeDetails));
			AssertEquals("Details", resData.Caption);
			AssertEquals(string.Empty, resData.FullDescription);
			resData = DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(instruction.PeriodForDischargeDetails), new[] { JobDeclaration.CaptionKeyImportUCC6 });
			AssertEquals("Details", resData.Caption);
			AssertEquals("[Annex A 4/17] Dates, Times, Periods and Places > Period for Discharge > Details", resData.FullDescription);
		}

		public void TestBillOfDischargeDetails()
		{
			CombineAssertions(() =>
			{
				instruction.BillOfDischargeDetails = "I'm reporting some things that might be important";
				AssertEquals("I'm reporting some things that might be important", instruction.BillOfDischargeDetails);

				Factory.Save();

				var details = instruction.Notes;
				AssertEquals("Length", 1, details.DatabaseCount);

				var note = details.FindByDescription(PredefinedNoteTypes.Instance.BillOfDischargeDetails.Description)[0];
				AssertEquals("I'm reporting some things that might be important", note.ST_NoteText);

				var otherFactory = new BusinessObjectFactory();
				var entryInstructionCopy = otherFactory.Load<CusEntryInstruction>(instruction.PK);
				AssertEquals("I'm reporting some things that might be important", entryInstructionCopy.BillOfDischargeDetails);

				entryInstructionCopy.BillOfDischargeDetails = "Changed the text";
				otherFactory.Save();

				var thirdFactory = new BusinessObjectFactory();
				var entryInstructionCopy2 = thirdFactory.Load<CusEntryInstruction>(instruction.PK);
				AssertEquals("Changed the text", entryInstructionCopy2.BillOfDischargeDetails);
			});
		}

		public void TestPeriodForDischargeDetails()
		{
			CombineAssertions(() =>
			{
				instruction.PeriodForDischargeDetails = "I'm reporting some things that might be important";
				AssertEquals("I'm reporting some things that might be important", instruction.PeriodForDischargeDetails);

				Factory.Save();

				var details = instruction.Notes;
				AssertEquals("Length", 1, details.DatabaseCount);

				var note = details.FindByDescription(PredefinedNoteTypes.Instance.PeriodForDischargeDetails.Description)[0];
				AssertEquals("I'm reporting some things that might be important", note.ST_NoteText);

				var otherFactory = new BusinessObjectFactory();
				var entryInstructionCopy = otherFactory.Load<CusEntryInstruction>(instruction.PK);
				AssertEquals("I'm reporting some things that might be important", entryInstructionCopy.PeriodForDischargeDetails);

				entryInstructionCopy.PeriodForDischargeDetails = "Changed the text";
				otherFactory.Save();

				var thirdFactory = new BusinessObjectFactory();
				var entryInstructionCopy2 = thirdFactory.Load<CusEntryInstruction>(instruction.PK);
				AssertEquals("Changed the text", entryInstructionCopy2.PeriodForDischargeDetails);
			});
		}

		public void TestDetailsOfPlannedActivitiesCaption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(instruction.DetailsOfPlannedActivities));
			AssertEquals("Planned Activities", resourceStringData.Caption);
			AssertEquals("[Annex A 7/5] Activities and Procedures > Details of Planned Activities", resourceStringData.FullDescription);
		}

		public void TestDetailsOfPlannedActivities()
		{
			CombineAssertions(() =>
			{
				instruction.DetailsOfPlannedActivities = "I'm reporting some things that might be important";
				AssertEquals("I'm reporting some things that might be important", instruction.DetailsOfPlannedActivities);

				Factory.Save();

				var details = instruction.Notes;
				AssertEquals("Length", 1, details.DatabaseCount);

				var note = details.FindByDescription(PredefinedNoteTypes.Instance.DetailsOfPlannedActivities.Description)[0];
				AssertEquals("I'm reporting some things that might be important", note.ST_NoteText);

				var otherFactory = new BusinessObjectFactory();
				var entryInstructionCopy = otherFactory.Load<CusEntryInstruction>(instruction.PK);
				AssertEquals("I'm reporting some things that might be important", entryInstructionCopy.DetailsOfPlannedActivities);

				entryInstructionCopy.DetailsOfPlannedActivities = "Changed the text";
				otherFactory.Save();

				var thirdFactory = new BusinessObjectFactory();
				var entryInstructionCopy2 = thirdFactory.Load<CusEntryInstruction>(instruction.PK);
				AssertEquals("Changed the text", entryInstructionCopy2.DetailsOfPlannedActivities);
			});
		}

		public void TestZG_Article86_3_UCCCaption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(instruction.ZG_Article86_3_UCC));
			AssertEquals("Calculate the Amount of Import Duty in Accordance with Article 86(3) of the Code", resourceStringData.Caption);
			AssertEquals("[Annex A 8/13] Others > Calculate the Amount of Import Duty in Accordance with Article 86(3) of the Code", resourceStringData.FullDescription);
		}

		public void TestAdditionalInformationCaption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(instruction.AdditionalInformation));
			AssertEquals("Additional Information", resourceStringData.Caption);
			AssertEquals("Additional Info.", resourceStringData.MediumCaption);
			AssertEquals("Add. Info.", resourceStringData.ShortCaption);
			AssertEquals("[Annex A 8/5] Others > Additional Information", resourceStringData.FullDescription);
		}

		public void TestAdditionalInformation()
		{
			instruction.AdditionalInformation = "I'm reporting some things that might be important";
			AssertEquals("I'm reporting some things that might be important", instruction.AdditionalInformation);

			Factory.Save();

			var details = instruction.Notes;
			AssertEquals("Length", 1, details.DatabaseCount);

			var note = details.FindByDescription(PredefinedNoteTypes.Instance.AdditionalInformation.Description)[0];
			AssertEquals("I'm reporting some things that might be important", note.ST_NoteText);

			var otherFactory = new BusinessObjectFactory();
			var entryInstructionCopy = otherFactory.Load<CusEntryInstruction>(instruction.PK);
			AssertEquals("I'm reporting some things that might be important", entryInstructionCopy.AdditionalInformation);

			entryInstructionCopy.AdditionalInformation = "Changed the text";
			otherFactory.Save();

			var thirdFactory = new BusinessObjectFactory();
			var entryInstructionCopy2 = thirdFactory.Load<CusEntryInstruction>(instruction.PK);
			AssertEquals("Changed the text", entryInstructionCopy2.AdditionalInformation);
		}

		public void TestIsCentralisedClearance()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("No authorisation", false, entryInstruction.IsCentralisedClearance);

				var authorisation = entryInstruction.CusAuthorizationUsages.AddNew();
				authorisation.AGC_Code = "AAA";
				AssertEquals("No CCL authorisation", false, entryInstruction.IsCentralisedClearance);

				authorisation.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CentralizedClearance;
				AssertEquals("CCL authorisation", true, entryInstruction.IsCentralisedClearance);
			});
		}

		public void TestZG_RateOfYieldCaption()
		{
			var resData = DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(instruction.ZG_RateOfYield), new[] { JobDeclaration.CaptionKeyImportUCC6 });
			AssertEquals("Rate of Yield", resData.Caption);
			AssertEquals("[Annex 5/5] Identification of goods > Rate of Yield", resData.FullDescription);
		}

		public void TestZG_ProcessedProductsCommodityCodeCaption()
		{
			var resData = DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(instruction.ZG_ProcessedProductsCommodityCode), new[] { JobDeclaration.CaptionKeyImportUCC6 });
			AssertEquals("Commodity Code", resData.Caption);
			AssertEquals("[Annex 5/5] Identification of goods > Processed Products > Commodity Code", resData.FullDescription);
		}

		public void TestZG_IdOfGoodCodeCaption()
		{
			var resData = DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(instruction.ZG_RateOfYield), new[] { JobDeclaration.CaptionKeyImportUCC6 });
			AssertEquals("Rate of Yield", resData.Caption);
			AssertEquals("[Annex 5/5] Identification of goods > Rate of Yield", resData.FullDescription);
		}

		public void TestProcessedProductDescriptionCaption()
		{
			var resData = DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(instruction.ProcessedProductDescription), new[] { JobDeclaration.CaptionKeyImportUCC6 });
			AssertEquals("Goods Description", resData.Caption);
			AssertEquals("[Annex 5/5] Identification of goods > Processed Products > Goods Description", resData.FullDescription);
		}

		public void TestIdentificationofGoodsDetailsCaption()
		{
			var resData = DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(instruction.IdentificationofGoodsDetails), new[] { JobDeclaration.CaptionKeyImportUCC6 });
			AssertEquals("Details", resData.Caption);
			AssertEquals("[Annex 5/8] Identification of goods > Identification of Goods > Details", resData.FullDescription);
		}

		public void TestZG_ProcessingProcedureCodeCaption()
		{
			var resData = DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(instruction.ZG_ProcessingProcedureCode), new[] { JobDeclaration.CaptionKeyImportUCC6 });
			AssertEquals("Processing Procedure", resData.ShortCaption);
			AssertEquals("Processing Procedure Code", resData.Caption);
			AssertEquals("[Annex A 6/2] Conditions and Terms > Economic Conditions > Processing Procedure Code", resData.FullDescription);
		}

		public void TestProcessingProcedureDetailsCaption()
		{
			var resData = DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(instruction.ProcessingProcedureDetails), new[] { JobDeclaration.CaptionKeyImportUCC6 });
			AssertEquals("Details", resData.ShortCaption);
			AssertEquals("Processing Procedure Details", resData.Caption);
			AssertEquals("[Annex A 6/2] Conditions and Terms > Economic Conditions > Details", resData.FullDescription);
		}

		public void TestValidateGoodsLocationDescription()
		{
			var entryInstructionAsProvider = Factory.New<CusEntryInstruction>() as ICusGoodsLocationProvider;
			CombineAssertions(() =>
			{
				entryInstructionAsProvider.ValidateGoodsLocationDescription();
				AssertNoNotifications("Default data", entryInstructionAsProvider.GoodsLocationDescriptionInfo);

				entryInstructionAsProvider.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
				entryInstructionAsProvider.ValidateGoodsLocationDescription();
				AssertHasMessageError("Goods Location empty Unlocode and contains notifications", entryInstructionAsProvider.GoodsLocationDescriptionInfo, "There are errors within the 'Location of Goods', please click on 'More..' to view the error information.");

				entryInstructionAsProvider.GoodsLocation.Unlocode = "12345";
				entryInstructionAsProvider.ValidateGoodsLocationDescription();
				AssertHasNotifications("Goods Location invalid Unlocode should contain notifications", entryInstructionAsProvider.GoodsLocationDescriptionInfo);

				entryInstructionAsProvider.GoodsLocation.Unlocode = "BSASD";
				entryInstructionAsProvider.ValidateGoodsLocationDescription();
				AssertNoNotifications("Goods Location valid Unlocode should not contain notifications", entryInstructionAsProvider.GoodsLocationDescriptionInfo);
			});
		}
	}

	class CusEntryInstructionForTest : CusEntryInstruction
	{
		public CusEntryInstructionForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString OrgCusCodeTypeForWarehouseExposed => OrgCusCodeTypeForWarehouse;
	}
}
