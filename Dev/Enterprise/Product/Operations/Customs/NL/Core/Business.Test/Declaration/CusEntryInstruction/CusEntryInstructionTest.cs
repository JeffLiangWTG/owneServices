using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(CusEntryInstruction))]
sealed class CusEntryInstructionTest : Customs.Business.Testing.CusEntryInstructionAbstractTest
{
	public void TestCusAuthorizationUsages()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		AssertType<EU.Business.CusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction>>(instruction.CusAuthorizationUsages);
	}

	public void TestSupportingDocuments()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(cei.SupportingDocuments);
			AssertType<SupportingDocumentCollection>(cei.SupportingDocuments);
		});
	}

	public void TestPreviousDocuments()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(cei.PreviousDocuments);
			AssertType<PreviousDocumentCollection>(cei.PreviousDocuments);
		});
	}

	public void TestValidation()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		AssertType<CusEntryInstructionValidation>(instruction.Validation);
	}

	public void TestAllEntryLineFees()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();

		AssertEquals("There should be no results when there is no entry header ", 0, instruction.AllEntryLineFees().Cast<CusEntryLineFee>().Count());

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = instruction.PK;
		var entryLine = entryHeader.AllEntryLines.AddNew();
		entryLine.Fees.AddNew();

		AssertEquals("There should be 1 result when 1 fees is added ", 1, instruction.AllEntryLineFees().Cast<CusEntryLineFee>().Count());

		entryLine.Fees.AddNew();

		AssertEquals("There should be 2 results when 2 fees are added ", 2, instruction.AllEntryLineFees().Cast<CusEntryLineFee>().Count());
	}

	public void TestAllAuthorizationUsages()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();

		AssertEquals("There should be no results when there are no authorizations ", 0, instruction.AllAuthorizationUsages().Cast<CusAuthorizationUsage>().Count());

		instruction.CusAuthorizationUsages.AddNew();

		AssertEquals("There should be 1 result when an authorization is added directly to the instruction.", 1, instruction.AllAuthorizationUsages().Cast<CusAuthorizationUsage>().Count());

		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		invoiceLine.CusAuthorizationUsages.AddNew();

		AssertEquals("There should be 2 results when also to the linked invoice line there is an authorization added ", 2, instruction.AllAuthorizationUsages().Cast<CusAuthorizationUsage>().Count());

		instruction.CusAuthorizationUsages.AddNew();
		invoiceLine.CusAuthorizationUsages.AddNew();

		AssertEquals("When both the entry instruction and it's linked invoice have 2 authorizations linked, the total amount for this entry instruction should be 4.", 4, instruction.AllAuthorizationUsages().Cast<CusAuthorizationUsage>().Count());
	}

	public void TestHasProcedureStartingWithAny()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		invoiceLine.JI_Procedure = "000000";

		ZString[] checkCodes = new ZString[] { NLConstants.ProcedureCodes._51, NLConstants.ProcedureCodes._53, NLConstants.ProcedureCodes._71 };

		AssertEquals("There should not be a procedure starting with 51, 53 or 71.", false, instruction.HasProcedureStartingWithAny(checkCodes));

		var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = instruction.PK;
		invoiceLine2.JI_Procedure = "510000";

		AssertEquals("There should be a procedure starting with 51.", true, instruction.HasProcedureStartingWithAny(checkCodes));
	}

	public void TestLookups()
	{
		AssertType<CusEntryInstructionLookups>(cei.Lookups);
	}

	public void TestJobDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		CombineAssertions(() =>
		{
			AssertType<JobDeclaration>(instruction.JobDeclaration);
			AssertNotNull(instruction.JobDeclaration);
		});
	}

	public void TestZG_IsHighValueOvrd()
	{
		AssertEquals("[14 07 000 000] Valuation Indicators formerly known as D.V.1 relationship information", DataBoundResourceStrings.GetDataForProperty(cei.ZG_IsHighValueOvrdInfo).FullDescription);
		AssertEquals("Valuation Indicators?", DataBoundResourceStrings.GetDataForProperty(cei.ZG_IsHighValueOvrdInfo).Caption);
		AssertEquals("V.I.?", DataBoundResourceStrings.GetDataForProperty(cei.ZG_IsHighValueOvrdInfo).ShortCaption);

		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		instruction.CEI_Style = "H1";
		instruction.ZG_IsHighValueOvrd = true;
		AssertEquals(true, instruction.ZG_IsHighValueOvrd);
		instruction.CEI_Style = "H2";
		AssertEquals(false, instruction.ZG_IsHighValueOvrd);
	}

	public void TestCEI_Procedure_Caption()
	{
		AssertEquals("[37] CPC", DataBoundResourceStrings.GetDataForProperty(cei.CEI_ProcedureInfo).Caption);
	}

	public void TestGoodsLocationDescription_Caption()
	{
		AssertCaptionAndFullDescription(GetInstruction(JobMessageTypeList.Codes.Import).GoodsLocationDescriptionInfo, "Location of Goods", "[UCC 5/23] Location of Goods");
	}

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

	public void TestAddInfo()
	{
		var instruction = GetInstruction(JobMessageTypeList.Codes.Import);
		var addInfoGetter = typeof(CusEntryInstruction).GetMethod("GetNewAddInfo", BindingFlags.NonPublic | BindingFlags.Instance);
		AssertType<AddInfoCusEntryInstruction>(addInfoGetter.Invoke(instruction, null));
	}

	public void TestAddInfoLookups()
	{
		var instruction = GetInstruction(JobMessageTypeList.Codes.Import);
		AssertType<AddInfoCusEntryInstructionLookups>(instruction.AddInfoLookups);
	}

	public void TestAddInfoValidation()
	{
		var instruction = GetInstruction(JobMessageTypeList.Codes.Import);
		AssertType<AddInfoCusEntryInstructionValidation>(instruction.AddInfoValidation);
	}

	public void TestAdditionalInfosType()
	{
		AssertType<AdditionalInfoCollection>(GetInstruction(JobMessageTypeList.Codes.Import).AdditionalInfos);
	}

	public void TestFiscalReferencesAreRemovedAndReadOnly()
	{
		CombineAssertions(() =>
		{
			AssertFiscalReferencesAreRemoved("H1", 2, false);
			AssertFiscalReferencesAreRemoved("H2", 0, true);
			AssertFiscalReferencesAreRemoved("H3", 2, false);
			AssertFiscalReferencesAreRemoved("H4", 2, false);
			AssertFiscalReferencesAreRemoved("H5", 0, true);
			AssertFiscalReferencesAreRemoved("H6", 2, false);
			AssertFiscalReferencesAreRemoved("I1", 2, false);
			AssertFiscalReferencesAreRemoved("I2", 2, false);
		});
	}

	public void AssertFiscalReferencesAreRemoved(string style, int expectedResult, bool expectedReadOnly)
	{
		var instruction = GetInstruction(JobMessageTypeList.Codes.Import);
		instruction.CEI_Style = "H1";
		instruction.FiscalReferences.AddNew();
		instruction.FiscalReferences.AddNew();

		instruction.CEI_Style = style;
		AssertEquals("Number of fiscal references for Entry Instruction " + style, expectedResult, instruction.FiscalReferences.Count);
		AssertEquals("Fiscal references collection ReadOnly for Entry Instruction " + style, expectedReadOnly, instruction.FiscalReferences.ReadOnly);
	}

	public void TestGetCusSupportingInfoTypesCore() => CombineAssertions(() =>
	{
		var instruction = GetInstruction(JobMessageTypeList.Codes.Import);
		var supportingInfoTypeSupporter = (Integration.Customs.ICusSupportingInfoTypeSupporter)instruction;

		AssertEquals("AdditionalInfo", typeof(AdditionalInfo), supportingInfoTypeSupporter.GetCusSupportingInfoTypes()[Customs.Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
		AssertEquals("SupportingDocument", typeof(SupportingDocument), supportingInfoTypeSupporter.GetCusSupportingInfoTypes()[Customs.Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
		AssertEquals("PreviousDocument", typeof(PreviousDocument), supportingInfoTypeSupporter.GetCusSupportingInfoTypes()[Customs.Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
	});

	CusEntryInstruction GetInstruction(string messageType)
	{
		var declaration = Factory.New<JobDeclaration>();
		if (!string.IsNullOrEmpty(messageType))
		{
			declaration.JE_MessageType = messageType;
		}
		return declaration.CustomsEntryInstructions.AddNew();
	}

	public void TestHasEmptyTransactionNatureForMessage()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();

		AssertEquals("There should be a Declaration code assigned with it.", false, instruction.HasEmptyTransactionNatureForMessage);

		AssertEmptyTransactionNatureForMessage(instruction);
	}

	public void TestHasNonEmptyTransactionNatureForMessage()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();

		instruction.ZG_TransNature = ZString.Empty;
		instruction.CEI_Style = "B4";
		AssertEquals("No transaction nature and no Declaration code assigned yet", false, instruction.HasNonEmptyTransactionNatureForMessage);

		AssertNonEmptyTransactionNatureForMessage(instruction);
	}

	public void TestCEI_SubStyle_Caption()
	{
		AssertCaptionAndFullDescription(GetInstruction(JobMessageTypeList.Codes.Import).CEI_SubStyleInfo, "Sub Style", "[UCC 1/2] Sub Style");
	}

	void AssertEmptyTransactionNatureForMessage(CusEntryInstruction instruction)
	{
		instruction.CEI_Style = "B4";
		AssertEquals("The Declaration code does not belong to the list.", false, instruction.HasEmptyTransactionNatureForMessage);

		var validDeclarationTypesForMessage = new ZString[] { DeclarationTypeList.Codes.B1, DeclarationTypeList.Codes.B2, DeclarationTypeList.Codes.C1, DeclarationTypeList.Codes.H1, DeclarationTypeList.Codes.H3, DeclarationTypeList.Codes.H4, DeclarationTypeList.Codes.H5, DeclarationTypeList.Codes.I1 };
		foreach(var style in validDeclarationTypesForMessage)
		{
			instruction.ZG_TransNature = ZString.Empty;
			instruction.CEI_Style = style;
			AssertEquals("The Declaration code is one of the valid types and Transaction Nature is empty.", true, instruction.HasEmptyTransactionNatureForMessage);

			instruction.ZG_TransNature = "72";
			AssertEquals("Transaction Nature not empty for the given declaration code of valid type.", false, instruction.HasEmptyTransactionNatureForMessage);
		}
	}

	void AssertNonEmptyTransactionNatureForMessage(CusEntryInstruction instruction)
	{
		instruction.CEI_Style = "B4";
		AssertEquals("The Declaration code is not one of the valid types.", false, instruction.HasNonEmptyTransactionNatureForMessage);

		var validDeclarationTypesForMessage = new ZString[] { DeclarationTypeList.Codes.B1, DeclarationTypeList.Codes.B2, DeclarationTypeList.Codes.C1, DeclarationTypeList.Codes.H1, DeclarationTypeList.Codes.H3, DeclarationTypeList.Codes.H4, DeclarationTypeList.Codes.H5, DeclarationTypeList.Codes.I1 };
		foreach (var style in validDeclarationTypesForMessage)
		{
			instruction.CEI_Style = ZString.Empty;
			instruction.ZG_TransNature = "72";
			AssertEquals("Transaction Nature assigned but Declaration code is empty", false, instruction.HasNonEmptyTransactionNatureForMessage);

			instruction.CEI_Style = style;
			AssertEquals("Transaction Nature assigned and the Declaration code is one of the valid types.", true, instruction.HasNonEmptyTransactionNatureForMessage);
		}
	}

	void AssertCaptionAndFullDescription(ZPropertyInfo propertyInfo, string expectedCaption, string expectedFullDescription, string messageType = "")
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(propertyInfo, null, [messageType == "IMP" ? JobDeclaration.CaptionKeyImportUCC6 : messageType == "EXP" ? JobDeclaration.CaptionKeyExportUCC6 : null]);

		CombineAssertions(() =>
		{
			AssertEquals($"{propertyInfo.Name} {messageType} Caption", expectedCaption, resourceStringData.Caption);
			AssertEquals($"{propertyInfo.Name} {messageType} FullDescription", expectedFullDescription, resourceStringData.FullDescription);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => GetInstruction(string.Empty);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetInstruction(string.Empty);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetInstruction(string.Empty);

	public override BusinessObject GetNewBusinessObjectSafeSaving() => GetInstruction(string.Empty);

	protected override void SetUp()
	{
		base.SetUp();
		cei = Factory.NewWithValidTestData<CusEntryInstruction>();
	}
	CusEntryInstruction cei;
}
