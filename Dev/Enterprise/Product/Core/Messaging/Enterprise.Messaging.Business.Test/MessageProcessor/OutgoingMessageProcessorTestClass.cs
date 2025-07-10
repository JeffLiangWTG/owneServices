using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Business.MessageProcessor.Testing
{
	class OutgoingMessageProcessorTestClass : OutgoingMessageProcessor, IDisposable
	{
		readonly List<(BusinessObjectFactory factory, IDisposable relatedDisposable)> factories = new List<(BusinessObjectFactory, IDisposable)>();

		public OutgoingMessageProcessorTestClass(LoggingInformation logger)
			: base(logger)
		{
		}

		public void Dispose()
		{
			foreach (var disposable in factories)
			{
				disposable.relatedDisposable.Dispose();
			}
		}

		public bool EnableQueryLogging { get; set; }

		public IEnumerable<BusinessObjectFactory> Factories => factories.Select(p => p.factory);

		internal override BusinessObjectFactory CreateFactory()
		{
			var factory = base.CreateFactory();

			if (EnableQueryLogging)
			{
				var disposable = factory.EnableTableHitQueryCollection(new[]
				{
					EDIMessageSchema.Constants.TableName
				});
				factories.Add((factory, disposable));
			}

			return factory;
		}

		protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages)
		{
			return new InterchangeProviderBaseTestClass(readyMessages);
		}

		protected override ZQuery MessageFilter
		{
			get { return messageFilter ?? (messageFilter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, "TST")); }
		}
		ZQuery messageFilter;

		#region InterchangeProviderBaseTestClass

		class InterchangeProviderBaseTestClass : InterchangeProviderBase
		{
			public InterchangeProviderBaseTestClass(NonDependentEDIMessageCollection messages)
				: base(messages)
			{
			}

			protected override string GetCollationKey(EDIMessage message)
			{
				return DoNotCollateType;
			}

			protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
			{
				var message = messages[0];
				message.EM_Status = EDIMessage.Status.Sent;
				interchange.EI_From = "FROM";
				interchange.EI_To = "TO";
				interchange.ContainedMessages.Add(message);
			}

			protected internal override string InstructionHowToSetInterchangeSenderID
			{
				get { return ""; }
			}
		}

		#endregion
	}
}
