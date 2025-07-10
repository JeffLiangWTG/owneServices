using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Business;
using Enterprise.xTMessaging.Shared;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using Xware.Xt.Grpc.Config;

namespace Enterprise.xTMessaging.ServiceTasks.Test
{
	class OutboundInterchangeProcessorTestWrapper : OutboundInterchangeProcessor
	{
		public OutboundInterchangeProcessorTestWrapper(ILogger logger, IList<bool> usingFactoryWithException = null, bool throwExceptionOnEndTransaction = false, bool throwExceptionOnStartTransaction = false) : base(logger)
		{
			this.usingFactoryWithException = usingFactoryWithException;
			this.throwExceptionOnEndTransaction = throwExceptionOnEndTransaction;
			this.throwExceptionOnStartTransaction = throwExceptionOnStartTransaction;
			this.logger = logger;
		}

		internal IMsgClientProvider ClientProvider { get; set; }
		internal ISubmitMsgAttributeModifier AttributeModifier { get; set; }
		internal Exception exceptionToThrow { get; set; }
		readonly bool throwExceptionOnEndTransaction;
		readonly bool throwExceptionOnStartTransaction;
		readonly ILogger logger;

		internal int BatchCount { get; private set; }

		protected override bool ProcessInterchangesBatch(IList<EDIInterchange> interchanges, DirectxTConnector connector, CancellationToken cancellationToken, bool withErrorReport = false)
		{
			BatchCount++;
			return base.ProcessInterchangesBatch(interchanges, connector, cancellationToken, withErrorReport);
		}

		protected override IMsgClientProvider GetMsgClientProvider(Configuration config, CancellationToken token)
		{
			return ClientProvider;
		}
		protected override ISubmitMsgAttributeModifier GetSubmitMsgAttributeModifier()
		{
			return AttributeModifier;
		}

		readonly IList<bool> usingFactoryWithException;
		int factoryCount;
		protected override BusinessObjectFactory GetFactory()
		{
			var factory = new BusinessObjectFactory();
			if (usingFactoryWithException != null && factoryCount < usingFactoryWithException.Count && usingFactoryWithException[factoryCount])
			{
				factory.Saving += (f) => throw new ZSaveException(new ZDataException(new ApplicationException("I am testing"), null, null), f);
			}
			factoryCount++;
			return factory;
		}

		protected override DirectxTConnector GetConnector(Func<ISubmitMsgAttributeModifier> getSubmitMsgAttributeModifier, Func<Configuration, CancellationToken, IMsgClientProvider> getMsgClientProvider, Configuration config,
			CancellationToken token)
		{
			if (exceptionToThrow == null)
			{
				exceptionToThrow = new Exception();
			}
			if (throwExceptionOnEndTransaction)
			{
				return new DirectxTConnector_FailOnEndTransaction(getMsgClientProvider(config, token), new Cw1DirectxTMessagingConfig(), logger, exceptionToThrow, getSubmitMsgAttributeModifier());
			}
			if (throwExceptionOnStartTransaction)
			{
				return new DirectxTConnector_FailOnStartTransaction(getMsgClientProvider(config, token), new Cw1DirectxTMessagingConfig(), logger, exceptionToThrow, getSubmitMsgAttributeModifier());
			}
			return base.GetConnector(getSubmitMsgAttributeModifier, getMsgClientProvider, config, token);
		}
	}

	class DirectxTConnector_FailOnStartTransaction : DirectxTConnector
	{
		public DirectxTConnector_FailOnStartTransaction(IMsgClientProvider msgClientProvider, IDirectxTMessagingConfig directxTMessagingConfig, ILogger logger, Exception exception, ISubmitMsgAttributeModifier msgAttributeModifier = null, IReceiveHandler receiveHandler = null)
			: base(msgClientProvider, directxTMessagingConfig, logger, msgAttributeModifier, receiveHandler)
		{
			this.exception = exception;
		}

		readonly Exception exception;

		public override void StartTransaction(DateTime? deadline, CancellationToken cancellationToken)
		{
			throw new XtTransactionException("StartTransaction exception", exception);
		}
	}

	class DirectxTConnector_FailOnEndTransaction : DirectxTConnector
	{
		public DirectxTConnector_FailOnEndTransaction(IMsgClientProvider msgClientProvider, IDirectxTMessagingConfig directxTMessagingConfig, ILogger logger, Exception exception, ISubmitMsgAttributeModifier msgAttributeModifier = null, IReceiveHandler receiveHandler = null)
			: base(msgClientProvider, directxTMessagingConfig, logger, msgAttributeModifier, receiveHandler)
		{
			this.exception = exception;
		}

		readonly Exception exception;

		public override void EndTransaction(bool commit, DateTime? deadline, CancellationToken cancellationToken)
		{
			throw new XtTransactionException("EndTransaction exception", exception);
		}
	}

	class InboundInterchangeProcessorTestWrapper : InboundInterchangeProcessor
	{
		public InboundInterchangeProcessorTestWrapper(ILogger logger) : base(logger) { }

		internal IMsgClientProvider ClientProvider { get; set; }

		protected override IMsgClientProvider GetMsgClientProvider(Configuration config, CancellationToken token)
		{
			return ClientProvider;
		}
	}

	class OutboundServiceTaskTestWrapper : OutboundServiceTask
	{
		public OutboundServiceTaskTestWrapper()
		{
			ServiceLogger = new TestServiceLogger();
		}
		internal IMsgClientProvider ClientProvider { get; set; }
		internal ISubmitMsgAttributeModifier AttributeModifier { get; set; }

		protected override IInterchangeProcessor GetMessageProcessor()
		{
			var result = new OutboundInterchangeProcessorTestWrapper(ServiceLogger);
			result.ClientProvider = ClientProvider;
			result.AttributeModifier = AttributeModifier;
			return result;
		}
	}

	class InboundServiceTaskTestWrapper : InboundServiceTask
	{
		public InboundServiceTaskTestWrapper()
		{
			ServiceLogger = new TestServiceLogger();
		}

		internal IMsgClientProvider ClientProvider { get; set; }

		protected override IInterchangeProcessor GetMessageProcessor()
		{
			var result = new InboundInterchangeProcessorTestWrapper(ServiceLogger);
			result.ClientProvider = ClientProvider;
			return result;
		}
	}
}
