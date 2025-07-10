using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Testing.Core.Factories
{
	public class DefaultDataObjectFactoryTest : TestCaseWithFactory
	{
		public void TestCreate_AllDataObjects_NotNull()
		{
			var factory = new DefaultDataObjectFactory();
			var typesToCreate = typeof(TransactionInfo).Assembly.GetTypes()
				.Where(o => o.IsPublic && o.IsClass && !o.IsAbstract && typeof(IDataObject).IsAssignableFrom(o));

			foreach (var type in typesToCreate)
			{
				var instance = factory.Create(type);

				AssertNotNull(type.FullName, instance);
			}
		}
	}
}
