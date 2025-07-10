using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class UNLOCODefaulterTest : TestCaseWithFactory
	{
		public void TestDefaultUNLOCOCode()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			bizObj.Z0_Code = "";
			var defaulter = new UNLOCODefaulter(Factory, () => bizObj.Z0_CodeInfo, null);
			defaulter.DefaultUNLOCOCode("LOCAL");
			AssertEquals("!ZZ", bizObj.Z0_Code);

			bizObj.Z0_Code = "~~";
			defaulter.DefaultUNLOCOCode("LOCAL", false);
			AssertEquals("!ZZ", bizObj.Z0_Code);
		}

		public void TestDefaultLocalCode_OfficeCode()
		{
			using (CACustomsDataRegistry.Instance.ShouldDefaultCustomsCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var bizObj = Factory.New<DummyBusinessObject>();
				var defaulter = new UNLOCODefaulter(Factory, () => bizObj.Z0_CodeInfo, () => Core.Constants.TransportModes.Sea);
				bizObj.Z0_Code = "!ZZ";
				bizObj.Z0_Description = "";
				defaulter.DefaultCustomsCode(() => bizObj.Z0_DescriptionInfo, CACustomsCodeType.Office);
				AssertEquals("LOCAL", bizObj.Z0_Description);

				bizObj = Factory.New<DummyBusinessObject>();
				defaulter = new UNLOCODefaulter(Factory, () => bizObj.Z0_CodeInfo, () => Core.Constants.TransportModes.Air);
				bizObj.Z0_Code = "!ZZ";
				bizObj.Z0_Description = "";
				defaulter.DefaultCustomsCode(() => bizObj.Z0_DescriptionInfo, CACustomsCodeType.Office);
				AssertEquals("Sea usage shouldn't be defaulted to Air transport", ZString.Empty, bizObj.Z0_Description);

				bizObj = Factory.New<DummyBusinessObject>();
				defaulter = new UNLOCODefaulter(Factory, () => bizObj.Z0_CodeInfo, () => Core.Constants.TransportModes.Sea);
				bizObj.Z0_Code = "!ZZ";
				bizObj.Z0_Description = "TSTS";
				defaulter.DefaultCustomsCode(() => bizObj.Z0_DescriptionInfo, CACustomsCodeType.Office, false);
				AssertEquals("Override existing value", "LOCAL", bizObj.Z0_Description);

				bizObj = Factory.New<DummyBusinessObject>();
				defaulter = new UNLOCODefaulter(Factory, () => bizObj.Z0_CodeInfo, () => Core.Constants.TransportModes.Sea);
				bizObj.Z0_Code = "!ZZ";
				bizObj.Z0_Description = "TSTS";
				defaulter.DefaultCustomsCode(() => bizObj.Z0_DescriptionInfo, CACustomsCodeType.Office);
				AssertEquals("Not override existing value", "TSTS", bizObj.Z0_Description);
			}

			using (CACustomsDataRegistry.Instance.ShouldDefaultCustomsCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var bizObj = Factory.New<DummyBusinessObject>();
				var defaulter = new UNLOCODefaulter(Factory, () => bizObj.Z0_CodeInfo, () => Core.Constants.TransportModes.Sea);
				bizObj.Z0_Code = "!ZZ";
				bizObj.Z0_Description = "";
				defaulter.DefaultCustomsCode(() => bizObj.Z0_DescriptionInfo, CACustomsCodeType.Office);
				AssertEquals("", bizObj.Z0_Description);
			}
		}

		public void TestDefaultLocalCode_SubLocation()
		{
			using (CACustomsDataRegistry.Instance.ShouldDefaultCustomsCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var bizObj = Factory.New<DummyBusinessObject>();
				var defaulter = new UNLOCODefaulter(Factory, () => bizObj.Z0_CodeInfo, () => Core.Constants.TransportModes.Sea);
				bizObj.Z0_Code = "!ZZ";
				bizObj.Z0_Description = "";
				defaulter.DefaultCustomsCode(() => bizObj.Z0_DescriptionInfo, CACustomsCodeType.SubLocation);
				AssertEquals("SUB1", bizObj.Z0_Description);

				bizObj = Factory.New<DummyBusinessObject>();
				defaulter = new UNLOCODefaulter(Factory, () => bizObj.Z0_CodeInfo, () => Core.Constants.TransportModes.Air);
				bizObj.Z0_Code = "!ZZ";
				bizObj.Z0_Description = "";
				defaulter.DefaultCustomsCode(() => bizObj.Z0_DescriptionInfo, CACustomsCodeType.SubLocation);
				AssertEquals("SUB1", bizObj.Z0_Description);

				bizObj = Factory.New<DummyBusinessObject>();
				defaulter = new UNLOCODefaulter(Factory, () => bizObj.Z0_CodeInfo, () => Core.Constants.TransportModes.Sea);
				bizObj.Z0_Code = "!ZZ";
				bizObj.Z0_Description = "TSTS";
				defaulter.DefaultCustomsCode(() => bizObj.Z0_DescriptionInfo, CACustomsCodeType.SubLocation, false);
				AssertEquals("Override existing value", "SUB1", bizObj.Z0_Description);

				bizObj = Factory.New<DummyBusinessObject>();
				defaulter = new UNLOCODefaulter(Factory, () => bizObj.Z0_CodeInfo, () => Core.Constants.TransportModes.Sea);
				bizObj.Z0_Code = "!ZZ";
				bizObj.Z0_Description = "TSTS";
				defaulter.DefaultCustomsCode(() => bizObj.Z0_DescriptionInfo, CACustomsCodeType.SubLocation);
				AssertEquals("Not override existing value", "TSTS", bizObj.Z0_Description);
			}

			using (CACustomsDataRegistry.Instance.ShouldDefaultCustomsCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var bizObj = Factory.New<DummyBusinessObject>();
				var defaulter = new UNLOCODefaulter(Factory, () => bizObj.Z0_CodeInfo, () => Core.Constants.TransportModes.Sea);
				bizObj.Z0_Code = "!ZZ";
				bizObj.Z0_Description = "";
				defaulter.DefaultCustomsCode(() => bizObj.Z0_DescriptionInfo, CACustomsCodeType.SubLocation);
				AssertEquals("", bizObj.Z0_Description);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var timeZoneSet = Factory.NewWithValidTestData<RefTimeZoneSet>();
			timeZoneSet.HasDaylightSavings = true;

			var unlocoZ = Factory.New<RefUNLOCO>();
			unlocoZ.RL_Code = "!ZZ";
			unlocoZ.RL_R3 = timeZoneSet.PK;
			var match = unlocoZ.RefLocoMaps.AddNew();
			match.RY_RN = Core.Constants.CountryGuids.Canada;
			match.RY_LocalPortCode = "LOCAL";
			match.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Sea;

			var match2 = unlocoZ.RefLocoMaps.AddNew();
			match2.RY_RN = Core.Constants.CountryGuids.Canada;
			match2.RY_LocalPortCode = "SUB1";
			match2.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Sub;
		}
	}
}
