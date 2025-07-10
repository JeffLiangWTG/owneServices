using System;
using System.Text;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(GuidRegistryDataType))]
	public class GuidRegistryDataTypeTest : RegistryDataTypeTestCase<GuidRegistryDataType>
	{
		[ExpectNoExceptions]
		public void TestValidate_OptionalWithEmptyGuid()
		{
			OptionalDataType.Validate(
				null,
				Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		[ExpectException(typeof(RegistryValidationException))]
		public void TestValidate_MandatoryWithEmptyGuid()
		{
			MandatoryDataType.Validate(
				new GuidRegistryItem(string.Empty, (MultilingualString)null, null, null, RegistryStorageFlags.All),
				Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		public override void TestGetGuidValue()
		{
			Guid value = Guid.NewGuid();
			AssertEquals("Should return an actual guid value for the item", value, ((IRegistryDataType)DataType).GetGuidValue(value));
		}

		#region Implementation

		protected override GuidRegistryDataType GetNewDataType()
		{
			return OptionalDataType;
		}

		GuidRegistryDataType OptionalDataType
		{
			get
			{
				if (optionalDataType == null)
				{
					optionalDataType = (GuidRegistryDataType)new GuidRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.IsValueOptional).DataType;
				}
				return optionalDataType;
			}
		}

		GuidRegistryDataType MandatoryDataType
		{
			get
			{
				if (mandatoryDataType == null)
				{
					mandatoryDataType = (GuidRegistryDataType)new GuidRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.IsValueMandatory).DataType;
				}
				return mandatoryDataType;
			}
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			Guid newGuid = Guid.NewGuid();
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(Guid.Empty, Encoding.Unicode.GetBytes(Guid.Empty.ToString())),
				new ValidSampleAndBinaryValueInDB(newGuid, Encoding.Unicode.GetBytes(newGuid.ToString()))
			};
		}

		GuidRegistryDataType optionalDataType;
		GuidRegistryDataType mandatoryDataType;

		#endregion
	}
}
