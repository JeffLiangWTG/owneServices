using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Testing.JobInvoicing.GatewayBilling
{
	public class GatewayDepartmentDefaultingTest : TestCaseWithFactory
	{
		public void TestGatewayDepartmentDefaultingRegistryDuplicates()
		{
			var era = Factory.NewWithValidTestData<GlbDepartment>();
			era.GE_Code = "ERA";

			var ila = Factory.NewWithValidTestData<GlbDepartment>();
			ila.GE_Code = "ILA";

			Factory.Save();

			var ilc = Factory.NewWithValidTestData<GlbDepartment>();
			ilc.GE_Code = "ILC";

			AssertExceptionThrown<RegistryValidationException>("Duplicate with GER", "Direction: Configuration with identical criteria already exists.", () => TestObjectCreator.SetupGatewayDepartmentDefaultingRegistry(OrgConstants.ServiceDirection.Code.Export, Constants.TransportModes.Road, "ALL", era.PK));
			AssertExceptionThrown<RegistryValidationException>("Dept invalid", "Department: Enter a valid selection.", () => TestObjectCreator.SetupGatewayDepartmentDefaultingRegistry(OrgConstants.ServiceDirection.Code.Import, Constants.TransportModes.All, Constants.AgentType.CoLoad, ilc.PK));
			AssertExceptionThrown<RegistryValidationException>("Direction invalid", "Direction: Enter a valid selection.", () => TestObjectCreator.SetupGatewayDepartmentDefaultingRegistry("BLA", Constants.TransportModes.All, Constants.AgentType.CoLoad, ila.PK));
			AssertExceptionThrown<RegistryValidationException>("Transport Mode invalid", "Transport Mode: Enter a valid selection.", () => TestObjectCreator.SetupGatewayDepartmentDefaultingRegistry(OrgConstants.ServiceDirection.Code.Import, "BLA", Constants.AgentType.CoLoad, ila.PK));
		}

		public void TestGatewayConsolDefaultDepartments()
		{
			var caa = Factory.NewWithValidTestData<GlbDepartment>();
			caa.GE_Code = "CAA";

			var dra = Factory.NewWithValidTestData<GlbDepartment>();
			dra.GE_Code = "DRA";

			var eaa = Factory.NewWithValidTestData<GlbDepartment>();
			eaa.GE_Code = "EAA";

			var dlc = Factory.NewWithValidTestData<GlbDepartment>();
			dlc.GE_Code = "DLC";

			var eac = Factory.NewWithValidTestData<GlbDepartment>();
			eac.GE_Code = "EAC";

			var iac = Factory.NewWithValidTestData<GlbDepartment>();
			iac.GE_Code = "IAC";

			var asc = Factory.NewWithValidTestData<GlbDepartment>();
			asc.GE_Code = "ASC";

			Factory.Save();

			var gda = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "GDA");
			var gdl = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "GDL");
			var gdr = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "GDR");
			var gds = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "GDS");
			var gea = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "GEA");
			var gel = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "GEL");
			var ger = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "GER");
			var ges = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "GES");
			var gia = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "GIA");
			var gil = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "GIL");
			var gir = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "GIR");
			var gis = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "GIS");

			CombineAssertions(() =>
			{
				assertDefaultGTWDept(Constants.AgentType.Agent, true, true, "AIR", gda);
				assertDefaultGTWDept(Constants.AgentType.Agent, true, true, "RAI", gdl);
				assertDefaultGTWDept(Constants.AgentType.Agent, true, true, "ROA", gdr);
				assertDefaultGTWDept(Constants.AgentType.Agent, true, true, "SEA", gds);
				assertDefaultGTWDept(Constants.AgentType.Agent, true, false, "AIR", gea);
				assertDefaultGTWDept(Constants.AgentType.Agent, true, false, "RAI", gel);
				assertDefaultGTWDept(Constants.AgentType.Agent, true, false, "ROA", ger);
				assertDefaultGTWDept(Constants.AgentType.Agent, true, false, "SEA", ges);
				assertDefaultGTWDept(Constants.AgentType.Agent, false, true, "AIR", gia);
				assertDefaultGTWDept(Constants.AgentType.Agent, false, true, "RAI", gil);
				assertDefaultGTWDept(Constants.AgentType.Agent, false, true, "ROA", gir);
				assertDefaultGTWDept(Constants.AgentType.Agent, false, true, "SEA", gis);
				assertDefaultGTWDept(Constants.AgentType.Agent, false, false, "AIR", gia);
				assertDefaultGTWDept(Constants.AgentType.Agent, false, false, "RAI", gil);
				assertDefaultGTWDept(Constants.AgentType.Agent, false, false, "ROA", gir);
				assertDefaultGTWDept(Constants.AgentType.Agent, false, false, "SEA", gis);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, true, true, "AIR", gda);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, true, true, "RAI", gdl);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, true, true, "ROA", gdr);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, true, true, "SEA", gds);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, true, false, "AIR", gea);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, true, false, "RAI", gel);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, true, false, "ROA", ger);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, true, false, "SEA", ges);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, false, true, "AIR", gia);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, false, true, "RAI", gil);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, false, true, "ROA", gir);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, false, true, "SEA", gis);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, false, false, "AIR", gia);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, false, false, "RAI", gil);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, false, false, "ROA", gir);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, false, false, "SEA", gis);
			});

			TestObjectCreator.SetupGatewayDepartmentDefaultingRegistry(Constants.FreightShipmentDirection.Code.Other, Constants.TransportModes.Air, Constants.AgentType.Agent, caa.PK);
			TestObjectCreator.SetupGatewayDepartmentDefaultingRegistry(Constants.FreightShipmentDirection.Code.Domestic, Constants.TransportModes.Road, Constants.AgentType.Agent, dra.PK);
			TestObjectCreator.SetupGatewayDepartmentDefaultingRegistry(Constants.FreightShipmentDirection.Code.Export, Constants.TransportModes.Air, Constants.AgentType.Agent, eaa.PK);
			TestObjectCreator.SetupGatewayDepartmentDefaultingRegistry(Constants.FreightShipmentDirection.Code.Domestic, Constants.TransportModes.Rail, Constants.AgentType.CoLoad, dlc.PK);
			TestObjectCreator.SetupGatewayDepartmentDefaultingRegistry(Constants.FreightShipmentDirection.Code.Export, Constants.TransportModes.All, Constants.AgentType.CoLoad, eac.PK);        //ger takes precedence over eac
			TestObjectCreator.SetupGatewayDepartmentDefaultingRegistry(Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.AgentType.CoLoad, iac.PK);
			TestObjectCreator.SetupGatewayDepartmentDefaultingRegistry(Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.Sea, Constants.AgentType.CoLoad, asc.PK);			//gis takes precedence over asc

			CombineAssertions(() =>
			{
				assertDefaultGTWDept(Constants.AgentType.Agent, true, true, "AIR", gda);
				assertDefaultGTWDept(Constants.AgentType.Agent, true, true, "RAI", gdl);
				assertDefaultGTWDept(Constants.AgentType.Agent, true, true, "ROA", dra);
				assertDefaultGTWDept(Constants.AgentType.Agent, true, true, "SEA", gds);
				assertDefaultGTWDept(Constants.AgentType.Agent, true, false, "AIR", eaa);
				assertDefaultGTWDept(Constants.AgentType.Agent, true, false, "RAI", gel);
				assertDefaultGTWDept(Constants.AgentType.Agent, true, false, "ROA", ger);
				assertDefaultGTWDept(Constants.AgentType.Agent, true, false, "SEA", ges);
				assertDefaultGTWDept(Constants.AgentType.Agent, false, true, "AIR", gia);
				assertDefaultGTWDept(Constants.AgentType.Agent, false, true, "RAI", gil);
				assertDefaultGTWDept(Constants.AgentType.Agent, false, true, "ROA", gir);
				assertDefaultGTWDept(Constants.AgentType.Agent, false, true, "SEA", gis);
				assertDefaultGTWDept(Constants.AgentType.Agent, false, false, "AIR", caa);
				assertDefaultGTWDept(Constants.AgentType.Agent, false, false, "RAI", gil);
				assertDefaultGTWDept(Constants.AgentType.Agent, false, false, "ROA", gir);
				assertDefaultGTWDept(Constants.AgentType.Agent, false, false, "SEA", gis);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, true, true, "AIR", gda);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, true, true, "RAI", dlc);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, true, true, "ROA", gdr);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, true, true, "SEA", gds);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, true, false, "AIR", gea);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, true, false, "RAI", gel);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, true, false, "ROA", ger);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, true, false, "SEA", ges);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, false, true, "AIR", iac);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, false, true, "RAI", gil);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, false, true, "ROA", gir);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, false, true, "SEA", gis);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, false, false, "AIR", gia);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, false, false, "RAI", gil);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, false, false, "ROA", gir);
				assertDefaultGTWDept(Constants.AgentType.CoLoad, false, false, "SEA", gis);
			});

			void assertDefaultGTWDept(string consolType, bool loadLocal, bool dischargeLocal, string transportMode, GlbDepartment expectedDept)
			{
				var creator = new TestObjectCreator(Factory);

				var org = loadLocal ? "AUSYD" : "USLAX";
				var dest = dischargeLocal ? "AUBNE" : "SGSIN";

				var consol = creator.CreateGatewayConsol(org, dest, receivingGatewayCompany: GlbCompany.CurrentCompany, transportMode: transportMode, consolType: consolType);
				using (var consolJobHeader = creator.CreateJob(consol, setCurrentDepartment: false))
				{
					AssertEquals($"{consolType}-{org}-{dest}-{transportMode}", expectedDept.GE_Code, consolJobHeader.Department.GE_Code);
				}
			}
		}
	}
}
