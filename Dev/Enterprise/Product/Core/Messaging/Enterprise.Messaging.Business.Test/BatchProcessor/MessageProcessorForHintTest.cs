using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Business.Testing
{
	sealed class MessageProcessorForHintTest : BaseMessageProcessor
	{
		EDIMessageOrder messageOrder;
		BusinessObjectFactory factory;
		IDisposable disposable;

		public MessageProcessorForHintTest()
		{
			messageOrder = base.MessageOrder;
		}

		public override void Dispose()
		{
			base.Dispose();
			disposable?.Dispose();
		}

		protected override EDIMessageOrder MessageOrder => messageOrder;

		public void SetMessageOrder(EDIMessageOrder order)
		{
			messageOrder = order;
		}

		public EDIMessageOrder GetDefaultMessageOrder()
		{
			return base.MessageOrder;
		}

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			return new List<ApplicationTypeMessageProcessor>
			{
				new ApplicationTypeMessageProcessorForTest(Array.Empty<ZGuid>())
			};
		}

		protected override BusinessObjectFactory GetNewFactoryCore()
		{
			if (factory == null)
			{
				factory = base.GetNewFactoryCore();
				disposable = factory.EnableTableHitQueryCollection(new[]
				{
					EDIMessageSchema.Constants.TableName
				});
			}

			return factory;
		}

		public string[] GetExecutedEDIMessageQueries()
		{
			return factory
				.TableSelects
				.Where(s => s.TableName == EDIMessageSchema.Constants.TableName)
				.SelectMany(s => s.Queries.Select(q => q.Query))
				.ToArray();
		}
	}
}
