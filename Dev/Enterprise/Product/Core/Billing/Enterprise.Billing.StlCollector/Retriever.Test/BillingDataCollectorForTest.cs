using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever.Testing
{
	class BillingDataCollectorForTest : BillingDataCollector
	{
		public BillingDataCollectorForTest()
			: this(new LoggerForTest())
		{
		}

		public BillingDataCollectorForTest(ILogger serviceLogger)
			: this(serviceLogger, new BusinessObjectFactory())
		{
		}

		public BillingDataCollectorForTest(ILogger serviceLogger, BusinessObjectFactory factory)
			: this(serviceLogger, factory, new ScriptLoader(new ScriptFactoryForTest()))
		{
		}

		public BillingDataCollectorForTest(IScriptLoader scriptLoader)
			: this(new LoggerForTest(), new BusinessObjectFactory(), scriptLoader)
		{
		}

		public BillingDataCollectorForTest(ILogger serviceLogger, BusinessObjectFactory factory, IScriptLoader scriptLoader)
			: base(serviceLogger, factory, scriptLoader.Load(factory))
		{
		}

		public BillingDataCollectorForTest(ILogger serviceLogger, BusinessObjectFactory factory, IEnumerable<IStlScriptWithConfig> stlScripts)
			: base(serviceLogger, factory, stlScripts)
		{
		}

		public IEnumerable<string> Logs { get { return ((LoggerForTest)generalLogger).LogEntries; } }
		public BusinessObjectFactory Factory => bizoFactory;
		public override bool SkipValidation { get; set; } = true;
		TimeSpan _exceptionThreshold = TimeSpan.FromHours(0);
		public override TimeSpan ExceptionThreshold
		{
			get => _exceptionThreshold;
			protected internal set => _exceptionThreshold = value;
		}
	}
}
