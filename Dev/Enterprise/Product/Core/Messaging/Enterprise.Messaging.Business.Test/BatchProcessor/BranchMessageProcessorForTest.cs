using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Business.Testing
{
	public class BranchMessageProcessorForTest : BranchMessageProcessor, IMessageProcessorForTest
	{
		public BranchMessageProcessorForTest(
			ZGuid[] messagePKsToFailProcessingOn = null,
			ZGuid[] messagePKsToFailPreProcessingOn = null,
			ZGuid[] messagePKsToDiscardPreProcessingOn = null,
			IReadOnlyDictionary<ZGuid, string> preProcessingFinalStatus = null,
			bool switchBranches = false)
		{
			this.messagePKsToFailProcessingOn = messagePKsToFailProcessingOn;
			this.messagePKsToFailPreProcessingOn = messagePKsToFailPreProcessingOn;
			this.messagePKsToDiscardPreProcessingOn = messagePKsToDiscardPreProcessingOn;
			this.preProcessingFinalStatus = preProcessingFinalStatus;
			this.switchBranches = switchBranches;
		}

		readonly ZGuid[] messagePKsToFailProcessingOn;
		readonly ZGuid[] messagePKsToFailPreProcessingOn;
		readonly ZGuid[] messagePKsToDiscardPreProcessingOn;
		readonly IReadOnlyDictionary<ZGuid, string> preProcessingFinalStatus;
		readonly bool switchBranches;

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			return new List<ApplicationTypeMessageProcessor>()
			{
				new PreProcessingApplicationTypeMessageProcessorForTest(messagePKsToFailProcessingOn, messagePKsToFailPreProcessingOn, messagePKsToDiscardPreProcessingOn, preProcessingFinalStatus, switchBranches),
				new UDMPreProcessingMessageProcessorForTest(messagePKsToFailProcessingOn)
			};
		}

		protected override BusinessObjectFactory GetNewFactoryCore()
		{
			var result = base.GetNewFactoryCore();
			result.Saved += new BusinessObjectFactory.SavedEventHandler((factory, saved) => FactorySaveCount++);
			return result;
		}

		public int FactorySaveCount { get; private set; }

		public new void Execute() => base.Execute(CancellationToken.None);

		public bool MessageShouldBeProcessedInASeparateFactoryExposed
		{
			get { return base.MessageShouldBeProcessedInASeparateFactory; }
		}

		public int MessagesProcessed
		{
			get { return ApplicationTypeProcessor.MessagesProcessed; }
			set { ApplicationTypeProcessor.MessagesProcessed = 0; }
		}

		public int MessagesPreProcessed
		{
			get { return ApplicationTypeProcessor.MessagesPreProcessed; }
			set { ApplicationTypeProcessor.MessagesPreProcessed = 0; }
		}

		public int ApplicationTypeProcessorFindCount => ApplicationTypeProcessor.MessageFilterAccesses;

		PreProcessingApplicationTypeMessageProcessorForTest ApplicationTypeProcessor => (PreProcessingApplicationTypeMessageProcessorForTest)MessageProcessors[0];

		class UDMPreProcessingMessageProcessorForTest : PreProcessingApplicationTypeMessageProcessorForTest
		{
			public UDMPreProcessingMessageProcessorForTest(ZGuid[] messagePKsToFailOn)
				: base(messagePKsToFailOn)
			{
			}

			protected override string ApplicationCodeCore => "UDM";
		}

		internal class PreProcessingApplicationTypeMessageProcessorForTest : ApplicationTypeMessageProcessorForTest
		{
			public PreProcessingApplicationTypeMessageProcessorForTest(
				ZGuid[] messagePKsToFailProcessingOn,
				ZGuid[] messagePKsToFailPreProcessingOn = null,
				ZGuid[] messagePKsToDiscardPreProcessingOn = null,
				IReadOnlyDictionary<ZGuid, string> preProcessingFinalStatus = null,
				bool switchBranches = false)
				: base(messagePKsToFailProcessingOn)
			{
				this.messagePKsToFailPreProcessingOn = messagePKsToFailPreProcessingOn;
				this.messagePKsToDiscardPreProcessingOn = messagePKsToDiscardPreProcessingOn;
				this.preProcessingFinalStatus = preProcessingFinalStatus;
				this.switchBranches = switchBranches;
			}

			readonly ZGuid[] messagePKsToFailPreProcessingOn;
			readonly ZGuid[] messagePKsToDiscardPreProcessingOn;
			readonly IReadOnlyDictionary<ZGuid, string> preProcessingFinalStatus;
			readonly bool switchBranches;
			public int MessagesPreProcessed;
			protected override bool RequiresPreProcessingCore => true;

			protected override void PreProcessMessageCore(EDIMessage message)
			{
				MessagesPreProcessed++;

				if (messagePKsToFailPreProcessingOn?.Contains(message.PK) is true)
				{
					//// Create EDIInterchange in Factory without essential fields EI_To and EI_From.
					message.Factory.New<EDIInterchange>();
					//// Will cause an exception only when this Factory is next saved.
				}

				if (message.EM_MessageText == TestConstants.HasLinkedObject && message.EM_LinkedObject == null)
				{
					var bizo = message.Factory.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Code, message.EM_MessageNum));
					message.EM_LinkedObject = bizo[0];
				}

				if (switchBranches)
				{
					message.EM_GB = GlbCompany.CurrentCompany.Branches[1].PK;
				}

				string finalStatus;

				if (preProcessingFinalStatus?.TryGetValue(message.PK, out var status) is true)
				{
					finalStatus = status;
				}
				else
				{
					finalStatus = messagePKsToDiscardPreProcessingOn?.Contains(message.PK) is true
						? EDIMessage.Status.Discarded
						: EDIMessage.Status.PreProcessedOK;
				}

				message.EM_Status = finalStatus;
			}
		}
	}
}
