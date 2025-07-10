using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NonCustomsLaw))]
internal class NonCustomsLawTest : Customs.Business.Testing.CusSupportingInfoTest<NonCustomsLaw>
{
	public void TestCSI_CodeMaxLength() => AssertEquals(3, NonCustomsLaw.CSI_CodeInfo.MaxLength);

	public void TestCSI_CodeCaption() => AssertEquals("Code Caption", "Type", NonCustomsLaw.CSI_CodeInfo.Description);

	public void TestCodeDescriptionCaption() => AssertEquals("Description Caption", "Description", NonCustomsLaw.CodeDescriptionInfo.Description);

	public void TestCodeDescription()
	{
		RefCusCodeTestHelper.CreateNonCustomsLawTypeCodesList(Factory);

		CombineAssertions(() =>
		{
			NonCustomsLaw.CSI_Code = "26";
			AssertEquals("Code Description", "Cultural property", NonCustomsLaw.CodeDescription);

			NonCustomsLaw.CSI_Code = ZString.Empty;
			AssertEquals("Code Description", ZString.Empty, NonCustomsLaw.CodeDescription);

			NonCustomsLaw.CSI_Code = "26";
			AssertEquals("Code Description", "Cultural property", NonCustomsLaw.CodeDescription);

			NonCustomsLaw.CSI_Code = RefCusCodeTestHelper.InvalidNonCustomsLawTypeCode;
			AssertEquals("Code Description", ZString.Empty, NonCustomsLaw.CodeDescription);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewNonCustomsLaw(Factory);

	protected override IEnumerable<NonCustomsLaw> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		yield return GetNewNonCustomsLaw(factory);
	}

	NonCustomsLaw GetNewNonCustomsLaw(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		return declaration.Invoices.AddNew().InvoiceLines.AddNew().NonCustomsLaws.AddNew();
	}
	NonCustomsLaw NonCustomsLaw => nonCustomsLaw ?? (nonCustomsLaw = GetNewNonCustomsLaw(Factory));
	NonCustomsLaw nonCustomsLaw;
}
