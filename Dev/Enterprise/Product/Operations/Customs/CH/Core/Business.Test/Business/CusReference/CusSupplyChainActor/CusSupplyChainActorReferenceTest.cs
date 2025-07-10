using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CusSupplyChainActorReference))]
sealed class CusSupplyChainActorReferenceTest : CusReferenceAbstractTest<CusSupplyChainActorReference>
{
	public void TestHumanReadableName() => AssertEquals("Supply Chain Actor Reference", CusSupplyChainActor.HumanReadableName);

	public void TestCFR_Reference_Caption() => CaptionTestHelper.AssertCaptions(CusSupplyChainActor.CFR_ReferenceInfo, caption: "Identification (BP-ID/UID/DUNS)", mediumCaption: "Identification", shortCaption: "ID");

	public void TestSetDefaultValues() => AssertEquals(CusReferenceTypeList.Codes.SupplyChainActor, CusSupplyChainActor.CFR_Type);

	public void TestValidation() => AssertType<CusSupplyChainActorReferenceValidation>(CusSupplyChainActor.Validation);

	public void TestParent() => AssertType<CusEntryInstruction>(CusSupplyChainActor.Parent);

	protected override BusinessObject GetNewBusinessObject() => CusSupplyChainActor;

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetCusSupplyChainActor(factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => CusSupplyChainActor;

	CusSupplyChainActorReference CusSupplyChainActor => cusSupplyChainActorReference ??= GetCusSupplyChainActor(Factory);
	CusSupplyChainActorReference cusSupplyChainActorReference;

	CusSupplyChainActorReference GetCusSupplyChainActor(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var cusSupplyChainActor = entryInstruction.SupplyChainActors.AddNew();
		cusSupplyChainActor.CFR_Code = "MF";
		cusSupplyChainActor.CFR_Reference = "X";
		factory.Save();

		return cusSupplyChainActor;
	}

	protected override IEnumerable<CusSupplyChainActorReference> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		yield return FillWithValidData(declaration.CustomsEntryInstructions.AddNew().SupplyChainActors.AddNew());
	}

	CusSupplyChainActorReference FillWithValidData(CusSupplyChainActorReference reference)
	{
		reference.CFR_Code = SupplyChainActorRoleList.Codes.CS;
		reference.CFR_Reference = "111";
		return reference;
	}
}
