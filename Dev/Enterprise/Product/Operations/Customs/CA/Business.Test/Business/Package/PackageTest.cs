using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(Package))]
	sealed class PackageTest : Customs.Business.Testing.BasePackageTest
	{
		public void TestOverriden()
		{
			var package = Factory.New<BasePackage>();
			AssertEquals("Validation type", typeof(PackageValidation), package.Validation.GetType());
		}

		public void TestPackTypeList_NotIID()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ZString.Empty;
			var package = declaration.Packages.AddNew();
			AssertType<ACROSSPackageTypes>(package.PackTypeList);
		}

		public void TestPackTypeList_IID()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"AAA", "AAAAA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"BG", "BG DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"BBB", "BBB DESC", new ZDateTime(1994, 3, 3), ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var declaration = newFactory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var package = declaration.Packages.AddNew();
			AssertEquals("AAA, BG", package.PackTypeList.CodesAsString);
		}

		protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

		new JobDeclaration declaration => (JobDeclaration)base.declaration;
	}
}
