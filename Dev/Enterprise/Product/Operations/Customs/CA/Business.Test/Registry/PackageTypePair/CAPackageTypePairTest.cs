using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Registry.Testing
{
	[TestedType(typeof(CAPackageTypePair))]
	sealed class CAPackageTypePairTest : RegistryBusinessObjectTemplateTestCase<CAPackageTypePair>
	{
		public void TestCustomsPackageTypeFieldType()
		{
			var packageType = new CAPackageTypePair();
			AssertEquals("CustomsPackageTypeFieldType", nameof(FieldType.Text), packageType.CustomsPackageTypeFieldType);
		}

		public void TestValidateCustomsPackageType()
		{
			var packageType = new CAPackageTypePair();
			packageType.CustomsPackageType = "AAA";
			AssertNoWarning(packageType.CustomsPackageTypeInfo, "Customs Package Type should be in Customs Package Type list");
			packageType.CustomsPackageType = "BAG";
			AssertHasWarning(packageType.CustomsPackageTypeInfo, "Customs Package Type should be in Customs Package Type list");
		}

		[ExpectNoExceptions]
		public void TestCustomsPackageTypesList()
		{
			var packageType = new CAPackageTypePair();
			Assert("AAA", packageType.CustomsPackageTypesList.ContainsCode("AAA"));
			Assert("BAG should not appear as it is not in the list", !packageType.CustomsPackageTypesList.ContainsCode("BAG"));
			Assert("BBB should not appear as it is too new", !packageType.CustomsPackageTypesList.ContainsCode("BBB"));
		}

		protected override CAPackageTypePair GetBusinessObjectToClone()
		{
			var packageType = new CAPackageTypePair();
			packageType.CustomsPackageType = packageType.CustomsPackageTypesList[0].Code;

			var type = ObjectFactory.GetType<IRefPackTypeCollection>();
			var packTypeCollection = (IRefPackTypeCollection)Activator.CreateInstance(type, RegistryFactory.Instance);
			var codeDescriptionPair = (CodeDescriptionPair)packTypeCollection.GetAsCodeDescriptionPair()[0];
			packageType.FreightPackageType = codeDescriptionPair.Code;

			packageType.CustomsPackageType = packageType.CustomsPackageTypesList[0].Code;
			return packageType;
		}

		protected override CAPackageTypePair GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override void SetUp()
		{
			base.SetUp();
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
		}
	}
}
