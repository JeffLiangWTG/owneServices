using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Registry.Testing
{
	[TestedType(typeof(UPEBranchIDsRegistryObjectCollection))]
	public class UPEBranchIDsRegistryObjectCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<UPEBranchIDsRegistryObjectCollection>
	{
		public void TestFindByBranch()
		{
			UPEBranchIDsRegistryObjectCollection collection = new UPEBranchIDsRegistryObjectCollection();
			UPEBranchIDsRegistryObject row1 = collection.AddNew();
			row1.FirstArrivalPort = GlbBranch.CurrentBranch.HomePort.PK;
			AssertNull("Should not find non existig Branch", collection.FindByPort(GlbCompany.CurrentCompany.PK));
			AssertEquals("Should find existig Branch", GlbBranch.CurrentBranch.HomePort.PK, collection.FindByPort(GlbBranch.CurrentBranch.HomePort.PK).FirstArrivalPort);
		}

		#region Implementation
		protected override UPEBranchIDsRegistryObjectCollection GetCollectionToTest()
		{
			return new UPEBranchIDsRegistryObjectCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new UPEBranchIDsRegistryObject();
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

		protected new UPEBranchIDsRegistryObjectCollection Collection
		{
			get
			{
				return base.Collection;
			}
		}
		#endregion
	}
}
