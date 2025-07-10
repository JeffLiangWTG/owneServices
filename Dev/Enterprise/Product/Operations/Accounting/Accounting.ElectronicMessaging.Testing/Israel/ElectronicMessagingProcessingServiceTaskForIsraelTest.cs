using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicMessaging.Testing.GlobalEInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Israel.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForIsrael))]
	public class ElectronicMessagingProcessingServiceTaskForIsraelTest : GlobalElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForIsrael>
	{
		protected override ElectronicMessagingProcessingServiceTaskForIsrael GetCountrySpecificServiceTask() => new ElectronicMessagingProcessingServiceTaskForIsrael();

		protected override ZString CountryCode => CountryCodes.Israel;

		protected override RefCurrency CurrencyOfTestTransaction => TestObjectCreator.EUR;

		protected override string ExpectedMessageTypeForGenerateInvoiceRequest => IsraelEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override string ExpectedMessageTypeForGenerateCancellationRequest => IsraelEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override int ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions => 0;

		protected override IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions => Array.Empty<int>();

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

			var vatCodeType = orgProxy.CustomsCodes.GetOrgCusCode(OrgCusCode.CodeTypes.VATCode, RefCountry.LoadFromCountryCode(Factory, CountryCodes.Israel))
				?? orgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "0123456789", CountryCodes.Israel);
			vatCodeType.OK_CustomsRegNo = "0123456789";

			orgProxy.OH_FullName = "Test Proxy Org Name";
			orgProxy.Factory.Save();
		}

		protected override void AddCountrySpecificCustomsCodesForDebtor(OrgHeader arOrg)
		{
			base.AddCountrySpecificCustomsCodesForDebtor(arOrg);

			var vatCodeType = arOrg.CustomsCodes.GetOrgCusCode(OrgCusCode.CodeTypes.VATCode, RefCountry.LoadFromCountryCode(Factory, CountryCodes.Israel))
				?? arOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "0123456789", CountryCodes.Israel);
			vatCodeType.OK_CustomsRegNo = "0123456789";

			var mainAddr = arOrg.MainAddressCollection.Count > 0
				? arOrg.MainAddressCollection[0]
				: arOrg.MainAddressCollection.AddNew();
			mainAddr.CompanyName = "Test Debtor Company Name";

			var contact = arOrg.Contacts.AddNew();
			contact.OC_ContactName = "Debtor Contact";
			arOrg.Factory.Save();
		}

		public override void TestSuccessfulCreationOfEDIInterchange_ServicePointWithSuffix()
		{
			Assert("This test must be removed for Israel eInvoicing.", true);
		}

		public override void TestSuccessfulCreatedEDIInterchangeBodyTextForCancellationMessageFromReversal()
			=> Assert("Not applicable; Israel does not handle cancellations.", true);
	}
}
