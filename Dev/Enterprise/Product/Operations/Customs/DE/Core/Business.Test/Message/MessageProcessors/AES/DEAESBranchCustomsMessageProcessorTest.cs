using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using MessageDefinitionsAESVersion3_0 = CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class DEAESBranchCustomsMessageProcessorTest : TestCaseWithFactory
	{
		public void TestResolveAESMessageProcessors()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAesSystem;
			var messages = AesResponseMessageDetails.Instance.ResponseMessages.Select(d => d.Key).ToList();
			var exitControlList = new ZString[] { 
				nameof(MessageDefinitionsAESVersion3_0.DEXTLF),
				nameof(MessageDefinitionsAESVersion3_0.DEERRG),
				nameof(MessageDefinitionsAESVersion3_0.DEXTSE),
				nameof(MessageDefinitionsAESVersion3_0.DEXTJE),
				nameof(MessageDefinitionsAESVersion3_0.DEXTDE),
			};
			CombineAssertions(() =>
			{
				foreach (var technicalMessageName in messages)
				{
					message.EM_ApplicationReference = technicalMessageName;
					message.EM_MessageSubType = technicalMessageName.In(exitControlList)
						? ExportMessageSubTypeList.Codes.EXT
						: technicalMessageName.StartsWith("DEERR")
							? ExportMessageSubTypeList.Codes.EXQ
							: "";
					var prc = processor.GetApplicationTypeProcessorCore(message);
					AssertEquals(technicalMessageName, true, prc is BranchCustomsApplicationTypeMessageProcessor);
				}
			});
		}

		public void TestResolveAESMessageProcessors_InvalidApplicationReference()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAesSystem;
			message.EM_ApplicationReference = "GCRECE";
			var prc = processor.GetApplicationTypeProcessorCore(message);
			AssertNull(prc);
		}

		protected override void SetUp()
		{
			base.SetUp();
			processor = new DEAESBranchCustomsMessageProcessor
			{
				Logger = new LoggingInformation()
			};
		}
		DEAESBranchCustomsMessageProcessor processor;
	}
}
