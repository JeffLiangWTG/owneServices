using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.RefLocoMap.Testing
{
	[TestedType(typeof(UPERefLocoMap))]
	internal class UPERefLocoMapTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCustomisationsLoadedForRegistry()
		{
			ErrorReporter.Clear();
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.New<UPERefLocoMap>();
			AssertEquals("", ErrorReporter.LastMessageReported);
		}

		public void TestUPSCodeValid_US()
		{
			unloco.RL_Code = "USAA1";
			unloco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			AssertUPSCodeValidCore(unloco, UPEUSLocoMapSystemUsageList.Codes.Ups);
		}

		public void TestUPSCodeValid_AU()
		{
			unloco.RL_Code = "AUAA1";
			unloco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			AssertUPSCodeValidCore(unloco, UPEAirSeaMailSystemUsageList.Codes.Ups);
		}

		public void TestUPSCodeValid_IS()
		{
			unloco.RL_Code = "ISAA1";
			unloco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Iceland;
			AssertUPSCodeValidCore(unloco, UPEISLocoMapSystemUsageList.Codes.Ups);
		}

		public void TestUPSCodeValid_SG()
		{
			unloco.RL_Code = "SGAA1";
			unloco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
			AssertUPSCodeValidCore(unloco, UPESGLocoMapSystemUsageList.Codes.Ups);
		}

		public void TestUPSCodeValid_Other()
		{
			RefUNLOCO unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "ERAA1";
			unloco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Eritrea;
			AssertUPSCodeValidCore(unloco, UPEOtherLocoMapSystemUsageList.Codes.Ups);
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			unloco = Factory.New<RefUNLOCO>();
			base.SetUp();
		}
		RefUNLOCO unloco;

		void AssertUPSCodeValidCore(RefUNLOCO unloco, string code)
		{
			Assert("Precondition", !LocoMap.HasErrors);

			LocoMap.RY_RL_NKLocoPort = unloco.RL_Code;
			LocoMap.RY_RN = unloco.Country.PK;
			LocoMap.RY_SystemUsage = code;
			Assert("UPS code is valid", !LocoMap.HasErrors);

			LocoMap.RY_SystemUsage = "ABC";
			Assert("ABC code is invalid", LocoMap.HasErrors);
		}

		UPERefLocoMap LocoMap
		{
			get { return locoMap ?? (locoMap = Factory.New<UPERefLocoMap>()); }
		}
		UPERefLocoMap locoMap;
	}
}

