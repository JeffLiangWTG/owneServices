using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(CusSupplyChainActorReference))]
sealed class CusSupplyChainActorReferenceTest : CusSupplyChainActorReferenceAbstractTest<CusSupplyChainActorReference>
{
	public void TestValidation() => AssertType<CusSupplyChainActorReferenceValidation>(CusReference.Validation);

	public void TestCFR_Reference_Caption() => CaptionTestHelper.AssertCaptions(CusReference.CFR_ReferenceInfo, caption: "Identification (BP-ID/UID/DUNS)", mediumCaption: "Identification", shortCaption: "ID");

	CusSupplyChainActorReference CusReference => cusReference ?? (cusReference = Factory.New<CusSupplyChainActorReference>());
	CusSupplyChainActorReference cusReference;
}
