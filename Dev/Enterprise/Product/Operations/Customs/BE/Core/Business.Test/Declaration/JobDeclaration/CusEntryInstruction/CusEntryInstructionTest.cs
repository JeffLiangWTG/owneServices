using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BE;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

[TestedType(typeof(CusEntryInstruction))]
sealed class CusEntryInstructionTest : Customs.Business.Testing.CusEntryInstructionAbstractTest
{
	public void TestGetCusSupportingInfoTypes_AdditionalInfo()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		AssertEquals(typeof(AdditionalInfo), ((Integration.Customs.ICusSupportingInfoTypeSupporter)entryInstruction).GetCusSupportingInfoTypes()[CusSupportingInfoTypeList.Codes.AdditionalInfo]);
	}

	public void TestPropertyAttributes()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		AssertEquals("CEI_OA_Warehouse2: Caption", "To Warehouse", entryInstruction.CEI_OA_Warehouse2Info.GetAttribute<ResourceStringDataAttribute>().Caption);
	}

	public void TestIsImportDeclarationType()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();

		CombineAssertions(() =>
		{
			AssertIsImportDeclarationType("H1", ImportCusEntryInstructionsDeclarationTypeList.Codes.FreeCirculation, true);
			AssertIsImportDeclarationType("H2", ImportCusEntryInstructionsDeclarationTypeList.Codes.CustomsWarehousing, true);
			AssertIsImportDeclarationType("H3", ImportCusEntryInstructionsDeclarationTypeList.Codes.TemporaryAdmission, true);
			AssertIsImportDeclarationType("H4", ImportCusEntryInstructionsDeclarationTypeList.Codes.InwardProcessing, true);
			AssertIsImportDeclarationType("H5", ImportCusEntryInstructionsDeclarationTypeList.Codes.IntroductionOfGoods, true);
			AssertIsImportDeclarationType("I1", ImportCusEntryInstructionsDeclarationTypeList.Codes.ImportSimplified, true);
			AssertIsImportDeclarationType("Empty", string.Empty, false);
			AssertIsImportDeclarationType("Invalid code", "~!", false);
		});

		void AssertIsImportDeclarationType(string message, string style, bool expected)
		{
			entryInstruction.CEI_Style = style;
			AssertEquals(message, expected, entryInstruction.IsImportDeclarationType());
		}
	}

	public void TestGuaranteeCreated()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();

		AssertEquals("Guarantee shouldn't exist yet", 0, entryInstruction.Guarantees.Count);

		_ = entryInstruction.GuaranteeOffice;
		AssertEquals("Guarantee should exist now", 1, entryInstruction.Guarantees.Count);

		var guarantee = entryInstruction.Guarantees[0];
		guarantee.PW_BondType = "type";
		guarantee.PW_BondNumber = "number";
		guarantee.PW_HolderIdentification = "identification";
		guarantee.PW_BondEffectiveDate = ZDateTime.Now;

		AssertEquals(guarantee, entryInstruction.Guarantee);
		AssertEquals(guarantee.PW_BondType, entryInstruction.GuaranteeType);
		AssertEquals(guarantee.PW_BondNumber, entryInstruction.GuaranteeReferenceNumber);
		AssertEquals(guarantee.PW_HolderIdentification, entryInstruction.GuaranteeOffice);
		AssertEquals(guarantee.PW_BondEffectiveDate, entryInstruction.GuaranteeDate);

		entryInstruction.GuaranteeType = "type2";
		entryInstruction.GuaranteeReferenceNumber = "number2";
		entryInstruction.GuaranteeOffice = "identification2";
		entryInstruction.GuaranteeDate = ZDateTime.Now;

		AssertEquals(guarantee, entryInstruction.Guarantee);
		AssertEquals(guarantee.PW_BondType, entryInstruction.GuaranteeType);
		AssertEquals(guarantee.PW_BondNumber, entryInstruction.GuaranteeReferenceNumber);
		AssertEquals(guarantee.PW_HolderIdentification, entryInstruction.GuaranteeOffice);
		AssertEquals(guarantee.PW_BondEffectiveDate, entryInstruction.GuaranteeDate);

		AssertEquals("Only 1 Guarantee should exist", 1, entryInstruction.Guarantees.Count);
	}

	public void TestJobDeclaration()
	{
		AssertType<JobDeclaration>(GetInstruction(string.Empty).JobDeclaration);
	}

	public void TestZG_ManualDeclarationProperty()
	{
		var entryInstruction = GetInstruction(string.Empty);

		entryInstruction.ZG_ManualDeclaration = true;
		AssertEquals(true, entryInstruction.ZG_ManualDeclaration);
		Factory.Save();
		AssertContains("ManualDeclaration", entryInstruction.CEI_AddInfo);

		entryInstruction.ZG_ManualDeclaration = false;
		AssertEquals(false, entryInstruction.ZG_ManualDeclaration);
		Factory.Save();
		AssertNotContains("ManualDeclaration", entryInstruction.CEI_AddInfo);
	}

	public void TestZG_ManualDeclaration_ReadOnlyProperty()
	{
		var entryInstruction = Factory.NewMoq<CusEntryInstruction>();
		var entryHeader = Factory.NewMoq<CusEntryHeader>();
		entryInstruction.Protected().Setup<CusEntryHeader>("EntryHeaderCore").Returns(entryHeader.Object);

		entryHeader.Setup(x => x.IsWaitingForResponse).Returns(false);
		entryHeader.Setup(x => x.HasBeenLodgedAtCustoms).Returns(false);
		AssertEquals(false, entryInstruction.Object.ZG_ManualDeclaration_ReadOnly);

		entryHeader.Setup(x => x.IsWaitingForResponse).Returns(true);
		AssertEquals(true, entryInstruction.Object.ZG_ManualDeclaration_ReadOnly);
		entryInstruction.VerifyAll();
	}

	public void TestIsExport()
	{
		AssertEquals(true, GetInstruction(JobMessageTypeList.Codes.Export).IsExport);
	}

	public void TestIsExport_WithoutDeclaration()
	{
		AssertEquals(false, Factory.New<CusEntryInstruction>().IsExport);
	}

	public void TestIsImport()
	{
		AssertEquals(true, GetInstruction(JobMessageTypeList.Codes.Import).IsImport);
	}

	public void TestIsImport_WithoutDeclaration()
	{
		AssertEquals(false, Factory.New<CusEntryInstruction>().IsImport);
	}

	public void TestReExport()
	{
		AssertEquals(true, GetInstruction(BEJobMessageTypeList.Codes.ReExport).IsReExport);
	}

	public void TestIsReExport_WithoutDeclaration()
	{
		AssertEquals(false, Factory.New<CusEntryInstruction>().IsReExport);
	}

	public void TestIsExitSummary()
	{
		AssertEquals(true, GetInstruction(BEJobMessageTypeList.Codes.ExitSummary).IsExitSummary);
	}

	public void TestIsExitSummary_WithoutDeclaration()
	{
		AssertEquals(false, Factory.New<CusEntryInstruction>().IsExitSummary);
	}

	public void TestExportLookups()
	{
		AssertType<ExportCusEntryInstructionLookups>(GetInstruction(JobMessageTypeList.Codes.Export).Lookups);
	}

	public void TestImportLookups()
	{
		AssertType<ImportCusEntryInstructionLookups>(GetInstruction(JobMessageTypeList.Codes.Import).Lookups);
	}

	public void TestMiscellaneousLookups()
	{
		AssertType<CusEntryInstructionLookups>(GetInstruction(JobMessageTypeList.Codes.MiscellaneousCustoms).Lookups);
	}

	public void TestExportValidation()
	{
		AssertType<ExportCusEntryInstructionValidation>(GetInstruction(JobMessageTypeList.Codes.Export).Validation);
	}

	public void TestImportValidation()
	{
		AssertType<ImportCusEntryInstructionValidation>(GetInstruction(JobMessageTypeList.Codes.Import).Validation);
	}

	public void TestMiscellaneousValidation()
	{
		AssertType<EU.Business.Declaration.CusEntryInstructionValidation>(GetInstruction(JobMessageTypeList.Codes.MiscellaneousCustoms).Validation);
	}

	public void TestDefaultingOfFiscalReferences()
	{
		var entryInstruction = GetInstruction(EUJobMessageTypeList.Codes.Import);
		var importer = GetImporter();
		entryInstruction.JobDeclaration.JE_OH_Importer = importer.PK;
		var euAddInfo = EUOrgImpAddInfo.Get(importer, Core.Constants.CountryCodes.Belgium);
		euAddInfo.ZO_UseFr3FiscalRepresentation = true;
		CombineAssertions(() =>
		{
			entryInstruction.CEI_Style = ImportCusEntryInstructionsDeclarationTypeList.Codes.FreeCirculation;
			AssertEquals("The BE entry instruction should contain a FR3 type of fiscal reference for type H1.", true, entryInstruction.FiscalReferences.Cast<CusFiscalReference>().Any(f => f.CFR_Code == FiscalReferenceCodeList.Codes.FR3_TaxRepresentative));
			entryInstruction.CEI_Style = ImportCusEntryInstructionsDeclarationTypeList.Codes.ImportSimplified;
			AssertEquals("The BE entry instruction should contain a FR3 type of fiscal reference for type I1.", true, entryInstruction.FiscalReferences.Cast<CusFiscalReference>().Any(f => f.CFR_Code == FiscalReferenceCodeList.Codes.FR3_TaxRepresentative));
			entryInstruction.CEI_Style = ImportCusEntryInstructionsDeclarationTypeList.Codes.ProbablyNoControlRequired;
			AssertEquals("The BE entry instruction should contain a FR3 type of fiscal reference for type H6", true, entryInstruction.FiscalReferences.Cast<CusFiscalReference>().Any(f => f.CFR_Code == FiscalReferenceCodeList.Codes.FR3_TaxRepresentative));
			entryInstruction.CEI_Style = ImportCusEntryInstructionsDeclarationTypeList.Codes.IntroductionOfGoods;
			AssertEquals("The BE entry instruction should not contain a FR3 type of fiscal reference.", false, entryInstruction.FiscalReferences.Cast<CusFiscalReference>().Any(f => f.CFR_Code == FiscalReferenceCodeList.Codes.FR3_TaxRepresentative));
		});
	}

	public void TestGoodsLocationDescription_Caption() => CombineAssertions(() =>
	{
		var cusEntryInstruction = GetInstruction(JobMessageTypeList.Codes.Import);
		AssertEquals("Location of Goods", DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(cusEntryInstruction.GoodsLocationDescription)).Caption);
		AssertEquals("[UCC 5/23] Location of Goods", DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(cusEntryInstruction.GoodsLocationDescription)).FullDescription);
	});

	public void TestGoodsLocationDescription()
	{
		var instruction = GetInstruction(JobMessageTypeList.Codes.Import);
		CombineAssertions(() =>
		{
			AssertEquals("GoodsLocationDescription empty when there's no GoodsLocation", ZString.Empty, instruction.GoodsLocationDescription);
			AssertNull("GoodsLocation doesn't exist", Customs.Business.CusGoodsLocation.Load<CusGoodsLocation>(instruction, CusGoodsLocationUseList.Codes.Departure));

			var goodsLocation = instruction.GoodsLocation;
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			AssertEquals("GoodsLocationDescription when there's GoodsLocation", CusGoodsLocationQualifierList.Codes.UnLocode, instruction.GoodsLocationDescription);
		});
	}

	public void TestGoodsLocation()
	{
		var instruction = GetInstruction(JobMessageTypeList.Codes.Import);
		var goodsLocation = instruction.GoodsLocation;
		CombineAssertions(() =>
		{
			AssertEquals("CGL_ParentID", instruction.PK, goodsLocation.CGL_ParentID);
			AssertEquals("CGL_ParentTableCode", CusEntryInstructionSchema.Constants.Prefix, goodsLocation.CGL_ParentTableCode);
			AssertEquals("CGL_LocationUse", CusGoodsLocationUseList.Codes.EntryInstruction, goodsLocation.CGL_LocationUse);
			AssertSame("Cached", goodsLocation, instruction.GoodsLocation);
			AssertEquals("IsRegisteredEditableChildObject", true, instruction.IsRegisteredEditableChildObject(goodsLocation));
		});
	}

	public void TestSupportingDocuments()
	{
		var instruction = GetInstruction(string.Empty);
		AssertType<SupportingDocumentCollection>(instruction.SupportingDocuments);
	}

	public void TestGetCusSupportingInfoTypes_SUP()
	{
		var instruction = GetInstruction(string.Empty);
		AssertEquals(typeof(SupportingDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)instruction).GetCusSupportingInfoTypes()[CusSupportingInfoTypeList.Codes.SupportingDocument]);
	}

	public void TestPreviousDocuments()
	{
		var instruction = GetInstruction(string.Empty);
		AssertType<PreviousDocumentCollection>(instruction.PreviousDocuments);
	}

	public void TestCEI_SubStyle_Caption()
	{
		var cusEntryInstruction = GetInstruction(JobMessageTypeList.Codes.Import);
		AssertEquals("[UCC 1/2] Sub Style", DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(cusEntryInstruction.CEI_SubStyle)).Caption);
	}

	protected override BusinessObject GetNewBusinessObject() => GetInstruction(JobMessageTypeList.Codes.Import);

	OrgHeader GetImporter()
	{
		var result = Factory.New<OrgHeader>();
		result.OH_Code = "IMPORTER";
		var cpvParty = Factory.New<OrgHeader>();
		cpvParty.OH_Code = "CPV PARTY";
		cpvParty.MainAddress.Address1 = "CPV ADDRESS";
		cpvParty.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "11111111111", Core.Constants.CountryCodes.Belgium);
		var relation = result.AllRelatedParties.AddNew();
		relation.PR_OH_RelatedParty = cpvParty.PK;
		relation.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
		relation.PR_PartyType = RelatedPartyTypeList.Codes.CustomsPostponedVatAccounting;
		return result;
	}

	CusEntryInstruction GetInstruction(string messageType)
	{
		var declaration = Factory.New<JobDeclaration>();
		if (!string.IsNullOrEmpty(messageType))
		{
			declaration.JE_MessageType = messageType;
		}
		return declaration.CustomsEntryInstructions.AddNew();
	}
}
