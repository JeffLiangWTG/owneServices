using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(Permit))]
public class PermitTest : Customs.Business.Testing.CusSupportingInfoTest<Permit>
{
	public void TestHumanReadableName()
	{
		AssertEquals("Permit", permit.HumanReadableName);
	}

	public void TestCSI_Code()
	{
		CombineAssertions(() =>
		{
			AssertEquals("MaxLength", 2, permit.CSI_CodeInfo.MaxLength);
			AssertEquals("Caption", "Type", permit.CSI_CodeInfo.Description);
		});
	}

	public void TestCSI_IssuerType()
	{
		CombineAssertions(() =>
		{
			AssertEquals("MaxLength", 2, permit.CSI_IssuerTypeInfo.MaxLength);
			AssertEquals("Caption", "Authority", permit.CSI_IssuerTypeInfo.Description);
		});
	}

	public void TestCSI_ReferenceNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("MaxLength", 35, permit.CSI_ReferenceNumberInfo.MaxLength);
			AssertEquals("Caption", "Number", permit.CSI_ReferenceNumberInfo.Description);
		});
	}

	public void TestCSI_DateOfIssue()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Issue Date", permit.CSI_DateOfIssueInfo.Description);
		});
	}

	public void TestCSI_Description()
	{
		CombineAssertions(() =>
		{
			AssertEquals("MaxLength", 70, permit.CSI_DescriptionInfo.MaxLength);
			AssertEquals("Caption", "Additional Information", permit.CSI_DescriptionInfo.Description);
		});
	}

	public void TestICusCodeDataTypeSupporter()
	{
		ICusCodeDataTypeSupporter supporter = permit;
		AssertEquals(typeof(PermitItemDetail), supporter.GetCusCodeDataTypes()[CusCodeDataTypeList.Codes.PermitItemDetails]);
	}

	public void TestPermitDetailsSavedAndLoaded()
	{
		permit.Parent.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		permit.CSI_Code = UniversalReferenceConstants.PermitCodes.GeneralEPermit;

		var detail1 = permit.PermitItemDetails.AddNew();
		detail1.CY_Code = PermitItemDetailKeyList.Codes.Key1;
		var detail2 = permit.PermitItemDetails.AddNew();
		detail2.CY_Code = PermitItemDetailKeyList.Codes.Key2;
		Factory.Save();

		var newFactory = new BusinessObjectFactory();
		var newPermit = newFactory.Load<Permit>(permit.PK);
		AssertEquals(2, newPermit.PermitItemDetails.Count);
		newPermit.PermitItemDetails.Contains(detail1.PK);
		newPermit.PermitItemDetails.Contains(detail2.PK);
	}

	public void TestSupportsPermitItemDetails()
	{
		CombineAssertions(() =>
		{
			permit.CSI_Code = "1";
			AssertEquals($"{permit.Parent.JobDeclaration.JE_MessageType} {permit.CSI_Code}", false, permit.SupportsPermitItemDetails);

			permit.CSI_Code = UniversalReferenceConstants.PermitCodes.GeneralEPermit;
			AssertEquals($"{permit.Parent.JobDeclaration.JE_MessageType} {permit.CSI_Code}", true, permit.SupportsPermitItemDetails);

			permit.CSI_Code = UniversalReferenceConstants.PermitCodes.SingleEPermit;
			AssertEquals($"{permit.Parent.JobDeclaration.JE_MessageType} {permit.CSI_Code}", true, permit.SupportsPermitItemDetails);
		});
	}

	public void TestPermitItemDetailsDeletedWhenNotSupported()
	{
		permit.Parent.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

		CombineAssertions(() =>
		{
			permit.CSI_Code = UniversalReferenceConstants.PermitCodes.GeneralEPermit;
			permit.PermitItemDetails.AddNew();
			Factory.Save();
			AssertEquals($"PermitItemDetails.Count when details supported", 1, permit.PermitItemDetails.Count);
			permit.CSI_Code = "1";
			Factory.Save();
			AssertEquals($"PermitItemDetails.Count when details not supported", 0, permit.PermitItemDetails.Count);
		});
	}

	public void TestHasQuantityAndLineNumberItemDetails()
	{
		AssertEquals("No ItemDetails", ZBool.False, permit.HasQuantityAndLineNumberItemDetails);
		permit.PermitItemDetails.AddNew().CY_Code = PermitItemDetailKeyList.Codes.Key1;
		AssertEquals("One ItemDetail with Key = 1", ZBool.False, permit.HasQuantityAndLineNumberItemDetails);
		permit.PermitItemDetails.AddNew().CY_Code = PermitItemDetailKeyList.Codes.Key2;
		AssertEquals("One ItemDetail with Key = 1 and one ItemDetail with Key = 2", ZBool.True, permit.HasQuantityAndLineNumberItemDetails);
		permit.PermitItemDetails[1].CY_Code = PermitItemDetailKeyList.Codes.Key3;
		AssertEquals("One ItemDetail with Key = 1 and one ItemDetail with Key = 3", ZBool.False, permit.HasQuantityAndLineNumberItemDetails);
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewPermit();

	protected override IEnumerable<Permit> GetBizObjsForCorrectlyTypeDecideTest(
		BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var permit = invoiceLine.Permits.AddNew();
		yield return permit;
	}

	Permit GetNewPermit()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		return invoiceLine.Permits.AddNew();
	}

	protected override void SetUp()
	{
		base.SetUp();
		permit = GetNewPermit();
	}
	Permit permit;
}
