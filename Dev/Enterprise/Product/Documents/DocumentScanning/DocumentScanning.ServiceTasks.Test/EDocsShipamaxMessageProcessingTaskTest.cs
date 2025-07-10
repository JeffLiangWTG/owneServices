using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.DocumentScanning.ServiceTasks.Test
{
	[TestedType(typeof(EDocsShipamaxMessageProcessingTask))]
	public sealed class EDocsShipamaxMessageProcessingTaskTest : ServiceTaskTestCase<EDocsShipamaxMessageProcessingTask>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "DSP", hostedServiceAttribute.Code);
				AssertEquals("Description", "Document Ingestion Processing Task", hostedServiceAttribute.Description);
				AssertEquals("Category", "DOC", hostedServiceAttribute.Category);
				AssertEquals("IsMandatory", expected: false, hostedServiceAttribute.IsMandatory);
				AssertEquals("MinimumPeriod", "10minutes", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("MaximumPeriod", "1day", hostedServiceAttribute.MaximumPeriod);
				AssertEquals("CanRunInAnyBranch", expected: true, hostedServiceAttribute.CanRunInAnyBranch);
				AssertEquals("AllowsMultipleInstances", expected: false, hostedServiceAttribute.AllowsMultipleInstances);
			});
		}

		public void TestEDocsShipamaxMessageProcessingTask_DoesNotCallProcessMessage_When_NoParseTypeIsEnabled()
		{
			using (DocManagerRegistry.Instance.SetTemporaryDocParsingRegistryValues(false))
			{
				TestEDocsShipamaxMessageProcessingTask_DoesNotCallProcessMessage_When_NecessaryConditionsNotMet("Failed to run service task. Please check if registry for at least one of the parsing types is enabled at System -> DocManager -> Document Ingestion -> Parse Types.");
			}
		}

		public void TestEDocsShipamaxMessageProcessingTask_DoesNotCallProcessMessage_When_DocumentParseUrlIsEmpty()
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DocManagerRegistry.Instance.DocumentParserUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				TestEDocsShipamaxMessageProcessingTask_DoesNotCallProcessMessage_When_NecessaryConditionsNotMet($"Failed to run service task. Please check if registry {DocManagerRegistry.Instance.DocumentParserUrl.GetLocationInEnglish()} has a valid url.");
			}
		}

		public void TestEDocsShipamaxMessageProcessingTask_DoesNotCallProcessMessage_When_NecessaryConditionsNotMet(string expectedLog)
		{
			//arrange
			var eDocsShipamaxMessageProcessingTask = new EDocsShipamaxMessageProcessingTask();
			var loggerMock = new Mock<ILogger>();
			eDocsShipamaxMessageProcessingTask.ServiceLogger = loggerMock.Object;

			//act
			AssertNoExceptionThrown(() => eDocsShipamaxMessageProcessingTask.RunTask(CancellationToken.None));

			//assert
			loggerMock.Verify(x => x.Log(LogType.Error, expectedLog));
		}

		#region Implementation

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"EDocsShipamaxMessage Queue",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.ShipamaxIntegration,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Internal)
				};
			}
		}

		#endregion
	}
}
