using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(IncoTermChargeCodes))]
	sealed class IncoTermChargeCodesTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new IncoTermChargeCodes();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
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

		public void TestSerializeDeserializeIncoTermChargeCodesEntity()
		{
			var businessObjectToSerialize = (IncoTermChargeCodes)GetBusinessObjectToSerialise();
			businessObjectToSerialize.SetDefaults(IncoTerms.CostAndFreight);
			businessObjectToSerialize.CustomsDuty = "CNR";

			var serializedValue = Serialize(businessObjectToSerialize);
			var deserializedObject = Deserialize<IncoTermChargeCodes>(serializedValue);

			AssertEquals(businessObjectToSerialize.CustomsDuty, deserializedObject.CustomsDuty);
		}

		#endregion
	}
}
