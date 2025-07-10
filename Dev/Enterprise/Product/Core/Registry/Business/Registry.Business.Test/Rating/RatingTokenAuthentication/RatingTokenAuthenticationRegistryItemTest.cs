using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(RatingTokenAuthenticationRegistryItem))]
	class RatingTokenAuthenticationRegistryItemTest : StronglyTypedRegistryItemTestCase<RatingTokenAuthenticationCollection>
	{
		protected override StronglyTypedRegistryItem<RatingTokenAuthenticationCollection, RatingTokenAuthenticationCollection> GetNewRegistryItem()
		{
			return new RatingTokenAuthenticationRegistryItem("RatingTokenAuthentication", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue, new RatingTokenAuthenticationCollection());
		}
	}

	[TestedType(typeof(RatingTokenAuthenticationCollectionRegistryDataType))]
	class TokenAuthenticationCollectionRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<RatingTokenAuthenticationCollectionRegistryDataType>
	{
		protected override RatingTokenAuthenticationCollectionRegistryDataType GetNewDataType()
		{
			return new RatingTokenAuthenticationCollectionRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.New<IGlbStaff>();
			(staff as BusinessObject).FillWithValidTestData();
			staff.GS_Code = "JWA";
			factory.Save();

			var defaultValue = new RatingTokenAuthenticationCollection();

			var dataType = GetNewDataType();
			var defaultValueBytes = dataType.Serialise(defaultValue);

			var overrideValue = new RatingTokenAuthenticationCollection();
			var tokenAuthentication1 = new RatingTokenAuthentication() { ClientId = "BAA7A0D0-B05F-40EA-B5CC-B10BE46363EA", StaffCode = "JWA" };
			var tokenAuthentication2 = new RatingTokenAuthentication() { ClientId = "BAA7A0D0-B05F-40EA-B5CC-B10BE46363EB", StaffCode = "JWA" };
			overrideValue.Add(tokenAuthentication1);
			overrideValue.Add(tokenAuthentication2);
			var overrideValueBytes = dataType.Serialise(overrideValue);

			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(defaultValue, defaultValueBytes), new ValidSampleAndBinaryValueInDB(overrideValue, overrideValueBytes) };
		}

		protected override string ExpectedEditorName => "RatingTokenAuthenticationRegistryItemEditor";
	}
}
