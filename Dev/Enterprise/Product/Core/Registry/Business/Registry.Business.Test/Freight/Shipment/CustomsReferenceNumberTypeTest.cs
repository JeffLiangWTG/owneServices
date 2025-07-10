using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CustomsReferenceNumberType))]
	sealed class CustomsReferenceNumberTypeTest : RegistryBusinessObjectTestCaseBase
	{
		public void TestCodeLengthDefaultValue()
		{
			CustomsReferenceNumberType customsReferenceNumberType = new CustomsReferenceNumberType();
			AssertEquals(3, customsReferenceNumberType.CodeMaxLength);
		}

		public void TestSerializeEmptyEntity()
		{
			CustomsReferenceNumberType customsReferenceNumberType = new CustomsReferenceNumberType();

			byte[] serializedValue = RegistryBusinessObjectTemplateTestCase.Serialize(customsReferenceNumberType);

			AssertEquals("<?xml version=\"1.0\" encoding=\"utf-16\"?><CustomsReferenceNumberType><Code /><Description /><IsUnique>Y</IsUnique><IsAutomation>N</IsAutomation></CustomsReferenceNumberType>",
				Encoding.Unicode.GetString(serializedValue).Trim());

			CustomsReferenceNumberType deserializedCustomsReferenceNumberType = RegistryBusinessObjectTemplateTestCase.Deserialize<CustomsReferenceNumberType>(serializedValue);

			AssertEquals(ZString.Empty, deserializedCustomsReferenceNumberType.Code);
			AssertEquals(string.Empty, deserializedCustomsReferenceNumberType.Description);
			AssertEquals(true, deserializedCustomsReferenceNumberType.IsUnique);
			AssertEquals(false, deserializedCustomsReferenceNumberType.IsAutomation);
		}

		public void TestSerializeFullyPopulatedEntity()
		{
			var customsReferenceNumberType = new CustomsReferenceNumberType();
			customsReferenceNumberType.Code = "HRN";
			customsReferenceNumberType.Description = (NoResString)"HORNED";
			customsReferenceNumberType.SystemDefined = true;

			byte[] serializedValue = RegistryBusinessObjectTemplateTestCase.Serialize(customsReferenceNumberType);

			AssertEquals("<?xml version=\"1.0\" encoding=\"utf-16\"?><CustomsReferenceNumberType><Code>HRN</Code><Description>HORNED</Description><IsUnique>Y</IsUnique><IsAutomation>N</IsAutomation></CustomsReferenceNumberType>",
				Encoding.Unicode.GetString(serializedValue).Trim());

			var deserializedCustomsReferenceNumberType = RegistryBusinessObjectTemplateTestCase.Deserialize<CustomsReferenceNumberType>(serializedValue);

			AssertEquals("HRN", deserializedCustomsReferenceNumberType.Code);
			AssertEquals("HORNED", deserializedCustomsReferenceNumberType.Description);
			AssertEquals(true, deserializedCustomsReferenceNumberType.IsUnique);
			AssertEquals(false, deserializedCustomsReferenceNumberType.IsAutomation);
			AssertEquals("Should not serialize 'SystemDefined' as it may change in future.", false, deserializedCustomsReferenceNumberType.SystemDefined);
		}

		public void TestCMRReadonly()
		{
			var customsReferenceNumberType = new CustomsReferenceNumberType();
			customsReferenceNumberType.Code = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CarrierMessageReference;
			customsReferenceNumberType.Description = (NoResString)"CMR";
			var clone = (CustomsReferenceNumberType)customsReferenceNumberType.Clone(null, null);
			Assert(clone.ReadOnly);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1199:Do Not Use Unnecessary Resource String In Unit Tests", Justification = "WI: WI00900275. Testing typing so cannot cast to NoResString instead of ResourceString")]
		public void TestCloneKeepsDescriptionAsResString()
		{
			var customsReferenceNumberType = new CustomsReferenceNumberType();
			customsReferenceNumberType.Code = "XXX";
			customsReferenceNumberType.Description = ResString.GetMultilingualString("test", "Test");
			var clone = (CustomsReferenceNumberType)customsReferenceNumberType.Clone(null, null);
			AssertType(typeof(ResourceString), clone.Description);
		}

		public void TestClone_SystemDefined()
		{
			var customsReferenceNumberType = new CustomsReferenceNumberType();
			customsReferenceNumberType.SystemDefined = true;
			var clone = (CustomsReferenceNumberType)customsReferenceNumberType.Clone(null, null);
			Assert(clone.SystemDefined);
		}

		public void TestReadOnlyMembers()
		{
			var customsReferenceNumberType = new CustomsReferenceNumberType();
			AssertEquals("Precondition.", false, customsReferenceNumberType.SystemDefined);
			Assert(!customsReferenceNumberType.CodeInfo.ReadOnly);
			Assert(!customsReferenceNumberType.DescriptionInfo.ReadOnly);
			Assert(!customsReferenceNumberType.IsUniqueInfo.ReadOnly);

			customsReferenceNumberType.SystemDefined = true;
			AssertEquals("Precondition.", true, customsReferenceNumberType.SystemDefined);
			Assert(customsReferenceNumberType.CodeInfo.ReadOnly);
			Assert(customsReferenceNumberType.DescriptionInfo.ReadOnly);
			Assert(customsReferenceNumberType.IsUniqueInfo.ReadOnly);
		}

		public void TestSystemDefined()
		{
			var customsReferenceNumberType = new CustomsReferenceNumberType();
			AssertEquals("Should not be system defined by default.", false, customsReferenceNumberType.SystemDefined);

			customsReferenceNumberType.SystemDefined = true;
			AssertEquals("Setter should set value.", true, customsReferenceNumberType.SystemDefined);
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new CustomsReferenceNumberType();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CustomsReferenceNumberType();
		}
	}
}
