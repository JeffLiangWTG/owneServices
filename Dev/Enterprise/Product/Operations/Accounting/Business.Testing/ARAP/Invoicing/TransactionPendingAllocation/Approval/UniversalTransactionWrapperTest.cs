using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;
using static Enterprise.Accounting.Business.TestObjectCreator;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(UniversalTransactionWrapper))]
	class UniversalTransactionWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestInitialize()
		{
			var wrapper = (UniversalTransactionWrapper)GetNewBusinessObject();

			AssertNotNull(wrapper.Lines);
			AssertEquals(0, wrapper.Lines.Count);

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddress.AddressType = nameof(DocAddressType.None);
			orgAddress.OrganizationCode = new ZCodeMappedZString("organization");
			orgAddress.CompanyName = "company name";
			orgAddress.Address1 = "address";
			orgAddress.City = "city";
			orgAddress.State = "state";
			orgAddress.Port = new UNLOCO { Code = "port" };
			orgAddress.Contact = "contact";
			universalTransaction.OrganizationAddress = orgAddress;

			var currentCompnay = GlbCompany.GetCurrentCompany(Factory);
			OrgPatternMatchOverride patternMatchOverride = AddMatchingRuleForOrganization(currentCompnay.OrgProxy, orgAddress.OrganizationCode.Value, TestObjectCreator.Creditor1);

			universalTransaction.TransactionDate = ZDateTime.Today.AddDays(-3);
			universalTransaction.DueDate = ZDateTime.Today.AddDays(2);
			universalTransaction.PostDate = ZDateTime.Today.AddDays(-1);
			universalTransaction.Number = "1234";
			universalTransaction.NumberOfSupportingDocuments = 5;
			universalTransaction.Description = "text";
			universalTransaction.Branch = new Branch { Code = "ZXC", Name = "branch name" };
			universalTransaction.Department = new Department { Code = "DPP", Name = "department name" };

			universalTransaction.OSCurrency = new Currency { Code = "GBP" };
			universalTransaction.LocalCurrency = new Currency { Code = "AUD" };
			universalTransaction.OSExGSTVATAmount = 120;
			universalTransaction.LocalExVATAmount = 60;
			universalTransaction.OSGSTVATAmount = 10;
			universalTransaction.LocalVATAmount = 5;
			universalTransaction.PlaceOfSupply = new PlaceOfSupply()
			{
				Location = new CodeDescriptionPair5Char() { Code = "NSW", Description = "New South Wales" },
				LocationType = new CodeDescriptionPair() { Code = "STA", Description = "State" }
			};

			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());

			bool isRefreshBindingCalled = false;
			((IBindingList)wrapper).ListChanged += (sender, e) => isRefreshBindingCalled = true;
			wrapper.Initialize(universalTransaction.Serialize(), false);
			Assert("RefreshBinding should be called", isRefreshBindingCalled);
			AssertEquals("Creditor", "ZCreditor1", wrapper.Creditor);
			AssertNotNull("Creditor Org Header", wrapper.CreditorOrgHeader);
			AssertEquals("Creditor Org Header", "ZCreditor1", wrapper.CreditorOrgHeader.OH_Code);
			AssertEquals("Creditor", "organization", wrapper.CreditorSource);
			AssertEquals("Creditor", "company name", wrapper.CreditorFullName);
			AssertEquals("Address", "address city state port", wrapper.Address);
			AssertEquals("Contact", "contact", wrapper.Contact);
			AssertEquals("TransactionDate", ZDateTime.Today.AddDays(-3), wrapper.TransactionDate);
			AssertEquals("DueDate", ZDateTime.Today.AddDays(2), wrapper.DueDate);
			AssertEquals("PostDate", ZDateTime.Today.AddDays(-1), wrapper.PostDate);
			AssertEquals("Number", "1234", wrapper.TransactionNumber);
			AssertEquals("NumberOfSupportingDocuments", 5, wrapper.NumberOfSupportingDocuments);
			AssertEquals("Description", "text", wrapper.Description);
			AssertEquals("Branch", "ZXC", wrapper.Branch);
			AssertEquals("Branch", "branch name", wrapper.BranchName);
			AssertEquals("Department", "DPP", wrapper.Department);
			AssertEquals("Department", "department name", wrapper.DepartmentName);
			AssertEquals("OSCurrency", "GBP", wrapper.OSCurrency);
			AssertEquals("LocalCurrency", "AUD", wrapper.LocalCurrency);
			AssertEquals("OSExGSTVATAmount", 120m, wrapper.OSExGSTVATAmount);
			AssertEquals("LocalExVATAmount", 60m, wrapper.LocalExVATAmount);
			AssertEquals("OSGSTVATAmount", 10m, wrapper.OSGSTVATAmount);
			AssertEquals("LocalVATAmount", 5m, wrapper.LocalVATAmount);
			AssertEquals("PlaceOfSupply", "NSW", wrapper.PlaceOfSupply);
			AssertEquals("PlaceOfSupplyType", "STA", wrapper.PlaceOfSupplyType);

			AssertEquals(0, wrapper.Lines.Count);

			orgAddress.AddressShortCode = "address short code";

			var universalLine1 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine1.Branch = new Branch { Code = "lBR" };
			universalLine1.Department = new Department { Code = "lDP" };
			universalLine1.ChargeCode = new ChargeCode { Code = "cCD" };
			universalLine1.GLAccount = new GLAccount { AccountCode = "123.13.12" };
			universalLine1.Description = "line text";
			universalLine1.IsFinalCharge = true;
			universalLine1.Sequence = 3;
			universalLine1.OSCurrency = new Currency { Code = "USD" };
			universalLine1.LocalCurrency = new Currency { Code = "AUD" };
			universalLine1.OSAmount = -80;
			universalLine1.OSGSTVATAmount = -10;
			universalLine1.LocalAmount = -40;
			universalLine1.LocalGSTVATAmount = -5;
			universalLine1.OSWHTAmount = -6;
			universalLine1.VATTaxID = new TaxID { TaxCode = "tax" };
			universalLine1.WithholdingTaxID = new TaxID { TaxCode = "WHT tax" };
			universalTransaction.PostingJournalCollection.Add(universalLine1);

			OrgPatternMatchOverride patternMatchOverrideForChargeCode = AddMatchingRuleForChargeCode(currentCompnay.OrgProxy, universalLine1.ChargeCode.Code.Value, TestObjectCreator.FRT);

			isRefreshBindingCalled = false;
			wrapper.Initialize(universalTransaction.Serialize(), false);
			Assert("RefreshBinding should be called", isRefreshBindingCalled);
			AssertEquals("Address", "address short code city state port", wrapper.Address);
			AssertEquals(1, wrapper.Lines.Count);
			var line = wrapper.Lines[0];
			AssertEquals("Branch", "lBR", line.Branch);
			AssertEquals("Department", "lDP", line.Department);
			AssertEquals("ChargeCode", "cCD", line.ChargeCodeSource);
			AssertEquals("ChargeCode", "FRT", line.ChargeCode);
			AssertEquals("GLAccount", "123.13.12", line.GLAccount);
			AssertEquals("Description", "line text", line.Description);
			AssertEquals("IsFinalCharge", true, line.IsFinalCharge);
			AssertEquals("Sequence", 3, line.Sequence);
			AssertEquals("OSCurrency", "USD", line.OSCurrency);
			AssertEquals("LocalCurrency", "AUD", line.LocalCurrency);
			AssertEquals("OSAmount", -80m, line.OSAmount);
			AssertEquals("OSGSTVATAmount", -10m, line.OSGSTVATAmount);
			AssertEquals("LocalAmount", -40m, line.LocalAmount);
			AssertEquals("LocalGSTVATAmount", -5m, line.LocalGSTVATAmount);
			AssertEquals("OSWHTAmount", -6m, line.OSWHTAmount);
			AssertEquals("VATTaxID", "tax", line.VATTaxID);
			AssertEquals("WithholdingTaxID", "WHT tax", line.WithholdingTaxID);
		}

		public void TestInitialize_CrossLedger()
		{
			var wrapper = (UniversalTransactionWrapper)GetNewBusinessObject();

			AssertNotNull(wrapper.Lines);
			AssertEquals(0, wrapper.Lines.Count);

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.None),
				OrganizationCode = new ZCodeMappedZString("organization"),
				CompanyName = "company name",
				Address1 = "address",
				City = "city",
				State = "state",
				Port = new UNLOCO { Code = "port" },
				Contact = "contact"
			};
			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				OrganizationAddress = orgAddress,
				BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), OrganizationCode = TestObjectCreator.NonCurrentCompany.OrgProxy.OH_Code, Country = Country.New(TestObjectCreator.NonCurrentCompany.Country) }
			};
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.DataContext.SetCompanyAndDataProviderDetails(TestObjectCreator.NonCurrentCompany);

			bool isRefreshBindingCalled = false;
			((IBindingList)wrapper).ListChanged += (sender, e) => isRefreshBindingCalled = true;
			wrapper.Initialize(universalTransaction.Serialize(), true);
			Assert("RefreshBinding should be called", isRefreshBindingCalled);
			AssertEquals("Creditor", ZString.Empty, wrapper.Creditor);
			AssertEquals("Creditor", universalTransaction.DataContext.DataProviderForCodeMapping, wrapper.CreditorSource);
			AssertEquals("Creditor", "", wrapper.CreditorFullName);
			AssertEquals("Address", "", wrapper.Address);
			AssertEquals("Contact", "", wrapper.Contact);

			var currentCompnay = GlbCompany.GetCurrentCompany(Factory);
			OrgPatternMatchOverride patternMatchOverride = AddMatchingRuleForOrganization(currentCompnay.OrgProxy, universalTransaction.DataContext.DataProviderForCodeMapping, TestObjectCreator.Creditor1);

			isRefreshBindingCalled = false;
			wrapper.Initialize(universalTransaction.Serialize(), true);
			Assert("RefreshBinding should be called", isRefreshBindingCalled);
			AssertEquals("Creditor", "ZCreditor1", wrapper.Creditor);
			AssertEquals("Creditor", universalTransaction.DataContext.DataProviderForCodeMapping, wrapper.CreditorSource);
			AssertEquals("Creditor", "", wrapper.CreditorFullName);
			AssertEquals("Address", "", wrapper.Address);
			AssertEquals("Contact", "", wrapper.Contact);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
