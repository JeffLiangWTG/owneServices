using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.Testing.GlobalEInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	public abstract class KoreaSouthElectronicMessagingProcessingServiceTaskTest<T> : GlobalElectronicMessagingProcessingServiceTaskTest<T> where T : KoreaSouthElectronicMessagingProcessingServiceTask
	{
		public abstract void TestHostedServiceTimeProperties();

		public override void TestSuccessfulCreatedEDIInterchangeBodyTextForCancellationMessageFromReversal() => Assert("Not applicable; KoreaSouth does not handle cancellations.", true);

		[TestDate(2023, 10, 26)]
		public void TestDoNotCreateEDIInterchange()
		{
			var company = Helper.CreateCompanyAndBranch(CountryCode + "1", "B01", CountryCode, true);
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, company.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AddCountrySpecificData();
				using (AccountingConfigurationRegistry.Instance.EnableKoreaSouthEDIInterchangeCreator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var arInvoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", company.LocalCurrency, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.AALSHI, TestObjectCreator.CC1.PK);
					SaveARInvoice(arInvoice);

					AssertEReportingStatus("E-Reporting status is Queued", EInvoicingPivotState.Queued, arInvoice, ExpectedPivotActionType);

					var serviceTask = GetCountrySpecificServiceTask();
					var logger = InitialiseAndRunTaskSchedule(serviceTask);

					arInvoice = new BusinessObjectFactory().Load<ARInvoice>(arInvoice.PK);
					AssertEReportingStatus("E-Reporting status is Batched", EInvoicingPivotState.Batched, arInvoice, ExpectedPivotActionType);

					var interchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
					AssertEquals("No Interchange was created", 0, interchanges.Length);
				}
			}
		}

		public void TestProcessMessageSubTypeRequest()
		{
			TestDateAttribute.Date = TestDate;
			var company = Helper.CreateCompanyAndBranch(CountryCode + "1", "B01", CountryCode, true);
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, company.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AddCountrySpecificData();
				AddAdditionalInformationForCompany(company);
				AddCountrySpecificCustomsCodesForBranchOrgProxy(company.FirstActiveBranch.OrgProxy);
				AddCountrySpecificCustomsCodesForDebtor(TestObjectCreator.AALSHI);
				AddCountrySpecificCredentialsForCompanyOrBranch(company.FirstActiveBranch);

				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					var arInvoiceGEN = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", company.LocalCurrency, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.AALSHI, TestObjectCreator.CC1.PK);
					SaveARInvoice(arInvoiceGEN, EInvoicingPivotActionType.Submit);
					var arInvoiceGEQ = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "002", company.LocalCurrency, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.AALSHI, TestObjectCreator.CC1.PK);
					SaveARInvoice(arInvoiceGEQ, EInvoicingPivotActionType.StatusCheck);

					AssertEReportingStatus("GEN E-Reporting status is Queued", EInvoicingPivotState.Queued, arInvoiceGEN, EInvoicingPivotActionType.Submit);
					AssertEReportingStatus("GEQ E-Reporting status is Queued", EInvoicingPivotState.Queued, arInvoiceGEQ, EInvoicingPivotActionType.StatusCheck);

					var serviceTask = GetCountrySpecificServiceTask();
					var logger = InitialiseAndRunTaskSchedule(serviceTask);

					var newFactory = new BusinessObjectFactory();
					var arInvoiceGENReload = newFactory.Load<ARInvoice>(arInvoiceGEN.PK);
					var arInvoiceGEQReload = newFactory.Load<ARInvoice>(arInvoiceGEQ.PK);
					AssertProcessMessageSubType(arInvoiceGENReload, arInvoiceGEQReload);

					var interchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
					AssertEquals("Only 1 Interchange was created", 1, interchanges.Length);
				}
			}
		}

		#region Override

		protected override void AssertEDIInterchange(IXmlEDIInterchange interchange, ZString servicePointSuffix)
		{
			EInvoicingTestHelper.AssertGEIEInvoicingEDIInterchange(interchange, expectedMessageType: ExpectedMessageTypeForGenerateInvoiceRequest, expectedTo: ExpectedServicePoint);
		}

		protected override ZString CountryCode => CountryCodes.KoreaSouth;

		protected override string ExpectedServicePoint => $"XHUB_{CountryCode}_EINVOICING";

		protected override RefCurrency CurrencyOfTestTransaction => TestObjectCreator.KRW;

		protected override int ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions => 1;

		protected override IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions => new int[] { 1 };

		protected override (int period, RunsEvery runningEvery) ExpectedServiceTaskRunFrequency => (1, RunsEvery.Hour);

		protected override void AddCountrySpecificData()
		{
			base.AddCountrySpecificData();

			AccountingMasterFilesRegistry.Instance.EReportingGEIMessageSystemType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.AlwaysTestSystem);
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime());
		}

		protected override void AddCountrySpecificCustomsCodesForBranchOrgProxy(OrgHeader orgProxy)
		{
			base.AddCountrySpecificCustomsCodesForBranchOrgProxy(orgProxy);

			var vatCodeType = orgProxy.CustomsCodes.GetOrgCusCode(OrgCusCode.CodeTypes.VATCode, RefCountry.LoadFromCountryCode(Factory, CountryCodes.KoreaSouth))
				?? orgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "0123456789", CountryCodes.KoreaSouth);
			vatCodeType.OK_CustomsRegNo = "0123456789";

			orgProxy.OH_FullName = "Test Proxy Org Name with at least 70 chars        AAAAAAAAABAAAAAAAAAB";

			var contact = orgProxy.Contacts.AddNew();
			contact.OC_ContactName = "Org Proxy Contact";
			contact.Allocations.AddNew().PC_Type = ReadyKoreaConstants.KRC;

			orgProxy.Factory.Save();
		}

		protected override void AddCountrySpecificCustomsCodesForDebtor(OrgHeader arOrg)
		{
			base.AddCountrySpecificCustomsCodesForDebtor(arOrg);

			var vatCodeType = arOrg.CustomsCodes.GetOrgCusCode(OrgCusCode.CodeTypes.VATCode, RefCountry.LoadFromCountryCode(Factory, CountryCodes.KoreaSouth))
				?? arOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "0123456789", CountryCodes.KoreaSouth);
			vatCodeType.OK_CustomsRegNo = "0123456789";

			var mainAddr = arOrg.MainAddressCollection.Count > 0
				? arOrg.MainAddressCollection[0]
				: arOrg.MainAddressCollection.AddNew();
			mainAddr.CompanyName = "Test Debtor Company Name with at least 70 chars   AAAAAAAAABAAAAAAAAAB";

			var contact = arOrg.Contacts.AddNew();
			contact.OC_ContactName = "Debtor Contact";
			contact.Allocations.AddNew().PC_Type = ReadyKoreaConstants.KRC;

			arOrg.Factory.Save();
		}

		protected override void BeforeSaveOfARAPINVCRDADJTransactions(ARInvoice arInvoice, ARCreditNote arCreditNote, ARAdjustmentNote arAdjustmentNote, APInvoice apInvoice, APCreditNote apCreditNote, APAdjustmentNote apAdjustmentNote)
		{
			base.BeforeSaveOfARAPINVCRDADJTransactions(arInvoice, arCreditNote, arAdjustmentNote, apInvoice, apCreditNote, apAdjustmentNote);

			if (arInvoice != null)
			{
				arInvoice.AH_ComplianceSubType = KoreaSouthComplianceInfo.ComplianceSubTypeCodes.TXI;
				foreach (InvoicingLineBase line in arInvoice.Lines.ToArray())
				{
					line.AL_AT = TestObjectCreator.FREEVAT.PK;
				}
			}

			if (arCreditNote != null)
			{
				arCreditNote.AH_Calc_AmendStatusCode = "01";
			}
		}

		#endregion

		#region Implementation

		protected void SaveARInvoice(ARInvoice arInvoice, string actionType = null)
		{
			BeforeSaveOfARAPINVCRDADJTransactions(arInvoice, null, null, null, null, null);

			Factory.Save();

			if ((actionType ?? ExpectedPivotActionType) == EInvoicingPivotActionType.StatusCheck)
			{
				PrepareTestDataForGEQRequest(arInvoice);
			}
		}

		protected void PrepareTestDataForGEQRequest(InvoicingBase invoicingBase)
		{
			var pivotForGEN = LoadPivot(invoicingBase, EInvoicingPivotActionType.Submit);
			pivotForGEN.AIP_Status = Core.Constants.EInvoicingPivotState.Delivered;
			TestObjectCreator.CreateEInvoicingBatchForPivot(pivotForGEN, NextBatchNumber, EInvoicingBatchState.Sent);
			Factory.Save();

			var pivotForGEQ = TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingBase, EInvoicingPivotActionType.StatusCheck, Core.Constants.EInvoicingPivotState.Queued);
			Factory.Save();
		}

		protected void AssertEReportingStatus(string message, string expected, InvoicingBase invoicingBase, string actionType)
		{
			var pivot = LoadPivot(invoicingBase, actionType);
			AssertEquals(message, expected, pivot.AIP_Status);
		}

		protected abstract void AssertProcessMessageSubType(params ARInvoice[] invoiceList);

		protected abstract string ExpectedPivotActionType { get; }

		protected AccEInvoicingTransactionPivot LoadPivot(InvoicingBase invoicingBase, string actionType)
		{
			var query = new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ActionType, actionType)
						  .AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoicingBase.PK)
						  .AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ParentTableCode, AccTransactionHeaderSchema.Constants.Prefix);

			return invoicingBase.Factory.LoadTop1<AccEInvoicingTransactionPivot>(query);
		}

		ZInt NextBatchNumber => int.Parse(AccountingNumberFountainWrapperFactory.Instance.AccEInvoicingBatchNumber.GetNext(Db.Connection), CultureInfo.InvariantCulture);

		#endregion
	}
}
