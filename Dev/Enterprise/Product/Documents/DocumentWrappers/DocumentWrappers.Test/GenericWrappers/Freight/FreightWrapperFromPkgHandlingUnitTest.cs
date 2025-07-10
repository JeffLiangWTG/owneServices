using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Packing.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromPkgHandlingUnit))]
	sealed class FreightWrapperFromPkgHandlingUnitTest : FreightWrapperTest
	{
		#region  TestSetPackageCollectionOverride

		public void TestSetPackageCollectionOverride()
		{
			var pkgHandlingUnit = Factory.New<PkgHandlingUnit>();
			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(pkgHandlingUnit);

			var loosePackageHeader1 = pkgJob.LoosePackageIDs.AddNew();
			loosePackageHeader1.KPH_PackageID = "XXX";
			loosePackageHeader1.CurrentPackageJob = pkgJob;

			var wrapper = new FreightWrapperFromPkgHandlingUnit(pkgHandlingUnit, Factory);
			(wrapper as IPackageOverrider).SetPackageCollectionOverride(packageHeaders: new[] { loosePackageHeader1 });
			AssertEquals("wrapper.Packages", 1, wrapper.Packages.Count);
			AssertContainsExactElementsInAnyOrder("wrapper.Packages", new[] { loosePackageHeader1 }, wrapper.Packages.Cast<PackageWrapper>().Select(item => item.WrappedObject));
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<PkgHandlingUnit>();
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromPkgHandlingUnit((PkgHandlingUnit)GetNewBusinessObjectToWrap(), Factory);
		}

		public override void TestOrgWrappersReturnTypesOnEmptyWrapper()
		{
			Assert(true);
		}

		protected override bool IsCarrierUsed
		{
			get { return false; }
		}

		#endregion
	}
}
