using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MailManager.Integration;
using Enterprise.MailManager.MessageProcessor;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.MailManager.Test
{
	public sealed class MessageFilterTestHelper<TFilter, TBizObj> where TBizObj : BusinessObject
	{
		public MessageFilterTestHelper()
		{
			var type = typeof(TFilter).Name.Contains("Test") ? typeof(TFilter).BaseType : typeof(TFilter);
			if (type != typeof(System.Object))
			{
				var typeHasAttribute = false;
				var assemblyAttributes = type.Assembly.GetCustomAttributes(typeof(MessageFilterAttribute), false);
				foreach (MessageFilterAttribute attr in assemblyAttributes)
				{
					if (attr.TypeName == type.FullName)
					{
						typeHasAttribute = true;
						break;
					}
				}

				TestCase.Assert(type.Name + " should have MessageFilter attribute in assembly.", typeHasAttribute);
			}
		}

		public bool Process(TBizObj bo)
		{
			return Process(ProcessorFactory.GetContext(Log), bo);
		}

		public bool Process(IMessageProcessorContext ctx, TBizObj bo)
		{
			return ProcessorFactory.GetProcessor<TBizObj>().Process(ctx, bo);
		}

		public TestServiceLogger Log
		{
			get
			{
				if (_log == null)
				{
					_log = new TestServiceLogger();
				}

				return _log;
			}
			set { _log = value; }
		}

		TestServiceLogger _log;

		public IMessageProcessorFactory ProcessorFactory
		{
			get
			{
				if (_processorFactory == null)
				{
					var taskCode = "TST";
					var config = new MessageFilterAttribute(taskCode, BusinessObjectFactory.GetTableNameFromType(typeof(TBizObj)), typeof(TFilter));
					_processorFactory = (IMessageProcessorFactory)Activator.CreateInstance(
						ObjectFactory.GetType<IMessageProcessorFactory>(),
						new object[] { taskCode, new IMessageFilterConfig[] { config } });
				}

				return _processorFactory;
			}
		}

		IMessageProcessorFactory _processorFactory;
	}
}
