using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.EMCS.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.ServiceTasks.Testing
{
	[TestedType(typeof(EMCSMessageRetrieverServiceTask))]
	public class EMCSMessageRetrieverServiceTaskTest : GBServiceTaskTestCase<EMCSMessageRetrieverServiceTask>
	{
		public void TestParseFullExample()
		{
			var decReference = "B000000001";
			var dec = Factory.New<EMCSJobDeclaration>();
			dec.JE_DeclarationReference = decReference;

			Factory.Save();

			var interchangePks = new List<ZGuid>();
			for (var i = 0; i < 17;)
			{
				var interchange = Factory.New<EMCSInterchange>();
				interchange.EI_ReceiveTransmit = EDIInterchange.Status.Received;
				interchange.EI_Status = EDIInterchange.Status.Queued;
				interchange.EI_From = "eHub";
				interchange.EI_To = "WTG";
				interchange.EI_InterchangeNum = (1234 + i).ToString();
				var bodyXml = LoadSampleXml($"Enterprise.Customs.GB.EMCS.ServiceTasks.Testing.TestFiles.InboundMessageBody{++i:00}.xml");
				var encodedBody = Convert.ToBase64String(Encoding.UTF8.GetBytes(bodyXml));

				var encodedMessage = EncodedResponse.Replace(EncodedMessagePlaceholder, encodedBody).Replace(MessageTypePlaceholder, "111");
				var encodedResponse = Convert.ToBase64String(Encoding.UTF8.GetBytes(encodedMessage));

				interchange.EI_BodyText = InterchangeMessage.Replace(ResponseBodyPlaceholder, encodedResponse);
				interchangePks.Add(interchange.PK);
			}

			Factory.Save();

			InitialiseAndRunTaskSchedule(new EMCSMessageRetrieverServiceTask());

			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIInterchange.ApplicationCodes.GbCustomsEMCS);
			query.AddToFilter(EDIMessageSchema.EM_EI, interchangePks);
			query.OrderBy = EDIMessage.Schema.EM_MessageNum;
			var messages = Factory.Load<EDIMessage>(query);

			CombineAssertions(() =>
			{
				AssertEquals("Should process all messages", 17, messages.Length);
				var knownMessages = messages.Where(x => EMCSResponseMessageDetails.Instance.ResponseMessages.ContainsKey(x.EM_MessageType));
				var unknownMessages = messages.Where(x => !EMCSResponseMessageDetails.Instance.ResponseMessages.ContainsKey(x.EM_MessageType));
				Assert("All known messages should have status processed OK", knownMessages.All(x => x.EM_Status == EDIMessage.Status.ProcessedOK));
				Assert("Known processed messages should be linked to job", knownMessages.All(x => x.EM_LinkedObject.PK == dec.PK));
				Assert("All unknown messages should have status failed", unknownMessages.All(x => x.EM_Status == EDIMessage.Status.Failed));
			});
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"UK Customs EMCS messages inbound",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.GbCustomsEMCS
						),
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"UK Customs EMCS interchanges inbound",
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIMessageStatusList.Codes.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.GbCustomsEMCS
						)
				};
			}
		}

		protected override ZString ServiceTaskCodeCore => "EMC";

		protected override void SetUpCore()
		{
			base.SetUpCore();
			branchEnvironment = DisposableEnvironment.ForBranch(GetBranchGuid());
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();
			branchEnvironment.Dispose();
		}

		IDisposable branchEnvironment;

		string LoadSampleXml(string key)
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(key))
			using (var sr = new StreamReader(stream))
			{
				return sr.ReadToEnd();
			}
		}

		Guid GetBranchGuid()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			company.GC_Code = "KSE";
			var testBranch = company.Branches.AddNew();
			testBranch.GB_Code = "AFC";
			testBranch.GB_RL_NKHomePort = "GBLON";
			testBranch.GB_BranchName = "Test Branch AFC";
			Factory.Save();
			return testBranch.PK.ToGuid();
		}

		const string ResponseBodyPlaceholder = "<<<ResponseBody>>>";
		const string InterchangeMessage = @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseHeader Provider=""EMCS"">
    <ServiceReference>B000000001</ServiceReference>
  </s0:ResponseHeader>
  <s0:ResponseBody ContentType=""XML"" Encoding=""base64"">
    <<<ResponseBody>>>
  </s0:ResponseBody>
</s0:GBCustomsBusinessResponse>";

		const string EncodedMessagePlaceholder = "<<<EncodedMessage>>>";
		const string MessageTypePlaceholder = "<<<MessageType>>>";
		const string EncodedResponse = @"[{""encodedMessage"":""<<<EncodedMessage>>>"",""messageType"":""<<<MessageType>>>"",""createdOn"":""2024-02-07T14:36:56.054536Z""}]";
	}
}
