using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(FiscalReference))]
sealed class FiscalReferenceTest : Customs.Business.Testing.CusSupportingInfoTest<FiscalReference>
{
	public void TestHumanReadableName()
	{
		AssertEquals("Fiscal Reference", fiscalReference.HumanReadableName);
	}

	public void TestCusEntryInstructionPivot()
	{
		var declaration = Factory.New<JobDeclaration>();
		var cei = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = cei.PK;

		var fr1 = declaration.FiscalReferences.AddNew();
		fr1.CSI_Code = "FR1";
		fr1.CSI_ReferenceNumber = "NL11111111";
		fr1.EntryInstructionID = entryHeader.EntryInstruction.PK;

		AssertNotEquals(Guid.Empty, fr1.EntryInstructionID.ToGuid());

		var pivot = Factory.LoadTop1<GenPivot>(new ZQuery(GenPivotSchema.XX_Relation1TableCode, CusSupportingInfoSchema.Constants.Prefix));

		AssertNotNull(pivot);
		AssertEquals(entryHeader.EntryInstruction.PK.ToGuid(), pivot.XX_Relation2ID.ToGuid());
		AssertEquals(fr1.PK.ToGuid(), pivot.XX_Relation1ID.ToGuid());
	}

	public void TestDefaults()
	{
		AssertEquals("FIS", fiscalReference.CSI_Type);
		AssertEquals("Holder EORI", fiscalReference.CSI_ReferenceNumberInfo.Description);
	}

	protected override IEnumerable<FiscalReference> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		yield return declaration.FiscalReferences.AddNew();
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		return declaration.FiscalReferences.AddNew();
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();

		fiscalReference = declaration.FiscalReferences.AddNew();
	}

	FiscalReference fiscalReference;

	JobDeclaration declaration;
}
