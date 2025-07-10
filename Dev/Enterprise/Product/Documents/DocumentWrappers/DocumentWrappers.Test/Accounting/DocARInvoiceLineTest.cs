using System;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineIntegration.RollUpSort;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocARInvoiceLine))]
	class DocARInvoiceLineTest : AccountingDocumentWrapperTestCase
	{
		#region GetDocumentWrappers

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocARInvoiceLine.New(Line, Factory) };
		}

		#endregion

		#region TestLineDescription

		public void TestLineDescription_LineDescriptionIsSameAsChargeCodeDescription()
		{
			var defaultChargeCodeDescription = "My Test Charge Code";
			AssertLineDescriptionForChargeCode(false, defaultChargeCodeDescription);
			AssertLineDescriptionForChargeCode(true, defaultChargeCodeDescription);
		}

		public void TestLineDescription_LineDescriptionIsAppended_ChargeCode()
		{
			var appendedLineDescription = "My Test Charge Code and some appended text";
			AssertLineDescriptionForChargeCode(false, appendedLineDescription);
			AssertLineDescriptionForChargeCode(true, appendedLineDescription);
		}

		public void TestLineDescription_LineDescriptionIsOverriden_ChargeCode()
		{
			var overriddenLineDescription = "This is a very different charge code description";
			AssertLineDescriptionForChargeCode(false, overriddenLineDescription);
			AssertLineDescriptionForChargeCode(true, overriddenLineDescription);
		}

		void AssertLineDescriptionForChargeCode(bool enableLocalChargeCodeDescriptionDefaultRegistryRegistryOn, string lineDescription)
		{
			var defaultChargeCodeDescription = "My Test Charge Code";
			var chargeCodeDescriptionInGerman = "Mein Testgebührencode";
			var chargeCode = TestObjectCreator.CreateChargeCode("ABC");
			chargeCode.AC_Desc = defaultChargeCodeDescription;
			Line.AL_AC = chargeCode.PK;
			Line.AL_Desc = lineDescription;

			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
			var expectedLineDescription = lineDescription;
			if (!enableLocalChargeCodeDescriptionDefaultRegistryRegistryOn && lineDescription.StartsWith(defaultChargeCodeDescription))
			{
				expectedLineDescription = chargeCodeDescriptionInGerman;
				if (lineDescription != defaultChargeCodeDescription)
				{
					expectedLineDescription += lineDescription.Substring(defaultChargeCodeDescription.Length);
				}
			}

			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableLocalChargeCodeDescriptionDefaultRegistryRegistryOn))
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
			using (var mockRes = Res.UseMockData())
			{
				var resKey = chargeCode.AC_DescInfo.CustomizableDataResourceStrings.GetMultilingualString(chargeCode, defaultChargeCodeDescription).ResourceKey;
				mockRes.Put(resKey, new ResourceStringData(resKey, chargeCodeDescriptionInGerman));
				AssertEquals(expectedLineDescription, InvoiceLineWrapper.LineDescription);
			}
		}

		public void TestLineDescription_LineDescriptionIsSameAsGLAccountDescription()
		{
			AssertLineDescriptionForGLAccount("My Test GL Account");
		}

		public void TestLineDescription_LineDescriptionIsOverriden_GLAccount()
		{
			AssertLineDescriptionForGLAccount("This is a very different GL account description");
		}

		void AssertLineDescriptionForGLAccount(string lineDescription)
		{
			var defaultGLAccountDescription = "My Test GL Account";
			var glAccountDescriptionInGerman = "Mein Test-Hauptbuchkonto";
			var glAccount = TestObjectCreator.CreateGLHeader("TestGLAcc");
			glAccount.AG_Description = defaultGLAccountDescription;
			Line.AL_AG = glAccount.PK;
			Line.AL_Desc = lineDescription;

			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
			var expectedLineDescription = lineDescription == defaultGLAccountDescription ? glAccountDescriptionInGerman : lineDescription;

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
			using (var mockRes = Res.UseMockData())
			{
				var resKey = glAccount.AG_DescriptionInfo.CustomizableDataResourceStrings.GetMultilingualString(glAccount, defaultGLAccountDescription).ResourceKey;
				mockRes.Put(resKey, new ResourceStringData(resKey, glAccountDescriptionInGerman));
				AssertEquals(expectedLineDescription, InvoiceLineWrapper.LineDescription);
			}
		}

		#endregion

		#region TestIsCommentLine

		public void TestIsCommentLine()
		{
			foreach (var data in new[] { new { RegValue = false, ExpectedQuantity = "" }, new { RegValue = true, ExpectedQuantity = "1" } })
			{
				using (AccountingConfigurationRegistry.Instance.PrintQuanityInInvoiceDocument.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, data.RegValue))
				{
					AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
					chargeCode.AC_ChargeType = "CMT";
					Line.AL_AC = chargeCode.PK;
					InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
					AssertEquals(true, InvoiceLineWrapper.IsCommentLine);
					AssertEquals(string.Empty, InvoiceLineWrapper.Quantity);

					chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
					chargeCode.AC_ChargeType = "FRT";
					Line.AL_AC = chargeCode.PK;
					Line.AL_Desc = "Test Charge";
					InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
					AssertEquals(false, InvoiceLineWrapper.IsCommentLine);
					AssertEquals(data.ExpectedQuantity, InvoiceLineWrapper.Quantity);

					Line.AL_AC = ZGuid.Empty;
					Line.AL_Desc = "Test Charge";
					InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
					AssertEquals(false, InvoiceLineWrapper.IsCommentLine);
					AssertEquals(data.ExpectedQuantity, InvoiceLineWrapper.Quantity);
				}
			}
		}

		#endregion

		#region TestContainerNumbers

		public void TestContainerNumbers()
		{
			Factory.Save();
			JobHeader jobHeader = Factory.NewJobForTesting<JobHeader>();
			Line.AL_JH = jobHeader.PK;

			CFSLoadListConsol loadList = Factory.New<CFSLoadListConsol>();
			jobHeader.JH_ParentID = loadList.PK;
			jobHeader.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			CFSContainer cfsContainer1 = loadList.Containers.AddNew();
			cfsContainer1.JC_ContainerNum = "CFS1111";
			cfsContainer1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			CFSContainer cfsContainer2 = loadList.Containers.AddNew();
			cfsContainer2.JC_ContainerNum = "CFS2222";
			cfsContainer2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
			AssertEquals("CFS1111:20GP CFS2222:40GP ", InvoiceLineWrapper.ContainerNumbers);

			CommonCartage cartage = Factory.New<CommonCartage>();
			var containerBookedMovesComparer = new ContainerBookedMovesComparer();
			cartage.ContainerBookedMoves.ApplySort(containerBookedMovesComparer);
			jobHeader.JH_ParentID = cartage.PK;
			jobHeader.JH_ParentTableCode = JobCartageSchema.Constants.Prefix;
			jobHeader.JH_JobNum = "TestJob";
			CommonContainer cartageContainer1 = cartage.ContainerBookedMoves.AddNew().Container;
			cartageContainer1.JC_ContainerNum = "CART1111";
			cartageContainer1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40OT").PK;
			CommonContainer cartageContainer2 = cartage.ContainerBookedMoves.AddNew().Container;
			cartageContainer2.JC_ContainerNum = "CART2222";
			cartageContainer2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
			Factory.Save();
			AssertEquals("CART1111:40OT CART2222:40RE ", InvoiceLineWrapper.ContainerNumbers);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			jobHeader.JH_ParentID = declaration.PK;
			jobHeader.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			declaration.CusContainers.AddNew();
			declaration.CusContainers[0].CO_ContainerNumber = "CONT1";
			declaration.CusContainers[0].CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			declaration.CusContainers.AddNew();
			declaration.CusContainers[1].CO_ContainerNumber = "CONT2";
			declaration.CusContainers[1].CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
			AssertEquals("CONT1, CONT2", InvoiceLineWrapper.ContainerNumbers);
		}

		#endregion

		#region TestChargeCodeIsCached

		public void TestChargeCodeIsCached()
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "USMIZ";
			JobHeader jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentTableCode = "JS";
			jobHeader.JH_ParentID = shipment.PK;
			Factory.Save();
			Invoice.AH_ExchangeRate = 0.717897m;
			Invoice.AH_TransactionCategory = "DCD";
			Invoice.AH_OH = orgHeader.PK;
			Line.AL_ExchangeRate = 0.707897m;
			Line.AL_AC = chargeCode.PK;
			Line.AL_OH = orgHeader.PK;
			Line.AL_JH = jobHeader.PK;
			Line.AL_Desc = "Test Description";
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_RX_NKSellCurrency = "UAH";
			charge.JR_AL_ARLine = Line.PK;
			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
			DocChargeCode chargeCode1 = InvoiceLineWrapper.ChargeCode;
			AssertEquals("Charge code should be cached", chargeCode1, InvoiceLineWrapper.ChargeCode);
		}

		#endregion

		#region TestInvoiceLineDescriptionForPeriodicInvoice

		public void TestInvoiceLineDescriptionForPeriodicInvoice()
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			if (orgHeader.CompanyData != null)
			{
				// This to create CompanyData, test needs it
			}

			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "USMIZ";
			JobHeader jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentTableCode = "JS";
			jobHeader.JH_ParentID = shipment.PK;
			Factory.Save();
			Invoice.AH_ExchangeRate = 0.717897m;
			Invoice.AH_TransactionCategory = "DCD";
			Invoice.AH_OH = orgHeader.PK;
			Line.AL_ExchangeRate = 0.707897m;
			Line.AL_AC = chargeCode.PK;
			Line.AL_OH = orgHeader.PK;
			Line.AL_JH = jobHeader.PK;
			Line.AL_Desc = "Test Description";
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_RX_NKSellCurrency = "UAH";
			charge.JR_OSSellExRate = 1m;
			charge.JR_AL_ARLine = Line.PK;
			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
			AssertEquals("Test Description", InvoiceLineWrapper.InvoiceLineDescriptionForPeriodicInvoice);

			orgHeader.CompanyData.InvoiceRollupOrGroups[0].PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.AllExRate;
			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
			AssertEquals("Test Description UAH 0.00 @ 1", InvoiceLineWrapper.InvoiceLineDescriptionForPeriodicInvoice);
		}

		#endregion

		#region TestInvoice

		public void TestInvoice()
		{
			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
			int hashCode = InvoiceLineWrapper.Invoice.GetHashCode();
			AssertEquals("Hash codes must be equal", hashCode, InvoiceLineWrapper.Invoice.GetHashCode());
		}

		#endregion

		#region TestAmountWithExchangeRate

		public void TestAmountWithExchangeRate()
		{
			DocARInvoiceLine.AmountWithExchangeRate amountWithExchangeRate = new DocARInvoiceLine.AmountWithExchangeRate();

			amountWithExchangeRate.Amount = 1000.847m;
			amountWithExchangeRate.ExchangeRate = 1m;

			AssertEquals("Should give empty string if no currency defined.", "", amountWithExchangeRate.ToString());

			RefCurrency uSD = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			DocCurrency docUSD = DocCurrency.New(uSD, Factory);

			amountWithExchangeRate.Currency = docUSD;
			amountWithExchangeRate.ExchangeRate = 0.1234567891m;
			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("Should give amount to 2 decimal places and exchange rate to 6 decimals.", "USD 1,000.85 @ 0.123457", amountWithExchangeRate.ToString());
			}

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("Should give amount to 2 decimal places and exchange rate to 6 decimals.", "USD 1.000,85 @ 0.123457", amountWithExchangeRate.ToString());
			}
		}

		#endregion

		#region TestGenericChargeCodeDescriptionAndType

		public void TestGenericChargeCodeDescriptionAndType()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "ABC";
			chargeCode.AC_Desc = "TestCharge Code";
			chargeCode.AC_ChargeType = "MRG";

			AccGLHeader glAccount = Factory.NewWithValidTestData<AccGLHeader>();
			glAccount.AG_AccountNum = "1234.99.99";
			glAccount.AG_Description = "Test GL Account";
			glAccount.AG_AccountType = "BSH";
			Factory.Save();

			ARInvoiceLine line = Factory.NewWithValidTestData<ARInvoiceLine>();
			line.AL_AC = chargeCode.PK;
			DocARInvoiceLine wrapper = DocARInvoiceLine.New(line, Factory);
			AssertEquals("ABC", wrapper.GenericChargeCode);
			AssertEquals("TestCharge Code", wrapper.GenericChargeDescription);
			AssertEquals("MRG", wrapper.GenericChargeType);

			line = Factory.NewWithValidTestData<ARInvoiceLine>();
			line.AL_AG = glAccount.PK;
			wrapper = DocARInvoiceLine.New(line, Factory);
			AssertEquals("1234.99.99", wrapper.GenericChargeCode);
			AssertEquals("Test GL Account", wrapper.GenericChargeDescription);
			AssertEquals("BSH", wrapper.GenericChargeType);
		}

		#endregion

		#region TestCharge

		public void TestCharge()
		{
			var line = Factory.New<ARInvoiceLine>();
			DocARInvoiceLine wrapper = DocARInvoiceLine.New(Line, Factory);
			AssertNull("Charge", wrapper.Charge);

			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = Line.PK;
			AssertNotNull("Charge", wrapper.Charge);
			AssertEquals("Charge", charge.PK, ((Charge)wrapper.Charge.WrappedObject).PK);
		}

		#endregion

		#region TestChargeOSAmountAndCurrency

		public void TestChargeOSAmountAndCurrency()
		{
			var line = Factory.New<ARInvoiceLine>();

			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = line.PK;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OSSellAmt = 1000m;
			charge.JR_OSSellExRate = 0.707m;
			var wrapper = DocARInvoiceLine.New(line, Factory);
			AssertEquals("ChargeOSAmountAndCurrency", "1,000.00 USD", wrapper.ChargeOSAmountAndCurrency);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Iceland))
			{
				AssertEquals("Formatting for Iceland", "1.000,00 USD", wrapper.ChargeOSAmountAndCurrency);
			}
		}

		#endregion

		#region TestChargeExchangeRate

		public void TestChargeExchangeRate()
		{
			var line = Factory.New<ARInvoiceLine>();

			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = line.PK;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OSSellAmt = 1000m;
			charge.JR_OSSellExRate = 0.7089m;

			var wrapper = DocARInvoiceLine.New(line, Factory);
			AssertEquals("ChargeExchangeRate", 0.7089m, wrapper.ChargeExchangeRate);
		}

		#endregion

		#region TestExchangeRateAndAmount

		public void TestExchangeRateAndAmount()
		{
			var audCurr = TestObjectCreator.AUD;
			var usdCurr = TestObjectCreator.USD;

			var orgFactory = new BusinessObjectFactory();
			var client = orgFactory.NewWithValidTestData<OrgHeader>();
			client.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			var group = client.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.None;
			group.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			group.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			orgFactory.Save();

			var invoice = Factory.New<ARInvoice>();
			var line = Factory.New<ARInvoiceLine>();
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge = Factory.New<Charge>();

			line.AL_AH = invoice.PK;
			charge.JR_AL_ARLine = line.PK;

			var receivableCharge = charge as IReceivablesPostingCharge;
			AssertNotNull("IReceivablesPostingCharge", receivableCharge);

			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = audCurr.RX_Code;
			invoice.AH_RX_NKTransactionCurrency = audCurr.RX_Code;
			Line.AL_RX_NKTransactionCurrency = audCurr.RX_Code;
			charge.JR_RX_NKSellCurrency = audCurr.RX_Code;

			charge.JR_OSSellAmt = 1000M;
			AssertEquals("JR_LocalSellAmt", 1000m, charge.JR_LocalSellAmt);
			var wrapper = DocARInvoiceLine.New(line, Factory);
			AssertEquals("", wrapper.ExchangeRateAndAmount);

			job.LocalChargesPK = client.PK;
			invoice.AH_JH = job.PK;
			invoice.AH_OH = client.PK;
			charge.JR_JH = job.PK;
			wrapper = DocARInvoiceLine.New(line, Factory);
			AssertEquals("", wrapper.ExchangeRateAndAmount);

			group.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.AllExRate;
			orgFactory.Save();

			wrapper = DocARInvoiceLine.New(line, Factory);
			AssertEquals("", wrapper.ExchangeRateAndAmount);

			charge.JR_RX_NKSellCurrency = usdCurr.RX_Code;
			charge.JR_OSSellAmt = 1000M;
			charge.JR_OSSellExRate = 0.78M;
			orgFactory.Save();

			AssertEquals("JR_LocalSellAmt", 1282.05m, charge.JR_LocalSellAmt);
			AssertEquals("OSSellAmount", 1000m, receivableCharge.OSSellAmount);
			wrapper = DocARInvoiceLine.New(line, Factory);
			AssertEquals("USD 1,000.00 @ 0.78", wrapper.ExchangeRateAndAmount);
		}

		public void TestExchangeRateAndAmountWithNoCharge()
		{
			var audCurr = TestObjectCreator.AUD;
			var usdCurr = TestObjectCreator.USD;

			var orgFactory = new BusinessObjectFactory();
			var client = orgFactory.NewWithValidTestData<OrgHeader>();
			client.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			var group = client.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.None;
			group.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			group.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			orgFactory.Save();

			var invoice = Factory.New<ARInvoice>();
			var line = Factory.New<ARInvoiceLine>();
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge = Factory.New<Charge>();

			line.AL_AH = invoice.PK;
			charge.JR_AL_ARLine = line.PK;

			var receivableCharge = charge as IReceivablesPostingCharge;

			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = audCurr.RX_Code;
			invoice.AH_RX_NKTransactionCurrency = audCurr.RX_Code;
			Line.AL_RX_NKTransactionCurrency = audCurr.RX_Code;
			job.LocalChargesPK = client.PK;
			invoice.AH_JH = job.PK;
			invoice.AH_OH = client.PK;
			charge.JR_JH = job.PK;
			group.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.AllExRate;
			orgFactory.Save();

			charge.JR_RX_NKSellCurrency = usdCurr.RX_Code;
			charge.JR_OSSellAmt = 1000M;
			charge.JR_OSSellExRate = 0.78M;
			orgFactory.Save();

			charge.Delete();
			orgFactory.Save();
			var wrapper = DocARInvoiceLine.New(line, Factory);
			AssertNoExceptionThrown(() => wrapper.ExchangeRateAndAmount.ToString());
		}

		#endregion

		[TestDate(2023, 08, 28)]
		public void TestTaxDate()
		{
			var taxDate = ZDate.Today.AddDays(-2);
			var goodsChargeCode = TestObjectCreator.CreateChargeCode("GOODS");
			goodsChargeCode.AC_GovtChargeCode = "TAX1";
			goodsChargeCode.AC_GoodsServiceType = GoodServiceTypes.Codes.GDS;

			var shipment = TestObjectCreator.CreateShipment("S00001");
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var lineGST = CreateInvoice(TestObjectCreator.GST1, taxDate);
			var lineNOT = CreateInvoice(TestObjectCreator.ExtraServiceTax, taxDate);
			var lineEXL = CreateInvoice(TestObjectCreator.ExcludedTax, taxDate);
			var lineWithoutTaxRate = CreateInvoice(null, ZDate.Empty);
			var lineWithInvalidTaxDate = CreateInvoice(TestObjectCreator.GST1, ZDate.Invalid);

			AssertLineTaxDate("expect empty if tax rate is NOT rate type", lineNOT, ZDate.Empty);
			AssertLineTaxDate("expect empty if tax rate is EXL rate type", lineEXL, ZDate.Empty);
			AssertLineTaxDate("expect the line tax date if rate type is different from NOT and EXL", lineGST, taxDate);
			AssertLineTaxDate("expect empty date if there is no tax rate", lineWithoutTaxRate, ZDate.Empty);
			AssertLineTaxDate("expect empty date if tax date is invalid", lineWithInvalidTaxDate, ZDate.Empty);

			InvoicingLineBase CreateInvoice(AccTaxRate taxRate, ZDate taxRateDate)
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV0001", TestObjectCreator.AUD, 1.0M, TestObjectCreator.Debtor);
				var line = TestObjectCreator.CreateARInvoiceLine(invoice, job, goodsChargeCode, TestObjectCreator.AUD, 1.0M, "GOODS Desc", 200M);
				line.AL_GovtChargeCode = "TAX1";

				line.AL_AT = taxRate?.PK ?? ZGuid.Empty;
				line.AL_TaxDate = taxRateDate;
				TestObjectCreator.CreateJobCharge(line, job, goodsChargeCode);
				return line;
			}

			void AssertLineTaxDate(string message, InvoicingLineBase line, ZDateTime expectedTaxDate)
			{
				var wrapper = DocARInvoiceLine.New(line, Factory);
				AssertEquals(message, expectedTaxDate, wrapper.TaxDate);
			}
		}

		public void TestLayoutWhenPrintedInPeriodicInvoiceWithInvalidJobParent()
		{
			var dummyConcreateJob = new MockJobHeaderParent(Factory.NewWithValidTestData<ForwardingShipment>());

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = TestObjectCreator.Debtor.PK;

			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			var job1 = new Job.Loader(Factory, dummyConcreateJob).TryCreate();
			line.AL_JH = job1.PK;

			var wrapper = DocARInvoiceLine.New(line, Factory);
			dummyConcreateJob.IsNullInvoicingSupporter = true;

			AssertExceptionMessage(job1.JH_JobNum);

			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.PlugInData = null;
			line.AL_JH = job2.PK;

			wrapper = DocARInvoiceLine.New(line, Factory);
			AssertNull("The job parent is not available", job2.Parent);

			AssertExceptionMessage(job2.JH_JobNum);

			void AssertExceptionMessage(string jobNumber)
			{
				string tmp = null;
				var expectedMessage = string.Format(CultureInfo.InvariantCulture, @"Invoice number {0} cannot be printed because a parent is not available for the {1} job.
You could try to retrieve an original printed invoice document from the invoice’s eDocs tab.
Use the 'Missing/Invalid Job Parent' filter in the Job Management module to list all jobs without a valid parent.", invoice.InvoiceNumber, jobNumber);

				var ex = AssertExceptionThrown<DataProviderException>(() => tmp = wrapper.LayoutWhenPrintedInPeriodicInvoice);
				AssertContains(expectedMessage, ex.Message);
				AssertContains("DocumentWrappers.DocARInvoiceLine.get_InvoiceType()", ex.StackTrace);
				AssertContains("DocARInvoiceLine.get_LayoutWhenPrintedInPeriodicInvoice()", ex.StackTrace);
			}
		}

		[ExpectNoExceptions]
		public void TestLayoutWhenPrintedInPeriodicInvoiceWithNullPluginData()
		{
			var dummyConcreateJob = new MockJobHeaderParent(Factory.NewWithValidTestData<ForwardingShipment>());

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = TestObjectCreator.Debtor.PK;

			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			var jobHeader = new Job.Loader(Factory, dummyConcreateJob).TryCreate();
			jobHeader.PlugInData = null;
			line.AL_JH = jobHeader.PK;

			var wrapper = DocARInvoiceLine.New(line, Factory);
			dummyConcreateJob.IsNullInvoicingSupporter = true;
			AssertEquals(InvoiceTypeLayoutList.Codes.CHG, wrapper.LayoutWhenPrintedInPeriodicInvoice);
		}

		class MockJobHeaderParent : IJobHeaderParent, IJobInvoicingPlugIn
		{
			public MockJobHeaderParent(ForwardingShipment shipment)
			{
				this.shipment = shipment;
			}

			readonly ForwardingShipment shipment;
			public bool IsNullInvoicingSupporter { get; set; }

			string IJobNumber.JobNumber => shipment.JobNumber;
			BusinessObjectFactory IJobHeaderParentCore.Factory => shipment.Factory;
			ZGuid IJobHeaderParentCore.PK => shipment.PK;
			string IJobHeaderParentCore.TableName => shipment.TableName;
			bool IJobHeaderParentCore.IsInDatabase => shipment.IsInDatabase;

			void IJobHeaderParent.SetJobNumberFieldOnSaving()
			{
			}

			void IJobHeaderParent.OnJobCreating(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobCreated(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobDeleting(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobDeleted(JobHeader job)
			{
			}

			bool IJobHeaderParent.AllowInvoiceDeletion
			{
				get { return true; }
			}

			bool IJobHeaderParent.IsDeleted => shipment.IsDeleted;

			public IJobInvoicingSupporter InvoicingSupporter => IsNullInvoicingSupporter ? null : shipment.InvoicingSupporter;
		}

		#region TestCrossExchangeRatesBasedOnJobChargeSellCurrency

		public void TestCrossExchangeRatesBasedOnJobChargeSellCurrency()
		{
			var gbpCurr = TestObjectCreator.GBP;
			var usdCurr = TestObjectCreator.USD;
			var eurCurr = TestObjectCreator.EUR;

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			GlbCompany.CurrentCompany.Factory.Save();
			var invoice = Factory.New<ARInvoice>();

			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			var group = client.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.None;
			group.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			group.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			Factory.Save();

			var line1 = (InvoicingLineBase)invoice.Lines.AddNew();
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge1 = Factory.New<Charge>();

			var line2 = (InvoicingLineBase)invoice.Lines.AddNew();
			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge2 = Factory.New<Charge>();

			var line3 = (InvoicingLineBase)invoice.Lines.AddNew();
			var job3 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge3 = Factory.New<Charge>();

			var line4 = (InvoicingLineBase)invoice.Lines.AddNew();
			var job4 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge4 = Factory.New<Charge>();

			charge1.JR_AL_ARLine = line1.PK;
			line1.AL_JH = job1.PK;
			charge1.JR_JH = job1.PK;

			charge2.JR_AL_ARLine = line2.PK;
			line2.AL_JH = job2.PK;
			charge2.JR_JH = job2.PK;

			charge3.JR_AL_ARLine = line3.PK;
			line3.AL_JH = job3.PK;
			charge3.JR_JH = job3.PK;

			charge4.JR_AL_ARLine = line4.PK;
			line4.AL_JH = job4.PK;
			charge4.JR_JH = job4.PK;

			var receivableCharge1 = charge1 as IReceivablesPostingCharge;
			var receivableCharge2 = charge2 as IReceivablesPostingCharge;
			var receivableCharge3 = charge3 as IReceivablesPostingCharge;
			var receivableCharge4 = charge4 as IReceivablesPostingCharge;

			charge1.JR_RX_NKSellCurrency = usdCurr.RX_Code;
			charge1.JR_OSSellAmt = 100M;
			charge1.JR_OSSellExRate = 0.5M;
			AssertEquals("JR_LocalSellAmt", 50m, charge1.JR_LocalSellAmt);

			charge2.JR_RX_NKSellCurrency = usdCurr.RX_Code;
			charge2.JR_OSSellAmt = 151M;
			charge2.JR_OSSellExRate = 0.5M;
			AssertEquals("JR_LocalSellAmt", 75.50m, charge2.JR_LocalSellAmt);

			charge3.JR_RX_NKSellCurrency = gbpCurr.RX_Code;
			charge3.JR_OSSellAmt = 555M;
			charge3.JR_OSSellExRate = 4.7874M;
			AssertEquals("JR_LocalSellAmt", 2657.01m, charge3.JR_LocalSellAmt);

			charge4.JR_RX_NKSellCurrency = gbpCurr.RX_Code;
			charge4.JR_OSSellAmt = 666M;
			charge4.JR_OSSellExRate = 4.7874M;
			AssertEquals("JR_LocalSellAmt", 3188.41m, charge4.JR_LocalSellAmt);

			var eurRate1 = ((IExchangeRateProvider)job1).GetExchangeRate(eurCurr.RX_Code, charge1.JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR, true);
			eurRate1.SetBuyRate_ForTestOnly(0.6m);
			var eurRate2 = ((IExchangeRateProvider)job2).GetExchangeRate(eurCurr.RX_Code, charge2.JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR, true);
			eurRate2.SetBuyRate_ForTestOnly(0.6m);
			var eurRate3 = ((IExchangeRateProvider)job3).GetExchangeRate(eurCurr.RX_Code, charge3.JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR, true);
			eurRate3.SetBuyRate_ForTestOnly(3.685m);
			var eurRate4 = ((IExchangeRateProvider)job4).GetExchangeRate(eurCurr.RX_Code, charge4.JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR, true);
			eurRate4.SetBuyRate_ForTestOnly(3.685m);

			var usdRate1 = ((IExchangeRateProvider)job1).GetExchangeRate(usdCurr.RX_Code, charge1.JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR, true);
			usdRate1.SetBuyRate_ForTestOnly(0.5m);
			var usdRate2 = ((IExchangeRateProvider)job2).GetExchangeRate(usdCurr.RX_Code, charge2.JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR, true);
			usdRate2.SetBuyRate_ForTestOnly(0.5m);
			var gbpRate1 = ((IExchangeRateProvider)job3).GetExchangeRate(gbpCurr.RX_Code, charge3.JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR, true);
			gbpRate1.SetBuyRate_ForTestOnly(4.7874m);
			var gbpRate2 = ((IExchangeRateProvider)job4).GetExchangeRate(gbpCurr.RX_Code, charge4.JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR, true);
			gbpRate2.SetBuyRate_ForTestOnly(4.7874m);

			charge1.ClearRevenueLinkOnlyTemporary();
			charge1.JR_RX_NKSellInvoiceCurrency = eurCurr.RX_Code;
			charge2.ClearRevenueLinkOnlyTemporary();
			charge2.JR_RX_NKSellInvoiceCurrency = eurCurr.RX_Code;
			charge3.ClearRevenueLinkOnlyTemporary();
			charge3.JR_RX_NKSellInvoiceCurrency = eurCurr.RX_Code;
			charge4.ClearRevenueLinkOnlyTemporary();
			charge4.JR_RX_NKSellInvoiceCurrency = eurCurr.RX_Code;

			group.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.AllExRate;

			AssertEquals("Sell Inv Amt", 100m, charge1.JR_OSSellAmt);
			AssertEquals("OSSellAmount", 83.33m, receivableCharge1.OSSellAmount);
			AssertEquals("Sell Inv Amt", 151m, charge2.JR_OSSellAmt);
			AssertEquals("OSSellAmount", 125.83m, receivableCharge2.OSSellAmount);
			AssertEquals("Sell Inv Amt", 555m, charge3.JR_OSSellAmt);
			AssertEquals("OSSellAmount", 721.03m, receivableCharge3.OSSellAmount);
			AssertEquals("Sell Inv Amt", 666m, charge4.JR_OSSellAmt);
			AssertEquals("OSSellAmount", 865.24m, receivableCharge4.OSSellAmount);

			charge1.JR_AL_ARLine = line1.PK;
			charge2.JR_AL_ARLine = line2.PK;
			charge3.JR_AL_ARLine = line3.PK;
			charge4.JR_AL_ARLine = line4.PK;

			var wrapper1 = DocARInvoiceLine.New(line1, Factory);
			var wrapper2 = DocARInvoiceLine.New(line2, Factory);
			var wrapper3 = DocARInvoiceLine.New(line3, Factory);
			var wrapper4 = DocARInvoiceLine.New(line4, Factory);

			invoice.AH_JH = job1.PK;
			invoice.AH_OH = client.PK;

			AssertEquals("Shows cross ex rate from USD to EUR", "USD 100.00 @ 0.833307", wrapper1.ExchangeRateAndAmount);
			AssertEquals("Shows cross ex rate from USD to EUR", "USD 151.00 @ 0.833307", wrapper2.ExchangeRateAndAmount);
			AssertEquals("Shows cross ex rate from GBP to EUR", "GBP 555.00 @ 1.299156", wrapper3.ExchangeRateAndAmount);
			AssertEquals("Shows cross ex rate from GBP to EUR", "GBP 666.00 @ 1.299156", wrapper4.ExchangeRateAndAmount);

			AssertEquals("Shows cross ex rate from USD to EUR", 0.833307m, wrapper1.ChargeExchangeRate);
			AssertEquals("Shows cross ex rate from USD to EUR", 0.833307m, wrapper2.ChargeExchangeRate);
			AssertEquals("Shows cross ex rate from GBP to EUR", 1.299156m, wrapper3.ChargeExchangeRate);
			AssertEquals("Shows cross ex rate from GBP to EUR", 1.299156m, wrapper4.ChargeExchangeRate);
		}

		#endregion

		#region TestChargeCurrency

		public void TestChargeCurrency()
		{
			var line = Factory.New<ARInvoiceLine>();

			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = line.PK;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OSSellAmt = 1000m;
			charge.JR_OSSellExRate = 0.7089m;

			var wrapper = DocARInvoiceLine.New(line, Factory);
			AssertEquals("ChargeCurrency", "USD", wrapper.ChargeCurrency);
		}

		#endregion

		#region TestSellRecognition

		public void TestSellRecognition()
		{
			var charge = Factory.New<Charge>();
			var line = Factory.New<ARInvoiceLine>();
			line.AL_RevRecognitionType = "IMM";
			charge.JR_AL_ARLine = line.PK;

			var wrapper = DocARInvoiceLine.New(line, Factory);
			AssertEquals("IMM", wrapper.SellRecognition);
		}

		#endregion

		#region TestCostRecognition

		public void TestCostRecognition()
		{
			var charge = Factory.New<Charge>();
			var transactionLine = Factory.New<ARInvoiceLine>();
			charge.JR_AL_APLine = transactionLine.PK;
			transactionLine.AL_RevRecognitionType = "IMM";
			var line = Factory.New<ARInvoiceLine>();
			charge.JR_AL_ARLine = line.PK;

			var wrapper = DocARInvoiceLine.New(line, Factory);
			AssertEquals("IMM", wrapper.CostRecognition);
		}

		#endregion

		#region TestProductName

		public void TestProductName()
		{
			var charge = Factory.New<Charge>();
			var attrib = charge.JobChargeAttributes.AddNew();
			attrib.EC_Name = JobChargeAttribTypeList.Codes.Product;
			attrib.EC_Value = "PRODUCT";

			var line = Factory.New<ARInvoiceLine>();
			charge.JR_AL_ARLine = line.PK;

			var wrapper = DocARInvoiceLine.New(line, Factory);
			AssertEquals("PRODUCT", wrapper.ProductName);
		}
		#endregion

		#region TestIsApproved

		public void TestIsApproved()
		{
			var charge = Factory.New<Charge>();
			charge.JR_AL_APLine = ZGuid.Empty;
			AssertEquals(false, charge.IsApproved);

			var apLine = Factory.New<ARInvoiceLine>();
			charge.JR_AL_APLine = apLine.PK;
			apLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			var arLine = Factory.New<ARInvoiceLine>();
			charge.JR_AL_ARLine = arLine.PK;

			var wrapper = DocARInvoiceLine.New(arLine, Factory);

			AssertEquals(false, wrapper.IsApproved);

			apLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;
			AssertEquals(false, wrapper.IsApproved);

			apLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			AssertEquals(false, wrapper.IsApproved);

			apLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			AssertEquals(true, wrapper.IsApproved);

			apLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.UnapprovedCost;
			AssertEquals(false, wrapper.IsApproved);
		}

		#endregion

		#region TestSellAccount

		public void TestSellAccount()
		{
			var line = Factory.New<ARInvoiceLine>();

			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = line.PK;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "ORGANISATION";

			charge.JR_OH_SellAccount = org1.PK;

			var wrapper = DocARInvoiceLine.New(line, Factory);
			AssertEquals("SellAccount", "ORGANISATION", wrapper.SellAccount);
		}

		#endregion

		#region TestCostAccount

		public void TestCostAccount()
		{
			var line = Factory.New<ARInvoiceLine>();
			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = line.PK;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "ORGANISATION";
			charge.JR_OH_CostAccount = org1.PK;

			var wrapper = DocARInvoiceLine.New(line, Factory);
			AssertEquals("CostAccount", "ORGANISATION", wrapper.CostAccount);
		}

		#endregion

		#region TestSellPosted

		public void TestSellPosted()
		{
			var charge = Factory.New<Charge>();
			AssertEquals(false, charge.IsRevenuePosted);

			var arLine = Factory.New<ARInvoiceLine>();
			charge.JR_AL_ARLine = arLine.PK;
			arLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;

			var wrapper = DocARInvoiceLine.New(arLine, Factory);
			AssertEquals(false, wrapper.SellPosted);

			arLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;
			AssertEquals(false, wrapper.SellPosted);

			arLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			AssertEquals(false, wrapper.SellPosted);

			arLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			AssertEquals(true, wrapper.SellPosted);
		}

		#endregion

		#region TestCostPosted

		public void TestCostPosted()
		{
			var charge = Factory.New<Charge>();

			charge.JR_AL_APLine = ZGuid.Empty;
			AssertEquals(false, charge.IsCostPosted);

			var apLine = Factory.New<ARInvoiceLine>();
			charge.JR_AL_APLine = apLine.PK;

			var arLine = Factory.New<ARInvoiceLine>();
			charge.JR_AL_ARLine = arLine.PK;

			var wrapper = DocARInvoiceLine.New(arLine, Factory);

			apLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			AssertEquals(true, wrapper.CostPosted);

			apLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;
			AssertEquals(false, wrapper.CostPosted);

			apLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			AssertEquals(false, wrapper.CostPosted);

			apLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			AssertEquals(true, wrapper.CostPosted);

			apLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.UnapprovedCost;
			AssertEquals(true, wrapper.CostPosted);
		}

		#endregion

		#region TestIsApportioned

		public void TestIsApportioned()
		{
			var line = Factory.New<ARInvoiceLine>();
			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = line.PK;

			var wrapper = DocARInvoiceLine.New(line, Factory);
			AssertEquals("JR_IsApportioned", false, wrapper.IsApportioned);

			charge.JR_E6 = ZGuid.NewZGuid();
			AssertEquals("JR_IsApportioned", true, wrapper.IsApportioned);
		}

		#endregion

		#region TestCFXJnl

		public void TestCFXJnl()
		{
			var arLine = Factory.New<ARInvoiceLine>();
			var cfxLine = Factory.New<ARInvoiceLine>();
			cfxLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			cfxLine.AL_LineAmount = 50m;

			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = arLine.PK;
			charge.JR_AL_CFXLine = cfxLine.PK;

			var wrapper = DocARInvoiceLine.New(arLine, Factory);
			AssertEquals("JR_CFXAmtReverseSign", 50m, wrapper.CFXJnl);
		}

		#endregion

		#region TestChargeCodePrint

		public void TestChargeCodePrint()
		{
			var arLine = Factory.New<ARInvoiceLine>();
			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = arLine.PK;

			var chargeCode = TestObjectCreator.CreateChargeCode("CC");
			chargeCode.AC_PrintSequence = 1;
			charge.JR_AC = chargeCode.PK;

			var wrapper = DocARInvoiceLine.New(arLine, Factory);
			AssertEquals("JR_CFXAmtReverseSign", (ZShort)1, wrapper.ChargeCodePrintSequence);

			chargeCode.AC_PrintSequence = 2;
			AssertEquals("JR_CFXAmtReverseSign", (ZShort)2, wrapper.ChargeCodePrintSequence);
		}

		#endregion

		#region TestDisplaySequence

		public void TestDisplaySequence()
		{
			var arLine = Factory.New<ARInvoiceLine>();
			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = arLine.PK;
			charge.JR_DisplaySequence = 1;

			var wrapper = DocARInvoiceLine.New(arLine, Factory);
			AssertEquals("EstimatedCost", 1, wrapper.DisplaySequence);

			charge.JR_DisplaySequence = 2;
			AssertEquals("EstimatedCost", 2, wrapper.DisplaySequence);
		}

		#endregion

		#region TestEstimatedCost

		public void TestEstimatedCost()
		{
			var arLine = Factory.New<ARInvoiceLine>();

			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = arLine.PK;
			charge.JR_EstimatedCost = 1234m;

			var wrapper = DocARInvoiceLine.New(arLine, Factory);
			AssertEquals("EstimatedCost", 1234m, wrapper.EstimatedCost);
		}

		#endregion

		#region TestEstimatedCostWithCurrency

		public void TestEstimatedCostWithCurrency()
		{
			var arLine = Factory.New<ARInvoiceLine>();

			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = arLine.PK;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_EstimatedCost = 1234m;

			var wrapper = DocARInvoiceLine.New(arLine, Factory);
			AssertEquals("EstimatedCostWithCurrency", "1,234.00 USD", wrapper.EstimatedCostWithCurrency);
		}

		#endregion

		#region TestEstimatedRevenue

		public void TestEstimatedRevenue()
		{
			var arLine = Factory.New<ARInvoiceLine>();

			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = arLine.PK;
			charge.JR_EstimatedRevenue = 1234m;

			var wrapper = DocARInvoiceLine.New(arLine, Factory);
			AssertEquals("EstimatedRevenue", 1234m, wrapper.EstimatedRevenue);
		}

		#endregion

		#region TestEstimatedRevenueWithCurrency

		public void TestEstimatedRevenueWithCurrency()
		{
			var arLine = Factory.New<ARInvoiceLine>();

			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = arLine.PK;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_EstimatedRevenue = 1234m;

			var wrapper = DocARInvoiceLine.New(arLine, Factory);
			AssertEquals("EstimatedRevenueWithCurrency", "1,234.00 USD", wrapper.EstimatedRevenueWithCurrency);
		}

		#endregion

		#region TestLocalCostAmount

		public void TestLocalCostAmount()
		{
			var arLine = Factory.New<ARInvoiceLine>();

			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = arLine.PK;
			charge.JR_LocalCostAmt = 1234m;

			var wrapper = DocARInvoiceLine.New(arLine, Factory);
			AssertEquals("LocalCostAmount", 1234m, wrapper.LocalCostAmount);
		}

		#endregion

		#region TestLocalCostAmountWithCurrency

		public void TestLocalCostAmountWithCurrency()
		{
			var arLine = Factory.New<ARInvoiceLine>();

			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = Line.PK;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_LocalCostAmt = 1234m;

			var wrapper = DocARInvoiceLine.New(Line, Factory);
			AssertEquals("LocalCostAmountWithCurrency", "1,234.00 USD", wrapper.LocalCostAmountWithCurrency);
		}

		#endregion

		#region TestLocalSellAmount

		public void TestLocalSellAmount()
		{
			var arLine = Factory.New<ARInvoiceLine>();

			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = arLine.PK;
			charge.JR_LocalSellAmt = 1234m;

			var wrapper = DocARInvoiceLine.New(arLine, Factory);
			AssertEquals("LocalSellAmount", 1234m, wrapper.LocalSellAmount);
		}

		#endregion

		#region TestLocalSellAmountWithCurrency

		public void TestLocalSellAmountWithCurrency()
		{
			var arLine = Factory.New<ARInvoiceLine>();

			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = arLine.PK;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_LocalSellAmt = 1234m;

			var wrapper = DocARInvoiceLine.New(arLine, Factory);
			AssertEquals("LocalSellAmountWithCurrency", "1,234.00 USD", wrapper.LocalSellAmountWithCurrency);
		}

		#endregion

		#region TestCostOSAmountAndCurrency

		public void TestCostOSAmountAndCurrency()
		{
			var arLine = Factory.New<ARInvoiceLine>();

			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = arLine.PK;
			charge.JR_RX_NKCostCurrency = "USD";
			charge.JR_OSCostAmt = 1000m;
			charge.JR_OSCostExRate = 0.707m;
			var wrapper = DocARInvoiceLine.New(arLine, Factory);
			AssertEquals("CostOSAmountAndCurrency", "1,000.00 USD", wrapper.CostOSAmountAndCurrency);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Iceland))
			{
				AssertEquals("Formatting for Iceland", "1.000,00 USD", wrapper.CostOSAmountAndCurrency);
			}
		}

		#endregion

		#region TestChargeExchangeRate

		public void TestCostExchangeRate()
		{
			var arLine = Factory.New<ARInvoiceLine>();

			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = arLine.PK;
			charge.JR_RX_NKCostCurrency = "USD";
			charge.JR_OSCostAmt = 1000m;
			charge.JR_OSCostExRate = 0.7089m;

			var wrapper = DocARInvoiceLine.New(arLine, Factory);
			AssertEquals("ChargeExchangeRate", 0.7089m, wrapper.CostExchangeRate);
		}

		#endregion

		#region TestCostCurrency

		public void TestCostCurrency()
		{
			var arLine = Factory.New<ARInvoiceLine>();

			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = arLine.PK;
			charge.JR_RX_NKCostCurrency = "USD";
			charge.JR_OSCostAmt = 1000m;
			charge.JR_OSCostExRate = 0.7089m;

			var wrapper = DocARInvoiceLine.New(arLine, Factory);
			AssertEquals("ChargeCurrency", "USD", wrapper.CostCurrency);
		}

		#endregion

		#region TestCostOSAmount

		public void TestCostOSAmount()
		{
			var arLine = Factory.New<ARInvoiceLine>();

			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = arLine.PK;
			charge.JR_RX_NKCostCurrency = "USD";
			charge.JR_OSCostAmt = 1000m;
			charge.JR_OSCostExRate = 0.7089m;

			var wrapper = DocARInvoiceLine.New(arLine, Factory);
			AssertEquals("ChargeOSAmount", 1000m, wrapper.CostOSAmount);
		}

		#endregion

		#region TestChargeOSAmount

		public void TestChargeOSAmount()
		{
			var arLine = Factory.New<ARInvoiceLine>();

			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = arLine.PK;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OSSellAmt = 1000m;
			charge.JR_OSSellExRate = 0.7089m;

			var wrapper = DocARInvoiceLine.New(arLine, Factory);
			AssertEquals("ChargeOSAmount", 1000m, wrapper.ChargeOSAmount);
		}

		#endregion

		#region TestChargeOSAmountForCLC

		public void TestChargeOSAmountForCLC()
		{
			var invoice = Factory.New<ARInvoice>();
			var invoiceLine = Factory.New<ARInvoiceLine>();
			invoiceLine.AL_AH = invoice.PK;

			var charge1 = Factory.New<Charge>();
			charge1.JR_AL_ARLine = invoiceLine.PK;
			charge1.JR_RX_NKSellCurrency = "USD";
			charge1.JR_OSSellAmt = 1000m;
			charge1.JR_OSSellExRate = 0.7089m;

			var wrapper1 = DocARInvoiceLine.New(invoiceLine, Factory);
			AssertEquals("ChargeOSAmount", 1000m, wrapper1.ChargeOSAmount);
			AssertEquals("ChargeOSAmountForCLC", 1000m, wrapper1.ChargeOSAmountForCLC);

			var creditNote = Factory.New<ARCreditNote>();
			var creditNoteLine = Factory.New<ARCreditNoteLine>();
			creditNoteLine.AL_AH = creditNote.PK;

			var charge2 = Factory.New<Charge>();
			charge2.JR_AL_ARLine = creditNoteLine.PK;
			charge2.JR_RX_NKSellCurrency = "USD";
			charge2.JR_OSSellAmt = -1000m;
			charge2.JR_OSSellExRate = 0.7089m;

			var wrapper2 = DocARInvoiceLine.New(creditNoteLine, Factory);
			AssertEquals("ChargeOSAmount", -1000m, wrapper2.ChargeOSAmount);
			AssertEquals("ChargeOSAmountForCLC", 1000m, wrapper2.ChargeOSAmountForCLC);
		}

		#endregion

		#region TestOSAmountDisplay

		public void TestOSAmountDisplay()
		{
			Invoice.ExchangeRate.Currency = "USD";
			Line.AL_OSAmount = 1111.22222222m;
			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
			AssertEquals("OSDisplay amount", "1,111.22", InvoiceLineWrapper.OSAmountDisplay);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Iceland))
			{
				AssertEquals("Formatting for Iceland", "1.111,22", InvoiceLineWrapper.OSAmountDisplay);
			}
		}

		#endregion

		#region TestExchangeRateDisplay

		public void TestExchangeRateDisplay()
		{
			Invoice.AH_ExchangeRate = 0.717897m;
			Line.AL_ExchangeRate = 0.707897m;
			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
			AssertEquals("Exchange Rate should be displayed in 4 decimal places", "0.7179", InvoiceLineWrapper.ExchangeRateDisplay);
		}

		#endregion

		#region TestIsConsignmentJob

		public void TestIsConsignmentJob()
		{
			var booking = (DtbBooking)Helper.CreateBooking("TB1");
			booking.KM_JobID = "TB-Ref";
			Invoice.AH_ConsolidatedInvoiceRef = "TC-Ref"; // Different from booking.KM_JobID
			var jobForBooking = GetInvoiceJob(booking, Invoice);
			Line.AL_JH = jobForBooking.PK;
			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
			AssertEquals("Since it's a transport booking it should be false.", false, InvoiceLineWrapper.IsConsignmentJob);
		}

		#endregion

		#region TestCustomsInvoiceLineType

		public void TestCustomsInvoiceLineType()
		{
			Invoice.AH_ConsolidatedInvoiceRef = "B120";
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B120";
			JobHeader job = GetInvoiceJob(declaration, Invoice);
			Line.AL_JH = job.PK;
			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
			Assert("Invoice Type", InvoiceLineWrapper.IsCustomJob);
		}

		#endregion

		#region TestLoadListInvoiceType

		public void TestLoadListInvoiceType()
		{
			CFSLoadListConsol loadListConsol = Factory.New<CFSLoadListConsol>();
			loadListConsol.JK_IsForwarding = ZBool.False;
			loadListConsol.JK_IsCFS = ZBool.True;

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = loadListConsol.JK_UniqueConsignRef;
			JobHeader job = GetInvoiceJob(loadListConsol, Invoice);
			Line.AL_JH = job.PK;

			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
			Assert("Invoice Type", InvoiceLineWrapper.IsLoadListJob);

			loadListConsol.JK_IsForwarding = ZBool.True;
			loadListConsol.JK_IsCFS = ZBool.True;

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = loadListConsol.JK_UniqueConsignRef;

			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
			Assert("Invoice Type", InvoiceLineWrapper.IsLoadListJob);
		}

		#endregion

		#region TestIsSundryCharges

		public void TestIsSundryCharges()
		{
			Line.AL_JH = GetInvoiceJob(Factory.New<SundryCharges>(), Invoice).PK;
			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
			Assert(InvoiceLineWrapper.IsSundryCharges);
		}

		#endregion

		#region TestIsVoyageAccounting

		public void TestIsVoyageAccounting()
		{
			Line.AL_JH = GetInvoiceJob(Factory.New<VoyageAccount>(), Invoice).PK;
			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
			Assert(InvoiceLineWrapper.IsVoyageAccounting);
		}

		#endregion

		#region TestIsContainerDetention

		public void TestIsContainerDetention()
		{
			Line.AL_JH = GetInvoiceJob(Factory.New<ContainerDetention>(), Invoice).PK;
			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
			Assert(InvoiceLineWrapper.IsContainerDetention);
		}

		#endregion

		#region TestShipmentInvoiceType

		public void TestShipmentInvoiceType()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S1234";

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			JobHeader job = GetInvoiceJob(shipment, Invoice);
			Line.AL_JH = job.PK;

			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
			Assert("Invoice Type", InvoiceLineWrapper.IsForwardingJob);

			shipment.JS_IsCFSRegistered = ZBool.True;
			shipment.JS_IsForwardRegistered = ZBool.False;

			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
			Assert("Invoice Type", !InvoiceLineWrapper.IsForwardingJob);

			shipment.JS_IsCFSRegistered = ZBool.True;
			shipment.JS_IsForwardRegistered = ZBool.True;

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			job = GetInvoiceJob(shipment, Invoice);
			Line.AL_JH = job.PK;

			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
			Assert("Invoice Type", InvoiceLineWrapper.IsForwardingJob);
		}

		#endregion

		#region TestTransportTypeInvoice

		public void TestTransportTypeInvoice()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ConsignmentID = "T0374839";
			Invoice.AH_ConsolidatedInvoiceRef = cartage.JJ_ConsignmentID;
			Factory.Save();
			JobHeader job = GetInvoiceJob(cartage, Invoice);
			Line.AL_JH = job.PK;

			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
			Assert("Invoice Type", InvoiceLineWrapper.IsLocalCartage);
		}

		#endregion

		#region TestClientReferenceForLoadList

		public void TestClientReferenceForLoadList()
		{
			CFSLoadListConsol consol = Factory.NewWithValidTestData<CFSLoadListConsol>();
			consol.JK_AgentsReference = "TEST";

			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			job.JH_ParentID = consol.PK;

			Line.AL_JH = job.PK;

			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);

			AssertEquals(consol.JK_AgentsReference, InvoiceLineWrapper.ClientReference);
		}

		#endregion

		#region TestClientReferenceForShipment

		public void TestClientReferenceForShipment()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ConsolReference = "TEST";

			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;

			Line.AL_JH = job.PK;

			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);

			AssertEquals(shipment.JS_ConsolReference, InvoiceLineWrapper.ClientReference);
		}

		#endregion

		#region TestClientReferenceForOther

		public void TestClientReferenceForOther()
		{
			BaseJobDeclaration declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();

			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			job.JH_ParentID = declaration.PK;

			Line.AL_JH = job.PK;

			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);

			AssertEquals("", InvoiceLineWrapper.ClientReference);
		}

		#endregion

		#region TestOtherRefernceForLoadListJob

		public void TestOtherRefernceForLoadListJob()
		{
			CFSLoadListConsol consol = Factory.NewWithValidTestData<CFSLoadListConsol>();
			consol.JK_AgentsReference = "TEST";
			consol.JK_BookingReference = "Con1";
			consol.JK_MasterBillNum = "Bill1";

			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			job.JH_ParentID = consol.PK;

			Line.AL_JH = job.PK;

			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);

			AssertEquals("Con1 / Bill1", InvoiceLineWrapper.OtherReference);
		}

		#endregion

		#region TestOtherRefernceForCFSShipmentJob

		public void TestOtherRefernceForCFSShipmentJob()
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			CFSShipment cFSShipment = Factory.New<CFSShipment>();

			cFSShipment.Consols.Add(consol);

			cFSShipment.JS_BookingReference = "Con1";
			consol.JK_MasterBillNum = "Bill1";

			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = cFSShipment.PK;

			Line.AL_JH = job.PK;

			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);

			AssertEquals("Con1 / Bill1", InvoiceLineWrapper.OtherReference);
		}

		public void TestOtherRefernceForCFSShipmentJobAfterDeleteShipmentWithNoException()
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			CFSShipment cFSShipment = Factory.New<CFSShipment>();
			consol.Shipments.Add(cFSShipment);

			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = cFSShipment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_JobNum = "TestJob";

			Line.AL_JH = job.PK;

			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);

			cFSShipment.Delete();

			AssertNoExceptionThrown(() =>
			{
				var otherReference = InvoiceLineWrapper.OtherReference;
			});
		}

		#endregion

		#region TestOtherReferenceForIsLocalCartage

		public void TestOtherReferenceForIsLocalCartage()
		{
			RefContainer containerType1 = Factory.New<RefContainer>();
			RefContainer containerType2 = Factory.New<RefContainer>();
			containerType1.RC_Code = "20FT";
			containerType2.RC_Code = "40FT";

			CommonCartage localTransport = Factory.New<CommonCartage>();
			localTransport.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			localTransport.JJ_OrderReferenceNumber = "OrderRef";

			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentTableCode = JobCartageSchema.Constants.Prefix;
			job.JH_ParentID = localTransport.PK;

			Line.AL_JH = job.PK;

			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
			AssertEquals("OrderRef", InvoiceLineWrapper.OtherReference);

			localTransport.ContainerBookedMoves.AddNew();
			localTransport.ContainerBookedMoves.AddNew();

			localTransport.Containers.ElementAt(0).JC_RC = containerType1.PK;
			localTransport.Containers.ElementAt(0).JC_ContainerNum = "CON1";
			localTransport.Containers.ElementAt(1).JC_RC = containerType2.PK;
			localTransport.Containers.ElementAt(1).JC_ContainerNum = "CON2";

			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);

			AssertEquals("OrderRef, CON1:20FT CON2:40FT ", InvoiceLineWrapper.OtherReference);
		}

		#endregion

		#region TestOtherReferenceForIsLocalCartage_NonFCLType

		public void TestOtherReferenceForIsLocalCartage_NonFCLType()
		{
			RefContainer containerType1 = Factory.New<RefContainer>();
			RefContainer containerType2 = Factory.New<RefContainer>();
			containerType1.RC_Code = "20FT";
			containerType2.RC_Code = "40FT";

			CommonCartage localTransport = Factory.New<CommonCartage>();
			localTransport.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;

			localTransport.ContainerBookedMoves.AddNew();
			localTransport.ContainerBookedMoves.AddNew();

			localTransport.Containers.ElementAt(0).JC_RC = containerType1.PK;
			localTransport.Containers.ElementAt(0).JC_ContainerNum = "CON1";
			localTransport.Containers.ElementAt(1).JC_RC = containerType2.PK;
			localTransport.Containers.ElementAt(1).JC_ContainerNum = "CON2";

			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentTableCode = JobCartageSchema.Constants.Prefix;
			job.JH_ParentID = localTransport.PK;

			Line.AL_JH = job.PK;

			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);

			AssertEquals("", InvoiceLineWrapper.OtherReference);
		}

		#endregion

		#region TestOtherReferenceForCustomsJob

		public void TestOtherReferenceForCustomsJob()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			Order order = Factory.New<Order>();
			order.JD_OrderNumber = "OrderNo";
			shipment.AttachedOrders.Add(order);

			BaseJobDeclaration customs = Factory.New<BaseJobDeclaration>();
			customs.JE_OwnerRef = "Owner";
			customs.JE_JS = shipment.PK;

			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			job.JH_ParentID = customs.PK;

			Line.AL_JH = job.PK;

			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);

			AssertEquals("Owner,OrderNo", InvoiceLineWrapper.OtherReference);
		}

		#endregion

		#region TestOtherReferenceForCustomsJob_WithContainerNo

		public void TestOtherReferenceForCustomsJob_WithContainerNo()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			Order order = Factory.New<Order>();
			order.JD_OrderNumber = "OrderNo";
			shipment.AttachedOrders.Add(order);

			BaseJobDeclaration customs = Factory.New<BaseJobDeclaration>();
			customs.JE_OwnerRef = "Owner";
			customs.JE_JS = shipment.PK;
			customs.CusContainers.AddNew();
			customs.CusContainers.AddNew();

			customs.CusContainers[0].CO_ContainerNumber = "CONT1";
			customs.CusContainers[0].CO_FCL_LCL_AIR = "FCL";
			customs.CusContainers[1].CO_ContainerNumber = "CONT2";
			customs.CusContainers[1].CO_FCL_LCL_AIR = "LCL";

			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			job.JH_ParentID = customs.PK;

			Line.AL_JH = job.PK;

			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);

			AssertEquals("Owner,OrderNo,CONT1 ", InvoiceLineWrapper.OtherReference);
		}

		#endregion

		#region TestOtherReferenceForShipment

		public void TestOtherReferenceForShipment()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			consol.Shipments.Add(shipment);

			var organisation1 = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var order1 = shipment.AttachedOrders.AddNew();
			order1.JD_OrderNumber = "Order number 1";

			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_JobNum = "TestJob";

			Line.AL_JH = job.PK;

			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);

			AssertEquals("Order number 1 ", InvoiceLineWrapper.OtherReference);
		}

		public void TestGetOtherReferenceForShipmentAfterDeleteShipmentWithNoException()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			consol.Shipments.Add(shipment);

			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_JobNum = "TestJob";

			Line.AL_JH = job.PK;

			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);

			job.Delete();
			shipment.Delete();

			AssertNoExceptionThrown(() =>
			{
				var otherReference = InvoiceLineWrapper.OtherReference;
			});
		}

		#endregion

		#region TestShippersReferenceForShipment

		public void TestShippersReferenceForShipment()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_BookingReference = "ABC12345";

			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_JobNum = "TestJob";

			Line.AL_JH = job.PK;

			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);

			AssertEquals("ABC12345", InvoiceLineWrapper.ShippersReference);
		}

		#endregion

		#region TestOSTaxDisplayMainRateWithAsterisks

		public void TestOSTaxDisplayMainRateWithAsterisks()
		{
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			DocARBatchInvoiceTest.SetupForTestingInvoiceTaxMessages(invoice);

			// note: AccTaxRate with empty AT_Type cannot be saved.
			invoice.Lines[0].TaxRate.AT_Type = ZString.Empty;

			AssertEquals("Line 0", "N/A *", DocARInvoiceLine.New(invoice.Lines[0], Factory).OSTaxDisplayMainRate);
			AssertEquals("Line 0", "N/A", DocARInvoiceLine.New(invoice.Lines[0], Factory).OSTaxDisplayMainRateNoAsterisks);

			invoice.Lines[1].TaxRate.AT_Type = AccTaxRate.Types.Rated;
			AssertEquals("Line 1", "0% **", DocARInvoiceLine.New(invoice.Lines[1], Factory).OSTaxDisplayMainRate);
			AssertEquals("Line 1", "0%", DocARInvoiceLine.New(invoice.Lines[1], Factory).OSTaxDisplayMainRateNoAsterisks);
			AssertEquals("Line 1", " **", DocARInvoiceLine.New(invoice.Lines[1], Factory).TaxRateAsterisks);
			AssertEquals("Line 1", " 2.", DocARInvoiceLine.New(invoice.Lines[1], Factory).TaxRateAsterisksAsNumbers);

			invoice.Lines[2].TaxRate.AT_Type = AccTaxRate.Types.CapitalRated;
			AssertEquals("Line 2", "0% ***", DocARInvoiceLine.New(invoice.Lines[2], Factory).OSTaxDisplayMainRate);
			AssertEquals("Line 2", "0%", DocARInvoiceLine.New(invoice.Lines[2], Factory).OSTaxDisplayMainRateNoAsterisks);

			invoice.Lines[3].TaxRate.AT_Type = AccTaxRate.Types.Rated;
			AssertEquals("Line 3", "0% ****", DocARInvoiceLine.New(invoice.Lines[3], Factory).OSTaxDisplayMainRate);
			AssertEquals("Line 3", "0%", DocARInvoiceLine.New(invoice.Lines[3], Factory).OSTaxDisplayMainRateNoAsterisks);

			invoice.Lines[4].TaxRate.AT_Type = AccTaxRate.Types.CapitalRated;
			AssertEquals("Line 4", "0% *****", DocARInvoiceLine.New(invoice.Lines[4], Factory).OSTaxDisplayMainRate);
			AssertEquals("Line 4", "0%", DocARInvoiceLine.New(invoice.Lines[4], Factory).OSTaxDisplayMainRateNoAsterisks);

			invoice.Lines[0].TaxRate.AT_Type = AccTaxRate.Types.Exempt;
			AssertEquals("Line 0", "Exempt *", DocARInvoiceLine.New(invoice.Lines[0], Factory).OSTaxDisplayMainRate);
			AssertEquals("Line 0", "Exempt", DocARInvoiceLine.New(invoice.Lines[0], Factory).OSTaxDisplayMainRateNoAsterisks);

			invoice.Lines[1].TaxRate.AT_Type = AccTaxRate.Types.ReverseRated;
			AssertEquals("Line 1", "Reverse **", DocARInvoiceLine.New(invoice.Lines[1], Factory).OSTaxDisplayMainRate);
			AssertEquals("Line 1", "Reverse", DocARInvoiceLine.New(invoice.Lines[1], Factory).OSTaxDisplayMainRateNoAsterisks);

			invoice.Lines[2].TaxRate.AT_Type = AccTaxRate.Types.NotReportable;
			AssertEquals("Line 2", "N/A ***", DocARInvoiceLine.New(invoice.Lines[2], Factory).OSTaxDisplayMainRate);
			AssertEquals("Line 2", "N/A", DocARInvoiceLine.New(invoice.Lines[2], Factory).OSTaxDisplayMainRateNoAsterisks);

			invoice.Lines[3].TaxRate.AT_Type = AccTaxRate.Types.Suspended;
			AssertEquals("Line 3", "Suspended ****", DocARInvoiceLine.New(invoice.Lines[3], Factory).OSTaxDisplayMainRate);
			AssertEquals("Line 3", "Suspended", DocARInvoiceLine.New(invoice.Lines[3], Factory).OSTaxDisplayMainRateNoAsterisks);

			invoice.Lines[4].TaxRate.AT_Type = AccTaxRate.Types.ExcludedFromTheTaxBase;
			AssertEquals("Line 4", "Excluded *****", DocARInvoiceLine.New(invoice.Lines[4], Factory).OSTaxDisplayMainRate);
			AssertEquals("Line 4", "Excluded", DocARInvoiceLine.New(invoice.Lines[4], Factory).OSTaxDisplayMainRateNoAsterisks);
		}

		#endregion

		public void TestTaxRateAsterisksAsNumbers()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var taxMessage = invoice.Factory.NewWithValidTestData<AccInvMsg>();
			taxMessage.A9_EnglishMsg = "ENGLISH 1";
			taxMessage.A9_LocalMsg = "LOCAL 1";
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_OSExTaxAmount = 100m;
			line.AL_A9_VATClass = taxMessage.PK;

			AssertEquals(ZString.Empty, DocARInvoiceLine.New(invoice.Lines[0], Factory).TaxRateAsterisksAsNumbers);
			AssertEquals(ZString.Empty, DocARInvoiceLine.New(invoice.Lines[0], Factory).TaxRateAsterisks);

			var taxRate = invoice.Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			line.AL_AT = taxRate.PK;
			line.AL_A9_VATClass = taxMessage.PK;

			AssertEquals(" 1.", DocARInvoiceLine.New(line, Factory).TaxRateAsterisksAsNumbers);
			AssertEquals(" *", DocARInvoiceLine.New(line, Factory).TaxRateAsterisks);
		}

		#region TestOSTaxDisplayWithAsterisks

		public void TestOSTaxDisplayWithAsterisks()
		{
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			DocARBatchInvoiceTest.SetupForTestingInvoiceTaxMessages(invoice);

			// note: AccTaxRate with empty AT_Type cannot be saved.
			invoice.Lines[0].TaxRate.AT_Type = ZString.Empty;
			invoice.Lines[1].TaxRate.AT_Type = ZString.Empty;
			invoice.Lines[2].TaxRate.AT_Type = ZString.Empty;
			invoice.Lines[3].TaxRate.AT_Type = ZString.Empty;
			invoice.Lines[4].TaxRate.AT_Type = ZString.Empty;

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("Line 0", "0%=0.00 *", DocARInvoiceLine.New(invoice.Lines[0], Factory).OSTaxDisplay);
				AssertEquals("Line 0", "0%=0.00", DocARInvoiceLine.New(invoice.Lines[0], Factory).OSTaxDisplayNoAsterisks);
				AssertEquals("Line 0", " *", DocARInvoiceLine.New(invoice.Lines[0], Factory).TaxRateAsterisks);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.Both.Code);
				AssertEquals("Line 0", "0%=0.00", DocARInvoiceLine.New(invoice.Lines[0], Factory).OSTaxDisplayNoAsterisksWithRegistryRule);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxAmount.Code);
				AssertEquals("Line 0", "0.00", DocARInvoiceLine.New(invoice.Lines[0], Factory).OSTaxDisplayNoAsterisksWithRegistryRule);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxRate.Code);
				AssertEquals("Line 0", "0%", DocARInvoiceLine.New(invoice.Lines[0], Factory).OSTaxDisplayNoAsterisksWithRegistryRule);
				AssertEquals("Line 0", " 1.", DocARInvoiceLine.New(invoice.Lines[0], Factory).TaxRateAsterisksAsNumbers);

				AssertEquals("Line 1", "0%=0.00 **", DocARInvoiceLine.New(invoice.Lines[1], Factory).OSTaxDisplay);
				AssertEquals("Line 1", "0%=0.00", DocARInvoiceLine.New(invoice.Lines[1], Factory).OSTaxDisplayNoAsterisks);
				AssertEquals("Line 1", " **", DocARInvoiceLine.New(invoice.Lines[1], Factory).TaxRateAsterisks);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.Both.Code);
				AssertEquals("Line 1", "0%=0.00", DocARInvoiceLine.New(invoice.Lines[1], Factory).OSTaxDisplayNoAsterisksWithRegistryRule);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxAmount.Code);
				AssertEquals("Line 1", "0.00", DocARInvoiceLine.New(invoice.Lines[1], Factory).OSTaxDisplayNoAsterisksWithRegistryRule);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxRate.Code);
				AssertEquals("Line 1", "0%", DocARInvoiceLine.New(invoice.Lines[1], Factory).OSTaxDisplayNoAsterisksWithRegistryRule);
				AssertEquals("Line 1", " 2.", DocARInvoiceLine.New(invoice.Lines[1], Factory).TaxRateAsterisksAsNumbers);

				AssertEquals("Line 2", "0%=0.00 ***", DocARInvoiceLine.New(invoice.Lines[2], Factory).OSTaxDisplay);
				AssertEquals("Line 2", "0%=0.00", DocARInvoiceLine.New(invoice.Lines[2], Factory).OSTaxDisplayNoAsterisks);
				AssertEquals("Line 2", " ***", DocARInvoiceLine.New(invoice.Lines[2], Factory).TaxRateAsterisks);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.Both.Code);
				AssertEquals("Line 2", "0%=0.00", DocARInvoiceLine.New(invoice.Lines[2], Factory).OSTaxDisplayNoAsterisksWithRegistryRule);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxAmount.Code);
				AssertEquals("Line 2", "0.00", DocARInvoiceLine.New(invoice.Lines[2], Factory).OSTaxDisplayNoAsterisksWithRegistryRule);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxRate.Code);
				AssertEquals("Line 2", " 3.", DocARInvoiceLine.New(invoice.Lines[2], Factory).TaxRateAsterisksAsNumbers);

				AssertEquals("Line 3", "0%=0.00 ****", DocARInvoiceLine.New(invoice.Lines[3], Factory).OSTaxDisplay);
				AssertEquals("Line 3", "0%=0.00", DocARInvoiceLine.New(invoice.Lines[3], Factory).OSTaxDisplayNoAsterisks);
				AssertEquals("Line 3", " ****", DocARInvoiceLine.New(invoice.Lines[3], Factory).TaxRateAsterisks);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.Both.Code);
				AssertEquals("Line 3", "0%=0.00", DocARInvoiceLine.New(invoice.Lines[3], Factory).OSTaxDisplayNoAsterisksWithRegistryRule);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxAmount.Code);
				AssertEquals("Line 3", "0.00", DocARInvoiceLine.New(invoice.Lines[3], Factory).OSTaxDisplayNoAsterisksWithRegistryRule);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxRate.Code);
				AssertEquals("Line 3", " 4.", DocARInvoiceLine.New(invoice.Lines[3], Factory).TaxRateAsterisksAsNumbers);

				AssertEquals("Line 4", "0%=0.00 *****", DocARInvoiceLine.New(invoice.Lines[4], Factory).OSTaxDisplay);
				AssertEquals("Line 4", "0%=0.00", DocARInvoiceLine.New(invoice.Lines[4], Factory).OSTaxDisplayNoAsterisks);
				AssertEquals("Line 4", " *****", DocARInvoiceLine.New(invoice.Lines[4], Factory).TaxRateAsterisks);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.Both.Code);
				AssertEquals("Line 4", "0%=0.00", DocARInvoiceLine.New(invoice.Lines[4], Factory).OSTaxDisplayNoAsterisksWithRegistryRule);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxAmount.Code);
				AssertEquals("Line 4", "0.00", DocARInvoiceLine.New(invoice.Lines[4], Factory).OSTaxDisplayNoAsterisksWithRegistryRule);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxRate.Code);
				AssertEquals("Line 4", " 5.", DocARInvoiceLine.New(invoice.Lines[4], Factory).TaxRateAsterisksAsNumbers);
			}

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("Line 0", "0%=0,00 *", DocARInvoiceLine.New(invoice.Lines[0], Factory).OSTaxDisplay);
				AssertEquals("Line 0", "0%=0,00", DocARInvoiceLine.New(invoice.Lines[0], Factory).OSTaxDisplayNoAsterisks);
				AssertEquals("Line 0", " *", DocARInvoiceLine.New(invoice.Lines[0], Factory).TaxRateAsterisks);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.Both.Code);
				AssertEquals("Line 0", "0%=0,00", DocARInvoiceLine.New(invoice.Lines[0], Factory).OSTaxDisplayNoAsterisksWithRegistryRule);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxAmount.Code);
				AssertEquals("Line 0", "0,00", DocARInvoiceLine.New(invoice.Lines[0], Factory).OSTaxDisplayNoAsterisksWithRegistryRule);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxRate.Code);
				AssertEquals("Line 0", " 1.", DocARInvoiceLine.New(invoice.Lines[0], Factory).TaxRateAsterisksAsNumbers);

				AssertEquals("Line 1", "0%=0,00 **", DocARInvoiceLine.New(invoice.Lines[1], Factory).OSTaxDisplay);
				AssertEquals("Line 1", "0%=0,00", DocARInvoiceLine.New(invoice.Lines[1], Factory).OSTaxDisplayNoAsterisks);
				AssertEquals("Line 1", " **", DocARInvoiceLine.New(invoice.Lines[1], Factory).TaxRateAsterisks);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.Both.Code);
				AssertEquals("Line 1", "0%=0,00", DocARInvoiceLine.New(invoice.Lines[1], Factory).OSTaxDisplayNoAsterisksWithRegistryRule);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxAmount.Code);
				AssertEquals("Line 1", "0,00", DocARInvoiceLine.New(invoice.Lines[1], Factory).OSTaxDisplayNoAsterisksWithRegistryRule);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxRate.Code);
				AssertEquals("Line 1", " 2.", DocARInvoiceLine.New(invoice.Lines[1], Factory).TaxRateAsterisksAsNumbers);

				AssertEquals("Line 2", "0%=0,00 ***", DocARInvoiceLine.New(invoice.Lines[2], Factory).OSTaxDisplay);
				AssertEquals("Line 2", "0%=0,00", DocARInvoiceLine.New(invoice.Lines[2], Factory).OSTaxDisplayNoAsterisks);
				AssertEquals("Line 2", " ***", DocARInvoiceLine.New(invoice.Lines[2], Factory).TaxRateAsterisks);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.Both.Code);
				AssertEquals("Line 2", "0%=0,00", DocARInvoiceLine.New(invoice.Lines[2], Factory).OSTaxDisplayNoAsterisksWithRegistryRule);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxAmount.Code);
				AssertEquals("Line 2", "0,00", DocARInvoiceLine.New(invoice.Lines[2], Factory).OSTaxDisplayNoAsterisksWithRegistryRule);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxRate.Code);
				AssertEquals("Line 2", " 3.", DocARInvoiceLine.New(invoice.Lines[2], Factory).TaxRateAsterisksAsNumbers);

				AssertEquals("Line 3", "0%=0,00 ****", DocARInvoiceLine.New(invoice.Lines[3], Factory).OSTaxDisplay);
				AssertEquals("Line 3", "0%=0,00", DocARInvoiceLine.New(invoice.Lines[3], Factory).OSTaxDisplayNoAsterisks);
				AssertEquals("Line 3", " ****", DocARInvoiceLine.New(invoice.Lines[3], Factory).TaxRateAsterisks);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.Both.Code);
				AssertEquals("Line 3", "0%=0,00", DocARInvoiceLine.New(invoice.Lines[3], Factory).OSTaxDisplayNoAsterisksWithRegistryRule);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxAmount.Code);
				AssertEquals("Line 3", "0,00", DocARInvoiceLine.New(invoice.Lines[3], Factory).OSTaxDisplayNoAsterisksWithRegistryRule);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxRate.Code);
				AssertEquals("Line 3", " 4.", DocARInvoiceLine.New(invoice.Lines[3], Factory).TaxRateAsterisksAsNumbers);

				AssertEquals("Line 4", "0%=0,00 *****", DocARInvoiceLine.New(invoice.Lines[4], Factory).OSTaxDisplay);
				AssertEquals("Line 4", "0%=0,00", DocARInvoiceLine.New(invoice.Lines[4], Factory).OSTaxDisplayNoAsterisks);
				AssertEquals("Line 4", " *****", DocARInvoiceLine.New(invoice.Lines[4], Factory).TaxRateAsterisks);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.Both.Code);
				AssertEquals("Line 4", "0%=0,00", DocARInvoiceLine.New(invoice.Lines[4], Factory).OSTaxDisplayNoAsterisksWithRegistryRule);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxAmount.Code);
				AssertEquals("Line 4", "0,00", DocARInvoiceLine.New(invoice.Lines[4], Factory).OSTaxDisplayNoAsterisksWithRegistryRule);
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxRate.Code);
				AssertEquals("Line 4", " 5.", DocARInvoiceLine.New(invoice.Lines[4], Factory).TaxRateAsterisksAsNumbers);
			}
		}

		#endregion

		#region TestAmountsForTotal

		public void TestAmountsForTotal()
		{
			var commentChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			commentChargeCode.AC_ChargeType = "CMT";

			var nonCommentChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			nonCommentChargeCode.AC_ChargeType = "FRT";

			foreach (var setCommentChargeCode in new[] { true, false })
			{
				ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
				DocARBatchInvoiceTest.SetupForTestingInvoiceTaxMessages(invoice);

				// note: AccTaxRate with empty AT_Type cannot be saved.
				invoice.Lines[0].TaxRate.AT_Type = ZString.Empty;

				invoice.Lines[0].AL_AC = setCommentChargeCode ? commentChargeCode.PK : nonCommentChargeCode.PK;
				invoice.ExchangeRate.Currency = "USD";
				invoice.Lines[0].AL_OSExTaxAmount = 123.00M;
				invoice.Lines[0].AL_OSAmount = 124.00M;
				invoice.Lines[0].AL_GSTVAT = 125.00M;
				invoice.Lines[0].AL_LineAmount = 126.00M;

				AssertEquals(setCommentChargeCode ? 0M : 123.00M, DocARInvoiceLine.New(invoice.Lines[0], Factory).OSExTaxAmountForTotal);
				AssertEquals(setCommentChargeCode ? 0M : 124.00M, DocARInvoiceLine.New(invoice.Lines[0], Factory).OSAmountForTotal);
				AssertEquals(setCommentChargeCode ? 0M : 125.00M, DocARInvoiceLine.New(invoice.Lines[0], Factory).GSTVATForTotal);
				AssertEquals(setCommentChargeCode ? 0M : 126.00M, DocARInvoiceLine.New(invoice.Lines[0], Factory).LineAmountForTotal);
				AssertEquals(setCommentChargeCode ? 0M : 251.00M, DocARInvoiceLine.New(invoice.Lines[0], Factory).LocalAmountAndTaxForTotal);
				AssertEquals(setCommentChargeCode ? "" : "0.00", DocARInvoiceLine.New(invoice.Lines[0], Factory).TaxAmountDisplayForTotal);
				AssertEquals(setCommentChargeCode ? "" : "0%=0.00 *", DocARInvoiceLine.New(invoice.Lines[0], Factory).OSTaxDisplayForTotal);
			}
		}

		#endregion

		public void TestTaxGroupCode_Fiji()
		{
			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
			AssertNull(InvoiceLineWrapper.TaxRate);
			Assert(InvoiceLineWrapper.TaxGroupCode.IsEmpty);

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_OSExTaxAmount = 100m;
			var taxRate = invoice.Factory.NewWithValidTestData<AccTaxRate>();
			line.AL_AT = taxRate.PK;

			Assert(DocARInvoiceLine.New(invoice.Lines[0], Factory).TaxGroupCode.IsEmpty);

			var taxMsg = Factory.New<AccInvMsg>();
			taxMsg.A9_Code = "FJ";
			taxMsg.A9_Description = "Fiji Demo Tax Msg";
			taxMsg.A9_IsActive = true;
			taxMsg.A9_EnglishMsg = "Fiji Demo Tax Msg";
			taxMsg.A9_LocalMsg = "Fiji Demo Tax Msg";
			taxMsg.A9_RN_NKCountryCode = Core.Constants.CountryCodes.Fiji;
			taxMsg.A9_TaxGroupCode = FijiComplianceInfo.TaxMessageGroupCodes.A;

			taxRate.AT_A9_DefaultVatClass = taxMsg.PK;

			AssertEquals("A", DocARInvoiceLine.New(invoice.Lines[0], Factory).TaxGroupCode);
		}

		public void TestTaxGroupCode_Samoa()
		{
			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
			AssertNull(InvoiceLineWrapper.TaxRate);
			Assert(InvoiceLineWrapper.TaxGroupCode.IsEmpty);

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_OSExTaxAmount = 100m;
			var taxRate = invoice.Factory.NewWithValidTestData<AccTaxRate>();
			line.AL_AT = taxRate.PK;

			Assert(DocARInvoiceLine.New(invoice.Lines[0], Factory).TaxGroupCode.IsEmpty);

			var taxMsg = Factory.New<AccInvMsg>();
			taxMsg.A9_Code = "WS";
			taxMsg.A9_Description = "Samoa Demo Tax Msg";
			taxMsg.A9_IsActive = true;
			taxMsg.A9_EnglishMsg = "Samoa Demo Tax Msg";
			taxMsg.A9_LocalMsg = "Samoa Demo Tax Msg";
			taxMsg.A9_RN_NKCountryCode = Core.Constants.CountryCodes.WesternSamoa;
			taxMsg.A9_TaxGroupCode = SamoaComplianceInfo.TaxMessageGroupCodes.A;

			taxRate.AT_A9_DefaultVatClass = taxMsg.PK;

			AssertEquals("A", DocARInvoiceLine.New(invoice.Lines[0], Factory).TaxGroupCode);
		}

		#region TestDisplayCurrency

		public void TestDisplayCurrency()
		{
			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
			Line.InvoiceBase.AH_RX_NKTransactionCurrency = "AUD";
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";
			AssertEquals("AUD", InvoiceLineWrapper.DisplayCurrency.Code);

			Line.InvoiceBase.AH_RX_NKTransactionCurrency = "VND";
			AssertEquals("VND", InvoiceLineWrapper.DisplayCurrency.Code);
		}

		#endregion

		#region TestOSAmountsForLocalCurrencyInvoice

		public void TestOSAmountsForLocalCurrencyInvoice()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.India);

			Line.TransactionHeader.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);

			Line.ExchangeRate.Currency = "USD";
			Line.ExchangeRate.Rate = 0.5M;

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			Line.AL_AT = taxRate.PK;
			Line.AL_OSExTaxAmount = 123.00M;
			Line.AL_OSTaxAmount = 124.00M;
			Line.AL_OSAmount = 125.00M;
			Line.AL_OSGSTAmount = 126.00M;
			Line.AL_OSExtraTaxAmount = 128.00M;

			AssertEquals(246.00M, InvoiceLineWrapper.OSExTaxAmount);
			AssertEquals(248.00M, InvoiceLineWrapper.OSTaxAmount);
			AssertEquals(494.00M, InvoiceLineWrapper.OSAmount);
			AssertEquals(252.00M, InvoiceLineWrapper.OSGSTAmount);
			AssertEquals(0.00M, InvoiceLineWrapper.OSQSTAmount);
			AssertEquals(0.00M, InvoiceLineWrapper.OSEDUAmount);
			AssertEquals(-256.00M, InvoiceLineWrapper.OSRETAmount);
			AssertEquals(256.00M, InvoiceLineWrapper.OSExtraTaxAmount);
			AssertEquals(0.00M, InvoiceLineWrapper.OSSPVAmount);
			AssertEquals(0.00M, InvoiceLineWrapper.LocalSPVAmount);

			Invoice.ExchangeRate.Currency = "USD";
			Line.AL_OSExTaxAmount = 123.00M;
			Line.AL_OSTaxAmount = 124.00M;
			Line.AL_OSAmount = 125.00M;
			Line.AL_OSGSTAmount = 126.00M;
			Line.AL_OSExtraTaxAmount = 128.00M;

			AssertEquals(123.00M, InvoiceLineWrapper.OSExTaxAmount);
			AssertEquals(124.00M, InvoiceLineWrapper.OSTaxAmount);
			AssertEquals(125.00M, InvoiceLineWrapper.OSAmount);
			AssertEquals(126.00M, InvoiceLineWrapper.OSGSTAmount);
			AssertEquals(0.00M, InvoiceLineWrapper.OSQSTAmount);
			AssertEquals(0.00M, InvoiceLineWrapper.OSEDUAmount);
			AssertEquals(-128.00M, InvoiceLineWrapper.OSRETAmount);
			AssertEquals(0.00M, InvoiceLineWrapper.OSSPVAmount);
			AssertEquals(0.00M, InvoiceLineWrapper.LocalSPVAmount);

			AssertEquals(128.00M, InvoiceLineWrapper.OSExtraTaxAmount);

			taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
			AssertEquals(123.00M, InvoiceLineWrapper.OSExTaxAmount);
			AssertEquals(124.00M, InvoiceLineWrapper.OSTaxAmount);
			AssertEquals(125.00M, InvoiceLineWrapper.OSAmount);
			AssertEquals(126.00M, InvoiceLineWrapper.OSGSTAmount);
			AssertEquals(128.00M, InvoiceLineWrapper.OSQSTAmount);
			AssertEquals(0.00M, InvoiceLineWrapper.OSEDUAmount);
			AssertEquals(-128.00M, InvoiceLineWrapper.OSRETAmount);
			AssertEquals(128.00M, InvoiceLineWrapper.OSExtraTaxAmount);

			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
			taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
			AssertEquals(123.00M, InvoiceLineWrapper.OSExTaxAmount);
			AssertEquals(124.00M, InvoiceLineWrapper.OSTaxAmount);
			AssertEquals(125.00M, InvoiceLineWrapper.OSAmount);
			AssertEquals(126.00M, InvoiceLineWrapper.OSGSTAmount);
			AssertEquals(0.00M, InvoiceLineWrapper.OSQSTAmount);
			AssertEquals(128.00M, InvoiceLineWrapper.OSEDUAmount);
			AssertEquals(-128.00M, InvoiceLineWrapper.OSRETAmount);
			AssertEquals(128.00M, InvoiceLineWrapper.OSExtraTaxAmount);

			taxRate.AT_Type = AccTaxRate.Types.IntegratedGST;
			taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
			Assert(InvoiceLineWrapper.HasIndiaIntegratedOrStateGST);
			AssertEquals(123.00M, InvoiceLineWrapper.OSExTaxAmount);
			AssertEquals(124.00M, InvoiceLineWrapper.OSTaxAmount);
			AssertEquals(125.00M, InvoiceLineWrapper.OSAmount);
			AssertEquals(126.00M, InvoiceLineWrapper.OSGSTAmount);
			AssertEquals(124.00M, InvoiceLineWrapper.OSIntegratedGSTAmount);

			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.StateGST;
			taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
			Assert(InvoiceLineWrapper.HasIndiaIntegratedOrStateGST);
			AssertEquals(123.00M, InvoiceLineWrapper.OSExTaxAmount);
			AssertEquals(124.00M, InvoiceLineWrapper.OSTaxAmount);
			AssertEquals(125.00M, InvoiceLineWrapper.OSAmount);
			AssertEquals(126.00M, InvoiceLineWrapper.OSGSTAmount);
			AssertEquals(128.00M, InvoiceLineWrapper.OSExtraTaxAmount);
			AssertEquals(-4.00M, InvoiceLineWrapper.OSCentreGSTAmount);
			AssertEquals(128.00M, InvoiceLineWrapper.OSStateGSTAmount);
		}

		public void TestOSAmountsForLocalCurrencyInvoice_SER()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.India);

			Line.TransactionHeader.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);

			Line.ExchangeRate.Currency = "USD";
			Line.ExchangeRate.Rate = 0.5M;

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Type = AccTaxRate.Types.ServiceTax;
			Line.AL_AT = taxRate.PK;
			Line.AL_OSExTaxAmount = 123.00M;
			Line.AL_OSTaxAmount = 124.00M;
			Line.AL_OSAmount = 125.00M;
			Line.AL_OSGSTAmount = 126.00M;
			Line.AL_OSExtraTaxAmount = 128.00M;

			AssertEquals(246.00M, InvoiceLineWrapper.OSExTaxAmount);
			AssertEquals(248.00M, InvoiceLineWrapper.OSTaxAmount);
			AssertEquals(494.00M, InvoiceLineWrapper.OSAmount);
			AssertEquals(252.00M, InvoiceLineWrapper.OSGSTAmount);
			AssertEquals(0.00M, InvoiceLineWrapper.OSQSTAmount);
			AssertEquals(0.00M, InvoiceLineWrapper.OSEDUAmount);
			AssertEquals(-256.00M, InvoiceLineWrapper.OSRETAmount);
			AssertEquals(256.00M, InvoiceLineWrapper.OSExtraTaxAmount);

			Invoice.ExchangeRate.Currency = "USD";
			Line.AL_OSExTaxAmount = 123.00M;
			Line.AL_OSTaxAmount = 124.00M;
			Line.AL_OSAmount = 125.00M;
			Line.AL_OSGSTAmount = 126.00M;
			Line.AL_OSExtraTaxAmount = 128.00M;

			AssertEquals(123.00M, InvoiceLineWrapper.OSExTaxAmount);
			AssertEquals(124.00M, InvoiceLineWrapper.OSTaxAmount);
			AssertEquals(125.00M, InvoiceLineWrapper.OSAmount);
			AssertEquals(126.00M, InvoiceLineWrapper.OSGSTAmount);
			AssertEquals(0.00M, InvoiceLineWrapper.OSQSTAmount);
			AssertEquals(0.00M, InvoiceLineWrapper.OSEDUAmount);
			AssertEquals(-128.00M, InvoiceLineWrapper.OSRETAmount);
			AssertEquals(128.00M, InvoiceLineWrapper.OSExtraTaxAmount);

			taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
			AssertEquals(123.00M, InvoiceLineWrapper.OSExTaxAmount);
			AssertEquals(124.00M, InvoiceLineWrapper.OSTaxAmount);
			AssertEquals(125.00M, InvoiceLineWrapper.OSAmount);
			AssertEquals(126.00M, InvoiceLineWrapper.OSGSTAmount);
			AssertEquals(128.00M, InvoiceLineWrapper.OSQSTAmount);
			AssertEquals(0.00M, InvoiceLineWrapper.OSEDUAmount);
			AssertEquals(-128.00M, InvoiceLineWrapper.OSRETAmount);
			AssertEquals(0.00M, InvoiceLineWrapper.OSSPVAmount);
			AssertEquals(0.00M, InvoiceLineWrapper.LocalSPVAmount);

			AssertEquals(128.00M, InvoiceLineWrapper.OSExtraTaxAmount);

			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
			taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
			AssertEquals(123.00M, InvoiceLineWrapper.OSExTaxAmount);
			AssertEquals(124.00M, InvoiceLineWrapper.OSTaxAmount);
			AssertEquals(125.00M, InvoiceLineWrapper.OSAmount);
			AssertEquals(126.00M, InvoiceLineWrapper.OSGSTAmount);
			AssertEquals(0.00M, InvoiceLineWrapper.OSQSTAmount);
			AssertEquals(128.00M, InvoiceLineWrapper.OSEDUAmount);
			AssertEquals(-128.00M, InvoiceLineWrapper.OSRETAmount);
			AssertEquals(0.00M, InvoiceLineWrapper.OSSPVAmount);
			AssertEquals(128.00M, InvoiceLineWrapper.OSExtraTaxAmount);

			taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.Italy;
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRemittedByCustomer;
			taxRate.SetRateNumerator_ForTestOnly(22);
			taxRate.SetExtraRate_ForTestOnly(0, 1);
			AssertEquals(123.00M, InvoiceLineWrapper.OSExTaxAmount);
			AssertEquals(124.00M, InvoiceLineWrapper.OSTaxAmount);
			AssertEquals(125.00M, InvoiceLineWrapper.OSAmount);
			AssertEquals(126.00M, InvoiceLineWrapper.OSGSTAmount);
			AssertEquals(0.00M, InvoiceLineWrapper.OSQSTAmount);
			AssertEquals(0.00M, InvoiceLineWrapper.OSEDUAmount);
			AssertEquals(-128.00M, InvoiceLineWrapper.OSRETAmount);
			AssertEquals(128.00M, InvoiceLineWrapper.OSSPVAmount);
			AssertEquals(128.00M, InvoiceLineWrapper.OSExtraTaxAmount);

			Line.AL_LocalExtraTaxAmount = 256;
			AssertEquals(256.00M, InvoiceLineWrapper.LocalSPVAmount);
		}

		#endregion

		#region TestJobNumber

		public void TestJobNumber()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
			Line.AL_JH = Guid.Empty;
			AssertEquals("The JobNumber should be empty because there is no JobHeader", ZString.Empty, InvoiceLineWrapper.JobNumber);
			Line.AL_JH = job.PK;
			AssertEquals("The JobNumber should now has value", Line.Job.JH_JobNum, InvoiceLineWrapper.JobNumber);
		}

		#endregion

		#region TestOSSBCAndKKCAmount

		public void TestTotalOSSBCAndKKCAmount()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.India);

			var taxRate1 = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate1.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
			taxRate1.AT_Type = AccTaxRate.Types.Rated;
			taxRate1.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
			taxRate1.SetRateNumerator_ForTestOnly(5);
			taxRate1.SetExtraRate_ForTestOnly(1, 1);

			Line.AL_AT = taxRate1.PK;
			Line.AL_OSExTaxAmount = 1000m;
			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);

			AssertEquals(5m, InvoiceLineWrapper.OSSBCAmount);
			AssertEquals(5m, InvoiceLineWrapper.OSKKCAmount);

			var taxRate2 = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate2.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
			taxRate2.AT_Type = AccTaxRate.Types.Rated;
			taxRate2.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
			taxRate2.SetRateNumerator_ForTestOnly(5);
			taxRate2.SetExtraRate_ForTestOnly(5, 10);

			Line.AL_AT = taxRate2.PK;
			Line.AL_OSExTaxAmount = 2000m;
			Line.AL_LineType = TransactionLineTypes.Revenue;
			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);

			AssertEquals(10m, InvoiceLineWrapper.OSSBCAmount);
			AssertEquals(0m, InvoiceLineWrapper.OSKKCAmount);

			Line.AL_LineType = TransactionLineTypes.Cost;
			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);

			AssertEquals(0m, InvoiceLineWrapper.OSSBCAmount);
			AssertEquals(10m, InvoiceLineWrapper.OSKKCAmount);
		}

		public void TestTotalOSSBCAndKKCAmount_SER()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.India);

			var taxRate1 = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate1.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
			taxRate1.AT_Type = AccTaxRate.Types.ServiceTax;
			taxRate1.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
			taxRate1.SetRateNumerator_ForTestOnly(5);
			taxRate1.SetExtraRate_ForTestOnly(1, 1);

			Line.AL_AT = taxRate1.PK;
			Line.AL_OSExTaxAmount = 1000m;
			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);

			AssertEquals(5m, InvoiceLineWrapper.OSSBCAmount);
			AssertEquals(5m, InvoiceLineWrapper.OSKKCAmount);

			var taxRate2 = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate2.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
			taxRate2.AT_Type = AccTaxRate.Types.ServiceTax;
			taxRate2.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
			taxRate2.SetRateNumerator_ForTestOnly(5);
			taxRate2.SetExtraRate_ForTestOnly(5, 10);

			Line.AL_AT = taxRate2.PK;
			Line.AL_OSExTaxAmount = 2000m;
			Line.AL_LineType = TransactionLineTypes.Revenue;
			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);

			AssertEquals(10m, InvoiceLineWrapper.OSSBCAmount);
			AssertEquals(0m, InvoiceLineWrapper.OSKKCAmount);

			Line.AL_LineType = TransactionLineTypes.Cost;
			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);

			AssertEquals(0m, InvoiceLineWrapper.OSSBCAmount);
			AssertEquals(10m, InvoiceLineWrapper.OSKKCAmount);
		}

		#endregion

		#region TestLocalAmountAndLocalExTaxAmount

		public void TestLocalAmountAndLocalExTaxAmount()
		{
			InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);

			Line.AL_LineAmount = 123.00M;
			Line.AL_GSTVAT = 12.30M;

			AssertEquals(135.30M, InvoiceLineWrapper.LocalAmount);
			AssertEquals(123.00M, InvoiceLineWrapper.LocalExTaxAmount);
		}

		#endregion

		#region TestCreditNoteLineAmountsWithAmountMultiplier

		public void TestCreditNoteLineAmountsWithAmountMultiplier()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			using (TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
			{
				Invoice.AH_TransactionType = TransactionTypes.CreditNote;
				var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_Type = AccTaxRate.Types.Rated;
				taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
				taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
				taxRate.SetRate_ForTestOnly(3333, 100);
				Factory.Save();
				InvoiceWrapper = DocARInvoice.New(Invoice, Factory);

				AssertEquals("Credit note amount", 0M, InvoiceWrapper.InvoiceAmountWithGST);
				AssertEquals("Credit note balance", 0M, InvoiceWrapper.Balance);

				var crdLine = (InvoicingLineBase)Invoice.Lines.AddNew();
				Invoice.AH_InvoiceDate = ZDateTime.Now;
				Invoice.AH_PostDate = ZDateTime.Now;
				Invoice.AH_RX_NKTransactionCurrency = "GBP";
				Invoice.AH_ExchangeRate = 2M;
				crdLine.AL_AT = taxRate.PK;
				crdLine.AL_OSTaxAmount = 50M;
				crdLine.AL_OSExTaxAmount = 150M;
				crdLine.AL_OSExtraTaxAmount = 9M;
				Invoice.AH_OSTaxAmount = 50M;
				Invoice.AH_OSExTaxAmount = 150M;
				Invoice.AH_LocalOutstandingAmount = 100M;
				Invoice.AH_DueDate = ZDateTime.Today;
				Invoice.AH_FullyPaidDate = ZDateTime.Empty;

				InvoiceLineWrapper = DocARInvoiceLine.New(crdLine, Factory);

				AssertEquals("Pre-condition: RegistryItem is False by default", false, AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.Value);

				AssertEquals("GSTVAT", 25M, InvoiceLineWrapper.GSTVAT);
				AssertEquals("OSExTaxAmount", 150M, InvoiceLineWrapper.OSExTaxAmount);
				AssertEquals("LocalExTaxAmount", 75M, InvoiceLineWrapper.LocalExTaxAmount);
				AssertEquals("OSTaxAmount", 50M, InvoiceLineWrapper.OSTaxAmount);
				AssertEquals("OSGSTAmount", 50M, InvoiceLineWrapper.OSGSTAmount);
				AssertEquals("OSQSTAmount", 0M, InvoiceLineWrapper.OSQSTAmount);
				AssertEquals("OSEDUAmount", 9M, InvoiceLineWrapper.OSEDUAmount);
				AssertEquals("OSRETAmount", -9M, InvoiceLineWrapper.OSRETAmount);

				AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				if (Invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					AssertEquals("GSTVAT", -25M, InvoiceLineWrapper.GSTVAT);
					AssertEquals("OSExTaxAmount", -150M, InvoiceLineWrapper.OSExTaxAmount);
					AssertEquals("LocalExTaxAmount", -75M, InvoiceLineWrapper.LocalExTaxAmount);
					AssertEquals("OSTaxAmount", -50M, InvoiceLineWrapper.OSTaxAmount);
					AssertEquals("OSGSTAmount", -50M, InvoiceLineWrapper.OSGSTAmount);
					AssertEquals("OSQSTAmount", 0M, InvoiceLineWrapper.OSQSTAmount);
					AssertEquals("OSEDUAmount", -9M, InvoiceLineWrapper.OSEDUAmount);
					AssertEquals("OSRETAmount", 9M, InvoiceLineWrapper.OSRETAmount);
				}
				else
				{
					AssertEquals("GSTVAT", 25M, InvoiceLineWrapper.GSTVAT);
					AssertEquals("OSExTaxAmount", 150M, InvoiceLineWrapper.OSExTaxAmount);
					AssertEquals("LocalExTaxAmount", 75M, InvoiceLineWrapper.LocalExTaxAmount);
					AssertEquals("OSTaxAmount", 50M, InvoiceLineWrapper.OSTaxAmount);
					AssertEquals("OSGSTAmount", 50M, InvoiceLineWrapper.OSGSTAmount);
					AssertEquals("OSQSTAmount", 0M, InvoiceLineWrapper.OSQSTAmount);
					AssertEquals("OSEDUAmount", 9M, InvoiceLineWrapper.OSEDUAmount);
					AssertEquals("OSRETAmount", -9M, InvoiceLineWrapper.OSRETAmount);
				}
			}
		}

		public void TestCreditNoteLineAmountsWithAmountMultiplier_SER()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			using (TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
			{
				Invoice.AH_TransactionType = TransactionTypes.CreditNote;
				var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_Type = AccTaxRate.Types.ServiceTax;
				taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
				taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
				taxRate.SetRate_ForTestOnly(3333, 100);
				Factory.Save();
				InvoiceWrapper = DocARInvoice.New(Invoice, Factory);

				AssertEquals("Credit note amount", 0M, InvoiceWrapper.InvoiceAmountWithGST);
				AssertEquals("Credit note balance", 0M, InvoiceWrapper.Balance);

				var crdLine = (InvoicingLineBase)Invoice.Lines.AddNew();
				Invoice.AH_InvoiceDate = ZDateTime.Now;
				Invoice.AH_PostDate = ZDateTime.Now;
				Invoice.AH_RX_NKTransactionCurrency = "GBP";
				Invoice.AH_ExchangeRate = 2M;
				crdLine.AL_AT = taxRate.PK;
				crdLine.AL_OSTaxAmount = 50M;
				crdLine.AL_OSExTaxAmount = 150M;
				crdLine.AL_OSExtraTaxAmount = 9M;
				Invoice.AH_OSTaxAmount = 50M;
				Invoice.AH_OSExTaxAmount = 150M;
				Invoice.AH_LocalOutstandingAmount = 100M;
				Invoice.AH_DueDate = ZDateTime.Today;
				Invoice.AH_FullyPaidDate = ZDateTime.Empty;

				InvoiceLineWrapper = DocARInvoiceLine.New(crdLine, Factory);

				AssertEquals("Pre-condition: RegistryItem is False by default", false, AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.Value);

				AssertEquals("GSTVAT", 25M, InvoiceLineWrapper.GSTVAT);
				AssertEquals("OSExTaxAmount", 150M, InvoiceLineWrapper.OSExTaxAmount);
				AssertEquals("LocalExTaxAmount", 75M, InvoiceLineWrapper.LocalExTaxAmount);
				AssertEquals("OSTaxAmount", 50M, InvoiceLineWrapper.OSTaxAmount);
				AssertEquals("OSGSTAmount", 50M, InvoiceLineWrapper.OSGSTAmount);
				AssertEquals("OSQSTAmount", 0M, InvoiceLineWrapper.OSQSTAmount);
				AssertEquals("OSEDUAmount", 9M, InvoiceLineWrapper.OSEDUAmount);
				AssertEquals("OSRETAmount", -9M, InvoiceLineWrapper.OSRETAmount);

				AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				if (Invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					AssertEquals("GSTVAT", -25M, InvoiceLineWrapper.GSTVAT);
					AssertEquals("OSExTaxAmount", -150M, InvoiceLineWrapper.OSExTaxAmount);
					AssertEquals("LocalExTaxAmount", -75M, InvoiceLineWrapper.LocalExTaxAmount);
					AssertEquals("OSTaxAmount", -50M, InvoiceLineWrapper.OSTaxAmount);
					AssertEquals("OSGSTAmount", -50M, InvoiceLineWrapper.OSGSTAmount);
					AssertEquals("OSQSTAmount", 0M, InvoiceLineWrapper.OSQSTAmount);
					AssertEquals("OSEDUAmount", -9M, InvoiceLineWrapper.OSEDUAmount);
					AssertEquals("OSRETAmount", 9M, InvoiceLineWrapper.OSRETAmount);
				}
				else
				{
					AssertEquals("GSTVAT", 25M, InvoiceLineWrapper.GSTVAT);
					AssertEquals("OSExTaxAmount", 150M, InvoiceLineWrapper.OSExTaxAmount);
					AssertEquals("LocalExTaxAmount", 75M, InvoiceLineWrapper.LocalExTaxAmount);
					AssertEquals("OSTaxAmount", 50M, InvoiceLineWrapper.OSTaxAmount);
					AssertEquals("OSGSTAmount", 50M, InvoiceLineWrapper.OSGSTAmount);
					AssertEquals("OSQSTAmount", 0M, InvoiceLineWrapper.OSQSTAmount);
					AssertEquals("OSEDUAmount", 9M, InvoiceLineWrapper.OSEDUAmount);
					AssertEquals("OSRETAmount", -9M, InvoiceLineWrapper.OSRETAmount);
				}
			}
		}

		public void TestCreditNoteLineOSSPVAmountsWithShowARCreditNoteAmountsWithOppositeSignRegistry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				using (AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					var creditNote = Factory.New<ARCreditNote>();
					var creditNoteLine = (InvoicingLineBase)creditNote.Lines.AddNew();

					GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "VND";
					creditNote.AH_RX_NKTransactionCurrency = "USD";
					creditNote.AH_ExchangeRate = 0.5;

					creditNoteLine.AL_AT = TestObjectCreator.VATSPV.PK;
					creditNoteLine.AL_OSExTaxAmount = 45.47M;
					AssertEquals("Line should have tax amount", 10m, creditNoteLine.AL_OSGSTAmount);
					var creditNoteLineWrapper = DocARInvoiceLine.New(creditNoteLine, Factory);
					var taxRate = FormatNumberUtil.FormatRateWithCurrentCompanysCulture(creditNoteLine.TaxRate.GetRateRaw_ForTestOnly());

					using (AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					{
						AssertEquals("Subtotal", -10.00m, creditNoteLineWrapper.OSSPVAmount);
						AssertEquals("Local SPV", -20.00m, creditNoteLineWrapper.LocalSPVAmount);
						AssertEquals("OS Tax Display Extra Amount", "10,00", creditNoteLineWrapper.OSTaxDisplayExtraAmount);
						AssertEquals("OS Tax Display No Asterisks", taxRate + "=10,00", creditNoteLineWrapper.OSTaxDisplayNoAsterisks);
						AssertEquals("OS Tax Display No Asterisks With Registry Rule", taxRate + "=10,00", creditNoteLineWrapper.OSTaxDisplayNoAsterisksWithRegistryRule);
					}

					using (AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					{
						AssertEquals("Subtotal", 10.00m, creditNoteLineWrapper.OSSPVAmount);
						AssertEquals("Local SPV", 20.00m, creditNoteLineWrapper.LocalSPVAmount);
						AssertEquals("OS Tax Display Extra Amount", "-10,00", creditNoteLineWrapper.OSTaxDisplayExtraAmount.Replace(" ", string.Empty));
						AssertEquals("OS Tax Display No Asterisks", taxRate + "=-10,00", creditNoteLineWrapper.OSTaxDisplayNoAsterisks.Replace(" ", string.Empty));
						AssertEquals("OS Tax Display No Asterisks With Registry Rule", taxRate + "=-10,00", creditNoteLineWrapper.OSTaxDisplayNoAsterisksWithRegistryRule.Replace(" ", string.Empty));
					}
				}
			}
		}

		#endregion

		#region TestPreventGrouping

		public void TestPreventGrouping()
		{
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			InvoiceLineWrapper = InvoiceWrapper.Lines[0] as DocARInvoiceLine;
			AssertNotNull("Line should be wrapped", InvoiceLineWrapper);
			Line.AL_PreventInvoicePrintGrouping = false;
			AssertEquals(false, InvoiceLineWrapper.PreventGrouping);

			AssertEquals("Pre-condition: Should not be a Periodic Invoice", false, InvoiceWrapper.IsPeriodicInvoice);
			Line.AL_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
			Line.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.NotGrouped;
			InvoiceWrapper = RecreateTestingDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			InvoiceLineWrapper = InvoiceWrapper.Lines[0] as DocARInvoiceLine;
			AssertEquals(true, InvoiceLineWrapper.PreventGrouping);

			Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice_Batching;
			InvoiceWrapper = RecreateTestingDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			InvoiceLineWrapper = InvoiceWrapper.Lines[0] as DocARInvoiceLine;
			Assert("Should be Periodic Invoice", InvoiceWrapper.IsPeriodicInvoice);
			AssertEquals(false, InvoiceLineWrapper.PreventGrouping);

			Line.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.NonJobRelated;
			InvoiceWrapper = RecreateTestingDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			InvoiceLineWrapper = InvoiceWrapper.Lines[0] as DocARInvoiceLine;
			AssertEquals(false, InvoiceLineWrapper.PreventGrouping);

			Line.AL_PreventInvoicePrintGrouping = true;
			InvoiceWrapper = RecreateTestingDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			InvoiceLineWrapper = InvoiceWrapper.Lines[0] as DocARInvoiceLine;
			AssertEquals(true, InvoiceLineWrapper.PreventGrouping);

			Assert("Still being Periodic Invoice", InvoiceWrapper.IsPeriodicInvoice);
			Line.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.NotGrouped;
			InvoiceWrapper = RecreateTestingDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			InvoiceLineWrapper = InvoiceWrapper.Lines[0] as DocARInvoiceLine;
			AssertEquals(true, InvoiceLineWrapper.PreventGrouping);

			Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			InvoiceWrapper = RecreateTestingDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			InvoiceLineWrapper = InvoiceWrapper.Lines[0] as DocARInvoiceLine;
			AssertEquals("Should not be a Periodic Invoice anymore", false, InvoiceWrapper.IsPeriodicInvoice);
			AssertEquals(true, InvoiceLineWrapper.PreventGrouping);

			Line.AL_AC = ZGuid.Empty;
			InvoiceWrapper = RecreateTestingDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			InvoiceLineWrapper = InvoiceWrapper.Lines[0] as DocARInvoiceLine;
			AssertEquals(true, InvoiceLineWrapper.PreventGrouping);
		}

		RefExchangeRate setupExchangeRateForGovtComplianceDocumentExchangeRate(BusinessObjectFactory factory)
		{
			RefExchangeRate exRate = factory.New<RefExchangeRate>();
			exRate.RE_ExpiryDate = new ZDateTime(2011, 12, 12);
			exRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exRate.RE_GC = GlbCompany.CurrentCompany.PK;
			exRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.Indonesia;
			exRate.RE_SellRate = 9030m;
			exRate.RE_StartDate = new ZDateTime(2011, 12, 01);

			factory.Save();

			return exRate;
		}

		#endregion

		#region TestGovtComplianceDocumentExchangeRate

		[TestDate(2011, 12, 12)]
		public void TestGovtComplianceDocumentExchangeRate()
		{
			AccountingConfigurationRegistry.Instance.AccGovtComplianceDocumentExchangeRateDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "PST");
			AccountingConfigurationRegistry.Instance.AccGovtComplianceDocumentExchangeRateType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "SEL");

			ZString originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			ZString originalCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			var rateFactory = new BusinessObjectFactory();

			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Indonesia);
				GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Indonesia;

				InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
				ExchangeRateReader.GetReaderInstance().ClearCache();
				AssertEquals("GovtComplianceDocumentExchangeRate", 0m, InvoiceLineWrapper.GovtComplianceDocumentExchangeRate);

				RefExchangeRate exRate = setupExchangeRateForGovtComplianceDocumentExchangeRate(rateFactory);

				Invoice.AH_PostDate = new ZDateTime(2011, 12, 12);
				Invoice.AH_InvoiceDate = new ZDateTime(2011, 12, 12);
				ExchangeRateReader.GetReaderInstance().ClearCache();
				AssertEquals("GovtComplianceDocumentExchangeRate", 9030m, InvoiceLineWrapper.GovtComplianceDocumentExchangeRate);

				Invoice.AH_PostDate = new ZDateTime(2011, 12, 13);
				InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
				ExchangeRateReader.GetReaderInstance().ClearCache();
				AssertEquals("GovtComplianceDocumentExchangeRate", 9030m, InvoiceLineWrapper.GovtComplianceDocumentExchangeRate);

				AccountingConfigurationRegistry.Instance.AccGovtComplianceDocumentExchangeRateDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "INV");
				ExchangeRateReader.GetReaderInstance().ClearCache();
				AssertEquals("GovtComplianceDocumentExchangeRate", 9030m, InvoiceLineWrapper.GovtComplianceDocumentExchangeRate);

				AccountingConfigurationRegistry.Instance.AccGovtComplianceDocumentExchangeRateType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "BUY");
				InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
				ExchangeRateReader.GetReaderInstance().ClearCache();
				AssertEquals("GovtComplianceDocumentExchangeRate", 0m, InvoiceLineWrapper.GovtComplianceDocumentExchangeRate);

				exRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
				rateFactory.Save();

				InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
				ExchangeRateReader.GetReaderInstance().ClearCache();
				AssertEquals("GovtComplianceDocumentExchangeRate", 9030m, InvoiceLineWrapper.GovtComplianceDocumentExchangeRate);

				AccountingConfigurationRegistry.Instance.AccGovtComplianceDocumentExchangeRateDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "PST");
				InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
				ExchangeRateReader.GetReaderInstance().ClearCache();
				AssertEquals("GovtComplianceDocumentExchangeRate", 9030m, InvoiceLineWrapper.GovtComplianceDocumentExchangeRate);

				Invoice.AH_PostDate = new ZDateTime(2011, 12, 12);
				InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);
				ExchangeRateReader.GetReaderInstance().ClearCache();
				AssertEquals("GovtComplianceDocumentExchangeRate", 9030m, InvoiceLineWrapper.GovtComplianceDocumentExchangeRate);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountry);
				GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = originalCurrency;
			}
		}

		#endregion

		#region TestCalculatedLocalExTaxAmount

		[TestDate(2011, 12, 12)]
		public void TestCalculatedLocalExTaxAmount()
		{
			AccountingConfigurationRegistry.Instance.AccGovtComplianceDocumentExchangeRateDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "PST");
			AccountingConfigurationRegistry.Instance.AccGovtComplianceDocumentExchangeRateType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "SEL");

			ZString originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Indonesia);

				InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);

				RefExchangeRate exRate = Factory.New<RefExchangeRate>();
				exRate.RE_ExpiryDate = new ZDateTime(2011, 12, 12);
				exRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
				exRate.RE_GC = GlbCompany.CurrentCompany.PK;
				exRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.Indonesia;
				exRate.RE_SellRate = 9030m;
				exRate.RE_StartDate = new ZDateTime(2011, 12, 01);
				Factory.Save();

				Invoice.AH_PostDate = new ZDateTime(2011, 12, 12);
				Invoice.AH_InvoiceDate = new ZDateTime(2011, 12, 12);
				ExchangeRateReader.GetReaderInstance().ClearCache();
				AssertEquals("GovtComplianceDocumentExchangeRate", 9030m, InvoiceLineWrapper.GovtComplianceDocumentExchangeRate);

				Line.AL_LocalExTaxAmount = 100m;
				AssertEquals("CalculatedLocalExTaxAmount", 100m, InvoiceLineWrapper.CalculatedLocalExTaxAmount);

				Invoice.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				Line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				AssertEquals("CalculatedLocalExTaxAmount", 100m, InvoiceLineWrapper.CalculatedLocalExTaxAmount);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Indonesia);
				GlbCompany.CurrentCompany.SetCurrency(Core.Constants.CurrencyCodes.UnitedStates);
				AssertEquals("CalculatedLocalExTaxAmount", 903000m, InvoiceLineWrapper.CalculatedLocalExTaxAmount);

				Invoice.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Indonesia;
				Line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Indonesia;
				Invoice.AH_ExchangeRate = 2m;
				Line.AL_OSExTaxAmount = 200m;
				AssertEquals("Line.AL_LocalExTaxAmount", 100m, Line.AL_LocalExTaxAmount);
				AssertEquals("Line.AL_OSExTaxAmount", 200m, Line.AL_OSExTaxAmount);
				AssertEquals("CalculatedLocalExTaxAmount", 200m, InvoiceLineWrapper.CalculatedLocalExTaxAmount);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountry);
			}
		}

		#endregion

		#region TestCalculatedLocalTaxAmount

		[TestDate(2011, 12, 12)]
		public void TestCalculatedLocalTaxAmount()
		{
			AccountingConfigurationRegistry.Instance.AccGovtComplianceDocumentExchangeRateDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "PST");
			AccountingConfigurationRegistry.Instance.AccGovtComplianceDocumentExchangeRateType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "SEL");

			ZString originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Indonesia);

				InvoiceLineWrapper = DocARInvoiceLine.New(Line, Factory);

				RefExchangeRate exRate = Factory.New<RefExchangeRate>();
				exRate.RE_ExpiryDate = new ZDateTime(2011, 12, 12);
				exRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
				exRate.RE_GC = GlbCompany.CurrentCompany.PK;
				exRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.Indonesia;
				exRate.RE_SellRate = 9030m;
				exRate.RE_StartDate = new ZDateTime(2011, 12, 01);
				Factory.Save();

				Invoice.AH_PostDate = new ZDateTime(2011, 12, 12);
				Invoice.AH_InvoiceDate = new ZDateTime(2011, 12, 12);
				ExchangeRateReader.GetReaderInstance().ClearCache();

				AssertEquals("GovtComplianceDocumentExchangeRate", 9030m, InvoiceLineWrapper.GovtComplianceDocumentExchangeRate);

				Line.AL_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
				Line.AL_LocalTaxAmount = 100m;
				AssertEquals("CalculatedLocalTaxAmount", 100m, InvoiceLineWrapper.CalculatedLocalTaxAmount);

				Invoice.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				Line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				Line.AL_LocalTaxAmount = 100m;
				AssertEquals("CalculatedLocalTaxAmount", 100m, InvoiceLineWrapper.CalculatedLocalTaxAmount);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Indonesia);
				GlbCompany.CurrentCompany.SetCurrency(Core.Constants.CurrencyCodes.UnitedStates);
				AssertEquals("CalculatedLocalTaxAmount", 903000m, InvoiceLineWrapper.CalculatedLocalTaxAmount);

				Invoice.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Indonesia;
				Line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Indonesia;
				Invoice.AH_ExchangeRate = 2m;
				Line.AL_OSTaxAmount = 200m;
				AssertEquals("Line.AL_LocalTaxAmount", 100m, Line.AL_LocalTaxAmount);
				AssertEquals("Line.AL_OSTaxAmount", 200m, Line.AL_OSTaxAmount);
				AssertEquals("CalculatedLocalTaxAmount", 200m, InvoiceLineWrapper.CalculatedLocalTaxAmount);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountry);
			}
		}

		#endregion

		#region TestGlobalChargeCodes0

		public void TestGlobalChargeCodes0()
		{
			TestGlobalChargeCodesCommon(0);
		}

		#endregion

		#region TestGlobalChargeCodes1

		public void TestGlobalChargeCodes1()
		{
			TestGlobalChargeCodesCommon(1);
		}

		void TestGlobalChargeCodesCommon(int numberGlobalChargeCodesToTest)
		{
			Assert(numberGlobalChargeCodesToTest == 0 || numberGlobalChargeCodesToTest == 1);

			// Arrange
			var chargeCode = TestObjectCreator.CreateChargeCode("CC");
			var orgHeader = TestObjectCreator.CreateOrgHeader("ORG1", false, true);

			InvoicingBase invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1);
			invoice.AH_OH = orgHeader.PK;

			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AC = chargeCode.PK;

			InvoiceLineWrapper = DocARInvoiceLine.New(line, Factory);

			if (numberGlobalChargeCodesToTest > 0)
			{
				var globalChargeCode = TestObjectCreator.CreateGlobalChargeCodeMapWithPivot("MyCode", "DescriptionOfChargeCode", orgHeader.PK, chargeCode.PK, LedgerTypes.AccountsReceivable);
			}

			Factory.Save();

			// Act
			var arGlobalChargeCodes = InvoiceLineWrapper.ARGlobalChargeCodes;

			// Assert
			AssertNotNull(arGlobalChargeCodes);
			AssertEquals(numberGlobalChargeCodesToTest, arGlobalChargeCodes.Count);

			if (numberGlobalChargeCodesToTest > 0)
			{
				AssertEquals("MyCode", arGlobalChargeCodes[0].GlobalCode);
				AssertEquals("DescriptionOfChargeCode", arGlobalChargeCodes[0].Description);
			}
		}

		#endregion

		#region Test Government Reporting

		public void TestGovernmentReporting()
		{
			foreach (var registryValue in new bool[] { true, false })
			{
				AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue);

				var goodsChargeCode = TestObjectCreator.CreateChargeCode("GOODS");
				goodsChargeCode.AC_GovtChargeCode = "TAX1";
				goodsChargeCode.AC_GoodsServiceType = GoodServiceTypes.Codes.GDS;

				var serviceChargeCode = TestObjectCreator.CreateChargeCode("SERVICE");
				serviceChargeCode.AC_GovtChargeCode = "TAX2";
				serviceChargeCode.AC_GoodsServiceType = GoodServiceTypes.Codes.SRV;

				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1M);
				var line = (InvoicingLineBase)invoice.Lines.AddNew();
				line.AL_AC = goodsChargeCode.PK;

				var wrapper = DocARInvoiceLine.New(line, Factory);
				AssertGovernmentReportingWrappers(registryValue, wrapper, "TAX1", GoodServiceTypes.Codes.GDS);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
				{
					AssertGovernmentReportingWrappers(registryValue, wrapper, "TAX1", "HSN");
				}

				line.AL_AC = serviceChargeCode.PK;
				wrapper = DocARInvoiceLine.New(line, Factory);
				AssertGovernmentReportingWrappers(registryValue, wrapper, "TAX2", GoodServiceTypes.Codes.SRV);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
				{
					AssertGovernmentReportingWrappers(registryValue, wrapper, "TAX2", "SAC");
				}
			}
		}

		static void AssertGovernmentReportingWrappers(bool registryValue, DocARInvoiceLine wrapper, string expectedReportingCode, string expectedReportingHeader)
		{
			if (registryValue)
			{
				AssertEquals(expectedReportingCode, wrapper.GovernmentReportingCode);
				AssertEquals(expectedReportingHeader, wrapper.GovernmentReportingCodeHeading);
			}
			else
			{
				AssertEquals(ZString.Empty, wrapper.GovernmentReportingCode);
				AssertEquals(ZString.Empty, wrapper.GovernmentReportingCodeHeading);
			}
		}

		#endregion

		#region ISortableDocLine

		public void TestOrgLevelSortOrder()
		{
			Charge charge = Factory.NewWithValidTestData<Charge>();
			OrgHeader org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			AssertNotNull("Org", org);
			AssertNotNull("Force creating CompanyData", org.CompanyData);

			Factory.Save();

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoiceLine line = (ARInvoiceLine)invoice.Lines.AddNew();
			line.AL_AC = charge.JR_AC;

			charge.JR_AL_ARLine = line.PK;
			DocARInvoiceLine wrapper = DocARInvoiceLine.New(line, Factory);
			AssertNotNull("Charge", wrapper.Charge);

			AccClientInvoiceOrder invoiceOrder = org.InvoiceOrders.AddNew();
			invoiceOrder.AI_AC = charge.JR_AC;
			invoiceOrder.AI_PrintOrder = 3;
			Assert("AI_InvoiceType.IsEmpty", invoiceOrder.AI_InvoiceType.IsEmpty);

			line.AL_OH = ZGuid.Empty;
			AssertEquals("OrgLevelSortOrder", 0, ((ISortableDocLine)wrapper).OrgLevelSortOrder);

			line.AL_OH = org.PK;
			AssertEquals("OrgLevelSortOrder", 3, ((ISortableDocLine)wrapper).OrgLevelSortOrder);

			invoice.AH_TransactionCategory = "BLA";
			AssertEquals("AOrgLevelSortOrder", 3, ((ISortableDocLine)wrapper).OrgLevelSortOrder);

			invoiceOrder.AI_InvoiceType = "BLA";
			AssertEquals("OrgLevelSortOrder", 3, ((ISortableDocLine)wrapper).OrgLevelSortOrder);

			invoice.AH_TransactionCategory = "BEE";
			AssertEquals("OrgLevelSortOrder", 0, ((ISortableDocLine)wrapper).OrgLevelSortOrder);

			invoiceOrder.AI_InvoiceType = "ALL";
			AssertEquals("OrgLevelSortOrder", 3, ((ISortableDocLine)wrapper).OrgLevelSortOrder);
		}

		public void TestChargePrintSeqSortOrder()
		{
			Charge charge = Factory.NewWithValidTestData<Charge>();

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoiceLine line = (ARInvoiceLine)invoice.Lines.AddNew();
			charge.ChargeCode.AC_PrintSequence = 10;
			DocARInvoiceLine wrapper = DocARInvoiceLine.New(line, Factory);

			AssertNull("Charge", wrapper.Charge);
			AssertEquals("ChargePrintSeqSortOrder", 0, ((ISortableDocLine)wrapper).ChargePrintSeqSortOrder);

			charge.JR_AL_ARLine = line.PK;
			line.AL_AC = charge.JR_AC;
			wrapper = RecreateTestingDocWrapper(line, (lineToWrap, factory) => DocARInvoiceLine.New(lineToWrap, Factory));

			AssertNotNull("Charge", wrapper.Charge);

			AssertEquals("ChargePrintSeqSortOrder", 10, ((ISortableDocLine)wrapper).ChargePrintSeqSortOrder);
		}

		public void TestUserEnteredSortOrder()
		{
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoiceLine line = (ARInvoiceLine)invoice.Lines.AddNew();
			DocARInvoiceLine wrapper = DocARInvoiceLine.New(line, Factory);

			AssertEquals("AL_Sequence", (short)1, line.AL_Sequence);
			AssertEquals("UserEnteredSortOrder", 1, ((ISortableDocLine)wrapper).UserEnteredSortOrder);

			line.AL_Sequence = 5;
			AssertEquals("UserEnteredSortOrder", 5, ((ISortableDocLine)wrapper).UserEnteredSortOrder);
		}

		public void TestAlphabeticalSortOrder()
		{
			Charge charge = Factory.NewWithValidTestData<Charge>();
			AccGLHeader account = Factory.NewWithValidTestData<AccGLHeader>();

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoiceLine line = (ARInvoiceLine)invoice.Lines.AddNew();
			DocARInvoiceLine wrapper = DocARInvoiceLine.New(line, Factory);

			AssertNull("Charge", wrapper.Charge);
			AssertNull("GLAccount", wrapper.GLAccount);
			AssertEquals("AlphabeticalSortOrder", string.Empty, ((ISortableDocLine)wrapper).AlphabeticalSortOrder);

			charge.JR_AL_ARLine = line.PK;
			line.AL_AG = account.PK;
			wrapper = RecreateTestingDocWrapper(line, (lineToWrap, factory) => DocARInvoiceLine.New(lineToWrap, Factory));
			AssertEquals("AlphabeticalSortOrder", account.AG_AccountNum, ((ISortableDocLine)wrapper).AlphabeticalSortOrder);

			line.AL_AG = ZGuid.Empty;
			line.AL_AC = charge.JR_AC;
			wrapper = RecreateTestingDocWrapper(line, (lineToWrap, factory) => DocARInvoiceLine.New(lineToWrap, Factory));
			AssertNotNull("Charge", wrapper.Charge);
			AssertEquals("AlphabeticalSortOrder", charge.ChargeCode.AC_Code, ((ISortableDocLine)wrapper).AlphabeticalSortOrder);
		}

		#endregion

		public void TestIsRollUpLine()
		{
			foreach (var data in new[] { new { RegValue = false, ExpectedQuantity = "" }, new { RegValue = true, ExpectedQuantity = "1" } })
			{
				using (AccountingConfigurationRegistry.Instance.PrintQuanityInInvoiceDocument.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, data.RegValue))
				{
					ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
					ARInvoiceLine line = (ARInvoiceLine)invoice.Lines.AddNew();
					line.AL_Desc = "Test Charge";
					DocARInvoiceLine wrapper = DocARInvoiceLine.New(line, Factory);
					AssertEquals(ZBool.False, wrapper.IsRollUpLine);
					AssertEquals(data.ExpectedQuantity, wrapper.Quantity);
				}
			}
		}

		#region Implementation

		ARInvoice Invoice;
		ARInvoiceLine Line;
		DocARInvoiceLine InvoiceLineWrapper;
		DocARInvoice InvoiceWrapper;

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			Invoice = Factory.New<ARInvoice>();
			Line = (ARInvoiceLine)Invoice.Lines.AddNew();
			Line.AL_AG = TestObjectCreator.GLHeader1.PK;
			base.SetUp();
		}

		JobHeader GetInvoiceJob(IJobInvoicingPlugIn plugIn, InvoicingBase invoice)
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(plugIn.TableName);
			job.JH_ParentID = plugIn.PK;
			invoice.AH_JH = job.PK;
			return job;
		}

		#region Helper

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}
		TransportBookingConsignmentTestHelper helper;

		#endregion

		#endregion

		#region FPOS

		public void TestARInvoiceLineFPOS()
		{
			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			var line = (ARInvoiceLine)arInvoice.Lines.AddNew();
			var arDocLine = DocARInvoiceLine.New(line, Factory);
			AssertPlaceOfSupplyProperties(line, arDocLine);
		}

		public void TestAPInvoiceLineFPOS()
		{
			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			var line = (APInvoiceLine)apInvoice.Lines.AddNew();
			var apDocLine = DocARInvoiceLine.New(line, Factory);
			AssertPlaceOfSupplyProperties(line, apDocLine);
		}

		void AssertPlaceOfSupplyProperties(TransactionLine line, DocARInvoiceLine lineWrapper)
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				line.AL_PlaceOfSupply = ZString.Empty;
				line.AL_PlaceOfSupplyType = ZString.Empty;
				AssertEquals(ZString.Empty, lineWrapper.FixedPlaceOfSupplyLabel);
				AssertEquals(ZString.Empty, lineWrapper.FixedPlaceOfSupply);

				line.AL_PlaceOfSupply = "AP";
				line.AL_PlaceOfSupplyType = PlaceOfSupplyTypes.State.Code;
				AssertEquals("State of Supply", lineWrapper.FixedPlaceOfSupplyLabel);
				AssertEquals("Andhra Pradesh", lineWrapper.FixedPlaceOfSupply);

				line.AL_PlaceOfSupply = PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry;
				line.AL_PlaceOfSupplyType = PlaceOfSupplyTypes.PredefinedRule.Code;
				AssertEquals("Place of Supply", lineWrapper.FixedPlaceOfSupplyLabel);
				AssertEquals("Foreign Country/Region", lineWrapper.FixedPlaceOfSupply);

				line.AL_PlaceOfSupply = ZString.Empty;
				line.AL_PlaceOfSupplyType = ZString.Empty;
				Assert(lineWrapper.FixedPlaceOfSupply.IsEmpty);

				line.AL_PlaceOfSupply = PlaceOfSupplyListProvider.Codes.OtherTerritories;
				line.AL_PlaceOfSupplyType = PlaceOfSupplyTypes.PredefinedRule.Code;
				AssertEquals("Place of Supply", lineWrapper.FixedPlaceOfSupplyLabel);
				AssertEquals("Other Territories", lineWrapper.FixedPlaceOfSupply);
			}
		}

		#endregion

	}
}
