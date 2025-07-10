using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestsSubclassesOf(typeof(BasePackagingProvider))]
	abstract class PackagingProviderAbstractTest<TProvider> : Customs.Business.Testing.DataProviderTestCase<TProvider>
		where TProvider : BasePackagingProvider
	{
		public abstract void TestTypeOfPackages();

		public abstract void TestNumberOfPackages();

		public abstract void TestNumberOfPackages_Bulk();

		public abstract void TestShippingMarks();

		public virtual void TestSequenceNumber()
		{
			package.B5_SequenceNumber = 2;
			AssertEquals(2, Provider.SequenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			Factory.Save();

			package = Factory.New<NctsPackage>();
			package.B5_UnitType = "NE";

			provider = (TProvider)Activator.CreateInstance(typeof(TProvider), package);
		}
		TProvider provider;
		protected NctsPackage package;

		protected override TProvider GetProvider() => provider;
	}
}
