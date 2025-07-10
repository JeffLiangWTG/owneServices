using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Testing.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Spain.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForSpain))]
	public class ElectronicMessagingProcessingServiceTaskForSpainTest : GlobalElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForSpain>
	{
		protected override ZString CountryCode => CountryCodes.Spain;

		protected override DateTime TestDate => new DateTime(2022, 04, 11);

		protected override string ExpectedMessageTypeForGenerateInvoiceRequest => SpainEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override ElectronicMessagingProcessingServiceTaskForSpain GetCountrySpecificServiceTask()
			=> new ElectronicMessagingProcessingServiceTaskForSpain();

		protected override int ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions => 5;

		protected override IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions => new int[] { 3, 2 };

		protected override void AddCountrySpecificCustomsCodesForBranchOrgProxy(OrgHeader orgProxy)
		{
			var nifNumber = Helper.AddCustomsCodeForCountryIfMissing(orgProxy, CountryCode, "NIF");
			Helper.AddCustomsCodeForCountryIfMissing(orgProxy, CountryCode, "SII", nifNumber);
		}

		protected override void AddCountrySpecificCustomsCodesForDebtor(OrgHeader orgProxy)
		{
			Helper.AddCustomsCodeForCountryIfMissing(orgProxy, CountryCode, "NIF");
		}

		protected override void AssertCountrySpecificGEIMessageContent(GlobalElectronicInvoicing geiMessage)
		{
			Assert("TODO: Add assertions in future Spain work items", true);
		}

		public override void TestSuccessfulCreatedEDIInterchangeBodyTextForCancellationMessageFromReversal()
			=> Assert("Not applicable; Spain does not support cancellation messages.", true);

		protected override (int period, RunsEvery runningEvery) ExpectedServiceTaskRunFrequency
			=> (30, RunsEvery.Minute);

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
