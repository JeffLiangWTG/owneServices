using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Registry.Testing
{
	[TestedType(typeof(CAPackageTypesRegistryDataType))]
	sealed class CAPackageTypesRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CAPackageTypesRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "CAPackageTypePairsRegistryItemEditor"; }
		}

		protected override CAPackageTypesRegistryDataType GetNewDataType()
		{
			return new CAPackageTypesRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();
			CAPackageTypePairCollection collection = new CAPackageTypePairCollection();
			PackageTypePair packageTypePair = collection.AddNew();
			var type = ObjectFactory.GetType<IRefPackTypeCollection>();
			var packTypeCollection = (IRefPackTypeCollection)Activator.CreateInstance(type, RegistryFactory.Instance);
			var codeDescriptionPair = (CodeDescriptionPair)packTypeCollection.GetAsCodeDescriptionPair()[0];
			packageTypePair.FreightPackageType = codeDescriptionPair.Code;

			packageTypePair.CustomsPackageType = packageTypePair.CustomsPackageTypesList[0].Code;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new CAPackageTypesRegistryDataType().Serialise(collection))
			};
		}

		protected override void SetUp()
		{
			base.SetUp();
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"AAA", "AAAAA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"BG", "BG DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"BBB", "BBB DESC", new ZDateTime(1994, 3, 3), ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			factory.Save();
		}
	}
}
