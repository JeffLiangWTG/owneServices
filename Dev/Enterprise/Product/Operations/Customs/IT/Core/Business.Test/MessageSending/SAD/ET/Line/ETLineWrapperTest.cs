using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

public abstract class ETLineWrapperAbstractTest : SADLineCommonWrapperTest<IETLine>
{
	public void TestSpecificConstructor()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		var entryLine = entryHeader.MergedLines.AddNew();

		entryHeader.CH_CEI_Instruction = ZGuid.Empty;
		AssertExceptionThrown<ArgumentNullException>("Should be exception when EntryLine has no EntryInstruction", () => GetLineWrapper(entryLine));
	}

	public void TestDeclarationType()
	{
		AssertEquals("Must be empty", "", sadLineWrapper.DeclarationType);
	}

	public void TestConsignor()
	{
		var supplier1 = GetSupplier1();
		var supplier2 = GetSupplier2();

		var invoiceA = jobDeclaration.Invoices.AddNew();
		var invoiceLine1A = invoiceA.InvoiceLines.AddNew();
		var invoiceB = jobDeclaration.Invoices.AddNew();
		var invoiceLine1B = invoiceB.InvoiceLines.AddNew();

		var entryLine1 = entryHeader.MergedLines.AddNew();
		var entryLine2 = entryHeader.MergedLines.AddNew();

		invoiceLine1A.JI_CL = entryLine1.PK;
		entryLine1.InvoiceLines.Reload(true);
		invoiceLine1B.JI_CL = entryLine2.PK;
		entryLine2.InvoiceLines.Reload(true);

		var sadHeaderWrapper = GetHeaderWrapper(entryHeader);

		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.Triangulation;
		AssertEquals("[PRE-CONDITION] HeaderDataDeclaredOnItems", false, sadHeaderWrapper.HeaderDataDeclaredOnItems);
		AssertConsignorWhenHeaderDataDeclaredOnItemsIsFalse();

		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;
		invoiceA.SupplierDocumentaryAddress.E2_OA_Address = supplier1.MainAddress.PK;
		invoiceB.SupplierDocumentaryAddress.E2_OA_Address = supplier2.MainAddress.PK;

		AssertEquals("[PRE-CONDITION] HeaderDataDeclaredOnItems", true, sadHeaderWrapper.HeaderDataDeclaredOnItems);
		AssertConsignorsWhenHeaderDataDeclaredOnItemsIsTrue();

		void AssertConsignorWhenHeaderDataDeclaredOnItemsIsFalse()
		{
			CombineAssertions("When HeaderDataDeclaredOnItems is false", () =>
			{
				var consignorEntryLine1 = GetLineWrapper(entryLine1).Consignor;
				AssertNotNull("EntryLine1->Consignor", consignorEntryLine1);
				AssertType<SADEmptyTraderWrapper>("EntryLine1->Consignor", consignorEntryLine1);

				var consignorEntryLine2 = GetLineWrapper(entryLine2).Consignor;
				AssertNotNull("EntryLine2->Consignor", consignorEntryLine1);
				AssertType<SADEmptyTraderWrapper>("EntryLine2->Consignor", consignorEntryLine1);
			});
		}

		void AssertConsignorsWhenHeaderDataDeclaredOnItemsIsTrue()
		{
			CombineAssertions("When HeaderDataDeclaredOnItems is true, Assert EntryLine1", () =>
			{
				var consignor = GetLineWrapper(entryLine1).Consignor;
				AssertNotNull("Consignor", consignor);
				AssertType<SADTraderWrapper>(consignor);
				AssertEquals("IdCountryCode", "IT", consignor.IdCountryCode);
				AssertEquals("ID", "385040449", consignor.ID);
				AssertEquals("Name", "IKEA", consignor.Name);
				AssertEquals("Address", "MAIN ADDRESS", consignor.Address);
				AssertEquals("Postcode", "4000", consignor.Postcode);
				AssertEquals("City", "ABCEXPMEL", consignor.City);
				AssertEquals("CountryCode", "ZA", consignor.CountryCode);
			});
			CombineAssertions("When HeaderDataDeclaredOnItems is true, Assert EntryLine2", () =>
			{
				var consignor = GetLineWrapper(entryLine2).Consignor;
				AssertNotNull("Consignor", consignor);
				AssertType<SADTraderWrapper>(consignor);
				AssertEquals("IdCountryCode", "IT", consignor.IdCountryCode);
				AssertEquals("ID", "496151550", consignor.ID);
				AssertEquals("Name", "BRIC", consignor.Name);
				AssertEquals("Address", "PRIMARY HOME", consignor.Address);
				AssertEquals("Postcode", "50005", consignor.Postcode);
				AssertEquals("City", "NET CITY", consignor.City);
				AssertEquals("CountryCode", "IT", consignor.CountryCode);
			});
		}

		OrgHeader GetSupplier1()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "IK1";
			supplier.CustomsCodes.AddNew("EOR", "385040449", "IT");
			var address = supplier.MainAddress;
			address.CompanyName = "IKEA";
			address.Address1 = "MAIN";
			address.Address2 = "ADDRESS";
			address.Postcode = "4000";
			address.City = "ABCEXPMEL";
			address.OA_RN_NKCountryCode = "ZA";
			return supplier;
		}

		OrgHeader GetSupplier2()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "BR1";
			supplier.CustomsCodes.AddNew("EOR", "496151550", "IT");
			var address = supplier.MainAddress;
			address.CompanyName = "BRIC";
			address.Address1 = "PRIMARY";
			address.Address2 = "HOME";
			address.Postcode = "50005";
			address.City = "NET CITY";
			address.OA_RN_NKCountryCode = "IT";
			return supplier;
		}
	}

	public void TestConsignee()
	{
		var consignee = sadLineWrapper.Consignee;
		AssertNotNull("Consignee", consignee);
		AssertType<SADEmptyTraderWrapper>("Consignee must be empty in this phase", consignee);
	}

	public void TestCountryOfDispatch()
	{
		var orgHeader1 = Factory.New<OrgHeader>();
		orgHeader1.MainAddress.OA_RN_NKCountryCode = "ES";
		var entryLine1 = entryHeader.MergedLines.AddNew();
		var invoiceHeader1 = jobDeclaration.Invoices.AddNew();
		invoiceHeader1.JZ_OH_Supplier = orgHeader1.PK;
		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine1.JI_CL = entryLine1.PK;

		CombineAssertions("entry instruction participants", () =>
		{
			entryHeader.EntryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.Triangulation;
			jobDeclaration.JE_RL_NKOrigin = "IT";

			AssertEquals("when participants is not buyers' console", "", sadLineWrapper.DispatchCountryCode);

			jobDeclaration.JE_RL_NKOrigin = string.Empty;
			entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;
			entryHeader.ResetInvoiceHeadersAndLines();

			AssertEquals("when participants is buyers' console", "", sadLineWrapper.DispatchCountryCode);
		});
	}

	public void TestDestinationCountryCode()
	{
		AssertEquals("Empty in this phase, DestinationCountryCode", "", sadLineWrapper.DestinationCountryCode);
	}

	public void TestPackages()
	{
		AssertNotNull("Packages", sadLineWrapper.Packages);
		AssertEquals("Packages count", 0, sadLineWrapper.Packages.Count());

		var invoiceLine = jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		var declarationBill = jobDeclaration.Bills.AddNew();
		var billPackingGroup = declarationBill.PackingGroups.AddNew();
		var package1 = jobDeclaration.Packages.AddNew();
		package1.CW_CR_HouseContainer = billPackingGroup.PK;
		package1.CW_PackQty = 100;
		package1.CW_PackType = "VG";
		var package2 = jobDeclaration.Packages.AddNew();
		package2.CW_CR_HouseContainer = billPackingGroup.PK;
		package2.CW_PackQty = 200;
		package2.CW_PackType = "VG";
		var package3 = jobDeclaration.Packages.AddNew();
		package3.CW_CR_HouseContainer = billPackingGroup.PK;
		package3.CW_PackQty = 15;
		package3.CW_PackType = "VG";

		var packageInvoiceLine1 = invoiceLine.PackagesPivot.AddNew();
		packageInvoiceLine1.CHC_CW = package1.PK;
		packageInvoiceLine1.CHC_NumberOfPacks = 99;

		var packageInvoiceLine2 = invoiceLine.PackagesPivot.AddNew();
		packageInvoiceLine2.CHC_CW = package2.PK;
		packageInvoiceLine2.CHC_NumberOfPacks = 50;
		entryLine.InvoiceLines.Reload(true);

		sadLineWrapper = GetLineWrapper(entryLine);

		AssertNotNull("Packages", sadLineWrapper.Packages);
		AssertEquals("Packages count", 1, sadLineWrapper.Packages.Count());
		AssertType<SADLinePackageWrapper>("Package Type", sadLineWrapper.Packages.First());
	}

	public override void TestCountryOfOrigin()
	{
		var invoiceLine = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();

		invoiceLine.JI_StateOrRegionOfOrigin = "";
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals(nameof(sadLineWrapper.CountryOfOrigin), "", sadLineWrapper.CountryOfOrigin);

		invoiceLine.JI_StateOrRegionOfOrigin = "MI";
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals(nameof(sadLineWrapper.CountryOfOrigin), "MI", sadLineWrapper.CountryOfOrigin);
	}

	public void TestComplementOfInformation()
	{
		AssertEquals("ComplementOfInformation", "", sadLineWrapper.ComplementOfInformation);
	}

	public void TestComplementOfInformationLng()
	{
		AssertEquals("ComplementOfInformationLng", "", sadLineWrapper.ComplementOfInformationLng);
	}

	protected override void SetUp()
	{
		base.SetUp();
		SetDeclarationMessageType(jobDeclaration);
	}

	protected virtual void SetDeclarationMessageType(JobDeclaration declaration)
	{
		declaration.JE_MessageType = "EXP";
	}

	protected sealed override IReadOnlyList<ZString> ExpectedNationalProcedures => new ZString[] { "0" };

	protected abstract IETHeader GetHeaderWrapper(CusEntryHeader entryHeader);
}

sealed class ETLineWrapperBaseOnlyTest : ETLineWrapperAbstractTest
{
	public void TestSecurityBlock()
	{
		var securityBlock = sadLineWrapper.SecurityBlock;
		AssertNotNull("SecurityBlock", securityBlock);
		AssertType<ETLineSecurityBlockWrapper>("SecurityBlock", securityBlock);
	}

	public void TestSpecialMentionGroup()
	{
		AssertNotNull("SpecialMentionGroup should not be null", sadLineWrapper.SpecialMentionGroup);
		AssertType<ETLineSpecialMentionGroupWrapper>("SpecialMentionGroup type", sadLineWrapper.SpecialMentionGroup);
	}

	protected override IETHeader GetHeaderWrapper(CusEntryHeader entryHeader)
	{
		return new ETHeaderWrapper(entryHeader);
	}

	protected override IETLine GetLineWrapper(CusEntryLine entryLine)
	{
		return new ETLineWrapper(entryLine);
	}
}
