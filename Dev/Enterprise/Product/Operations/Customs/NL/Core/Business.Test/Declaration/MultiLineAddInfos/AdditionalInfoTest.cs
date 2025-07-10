using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(AdditionalInfo))]
class AdditionalInfoTest : Customs.Business.Testing.CusSupportingInfoTest<AdditionalInfo>
{
	public void TestValidationType()
	{
		AssertType<AdditionalInfoValidation>(headerAddInfo.Validation);
	}

	public void TestCSI_Description_ReadOnly()
	{
		AssertEquals("Description read only", true, headerAddInfo.CSI_DescriptionInfo.ReadOnly);
	}

	public void TestCSI_ReferenceNumber_MaxLength()
	{
		AssertEquals(70, lineAddInfo.CSI_ReferenceNumberInfo.MaxLength);
	}

	public void TestDefaultReferenceNumber()
	{
		declaration.JE_HouseBill = "House Way Bill";
		declaration.JE_MasterBill = "Master Way Bill";
		headerAddInfo.CSI_SubType = "TRA";

		headerAddInfo.CSI_ReferenceNumber = ZString.Empty;
		headerAddInfo.CSI_Code = "N703";
		AssertEquals("N703", headerAddInfo.CSI_ReferenceNumber, declaration.JE_HouseBill);
		headerAddInfo.CSI_ReferenceNumber = ZString.Empty;
		headerAddInfo.CSI_Code = "N704";
		AssertEquals("N704", headerAddInfo.CSI_ReferenceNumber, declaration.JE_MasterBill);
		headerAddInfo.CSI_ReferenceNumber = ZString.Empty;
		headerAddInfo.CSI_Code = "N705";
		AssertEquals("N705", headerAddInfo.CSI_ReferenceNumber, declaration.JE_MasterBill);
		headerAddInfo.CSI_ReferenceNumber = ZString.Empty;
		headerAddInfo.CSI_Code = "N714";
		AssertEquals("N714", headerAddInfo.CSI_ReferenceNumber, declaration.JE_HouseBill);
		headerAddInfo.CSI_ReferenceNumber = ZString.Empty;
		headerAddInfo.CSI_Code = "N741";
		AssertEquals("N741", headerAddInfo.CSI_ReferenceNumber, declaration.JE_MasterBill);

		headerAddInfo.CSI_ReferenceNumber = ZString.Empty;
		headerAddInfo.CSI_SubType = "REF";
		headerAddInfo.CSI_Code = "N703";
		AssertNullOrEmpty(headerAddInfo.CSI_ReferenceNumber);
	}

	public void TestHasDuplicateInAdditionalInfoCollectionOnInvoiceLineParent()
	{
		lineAddInfo.CSI_SubType = "TRA";
		lineAddInfo.CSI_Code = "Y904";
		lineAddInfo.CSI_ReferenceNumber = "Reference";
		var lineAddInfo2 = ((JobComInvoiceLine)lineAddInfo.Parent).AdditionalInfos.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("No duplicate exists", false, lineAddInfo.HasDuplicateInAdditionalInfoCollectionOnInvoiceLineParent);
			lineAddInfo2.CSI_SubType = lineAddInfo.CSI_SubType;
			lineAddInfo2.CSI_Code = lineAddInfo.CSI_Code;
			lineAddInfo2.CSI_ReferenceNumber = lineAddInfo.CSI_ReferenceNumber;
			AssertEquals("Duplicate exists", true, lineAddInfo.HasDuplicateInAdditionalInfoCollectionOnInvoiceLineParent);
			AssertEquals("Duplicate exists on duplicated line", true, lineAddInfo2.HasDuplicateInAdditionalInfoCollectionOnInvoiceLineParent);
		});
	}

	protected override IEnumerable<AdditionalInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var dec = factory.New<JobDeclaration>();
		var header = dec.Invoices.AddNew();
		var line = dec.InvoiceLines.AddNew();
		yield return dec.AdditionalInfos.AddNew();
		yield return header.AdditionalInfos.AddNew();
		yield return line.AdditionalInfos.AddNew();
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var dec = Factory.New<JobDeclaration>();
		var header = dec.Invoices.AddNew();
		var line = dec.InvoiceLines.AddNew();
		var add = header.AdditionalInfos.AddNew();
		return add;
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		headerAddInfo = invoice.AdditionalInfos.AddNew();
		lineAddInfo = invoiceLine.AdditionalInfos.AddNew();
	}
	AdditionalInfo headerAddInfo;
	AdditionalInfo lineAddInfo;
	JobDeclaration declaration;
}
