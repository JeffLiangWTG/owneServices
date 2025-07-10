using System.Text;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Registry.Testing
{
	[TestedType(typeof(DelayFactorRegistryBusinessObject))]
	sealed class DelayFactorRegistryBusinessObjectTest : RegistryBusinessObjectTemplateTestCase<DelayFactorRegistryBusinessObject>
	{
		public void TestDelayIntervalTypeList()
		{
			var testBizo = new DelayFactorRegistryBusinessObject(5, DelayIntervalTypeCodes.Codes.None, 5, DelayIntervalTypeCodes.Codes.None);

			var list = testBizo.HVSDelayIntervalTypeList;
			AssertEquals("2 members", 2, list.Count);
			Assert(list.ContainsCode(DelayIntervalTypeCodes.Codes.None));
			Assert(list.ContainsCode(DelayIntervalTypeCodes.Codes.DAR));
			list = testBizo.CONDelayIntervalTypeList;
			AssertEquals("2 members", 2, list.Count);
			Assert(!list.ContainsCode(DelayIntervalTypeCodes.Codes.Default));
			Assert(list.ContainsCode(DelayIntervalTypeCodes.Codes.None));
		}

		public void TestEquate()
		{
			var bizo1 = new DelayFactorRegistryBusinessObject(5, DelayIntervalTypeCodes.Codes.None, 6, DelayIntervalTypeCodes.Codes.Default);
			var bizo2 = new DelayFactorRegistryBusinessObject(5, DelayIntervalTypeCodes.Codes.None, 6, DelayIntervalTypeCodes.Codes.Default);
			Assert(bizo1.Equals(bizo2));
			bizo1.HVSDelayIntervalType = DelayIntervalTypeCodes.Codes.DAR;
			bizo2.HVSDelayIntervalType = DelayIntervalTypeCodes.Codes.DAR;
			Assert(bizo1.Equals(bizo2));
			bizo1.CONDelayInterval = 5;
			Assert(!bizo1.Equals(bizo2));
			bizo1.CONDelayInterval = 6;
			Assert(bizo1.Equals(bizo2));
		}

		public void TestValidateCONDelayInterval()
		{
			var expectederror = "Goods must be accounted for by the 24th of the month.";
			var bizo = new DelayFactorRegistryBusinessObject(5, DelayIntervalTypeCodes.Codes.None, 6, DelayIntervalTypeCodes.Codes.Default);
			bizo.CONDelayInterval = 24;
			bizo.CONDelayIntervalType = DelayIntervalTypeCodes.Codes.DAY;
			bizo.ValidateCONDelayInterval();
			AssertNoErrorContaining(bizo.CONDelayIntervalInfo, expectederror);

			bizo.CONDelayInterval = 25;
			bizo.ValidateCONDelayInterval();
			AssertHasErrorContaining(bizo.CONDelayIntervalInfo, expectederror);

			bizo.CONDelayInterval = 26;
			bizo.ValidateCONDelayInterval();
			AssertHasErrorContaining(bizo.CONDelayIntervalInfo, expectederror);

			bizo.CONDelayInterval = -1;
			bizo.ValidateCONDelayInterval();
			AssertHasError(bizo.CONDelayIntervalInfo, "value cannot be negative.");

			bizo.CONDelayInterval = 0;
			bizo.ValidateCONDelayInterval();
			AssertHasError(bizo.CONDelayIntervalInfo, "value cannot be zero.");
		}

		public void TestReadOnly()
		{
			var bizo = new DelayFactorRegistryBusinessObject();
			bizo.HVSDelayIntervalType = DelayIntervalTypeCodes.Codes.None;
			Assert(bizo.HVSDelayIntervalInfo.ReadOnly);
			bizo.CONDelayIntervalType = DelayIntervalTypeCodes.Codes.None;
			Assert(bizo.CONDelayIntervalInfo.ReadOnly);
			bizo.HVSDelayIntervalType = DelayIntervalTypeCodes.Codes.DAR;
			Assert(!bizo.HVSDelayIntervalInfo.ReadOnly);
			bizo.CONDelayIntervalType = DelayIntervalTypeCodes.Codes.DAR;
			Assert(!bizo.CONDelayIntervalInfo.ReadOnly);
		}

		[ExpectNoExceptions]
		public void TestDeserialise_LegacyData()
		{
			var dummyDataType = new DummyNonPersistentBusinessObjectRegistryDataType(ExpectedBusinessObjectType);
			var legacyData = Encoding.Unicode.GetBytes(@"<?xml version=""1.0"" encoding=""utf-16""?>
<DelayFactorRegistryBusinessObject>
    <HVSDelayInterval>5</HVSDelayInterval>
    <HVSDelayIntervalType>NON</HVSDelayIntervalType>
    <LVSDelayInterval>7</LVSDelayInterval>
    <LVSDelayIntervalType>DAR</LVSDelayIntervalType>
    <CONDelayInterval>6</CONDelayInterval>
    <CONDelayIntervalType>DAY</CONDelayIntervalType>
</DelayFactorRegistryBusinessObject>");
			var expectedBO = GetBusinessObjectToSerialise();
			var newBusinessObject = (DelayFactorRegistryBusinessObject)dummyDataType.Deserialise(legacyData);
			CheckAllPropertiesAreEqual(expectedBO, newBusinessObject, isClone: false);
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DelayFactorRegistryBusinessObject(5, DelayIntervalTypeCodes.Codes.None, 6, DelayIntervalTypeCodes.Codes.DAY);
		}

		protected override DelayFactorRegistryBusinessObject GetBusinessObjectToClone()
		{
			return (DelayFactorRegistryBusinessObject)GetNewBusinessObject();
		}

		protected override DelayFactorRegistryBusinessObject GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
