using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ServiceTasks.Testing
{
	[TestedType(typeof(DEICustomsInterchangeProcessingServiceTask))]
	public class DEICustomsInterchangeProcessingServiceTaskTest : GMDCustomsMessagingServiceTest<DEICustomsInterchangeProcessingServiceTask>
	{
		public void TestServiceAttribute()
		{
			AssertSingleHostedServiceAttribute("DEI", "DE Customs Acknowledgement Processing", "DEC");
		}

		public void TestHostedServiceBindingAttribute()
		{
			TestHelper.AssertSingleHostedServiceAttribute<DEICustomsInterchangeProcessingServiceTask>(ServiceTaskApplicationCodeList.Codes.DEIMessageProcessing,
				ServiceTaskApplicationCodeList.Descriptions.DEIMessageProcessing,
				"DEC",
				typeof(DEICustomsInterchangeProcessingServiceTask),
				"60Seconds",
				Core.Constants.CountryCodes.Germany,
				true);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						ServiceTaskApplicationCodeList.Descriptions.DEIMessageProcessing,
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + GenericMessageDeliveryInterchangeTypeList.Codes.DECustomsAcknowledgementSystem,
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GenericMessageDelivery),
				};
			}
		}

		protected override void AssertResult(BusinessObjectFactory factory, GMDCustomsMessagingServiceTestHelperData testData, DEICustomsInterchangeProcessingServiceTask serviceTask)
		{
			var interchange = factory.Load<EDIInterchange>(testData.InterchangePK);
			CombineAssertions(() =>
			{
				AssertEquals("EI_Status", EDIInterchange.Status.Received, interchange.EI_Status);
			});
		}

		protected override DEICustomsInterchangeProcessingServiceTask CreateServiceTask() => new DEICustomsInterchangeProcessingServiceTask();

		protected override GMDCustomsMessagingServiceTestHelperData SetupDataForTesting()
		{
			var message = Factory.New<AtlasEDIMessage>();
			Factory.Save();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = ReferencedMessageIdentifier;
			message.EM_EI = Factory.NewWithValidTestData<EDIInterchange>().PK;
			Factory.Save();

			var interchange = CreateInterchange();
			return new GMDCustomsMessagingServiceTestHelperData()
			{
				InterchangePK = interchange.PK
			};
		}

		EDIInterchange CreateInterchange()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.GenericMessageDelivery;
			interchange.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.DECustomsAcknowledgementSystem;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_From = "DEEAES";
			interchange.EI_To = "KDSER";
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_BodyText = $@"
<DECustomsData>
	<LogbookTime>2020-04-05T10:17:58.2347048+02:00</LogbookTime>
	<CustomsData>
		<CUSINF>
			<AppCode>DEA</AppCode>
			<ReferencedMessageIdentifier>{ReferencedMessageIdentifier}</ReferencedMessageIdentifier>
		</CUSINF>
	</CustomsData>
	<AttachedDocumentCollection>
		<AttachedDocument>
			<FileName>SVM-1-DE9000348-0001-DE005866_58660000003234841.pdf</FileName>
			<Type>
				<Code>CAU</Code>
				<Description>Report SCPRLI</Description>
			</Type>
			<ImageData>MkE5UlZFQUMtQVpQMlU0RjItSkhKRVNDUkMtWllSSFpOUEYtTFREODQ2TFYtMzlRRDZGTk0tM1Y3VzZNRDgtUTdVMjk5VTI=</ImageData>
		</AttachedDocument>
	</AttachedDocumentCollection>
</DECustomsData>";
			return interchange;
		}

		const string ReferencedMessageIdentifier = "DE90003480000956";
	}
}
