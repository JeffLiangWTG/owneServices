using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Testing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.Accounting.DocRollUpSort;
using Enterprise.DocumentWrappers.Accounting.DocRollUpSort.RollUpper;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	public abstract class DocARBaseInvoiceTest : AccountingDocumentWrapperTestCase
	{
		protected abstract DocARBaseInvoice GetBaseInvoiceWrapper();

		protected abstract TransactionHeader GetWrappedInvoice();

		public void TestDefaultBrandName()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var organisation = GlbCompany.CurrentCompany.OrgProxy;
			var branch = GlbBranch.CurrentBranch;
			var companyNameOverride = "TEST COMPANY NAME OVERRIDE";

			var orgAddress = testObjectCreator.CreateAddress(organisation);
			orgAddress.AddAddressType(OrgAddressType.Receivables);

			var invoice = testObjectCreator.CreateARInvoice<ARInvoice>("10001", testObjectCreator.AUD, 1m, testObjectCreator.AALSHI);
			var docInvoice = DocARInvoice.New(invoice, Factory);

			using (AccountingConfigurationRegistry.Instance.PrintBranchAddressInFooter.SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, true))
			{
				AssertEquals("Precondition : Company Name Override is empty", ZString.Empty, orgAddress.OA_CompanyNameOverride);
				AssertEquals("Default Brand Name - Organisation Name", organisation.OH_FullName.ToUpper(), docInvoice.BrandName);

				orgAddress.OA_CompanyNameOverride = companyNameOverride;
				AssertEquals("Precondition : Company Name Override is not empty", companyNameOverride, orgAddress.OA_CompanyNameOverride);
				AssertEquals("Default Brand Name - Company Name Override", companyNameOverride, docInvoice.BrandName);
			}

			using (AccountingConfigurationRegistry.Instance.PrintBranchAddressInFooter.SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, false))
			{
				orgAddress.OA_CompanyNameOverride = ZString.Empty;
				AssertEquals("Precondition : Company Name Override is empty", ZString.Empty, orgAddress.OA_CompanyNameOverride);
				AssertEquals("Default Brand Name - Company Name", GlbCompany.CurrentCompany.GC_Name.ToUpper(), docInvoice.BrandName);

				orgAddress.OA_CompanyNameOverride = companyNameOverride;
				AssertEquals("Precondition : Company Name Override is not empty", companyNameOverride, orgAddress.OA_CompanyNameOverride);
				AssertEquals("Default Brand Name - Company Name Override", companyNameOverride, docInvoice.BrandName);
			}
		}

		public void TestInvoiceLineByChargeWillPopulateChargeType()
		{
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			var line1 = (InvoiceLine)invoice.Lines.AddNew();
			var line2 = (InvoiceLine)invoice.Lines.AddNew();

			AccChargeCode cMTCharge = Factory.NewWithValidTestData<AccChargeCode>();
			cMTCharge.AC_ChargeType = "CMT";

			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			line1.AL_AC = cMTCharge.PK;
			line2.AL_AC = cMTCharge.PK;
			line1.AL_AT = taxRate.PK;
			line2.AL_AT = taxRate.PK;

			DocARInvoice docInvoice = DocARInvoice.New(invoice, Factory);
			DocARInvoiceLineCollection rollupLines = docInvoice.InvoiceLineByCharge;
			AssertEquals("expect 1 rollup line", 1, rollupLines.Count);
			AssertEquals("charge type should be populated", "CMT", rollupLines[0].ChargeCode.ChargeType);
		}

		public void TestGetTranslatedTaxCodeFromEUCountryCodeDependsOnCurrentLanguage()
		{
			ZString currentCountryCode = GlbCompany.CurrentCompany.Country.Code;
			try
			{
				RefCountryCollection allCountries = new RefCountryCollection(Factory);
				foreach (RefCountry country in allCountries)
				{
					if (country.IsPartOfEuropeanUnion || country.RN_Code == Constants.CountryCodes.Switzerland)
					{
						ZString countryCode = country.Code;
						GlbCompany.CurrentCompany.SetCountry(countryCode);
						AssertEquals("Should be VAT", "VAT", DocARBaseInvoice.GetTranslatedTaxCodeFromCountryCode(countryCode));
						using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.French))
						{
							AssertEquals("Should be TVA", "TVA", DocARBaseInvoice.GetTranslatedTaxCodeFromCountryCode(countryCode));
						}
						using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.Dutch))
						{
							AssertEquals("Should be BTW", "BTW", DocARBaseInvoice.GetTranslatedTaxCodeFromCountryCode(countryCode));
						}
						using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.German))
						{
							AssertEquals("Should be MwSt", "MwSt", DocARBaseInvoice.GetTranslatedTaxCodeFromCountryCode(countryCode));
						}
					}
				}

				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Turkey);
				using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.Turkish))
				{
					AssertEquals("Should be KDV", "KDV", DocARBaseInvoice.GetTranslatedTaxCodeFromCountryCode(Constants.CountryCodes.Turkey));
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(currentCountryCode);
			}
		}

		public void TestInvoiceMailToAddress()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_RL_NKClosestPort = "AUSYD";

			OrgAddress address = org.Addresses.AddNew();
			address.OA_Address1 = "Address Line 1";
			address.OA_Address2 = "Address Line 2";
			address.OA_City = "SYDNEY";
			address.OA_RL_NKRelatedPortCode = "AUSYD";

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			Invoice.AH_OH = org.PK;
			DocARBaseInvoice docInvoice = GetBaseInvoiceWrapper();

			AssertEquals("Precondition: Org Country", Core.Constants.CountryCodes.Australia, docInvoice.AccountOrg.Country.Code);
			AssertEquals("Precondition: Current Company Country", Core.Constants.CountryCodes.Australia, docInvoice.Branch.MailToAddress.Country.Code);

			AssertEquals("Mail to Address", "EAGLE DATAMATION INTERNATIONAL\n10 HUTCHESON STREET\nALBION QLD\n4010\nAUSTRALIA", docInvoice.MailToAddressWithCountry);

			org.OH_RL_NKClosestPort = "NZAKL";
			address.OA_RL_NKRelatedPortCode = "NZAKL";
			docInvoice = GetBaseInvoiceWrapper();

			AssertEquals("Precondition: Org Country", Core.Constants.CountryCodes.NewZealand, docInvoice.AccountOrg.Country.Code);
			AssertEquals("Precondition: Current Company Country", Core.Constants.CountryCodes.Australia, docInvoice.Branch.MailToAddress.Country.Code);

			ZString expected = docInvoice.BrandName + System.Environment.NewLine + docInvoice.Branch.MailToAddress.PostalAddressExcludeName;
			AssertEquals("Mail to Address", "EAGLE DATAMATION INTERNATIONAL\n10 HUTCHESON STREET\nALBION QLD\n4010\nAUSTRALIA", docInvoice.MailToAddressWithCountry);
		}

		public void TestMailToAddressWithCountry_ShouldPortugalStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting()
		{
			if (InvoicingBase is ARInvoice)
			{
				InvoiceBatch.Line.Add(InvoicingBase);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
				{
					AccountingConfigurationRegistry.Instance.ARStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

					var docAddress = InvoicingBase.DocAddresses.AddNew(DocAddressType.BranchOrCompanyProxyARAdress);
					docAddress.E2_CompanyName = "TestCompany";
					docAddress.E2_Address1 = "TestAddress1";
					docAddress.E2_Address2 = "TestAddress2";

					InvoiceWrapper = GetBaseInvoiceWrapper();
					if (InvoiceWrapper.TransactionHeader is InvoicingBase)
					{
						AssertEquals("EAGLE DATAMATION INTERNATIONAL", InvoiceWrapper.BrandName);
						AssertEquals("EAGLE DATAMATION INTERNATIONAL\nTESTADDRESS1\nTESTADDRESS2", InvoiceWrapper.MailToAddressWithCountry);
					}
					else
					{
						Assert(true);
					}
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestStoredInvoiceRecepientNameandAddress()
		{
			if (InvoicingBase is ARInvoice)
			{
				InvoiceBatch.Line.Add(InvoicingBase);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
				{
					AccountingConfigurationRegistry.Instance.ARStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

					var docAddress = InvoicingBase.DocAddresses.AddNew(DocAddressType.DebtorAddress);
					docAddress.E2_CompanyName = "TestCompany";
					docAddress.E2_Address1 = "TestAddress1";
					docAddress.E2_Address2 = "TestAddress2";
					docAddress.E2_GovRegNum = "TestGov";

					InvoiceWrapper = GetBaseInvoiceWrapper();
					if (InvoiceWrapper.TransactionHeader is InvoicingBase)
					{
						AssertEquals("TESTCOMPANY\r\nTESTADDRESS1\r\nTESTADDRESS2", InvoiceWrapper.StoredInvoiceRecepientNameandAddress);
					}
					else
					{
						Assert(true);
					}
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestRecipientGovtTaxID()
		{
			ZString originalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.VietNam);
				OrgHeader header = Factory.New<OrgHeader>();
				header.OH_Code = "ORG";

				Invoice.AH_OH = header.PK;

				OrgCusCode vatCode = Invoice.Header.CustomsCodes.AddNew();
				vatCode.OK_CustomsRegNo = "0100233488-999";
				vatCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
				vatCode.OK_RN_NKCodeCountry = Constants.CountryCodes.VietNam;

				AssertEquals("Recipient Govt Tax ID", "0100233488-999", InvoiceWrapper.RecipientGovtTaxID);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountryCode);
			}
		}

		public void TestLocalTaxMessages()
		{
			SetupCountryForTestingInvoiceTaxMessages();
			InvoicingBase invoiceBase = SetupForTestingInvoiceTaxMessagesCore("AUSYD");
			SetupForTestingInvoiceTaxMessages(invoiceBase);

			Factory.Save();

			string expectedString = @"* LOCAL 1
** LOCAL 2
*** LOCAL 3
**** LOCAL 4
***** LOCAL 5";
			AssertEquals(expectedString, GetBaseInvoiceWrapper().LocalLanguageTaxMessages);

			expectedString = @"1. LOCAL 1
2. LOCAL 2
3. LOCAL 3
4. LOCAL 4
5. LOCAL 5";

			AssertEquals(expectedString, GetBaseInvoiceWrapper().LocalLanguageTaxMessagesWithNumbers);
		}

		public void TestLocalTaxMessagesForNotReportTaxId()
		{
			SetupCountryForTestingInvoiceTaxMessages();
			InvoicingBase invoiceBase = SetupForTestingInvoiceTaxMessagesCore("AUSYD");
			SetupForTestingInvoiceTaxMessages(invoiceBase);

			invoiceBase.Lines[4].TaxRate.AT_Code = "NOTREPORT1";

			Factory.Save();

			string expectedString = @"* LOCAL 1
** LOCAL 2
*** LOCAL 3
**** LOCAL 4
***** LOCAL 5";
			AssertEquals(expectedString, GetBaseInvoiceWrapper().LocalLanguageTaxMessages);
		}

		public void TestLocalTaxMessagesReturnsEnglishMessageIfItIsEmpty()
		{
			SetupCountryForTestingInvoiceTaxMessages();
			InvoicingBase invoiceBase = SetupForTestingInvoiceTaxMessagesCore("AUSYD");
			SetupForTestingInvoiceTaxMessages(invoiceBase, true);

			Factory.Save();

			string expectedString = @"* ENGLISH 1
** ENGLISH 2
*** ENGLISH 3
**** ENGLISH 4
***** ENGLISH 5";
			AssertEquals(expectedString, GetBaseInvoiceWrapper().LocalLanguageTaxMessages);
		}

		public void TestCurrentCompanyCity()
		{
			GlbBranch glbBranch = Factory.New<GlbBranch>();
			glbBranch.GB_City = "City name test";

			Invoice = GetWrappedInvoice();
			Invoice.AH_GB = glbBranch.PK;
			InvoiceWrapper = GetBaseInvoiceWrapper();

			var currentCompanyCity = InvoiceWrapper.CurrentCompanyCity;

			Assert("The wrapper should not be null or empty", !string.IsNullOrEmpty(currentCompanyCity));
			AssertEquals("Check is the city name of TransactionHeader is the same as DocumentWrapper", "City name test", currentCompanyCity);
		}

		public virtual void TestEnglishTaxMessages()
		{
			SetupCountryForTestingInvoiceTaxMessages();
			InvoicingBase invoiceBase = SetupForTestingInvoiceTaxMessagesCore("USLAX");
			SetupForTestingInvoiceTaxMessages(invoiceBase);

			Factory.Save();

			ZString expectedString = @"* ENGLISH 1
** ENGLISH 2
*** ENGLISH 3
**** ENGLISH 4
***** ENGLISH 5";
			AssertEquals(expectedString, GetBaseInvoiceWrapper().EnglishLanguageTaxMessages);

			expectedString = @"1. ENGLISH 1
2. ENGLISH 2
3. ENGLISH 3
4. ENGLISH 4
5. ENGLISH 5";
			AssertEquals(expectedString, GetBaseInvoiceWrapper().EnglishLanguageTaxMessagesWithNumbers);
		}

		public void TestEnglishTaxMessagesForNotReportTaxId()
		{
			SetupCountryForTestingInvoiceTaxMessages();
			InvoicingBase invoiceBase = SetupForTestingInvoiceTaxMessagesCore("USLAX");
			SetupForTestingInvoiceTaxMessages(invoiceBase);

			invoiceBase.Lines[4].TaxRate.AT_Code = "NOTREPORT1";

			Factory.Save();

			ZString expectedString = @"* ENGLISH 1
** ENGLISH 2
*** ENGLISH 3
**** ENGLISH 4
***** ENGLISH 5";
			AssertEquals(expectedString, GetBaseInvoiceWrapper().EnglishLanguageTaxMessages);
		}

		public void TestTaxMessages_NOTREPORT_EU()
		{
			TestTaxMessages_NOTREPORT(Constants.CountryCodes.France, "DEDRS");
		}

		public void TestTaxMessages_NOTREPORT_NonEU()
		{
			TestTaxMessages_NOTREPORT(Constants.CountryCodes.Jamaica, "JMSAW");
		}

		void TestTaxMessages_NOTREPORT(string countryCode, string innerUnloco)
		{
			ZString originalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);
				Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).SetCountry(countryCode);

				InvoicingBase invoiceBase = SetupForTestingInvoiceTaxMessagesCore("USLAX");
				InvoicingBaseTest.SetupForTestingNOTREPORTMessagesDisplay(invoiceBase);

				Factory.Save();

				AssertEquals(@"* NOTREPORT ENGLISH MESSAGE", GetBaseInvoiceWrapper().EnglishLanguageTaxMessages);

				((ForwardingShipment)invoiceBase.Lines[1].GenericJobObject.Consumer).JS_RL_NKDestination = innerUnloco;
				if (invoiceBase.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					AssertEquals(@"* NOTREPORT ENGLISH MESSAGE", GetBaseInvoiceWrapper().EnglishLanguageTaxMessages);
				}
				else if (((ForwardingShipment)invoiceBase.Lines[1].GenericJobObject.Consumer).Destination.Country.IsPartOfEuropeanUnion)
				{
					invoiceBase.Header.CustomsCodes.AddNew(Country.GetConsumptionTaxDescription(countryCode), "ABCDEF", GlbCompany.CurrentCompany.Country);

					AssertEquals(@"* NOTREPORT ENGLISH MESSAGE", GetBaseInvoiceWrapper().EnglishLanguageTaxMessages);
				}
				else
				{
					AssertEquals(@"* NOTREPORT ENGLISH MESSAGE", GetBaseInvoiceWrapper().EnglishLanguageTaxMessages);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountryCode);
			}
		}

		public void TestTaxMessagePresentWhenItalianStampDuty()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			InvoicingBase invoice = SetupForTestingInvoiceTaxMessagesCore("ITROM");
			AccTaxRate taxRate = creator.GST1;
			AccInvMsg invoiceMessage = Factory.NewWithValidTestData<AccInvMsg>();
			invoiceMessage.A9_EnglishMsg = "ENGLISH Linked Invoice Message";
			invoiceMessage.A9_LocalMsg = "LOCAL Linked Invoice Message";
			taxRate.AT_A9_DefaultVatClass = invoiceMessage.PK;
			AccChargeCode chargeCode = creator.CreateChargeCode("BOLLO", "Stamp Duty", "NON", 1.0m, taxRate, creator.WHTFREE1);
			AccountingConfigurationRegistry.Instance.StampDutyChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AC = chargeCode.PK;
			line.AL_LocalExTaxAmount = 2500m;
			line.AL_GSTVAT = 0m;
			line.AL_OSTaxAmount = 0m;
			line.AL_AT = taxRate.PK;

			Factory.Save();

			AssertEquals("* ENGLISH Linked Invoice Message", GetBaseInvoiceWrapper().EnglishLanguageTaxMessages);

			AccountingConfigurationRegistry.Instance.StampDutyChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());
			invoice = SetupForTestingInvoiceTaxMessagesCore("ITROM");

			line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AC = chargeCode.PK;
			line.AL_LocalExTaxAmount = 2500m;
			line.AL_GSTVAT = 0m;
			line.AL_OSTaxAmount = 0m;
			line.AL_AT = taxRate.PK;
			invoice.IncludeInTheBatch = true;

			Factory.Save();

			AssertEquals("* ENGLISH Linked Invoice Message", GetBaseInvoiceWrapper().EnglishLanguageTaxMessages);
		}

		public void TestInvoiceLogo()
		{
			DocumentsDataRegistry.Instance.EnableClientBranding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, false);

			Invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;

			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Bitmap(3, 3));
			AssertEquals("Default logo", SystemDataRegistry.Instance.CompanyLogo.Value.Size, InvoiceWrapper.InvoiceLogo.Size);

			SystemDataRegistry.Instance.InvoceAndStatementLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Bitmap(2, 2));
			AssertEquals("Should return InvoiceAndStatementLogo", new Size(2, 2), InvoiceWrapper.InvoiceLogo.Size);

			SystemDataRegistry.Instance.InvoceAndStatementLogo.SetValue(Guid.Empty, Guid.Empty, GlbDepartment.CurrentDepartment.PK.ToGuid(), new Bitmap(5, 5));
			AssertEquals("Should return InvoiceAndStatementLogo", new Size(5, 5), InvoiceWrapper.InvoiceLogo.Size);

			SystemDataRegistry.Instance.InvoceAndStatementLogo.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), new Bitmap(4, 4));
			AssertEquals("Should return InvoiceAndStatementLogo", new Size(4, 4), InvoiceWrapper.InvoiceLogo.Size);

			OrgHeader client = OrgHeader.New(Factory);
			client.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			var header = Factory.NewJobForTesting<JobHeader>();
			header.LocalChargesPK = client.PK;
			Invoice.AH_JH = header.PK;

			DocumentsDataRegistry.Instance.EnableClientBranding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, true);
			ClientTariffAndLevelCollection collection = new ClientTariffAndLevelCollection();
			var element1 = collection.AddNew();
			element1.CodeList.AddPair("1", "Desc");
			element1.Code = "1";
			element1.Description = (NoResString)"Desc";
			element1.BrandName = "blah";
			element1.BrandEmailAddress = "blah@blah.com";
			element1.Image = new Bitmap(1, 1);
			DocumentsDataRegistry.Instance.ClientTariffAndLevels.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, collection);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContactOrganisationPK, client.PK.ToString());
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContactType, "CNE");
			InvoiceWrapper.SetTemplateConstants(constants);

			AssertEquals("Client logo", new Size(1, 1), InvoiceWrapper.InvoiceLogo.Size);
		}

		public void TestShowInvoiceTotalsbyTaxRate()
		{
			AssertEquals("modified", false, InvoiceWrapper.ShowInvoiceTotalsbyTaxRate);
			AccountingConfigurationRegistry.Instance.DisplayInvoiceTotalsbyTaxRate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("modified", true, InvoiceWrapper.ShowInvoiceTotalsbyTaxRate);
		}

		public void TestElectronicPayments()
		{
			AssertEquals("modified", false, InvoiceWrapper.ShowElectronicPaymentsDetails);
			AccountingConfigurationRegistry.Instance.EnableElectronicPayments.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("modified", true, InvoiceWrapper.ShowElectronicPaymentsDetails);

			AssertEquals("default", ZString.Empty, InvoiceWrapper.ElectronicPaymentBillerCode);
			AccountingConfigurationRegistry.Instance.ElectronicPaymentBillerCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345");
			AssertEquals("modified", "12345", InvoiceWrapper.ElectronicPaymentBillerCode);

			AssertEquals("default", (string)AccountingConfigurationRegistry.Instance.ElectronicPaymentTerms.Value, (string)InvoiceWrapper.ElectronicPaymentTerms);
			AccountingConfigurationRegistry.Instance.ElectronicPaymentTerms.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "blah blah");
			AssertEquals("modified", "blah blah", InvoiceWrapper.ElectronicPaymentTerms);
		}

		public void TestShowBankDetails()
		{
			Invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			AssertEquals("Show Bank Details", true, InvoiceWrapper.ShowBankDetails);

			Invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.AdjustmentNote;
			AssertEquals("Show Bank Details", true, InvoiceWrapper.ShowBankDetails);

			Invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.CreditNote;
			AssertEquals("Show Bank Details", false, InvoiceWrapper.ShowBankDetails);

			Invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.InvoiceBatch;
			AssertEquals("Show Bank Details", true, InvoiceWrapper.ShowBankDetails);
		}

		public void TestReceiptBankAccount()
		{
			AccBankAccount bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_Code = "ABC";
			bankAccount.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			Invoice.AH_OH = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			Invoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			InvoiceBatch.AH_OH = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			InvoiceBatch.Header.CompanyData.OB_AB_ARPayToAccount = bankAccount.PK;
			InvoiceBatch.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertNotNull("ARPayToAccount should not be null", Invoice.Header.CompanyData.ARPayToAccount);

			InvoiceWrapper = GetBaseInvoiceWrapper();

			DocBankAccount bankAccountWrapper = InvoiceWrapper.ReceiptBankAccount;

			AssertNotNull("Bank Account should not be null", Invoice.ReceiptBankAccount);
			AssertNotNull("Bank account wrapper should not be null", bankAccountWrapper);
			AssertEquals("Bank Account", bankAccount.AB_Code, bankAccountWrapper.Code);
		}

		public void TestBankAccount()
		{
			AssertNull("BankAccount", InvoiceWrapper.BankAccount);

			var bankAccount = Factory.New<AccBankAccount>();
			Invoice.AH_AB = bankAccount.PK;
			AssertNotNull("BankAccount", InvoiceWrapper.BankAccount);
			AssertEquals("BankAccount is of type DocBankAccount", typeof(DocBankAccount), InvoiceWrapper.BankAccount.GetType());
		}

		public void TestGLAccount()
		{
			AssertNull("GLAccount", InvoiceWrapper.GLAccount);

			var gLHeader = Factory.New<AccGLHeader>();
			Invoice.AH_AG = gLHeader.PK;
			AssertNotNull("GLAccount", InvoiceWrapper.GLAccount);
			AssertEquals("GLAccount is of type DocGLAccount", typeof(DocGLAccount), InvoiceWrapper.GLAccount.GetType());
		}

		public void TestBranch()
		{
			Invoice.AH_GB = ZGuid.Empty;
			AssertNull("Branch", InvoiceWrapper.Branch);

			var branch = Factory.New<GlbBranch>();
			Invoice.AH_GB = branch.PK;
			AssertNotNull("Branch", InvoiceWrapper.Branch);
			AssertEquals("Branch is of type DocBranch", typeof(DocBranch), InvoiceWrapper.Branch.GetType());
		}

		public void TestDepartment()
		{
			Invoice.AH_GE = ZGuid.Empty;
			AssertNull("Department", InvoiceWrapper.Department);

			var department = Factory.New<GlbDepartment>();
			Invoice.AH_GE = department.PK;
			AssertNotNull("Department", InvoiceWrapper.Department);
			AssertEquals("Department is of type DocDepartment", typeof(DocDepartment), InvoiceWrapper.Department.GetType());
		}

		public void TestAccountOrg()
		{
			AssertNull("AccountOrg", InvoiceWrapper.AccountOrg);

			var header = Factory.New<OrgHeader>();
			header.OH_Code = "ORG";
			Invoice.AH_OH = header.PK;
			AssertNotNull("AccountOrg", InvoiceWrapper.AccountOrg);
			AssertEquals("AccountOrg is of type DocOrganisation", typeof(DocOrganisation), InvoiceWrapper.AccountOrg.GetType());
		}

		public void TestAccountOrgAddress()
		{
			OrgHeader debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.OH_FullName = "DEBTOR";
			OrgAddress debtorARAddress = debtor.Addresses.AddNew(OrgAddressType.Receivables, true);
			debtorARAddress.OA_Address1 = "DEBTOR AR ADDRESS";
			OrgAddress debtorMiscAddress = debtor.Addresses.AddNew(OrgAddressType.Receivables, true);
			debtorMiscAddress.OA_Address1 = "DEBTOR MISC ADDRESS";

			OrgHeader localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_FullName = "LOCAL CLIENT";
			OrgAddress localClientARAddress = localClient.Addresses.AddNew(OrgAddressType.Receivables, true);
			localClientARAddress.OA_Address1 = "LOCAL CLIENT AR ADDRESS";
			OrgAddress localClientMiscAddress = localClient.Addresses.AddNew(OrgAddressType.Miscellaneous, true);
			localClientMiscAddress.OA_Address1 = "LOCAL CLIENT MISC ADDRESS";

			OrgHeader overseaAgent = Factory.NewWithValidTestData<OrgHeader>();
			overseaAgent.OH_FullName = "OVERSEA AGENT";
			OrgAddress overseaAgentARAddress = overseaAgent.Addresses.AddNew(OrgAddressType.Receivables, true);
			overseaAgentARAddress.OA_Address1 = "OVERSEA AGENT AR ADDRESS";

			AssertNull(InvoiceWrapper.AccountOrgAddress);

			Invoice.AH_OH = debtor.PK;
			AssertEquals("Non-Job Related Invoice. Should be Debtor's AR Address", debtorARAddress.OA_Address1, InvoiceWrapper.AccountOrgAddress.Address1);

			Job job = Factory.NewJobForTesting<Job>();
			Invoice.AH_JH = job.PK;
			AssertEquals("Job Related Invoice. Should be Debtor's AR Address", debtorARAddress.OA_Address1, InvoiceWrapper.AccountOrgAddress.Address1);

			job.JH_OA_LocalChargesAddr = localClientARAddress.PK;
			AssertEquals("Debtor != Local Client. Should be Debtor's AR Address", debtorARAddress.OA_Address1, InvoiceWrapper.AccountOrgAddress.Address1);

			debtor.OH_FullName = localClient.OH_FullName;
			AssertEquals("Debtor != Local Client, but has the same name). Should still be Debtor's AR Address",
				debtorARAddress.OA_Address1, InvoiceWrapper.AccountOrgAddress.Address1);

			Invoice.AH_OH = localClient.PK;
			job.JH_OA_LocalChargesAddr = localClientMiscAddress.PK;
			AssertEquals("Debtor == Local Client. Should be the Local Client's Address that is selected on the Job",
				localClientMiscAddress.OA_Address1, InvoiceWrapper.AccountOrgAddress.Address1);

			Invoice.AH_OH = overseaAgent.PK;
			job.JH_OA_AgentCollectAddr = overseaAgentARAddress.PK;
			AssertEquals("Debtor == Oversea Agent. Should be the Oversea Agent's Address that is selected on the Job",
				overseaAgentARAddress.OA_Address1, InvoiceWrapper.AccountOrgAddress.Address1);

			OrgAddress newAddress = Factory.NewWithValidTestData<OrgAddress>();
			Invoice.AH_OA_InvoiceAddressOverride = newAddress.PK;
			AssertEquals("Should be InvoiceAddressOverride", newAddress.PK, InvoiceWrapper.AccountOrgAddress.OrgAddress.PK);
		}

		public void TestCurrency()
		{
			Invoice.AH_RX_NKTransactionCurrency = ZString.Empty;
			AssertNull("Currency", InvoiceWrapper.Currency);

			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery());
			Invoice.AH_RX_NKTransactionCurrency = currency.RX_Code;
			AssertNotNull("Currency", InvoiceWrapper.Currency);
			AssertEquals("Currency is of type DocCurrency", typeof(DocCurrency), InvoiceWrapper.Currency.GetType());
		}

		public void TestShowFooter()
		{
			if (InvoiceWrapper is DocARInvoice)
			{
				Invoice.AH_TransactionType = TransactionTypes.CreditNote;
				AssertEquals("Show Footer for Invoice", true, InvoiceWrapper.ShowFooter);

				Invoice.AH_TransactionType = TransactionTypes.CreditNote;
				AssertEquals("Show Footer for Credit Note", true, InvoiceWrapper.ShowFooter);

				Invoice.AH_TransactionType = TransactionTypes.AdjustmentNote;
				AssertEquals("Show Footer for Adjustment Note", true, InvoiceWrapper.ShowFooter);
			}
			else
			{
				Invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.InvoiceBatch;
				AssertEquals("Show Footer for Invoice Batch", true, InvoiceWrapper.ShowFooter);
			}
		}

		public void TestInvoicePrinted()
		{
			Invoice.AH_InvoicePrinted = ZBool.False;
			Assert("!InvoicePrinted", !InvoiceWrapper.InvoicePrinted);

			Invoice.AH_InvoicePrinted = ZBool.True;
			Assert("InvoicePrinted", InvoiceWrapper.InvoicePrinted);
		}

		public void TestIsCancelled()
		{
			Invoice.AH_IsCancelled = ZBool.False;
			Assert("!IsCancelled", !InvoiceWrapper.IsCancelled);

			Invoice.AH_IsCancelled = ZBool.True;
			Assert("IsCancelled", InvoiceWrapper.IsCancelled);
		}

		public void TestIsDisbursement()
		{
			Invoice.AH_TransactionCategory = "";
			Assert("!IsDisbursement", !InvoiceWrapper.IsDisbursement);

			Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			Assert("IsDisbursement", InvoiceWrapper.IsDisbursement);
		}

		public void TestChequeDrawer()
		{
			ZString chequeDrawer = new ZString("ChequeDrawer");
			Invoice.AH_ChequeDrawer = chequeDrawer;
			AssertEquals("ChequeDrawer", chequeDrawer, InvoiceWrapper.ChequeDrawer);
		}

		public void TestChequeOrReference()
		{
			ZString chequeOrReference = new ZString("ChequeOrReference");
			Invoice.AH_ChequeOrReference = chequeOrReference;
			AssertEquals("ChequeOrReference", chequeOrReference, InvoiceWrapper.ChequeOrReference);
		}

		public void TestConsolidatedInvoiceRef()
		{
			ZString consolidatedInvoiceRef = new ZString("ConsolidatedInvoiceRef");
			Invoice.AH_ConsolidatedInvoiceRef = consolidatedInvoiceRef;
			AssertEquals("ConsolidatedInvoiceRef", consolidatedInvoiceRef, InvoiceWrapper.ConsolidatedInvoiceRef);
		}

		public void TestDesc()
		{
			ZString desc = new ZString("Desc");
			Invoice.AH_Desc = desc;
			InvoiceWrapper = GetBaseInvoiceWrapper();
			AssertEquals("Desc", desc, InvoiceWrapper.Desc);
		}

		public void TestDrawerBank()
		{
			ZString drawerBank = new ZString("DrawerBank");
			Invoice.AH_DrawerBank = drawerBank;
			AssertEquals("DrawerBank", drawerBank, InvoiceWrapper.DrawerBank);
		}

		public void TestInvoiceTerm()
		{
			ZString invoiceTerm = new ZString("TTT");
			Invoice.AH_InvoiceTerm = invoiceTerm;
			AssertEquals("InvoiceTerm", invoiceTerm, InvoiceWrapper.InvoiceTerm);
		}

		public void TestLedger()
		{
			ZString ledger = new ZString("LL");
			Invoice.AH_Ledger = ledger;
			AssertEquals("Ledger", ledger, InvoiceWrapper.Ledger);
		}

		public void TestAccountCode()
		{
			AssertEquals("AccountCode", ZString.Empty, InvoiceWrapper.AccountCode);

			var header = Factory.New<OrgHeader>();
			header.OH_Code = "ORG1";
			Invoice.AH_OH = header.PK;

			AssertEquals("AccountCode", "ORG1", InvoiceWrapper.AccountCode);

			header.OH_Code = "CCC";
			AssertEquals("AccountCode", "CCC", InvoiceWrapper.AccountCode);
		}

		public void TestAccountCodeForPortugalMissingRegistrationNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			{
				var header = Factory.New<OrgHeader>();
				header.OH_Code = "ORG1";
				header.OH_RL_NKClosestPort = "PTLIS";
				AddCustomsCodeForTest(header, Constants.CountryCodes.Portugal, "", "IVA");
				Invoice.AH_OH = header.PK;
				Factory.Save();

				if (IsTransactionEligibleForPortugalRegistrationCodeHelper())
				{
					AssertEquals("AccountCode for Portugal should be Consumidor final when registration number is null for AR INV or CRD transactions", "Consumidor final", InvoiceWrapper.AccountCode);
					AddCustomsCodeForTest(header, Constants.CountryCodes.Portugal, "123456789", "IVA");
					Factory.Save();
					AssertEquals("AccountCode for Portugal should be Consumidor final even when registration number is updated after posting for AR INV or CRD transactions", "Consumidor final", InvoiceWrapper.AccountCode);
				}
				else
				{
					AssertEquals("AccountCode for Portugal should be ORG1 even when registration number is null for Non AR INV or CRD transactions", "ORG1", InvoiceWrapper.AccountCode);
				}
			}
		}

		public void TestShowRecipientNameAndAddressForPortugalMissingRegistrationCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			{
				var header = Factory.New<OrgHeader>();
				header.OH_Code = "ORG1";
				header.OH_RL_NKClosestPort = "PTLIS";
				AddCustomsCodeForTest(header, Constants.CountryCodes.Portugal, "", "IVA");
				Invoice.AH_OH = header.PK;
				Factory.Save();

				if (IsTransactionEligibleForPortugalRegistrationCodeHelper())
				{
					AssertEquals("ShowRecipientNameAndAddress should be false when registration number is null for AR INV or CRD transactions", false, InvoiceWrapper.ShowRecipientNameAndAddress);
					AddCustomsCodeForTest(header, Constants.CountryCodes.Portugal, "123456789", "IVA");
					Factory.Save();
					AssertEquals("ShowRecipientNameAndAddress should be false even when registration number is updated after posting for AR INV or CRD transactions", false, InvoiceWrapper.ShowRecipientNameAndAddress);
				}
				else
				{
					AssertEquals("ShowRecipientNameAndAddress should be true even when registration number is null for non AR INV or CRD transactions", true, InvoiceWrapper.ShowRecipientNameAndAddress);
				}
			}
		}

		public void TestShowRecipientNameAndAddressWhenPortugalRegistrationCodeAvailable()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			{
				var header = Factory.New<OrgHeader>();
				header.OH_Code = "ORG1";
				header.OH_RL_NKClosestPort = "PTLIS";
				AddCustomsCodeForTest(header, Constants.CountryCodes.Portugal, "123456789", "IVA");
				Invoice.AH_OH = header.PK;
				Factory.Save();

				if (IsTransactionEligibleForPortugalRegistrationCodeHelper())
				{
					AssertEquals("ShowRecipientNameAndAddress should be false when registration number is null for AR INV or CRD transactions", true, InvoiceWrapper.ShowRecipientNameAndAddress);
				}
				else
				{
					AssertEquals("ShowRecipientNameAndAddress should be true even when registration number is null for non AR INV or CRD transactions", true, InvoiceWrapper.ShowRecipientNameAndAddress);
				}
			}
		}

		public void TestShowRecipientNameAndAddressForChileMissingRegistrationCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Chile))
			{
				var header = Factory.New<OrgHeader>();
				header.OH_Code = "ORG1";
				AddCustomsCodeForTest(header, Constants.CountryCodes.Chile, "", "IVA");
				Invoice.AH_OH = header.PK;
				Factory.Save();

				AssertEquals("ShowRecipientNameAndAddress", true, InvoiceWrapper.ShowRecipientNameAndAddress);
			}
		}

		bool IsTransactionEligibleForPortugalRegistrationCodeHelper()
		{
			return Invoice.AH_Ledger == LedgerTypes.AccountsReceivable && Invoice.AH_TransactionType == TransactionTypes.Invoice || Invoice.AH_TransactionType == TransactionTypes.CreditNote;
		}

		public void TestAccountName()
		{
			AssertEquals("AccountName", ZString.Empty, InvoiceWrapper.AccountName);

			var header = Factory.New<OrgHeader>();
			header.OH_Code = "ORG1";
			Invoice.AH_OH = header.PK;

			AssertEquals("AccountName", ZString.Empty, InvoiceWrapper.AccountName);

			header.OH_FullName = "Planet Express";
			AssertEquals("AccountName", "Planet Express", InvoiceWrapper.AccountName);

			header.MainAddress.OA_CompanyNameOverride = "Martians are coming";
			AssertEquals("AccountName", "Planet Express", InvoiceWrapper.AccountName);
		}

		public void TestAccountFullName()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "ORG1";
			org.OH_FullName = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the";

			Invoice.AH_OH = org.PK;
			Assert("Precondition: OH_FullName has more than 50 characters", org.OH_FullName.Length > 50);
			Assert("AccountFullName returns full OH_FullName without any truncation", InvoiceWrapper.AccountFullName.Length > 50);
			AssertEquals(org.OH_FullName, InvoiceWrapper.AccountFullName);
		}

		public virtual void TestOrganisationARAgreedPaymentMethod()
		{
			AssertEquals(ZString.Empty, Invoice.AH_AgreedPaymentMethodOverride);
			AssertEquals("No payment method found", ZString.Empty, InvoiceWrapper.OrganisationARAgreedPaymentMethod);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var companyData = orgHeader.CompanyData;
			Factory.Save();
			Invoice.AH_OH = orgHeader.PK;
			AssertEquals(ZString.Empty, Invoice.AH_AgreedPaymentMethodOverride);
			AssertEquals("Defaults to payment method of the organization", ZString.Empty, InvoiceWrapper.OrganisationARAgreedPaymentMethod);

			companyData.OB_ARCreditAgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck;
			Factory.Save();
			AssertEquals(ZString.Empty, Invoice.AH_AgreedPaymentMethodOverride);
			AssertEquals("The AR Agreed Payment Method will refer to AH_AgreedPaymentMethod", ZString.Empty, InvoiceWrapper.OrganisationARAgreedPaymentMethod);

			Invoice.AH_AgreedPaymentMethodOverride = OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck;
			Factory.Save();
			AssertEquals("The AR Agreed Payment Method will refer to AH_AgreedPaymentMethod", OrgDescriptions.CreditAgreedPaymentMethods.BusinessCheck, InvoiceWrapper.OrganisationARAgreedPaymentMethod);
		}

		public void TestOrganisationAPAgreedPaymentMethod()
		{
			AssertEquals(ZString.Empty, Invoice.AH_AgreedPaymentMethodOverride);
			AssertEquals("No payment method found", ZString.Empty, InvoiceWrapper.OrganisationAPAgreedPaymentMethod);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var companyData = orgHeader.CompanyData;
			Factory.Save();
			Invoice.AH_OH = orgHeader.PK;
			AssertEquals(ZString.Empty, Invoice.AH_AgreedPaymentMethodOverride);
			AssertEquals("Defaults to payment method of the organization", ZString.Empty, InvoiceWrapper.OrganisationAPAgreedPaymentMethod);

			companyData.OB_APCreditAgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck;
			Factory.Save();
			AssertEquals(ZString.Empty, Invoice.AH_AgreedPaymentMethodOverride);
			AssertEquals("The AP Agreed Payment Method will refer to AH_AgreedPaymentMethod", ZString.Empty, InvoiceWrapper.OrganisationAPAgreedPaymentMethod);

			Invoice.AH_AgreedPaymentMethodOverride = OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck;
			Factory.Save();
			AssertEquals("The AP Agreed Payment Method will refer to AH_AgreedPaymentMethod", OrgDescriptions.CreditAgreedPaymentMethods.BusinessCheck, InvoiceWrapper.OrganisationARAgreedPaymentMethod);
		}

		public void TestReceiptBatchNo()
		{
			ZString receiptBatchNo = new ZString("ReceiptBatchNo");
			Invoice.AH_ReceiptBatchNo = receiptBatchNo;
			AssertEquals("ReceiptBatchNo", receiptBatchNo, InvoiceWrapper.ReceiptBatchNo);
		}

		public void TestReceiptType()
		{
			ZString receiptType = new ZString("RRR");
			Invoice.AH_ReceiptType = receiptType;
			AssertEquals("ReceiptType", receiptType, InvoiceWrapper.ReceiptType);
		}

		public void TestTransactionNumber()
		{
			ZString transactionNumber = new ZString("TransactionNum");
			Invoice.AH_TransactionNum = transactionNumber;
			AssertEquals("TransactionNumber", transactionNumber, InvoiceWrapper.TransactionNumber);
		}

		public void TestTransactionReference()
		{
			ZString transactionReference = new ZString("TransactionReference");
			Invoice.AH_TransactionReference = transactionReference;
			AssertEquals("TransactionReference", transactionReference, InvoiceWrapper.TransactionReference);
		}

		public void TestTransactionType()
		{
			ZString transactionType = new ZString("TTT");
			Invoice.AH_TransactionType = transactionType;
			AssertEquals("TransactionType", transactionType, InvoiceWrapper.TransactionType);
		}

		#region TestRecipientTaxIDNumberInRecipientCountry

		public void TestRecipientTaxIDNumberInRecipientCountryForBrexit()
		{
			var header = Factory.New<OrgHeader>();
			Invoice.AH_OH = header.PK;
			var ukAddress = Factory.New<OrgAddress>();
			ukAddress.OA_Address1 = "UK Test address";
			ukAddress.OA_RN_NKCountryCode = Constants.CountryCodes.UnitedKingdom;
			AddCustomsCodeForTest(header, Constants.CountryCodes.UnitedKingdom, "111111_GB", Country.GetConsumptionTaxDescription(Core.Constants.CountryCodes.UnitedKingdom), ukAddress);

			var gbCountry = RefCountry.LoadFromCountryCode(new BusinessObjectFactory(), Constants.CountryCodes.UnitedKingdom);
			gbCountry.RN_EconomicGrouping = EconomicGroupList.Codes.EuropeanUnion;
			gbCountry.Factory.Save();
			AssertRecipientTaxIDNumberInRecipientCountry(header, "GBLON", true, ZGuid.Empty, "GB111111_GB", ukAddress);

			gbCountry.RN_EconomicGrouping = "";
			gbCountry.Factory.Save();
			AssertRecipientTaxIDNumberInRecipientCountry(header, "GBLON", true, ZGuid.Empty, "GB111111_GB", ukAddress);
		}

		public void TestRecipientTaxIDNumberInRecipientCountry()
		{
			ZString storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				var header = Factory.New<OrgHeader>();
				Invoice.AH_OH = header.PK;

				var chinaAddress = Factory.New<OrgAddress>();
				chinaAddress.OA_Address1 = "CH Test address";
				chinaAddress.OA_RN_NKCountryCode = Constants.CountryCodes.China;
				var ukAddress = Factory.New<OrgAddress>();
				ukAddress.OA_Address1 = "UK Test address";
				ukAddress.OA_RN_NKCountryCode = Constants.CountryCodes.UnitedKingdom;
				var brazilAddress = Factory.New<OrgAddress>();
				brazilAddress.OA_Address1 = "BR Test address";
				brazilAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Brazil;
				var argentinaAddress = Factory.New<OrgAddress>();
				argentinaAddress.OA_Address1 = "AR Test address";
				argentinaAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Argentina;
				var indiaAddress = Factory.New<OrgAddress>();
				indiaAddress.OA_Address1 = "IN Test address";
				indiaAddress.OA_RN_NKCountryCode = Constants.CountryCodes.India;
				var chileAddress = Factory.New<OrgAddress>();
				chileAddress.OA_Address1 = "CL Test address";
				chileAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Chile;
				AddCustomsCodeForTest(header, Constants.CountryCodes.China, "111111_CN", "VAT", chinaAddress);
				AddCustomsCodeForTest(header, Constants.CountryCodes.UnitedKingdom, "111111_GB", "VAT", ukAddress);
				AddCustomsCodeForTest(header, Constants.CountryCodes.Brazil, "111111_BR", "CMT", brazilAddress);
				AddCustomsCodeForTest(header, Constants.CountryCodes.Argentina, "111111_AR", "IVA", argentinaAddress);
				AddCustomsCodeForTest(header, Constants.CountryCodes.India, "111111_IN", "SER", indiaAddress);
				AddCustomsCodeForTest(header, Constants.CountryCodes.Chile, "111111_CL", "IVA", chileAddress);

				var address = Factory.NewWithValidTestData<OrgAddress>();
				address.OA_RN_NKCountryCode = Constants.CountryCodes.China;
				AssertRecipientTaxIDNumberInRecipientCountry(header, "GBLON", true, address.PK, "111111_CN", chinaAddress);
				AssertRecipientTaxIDNumberInRecipientCountry(header, "GBLON", false, address.PK, "GB111111_GB", ukAddress);
				AssertRecipientTaxIDNumberInRecipientCountry(header, "GBLON", true, ZGuid.Empty, "GB111111_GB", ukAddress);

				var address1 = Factory.NewWithValidTestData<OrgAddress>();
				address1.OA_RN_NKCountryCode = Constants.CountryCodes.Brazil;
				AssertRecipientTaxIDNumberInRecipientCountry(header, "ARABA", true, address1.PK, "111111_BR", brazilAddress);
				AssertRecipientTaxIDNumberInRecipientCountry(header, "ARABA", false, address1.PK, "111111_AR", argentinaAddress);
				AssertRecipientTaxIDNumberInRecipientCountry(header, "ARABA", true, ZGuid.Empty, "111111_AR", argentinaAddress);

				var address2 = Factory.NewWithValidTestData<OrgAddress>();
				address2.OA_RN_NKCountryCode = Constants.CountryCodes.Argentina;
				AssertRecipientTaxIDNumberInRecipientCountry(header, "BR6MO", true, address2.PK, "111111_AR", argentinaAddress);
				AssertRecipientTaxIDNumberInRecipientCountry(header, "BR6MO", false, address2.PK, "111111_BR", brazilAddress);
				AssertRecipientTaxIDNumberInRecipientCountry(header, "BR6MO", true, ZGuid.Empty, "111111_BR", brazilAddress);

				var address3 = Factory.NewWithValidTestData<OrgAddress>();
				address3.OA_RN_NKCountryCode = Constants.CountryCodes.India;
				AssertRecipientTaxIDNumberInRecipientCountry(header, "CLA2T", true, address3.PK, "111111_IN", indiaAddress);
				AssertRecipientTaxIDNumberInRecipientCountry(header, "CLA2T", false, address3.PK, "111111_CL", chileAddress);
				AssertRecipientTaxIDNumberInRecipientCountry(header, "CLA2T", true, ZGuid.Empty, "111111_CL", chileAddress);

				var address4 = Factory.NewWithValidTestData<OrgAddress>();
				address4.OA_RN_NKCountryCode = Constants.CountryCodes.Chile;
				AssertRecipientTaxIDNumberInRecipientCountry(header, "IN5PA", true, address4.PK, "111111_CL", chileAddress);
				AssertRecipientTaxIDNumberInRecipientCountry(header, "IN5PA", false, address4.PK, "111111_IN", indiaAddress);
				AssertRecipientTaxIDNumberInRecipientCountry(header, "IN5PA", true, ZGuid.Empty, "111111_IN", indiaAddress);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(storedCountry);
			}
		}

		public void TestRecipientTaxIDNumberInRecipientCountry_PortugalUnknownTaxID()
		{
			if (InvoicingBase is ARInvoice)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
				{
					var header = Factory.New<OrgHeader>();
					InvoicingBase.AH_OH = header.PK;
					InvoiceBatch.AH_OH = header.PK;
					// MAke sure it is taxed
					var otherTaxRate = Factory.New<AccTaxRate>();
					otherTaxRate.AT_RN_NKCountry = Constants.CountryCodes.Portugal;
					var otherLine = (AccTransactionLines)InvoicingBase.Lines.AddNew();
					otherLine.AL_AT = otherTaxRate.PK;
					InvoiceBatch.Line.Add(InvoicingBase);

					var portugalAddress = Factory.New<OrgAddress>();
					portugalAddress.OA_Address1 = "P Test address";
					portugalAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Portugal;
					var spainAddress = Factory.New<OrgAddress>();
					spainAddress.OA_Address1 = "S Test address";
					spainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Spain;
					var mexicoAddress = Factory.New<OrgAddress>();
					mexicoAddress.OA_Address1 = "M Test address";
					mexicoAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Mexico;
					var ptCode = AddCustomsCodeForTest(header, Constants.CountryCodes.Portugal, "123456789", "IVA", portugalAddress);
					var esCode = AddCustomsCodeForTest(header, Constants.CountryCodes.Spain, "987654321", "NIF", spainAddress);
					var mxCode = AddCustomsCodeForTest(header, Constants.CountryCodes.Mexico, "231231231", "IVA", mexicoAddress);

					var address = Factory.NewWithValidTestData<OrgAddress>();
					address.OA_RN_NKCountryCode = Constants.CountryCodes.Portugal;
					AssertRecipientTaxIDNumberInRecipientCountry(header, "PTLIS", true, address.PK, "PT123456789", portugalAddress);
					AssertRecipientTaxIDNumberInRecipientCountry(header, "PTLIS", false, address.PK, "PT123456789", portugalAddress);
					AssertRecipientTaxIDNumberInRecipientCountry(header, "PTLIS", true, ZGuid.Empty, "PT123456789", portugalAddress);

					var address1 = Factory.NewWithValidTestData<OrgAddress>();
					address1.OA_RN_NKCountryCode = Constants.CountryCodes.Spain;
					AssertRecipientTaxIDNumberInRecipientCountry(header, "ESMAD", true, address1.PK, "ES987654321", spainAddress);
					AssertRecipientTaxIDNumberInRecipientCountry(header, "ESMAD", false, address1.PK, "ES987654321", spainAddress);
					AssertRecipientTaxIDNumberInRecipientCountry(header, "ESMAD", true, ZGuid.Empty, "ES987654321", spainAddress);

					var address2 = Factory.NewWithValidTestData<OrgAddress>();
					address2.OA_RN_NKCountryCode = Constants.CountryCodes.Mexico;
					AssertRecipientTaxIDNumberInRecipientCountry(header, "MXMEX", true, address2.PK, "231231231", mexicoAddress);
					AssertRecipientTaxIDNumberInRecipientCountry(header, "MXMEX", false, address2.PK, "231231231", mexicoAddress);
					AssertRecipientTaxIDNumberInRecipientCountry(header, "MXMEX", true, ZGuid.Empty, "231231231", mexicoAddress);

					ptCode.OK_CustomsRegNo = "999999990";
					AssertRecipientTaxIDNumberInRecipientCountry(header, "PTLIS", true, address.PK, "", null);
					AssertRecipientTaxIDNumberInRecipientCountry(header, "PTLIS", false, address.PK, "", null);
					AssertRecipientTaxIDNumberInRecipientCountry(header, "PTLIS", true, ZGuid.Empty, "", null);

					AssertRecipientTaxIDNumberInRecipientCountry(header, "ESMAD", true, address1.PK, "ES987654321", spainAddress);
					AssertRecipientTaxIDNumberInRecipientCountry(header, "ESMAD", false, address1.PK, "ES987654321", spainAddress);
					AssertRecipientTaxIDNumberInRecipientCountry(header, "ESMAD", true, ZGuid.Empty, "ES987654321", spainAddress);

					AssertRecipientTaxIDNumberInRecipientCountry(header, "MXMEX", true, address2.PK, "231231231", mexicoAddress);
					AssertRecipientTaxIDNumberInRecipientCountry(header, "MXMEX", false, address2.PK, "231231231", mexicoAddress);
					AssertRecipientTaxIDNumberInRecipientCountry(header, "MXMEX", true, ZGuid.Empty, "231231231", mexicoAddress);

					ptCode.Delete();
					AssertRecipientTaxIDNumberInRecipientCountry(header, "PTLIS", true, address.PK, "", null);
					AssertRecipientTaxIDNumberInRecipientCountry(header, "PTLIS", false, address.PK, "", null);
					AssertRecipientTaxIDNumberInRecipientCountry(header, "PTLIS", true, ZGuid.Empty, "", null);

					AssertRecipientTaxIDNumberInRecipientCountry(header, "ESMAD", true, address1.PK, "ES987654321", spainAddress);
					AssertRecipientTaxIDNumberInRecipientCountry(header, "ESMAD", false, address1.PK, "ES987654321", spainAddress);
					AssertRecipientTaxIDNumberInRecipientCountry(header, "ESMAD", true, ZGuid.Empty, "ES987654321", spainAddress);

					AssertRecipientTaxIDNumberInRecipientCountry(header, "MXMEX", true, address2.PK, "231231231", mexicoAddress);
					AssertRecipientTaxIDNumberInRecipientCountry(header, "MXMEX", false, address2.PK, "231231231", mexicoAddress);
					AssertRecipientTaxIDNumberInRecipientCountry(header, "MXMEX", true, ZGuid.Empty, "231231231", mexicoAddress);
				}
			}
			else
			{
				Assert(true);
			}
		}

		void AssertRecipientTaxIDNumberInRecipientCountry(OrgHeader header, string closestPort, bool isGlobalAccount, ZGuid addressPK, string expectedValue, OrgAddress expectedAddress)
		{
			header.OH_RL_NKClosestPort = closestPort;
			header.OH_IsGlobalAccount = isGlobalAccount;
			header.ResetCodeForTaxRegistration_ForTestOnly();
			Invoice.AH_OA_InvoiceAddressOverride = addressPK;
			InvoiceWrapper = GetBaseInvoiceWrapper();
			AssertEquals(expectedValue, InvoiceWrapper.RecipientTaxIDNumberInRecipientCountry);
			var expectedDocAddress = expectedAddress != null ? DocAddress.New(Factory.Load<OrgAddress>(expectedAddress.PK), Factory) : null;
			AssertEquals(expectedDocAddress?.ToString(), InvoiceWrapper.RecipientTaxIDPremisesAddressInRecipientCountry?.ToString());
		}

		OrgCusCode AddCustomsCodeForTest(OrgHeader header, ZString countryCode, ZString customsRegNo, ZString codeType, OrgAddress premisesAddress = null)
		{
			if (countryCode.IsEmpty || customsRegNo.IsEmpty || codeType.IsEmpty)
			{
				return null;
			}

			var taxCode = header.CustomsCodes.AddNew();
			taxCode.OK_RN_NKCodeCountry = countryCode;
			taxCode.OK_CustomsRegNo = customsRegNo;
			taxCode.OK_CodeType = codeType;
			if (premisesAddress != null)
			{
				taxCode.OK_OA_PremisesAddress = premisesAddress.PK;
			}
			return taxCode;
		}

		#endregion

		#region TestRecipientTaxIDHeadingInRecipientCountry

		[ExpectNoExceptions]
		public void TestGetICountryComplianceInfo_DifferentCountryCodePassed_WhenRecipientTaxIDHeadingCalled()
		{
			var countryCode = Constants.CountryCodes.Australia;
			GlbCompany.CurrentCompany.SetCountry(countryCode);

			var mockComplianceFactory = new Mock<ICountryComplianceFactoryIntegration>();
			ObjectFactory.Substitute(mockComplianceFactory.Object);

			AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var header = Factory.New<OrgHeader>();
			Invoice.AH_OH = header.PK;
			InvoiceBatch = Factory.New<InvoiceBatchHeader>();
			InvoiceBatch.AH_OH = header.PK;

			var transactionLine = InvoicingBase.Lines.AddNew();
			transactionLine.AL_AT = Factory.New<AccTaxRate>().PK;

			InvoiceBatch.Line.Add(InvoicingBase);

			InvoiceWrapper = GetBaseInvoiceWrapper();
			var recipientTaxIDHeading = InvoiceWrapper.RecipientTaxIDHeading;
			mockComplianceFactory.Verify(x => x.GetICountryComplianceInfo(countryCode));

			countryCode = Constants.CountryCodes.Spain;
			GlbCompany.CurrentCompany.SetCountry(countryCode);
			recipientTaxIDHeading = InvoiceWrapper.RecipientTaxIDHeading;
			mockComplianceFactory.Verify(x => x.GetICountryComplianceInfo(countryCode));
		}

		[ExpectNoExceptions]
		public void TestRecipientTaxIDHeading_GetRecipientTaxIDHeading_NotCalledWhenIsNotTaxed()
		{
			var mockCountryComplianceInfo = GetCountryComplianceInfoMock();
			AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var header = Factory.New<OrgHeader>();
			Invoice.AH_OH = header.PK;
			InvoiceBatch = Factory.New<InvoiceBatchHeader>();
			InvoiceBatch.AH_OH = header.PK;

			AssertEquals("Precondition: IsTaxed", false, InvoiceWrapper.IsTaxed);
			var recipientTaxIDHeading = InvoiceWrapper.RecipientTaxIDHeading;
			mockCountryComplianceInfo.Verify(x => x.GetRecipientTaxIDHeading(), Times.Never());

			var transactionLine = InvoicingBase.Lines.AddNew();
			transactionLine.AL_AT = Factory.New<AccTaxRate>().PK;

			InvoiceBatch.Line.Add(InvoicingBase);
			InvoiceWrapper = GetBaseInvoiceWrapper();

			recipientTaxIDHeading = InvoiceWrapper.RecipientTaxIDHeading;
			mockCountryComplianceInfo.Verify(x => x.GetRecipientTaxIDHeading(), Times.Once());
		}

		[ExpectNoExceptions]
		public void TestRecipientTaxIDHeading_GetRecipientTaxIDHeading_NotCalledWhenDisplayRecipientTaxIDIsFalse()
		{
			var mockCountryComplianceInfo = GetCountryComplianceInfoMock();
			AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			var header = Factory.New<OrgHeader>();
			Invoice.AH_OH = header.PK;
			InvoiceBatch = Factory.New<InvoiceBatchHeader>();
			InvoiceBatch.AH_OH = header.PK;

			var transactionLine = InvoicingBase.Lines.AddNew();
			transactionLine.AL_AT = Factory.New<AccTaxRate>().PK;

			InvoiceBatch.Line.Add(InvoicingBase);

			InvoiceWrapper = GetBaseInvoiceWrapper();

			Assert("Precondition: IsTaxed", InvoiceWrapper.IsTaxed);

			var recipientTaxIDHeading = InvoiceWrapper.RecipientTaxIDHeading;
			mockCountryComplianceInfo.Verify(x => x.GetRecipientTaxIDHeading(), Times.Never());

			AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			recipientTaxIDHeading = InvoiceWrapper.RecipientTaxIDHeading;
			mockCountryComplianceInfo.Verify(x => x.GetRecipientTaxIDHeading(), Times.Once());
		}

		public void TestRecipientTaxIDHeading_UsesDisplayRecipientTaxIDHeadingRegistryValue()
		{
			var recipientTaxIDHeadingText = "testHeading";
			var registryRecipientTaxIDHeadingText = "registryRecipientTaxIDHeading";

			var mockCountryComplianceInfo = GetCountryComplianceInfoMock();
			mockCountryComplianceInfo.Setup(x => x.GetRecipientTaxIDHeading()).Returns(recipientTaxIDHeadingText);

			AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.DisplayRecipientTaxIDHeading.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryRecipientTaxIDHeadingText);

			var header = Factory.New<OrgHeader>();
			Invoice.AH_OH = header.PK;
			InvoiceBatch = Factory.New<InvoiceBatchHeader>();
			InvoiceBatch.AH_OH = header.PK;

			var transactionLine = InvoicingBase.Lines.AddNew();
			transactionLine.AL_AT = Factory.New<AccTaxRate>().PK;

			InvoiceBatch.Line.Add(InvoicingBase);
			InvoiceWrapper = GetBaseInvoiceWrapper();

			var recipientTaxIDHeading = InvoiceWrapper.RecipientTaxIDHeading;
			AssertEquals("Recipient Tax ID Heading", registryRecipientTaxIDHeadingText, recipientTaxIDHeading);
		}

		public void TestRecipientTaxIDHeading_UsesGetRecipientTaxIDHeading_DisplayRecipientTaxIDHeadingRegistryValueIsNotSet()
		{
			var recipientTaxIDHeadingText = "testHeading";

			var mockCountryComplianceInfo = GetCountryComplianceInfoMock();
			mockCountryComplianceInfo.Setup(x => x.GetRecipientTaxIDHeading()).Returns(recipientTaxIDHeadingText);

			AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.DisplayRecipientTaxIDHeading.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, null);

			var header = Factory.New<OrgHeader>();
			Invoice.AH_OH = header.PK;
			InvoiceBatch = Factory.New<InvoiceBatchHeader>();
			InvoiceBatch.AH_OH = header.PK;

			var transactionLine = InvoicingBase.Lines.AddNew();
			transactionLine.AL_AT = Factory.New<AccTaxRate>().PK;

			InvoiceBatch.Line.Add(InvoicingBase);
			InvoiceWrapper = GetBaseInvoiceWrapper();

			var recipientTaxIDHeading = InvoiceWrapper.RecipientTaxIDHeading;
			AssertEquals("Recipient Tax ID Heading", recipientTaxIDHeadingText, recipientTaxIDHeading);
		}

		#endregion

		public void TestRecipientTaxIDWithoutTax()
		{
			ZString storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				OrgHeader header = Factory.New<OrgHeader>();
				header.MiscServ.OM_ARDontShowTaxOnDocs = ZBool.True;

				OrgCusCode taxCode = header.CustomsCodes.AddNew();
				taxCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Netherlands;
				taxCode.OK_CodeType = ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(Core.Constants.CountryCodes.Netherlands);
				taxCode.OK_CustomsRegNo = "123456";

				taxCode = header.CustomsCodes.AddNew();
				taxCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
				taxCode.OK_CodeType = ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(Core.Constants.CountryCodes.Australia);
				taxCode.OK_CustomsRegNo = "654321";

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Netherlands);
				AssertEquals("PreCondition: Local VAT Code", "123456", header.RawTaxRegistrationNumber);
				AssertEquals("PreCondition: is not taxed", ZBool.True, header.MiscServ.OM_ARDontShowTaxOnDocs);

				Invoice.AH_OH = header.PK;
				InvoiceBatch = Factory.New<InvoiceBatchHeader>();
				InvoiceBatch.AH_OH = header.PK;

				var transactionLine = InvoicingBase.Lines.AddNew();
				transactionLine.AL_AT = Factory.New<AccTaxRate>().PK;

				InvoiceBatch.Line.Add(InvoicingBase);

				Invoice = GetWrappedInvoice();
				InvoiceWrapper = GetBaseInvoiceWrapper();

				AssertEquals("Recipient Tax ID Heading when Current Company is Netherlands", "Client VAT #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number when Current Company is Netherlands", "NL123456", InvoiceWrapper.RecipientTaxIDNumber);

				AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				AssertEquals("Recipient Tax ID Heading", "", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number", "", InvoiceWrapper.RecipientTaxIDNumber);

				AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AssertEquals("Recipient Tax ID Heading", "Client VAT #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number", "NL123456", InvoiceWrapper.RecipientTaxIDNumber);

				AccountingConfigurationRegistry.Instance.DisplayRecipientTaxIDHeading.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "DefaultHeading");
				AssertEquals("Recipient Tax ID Heading", "DefaultHeading", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number", "NL123456", InvoiceWrapper.RecipientTaxIDNumber);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				header.ResetCodeForTaxRegistration_ForTestOnly();
				AccountingConfigurationRegistry.Instance.DisplayRecipientTaxIDHeading.Inner.DeleteValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.Inner.DeleteValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				AssertEquals("654321", header.RawTaxRegistrationNumber);
				AssertEquals("Recipient Tax ID Heading when Current Company is Australia", ZString.Empty, InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number when Current Company is Australia", ZString.Empty, InvoiceWrapper.RecipientTaxIDNumber);

				AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				AssertEquals("Recipient Tax ID Heading", "", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number", "", InvoiceWrapper.RecipientTaxIDNumber);

				AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AssertEquals("Recipient Tax ID Heading", "Client ABN #", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number", "654321", InvoiceWrapper.RecipientTaxIDNumber);

				AccountingConfigurationRegistry.Instance.DisplayRecipientTaxIDHeading.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "DefaultHeading");
				AssertEquals("Recipient Tax ID Heading", "DefaultHeading", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number", "654321", InvoiceWrapper.RecipientTaxIDNumber);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(storedCountry);
			}
		}

		public virtual void TestRecipientTaxIDForEUWithEmptyCode()
		{
			AssertRecipientTaxId(Core.Constants.CountryCodes.Austria, "", "", "Client VAT #:", "", "DEBER");
			AssertRecipientTaxId(Core.Constants.CountryCodes.Netherlands, "", "", "", "", "DEBER");
		}

		public void TestRecipientTaxIDForBrexit()
		{
			var gbCountry = RefCountry.LoadFromCountryCode(new BusinessObjectFactory(), Constants.CountryCodes.UnitedKingdom);
			gbCountry.RN_EconomicGrouping = EconomicGroupList.Codes.EuropeanUnion;
			gbCountry.Factory.Save();
			AssertRecipientTaxId(Core.Constants.CountryCodes.UnitedKingdom, Country.GetConsumptionTaxDescription(Core.Constants.CountryCodes.UnitedKingdom), "345612", "Client VAT #:", "GB345612", "GBLON");

			gbCountry.RN_EconomicGrouping = "";
			gbCountry.Factory.Save();
			AssertRecipientTaxId(Core.Constants.CountryCodes.UnitedKingdom, Country.GetConsumptionTaxDescription(Core.Constants.CountryCodes.UnitedKingdom), "345612", "Client VAT #:", "GB345612", "GBLON");
		}

		public void TestRecipientTaxID()
		{
			ZString storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				OrgHeader header = Factory.New<OrgHeader>();
				header.OH_RL_NKClosestPort = "GBLON";
				header.LocalBusinessRegNo = "ABC123";
				OrgCusCode taxCode = header.CustomsCodes.AddNew();
				taxCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
				taxCode.OK_CodeType = Country.GetConsumptionTaxDescription(Core.Constants.CountryCodes.UnitedKingdom);
				taxCode.OK_CustomsRegNo = "123456";

				AssertEquals("PreCondition: Local Business Reg Number", "ABC123", header.LocalBusinessRegNo);

				taxCode = header.CustomsCodes.AddNew();
				taxCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
				taxCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
				taxCode.OK_CustomsRegNo = "VAT12345";

				Invoice.AH_OH = header.PK;
				InvoiceBatch = Factory.New<InvoiceBatchHeader>();
				InvoiceBatch.AH_OH = header.PK;

				var transactionLine = InvoicingBase.Lines.AddNew();
				transactionLine.AL_AT = Factory.New<AccTaxRate>().PK;

				InvoiceBatch.Line.Add(InvoicingBase);

				InvoiceWrapper = GetBaseInvoiceWrapper();

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				AssertEquals("Recipient Tax ID Heading when not a South African Company", ZString.Empty, InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number when not a South African Company", ZString.Empty, InvoiceWrapper.RecipientTaxIDNumber);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.SouthAfrica);
				AssertEquals("Recipient Tax ID Heading for a South African Company", "Client VAT #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number for a South African Company", header.LocalBusinessRegNo, InvoiceWrapper.RecipientTaxIDNumber);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Morocco);
				AssertEquals("Recipient Tax ID Heading for a Moroccan Company", "Client VAT #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number for a Moroccan Company", header.LocalBusinessRegNo, InvoiceWrapper.RecipientTaxIDNumber);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
				header.ResetCodeForTaxRegistration_ForTestOnly();
				AssertEquals("Recipient Tax ID Heading for a Taiwan Company", "Client Tax #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number for a Taiwan Company", header.LocalVATCode, InvoiceWrapper.RecipientTaxIDNumber);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedKingdom);
				header.ResetCodeForTaxRegistration_ForTestOnly();
				AssertEquals("Recipient Tax ID Heading for a European Union Company", "Client VAT #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number for a European Union Company", "GB123456", InvoiceWrapper.RecipientTaxIDNumber);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Germany);
				header.ResetCodeForTaxRegistration_ForTestOnly();
				AssertEquals("Recipient Tax ID Heading for a European Union Company", "Client VAT ID No:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number for a European Union Company", "GB123456", InvoiceWrapper.RecipientTaxIDNumber);

				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Bangladesh);
				header.ResetCodeForTaxRegistration_ForTestOnly();
				AssertEquals("Recipient Tax ID Heading for a Bangladesh Company", "Client BIN #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax Id Number for a Bangladesh Company", header.LocalVATCode, InvoiceWrapper.RecipientTaxIDNumber);

				using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.Italian))
				{
					GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Italy);
					header.ResetCodeForTaxRegistration_ForTestOnly();
					AssertEquals("Recipient Tax ID Heading for a European Union Company", "P.IVA cliente:", InvoiceWrapper.RecipientTaxIDHeading);
				}

				AssertRecipientTaxId(Core.Constants.CountryCodes.Philippines, Country.GetConsumptionTaxDescription(Core.Constants.CountryCodes.Philippines), "789012", "TIN:", "789012");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Maldives, OrgCusCode.MaldivesCodeTypes.TIN, "799878", "TIN:", "799878");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Tonga, OrgCusCode.TongaCodeTypes.TIN, "345345", "TIN:", "345345");
				AssertRecipientTaxId(Core.Constants.CountryCodes.SriLanka, Country.GetConsumptionTaxDescription(Core.Constants.CountryCodes.SriLanka), "abc123456", "Client VAT #:", "abc123456");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Ethiopia, Country.GetConsumptionTaxDescription(Core.Constants.CountryCodes.Ethiopia), "abc123456", "Client VAT #:", "abc123456");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Uganda, Country.GetConsumptionTaxDescription(Core.Constants.CountryCodes.Uganda), "xyz3434343", "Client VAT #:", "xyz3434343");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Peru, OrgCusCode.PeruCodeTypes.GovernmentTaxFileCode, "135246", "R.U.C.", "135246");
				AssertRecipientTaxId(Core.Constants.CountryCodes.China, Country.GetConsumptionTaxDescription(Core.Constants.CountryCodes.China), "123123123", "VAT", "123123123");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Netherlands, OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, "555666", "Client VAT #:", "NL555666");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Norway, Country.GetConsumptionTaxDescription(Core.Constants.CountryCodes.Norway), "345612", "Client VAT #:", "NO345612");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Turkey, Country.GetConsumptionTaxDescription(Core.Constants.CountryCodes.Turkey), "345612", "CLIENT TIN:", "345612");
				AssertRecipientTaxId(Core.Constants.CountryCodes.CostaRica, Country.GetConsumptionTaxRegistrationOrgCusCode(Core.Constants.CountryCodes.CostaRica), "444555", "CÉD. JURÍDICA #", "444555");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Colombia, ColombiaOrgCusCodeInfo.OrgCusCodes.NIT, "458797", "Client NIT #", "458797");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Guatemala, OrgCusCode.GuatemalaCodeTypes.NumeroDeIdentificacionTributaria, "453678", "Client NIT #", "453678");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Venezuela, OrgCusCode.VenezuelaCodeTypes.RegistroUnicoDeInformacionFiscal, "948738", "Client RIF #", "948738");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Ecuador, OrgCusCode.EcuadorCodeTypes.RUC, "568771", "Client RUC #", "568771");
				AssertRecipientTaxId(Core.Constants.CountryCodes.ElSalvador, ElSalvadorOrgCusCodeInfo.OrgCusCodes.NRC, "940584", "Client NRC #", "940584");
				AssertRecipientTaxId(Core.Constants.CountryCodes.PuertoRico, OrgCusCode.PuertoRicoCodeTypes.NRC, "940584", "Client NRC #", "940584");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Paraguay, OrgCusCode.ParaguayCodeTypes.RUC, "395867", "Client RUC #", "395867");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Uruguay, UruguayOrgCusCodeInfo.OrgCusCodes.RUT, "998877", "Client RUT #", "998877");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Azerbaijan, OrgCusCode.CodeTypes.VATCode, "344334", "Client VAT #:", "344334");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Bolivia, OrgCusCode.BoliviaCodeTypes.NIT, "565654", "Client NIT #", "565654");
				AssertRecipientTaxId(Core.Constants.CountryCodes.FrenchPolynesia, OrgCusCode.FrenchPolynesiaCodeTypes.TAH, "233445", "Client TAHITI #", "233445");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Honduras, OrgCusCode.HondurasCodeTypes.RTN, "567898", "Client RTN #", "567898");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Kenya, OrgCusCode.CodeTypes.VATCode, "344334", "Client VAT #:", "344334");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Mauritius, OrgCusCode.CodeTypes.VATCode, "344334", "Client VAT #:", "344334");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Nicaragua, OrgCusCode.NicaraguaCodeTypes.RUC, "987898", "Client RUC #", "987898");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Mongolia, OrgCusCode.CodeTypes.VATCode, "88765", "Client VAT #:", "88765");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Botswana, OrgCusCode.CodeTypes.VATCode, "12443", "Client VAT #:", "12443");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Kazakhstan, OrgCusCode.CodeTypes.VATCode, "12444", "Client VAT #:", "12444");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Tanzania, OrgCusCode.TanzaniaCodeTypes.VRN, "12445", "Client VAT #:", "12445");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Mali, OrgCusCode.MaliCodeTypes.NIF, "444555", "Client NIF #", "444555");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Jordan, OrgCusCode.CodeTypes.GSTCode, "345543", "Client GST #", "345543");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Zimbabwe, OrgCusCode.CodeTypes.VATCode, "111111", "CLIENT VAT #:", "111111");
				AssertRecipientTaxId(Core.Constants.CountryCodes.KoreaSouth, OrgCusCode.CodeTypes.VATCode, "111111", "Client VAT #:", "111111");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Thailand, OrgCusCode.CodeTypes.VATCode, "111111", "Client VAT #", "111111");
				AssertRecipientTaxId(Core.Constants.CountryCodes.VietNam, OrgCusCode.CodeTypes.VATCode, "0100233488-999", "Client MST #", "0100233488-999");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Senegal, OrgCusCode.SenegalCodeTypes.NIN, "12444", "Client NINEA #:", "12444");
				AssertRecipientTaxId(Core.Constants.CountryCodes.CoteDivoire, OrgCusCode.CoteDivoireCodeTypes.NCC, "12445", "Client CC #:", "12445");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Cameroon, OrgCusCode.CameroonCodeTypes.NIU, "444555", "Client NIU #:", "444555");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Mozambique, OrgCusCode.MozambiqueCodeTypes.NUI, "345543", "Client NUIT #:", "345543");
				AssertRecipientTaxId(Core.Constants.CountryCodes.EquatorialGuinea, OrgCusCode.EquatorialGuineaCodeTypes.NIF, "444555", "Client NIF #", "444555");
				AssertRecipientTaxId(Core.Constants.CountryCodes.DominicanRepublic, DominicanRepublicOrgCusCodeInfo.OrgCusCodes.RNC, "345543", "Client RNC #:", "345543");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Yemen, OrgCusCode.CodeTypes.GSTCode, "345543", "Client GST #:", "345543");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Algeria, AlgeriaOrgCusCodeInfo.OrgCusCodes.NIF, "444555", "Client NIF #", "444555");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Malawi, OrgCusCode.MalawiCodeTypes.TIN, "444555", "Client TPIN #:", "444555");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Niger, OrgCusCode.NigerCodeTypes.NIF, "444555", "Client NIF #:", "444555");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Ghana, OrgCusCode.GhanaCodeTypes.TIN, "444555", "Client TIN #:", "444555");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Belarus, OrgCusCode.BelarusCodeTypes.TIN, "444555", "Client TIN #:", "444555");
				AssertRecipientTaxId(Core.Constants.CountryCodes.SierraLeone, OrgCusCode.BelarusCodeTypes.TIN, "444555", "Client TIN #:", "444555");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Cambodia, OrgCusCode.CodeTypes.VATCode, "444555", "Client VATTIN #:", "444555");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Malta, OrgCusCode.CodeTypes.VATCode, "444555", "Client VAT #:", "MT444555");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Kiribati, OrgCusCode.KiribatiCodeTypes.TaxIdentificationNumber, "444555", "Client TIN #:", "444555");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Nepal, OrgCusCode.CodeTypes.VATCode, "444555", "Client VAT #:", "444555");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Kosovo, OrgCusCode.KosovoCodeTypes.TVS, "444555", "CLIENT TVSH #:", "444555");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Iran, OrgCusCode.CodeTypes.VATCode, "444555", "Client VAT #:", "444555");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Georgia, OrgCusCode.CodeTypes.VATCode, "444555", "Client VAT #:", "444555");
				AssertRecipientTaxId(Core.Constants.CountryCodes.NewCaledonia, OrgCusCode.NewCaledoniaCodeTypes.TGC, "444555", "CLIENT TGC #:", "444555");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Chad, OrgCusCode.ChadCodeTypes.NIF, "444555", "Client NIF #:", "444555");
				AssertRecipientTaxId(Core.Constants.CountryCodes.LaoPeoplesDemocraticRepublic, OrgCusCode.CodeTypes.VATCode, "444555", "Client VAT #:", "444555");
				AssertRecipientTaxId(Core.Constants.CountryCodes.BosniaAndHerzegovina, OrgCusCode.BosniaAndHerzegovinaCodeTypes.PDV, "444555", "Client PDV #:", "444555");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Angola, OrgCusCode.CodeTypes.IVA, "444555", "Client IVA #:", "444555");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Guyana, OrgCusCode.CodeTypes.VATCode, "444555", "Client VAT #:", "444555");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Mauritania, OrgCusCode.CodeTypes.TVACode, "546123", "Client TVA #:", "546123");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Montenegro, MontenegroOrgCusCodeInfo.OrgCusCodes.PDV, "78542", "Client PDV #:", "78542");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Bahamas, OrgCusCode.CodeTypes.VATCode, "78542", "CLIENT VAT #:", "78542");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Moldova, OrgCusCode.CodeTypes.TVACode, "125487", "Client TVA #:", "125487");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Lesotho, OrgCusCode.CodeTypes.VATCode, "978645", "CLIENT VAT #:", "978645");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Guinea, OrgCusCode.CodeTypes.TVACode, "536987", "Client TVA #:", "536987");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Pakistan, OrgCusCode.CodeTypes.VATCode, "546123", "Client STRN #:", "546123");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Uzbekistan, UzbekistanOrgCusCodeInfo.OrgCusCodes.QQS, "74258", "CLIENT QQS #:", "74258");
				AssertRecipientTaxId(Core.Constants.CountryCodes.Vanuatu, OrgCusCode.CodeTypes.VATCode, "475821", "CLIENT TIN #:", "475821");
				AssertRecipientTaxId(Core.Constants.CountryCodes.SaintMartin, SaintMartinOrgCusCodeInfo.OrgCusCodes.TGC, "444555", "CLIENT TGCA #", "444555");
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(storedCountry);
			}
		}

		protected void AssertRecipientTaxId(string countryCode, string cusCodeUsedForVAT, string cusRegNumber, string expectedTaxIDHeading, string expectedTaxIDNumber, string closestPort = "FRPAR")
		{
			var header = Factory.New<OrgHeader>();
			SetupCodesForRecipientTaxIdTest(header, countryCode, cusCodeUsedForVAT, cusRegNumber, closestPort);

			Invoice.AH_OH = header.PK;
			var transactionLine = (AccTransactionLines)InvoicingBase.Lines.AddNew();
			transactionLine.AL_AT = Factory.New<AccTaxRate>().PK;

			InvoiceBatch = Factory.New<InvoiceBatchHeader>();
			InvoiceBatch.AH_OH = header.PK;
			InvoiceBatch.Line.Add(InvoicingBase);

			InvoiceWrapper = GetBaseInvoiceWrapper();

			GlbCompany.CurrentCompany.SetCountry(countryCode);
			Invoice.Header.ResetCodeForTaxRegistration_ForTestOnly();
			AccountingConfigurationRegistry.Instance.DisplayRecipientTaxIDHeading.Inner.DeleteValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.Inner.DeleteValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

			AssertEquals("Recipient Tax ID Heading", expectedTaxIDHeading, InvoiceWrapper.RecipientTaxIDHeading);
			AssertEquals("Recipient Tax ID Number", expectedTaxIDNumber, InvoiceWrapper.RecipientTaxIDNumber);

			if (!expectedTaxIDNumber.IsNullOrEmpty() || countryCode != Core.Constants.CountryCodes.Netherlands)
			{
				AssertRecipientTaxIdWithDisplayRecipientTaxIDHeading(expectedTaxIDNumber);
			}
		}

		protected virtual void AssertRecipientTaxIdWithDisplayRecipientTaxIDHeading(string expectedTaxIDNumber)
		{
			AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("Recipient Tax ID Heading", "", InvoiceWrapper.RecipientTaxIDHeading);
			AssertEquals("Recipient Tax ID Number", "", InvoiceWrapper.RecipientTaxIDNumber);

			AccountingConfigurationRegistry.Instance.DisplayRecipientTaxIDHeading.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "DefaultHeading");
			AssertEquals("Recipient Tax ID Heading", "", InvoiceWrapper.RecipientTaxIDHeading);
			AssertEquals("Recipient Tax ID Number", "", InvoiceWrapper.RecipientTaxIDNumber);

			AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("Recipient Tax ID Heading", "DefaultHeading", InvoiceWrapper.RecipientTaxIDHeading);
			AssertEquals("Recipient Tax ID Number", expectedTaxIDNumber, InvoiceWrapper.RecipientTaxIDNumber);
		}

		protected virtual void SetupCodesForRecipientTaxIdTest(OrgHeader header, string countryCode, string cusCodeUsedForVAT, string cusRegNumber, string closestPort)
		{
			header.OH_RL_NKClosestPort = closestPort;
			AddCustomsCodeForTest(header, countryCode, cusRegNumber, cusCodeUsedForVAT);
		}

		public void TestRecipientTaxIDForMalaysia()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Malaysia))
			using (AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.DisplayRecipientTaxIDHeading.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, null))
			{
				var header = CreateOrgHeaderForMalaysia();

				Invoice.AH_OH = header.PK;
				InvoiceBatch.AH_OH = header.PK;

				var taxRate1 = Factory.New<AccTaxRate>();
				var taxRate2 = Factory.New<AccTaxRate>();

				var transactionLine1 = (AccTransactionLines)BatchedInvoice.Lines.AddNew();
				transactionLine1.AL_AT = taxRate1.PK;
				var transactionLine2 = (AccTransactionLines)BatchedInvoice.Lines.AddNew();
				transactionLine2.AL_AT = taxRate2.PK;
				InvoiceBatch.Line.Add(BatchedInvoice);

				var transactionLine3 = (AccTransactionLines)InvoicingBase.Lines.AddNew();
				transactionLine3.AL_AT = taxRate1.PK;
				var transactionLine4 = (AccTransactionLines)InvoicingBase.Lines.AddNew();
				transactionLine4.AL_AT = taxRate2.PK;

				AssertRecipientTaxIDCoreForMalaysia(taxRate1, taxRate2);
			}
		}

		protected virtual void AssertRecipientTaxIDCoreForMalaysia(AccTaxRate taxRate1, AccTaxRate taxRate2)
		{
			taxRate1.AT_Code = "GST";
			taxRate1.AT_ExtraTaxRateType = ZString.Empty;
			taxRate2.AT_Code = "GST";
			taxRate2.AT_ExtraTaxRateType = ZString.Empty;
			AssertEquals("Client GST #:", InvoiceWrapper.RecipientTaxIDHeading);
			AssertEquals("GST123456", InvoiceWrapper.RecipientTaxIDNumber);

			taxRate1.AT_Code = "GST";
			taxRate1.AT_ExtraTaxRateType = ZString.Empty;
			taxRate2.AT_Code = "SVC";
			taxRate2.AT_ExtraTaxRateType = "SER";
			AssertEquals("Client GST #:", InvoiceWrapper.RecipientTaxIDHeading);
			AssertEquals("GST123456", InvoiceWrapper.RecipientTaxIDNumber);

			taxRate1.AT_Code = "SVC";
			taxRate1.AT_ExtraTaxRateType = "SER";
			taxRate2.AT_Code = "SVC";
			taxRate2.AT_ExtraTaxRateType = "SER";
			AssertEquals("Client SST #:", InvoiceWrapper.RecipientTaxIDHeading);
			AssertEquals("SER123456", InvoiceWrapper.RecipientTaxIDNumber);

			taxRate1.AT_Code = "XXX";
			taxRate1.AT_ExtraTaxRateType = "TST";
			taxRate2.AT_Code = "XXX";
			taxRate2.AT_ExtraTaxRateType = "TST";
			AssertEquals(ZString.Empty, InvoiceWrapper.RecipientTaxIDHeading);
			AssertEquals(ZString.Empty, InvoiceWrapper.RecipientTaxIDNumber);
		}

		protected OrgHeader CreateOrgHeaderForMalaysia()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_RL_NKClosestPort = "MYABU";
			header.LocalBusinessRegNo = "ABC123";

			var taxCode1 = header.CustomsCodes.AddNew();
			taxCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Malaysia;
			taxCode1.OK_CodeType = OrgCusCode.CodeTypes.GSTCode;
			taxCode1.OK_CustomsRegNo = "GST123456";

			var taxCode2 = header.CustomsCodes.AddNew();
			taxCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Malaysia;
			taxCode2.OK_CodeType = MalaysiaOrgCusCodeInfo.OrgCusCodes.SER;
			taxCode2.OK_CustomsRegNo = "SER123456";

			var taxCode3 = header.CustomsCodes.AddNew();
			taxCode3.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			taxCode3.OK_CodeType = OrgCusCode.CodeTypes.GSTCode;
			taxCode3.OK_CustomsRegNo = "GSTTW1234";

			return header;
		}

		public void TestRecipientTaxID_LoginCountryNotEqualRecipientCountryOfRegistration()
		{
			var storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				var header = Factory.New<OrgHeader>();
				header.OH_RL_NKClosestPort = "GBLON";
				header.CustomsCodes.RemoveAll();

				var taxCode = header.CustomsCodes.AddNew();
				taxCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
				taxCode.OK_CodeType = Country.GetConsumptionTaxDescription(Core.Constants.CountryCodes.UnitedKingdom);
				taxCode.OK_CustomsRegNo = "123456";

				Invoice.AH_OH = header.PK;
				InvoiceBatch = Factory.New<InvoiceBatchHeader>();
				InvoiceBatch.AH_OH = header.PK;

				var transactionLine = (AccTransactionLines)InvoicingBase.Lines.AddNew();
				transactionLine.AL_AT = Factory.New<AccTaxRate>().PK;

				InvoiceBatch.Line.Add(InvoicingBase);

				Invoice = GetWrappedInvoice();
				InvoiceWrapper = GetBaseInvoiceWrapper();

				//In the Same Country
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedKingdom);
				Invoice.Header.ResetCodeForTaxRegistration_ForTestOnly();
				AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				AssertEquals("Recipient Tax ID Heading for a United Kingdom Company", "Client VAT #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number for a United Kingdom Company", "GB123456", InvoiceWrapper.RecipientTaxIDNumber);

				//In Another ENU
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Germany);
				Invoice.Header.ResetCodeForTaxRegistration_ForTestOnly();

				AssertEquals("Recipient Tax ID Heading for a German Company", "Client VAT ID No:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number for another ENU Company", "GB123456", InvoiceWrapper.RecipientTaxIDNumber);

				//Out of ENU
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
				Invoice.Header.ResetCodeForTaxRegistration_ForTestOnly();

				AssertEquals("Recipient Tax ID Heading for a New Zealand Company", "Client GST #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Should be empty because New Zealand is not in ENU", ZString.Empty, InvoiceWrapper.RecipientTaxIDNumber);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(storedCountry);
			}
		}

		public void TestRecipientTaxIDForSpain()
		{
			if (InvoicingBase is ARInvoice)
			{
				var header = TestObjectCreator.ABIGAS;
				var nifCode = header.CustomsCodes.AddNew();
				nifCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Spain;
				nifCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				nifCode.OK_CustomsRegNo = "999999";

				InvoicingBase.AH_OH = header.PK;
				InvoiceBatch.AH_OH = header.PK;
				var otherTaxRate = Factory.New<AccTaxRate>();
				otherTaxRate.AT_RN_NKCountry = Constants.CountryCodes.Spain;
				var otherLine = (AccTransactionLines)InvoicingBase.Lines.AddNew();
				otherLine.AL_AT = otherTaxRate.PK;
				InvoiceBatch.Line.Add(InvoicingBase);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Spain))
				{
					InvoiceWrapper = GetBaseInvoiceWrapper();
					AssertEquals("Client VAT #:", InvoiceWrapper.RecipientTaxIDHeading);
					AssertEquals("ES999999", InvoiceWrapper.RecipientTaxIDNumber);
				}

				var igicTaxRate = Factory.New<AccTaxRate>();
				igicTaxRate.AT_RN_NKCountry = Constants.CountryCodes.Spain;
				igicTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.RegionalTax;
				var igicLine = (AccTransactionLines)InvoicingBase.Lines.AddNew();
				igicLine.AL_AT = igicTaxRate.PK;
				InvoiceBatch.Line.Add(InvoicingBase);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Spain))
				{
					InvoiceWrapper = GetBaseInvoiceWrapper();
					AssertEquals("Client VAT #:", InvoiceWrapper.RecipientTaxIDHeading);
					AssertEquals(string.Empty, InvoiceWrapper.RecipientTaxIDNumber);
				}

				var igicCode = header.CustomsCodes.AddNew();
				igicCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Spain;
				igicCode.OK_CodeType = OrgCusCode.SpainCodeTypes.IGC;
				igicCode.OK_CustomsRegNo = "123456";

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Spain))
				{
					InvoiceWrapper = GetBaseInvoiceWrapper();
					AssertEquals("Client NIF #:", InvoiceWrapper.RecipientTaxIDHeading);
					AssertEquals("123456", InvoiceWrapper.RecipientTaxIDNumber);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestRecipientTaxIDNumber_RecipientTaxIDNumberInRecipientCountry_Portugal()
		{
			if (InvoicingBase is ARInvoice)
			{
				var header = TestObjectCreator.ABIGAS;
				header.OH_IsGlobalAccount = true;
				var portugalAddress = Factory.New<OrgAddress>();
				portugalAddress.OA_Address1 = "P Test address";
				portugalAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Portugal;
				var spainAddress = Factory.New<OrgAddress>();
				spainAddress.OA_Address1 = "S Test address";
				spainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Spain;
				var mexicoAddress = Factory.New<OrgAddress>();
				mexicoAddress.OA_Address1 = "M Test address";
				mexicoAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Mexico;
				var ptCode = AddCustomsCodeForTest(header, Constants.CountryCodes.Portugal, "123456789", "IVA", portugalAddress);
				var esCode = AddCustomsCodeForTest(header, Constants.CountryCodes.Spain, "987654321", "NIF", spainAddress);
				var mxCode = AddCustomsCodeForTest(header, Constants.CountryCodes.Mexico, "231231231", "IVA", mexicoAddress);

				InvoicingBase.AH_OH = header.PK;
				InvoicingBase.AH_OA_InvoiceAddressOverride = header.MainAddress.PK;
				InvoiceBatch.AH_OH = header.PK;
				InvoiceBatch.AH_OA_InvoiceAddressOverride = header.MainAddress.PK;

				var otherTaxRate = Factory.New<AccTaxRate>();
				otherTaxRate.AT_RN_NKCountry = Constants.CountryCodes.Portugal;
				var otherLine = (AccTransactionLines)InvoicingBase.Lines.AddNew();
				otherLine.AL_AT = otherTaxRate.PK;
				InvoiceBatch.Line.Add(InvoicingBase);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
				{
					Assert("Pre-condition: DisplayRecipientTaxID is True", AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.Value);

					AssertRecipientIDs(Constants.CountryCodes.Portugal, "PT123456789", "PT123456789", portugalAddress);
					AssertRecipientIDs(Constants.CountryCodes.Spain, "PT123456789", "ES987654321", spainAddress);
					AssertRecipientIDs(Constants.CountryCodes.Mexico, "PT123456789", "231231231", mexicoAddress);
					AssertNoRecepientTaxIDNumberWhenDisabledByRegistry();

					ptCode.OK_CustomsRegNo = "PT123456789";
					AssertRecipientIDs(Constants.CountryCodes.Portugal, "PT123456789", "PT123456789", portugalAddress);
					AssertRecipientIDs(Constants.CountryCodes.Spain, "PT123456789", "ES987654321", spainAddress);
					AssertRecipientIDs(Constants.CountryCodes.Mexico, "PT123456789", "231231231", mexicoAddress);
					AssertNoRecepientTaxIDNumberWhenDisabledByRegistry();

					ptCode.OK_CustomsRegNo = "999999990";
					AssertRecipientIDs(Constants.CountryCodes.Portugal, "XXXXXXXXX", "", null);
					AssertRecipientIDs(Constants.CountryCodes.Spain, "ES987654321", "ES987654321", spainAddress);
					AssertRecipientIDs(Constants.CountryCodes.Mexico, "", "231231231", mexicoAddress);
					AssertNoRecepientTaxIDNumberWhenDisabledByRegistry();

					ptCode.OK_CustomsRegNo = "PT999999990";
					AssertRecipientIDs(Constants.CountryCodes.Portugal, "XXXXXXXXX", "", null);
					AssertRecipientIDs(Constants.CountryCodes.Spain, "ES987654321", "ES987654321", spainAddress);
					AssertRecipientIDs(Constants.CountryCodes.Mexico, "", "231231231", mexicoAddress);
					AssertNoRecepientTaxIDNumberWhenDisabledByRegistry();

					ptCode.Delete();
					AssertRecipientIDs(Constants.CountryCodes.Portugal, "XXXXXXXXX", "", null);
					AssertRecipientIDs(Constants.CountryCodes.Spain, "ES987654321", "ES987654321", spainAddress);
					AssertRecipientIDs(Constants.CountryCodes.Mexico, "", "231231231", mexicoAddress);
					AssertNoRecepientTaxIDNumberWhenDisabledByRegistry();

					esCode.Delete();
					mxCode.Delete();
					AssertRecipientIDs(Constants.CountryCodes.Portugal, "XXXXXXXXX", "", null);
					AssertRecipientIDs(Constants.CountryCodes.Spain, "XXXXXXXXX", "", null);
					AssertRecipientIDs(Constants.CountryCodes.Mexico, "XXXXXXXXX", "", null);
					AssertNoRecepientTaxIDNumberWhenDisabledByRegistry();

					ptCode = AddCustomsCodeForTest(header, Constants.CountryCodes.Portugal, "999999990", "IVA", portugalAddress);
					AssertRecipientIDs(Constants.CountryCodes.Portugal, "XXXXXXXXX", "", null);
					AssertRecipientIDs(Constants.CountryCodes.Spain, "XXXXXXXXX", "", null);
					AssertRecipientIDs(Constants.CountryCodes.Mexico, "XXXXXXXXX", "", null);
					AssertNoRecepientTaxIDNumberWhenDisabledByRegistry();

					ptCode.OK_CustomsRegNo = "PT999999990";
					AssertRecipientIDs(Constants.CountryCodes.Portugal, "XXXXXXXXX", "", null);
					AssertRecipientIDs(Constants.CountryCodes.Spain, "XXXXXXXXX", "", null);
					AssertRecipientIDs(Constants.CountryCodes.Mexico, "XXXXXXXXX", "", null);
					AssertNoRecepientTaxIDNumberWhenDisabledByRegistry();

					esCode = AddCustomsCodeForTest(header, Constants.CountryCodes.Spain, "987654321", "GBR", spainAddress);   // Non-primary tax registration GBR
					AssertRecipientIDs(Constants.CountryCodes.Portugal, "XXXXXXXXX", "", null);
					AssertRecipientIDs(Constants.CountryCodes.Spain, "", "ES987654321", spainAddress);
					AssertRecipientIDs(Constants.CountryCodes.Mexico, "XXXXXXXXX", "", null);
					AssertNoRecepientTaxIDNumberWhenDisabledByRegistry();

					mxCode = AddCustomsCodeForTest(header, Constants.CountryCodes.Mexico, "231231231", "GBR", mexicoAddress);  // Non-primary tax registration GBR
					ptCode.Delete();
					AssertRecipientIDs(Constants.CountryCodes.Portugal, "XXXXXXXXX", "", null);
					AssertRecipientIDs(Constants.CountryCodes.Spain, "", "ES987654321", spainAddress);
					AssertRecipientIDs(Constants.CountryCodes.Mexico, "", "231231231", mexicoAddress);
					AssertNoRecepientTaxIDNumberWhenDisabledByRegistry();
				}

				void AssertRecipientIDs(string countryCode, string expectedRecipientID, string expectedRecipientIDHomeCountry, OrgAddress expectedAddress)
				{
					header.OH_RL_NKClosestPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, countryCode)).Code;
					header.MainAddress.OA_RN_NKCountryCode = countryCode;
					header.ResetCodeForTaxRegistration_ForTestOnly();
					InvoiceWrapper = GetBaseInvoiceWrapper();
					AssertEquals(nameof(InvoiceWrapper.RecipientTaxIDNumber), expectedRecipientID, InvoiceWrapper.RecipientTaxIDNumber);
					AssertEquals(nameof(InvoiceWrapper.RecipientTaxIDNumberInRecipientCountry), expectedRecipientIDHomeCountry, InvoiceWrapper.RecipientTaxIDNumberInRecipientCountry);
					var expectedDocAddress = expectedAddress != null ? DocAddress.New(Factory.Load<OrgAddress>(expectedAddress.PK), Factory) : null;
					AssertEquals(nameof(InvoiceWrapper.RecipientTaxIDPremisesAddressInRecipientCountry), expectedDocAddress?.ToString(), InvoiceWrapper.RecipientTaxIDPremisesAddressInRecipientCountry?.ToString());
				}

				void AssertNoRecepientTaxIDNumberWhenDisabledByRegistry()
				{
					using (AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					{
						var wrapper = GetBaseInvoiceWrapper();
						Assert(wrapper.RecipientTaxIDNumber.IsEmpty);
					}
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestRecipientTaxIDNumberNoException_Portugal()
		{
			if (InvoicingBase is ARInvoice)
			{
				var header = TestObjectCreator.ABIGAS;
				var ptCode = AddCustomsCodeForTest(header, Constants.CountryCodes.Portugal, "999999990", "IVA");

				InvoicingBase.AH_OH = header.PK;
				InvoiceBatch.AH_OH = header.PK;

				var otherTaxRate = Factory.New<AccTaxRate>();
				otherTaxRate.AT_RN_NKCountry = Constants.CountryCodes.Portugal;
				var otherLine = (AccTransactionLines)InvoicingBase.Lines.AddNew();
				otherLine.AL_AT = otherTaxRate.PK;
				InvoiceBatch.Line.Add(InvoicingBase);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
				{
					header.OH_RL_NKClosestPort = ZString.Empty;
					header.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
					InvoiceWrapper = GetBaseInvoiceWrapper();
					AssertNoExceptionThrown(() => getRecipientTaxIDNumber());
				}

				ZString getRecipientTaxIDNumber()
				{
					return InvoiceWrapper.RecipientTaxIDNumber;
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestRecipientTaxIDNumber_ShouldPortugalStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting()
		{
			if (InvoicingBase is ARInvoice)
			{
				InvoiceBatch.Line.Add(InvoicingBase);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
				{
					AccountingConfigurationRegistry.Instance.ARStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

					var docAddress = InvoicingBase.DocAddresses.CreateWithAddressType(DocAddressType.DebtorAddress);
					docAddress.E2_AddressOverride = true;
					docAddress.E2_CompanyName = GlbCompany.CurrentCompany.CompanyName;
					docAddress.E2_GovRegNum = "Debtor Address GovRegNum";

					InvoiceWrapper = GetBaseInvoiceWrapper();
					if (InvoiceWrapper.TransactionHeader is InvoicingBase)
					{
						AssertEquals("Debtor Address GovRegNum", InvoiceWrapper.RecipientTaxIDNumber);
					}
					else
					{
						Assert(true);
					}
				}
			}
			else
			{
				Assert(true);
			}
		}

		[ExpectNoExceptions]
		public void TestGetICountryComplianceInfo_DifferentCountryCodePassed_WhenRecipientLocalBusinessRegHeadingCalled()
		{
			var countryCode = Constants.CountryCodes.Australia;
			GlbCompany.CurrentCompany.SetCountry(countryCode);

			var recipientLocalBusinessRegNumberCodeType = "TST";
			var mockComplianceFactory = new Mock<ICountryComplianceFactoryIntegration>();
			var mockCountryComplianceInfo = new Mock<ICountryComplianceInfo>();
			mockComplianceFactory.Setup(x => x.GetICountryComplianceInfo(It.IsAny<ZString>())).Returns(mockCountryComplianceInfo.Object);
			mockCountryComplianceInfo.Setup(x => x.GetRecipientLocalBusinessRegNumberCodeType()).Returns(recipientLocalBusinessRegNumberCodeType);
			mockCountryComplianceInfo.Setup(x => x.GetConsumptionTaxCode()).Returns("GST");
			mockCountryComplianceInfo.Setup(x => x.GetConsumptionTaxRegistrationCode()).Returns("ABN");
			ObjectFactory.Substitute(mockComplianceFactory.Object);

			var header = Factory.NewWithValidTestData<OrgHeader>();
			Invoice.AH_OH = header.PK;
			InvoiceWrapper = GetBaseInvoiceWrapper();

			header.SetCustomsCode(recipientLocalBusinessRegNumberCodeType, RefCountry.LoadFromCountryCode(Factory, countryCode), "12345");

			AssertNotNull("Precondition: Current Company", InvoiceWrapper.CurrentCompany);
			AssertNotNull("Precondition: Current Company Country", InvoiceWrapper.CurrentCompany.Country);
			AssertNotNullOrEmpty("Precondition: RecipientLocalBusinessRegNumber", InvoiceWrapper.RecipientLocalBusinessRegNumber);

			var recipientLocalBusinessRegNumber = InvoiceWrapper.RecipientLocalBusinessRegHeading;
			mockComplianceFactory.Verify(x => x.GetICountryComplianceInfo(countryCode));

			countryCode = Constants.CountryCodes.Spain;
			GlbCompany.CurrentCompany.SetCountry(countryCode);
			header.SetCustomsCode(recipientLocalBusinessRegNumberCodeType, RefCountry.LoadFromCountryCode(Factory, countryCode), "12345");

			recipientLocalBusinessRegNumber = InvoiceWrapper.RecipientLocalBusinessRegHeading;
			mockComplianceFactory.Verify(x => x.GetICountryComplianceInfo(countryCode));
		}

		[ExpectNoExceptions]
		public void TestRecipientLocalBusinessRegHeading_UsesGetRecipientLocalBusinessRegHeading_NotCalledWhenRecipientLocalBusinessRegNumberIsNotSet()
		{
			var recipientLocalBusinessRegNumberCodeType = "TST";

			var header = Factory.NewWithValidTestData<OrgHeader>();
			Invoice.AH_OH = header.PK;
			InvoiceWrapper = GetBaseInvoiceWrapper();

			var mockCountryComplianceInfo = GetCountryComplianceInfoMock();
			mockCountryComplianceInfo.Setup(x => x.GetRecipientLocalBusinessRegNumberCodeType()).Returns(recipientLocalBusinessRegNumberCodeType);
			mockCountryComplianceInfo.Setup(x => x.GetConsumptionTaxCode()).Returns("GST");
			mockCountryComplianceInfo.Setup(x => x.GetConsumptionTaxRegistrationCode()).Returns("ABN");

			AssertNotNull("Precondition: Current Company", InvoiceWrapper.CurrentCompany);
			AssertNotNull("Precondition: Current Company Country", InvoiceWrapper.CurrentCompany.Country);
			AssertNullOrEmpty("Precondition: RecipientLocalBusinessRegNumber", InvoiceWrapper.RecipientLocalBusinessRegNumber);

			var recipientLocalBusinessRegHeading = InvoiceWrapper.RecipientLocalBusinessRegHeading;
			mockCountryComplianceInfo.Verify(x => x.GetRecipientLocalBusinessRegHeading(), Times.Never());

			header.SetCustomsCode(recipientLocalBusinessRegNumberCodeType, RefCountry.LoadFromCountryCode(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode), "12345");

			recipientLocalBusinessRegHeading = InvoiceWrapper.RecipientLocalBusinessRegHeading;
			mockCountryComplianceInfo.Verify(x => x.GetRecipientLocalBusinessRegHeading(), Times.Once());
		}

		[ExpectNoExceptions]
		public void TestGetICountryComplianceInfo_DifferentCountryCodePassed_WhenRecipientLocalBusinessRegNumberCalled()
		{
			var countryCode = Core.Constants.CountryCodes.Australia;
			GlbCompany.CurrentCompany.SetCountry(countryCode);

			var mockComplianceFactory = new Mock<ICountryComplianceFactoryIntegration>();
			ObjectFactory.Substitute(mockComplianceFactory.Object);

			var header = Factory.NewWithValidTestData<OrgHeader>();
			Invoice.AH_OH = header.PK;
			InvoiceBatch.Line.Add(InvoicingBase);
			InvoiceWrapper = GetBaseInvoiceWrapper();

			AssertNotNull("Precondition: Current Company", InvoiceWrapper.CurrentCompany);
			AssertNotNull("Precondition: Current Company Country", InvoiceWrapper.CurrentCompany.Country);
			AssertNotNull("Precondition: Transaction Header", InvoiceWrapper.TransactionHeader.Header);

			var recipientLocalBusinessRegNumber = InvoiceWrapper.RecipientLocalBusinessRegNumber;
			mockComplianceFactory.Verify(x => x.GetICountryComplianceInfo(countryCode));

			countryCode = Core.Constants.CountryCodes.Spain;
			GlbCompany.CurrentCompany.SetCountry(countryCode);
			recipientLocalBusinessRegNumber = InvoiceWrapper.RecipientLocalBusinessRegNumber;
			mockComplianceFactory.Verify(x => x.GetICountryComplianceInfo(countryCode));
		}

		[ExpectNoExceptions]
		public void TestRecipientLocalBusinessRegNumber_UsesGetRecipientLocalBusinessRegNumberCodeType_NotCalledWhenTransactionHeaderIsNotSet()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			InvoiceWrapper = GetBaseInvoiceWrapper();

			var mockCountryComplianceInfo = GetCountryComplianceInfoMock();

			AssertNotNull("Precondition: Current Company", InvoiceWrapper.CurrentCompany);
			AssertNotNull("Precondition: Current Company Country", InvoiceWrapper.CurrentCompany.Country);
			AssertNull("Precondition: Transaction Header", InvoiceWrapper.TransactionHeader.Header);

			var recipientLocalBusinessRegNumber = InvoiceWrapper.RecipientLocalBusinessRegNumber;
			mockCountryComplianceInfo.Verify(x => x.GetRecipientLocalBusinessRegNumberCodeType(), Times.Never());

			Invoice.AH_OH = header.PK;
			recipientLocalBusinessRegNumber = InvoiceWrapper.RecipientLocalBusinessRegNumber;
			mockCountryComplianceInfo.Verify(x => x.GetRecipientLocalBusinessRegNumberCodeType(), Times.Once());
		}

		public void TestRecipientLocalBusinessRegNumber()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			Invoice.AH_OH = header.PK;
			InvoiceBatch.Line.Add(InvoicingBase);
			InvoiceWrapper = GetBaseInvoiceWrapper();

			//Tests RecipientLocalBusinessRegNumber only
			AddTaxCode(Constants.CountryCodes.France, OrgCusCode.FranceCodeTypes.Siren, "121212");
			AssertRecipientLocalBusinessReg(Constants.CountryCodes.France, "121212", "SIRET/SIREN:");
			AddTaxCode(Constants.CountryCodes.France, OrgCusCode.FranceCodeTypes.Siret, "343434");
			AssertRecipientLocalBusinessReg(Constants.CountryCodes.France, "343434", "SIRET/SIREN:");

			AddTaxCode(Constants.CountryCodes.Australia, Country.GetConsumptionTaxRegistrationOrgCusCode(Constants.CountryCodes.Australia), "133333");
			AssertEquals("LocalBusinessRegNo is ABN for Australia", "133333", header.LocalBusinessRegNo);
			AssertRecipientLocalBusinessReg(Constants.CountryCodes.Australia, string.Empty, string.Empty);

			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Morocco, OrgCusCode.MoroccoCodeTypes.ICE, "111111", "ICE");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Greece, OrgCusCode.GreeceCodeTypes.DOY, "222222", "Client DOY:");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Spain, OrgCusCode.SpainCodeTypes.DNI, "333333", "Client DNI:");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Denmark, OrgCusCode.DenmarkCodeTypes.EANLocationNumber, "444444", "EAN NUMBER");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.SriLanka, OrgCusCode.SriLankaCodeTypes.SVATBusinessRegistrationNumber, "555555", "Client SVAT #");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Ethiopia, OrgCusCode.EthiopiaCodeTypes.TaxIdentificationNumber, "666666", "TIN:");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Uganda, UgandaOrgCusCodeInfo.OrgCusCodes.TaxIdentificationNumber, "777777", "TIN:");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Romania, OrgCusCode.RomaniaCodeTypes.CIF, "999999", "CLIENT CIF");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Turkey, TurkeyOrgCusCodeInfo.OrgCusCodes.VDM, "122222", "CLIENT TAX OFFICE:");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Azerbaijan, OrgCusCode.AzerbaijanCodeTypes.TIN, "144444", "CLIENT TIN #");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Kenya, OrgCusCode.KenyaCodeTypes.PIN, "155555", "CLIENT PIN #");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Mauritius, OrgCusCode.CodeTypes.BusinessRegistrationNumber, "166666", "CLIENT BRN #");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Tanzania, OrgCusCode.TanzaniaCodeTypes.TIN, "177777", "CLIENT TIN #");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Thailand, OrgCusCode.ThailandCodeTypes.BID, "188888", "Client Branch");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Peru, OrgCusCode.PeruCodeTypes.DNI, "199999", "DNI #:");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Madagascar, OrgCusCode.MadagascarCodeTypes.NIS, "233333", "N° Statistique:");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Lithuania, OrgCusCode.LithuaniaCodeTypes.IMK, "244444", "CLIENT ĮM.KODA #");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Croatia, OrgCusCode.CroatiaCodeTypes.OIB, "255555", "CLIENT OIB #:");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Kosovo, OrgCusCode.KosovoCodeTypes.NFK, "266666", "CLIENT NFK #:");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Hungary, OrgCusCode.CodeTypes.TaxFileCode, "277777", "CLIENT TAX #");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Angola, OrgCusCode.AngolaCodeTypes.NumeroDeIdentificacioFiscal, "288888", "CLIENT NIF #");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.ElSalvador, ElSalvadorOrgCusCodeInfo.OrgCusCodes.NIT, "299999", "CLIENT NIT #");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Guyana, GuyanaOrgCusCodeInfo.OrgCusCodes.TIN, "311111", "CLIENT TIN #");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Mauritania, MauritaniaOrgCusCodeInfo.OrgCusCodes.NIF, "322222", "CLIENT NIF #");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Pakistan, PakistanOrgCusCodeInfo.OrgCusCodes.NTN, "344444", "CLIENT NTN #");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Congo, CongoOrgCusCodeInfo.OrgCusCodes.NIU, "355555", "CLIENT NIU #");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Lesotho, LesothoOrgCusCodeInfo.OrgCusCodes.TIN, "456987", "CLIENT TIN #");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Moldova, MoldovaOrgCusCodeInfo.OrgCusCodes.NCF, "355555", "CLIENT CF #");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Guinea, GuineaOrgCusCodeInfo.OrgCusCodes.NIF, "3666666", "CLIENT NIF #");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Montenegro, MontenegroOrgCusCodeInfo.OrgCusCodes.PIB, "366666", "CLIENT PIB #");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Bahamas, BahamasOrgCusCodeInfo.OrgCusCodes.TIN, "366666", "CLIENT TIN #");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Uzbekistan, UzbekistanOrgCusCodeInfo.OrgCusCodes.STR, "488888", "CLIENT STIR #");
			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.Russia, OrgCusCode.RussiaCodeTypes.KPP, "599999", "CLIENT KPP #");

			//Tests RecipientLocalBusinessRegNumber and RecipientLocalBusinessRegNumber2
			AddTaxCode(Constants.CountryCodes.Italy, ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale, "111222");
			AddTaxCode(Constants.CountryCodes.Italy, ItalyOrgCusCodeInfo.OrgCusCodes.CUU, "IT CUU");
			AssertRecipientLocalBusinessReg(Constants.CountryCodes.Italy, "111222", "Codice Fiscale:");
			AssertRecipientLocalBusinessReg2(Constants.CountryCodes.Italy, "IT CUU", "COD. UNIV. UFFICIO");

			AddTaxCode(Constants.CountryCodes.KoreaSouth, KoreaSouthComplianceInfo.CodeTypes.KBC, "333444");
			AddTaxCode(Constants.CountryCodes.KoreaSouth, KoreaSouthComplianceInfo.CodeTypes.KBT, "444555");
			AssertRecipientLocalBusinessReg(Constants.CountryCodes.KoreaSouth, "444555", "Type");
			AssertRecipientLocalBusinessReg2(Constants.CountryCodes.KoreaSouth, "333444", "Category");

			Invoice.Header.OH_RL_NKClosestPort = "INBOM";
			AddTaxCode(Constants.CountryCodes.India, IndiaOrgCusCodeInfo.OrgCusCodes.PAN, "111111PAN");
			AddTaxCode(Constants.CountryCodes.India, IndiaOrgCusCodeInfo.OrgCusCodes.GID, "22222GID");
			AssertRecipientLocalBusinessReg(Constants.CountryCodes.India, "111111PAN", "CLIENT PAN #");
			AssertRecipientLocalBusinessReg2(Constants.CountryCodes.India, "22222GID", "Client GID");

			AddTaxCode(Constants.CountryCodes.BosniaAndHerzegovina, OrgCusCode.BosniaAndHerzegovinaCodeTypes.IDB, "777888");
			AddTaxCode(Constants.CountryCodes.BosniaAndHerzegovina, OrgCusCode.BosniaAndHerzegovinaCodeTypes.JMB, "888999");
			AssertRecipientLocalBusinessReg(Constants.CountryCodes.BosniaAndHerzegovina, "777888", "CLIENT ID #");
			AssertRecipientLocalBusinessReg2(Constants.CountryCodes.BosniaAndHerzegovina, "888999", "MATICNOM BROJ");

			SetupAndAssertRecipientLocalBusinessReg(Constants.CountryCodes.CostaRica, CostaRicaOrgCusCodeInfo.OrgCusCodes.IndividualIdentificationNumber, "211111", "CÉD. FÍSICA #");
			AssertRecipientLocalBusinessReg2(Constants.CountryCodes.CostaRica, string.Empty, string.Empty);
			AddTaxCode(Constants.CountryCodes.CostaRica, CostaRicaOrgCusCodeInfo.OrgCusCodes.NITIdentificationNumber, "747474");
			AssertRecipientLocalBusinessReg2(Constants.CountryCodes.CostaRica, "747474", "NITE #");
			AddTaxCode(Constants.CountryCodes.CostaRica, CostaRicaOrgCusCodeInfo.OrgCusCodes.DIMEXDocumentIdentificationNumber, "858585");
			AssertRecipientLocalBusinessReg2(Constants.CountryCodes.CostaRica, "858585", "DIMEX #");

			AddTaxCode(Constants.CountryCodes.Brazil, BrazilOrgCusCodeInfo.OrgCusCodes.MunicipalTaxPayerRegistration, "1111IMF");
			AddTaxCode(Constants.CountryCodes.Brazil, BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration, "2222CPF");
			AssertRecipientLocalBusinessReg(Constants.CountryCodes.Brazil, "1111IMF", "IM");
			AssertRecipientLocalBusinessReg2(Constants.CountryCodes.Brazil, "2222CPF", "CPF");

			void AddTaxCode(string countryCode, string codeType, string regNo)
			{
				var taxCode = header.CustomsCodes.AddNew();
				taxCode.OK_RN_NKCodeCountry = countryCode;
				taxCode.OK_CodeType = codeType;
				taxCode.OK_CustomsRegNo = regNo;
			}

			void AssertRecipientLocalBusinessReg(string countryCode, string regNo, string regHeading)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					AssertEquals($"Recipient Local Business Reg Heading when Current Company is {countryCode}", regHeading, InvoiceWrapper.RecipientLocalBusinessRegHeading);
					AssertEquals($"Recipient Tax ID Number when Current Company is {countryCode}", regNo, InvoiceWrapper.RecipientLocalBusinessRegNumber);
				}
			}

			void AssertRecipientLocalBusinessReg2(string countryCode, string regNo, string regHeading)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					AssertEquals($"Recipient Local Business Reg2 Heading when Current Company is {countryCode}", regHeading, InvoiceWrapper.RecipientLocalBusinessReg2Heading);
					AssertEquals($"Recipient Tax ID 2 Number when Current Company is {countryCode}", regNo, InvoiceWrapper.RecipientLocalBusinessReg2Number);
				}
			}

			void SetupAndAssertRecipientLocalBusinessReg(string countryCode, string codeType, string regNo, string regHeading)
			{
				AddTaxCode(countryCode, codeType, regNo);
				AssertRecipientLocalBusinessReg(countryCode, regNo, regHeading);
			}
		}

		#region Test Recipient State and Country

		public void TestRecipientLocalState()
		{
			var header1 = Factory.New<OrgHeader>();
			header1.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Australia;
			header1.MainAddress.OA_State = "NSW";

			var header2 = Factory.New<OrgHeader>();
			header2.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.India;
			header2.MainAddress.OA_State = "WB";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				Invoice.AH_OH = header1.PK;
				InvoiceWrapper = GetBaseInvoiceWrapper();

				AssertEquals("State", InvoiceWrapper.RecipientLocalStateHeading);
				AssertEquals("New South Wales", InvoiceWrapper.RecipientLocalState);

				Invoice.AH_OH = header2.PK;
				InvoiceWrapper = GetBaseInvoiceWrapper();

				AssertEquals(ZString.Empty, InvoiceWrapper.RecipientLocalStateHeading);
				AssertEquals(ZString.Empty, InvoiceWrapper.RecipientLocalState);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				Invoice.AH_OH = header1.PK;
				InvoiceWrapper = GetBaseInvoiceWrapper();

				AssertEquals(ZString.Empty, InvoiceWrapper.RecipientLocalStateHeading);
				AssertEquals(ZString.Empty, InvoiceWrapper.RecipientLocalState);

				Invoice.AH_OH = header2.PK;
				InvoiceWrapper = GetBaseInvoiceWrapper();

				AssertEquals("State of Supply", InvoiceWrapper.RecipientLocalStateHeading);
				AssertEquals("West Bengal", InvoiceWrapper.RecipientLocalState);
			}
		}

		public void TestRecipientCountryName()
		{
			var header = Factory.New<OrgHeader>();
			header.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Australia;
			header.MainAddress.OA_State = "NSW";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				Invoice.AH_OH = header.PK;
				InvoiceWrapper = GetBaseInvoiceWrapper();

				AssertEquals("Country", InvoiceWrapper.RecipientCountryNameHeading);
				AssertEquals("Australia", InvoiceWrapper.RecipientCountryName);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				Invoice.AH_OH = header.PK;
				InvoiceWrapper = GetBaseInvoiceWrapper();

				AssertEquals("Country of Supply", InvoiceWrapper.RecipientCountryNameHeading);
				AssertEquals("Australia", InvoiceWrapper.RecipientCountryName);
			}
		}

		#endregion

		#region Test Transaction Header Branch Reg

		public void TestBranchTaxID()
		{
			var india = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.India);

			var companyOrgProxy = Invoice.Company.OrgProxy;
			companyOrgProxy.OH_Code = "OH123";
			companyOrgProxy.OH_RL_NKClosestPort = "INBOM";
			companyOrgProxy.SetCustomsCode(OrgCusCode.CodeTypes.GSTCode, india, "123");

			var branchOrgProxy = Factory.New<OrgHeader>();
			branchOrgProxy.OH_Code = "OH456";
			branchOrgProxy.OH_RL_NKClosestPort = "INBOM";
			branchOrgProxy.SetCustomsCode(OrgCusCode.CodeTypes.GSTCode, india, "456");

			var branch = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);
			Invoice.AH_GB = branch.PK;

			Factory.Save();

			Invoice.AH_OH = companyOrgProxy.PK;
			InvoiceWrapper = GetBaseInvoiceWrapper();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				branch.GB_OH_OrgProxy = branchOrgProxy.PK;
				AssertNotNull("Precondition", branch.OrgProxy);

				AssertEquals("GST from branch since branch org proxy exists", "456", InvoiceWrapper.BranchTaxIDNumber);
				AssertEquals("GSTIN", InvoiceWrapper.BranchTaxIDHeading);

				branch.GB_OH_OrgProxy = ZGuid.Empty;
				AssertNull("Precondition", branch.OrgProxy);
				AssertEquals("GST from company since company org proxy exists", "123", InvoiceWrapper.BranchTaxIDNumber);
				AssertEquals("GSTIN", InvoiceWrapper.BranchTaxIDHeading);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedArabEmirates))
			{
				branch.GB_OH_OrgProxy = branchOrgProxy.PK;
				AssertNotNull("Precondition", branch.OrgProxy);

				AssertEquals("VAT from branch since branch org proxy exists", "456", InvoiceWrapper.BranchTaxIDNumber);
				AssertEquals("VAT #", InvoiceWrapper.BranchTaxIDHeading);

				branch.GB_OH_OrgProxy = ZGuid.Empty;
				AssertNull("Precondition", branch.OrgProxy);
				AssertEquals("VAT from company since company org proxy exists", "123", InvoiceWrapper.BranchTaxIDNumber);
				AssertEquals("VAT #", InvoiceWrapper.BranchTaxIDHeading);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Bahrain))
			{
				branch.GB_OH_OrgProxy = branchOrgProxy.PK;
				AssertNotNull("Precondition", branch.OrgProxy);

				AssertEquals("VAT from branch since branch org proxy exists", "456", InvoiceWrapper.BranchTaxIDNumber);
				AssertEquals("VAT #", InvoiceWrapper.BranchTaxIDHeading);

				branch.GB_OH_OrgProxy = ZGuid.Empty;
				AssertNull("Precondition", branch.OrgProxy);
				AssertEquals("VAT from company since company org proxy exists", "123", InvoiceWrapper.BranchTaxIDNumber);
				AssertEquals("VAT #", InvoiceWrapper.BranchTaxIDHeading);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Kuwait))
			{
				branch.GB_OH_OrgProxy = branchOrgProxy.PK;
				AssertNotNull("Precondition", branch.OrgProxy);

				AssertEquals("VAT from branch since branch org proxy exists", "456", InvoiceWrapper.BranchTaxIDNumber);
				AssertEquals("VAT #", InvoiceWrapper.BranchTaxIDHeading);

				branch.GB_OH_OrgProxy = ZGuid.Empty;
				AssertNull("Precondition", branch.OrgProxy);
				AssertEquals("VAT from company since company org proxy exists", "123", InvoiceWrapper.BranchTaxIDNumber);
				AssertEquals("VAT #", InvoiceWrapper.BranchTaxIDHeading);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Oman))
			{
				branch.GB_OH_OrgProxy = branchOrgProxy.PK;
				AssertNotNull("Precondition", branch.OrgProxy);

				AssertEquals("VAT from branch since branch org proxy exists", "456", InvoiceWrapper.BranchTaxIDNumber);
				AssertEquals("VAT #", InvoiceWrapper.BranchTaxIDHeading);

				branch.GB_OH_OrgProxy = ZGuid.Empty;
				AssertNull("Precondition", branch.OrgProxy);
				AssertEquals("VAT from company since company org proxy exists", "123", InvoiceWrapper.BranchTaxIDNumber);
				AssertEquals("VAT #", InvoiceWrapper.BranchTaxIDHeading);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Sudan))
			{
				branch.GB_OH_OrgProxy = branchOrgProxy.PK;
				AssertNotNull("Precondition", branch.OrgProxy);

				AssertEquals("VAT from branch since branch org proxy exists", "456", InvoiceWrapper.BranchTaxIDNumber);
				AssertEquals("VAT #", InvoiceWrapper.BranchTaxIDHeading);

				branch.GB_OH_OrgProxy = ZGuid.Empty;
				AssertNull("Precondition", branch.OrgProxy);
				AssertEquals("VAT from company since company org proxy exists", "123", InvoiceWrapper.BranchTaxIDNumber);
				AssertEquals("VAT #", InvoiceWrapper.BranchTaxIDHeading);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Qatar))
			{
				branch.GB_OH_OrgProxy = branchOrgProxy.PK;
				AssertNotNull("Precondition", branch.OrgProxy);

				AssertEquals("VAT from branch since branch org proxy exists", "456", InvoiceWrapper.BranchTaxIDNumber);
				AssertEquals("VAT #", InvoiceWrapper.BranchTaxIDHeading);

				branch.GB_OH_OrgProxy = ZGuid.Empty;
				AssertNull("Precondition", branch.OrgProxy);
				AssertEquals("VAT from company since company org proxy exists", "123", InvoiceWrapper.BranchTaxIDNumber);
				AssertEquals("VAT #", InvoiceWrapper.BranchTaxIDHeading);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SaudiArabia))
			{
				branch.GB_OH_OrgProxy = branchOrgProxy.PK;
				AssertNotNull("Precondition", branch.OrgProxy);

				AssertEquals("VAT from branch since branch org proxy exists", "456", InvoiceWrapper.BranchTaxIDNumber);
				AssertEquals("VAT #", InvoiceWrapper.BranchTaxIDHeading);

				branch.GB_OH_OrgProxy = ZGuid.Empty;
				AssertNull("Precondition", branch.OrgProxy);
				AssertEquals("VAT from company since company org proxy exists", "123", InvoiceWrapper.BranchTaxIDNumber);
				AssertEquals("VAT #", InvoiceWrapper.BranchTaxIDHeading);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				branch.GB_OH_OrgProxy = branchOrgProxy.PK;
				AssertNotNull("Precondition", branch.OrgProxy);

				AssertEquals("BranchTaxIDNumber is not applicable for Australia", ZString.Empty, InvoiceWrapper.BranchTaxIDNumber);
				AssertEquals(ZString.Empty, InvoiceWrapper.BranchTaxIDHeading);

				branch.GB_OH_OrgProxy = ZGuid.Empty;
				AssertNull("Precondition", branch.OrgProxy);
				AssertEquals("BranchTaxIDNumber is not applicable for Australia", ZString.Empty, InvoiceWrapper.BranchTaxIDNumber);
				AssertEquals(ZString.Empty, InvoiceWrapper.BranchTaxIDHeading);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Ecuador))
			{
				var ecuador = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.Ecuador);
				var companySRI = "1231231234";
				var branchSRI = "3213213214";
				var branchTaxIDHeading = "AUT. SRI";

				branch.GB_OH_OrgProxy = branchOrgProxy.PK;
				AssertNotNull("Precondition", branch.OrgProxy);
				AssertEquals("SRF from branch is not yet set", "", InvoiceWrapper.BranchTaxIDNumber);

				companyOrgProxy.SetCustomsCode(OrgCusCode.EcuadorCodeTypes.SRI, ecuador, companySRI);
				branchOrgProxy.SetCustomsCode(OrgCusCode.EcuadorCodeTypes.SRI, ecuador, branchSRI);

				Factory.Save();
				AssertEquals("SRI from branch since branch org proxy exists", branchSRI, InvoiceWrapper.BranchTaxIDNumber);
				AssertEquals(branchTaxIDHeading, InvoiceWrapper.BranchTaxIDHeading);

				branch.GB_OH_OrgProxy = ZGuid.Empty;
				AssertNull("Precondition", branch.OrgProxy);
				AssertEquals("SRI from company since company org proxy exists", companySRI, InvoiceWrapper.BranchTaxIDNumber);
				AssertEquals(branchTaxIDHeading, InvoiceWrapper.BranchTaxIDHeading);
			}
		}

		public void TestBranchTaxID_DJ()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Djibouti))
			{
				var companyOrgProxy = Invoice.Company.OrgProxy;
				companyOrgProxy.OH_Code = "OH123";
				companyOrgProxy.OH_RL_NKClosestPort = "DJAII";
				companyOrgProxy.SetCustomsCode(DjiboutiOrgCusCodeInfo.OrgCusCodes.NIF, GlbCompany.CurrentCompany.Country, "789");

				var branchOrgProxy = Factory.New<OrgHeader>();
				branchOrgProxy.OH_Code = "OH456";
				branchOrgProxy.OH_RL_NKClosestPort = "DJAII";
				branchOrgProxy.SetCustomsCode(DjiboutiOrgCusCodeInfo.OrgCusCodes.NIF, GlbCompany.CurrentCompany.Country, "589");

				var branch = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);
				Invoice.AH_GB = branch.PK;

				Factory.Save();

				Invoice.AH_OH = companyOrgProxy.PK;
				InvoiceWrapper = GetBaseInvoiceWrapper();

				branch.GB_OH_OrgProxy = branchOrgProxy.PK;
				AssertNotNull("Precondition", branch.OrgProxy);
				AssertEquals("NIF from branch since branch org proxy exists", "589", InvoiceWrapper.BranchTaxIDNumber);
				AssertEquals("NIF #", InvoiceWrapper.BranchTaxIDHeading);

				branch.GB_OH_OrgProxy = ZGuid.Empty;
				AssertNull("Precondition", branch.OrgProxy);
				AssertEquals("NIF from company since company org proxy exists", "789", InvoiceWrapper.BranchTaxIDNumber);
				AssertEquals("NIF #", InvoiceWrapper.BranchTaxIDHeading);
			}
		}

		public void TestBranchBusRegNumber()
		{
			var india = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.India);

			var companyOrgProxy = Invoice.Company.OrgProxy;
			companyOrgProxy.OH_Code = "OH123";
			companyOrgProxy.OH_RL_NKClosestPort = "INBOM";
			companyOrgProxy.SetCustomsCode(IndiaOrgCusCodeInfo.OrgCusCodes.PAN, india, "123");

			var branchOrgProxy = Factory.New<OrgHeader>();
			branchOrgProxy.OH_Code = "OH456";
			branchOrgProxy.OH_RL_NKClosestPort = "INBOM";
			branchOrgProxy.SetCustomsCode(IndiaOrgCusCodeInfo.OrgCusCodes.PAN, india, "456");

			var branch = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);
			Invoice.AH_GB = branch.PK;

			Factory.Save();

			Invoice.AH_OH = companyOrgProxy.PK;
			InvoiceWrapper = GetBaseInvoiceWrapper();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				branch.GB_OH_OrgProxy = branchOrgProxy.PK;
				AssertNotNull("Precondition", branch.OrgProxy);

				AssertEquals("GST from branch since branch org proxy exists", "456", InvoiceWrapper.BranchBusRegNumber);
				AssertEquals("PAN", InvoiceWrapper.BranchBusRegHeading);

				branch.GB_OH_OrgProxy = ZGuid.Empty;

				AssertNull("Precondition", branch.OrgProxy);
				AssertEquals("GST from branch since company org proxy exists", "123", InvoiceWrapper.BranchBusRegNumber);
				AssertEquals("PAN", InvoiceWrapper.BranchBusRegHeading);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				branch.GB_OH_OrgProxy = branchOrgProxy.PK;
				AssertNotNull("Precondition", branch.OrgProxy);

				AssertEquals("BranchTaxIDNumber is not applicable for Australia", ZString.Empty, InvoiceWrapper.BranchBusRegNumber);
				AssertEquals(ZString.Empty, InvoiceWrapper.BranchBusRegHeading);

				branch.GB_OH_OrgProxy = ZGuid.Empty;

				AssertNull("Precondition", branch.OrgProxy);
				AssertEquals("BranchTaxIDNumber is not applicable for Australia", ZString.Empty, InvoiceWrapper.BranchBusRegNumber);
				AssertEquals(ZString.Empty, InvoiceWrapper.BranchBusRegHeading);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Ecuador))
			{
				var ecuador = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.Ecuador);
				var companySRF = "12-01-12";
				var branchSRF = "12-01-12";

				var branchBusRegHeading = "FECHA AUT. SRI";

				branch.GB_OH_OrgProxy = branchOrgProxy.PK;
				AssertNotNull("Precondition", branch.OrgProxy);

				AssertEquals("SRF from branch is not yet set", "", InvoiceWrapper.BranchBusRegNumber);

				companyOrgProxy.SetCustomsCode(OrgCusCode.EcuadorCodeTypes.SRF, ecuador, companySRF);
				branchOrgProxy.SetCustomsCode(OrgCusCode.EcuadorCodeTypes.SRF, ecuador, branchSRF);

				Factory.Save();

				AssertEquals("SRF from branch since branch org proxy exists", branchSRF, InvoiceWrapper.BranchBusRegNumber);
				AssertEquals(branchBusRegHeading, InvoiceWrapper.BranchBusRegHeading);

				branch.GB_OH_OrgProxy = ZGuid.Empty;
				AssertNull("Precondition", branch.OrgProxy);
				AssertEquals("srf from branch since company org proxy exists", companySRF, InvoiceWrapper.BranchBusRegNumber);
				AssertEquals(branchBusRegHeading, InvoiceWrapper.BranchBusRegHeading);
			}
		}

		public void TestBranchFullName()
		{
			var companyOrgProxy = Invoice.Company.OrgProxy;
			companyOrgProxy.OH_Code = "OH123";
			companyOrgProxy.OH_RL_NKClosestPort = "INBOM";
			companyOrgProxy.OH_FullName = "Company proxy full name";
			var branchOrgProxy = Factory.New<OrgHeader>();
			branchOrgProxy.OH_Code = "OH456";
			branchOrgProxy.OH_RL_NKClosestPort = "INBOM";
			branchOrgProxy.OH_FullName = "Branch proxy full name";
			var branch = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);
			branch.GB_OH_OrgProxy = branchOrgProxy.PK;
			Factory.Save();
			Invoice.AH_GB = branch.PK;
			InvoiceWrapper = GetBaseInvoiceWrapper();
			branch.GB_OH_OrgProxy = branchOrgProxy.PK;
			AssertNotNull("Precondition", branch.OrgProxy);
			AssertEquals("Full name from branch since branch org proxy exists", "Branch proxy full name", InvoiceWrapper.BranchFullName);
			branch.GB_OH_OrgProxy = ZGuid.Empty;
			AssertNull("Precondition", branch.OrgProxy);
			AssertEquals("Full name from branch since company org proxy exists", "Company proxy full name", InvoiceWrapper.BranchFullName);
		}

		#endregion

		public void TestDueDate()
		{
			ZDateTime dueDate = new ZDateTime(2004, 01, 01);
			Invoice.AH_DueDate = dueDate;
			AssertEquals("DueDate", dueDate, InvoiceWrapper.DueDate);
		}

		public void TestGSTAmount()
		{
			ZDecimal gSTAmount = new ZDecimal(12.30);
			Invoice.AH_LocalTaxAmount = gSTAmount;
			AssertEquals("GSTAmount", gSTAmount, InvoiceWrapper.GSTAmount);
		}

		public void TestInvoiceAmount()
		{
			ZDecimal invoiceAmount = new ZDecimal(12.30);
			Invoice.AH_LocalExTaxAmount = invoiceAmount;
			AssertEquals("InvoiceAmount", invoiceAmount, InvoiceWrapper.InvoiceAmount);
		}

		public void TestInvoiceDate()
		{
			ZDateTime invoiceDate = new ZDateTime(2004, 01, 01);
			Invoice.AH_InvoiceDate = invoiceDate;
			AssertEquals("InvoiceDate", invoiceDate, InvoiceWrapper.InvoiceDate);
		}

		public void TestFullyPaidDate()
		{
			ZDateTime fullyPaidDate = new ZDateTime(2004, 01, 01);
			Invoice.AH_FullyPaidDate = fullyPaidDate;
			AssertEquals("FullyPaidDate", fullyPaidDate, InvoiceWrapper.FullyPaidDate);
		}

		public void TestPostDate()
		{
			ZDateTime postDate = new ZDateTime(2004, 01, 01);
			Invoice.AH_PostDate = postDate;
			AssertEquals("PostDate", postDate, InvoiceWrapper.PostDate);
		}

		public void TestExchangeRateForReciprocalCompany()
		{
			ExchangeRateTestHelper(true);
		}

		public void TestExchangeRateForNonReciprocalCompany()
		{
			ExchangeRateTestHelper(false);
		}

		void ExchangeRateTestHelper(bool isReciprocal)
		{
			var backup = TestObjectCreator.SetCurrentCompanyReciprocal(isReciprocal);
			try
			{
				ZDecimal exchangeRate = new ZDecimal(12.12345678);
				Invoice.AH_ExchangeRate = exchangeRate;
				var expectedValue = Utilities.Round(exchangeRate, GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces);
				AssertEquals("ExchangeRate", expectedValue, InvoiceWrapper.ExchangeRate);
			}
			finally
			{
				TestObjectCreator.SetCurrentCompanyReciprocal(backup);
			}
		}

		public virtual void TestOSTotal()
		{
			ZDecimal oSTotal = new ZDecimal(123.34);
			Invoice.AH_OSTotalAmount = oSTotal;
			AssertEquals("OSTotal", oSTotal, InvoiceWrapper.OSTotal);
		}

		public void TestOutstandingAmount()
		{
			ZDecimal outstandingAmount = new ZDecimal(123.34);
			Invoice.AH_OutstandingAmount = outstandingAmount;
			AssertEquals("OutstandingAmount", outstandingAmount, InvoiceWrapper.OutstandingAmount);
		}

		public void TestWithholdingTax()
		{
			ZDecimal withholdingTax = new ZDecimal(1);
			Invoice.AH_WithholdingTax = withholdingTax;
			AssertEquals("WithholdingTax", withholdingTax, InvoiceWrapper.WithholdingTax);
		}

		public void TestInvoiceTermDays()
		{
			Invoice.AH_InvoiceTermDays = (ZByte)1;
			AssertEquals("InvoiceTermDays", 1, Convert.ToInt32(InvoiceWrapper.InvoiceTermDays));
		}

		public void TestTransactionCount()
		{
			Invoice.AH_TransactionCount = 1;
			AssertEquals("TransactionCount", 1, Convert.ToInt32(InvoiceWrapper.TransactionCount));
		}

		public void TestTransactionBelongsToGroup()
		{
			Invoice.AH_TransactionBelongsToGroup = ZGuid.Empty;
			AssertEquals("TransactionBelongsToGroup", ZGuid.Empty, InvoiceWrapper.TransactionBelongsToGroup);

			ZGuid transactionBelongsToGroup = ZGuid.NewZGuid();
			Invoice.AH_TransactionBelongsToGroup = transactionBelongsToGroup;
			AssertEquals("TransactionBelongsToGroup", transactionBelongsToGroup, InvoiceWrapper.TransactionBelongsToGroup);
		}

		//Case - Bollo tax rate is of Exempt type
		public void TestStampDutyTaxMessageIsCrossReferencedForRolledUpLines_Case1()
		{
			TestCase_StampDutyTaxMessageIsCrossReferencedForRolledUpLines(AccTaxRate.Types.Exempt);
		}

		//Case - Bollo tax rate is of Not Reportable type
		public void TestStampDutyTaxMessageIsCrossReferencedForRolledUpLines_Case2()
		{
			TestCase_StampDutyTaxMessageIsCrossReferencedForRolledUpLines(AccTaxRate.Types.NotReportable);
		}

		void TestCase_StampDutyTaxMessageIsCrossReferencedForRolledUpLines(ZString stampDutyTaxType)
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			#region Setup Stamp Duty settings

			var taxInvMsg1 = Factory.NewWithValidTestData<AccInvMsg>();
			taxInvMsg1.A9_Code = "AAA";
			taxInvMsg1.A9_EnglishMsg = "English AAA";
			taxInvMsg1.A9_LocalMsg = "Local AAA";
			taxInvMsg1.A9_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;

			var aRT7 = Factory.NewWithValidTestData<AccTaxRate>();
			aRT7.AT_Code = "ART7";
			aRT7.AT_Type = AccTaxRate.Types.Rated;
			aRT7.AT_RN_NKCountry = Core.Constants.CountryCodes.Italy;
			aRT7.SetRateNumerator_ForTestOnly(0);
			aRT7.AT_A9_DefaultVatClass = taxInvMsg1.PK;

			var taxInvMsg2 = Factory.NewWithValidTestData<AccInvMsg>();
			taxInvMsg2.A9_Code = "Bollo";
			taxInvMsg2.A9_EnglishMsg = "English Bollo";
			taxInvMsg2.A9_LocalMsg = "Local Bollo";
			taxInvMsg2.A9_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;

			var aRT2 = Factory.NewWithValidTestData<AccTaxRate>();
			aRT2.AT_Code = "ART2";
			aRT2.AT_Type = AccTaxRate.Types.Rated;
			aRT2.AT_RN_NKCountry = Core.Constants.CountryCodes.Italy;
			aRT2.SetRateNumerator_ForTestOnly(0);

			var bolloTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
			bolloTaxRate.AT_Code = "BOLLOTAX";
			bolloTaxRate.AT_Type = stampDutyTaxType;
			bolloTaxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.Italy;
			bolloTaxRate.SetRateNumerator_ForTestOnly(0);

			Factory.Save();

			string value = aRT7.PK.ToString() + "," + aRT2.PK.ToString();
			AccountingConfigurationRegistry.Instance.TaxIDsAttractingStampDuty.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, value);
			AccountingConfigurationRegistry.Instance.StampDutyFixedAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 1.81m);
			AccountingConfigurationRegistry.Instance.StampDutyThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 77.47m);

			var chargeCode = testObjectCreator.CreateChargeCode("BOLLO", "Stamp Duty", "MRG", 1m, bolloTaxRate, null);
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			AccountingConfigurationRegistry.Instance.StampDutyChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.StampDutyInvoiceTaxMessage.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, taxInvMsg2.PK.ToGuid());

			Factory.Save();

			#endregion

			AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.Both.Code);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Italy);
			Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).SetCountry(Core.Constants.CountryCodes.Italy);

			var italyOrg = TestObjectCreator.CreateOrgHeader("ITORG", false, true, "ITROM");

			ARInvoice aRInvoice1 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			aRInvoice1.AH_OH = italyOrg.PK;

			aRInvoice1.Lines.AddNew();
			aRInvoice1.Lines.AddNew();
			aRInvoice1.Lines.AddNew();
			aRInvoice1.Lines.AddNew();

			var chargeCode1 = TestObjectCreator.CC1;
			chargeCode1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			var chargeCode2 = TestObjectCreator.CC1;
			chargeCode2.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			SetLineDetails(aRInvoice1.Lines[0], chargeCode1, aRT7, 120m, 12m, TestObjectCreator.Job1);
			TestObjectCreator.CreateJobCharge(aRInvoice1.Lines[0], TestObjectCreator.Job1, chargeCode1, TestObjectCreator.AUD);

			SetLineDetails(aRInvoice1.Lines[1], chargeCode2, aRT7, 70m, 7m, TestObjectCreator.Job1);
			TestObjectCreator.CreateJobCharge(aRInvoice1.Lines[1], TestObjectCreator.Job1, chargeCode1, TestObjectCreator.AUD);

			SetLineDetails(aRInvoice1.Lines[2], chargeCode1, TestObjectCreator.GSTFREE1, 70m, 0m, TestObjectCreator.Job1);
			TestObjectCreator.CreateJobCharge(aRInvoice1.Lines[2], TestObjectCreator.Job1, chargeCode1, TestObjectCreator.AUD);

			SetLineDetails(aRInvoice1.Lines[3], chargeCode2, aRT2, 210m, 21m, TestObjectCreator.Job1);
			TestObjectCreator.CreateJobCharge(aRInvoice1.Lines[3], TestObjectCreator.Job1, chargeCode1, TestObjectCreator.AUD);

			Factory.Save();

			DocARInvoice docInvoice = DocARInvoice.New(aRInvoice1, Factory);

			AssertContains("Tax message must contain Local Bollo", "Local Bollo", docInvoice.LocalLanguageTaxMessagesWithNumbers);

			var linesGroupedByJob = docInvoice.InvoiceLineByJob;
			AssertNotNull(linesGroupedByJob);
			AssertEquals("Lines will grouped in 4 groupes. All with job numbers and the bollo line without job number", 4, linesGroupedByJob.Count);

			var bolloLine = linesGroupedByJob.FirstOrDefault(x => ((DocARInvoiceLineForRollUp)x).JobNumber.IsEmpty);
			AssertNotNull("The bollo line does not have any job number", bolloLine);

			AssertNotEquals("Bollo line should have some asterisk notation", string.Empty, ((DocARInvoiceLineForRollUp)bolloLine).TaxRateAsterisksAsNumbers);
		}

		public void TestAmountSplittedByChargeCodeForRollupByJobLines()
		{
			AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxAmount.Code);

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			ARInvoice aRInvoice1 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;

			aRInvoice1.Lines.AddNew();
			aRInvoice1.Lines.AddNew();
			aRInvoice1.Lines.AddNew();
			aRInvoice1.Lines.AddNew();

			var chargeCode1 = testObjectCreator.CC1;
			chargeCode1.AC_ChargeGroup = "FRT";
			var chargeCode2 = testObjectCreator.CC2;
			chargeCode2.AC_ChargeGroup = "FRT";

			SetLineDetails(aRInvoice1.Lines[0], chargeCode1, testObjectCreator.GST1, 120m, 12m, testObjectCreator.Job1);
			testObjectCreator.CreateJobCharge(aRInvoice1.Lines[0], testObjectCreator.Job1, chargeCode1, testObjectCreator.AUD);

			SetLineDetails(aRInvoice1.Lines[1], chargeCode2, testObjectCreator.GST1, 70m, 7m, testObjectCreator.Job1);
			testObjectCreator.CreateJobCharge(aRInvoice1.Lines[1], testObjectCreator.Job1, chargeCode1, testObjectCreator.AUD);

			SetLineDetails(aRInvoice1.Lines[2], chargeCode1, testObjectCreator.GSTFREE1, 70m, 0m, testObjectCreator.Job1);
			testObjectCreator.CreateJobCharge(aRInvoice1.Lines[2], testObjectCreator.Job1, chargeCode1, testObjectCreator.AUD);

			SetLineDetails(aRInvoice1.Lines[3], chargeCode2, testObjectCreator.GST1, 210m, 21m, testObjectCreator.Job1);
			testObjectCreator.CreateJobCharge(aRInvoice1.Lines[3], testObjectCreator.Job1, chargeCode1, testObjectCreator.AUD);

			Factory.Save();

			AssertEquals("OS Amount", 132m, aRInvoice1.Lines[0].AL_OSAmount);
			AssertEquals("OS Amount", 77m, aRInvoice1.Lines[1].AL_OSAmount);
			AssertEquals("OS Amount", 70m, aRInvoice1.Lines[2].AL_OSAmount);
			AssertEquals("OS Amount", 231m, aRInvoice1.Lines[3].AL_OSAmount);

			DocARInvoice docInvoice = DocARInvoice.New(aRInvoice1, Factory);

			AssertNotNull(docInvoice.InvoiceLineByJob);
			AssertEquals(1, docInvoice.InvoiceLineByJob.Count);
			AssertEquals("Total for chargeCode1", 202m, (docInvoice.InvoiceLineByJob[0].AmountSplittedByChargeCode[chargeCode1.AC_Code]).TotalAmount);
			AssertEquals("Total for chargeCode2", 308m, (docInvoice.InvoiceLineByJob[0].AmountSplittedByChargeCode[chargeCode2.AC_Code]).TotalAmount);
		}

		public void TestCommentLineIsEliminatedWhenRollupByJob()
		{
			AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.Both.Code);

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			ARInvoice aRInvoice1 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			aRInvoice1.Lines.AddNew();
			aRInvoice1.Lines.AddNew();
			SetLineDetails(aRInvoice1.Lines[0], testObjectCreator.CC1, testObjectCreator.GST1, 120m, 12m, testObjectCreator.Job1);
			testObjectCreator.CreateJobCharge(aRInvoice1.Lines[0], testObjectCreator.Job1, testObjectCreator.CC1, testObjectCreator.AUD);
			SetLineDetails(aRInvoice1.Lines[1], testObjectCreator.CommentChargeCode, null, 0m, 0m, testObjectCreator.Job1);
			testObjectCreator.CreateJobCharge(aRInvoice1.Lines[1], testObjectCreator.Job1, testObjectCreator.CommentChargeCode, testObjectCreator.AUD);

			ARInvoice aRInvoice2 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			aRInvoice2.Lines.AddNew();
			SetLineDetails(aRInvoice2.Lines[0], testObjectCreator.CommentChargeCode, null, 0m, 0m, testObjectCreator.Job2);
			testObjectCreator.CreateJobCharge(aRInvoice2.Lines[0], testObjectCreator.Job2, testObjectCreator.CommentChargeCode, testObjectCreator.AUD);

			Factory.Save();

			DocARInvoice docInvoice1 = DocARInvoice.New(aRInvoice1, Factory);
			AssertEquals("Precondition", 2, docInvoice1.InvoiceLine.Count);
			AssertEquals(testObjectCreator.CC1, docInvoice1.InvoiceLine[0].ChargeCode.AccChargeCode);
			AssertEquals(testObjectCreator.CommentChargeCode, docInvoice1.InvoiceLine[1].ChargeCode.AccChargeCode);
			AssertNotNull(docInvoice1.InvoiceLineByJob);
			AssertEquals(1, docInvoice1.InvoiceLineByJob.Count);
			AssertEquals("Postcondition", 1, docInvoice1.InvoiceLine.Count);
			AssertEquals(testObjectCreator.CC1, docInvoice1.InvoiceLine[0].ChargeCode.AccChargeCode);
			AssertEquals("OSExTaxAmount", 120m, docInvoice1.InvoiceLineByJob[0].OSExTaxAmount);

			DocARInvoice docInvoice2 = DocARInvoice.New(aRInvoice2, Factory);
			AssertEquals("Precondition", 1, docInvoice2.InvoiceLine.Count);
			AssertEquals(testObjectCreator.CommentChargeCode, docInvoice2.InvoiceLine[0].ChargeCode.AccChargeCode);
			AssertNotNull(docInvoice2.InvoiceLineByJob);
			AssertEquals(1, docInvoice2.InvoiceLineByJob.Count);
			AssertEquals("Postcondition", 1, docInvoice2.InvoiceLine.Count);
			AssertEquals(testObjectCreator.CommentChargeCode, docInvoice2.InvoiceLine[0].ChargeCode.AccChargeCode);
			AssertEquals("OSExTaxAmount", 0m, docInvoice2.InvoiceLineByJob[0].OSExTaxAmount);
		}

		public void TestOSTaxDisplayForRollUpLine()
		{
			RefCurrency aUD = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_RX_NKTransactionCurrency = aUD.RX_Code;
			DocARBaseInvoiceForTest testBizO = new DocARBaseInvoiceForTest(invoice, Factory);
			DocARInvoiceLine.AmountWithExchangeRate amountWithExchangeRate = new DocARInvoiceLine.AmountWithExchangeRate();

			DocARInvoiceLineForRollUp rollUpLine = testBizO.CreateWrapperForRolledUpLineForTest("bla-bla", 111.11, 11.111, 111, 11, amountWithExchangeRate);
			AssertEquals("GST Amount", "11.11", rollUpLine.OSTaxDisplay);
			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				rollUpLine = testBizO.CreateWrapperForRolledUpLineForTest("bla-bla", 111.11, 11.111, 111, 11, amountWithExchangeRate);
				AssertEquals("GST Amount", "11,11", rollUpLine.OSTaxDisplay);
			}

			RefCurrency vND = RefCurrency.LoadFromCurrencyCode(Factory, "VND");
			invoice.AH_RX_NKTransactionCurrency = vND.RX_Code;
			testBizO = new DocARBaseInvoiceForTest(invoice, Factory);
			rollUpLine = testBizO.CreateWrapperForRolledUpLineForTest("bla-bla", 111.11, 11.111, 111, 11, amountWithExchangeRate);
			AssertEquals("GST Amount", "11", rollUpLine.OSTaxDisplay);
		}

		public void TestGetOSTaxAmountDisplayForSingleRollUpLineMYSER()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Malaysia))
			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Malaysia))
			using (AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.Both.Code))
			{
				var myr = RefCurrency.LoadFromCurrencyCode(Factory, "MYR");
				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				invoice.AH_RX_NKTransactionCurrency = myr.RX_Code;
				var amountWithExchangeRate = new DocARInvoiceLine.AmountWithExchangeRate();

				var taxRate = TestObjectCreator.CreateTaxRate("SVC", "Service Tax", AccTaxRate.Types.ServiceTax, 6, AccTaxRate.ExtraTypes.ServiceTax, 6, 1);
				var line = (InvoiceLine)invoice.Lines.AddNew();
				line.AL_AT = taxRate.PK;
				line.AL_A9_VATClass = TestObjectCreator.TaxMsg1.PK;
				line.AL_LocalExTaxAmount = 20m;
				line.AL_LocalTaxAmount = 1.2m;

				var docLine = DocARInvoiceLine.New(line, Factory);
				var collection = new DocARInvoiceLineCollection(Factory);
				collection.Add(docLine);

				var docInvoiceForRolledUp = new DocARBaseInvoiceForTest(invoice, Factory).
					CreateWrapperForRolledUpLineForGroupingForTest(new ZString("Test01"), 100, 10, 90, 9, amountWithExchangeRate, collection, null);

				AssertEquals(1.2m, collection.TotalOSSERAmount);
				AssertEquals("SERVICE TAX\r\n6%=1.20", docInvoiceForRolledUp.GetOSTaxAmountDisplayWithRegistryRule());
				AssertEquals("SERVICE TAX\r\n6%=1.20", docInvoiceForRolledUp.OSTaxDisplayNoAsterisksWithRegistryRule);
			}
		}

		public void TestGetOSTaxAmountDisplayForMultiRollUpLineMYSER()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Malaysia))
			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Malaysia))
			using (AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.Both.Code))
			{
				var myr = RefCurrency.LoadFromCurrencyCode(Factory, "MYR");
				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				invoice.AH_RX_NKTransactionCurrency = myr.RX_Code;
				var amountWithExchangeRate = new DocARInvoiceLine.AmountWithExchangeRate();

				var taxRate1 = TestObjectCreator.CreateTaxRate("SVC", "Service Tax", AccTaxRate.Types.ServiceTax, 6, AccTaxRate.ExtraTypes.ServiceTax, 6, 1);
				var line1 = (InvoiceLine)invoice.Lines.AddNew();
				line1.AL_AT = taxRate1.PK;
				line1.AL_A9_VATClass = TestObjectCreator.TaxMsg1.PK;
				line1.AL_LocalExTaxAmount = 20m;
				line1.AL_LocalTaxAmount = 1.2m;

				var line2 = (InvoiceLine)invoice.Lines.AddNew();
				line2.AL_AT = taxRate1.PK;
				line2.AL_A9_VATClass = TestObjectCreator.TaxMsg1.PK;
				line2.AL_LocalExTaxAmount = 10m;
				line2.AL_LocalTaxAmount = 0.3m;

				var collection = new DocARInvoiceLineCollection(Factory);
				collection.Add(DocARInvoiceLine.New(line1, Factory));
				collection.Add(DocARInvoiceLine.New(line2, Factory));

				var docInvoiceForRolledUp = new DocARBaseInvoiceForTest(invoice, Factory).
					CreateWrapperForRolledUpLineForGroupingForTest(new ZString("Test02"), 100, 10, 90, 9, amountWithExchangeRate, collection, null);

				AssertEquals(1.5m, collection.TotalOSSERAmount);
				AssertEquals("SERVICE TAX\r\n6%=1.50", docInvoiceForRolledUp.GetOSTaxAmountDisplayWithRegistryRule());
				AssertEquals("SERVICE TAX\r\n6%=1.50", docInvoiceForRolledUp.OSTaxDisplayNoAsterisksWithRegistryRule);
			}
		}

		public void TestGetOSTaxAmountDisplayForRollUpLineWithDifferentTaxRateMYSER()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Malaysia))
			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Malaysia))
			using (AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.Both.Code))
			{
				var myr = RefCurrency.LoadFromCurrencyCode(Factory, "MYR");
				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				invoice.AH_RX_NKTransactionCurrency = myr.RX_Code;
				var amountWithExchangeRate = new DocARInvoiceLine.AmountWithExchangeRate();

				var taxRate1 = TestObjectCreator.CreateTaxRate("SVC", "Service Tax", AccTaxRate.Types.ServiceTax, 6, AccTaxRate.ExtraTypes.ServiceTax, 6, 1);
				var line1 = (InvoiceLine)invoice.Lines.AddNew();
				line1.AL_AT = taxRate1.PK;
				line1.AL_A9_VATClass = TestObjectCreator.TaxMsg1.PK;
				line1.AL_LocalExTaxAmount = 20m;
				line1.AL_LocalTaxAmount = 1.2m;

				var taxRate2 = TestObjectCreator.CreateTaxRate("SV2", "Service Tax 2", AccTaxRate.Types.ServiceTax, 3, AccTaxRate.ExtraTypes.ServiceTax, 3, 1);
				var line2 = (InvoiceLine)invoice.Lines.AddNew();
				line2.AL_AT = taxRate2.PK;
				line2.AL_A9_VATClass = TestObjectCreator.TaxMsg1.PK;
				line2.AL_LocalExTaxAmount = 10m;
				line2.AL_LocalTaxAmount = 0.3m;

				var collection = new DocARInvoiceLineCollection(Factory);
				collection.Add(DocARInvoiceLine.New(line1, Factory));
				collection.Add(DocARInvoiceLine.New(line2, Factory));

				var docInvoiceForRolledUp = new DocARBaseInvoiceForTest(invoice, Factory).
					CreateWrapperForRolledUpLineForGroupingForTest(new ZString("Test03"), 100, 10, 90, 9, amountWithExchangeRate, collection, null);

				AssertEquals(1.5m, collection.TotalOSSERAmount);
				AssertEquals("N/A", docInvoiceForRolledUp.GetOSTaxAmountDisplayWithRegistryRule());
				AssertEquals("10.00", docInvoiceForRolledUp.OSTaxDisplayNoAsterisksWithRegistryRule);
			}
		}

		public void TestOSTaxDisplayForRollUpLineForNotReportableTaxRate()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Type = AccTaxRate.Types.NotReportable;
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var line = (InvoiceLine)invoice.Lines.AddNew();
			line.AL_AT = taxRate.PK;
			line.AL_A9_VATClass = TestObjectCreator.TaxMsg1.PK;

			DocARInvoice docInvoice = DocARInvoice.New(invoice, Factory);
			DocARInvoiceLineCollection rollupLines = docInvoice.InvoiceLineByCharge;
			AssertEquals("expect 1 rollup line", 1, rollupLines.Count);
			AssertEquals("1.", rollupLines[0].TaxRateAsterisksAsNumbers);
			AssertEquals("Not Applicable *", rollupLines[0].OSTaxDisplay);
		}

		public void TestOSTaxDisplayForRollUpLineWhenChargeCodeIsCommentType()
		{
			DocARBaseInvoiceForTest testBizO = new DocARBaseInvoiceForTest(Invoice, Factory);
			DocARInvoiceLine.AmountWithExchangeRate amountWithExchangeRate = new DocARInvoiceLine.AmountWithExchangeRate();
			AccChargeCode cMTCharge = Factory.NewWithValidTestData<AccChargeCode>();
			cMTCharge.AC_ChargeType = "CMT";
			DocChargeCode docCMTCharge = DocChargeCode.New(cMTCharge, Factory);
			DocARInvoiceLineForRollUp rollUpLine = testBizO.CreateWrapperForRolledUpLineForTest("this is comment only", 0, 0, 0, 0, amountWithExchangeRate, docCMTCharge);
			AssertEquals("expect blank if charge is CMT type", "", rollUpLine.OSTaxDisplay);

			AccChargeCode fRTCharge = Factory.NewWithValidTestData<AccChargeCode>();
			fRTCharge.AC_ChargeType = "FRT";
			DocChargeCode docFRTCharge = DocChargeCode.New(fRTCharge, Factory);
			rollUpLine = testBizO.CreateWrapperForRolledUpLineForTest("this is FRT charge", 0, 0, 0, 0, amountWithExchangeRate, docFRTCharge);
			AssertEquals("expect N/A if charge is non-CMT type", "N/A", rollUpLine.OSTaxDisplay);

			rollUpLine = testBizO.CreateWrapperForRolledUpLineForTest("this is FRT charge", 0, 0, 0, 0, amountWithExchangeRate, null);
			AssertEquals("expect N/A if charge is null", "N/A", rollUpLine.OSTaxDisplay);
		}

		public void TestAccountOrgWrapperContextIsSetFromParent()
		{
			InvoiceWrapper.SetReportNameForTesting("Test invoice report name");
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_Code = "ORG";
			Invoice.AH_OH = header.PK;
			AssertEquals("Test invoice report name", InvoiceWrapper.AccountOrg.ReportName);
		}

		public void TestStampDutyARDocumentMessageByEvent()
		{
			var testWrapper = new DocARBaseInvoiceForTest(Invoice, Factory);
			Assert("TestStampDutyARDocumentMessage should be empty by default", testWrapper.StampDutyARDocumentMessage.IsEmpty);
			AccountingConfigurationRegistry.Instance.StampDutyARDocumentMessage.SetValue(TestObjectCreator.NonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "OtherCompany Settings should not have any effect");
			Assert("TestStampDutyARDocumentMessage should still be empty", testWrapper.StampDutyARDocumentMessage.IsEmpty);

			if (Invoice.GetType().IsAssignableFrom(typeof(ARInvoice)) || Invoice.GetType().IsAssignableFrom(typeof(ARCreditNote)))
			{
				AccountingConfigurationRegistry.Instance.StampDutyARDocumentMessage.SetValue(Invoice.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, "This is our StampDutyARDocumentMessage");
				Assert("TestStampDutyARDocumentMessage should still be empty", testWrapper.StampDutyARDocumentMessage.IsEmpty);

				var sdlEvent = Invoice.Logs.AddNew(AutoEvents.StampDutyLiability);
				AssertEquals("TestStampDutyARDocumentMessage", "This is our StampDutyARDocumentMessage", testWrapper.StampDutyARDocumentMessage);

				sdlEvent.Cancel();
				Assert("TestStampDutyARDocumentMessage should be empty as Event is cancelled", testWrapper.StampDutyARDocumentMessage.IsEmpty);

				Invoice.Logs.AddNew(AutoEvents.StampDutyLiability);
				AssertEquals("TestStampDutyARDocumentMessage", "This is our StampDutyARDocumentMessage", testWrapper.StampDutyARDocumentMessage);

				AccountingConfigurationRegistry.Instance.StampDutyARDocumentMessage.SetValue(Invoice.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, string.Empty);
				Assert("TestStampDutyARDocumentMessage should be empty again", testWrapper.StampDutyARDocumentMessage.IsEmpty);
			}
		}

		public void TestStampDutyARDocumentMessageByStampDutyChargeLine()
		{
			var testWrapper = new DocARBaseInvoiceForTest(Invoice, Factory);
			Assert("TestStampDutyARDocumentMessage should be empty by default", testWrapper.StampDutyARDocumentMessage.IsEmpty);
			AccountingConfigurationRegistry.Instance.StampDutyARDocumentMessage.SetValue(TestObjectCreator.NonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "OtherCompany Settings should not have any effect");
			Assert("TestStampDutyARDocumentMessage should still be empty", testWrapper.StampDutyARDocumentMessage.IsEmpty);

			if (Invoice.GetType().IsAssignableFrom(typeof(ARInvoice)) || Invoice.GetType().IsAssignableFrom(typeof(ARCreditNote)))
			{
				AccountingConfigurationRegistry.Instance.StampDutyChargeCode.SetValue(Invoice.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CC11.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.StampDutyFixedAmount.SetValue(Invoice.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, 10m);
				AccountingConfigurationRegistry.Instance.StampDutyARDocumentMessage.SetValue(Invoice.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, "This is our StampDutyARDocumentMessage");
				Assert("TestStampDutyARDocumentMessage should still be empty", testWrapper.StampDutyARDocumentMessage.IsEmpty);

				var stampDutyLine = TestObjectCreator.CreateInvoiceLine((InvoicingBase)Invoice, 10m);
				stampDutyLine.AL_AC = TestObjectCreator.CC11.PK;
				AssertEquals("TestStampDutyARDocumentMessage", "This is our StampDutyARDocumentMessage", testWrapper.StampDutyARDocumentMessage);

				AccountingConfigurationRegistry.Instance.StampDutyARDocumentMessage.SetValue(Invoice.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, string.Empty);
				Assert("TestStampDutyARDocumentMessage should be empty again", testWrapper.StampDutyARDocumentMessage.IsEmpty);
			}
		}

		public void TestEnglishLanguageTaxMessagesWithNumbersWithDeletedLines()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Type = AccTaxRate.Types.NotReportable;
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var line = (InvoiceLine)invoice.Lines.AddNew();
			line.AL_AT = taxRate.PK;
			line.AL_A9_VATClass = TestObjectCreator.TaxMsg1.PK;
			var docInvoice = DocARInvoice.New(invoice, Factory);
			docInvoice.Lines.OfType<DocARInvoiceLine>().ForEach(x => x.Line.Delete());
			Assert("InvocingLine is deleted", ((DocARInvoiceLine)docInvoice.Lines[0]).Line.IsDeleted);
			AssertEquals("Get EnglishLanguageTaxMessagesWithNumbers with no exceptions when DocARInvoiceLine has deleted invocingLines", ZString.Empty, docInvoice.EnglishLanguageTaxMessagesWithNumbers);
		}

		public virtual void TestInvoiceLineByNON()
		{
			var docInvoice = DocARInvoice.New(Factory.NewWithValidTestData<ARInvoice>(), Factory);
			AssertEquals("InvoiceLineByNON count", 0, docInvoice.InvoiceLineByNON.Count);
		}

		public void TestRollUpDoesNotCauseErrorForTaxCoreCountries()
		{
			foreach (var countryCode in TaxCoreCountryHelper.GetTaxCoreSupportedCountries())
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var testObjectCreator = new TestObjectCreator(Factory);
					var docARBaseInvoiceForTest = new DocARBaseInvoiceForTest(InvoicingBase, Factory);
					var line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
					line.AL_AT = testObjectCreator.FREECAPGST.PK;

					docARBaseInvoiceForTest.InvoiceLine.Add(DocARInvoiceLine.New(line, Factory));
					AssertNoExceptionThrown(() => { var result = docARBaseInvoiceForTest.TestRollUpGrouperByChargeAndTaxForRollupForTest(); });
				}
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			InvoiceBatch = Factory.New<InvoiceBatchHeader>();
			BatchedInvoice = Factory.New<ARInvoice>();

			BatchedInvoice.AH_AH_InvoiceStatement = InvoiceBatch.PK;

			InvoiceBatch.Line.Add(BatchedInvoice);
			InvoiceBatch.AH_TransactionNum = "00001001";

			Invoice = GetWrappedInvoice();
			Invoice.AH_TransactionNum = "TESTINVFORWRAP001";
			InvoiceWrapper = GetBaseInvoiceWrapper();

			base.SetUp();
		}

		protected InvoicingBase InvoicingBase
		{
			get
			{
				if (fInvoicingBase == null)
				{
					fInvoicingBase = Invoice as InvoicingBase;
					if (fInvoicingBase == null)
					{
						fInvoicingBase = Factory.New<ARInvoice>();
					}
				}
				return fInvoicingBase;
			}

			set
			{
				fInvoicingBase = value;
			}
		}
		InvoicingBase fInvoicingBase;

		internal static void SetupForTestingInvoiceTaxMessages(InvoicingBase invoice, bool makeLocalMessageEmpty = false)
		{
			var random = new Random();
			var objectCreator = new TestObjectCreator(invoice.Factory);

			var chargeCodes = new AccChargeCode[] { objectCreator.CC1, objectCreator.CC2, objectCreator.CC3, objectCreator.FRT, objectCreator.CC5 };

			AccTaxRate[] taxRates = new AccTaxRate[5];
			AccInvMsg[] messages = new AccInvMsg[6];

			invoice.AH_TransactionNum = RandomString(10);
			var localClient = objectCreator.CreateOrgHeader(RandomString(5), false, true);
			var agent = objectCreator.CreateOrgHeader(RandomString(5), false, true);
			var shipment = objectCreator.CreateShipment(RandomString(7));
			var job = objectCreator.CreateJob(shipment, localClient, 0M, agent, 0M);

			for (int i = 0; i < 6; i++)
			{
				messages[i] = invoice.Factory.NewWithValidTestData<AccInvMsg>();
				messages[i].A9_EnglishMsg = "ENGLISH " + (i + 1);
				messages[i].A9_LocalMsg = makeLocalMessageEmpty ? "" : "LOCAL " + (i + 1);

				if (i < 5)
				{
					taxRates[i] = invoice.Factory.NewWithValidTestData<AccTaxRate>();
					taxRates[i].AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

					chargeCodes[i].AC_PrintSequence = (ZShort)(5 - i);
					InvoicingLineBase line = objectCreator.CreateInvoiceLine(invoice, job, chargeCodes[i], 500m);
					line.AL_AT = taxRates[i].PK;
					line.AL_A9_VATClass = messages[i].PK;

					objectCreator.CreateJobCharge(line, job, chargeCodes[i]);
				}
			}

			string RandomString(int length)
			{
				const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
				return new string(Enumerable.Repeat(chars, length)
				  .Select(s => s[random.Next(s.Length)]).ToArray());
			}
		}

		protected void SetLineDetails(InvoicingLineBase line, AccChargeCode chargeCode, AccTaxRate tax, ZDecimal exTaxAmount, ZDecimal taxAmount, JobHeader job)
		{
			line.AL_JH = job != null ? job.PK : ZGuid.Empty;
			SetLineDetails(line, chargeCode, tax, exTaxAmount, taxAmount);
		}

		protected void SetLineDetails(InvoicingLineBase line, AccChargeCode chargeCode, AccTaxRate tax, ZDecimal exTaxAmount, ZDecimal taxAmount)
		{
			line.AL_AC = chargeCode.PK;
			line.AL_AT = tax != null ? tax.PK : ZGuid.Empty;
			line.AL_OSExTaxAmount = exTaxAmount;
			line.AL_OSTaxAmount = taxAmount;
		}

		protected virtual void SetupCountryForTestingInvoiceTaxMessages()
		{
		}

		protected virtual InvoicingBase SetupForTestingInvoiceTaxMessagesCore(string organisationUnloco)
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsDebtor = true;
			org.OH_RL_NKClosestPort = organisationUnloco;

			return SetupForTestingInvoiceTaxMessagesCore(org);
		}

		protected InvoicingBase SetupForTestingInvoiceTaxMessagesCore(OrgHeader org)
		{
			Invoice = GetWrappedInvoice();
			Invoice.AH_TransactionNum = "TESTINVFORWRAP002";
			InvoicingBase result = Invoice as InvoicingBase;
			AssertNotNull(result);
			result.AH_OH = org.PK;
			return result;
		}

		protected Mock<ICountryComplianceInfo> GetCountryComplianceInfoMock()
		{
			var mockComplianceFactory = new Mock<ICountryComplianceFactoryIntegration>();
			var mockCountryComplianceInfo = new Mock<ICountryComplianceInfo>();
			mockComplianceFactory.Setup(x => x.GetICountryComplianceInfo(It.IsAny<ZString>())).Returns(mockCountryComplianceInfo.Object);
			ObjectFactory.Substitute(mockComplianceFactory.Object);

			return mockCountryComplianceInfo;
		}

		protected TransactionHeader Invoice;
		protected InvoicingBase BatchedInvoice;
		protected InvoiceBatchHeader InvoiceBatch;

		protected DocARBaseInvoice InvoiceWrapper;

		public class DocARBaseInvoiceForTest : DocARBaseInvoice
		{
			public DocARBaseInvoiceForTest(TransactionHeader wrappedInvoice, BusinessObjectFactory factoryToWrap)
				: base(wrappedInvoice, factoryToWrap)
			{
			}

			public DocARInvoiceLineForRollUp CreateWrapperForRolledUpLineForTest(ZString description, ZDecimal amount, ZDecimal gSTAmount, ZDecimal localAmount, ZDecimal localGSTVAT, DocARInvoiceLine.AmountWithExchangeRate exchangeRates, DocChargeCode chargeCode = null)
			{
				return DocLineRollUpper.CreateWrapperForRolledUpLine(description, amount, gSTAmount, localAmount, localGSTVAT, exchangeRates, chargeCode);
			}

			public DocARInvoiceLineForRollUp CreateWrapperForRolledUpLineForGroupingForTest(IZType description, ZDecimal amount, ZDecimal gSTAmount, ZDecimal localAmount, ZDecimal localGSTVAT,
				DocARInvoiceLine.AmountWithExchangeRate exchangeRates, DocARInvoiceLineCollection rolledUpLines, DocChargeCode chargeCode, bool isSpacerLine = false)
			{
				var rollUpWrapper = DocLineRollUpper.CreateWrapperForRolledUpLine(description, amount, gSTAmount, localAmount, localGSTVAT, exchangeRates, chargeCode);
				rollUpWrapper.IsSpacerLine = isSpacerLine;
				DocLineRollUpper.PrepareRollUpLineForGrouping(rollUpWrapper, rolledUpLines);
				return rollUpWrapper;
			}

			public DocARInvoiceLineCollection TestRollUpGrouperByChargeAndTaxForRollupForTest()
			{
				var grouper = new RollUpGrouperByChargeAndTax(DocLineRollUpper, this, Factory, InvoiceLine);
				return grouper.RollUp();
			}

			protected override DocARInvoiceLineCollection GetInvoiceLines()
			{
				return new DocARInvoiceLineCollection(Factory);
			}
		}

		#endregion

		#region FPOS

		public void TestARInvoiceHeaderFPOS()
		{
			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			var arDoc = DocARInvoice.New(arInvoice, Factory);
			AssertPlaceOfSupplyProperties(arInvoice, arDoc);
		}

		public void TestAPInvoiceHeaderFPOS()
		{
			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			var apDoc = DocAPInvoice.New(apInvoice, Factory);
			AssertPlaceOfSupplyProperties(apInvoice, apDoc);
		}

		void AssertPlaceOfSupplyProperties(TransactionHeader transaction, DocARInvoiceCommon docWrapper)
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var mainAddress = org.MainAddress;
				mainAddress.FillWithValidTestData();
				mainAddress.OA_RL_NKRelatedPortCode = "INADE";
				mainAddress.OA_State = "AP";
				transaction.AH_OH = org.PK;

				AssertEquals(GlbCompany.CurrentCompany.Country.Code, mainAddress.Country.Code);
				AssertEquals("State of Supply", docWrapper.RecipientLocalStateHeading);
				AssertEquals("Andhra Pradesh", docWrapper.RecipientLocalState); // Full name of the state.

				transaction.AH_PlaceOfSupplyType = String.Empty;
				AssertEquals(docWrapper.RecipientLocalStateHeading, docWrapper.FixedPlaceOfSupplyLabel);
				AssertEquals("Andhra Pradesh", docWrapper.FixedPlaceOfSupply);

				transaction.AH_PlaceOfSupply = "DL";
				AssertEquals("State of Supply", docWrapper.FixedPlaceOfSupplyLabel);
				AssertEquals("Delhi", docWrapper.FixedPlaceOfSupply);

				transaction.AH_PlaceOfSupply = PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry;
				AssertEquals("Place of Supply", docWrapper.FixedPlaceOfSupplyLabel);
				AssertEquals("Foreign Country/Region", docWrapper.FixedPlaceOfSupply);

				transaction.AH_PlaceOfSupply = PlaceOfSupplyListProvider.Codes.OtherTerritories;
				AssertEquals("Place of Supply", docWrapper.FixedPlaceOfSupplyLabel);
				AssertEquals("Other Territories", docWrapper.FixedPlaceOfSupply);
			}
		}

		public void TestRollUpLinesWithSamePlaceOfSupply()
		{
			AssertRollUpLinePlaceOfSupply(true);
		}

		public void TestRollUpLinesWithDifferentPlacesOfSupply()
		{
			AssertRollUpLinePlaceOfSupply(false);
		}

		void AssertRollUpLinePlaceOfSupply(bool isSame)
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_FullName = "Org for company";
				OrgAddress mainAddress = org.MainAddress;
				mainAddress.FillWithValidTestData();
				mainAddress.OA_RL_NKRelatedPortCode = "INADE";
				mainAddress.OA_State = "AP";

				var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
				var line = (InvoiceLine)arInvoice.Lines.AddNew();
				var line2 = (InvoiceLine)arInvoice.Lines.AddNew();
				line.TransactionHeader.AH_OH = org.PK;
				line.AL_PlaceOfSupplyType = PlaceOfSupplyTypes.State.Code;
				line.AL_PlaceOfSupply = "DL";
				line2.TransactionHeader.AH_OH = org.PK;
				line2.AL_PlaceOfSupplyType = PlaceOfSupplyTypes.State.Code;
				line2.AL_PlaceOfSupply = isSame ? "DL" : "JH";

				var amountWithExchangeRate = new DocARInvoiceLine.AmountWithExchangeRate();
				var docLine = DocARInvoiceLine.New(line, Factory);
				var docline2 = DocARInvoiceLine.New(line2, Factory);
				var collection = new DocARInvoiceLineCollection(Factory);
				collection.Add(docLine);
				collection.Add(docline2);

				var docInvoiceForRolledUp = new DocARBaseInvoiceForTest(arInvoice, Factory).CreateWrapperForRolledUpLineForGroupingForTest(new ZString("Test01"), 100, 10, 90, 9, amountWithExchangeRate, collection, null);

				AssertEquals("Place of Supply Name", isSame ? "Delhi" : "", docInvoiceForRolledUp.FixedPlaceOfSupply);
				AssertEquals("Place of Supply Label", isSame ? "State of Supply" : "", docInvoiceForRolledUp.FixedPlaceOfSupplyLabel);
			}
		}

		public void TestSpacerAndCommentLineControlsForFPOS()
		{
			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			var line = (InvoiceLine)arInvoice.Lines.AddNew();
			line.AL_PlaceOfSupplyType = PlaceOfSupplyTypes.State.Code;
			line.AL_PlaceOfSupply = "DL";

			var amountWithExchangeRate = new DocARInvoiceLine.AmountWithExchangeRate();
			var docLine = DocARInvoiceLine.New(line, Factory);
			var docLineCollection = new DocARInvoiceLineCollection(Factory);
			docLineCollection.Add(docLine);

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_ChargeType = "CMT"; //Charge type for setting line as a comment line
			var docChargeCode = DocChargeCode.New(chargeCode, Factory);

			var docInvoiceForRollUpWithCommentLine = new DocARBaseInvoiceForTest(arInvoice, Factory).CreateWrapperForRolledUpLineForGroupingForTest(new ZString("Test04"), 0, 0, 0, 0, amountWithExchangeRate, docLineCollection, docChargeCode);
			Assert(docInvoiceForRollUpWithCommentLine.IsCommentLine);
			AssertEquals("Place of Supply Value should not be set for Comment Line. Value should be empty.", "", docInvoiceForRollUpWithCommentLine.FixedPlaceOfSupply);
			AssertEquals("Place of Supply Label Value should not be set for Comment Line. Value should be empty.", "", docInvoiceForRollUpWithCommentLine.FixedPlaceOfSupplyLabel);

			docLineCollection = new DocARInvoiceLineCollection(Factory);
			docLineCollection.Add(docLine);

			var docInvoiceForRollUpWithSpacerLine = new DocARBaseInvoiceForTest(arInvoice, Factory).CreateWrapperForRolledUpLineForGroupingForTest(new ZString("Test05"), 0, 0, 0, 0, amountWithExchangeRate, docLineCollection, null, isSpacerLine: true);
			Assert(docInvoiceForRollUpWithSpacerLine.IsSpacerLine);
			AssertEquals("Place of Supply Value should not be set for Spacer Line. Value should be empty.", "", docInvoiceForRollUpWithSpacerLine.FixedPlaceOfSupply);
			AssertEquals("Place of Supply Label Value should not be set for Spacer Line. Value should be empty.", "", docInvoiceForRollUpWithSpacerLine.FixedPlaceOfSupplyLabel);
		}

		#endregion

		#region PrimaryRegistrationPremiseAddressTest

		void AssertRegistredPremiseAddress(
			string taxType,
			ZString taxNumber,
			bool expectEmptyPremiseAddress,
			ZGuid taxPK,
			ZString expectedPrimaryTaxCode,
			string companyCode = "ZDebtor",
			string fullName = "Test Company Name",
			string address1 = "random address",
			string city = "Mumbai",
			string state = "MH",
			string postCode = "220017",
			string closestPort = "INBOM",
			string countryCode = "IN"
		)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
				OrgHeader header = Factory.New<OrgHeader>();
				header.OH_Code = companyCode;
				header.OH_FullName = fullName;
				header.MainAddress.OA_Address1 = address1;
				header.MainAddress.OA_City = city;
				header.MainAddress.OA_State = state;
				header.MainAddress.OA_PostCode = postCode;
				header.MainAddress.OA_RN_NKCountryCode = countryCode;
				header.OH_RL_NKClosestPort = closestPort;
				header.OH_IsDebtor = true;
				header.OH_IsCreditor = true;

				header.CompanyData.SetARTaxApplicable(true);
				header.MiscServ.OM_ARWHTApplicable = true;

				header.CompanyData.SetAPTaxApplicable(true);
				header.MiscServ.OM_APWHTApplicable = true;

				if (taxPK != ZGuid.Empty)
				{
					OrgCusCode testCusCode1 = Factory.New<OrgCusCode>();
					testCusCode1.OK_CodeType = taxType;
					testCusCode1.OK_CustomsRegNo = taxNumber;
					if (expectEmptyPremiseAddress)
					{
						testCusCode1.OK_OA_PremisesAddress = header.MainAddress.EntityPK;
					}
					testCusCode1.OK_OH = header.PK;
				}
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("100012", TestObjectCreator.AUD, 1m, header);
				var line1 = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.CC2, TestObjectCreator.AUD, 1m, "Test Line1", 100m);
				var line2 = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.CC2, TestObjectCreator.AUD, 1m, "Test Line2", 200m);
				line1.AL_AT = taxPK;
				line2.AL_AT = taxPK;
				var docInvoice = DocARInvoice.New(invoice, Factory);
				AssertEquals(expectedPrimaryTaxCode, docInvoice.RecipientTaxIDNumber);
				if (expectEmptyPremiseAddress)
				{
					AssertEquals("Recipient main address should equal premiseAddress associated with tax id number",
						header.MainAddress.EntityPK,
						docInvoice.RecipientTaxIDPremisesAddress.OrgAddress.EntityPK);
				}
				else
				{
					AssertNull(docInvoice.RecipientTaxIDPremisesAddress);
				}
			}
		}

		[TestDate(2021, 06, 01)]
		public void TestRegisteredPremiseAddressMacroWhenPrimaryRegistrationWithPremiseAddressIndia()
		{
			AssertRegistredPremiseAddress(
				taxType: OrgCusCode.CodeTypes.GSTCode,
				taxNumber: "27AAACI168G2ZO",
				expectEmptyPremiseAddress: true,
				taxPK: TestObjectCreator.GST1.PK,
				expectedPrimaryTaxCode: "27AAACI168G2ZO");
		}

		[TestDate(2021, 06, 01)]
		public void TestRegisteredPremiseAddressMacroWhenPrimaryRegistrationWithoutPremiseAddressIndia()
		{
			AssertRegistredPremiseAddress(
				taxType: OrgCusCode.CodeTypes.GSTCode,
				taxNumber: "27AAACI168G2ZO",
				expectEmptyPremiseAddress: false,
				taxPK: TestObjectCreator.GST1.PK,
				expectedPrimaryTaxCode: "27AAACI168G2ZO"
			);
		}

		[TestDate(2021, 06, 01)]
		public void TestRegisteredPremiseAddressMacroWhenNoPrimaryRegistrationIndia()
		{
			AssertRegistredPremiseAddress(
				taxType: IndiaOrgCusCodeInfo.OrgCusCodes.SER,
				taxNumber: "sertax",
				expectEmptyPremiseAddress: false,
				taxPK: TestObjectCreator.ServiceTax.PK,
				expectedPrimaryTaxCode: ZString.Empty
			);
		}

		[TestDate(2021, 06, 01)]
		public void TestRegisteredPremiseAddressMacroWhenPrimaryRegistrationWithPremiseAddressAustrailia()
		{
			AssertRegistredPremiseAddress(
				taxType: OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber,
				taxNumber: "41065894724",
				expectEmptyPremiseAddress: true,
				taxPK: TestObjectCreator.GST1.PK,
				expectedPrimaryTaxCode: "41065894724",
				address1: "1 main street",
				city: "Sydney",
				state: "NSW",
				postCode: "2002",
				closestPort: "AUSYD",
				countryCode: "AU"
			);
		}

		[TestDate(2021, 06, 01)]
		public void TestRegisteredPremiseAddressMacroWhenPrimaryRegistrationWithoutPremiseAddressAustrailia()
		{
			AssertRegistredPremiseAddress(
				taxType: OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber,
				taxNumber: "41065894724",
				expectEmptyPremiseAddress: false,
				taxPK: TestObjectCreator.GST1.PK,
				expectedPrimaryTaxCode: "41065894724",
				address1: "1 main street",
				city: "Sydney",
				state: "NSW",
				postCode: "2002",
				closestPort: "AUSYD",
				countryCode: "AU"
			);
		}

		[TestDate(2021, 06, 01)]
		public void TestRegisteredPremiseAddressMacroWhenNoPrimaryRegistrationAustrailia()
		{
			AssertRegistredPremiseAddress(
				taxType: OrgCusCode.AustraliaCodeTypes.ARN,
				taxNumber: ZString.Empty,
				expectEmptyPremiseAddress: false,
				taxPK: ZGuid.Empty,
				expectedPrimaryTaxCode: ZString.Empty,
				address1: "1 main street",
				city: "Sydney",
				state: "NSW",
				postCode: "2002",
				closestPort: "AUSYD",
				countryCode: "AU"
			)
			;
		}

		#endregion

	}
}
