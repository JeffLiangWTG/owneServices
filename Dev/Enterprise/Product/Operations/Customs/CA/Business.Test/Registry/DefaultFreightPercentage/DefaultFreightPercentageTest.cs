using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Registry.Testing
{
	[TestedType(typeof(DefaultFreightPercentage))]
	sealed class DefaultFreightPercentageTest : Enterprise.Registry.Business.Testing.RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidateModeofTransport()
		{
			BizObj.RunPreSaveValidation();
			AssertEquals("ModeofTransport should not have have any errors", true, BizObj.ModeofTransportInfo.HasErrors());

			BizObj.ModeofTransport = "XXX";
			AssertEquals("ModeofTransport should have errors", true, BizObj.ModeofTransportInfo.HasErrors());

			BizObj.ModeofTransport = "SEA";
			AssertEquals("ModeofTransport should not have any errors", false, BizObj.ModeofTransportInfo.HasErrors());

			AssertEquals(3, BizObj.ModeofTransportInfo.MaxLength);
		}

		public void TestValidateFreightPercentage()
		{
			BizObj.RunPreSaveValidation();
			Assert("FreightPercentage should not have errors", !BizObj.FreightPercentageInfo.HasErrors());

			BizObj.FreightPercentage = 0.12m;
			Assert("FreightPercentage should not have any errors", !BizObj.FreightPercentageInfo.HasErrors());
		}

		#region Test ZPropertyInfos

		public void TestNewZPropertyInfos()
		{
			TestZPropertyInfo(BizObj.ModeofTransportInfo, DefaultFreightPercentage.Schema.ModeofTransport);
			TestZPropertyInfo(BizObj.FreightPercentageInfo, DefaultFreightPercentage.Schema.FreightPercentage);
		}

		void TestZPropertyInfo(ZPropertyInfo propertyInfo, string expectedName)
		{
			AssertNotNull("ZPropertyInfo for " + propertyInfo.Name + " was null", propertyInfo);
			AssertEquals("PropertyInfo.Name", expectedName, propertyInfo.Name);
		}

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			BizObj.ModeofTransport = "ROA";
			BizObj.FreightPercentage = 0.12m;

			return BizObj;
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		new DefaultFreightPercentage BizObj
		{
			get { return (DefaultFreightPercentage)base.BizObj; }
		}

		#endregion
	}
}
