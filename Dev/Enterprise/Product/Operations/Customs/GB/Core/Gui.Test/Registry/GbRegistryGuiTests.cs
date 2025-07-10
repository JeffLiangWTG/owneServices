using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Registry.Testing
{
	[TestedType(typeof(BadgeCodeSettingCollectionRegistryItem))]
	public class BadgeCodeSettingCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<BadgeCodeSettingCollection>
	{
		protected override StronglyTypedRegistryItem<BadgeCodeSettingCollection, BadgeCodeSettingCollection> GetNewRegistryItem()
		{
			return new BadgeCodeSettingCollectionRegistryItem("", (NoResString)"", "", "", RegistryStorageFlags.System);
		}
	}

	[TestedType(typeof(BadgeCodeControl))]
	public class Test : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new BadgeCodeSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((BadgeCodeControl)control).BadgeCodesGrid.ReadOnly;
		}
	}
}
