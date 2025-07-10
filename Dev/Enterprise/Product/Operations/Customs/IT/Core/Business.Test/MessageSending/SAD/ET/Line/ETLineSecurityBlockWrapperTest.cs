using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

public abstract class ETLineSecurityBlockWrapperAbstractTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => GetLineSecurityBlock(null));
		AssertNoExceptionThrown(() => GetLineSecurityBlock(Factory.New<CusEntryLine>()));
	}

	public void TestUNDangerousGoodsCode()
	{
		AssertEquals("UNDangerousGoodsCode", "", lineSecurityBlock.UNDangerousGoodsCode);

		var invoice1 = declaration.Invoices.AddNew();
		invoice1.JZ_InvoiceNumber = "1";
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_CL = entryLine.PK;

		var invoice3 = declaration.Invoices.AddNew();
		invoice3.JZ_InvoiceNumber = "3";
		var invoiceLine3 = invoice3.InvoiceLines.AddNew();
		invoiceLine3.JI_CL = entryLine.PK;

		var invoice2 = declaration.Invoices.AddNew();
		invoice2.JZ_InvoiceNumber = "2";
		var invoiceLine2_2 = invoice2.InvoiceLines.AddNew();
		invoiceLine2_2.JI_OrderNumber = "2";
		invoiceLine2_2.JI_CL = entryLine.PK;
		var invoiceLine2_1 = invoice2.InvoiceLines.AddNew();
		invoiceLine2_1.JI_OrderNumber = "1";
		invoiceLine2_1.JI_CL = entryLine.PK;
		var substance1001 = UNDGSubstanceLoader.LoadSubstances(Factory, "1001", "", "IMO").First();
		var substance1002 = UNDGSubstanceLoader.LoadSubstances(Factory, "1002", "", "IMO").First();
		invoiceLine1.UNDGs.AddNew();
		invoiceLine3.UNDGs.AddNew().DI_DG = substance1002.PK;
		invoiceLine2_1.UNDGs.AddNew().DI_DG = substance1001.PK;
		entryLine.InvoiceLines.Reload(true);

		lineSecurityBlock = GetLineSecurityBlock(entryLine);
		AssertEquals("UNDangerousGoodsCode", "1001", lineSecurityBlock.UNDangerousGoodsCode);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		SetDeclarationMessageType(declaration);
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryLine = entryHeader.MergedLines.AddNew();
		lineSecurityBlock = GetLineSecurityBlock(entryLine);
	}

	protected JobDeclaration declaration;
	protected CusEntryLine entryLine;
	protected IETLineSecurityBlock lineSecurityBlock;

	protected virtual void SetDeclarationMessageType(JobDeclaration declaration)
	{
		declaration.JE_MessageType = "EXP";
	}

	protected abstract IETLineSecurityBlock GetLineSecurityBlock(CusEntryLine entryLine);

	public abstract void TestConsignor();
	public abstract void TestConsignee();
}

sealed class ETLineSecurityBlockWrapperBaseOnlyTest : ETLineSecurityBlockWrapperAbstractTest
{
	public override void TestConsignor()
	{
		var consignor = lineSecurityBlock.Consignor;
		AssertNotNull("Consignor", consignor);
		AssertType<SADEmptyTraderWrapper>("Consignor", consignor);
	}

	public override void TestConsignee()
	{
		var consignee = lineSecurityBlock.Consignee;
		AssertNotNull("Consignee", consignee);
		AssertType<SADEmptyTraderWrapper>("Consignee", consignee);
	}

	public void TestTransportChargesMethodOfPayment()
	{
		AssertEquals("TransportChargesMethodOfPayment", "", lineSecurityBlock.TransportChargesMethodOfPayment);
	}

	public void TestCommercialReferenceNumber()
	{
		AssertEquals("Empty in this phase, CommercialReferenceNumber", "", lineSecurityBlock.CommercialReferenceNumber);
	}

	protected override IETLineSecurityBlock GetLineSecurityBlock(CusEntryLine entryLine)
	{
		return new ETLineSecurityBlockWrapper(entryLine);
	}
}
