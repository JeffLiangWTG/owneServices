using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.eHub;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Customs.Testing
{
	[TestedType(typeof(ScavengingSettingCollection))]
	sealed class ScavengingSettingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ScavengingSettingCollection>
	{
		#region Implementation
		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override ScavengingSettingCollection GetCollectionToTest()
		{
			return new ScavengingSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ScavengingSetting();
		}

		#endregion
	}
}
