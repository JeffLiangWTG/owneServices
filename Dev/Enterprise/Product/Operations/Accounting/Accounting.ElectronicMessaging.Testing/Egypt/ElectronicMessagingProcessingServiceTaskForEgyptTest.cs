using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using CargoWise.Accounting.eInvoicing.Egypt;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.DataTransfer.EInvoicing.Egypt;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Accounting.ElectronicMessaging.Testing.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Egypt.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForEgypt))]
	public class ElectronicMessagingProcessingServiceTaskForEgyptTest : GlobalElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForEgypt>
	{
		protected override ZString CountryCode => CountryCodes.Egypt;

		protected override DateTime TestDate => new DateTime(2021, 05, 23);

		protected override string ExpectedMessageTypeForGenerateInvoiceRequest => EgyptEInvoiceAPICommandList.Codes.GenerateInvoiceSubmission;

		protected override ElectronicMessagingProcessingServiceTaskForEgypt GetCountrySpecificServiceTask() => new ElectronicMessagingProcessingServiceTaskForEgypt();

		protected override int ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions => 2;

		protected override IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions => new int[] { 2 };

		protected override bool CountryUsesPendingPivotStatus => true;

		protected override RefCurrency CurrencyOfTestTransaction => TestObjectCreator.GetCurrency(Enterprise.Core.Constants.CurrencyCodes.Egypt);

		protected override void AddCountrySpecificCustomsCodesForBranchOrgProxy(OrgHeader orgProxy)
		{
			Helper.AddCustomsCodeForCountryIfMissing(orgProxy, CountryCode, OrgCusCode.CodeTypes.CorporationCode);
			Helper.AddCustomsCodeForCountryIfMissing(orgProxy, CountryCode, OrgCusCode.EgyptCodeTypes.CommercialRegistrationNumber);
			Helper.AddCustomsCodeForCountryIfMissing(orgProxy, CountryCode, OrgCusCode.CodeTypes.GovBusinessCode);
		}

		protected override void AddCountrySpecificCredentialsForCompanyOrBranch(GlbBranch branchWithCompany)
		{
			var credential = AccountingElectronicMessagingRegistry.Instance.EgyptEInvoicingCredentials.Value;
			credential.ClientId = "cd41ae9a-fcdd-46d0-8ec3-aaa26c93b03b";
			credential.ClientSecret = "1d8b2aca-480d-4509-b7ce-c12e84c86800";
			AccountingElectronicMessagingRegistry.Instance.EgyptEInvoicingCredentials.SetValue(branchWithCompany.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, credential);
		}

		protected override void SimulateUserAction(BusinessObjectFactory factory, IEnumerable<AccEInvoicingTransactionPivot> pivots)
		{
			Signatures.Clear();
			using (var fakeSignatureMaker = SHA256.Create())
			{
				foreach (var pivot in pivots)
				{
					var transaction = (InvoicingBase)pivot.Parent;

					var notifications = new Logger();
					var mapper = new InvoicingBaseToXmlConverter();
					var (mappingSuccessful, mappedXml) = mapper.ConvertToXml(transaction, notifications);
					Assert("Mapping should be successful, errors: " + notifications.ToString(), mappingSuccessful);

					var xmlDoc = new XmlDocument();
					xmlDoc.LoadXml(mappedXml);
					var signatureData = EgyptSignatureHelper.GetNormalisedStringForDigitalSignature(xmlDoc.DocumentElement);
					var dataToSign = ZBlob.FromUTF8(signatureData);
					var signature = fakeSignatureMaker.ComputeHash(dataToSign);
					Signatures.Add(Convert.ToBase64String(signature));
					EgyptSignatureHelper.AddSignatureToDocument(xmlDoc, xmlDoc.DocumentElement, signature);

					var authRecord = factory.New<AccTransactionHeaderAuthorisationRecord>();
					authRecord.AHF_ParentId = transaction.PK;
					authRecord.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
					authRecord.AHF_RecordType = AccTransactionHeaderAuthorisationRecordTypes.Egypt;
					authRecord.AHF_AuthorisationData = ZBlob.FromUTF8(xmlDoc.OuterXml);

					pivot.AIP_ErrorDescription = ZString.Empty;
					pivot.AIP_Status = EInvoicingPivotState.Queued;
				}
			}
			factory.Save();
		}

		readonly List<string> Signatures = new List<string>();

		protected override void AssertCountrySpecificGEIMessageContent(GlobalElectronicInvoicing geiMessage)
		{
			AssertEquals(nameof(geiMessage.Transaction), string.Empty, geiMessage.Transaction);

			AssertNotNullOrEmpty(nameof(geiMessage.Payload), geiMessage.Payload);
			var payload = Encoding.UTF8.GetString(Convert.FromBase64String(geiMessage.Payload));
			var xmlDoc = new XmlDocument();
			AssertNoExceptionThrown("Payload is valid XML", () => xmlDoc.LoadXml(payload));
			foreach (var s in Signatures)
			{
				AssertContains("Signature should be in payload", s, payload);
			}

			AssertEquals(2, geiMessage.Header.ElectronicInvoiceBatchRequest.Credentials.Count);
			var usernameCredential = geiMessage.Header.ElectronicInvoiceBatchRequest.Credentials.Cast<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential>().Single(x => x.Key == CredentialKeys.Username);
			AssertEquals("ClientId Credential", "cd41ae9a-fcdd-46d0-8ec3-aaa26c93b03b", usernameCredential.Value.Value);

			var passwordCredential = geiMessage.Header.ElectronicInvoiceBatchRequest.Credentials.Cast<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential>().Single(x => x.Key == CredentialKeys.Password);
			AssertEquals("Client Secret Credential .Encrypted", true, passwordCredential.Value.Encrypted);
			AssertNotNullOrEmpty("Client Secret Credential .Value", passwordCredential.Value.Value);
			AssertNoExceptionThrown("Client Secret Credential", () => Convert.FromBase64String(passwordCredential.Value.Value));   // eHub encryption is not deterministic, so just assert we can decode.
		}

		public override void TestSuccessfulCreatedEDIInterchangeBodyTextForCancellationMessageFromReversal()
			=> Assert("Not applicable; Egypt does not handle cancellations.", true);

		protected override (int period, RunsEvery runningEvery) ExpectedServiceTaskRunFrequency
			=> (30, RunsEvery.Minute);

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
