using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(CusEntryLine))]
sealed class CusEntryLineTest : EU.Business.Declaration.Testing.CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
{
	protected override Type GetExpectedTaxBoxSupporterType() => typeof(CusEntryLineFee);

	public void TestTypeOfHeader()
	{
		AssertType<CusEntryHeader>(entryLine.Header);
	}

	public void TestTypeOfCusEntryLineFeeCollection()
	{
		AssertType<EU.Business.Declaration.CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>>(entryLine.Fees);
	}

	protected override Type ExpectedTypeOfFees => typeof(EU.Business.Declaration.CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>);

	protected override List<(string CSI_Code, string CSI_SubType)> ExpectedPreviousDocumentCodesAndSubTypes => new List<(string, string)>
	{
		new ("123", "Y"),
		new ("456", "K"),
	};

	public override void TestAdditionalInfos()
	{
		EU.Business.Declaration.Testing.CusEntryHeaderTestHelper.CreateAdditionalInfos(Factory);

		var dec = GetJobDeclarationForTestWithValidTestData();
		dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		var inv = dec.Invoices.AddNew();
		var invLine1 = inv.JobComInvoiceLines.AddNew();
		invLine1.JI_Tariff = "2203001010";
		var invLine2 = inv.JobComInvoiceLines.AddNew();
		invLine2.JI_Tariff = "2203001010";

		var addInfo1 = invLine1.AdditionalInfos.AddNew();
		addInfo1.CSI_Code = "9001";
		addInfo1.CSI_Description = "9001 Desc";

		var addInfo2 = invLine2.AdditionalInfos.AddNew();
		addInfo2.CSI_Code = "9001";
		addInfo2.CSI_Description = "9001 Desc";

		var addInfo3 = inv.AdditionalInfos.AddNew();
		addInfo3.CSI_Code = "9003";
		addInfo3.CSI_Description = "9003 Desc";

		var addInfo4 = inv.AdditionalInfos.AddNew();
		addInfo4.CSI_Code = "9001";
		addInfo4.CSI_Description = "9001 Desc";

		var addInfo5 = inv.AdditionalInfos.AddNew();
		addInfo5.CSI_Code = "9001";
		addInfo5.CSI_Description = "9001 Desc";

		DoMerge(dec);

		CombineAssertions(() =>
		{
			AssertEquals("Header Count 1", 1, dec.ActiveEntryHeaders.Count);
			AssertEquals("MergedLines Count 1", 1, dec.ActiveEntryHeaders[0].MergedLines.Count);
			AssertEquals("AdditionalInfos Count 2", 2, ((CusEntryLine)dec.ActiveEntryHeaders[0].MergedLines[0]).AdditionalInfos.Count());

			addInfo2.CSI_Code = "12345";
			addInfo4.CSI_Code = "67890";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merged Lines Count 2", 2, dec.ActiveEntryHeaders[0].MergedLines.Count);
			AssertEquals("AdditionalInfos[0] Count 3", 3, ((CusEntryLine)dec.ActiveEntryHeaders[0].MergedLines[0]).AdditionalInfos.Count());
			AssertEquals("AdditionalInfos[1] Count 4", 4, ((CusEntryLine)dec.ActiveEntryHeaders[0].MergedLines[1]).AdditionalInfos.Count());

			AssertType<AdditionalInfo>(((CusEntryLine)dec.ActiveEntryHeaders[0].MergedLines[0]).AdditionalInfos.First());
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = (JobDeclaration)ImportJobDeclaration;
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
		invoice.InvoiceLines.AddNew();

		DoMerge(declaration);
		entryLine = ((CusEntryHeader)declaration.ActiveEntryHeaders[0]).AllEntryLines[0];
	}

	protected override void DoMerge(BaseJobDeclaration declaration)
	{
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		base.DoMerge(declaration);
	}

	CusEntryLine entryLine;
}
