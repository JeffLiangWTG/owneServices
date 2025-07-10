using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(DefaultPremiseID))]
	sealed class DefaultPremiseIDTest : Registry.Business.Testing.RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidateAirlineCode()
		{
			BizObj.RunPreSaveValidation();
			AssertEquals("AirlineCode should have errors", true, BizObj.AirlineCodeInfo.HasErrors());

			BizObj.AirlineCode = "QF";
			AssertEquals("AirlineCode should not have any errors", false, BizObj.AirlineCodeInfo.HasErrors());

			AssertEquals(RefAirlineSchema.RM_TwoCharacterCode.MaxLength, BizObj.AirlineCodeInfo.MaxLength);
		}

		public void TestValidatePortOfDischarge()
		{
			BizObj.RunPreSaveValidation();
			AssertEquals("PortOfDischarge should not have have any errors", true, BizObj.PortOfDischargeInfo.HasErrors());

			BizObj.PortOfDischarge = "XXX";
			AssertEquals("PortOfDischarge should have errors", true, BizObj.PortOfDischargeInfo.HasErrors());

			BizObj.PortOfDischarge = "AUSYD";
			AssertEquals("PortOfDischarge should not have any errors", false, BizObj.PortOfDischargeInfo.HasErrors());

			AssertEquals(RefUNLOCOSchema.RL_Code.MaxLength, BizObj.PortOfDischargeInfo.MaxLength);
		}

		public void TestValidatePremiseID()
		{
			BizObj.RunPreSaveValidation();
			AssertEquals("PremiseID should have errors", true, BizObj.PremiseIDInfo.HasErrors());

			BizObj.PremiseID = "blah";
			AssertEquals("PremiseID should not have any errors", false, BizObj.PremiseIDInfo.HasErrors());

			AssertEquals(CusUnderbondSchema.C4_DischargePremiseID.MaxLength, BizObj.PremiseIDInfo.MaxLength);
		}

		#region Test ZPropertyInfos

		public void TestNewZPropertyInfos()
		{
			TestZPropertyInfo(BizObj.AirlineCodeInfo, DefaultPremiseID.Schema.AirlineCode, RefAirlineSchema.RM_TwoCharacterCode.MaxLength);
			TestZPropertyInfo(BizObj.PortOfDischargeInfo, DefaultPremiseID.Schema.PortOfDischarge);
			TestZPropertyInfo(BizObj.PremiseIDInfo, DefaultPremiseID.Schema.PremiseID, CusUnderbondSchema.C4_DischargePremiseID.MaxLength);
		}

		void TestZPropertyInfo(ZPropertyInfo propertyInfo, string expectedName)
		{
			AssertNotNull("ZPropertyInfo for " + propertyInfo.Name + " was null", propertyInfo);
			AssertEquals("PropertyInfo.Name", expectedName, propertyInfo.Name);
		}

		void TestZPropertyInfo(ZPropertyInfo propertyInfo, string expectedName, int expectedMaxLength)
		{
			TestZPropertyInfo(propertyInfo, expectedName);
			AssertEquals("PropertyInfo.MaxLength", expectedMaxLength, propertyInfo.MaxLength);
		}

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			BizObj.AirlineCode = "QF";
			BizObj.PortOfDischarge = "AUSYD";
			BizObj.PremiseID = "9914N";

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

		new DefaultPremiseID BizObj
		{
			get { return (DefaultPremiseID)base.BizObj; }
		}

		#endregion
	}
}
