using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.AE.Business.AEConstants;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(CusEntryInstruction))]
sealed class CusEntryInstructionTest : CusEntryInstructionAbstractTest
{
	[ExpectNoExceptions]
	public void TestAllAddInfoColumnsAreInModelView() => ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(Instruction, "AECusEntryInstruction");

	public void TestCEI_Style()
	{
		var info = DataBoundResourceStrings.GetDataForProperty(Instruction.CEI_StyleInfo);
		AssertEquals("CPC", info.Caption);
	}

	public void TestCEI_DateForDuty()
	{
		var info = DataBoundResourceStrings.GetDataForProperty(Instruction.CEI_DateForDutyInfo);
		AssertEquals("Assessment Date", info.Caption);
	}

	public void TestCEI_DeclarationPurpose() => CombineAssertions(() =>
	{
		var info = DataBoundResourceStrings.GetDataForProperty(Instruction.CEI_DeclarationPurposeInfo);
		AssertEquals("Declaration Purpose", info.Caption);
		AssertEquals("Dec. Purpose", info.MediumCaption);
		AssertEquals("Dec. Purp.", info.ShortCaption);
	});

	public void TestCEI_DeclarationPurposeDetails() => CombineAssertions(() =>
	{
		var info = DataBoundResourceStrings.GetDataForProperty(Instruction.CEI_DeclarationPurposeDetailsInfo);
		AssertEquals("Declaration Purpose Details", info.Caption);
		AssertEquals("Dec. Purp. Details", info.MediumCaption);
		AssertEquals("Purp. Det.", info.ShortCaption);

		Instruction.CEI_DeclarationPurpose = RefCusCodeList.Codes.DeclarationPurpose.Others;
		Assert("Not read Only when CEI_DeclarationPurpose == Others", !Instruction.CEI_DeclarationPurposeDetailsInfo.ReadOnly);
		Instruction.CEI_DeclarationPurposeDetails = "Some details";

		Instruction.CEI_DeclarationPurpose = "1";
		Assert("Read only when CEI_DeclarationPurpose != Others", Instruction.CEI_DeclarationPurposeDetailsInfo.ReadOnly);
		AssertEquals("Clear CEI_DeclarationPurposeDetails when CEI_DeclarationPurpose changes", ZString.Empty, Instruction.CEI_DeclarationPurposeDetails);
	});

	public void TestCEI_TradeType()
	{
		var info = DataBoundResourceStrings.GetDataForProperty(Instruction.CEI_TradeTypeInfo);
		AssertEquals("Trade Type", info.Caption);
	}

	public void TestGetCusSupportingInfoTypes()
	{
		var actualTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)Instruction).GetCusSupportingInfoTypes();
		AssertEquals("DAV - DocumentAvailability", typeof(DocumentAvailability), actualTypes[AEConstants.CusSupportingInfoTypes.Codes.DocumentAvailability]);
	}

	public void TestGetFetchStrategies()
	{
		var expectedTypes = new[] { typeof(CusSupportingInfoTypeSupporterFetchStrategy) };
		var actualTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)Instruction).GetFetchStrategies().Select(c => c.GetType());

		AssertContainsExactElementsInAnyOrder(expectedTypes, actualTypes);
	}

	public void TestLookups() => AssertType<CusEntryInstructionLookups>(Instruction.Lookups);

	public void TestValidation() => AssertType<CusEntryInstructionValidation>(Instruction.Validation);

	CusEntryInstruction Instruction => instruction ??= Declaration.CustomsEntryInstructions.AddNew();
	CusEntryInstruction instruction;

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;
}
