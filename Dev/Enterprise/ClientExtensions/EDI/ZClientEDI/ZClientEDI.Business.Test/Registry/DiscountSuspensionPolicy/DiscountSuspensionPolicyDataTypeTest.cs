using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(DiscountSuspensionPolicyDataType))]
	class DiscountSuspensionPolicyDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DiscountSuspensionPolicyDataType>
	{
		protected override DiscountSuspensionPolicyDataType GetNewDataType()
		{
			return new DiscountSuspensionPolicyDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "DiscountSuspensionPolicyRegistryEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = new DiscountSuspensionPolicyCollection();
			var collection2 = new DiscountSuspensionPolicyCollection();
			var item1 = collection1.AddNew();
			item1.ProductCode = "ENT";
			item1.PolicyCode = "ALW";

			var item2 = collection2.AddNew();
			item2.ProductCode = "CW1";
			item2.PolicyCode = "NVR";

			var byteArrayValue1 = new DiscountSuspensionPolicyDataType().Serialise(collection1);
			var byteArrayValue2 = new DiscountSuspensionPolicyDataType().Serialise(collection2);

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, byteArrayValue1),
				new ValidSampleAndBinaryValueInDB(collection2, byteArrayValue2)
			};
		}
	}
}
