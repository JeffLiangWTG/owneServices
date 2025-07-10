using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.Testing.Core.Factories
{
	public class WriterStrategyDataObjectFactoryTest : TestCaseWithFactory
	{
		public void TestCreate_AllDataObjects_NotNull()
		{
			var factory = new SetWriterStrategyDataObjectFactory(Mock.Of<IDataObjectWriterStrategy>());
			var typesToCreate = typeof(TransactionInfo).Assembly.GetTypes()
				.Where(o => o.IsPublic && o.IsClass && !o.IsAbstract && typeof(IDataObject).IsAssignableFrom(o));

			foreach (var type in typesToCreate)
			{
				var instance = factory.Create(type);

				AssertNotNull(type.FullName, instance);
			}
		}

		[ExpectNoExceptions]
		public void TestCreate_TransactionInfoAndCallAllowSet_NoException()
		{
			var factory = new SetWriterStrategyDataObjectFactory(Mock.Of<IDataObjectWriterStrategy>());

			var instance = factory.Create<TransactionInfo>();
			_ = instance.IsAllowSet("Test");

			AssertNotNull(instance);
		}

		[ExpectNoExceptions]
		public void TestCreate_AttachedDocumentAndSetContextCollection_NotExceptions()
		{
			var factory = new SetWriterStrategyDataObjectFactory(Mock.Of<IDataObjectWriterStrategy>());

			var instance = factory.Create<AttachedDocument>();
			_ = instance.SetContextCollection(() => new List<Context>());

			AssertNotNull(instance);
		}
	}
}
