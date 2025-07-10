using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Jordan;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Accounting.ElectronicMessaging.Testing.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Testing.Jordan
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForJordan))]
	sealed class ElectronicMessagingProcessingServiceTaskForJordanTest : GlobalElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForJordan>
	{
		protected override IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions => [1];

		protected override ZString CountryCode => CountryCodes.Jordan;

		protected override ElectronicMessagingProcessingServiceTaskForJordan GetCountrySpecificServiceTask() => new ElectronicMessagingProcessingServiceTaskForJordan();

		protected override void AddCountrySpecificCredentialsForCompanyOrBranch(GlbBranch branchWithCompany)
		{
			var newFactory = new BusinessObjectFactory();

			var credentials = AccountingElectronicMessagingRegistry.Instance.JordanEInvoicingCredentials.Value;
			credentials.ClientId = ClientID;
			credentials.ClientSecret = ClientSecret;
			AccountingElectronicMessagingRegistry.Instance.JordanEInvoicingCredentials.SetTemporaryValue(branchWithCompany.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, credentials);
		}

		protected override void AddCountrySpecificCustomsCodesForBranchOrgProxy(OrgHeader arOrg)
		{
			arOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GSTCode, "1", CountryCodes.Jordan);
			arOrg.CustomsCodes.AddNew(JordanOrgCusCodeInfo.OrgCusCodes.BusinessActivityNumber, "1", CountryCodes.Jordan);
		}

		public override void TestSuccessfulCreatedEDIInterchangeBodyTextForCancellationMessageFromReversal()
		{
			Assert("Not applicable; Jordan does not handle cancellations.", true);
		}

		protected override string ExpectedMessageTypeForGenerateInvoiceRequest => JordanEInvoiceAPICommandList.Codes.SubmitTransaction;

		string ClientID => "Client ID";
		string ClientSecret => "Client Secret";
	}
}
