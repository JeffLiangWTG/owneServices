using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CustomsCodesDefaulterTest : TestCaseWithFactory
	{
		public void TestDefaultCustomsCode_OfficeCode()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			bizObj.Z0_Code = "";
			var defaulter = new CustomsCodesDefaulter(Factory, bizObj.Z0_CodeInfo, () => { return new[] { address }; },
				() => { return new[] { transport1 }; }, () => { return Core.Constants.TransportModes.Sea; }, OrgCusCode.CACodeTypes.CustomsOfficeCode, true);
			defaulter.DefaultCustomsCode();
			AssertEquals("Default from org when only one leg", "0001", bizObj.Z0_Code);

			bizObj = Factory.New<DummyBusinessObject>();
			bizObj.Z0_Code = "";
			defaulter = new CustomsCodesDefaulter(Factory, bizObj.Z0_CodeInfo, () => { return new[] { address }; },
				() => { return new[] { transport1, transport2 }; }, () => { return Core.Constants.TransportModes.Air; }, OrgCusCode.CACodeTypes.CustomsOfficeCode, true);
			defaulter.DefaultCustomsCode();
			AssertEquals("Default from org when multiple legs and transport mode not sea", "0001", bizObj.Z0_Code);

			bizObj = Factory.New<DummyBusinessObject>();
			bizObj.Z0_Code = "";
			defaulter = new CustomsCodesDefaulter(Factory, bizObj.Z0_CodeInfo, () => { return new[] { address }; },
				() => { return new[] { transport1, transport2 }; }, () => { return Core.Constants.TransportModes.Sea; }, OrgCusCode.CACodeTypes.CustomsOfficeCode, false);
			defaulter.DefaultCustomsCode();
			AssertEquals("Do not default when fallbackUNLOCO is false", "", bizObj.Z0_Code);

			bizObj = Factory.New<DummyBusinessObject>();
			bizObj.Z0_Code = "";
			defaulter = new CustomsCodesDefaulter(Factory, bizObj.Z0_CodeInfo, () => { return new[] { address }; },
				() => { return new[] { transport1, transport2 }; }, () => { return Core.Constants.TransportModes.Sea; }, OrgCusCode.CACodeTypes.CustomsOfficeCode, true);
			defaulter.DefaultCustomsCode();
			AssertEquals("Default from UNLOCO", "TTGT", bizObj.Z0_Code);
		}

		public void TestDefaultCustomsCode_SubLocation()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			bizObj.Z0_Code = "";
			var defaulter = new CustomsCodesDefaulter(Factory, bizObj.Z0_CodeInfo, () => { return new[] { address }; },
				() => { return new[] { transport1 }; }, () => { return Core.Constants.TransportModes.Sea; }, OrgCusCode.CodeTypes.ControlledPremisesID, true);
			defaulter.DefaultCustomsCode();
			AssertEquals("Default from org when only one leg", "0002", bizObj.Z0_Code);

			bizObj = Factory.New<DummyBusinessObject>();
			bizObj.Z0_Code = "";
			defaulter = new CustomsCodesDefaulter(Factory, bizObj.Z0_CodeInfo, () => { return new[] { address }; },
				() => { return new[] { transport1, transport2 }; }, () => { return Core.Constants.TransportModes.Air; }, OrgCusCode.CodeTypes.ControlledPremisesID, true);
			defaulter.DefaultCustomsCode();
			AssertEquals("Default from org when multiple legs and transport mode not sea", "0002", bizObj.Z0_Code);

			bizObj = Factory.New<DummyBusinessObject>();
			bizObj.Z0_Code = "";
			defaulter = new CustomsCodesDefaulter(Factory, bizObj.Z0_CodeInfo, () => { return new[] { address }; },
				() => { return new[] { transport1, transport2 }; }, () => { return Core.Constants.TransportModes.Sea; }, OrgCusCode.CodeTypes.ControlledPremisesID, false);
			defaulter.DefaultCustomsCode();
			AssertEquals("Do not default when fallbackUNLOCO is false", "", bizObj.Z0_Code);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new DeclarationTestHelper(Factory, true);
			var canada = Factory.Load<RefCountry>(Enterprise.Core.Constants.CountryGuids.Canada);
			var depot = helper.CreateOrganisation("DPT", "DEPOT NAME", "CATOR", "DEPOT ADDRESS", "DEPOT CITY", "123 4567");
			var depotCOC = depot.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.CustomsOfficeCode, "0001", canada);
			depotCOC.OK_OA_PremisesAddress = depot.MainAddress.PK;
			var depotCCP = depot.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "0002", canada);
			depotCCP.OK_OA_PremisesAddress = depot.MainAddress.PK;
			address = depot.MainAddress;

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			transport1 = dec.TransportsIncludingRelated.AddNew();
			SetUpTransport(transport1, "USPHL", "CABLO", Core.Constants.TransportModes.Sea);
			transport2 = dec.TransportsIncludingRelated.AddNew();
			SetUpTransport(transport2, "CABLO", "CATTG", Core.Constants.TransportModes.Sea);

			var loco = Factory.New<RefUNLOCO>();
			loco.RL_Code = "CATTG";
			var match = loco.RefLocoMaps.AddNew();
			match.RY_RN = Core.Constants.CountryGuids.Canada;
			match.RY_LocalPortCode = "TTGTE";
			match.RY_SystemUsage = CALocoMapSystemUsageList.Codes.All;
		}

		void SetUpTransport(Transport transaction, ZString startPort, ZString discPort, ZString transportMode)
		{
			transaction.JW_RL_NKLoadPort = startPort;
			transaction.JW_RL_NKDiscPort = discPort;
			transaction.JW_ETD = ZDateTime.Today;
			transaction.JW_TransportMode = transportMode;
		}

		OrgAddress address;
		Transport transport1;
		Transport transport2;
	}
}
