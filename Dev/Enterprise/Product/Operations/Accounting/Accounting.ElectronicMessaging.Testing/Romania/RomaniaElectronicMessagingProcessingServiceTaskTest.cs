using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Testing.GlobalEInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Romania.Testing
{
	[TestedType(typeof(RomaniaElectronicMessagingProcessingServiceTask))]
	public class RomaniaElectronicMessagingProcessingServiceTaskTest : GlobalElectronicMessagingProcessingServiceTaskTest<RomaniaElectronicMessagingProcessingServiceTask>
	{
		protected override RomaniaElectronicMessagingProcessingServiceTask GetCountrySpecificServiceTask() => new RomaniaElectronicMessagingProcessingServiceTask();

		protected override ZString CountryCode => CountryCodes.Romania;

		protected override RefCurrency CurrencyOfTestTransaction => TestObjectCreator.EUR;

		protected override string ExpectedServicePoint => "XHUB_RO_EINVOICING";

		protected override string ExpectedMessageTypeForGenerateInvoiceRequest => RomaniaEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override string ExpectedMessageTypeForGenerateCancellationRequest => RomaniaEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override int ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions => 3;

		protected override IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions => new int[] { 1, 1, 1 };

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

			var vatCodeType = orgProxy.CustomsCodes.GetOrgCusCode(OrgCusCode.CodeTypes.VATCode, RefCountry.LoadFromCountryCode(Factory, CountryCodes.Romania))
				?? orgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "0123456789", CountryCodes.Romania);
			vatCodeType.OK_CustomsRegNo = "0123456789";

			orgProxy.OH_FullName = "Test Proxy Org Name";
			orgProxy.Factory.Save();
		}

		protected override void AddCountrySpecificCustomsCodesForDebtor(OrgHeader arOrg)
		{
			base.AddCountrySpecificCustomsCodesForDebtor(arOrg);

			var vatCodeType = arOrg.CustomsCodes.GetOrgCusCode(OrgCusCode.CodeTypes.VATCode, RefCountry.LoadFromCountryCode(Factory, CountryCodes.Romania))
				?? arOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "0123456789", CountryCodes.Romania);
			vatCodeType.OK_CustomsRegNo = "0123456789";

			var mainAddr = arOrg.MainAddressCollection.Count > 0
				? arOrg.MainAddressCollection[0]
				: arOrg.MainAddressCollection.AddNew();
			mainAddr.CompanyName = "Test Debtor Company Name";

			var contact = arOrg.Contacts.AddNew();
			contact.OC_ContactName = "Debtor Contact";
			arOrg.Factory.Save();
		}

		protected override void AddAdditionalInformationForCompany(GlbCompany glbCompany)
		{
			var wrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(glbCompany);
			var credential = wrapper.GetGlbExternalPasswordOrCreateNew<GlbCompanyEInvoicingCertificateCredential>(PasswordTypesList.Codes.EIM);
			credential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			credential.GP_ExpiryDate = DateTime.UtcNow.AddDays(5);
		}

		public override void TestDataProviderType()
		{
			var serviceTask = GetCountrySpecificServiceTask();
			AssertType<RomaniaElectronicMessagingProcessingServiceTaskDataProvider>(serviceTask.DataProvider);
		}

		public override void TestSuccessfulCreatedEDIInterchangeBodyTextForCancellationMessageFromReversal()
			=> Assert("Not applicable; Romania does not handle cancellations.", true);

		public void TestMessageName()
		{
			var serviceTask = GetCountrySpecificServiceTask();
			AssertEquals("Electronic Invoice", serviceTask.MessageName);
		}

		public void TestTaskName()
		{
			var serviceTask = GetCountrySpecificServiceTask();
			AssertEquals("RO E-Invoice Processing", serviceTask.TaskName);
		}

		public void TestGetEInvoiceBatchCreator()
		{
			var serviceTask = GetCountrySpecificServiceTask();
			var methodInfo =
				typeof(RomaniaElectronicMessagingProcessingServiceTask).GetMethod("GetEInvoiceBatchCreator",
					BindingFlags.NonPublic | BindingFlags.Instance);
			var result = methodInfo.Invoke(serviceTask, new object[] { GlbCompany.CurrentCompany });
			AssertType<RomaniaEInvoicingBatchCreator>(result);
		}

		public void TestGetEInvoicingDataValidator()
		{
			var serviceTask = GetCountrySpecificServiceTask();
			var methodInfo =
				typeof(RomaniaElectronicMessagingProcessingServiceTask).GetMethod("GetEInvoicingDataValidator",
					BindingFlags.NonPublic | BindingFlags.Instance);
			var result = methodInfo.Invoke(serviceTask, new object[] { GlbCompany.CurrentCompany });
			AssertType<RomaniaEInvoicingDataValidator>(result);
		}
	}
}
