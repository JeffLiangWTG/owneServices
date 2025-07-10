using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Registry.Testing
{
	[TestedType(typeof(UPEGlbGroupsRegistryObjectCollection))]
	public class UPEGlbGroupsRegistryObjectCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<UPEGlbGroupsRegistryObjectCollection>
	{
		#region Implementation
		protected override UPEGlbGroupsRegistryObjectCollection GetCollectionToTest()
		{
			return new UPEGlbGroupsRegistryObjectCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new UPEGlbGroupsRegistryObject();
		}

		protected override bool RequiresFactory
		{
			get
			{
				return false;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		protected new UPEGlbGroupsRegistryObjectCollection Collection
		{
			get
			{
				return base.Collection;
			}
		}
		#endregion
	}
}
