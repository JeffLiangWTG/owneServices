using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(StatisticsFoldupInfoCollection))]
	sealed class StatisticsFoldupInfoCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<StatisticsFoldupInfoCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new StatisticsFoldupInfo(new FallbackLevel(Environment.Env.CurrentCompany, Environment.Env.CurrentBranch, Environment.Env.CurrentDepartment), Factory, GetCollectionToTest());
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override StatisticsFoldupInfoCollection GetCollectionToTest()
		{
			return collection ?? (collection = new StatisticsFoldupInfoCollection());
		}

		StatisticsFoldupInfoCollection collection;
	}
}
