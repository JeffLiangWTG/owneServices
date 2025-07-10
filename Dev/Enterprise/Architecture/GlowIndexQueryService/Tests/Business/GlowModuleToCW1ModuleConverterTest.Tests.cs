using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace GlowIndexQueryService.Tests.Business
{
	public class GlowModuleToCW1ModuleConverterTest : TestCase
	{
		public void TestConvertEntityTypeToTableCode_WhenEntityTypeIsDerivedFromHierarchy_ShouldReturnBasedEntityTypeTableCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("LTC", GlowModuleToCW1ModuleConverter.ConvertEntityTypeToTableCode("IDtbLandTransportConsignment"));
				AssertEquals("LTC", GlowModuleToCW1ModuleConverter.ConvertEntityTypeToTableCode("IDtbCarrierBookingConsignment"));
				AssertEquals("LTS", GlowModuleToCW1ModuleConverter.ConvertEntityTypeToTableCode("IDtbConsignmentDeliveryAddress"));
				AssertEquals("LTS", GlowModuleToCW1ModuleConverter.ConvertEntityTypeToTableCode("IDtbConsignmentMultiAddress"));
				AssertEquals("LTS", GlowModuleToCW1ModuleConverter.ConvertEntityTypeToTableCode("IDtbConsignmentPickupAddress"));
			});
		}

		public void TestConvertEntityTypeToTableCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("JS", GlowModuleToCW1ModuleConverter.ConvertEntityTypeToTableCode("IJobShipment"));
				AssertEquals("B0", GlowModuleToCW1ModuleConverter.ConvertEntityTypeToTableCode("ICusInBondOceanBill"));
				AssertEquals("GS", GlowModuleToCW1ModuleConverter.ConvertEntityTypeToTableCode("IGlbStaff"));
				AssertEquals("JK", GlowModuleToCW1ModuleConverter.ConvertEntityTypeToTableCode("IJobConsol"));
				AssertEquals("US", GlowModuleToCW1ModuleConverter.ConvertEntityTypeToTableCode("IUSCCarrierAndFIRM"));
				AssertEquals(null, GlowModuleToCW1ModuleConverter.ConvertEntityTypeToTableCode("abcfooblah"));
				AssertEquals("S5", GlowModuleToCW1ModuleConverter.ConvertEntityTypeToTableCode("IStmScheduleTask.Of.IStmMenuItem"));
				AssertEquals("S5", GlowModuleToCW1ModuleConverter.ConvertEntityTypeToTableCode("IStmScheduleTask[[IStmMenuItem]]"));
			});
		}

		[UseSnapshotProtection]
		public void TestConvertEntityToBizo()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "testStaff";
			staff.GS_Code = "STF";
			factory.Save();

			// act
			var staff2 = (GlbStaff)GlowModuleToCW1ModuleConverter.ConvertEntityToBizo(staff.PK.ToString(), "IGlbStaff");

			// assert
			AssertEquals(staff.PK, staff2.PK);
			AssertEquals(staff.GS_FullName, staff2.GS_FullName);
		}

		[UseSnapshotProtection]
		public void TestConvertEntityToBizo_GlowEntityDoesntExist()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "testStaff";
			staff.GS_Code = "STF";
			factory.Save();

			// act
			var bizo = GlowModuleToCW1ModuleConverter.ConvertEntityToBizo(staff.PK.ToString(), "abcfooblah");

			// assert
			AssertNull(bizo);
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestConvertEntityToBizo_WithDisposableAction()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "testStaff";
			staff.GS_Code = "STF";
			factory.Save();

			_ = GlowModuleToCW1ModuleConverter.ConvertEntityToBizo(staff.PK.ToString(), "IGlbStaff");
			var thread = new Thread(() => GlowModuleToCW1ModuleConverter.ConvertEntityToBizo(staff.PK.ToString(), "IGlbStaff"));
			thread.Start();
			thread.Join();
		}

		public void TestConvertModuleIdentifierToEntityType()
		{
			using (new GlowIndexQueryEngineMock(mock =>
			{
				_ = mock.Setup(e => e.GetGlowEntityTypes()).Returns(new HashSet<string>() { "IDummyBusinessObject", "IStmScheduleTask.Of.IStmMenuItem" });
			}))
			{
				AssertEquals("IDummyBusinessObject", GlowModuleToCW1ModuleConverter.ConvertModuleIdentifierToEntityType(DummyModuleIDs.Dummy));
				AssertEquals("IStmScheduleTask[[IStmMenuItem]]", GlowModuleToCW1ModuleConverter.ConvertModuleIdentifierToEntityType(ModuleIDs.ReportManagement));
				AssertEquals("IStmScheduleTask[[IStmMenuItem]]", GlowModuleToCW1ModuleConverter.ConvertModuleIdentifierToEntityType(ModuleIDs.ScheduledReports));
			}
		}

		public void TestGetAllModuleIDs()
		{
			using (new GlowIndexQueryEngineMock(mock =>
			{
				_ = mock.Setup(e => e.GetGlowEntityTypes()).Returns(new HashSet<string>() { "IDummyBusinessObject", "IStmScheduleTask.Of.IStmMenuItem" });
			}))
			{
				var allModuleIDs = GlowModuleToCW1ModuleConverter.GetAllModuleIDs();
				AssertContainsExactElementsInAnyOrder(
					[DummyModuleIDs.Dummy, DummyModuleIDs.DummyNoPopup, DummyModuleIDs.DummyWithTemplates, DummyModuleIDs.DummyThatHitsFilterBizoOnDispose, ModuleIDs.ReportManagement, ModuleIDs.ScheduledReports],
					allModuleIDs);
			}
		}

		public void TestCheckIsVerifiedModule()
		{
			var allModuleIDs = GlowModuleToCW1ModuleConverter.GetAllModuleIDs();
			var verifiedModuleList = new[]
			{
				ModuleIDs.GlbBranch,
				ModuleIDs.Project,
				ModuleIDs.WorkItem,
				ModuleIDs.GlbCapability,
				ModuleIDs.ReportStatistics,
				ModuleIDs.BMControlCustomisation,
				ModuleIDs.AcceptabilityBand,
				ModuleIDs.GlbDepartment,
				ModuleIDs.SalesEnquiry,
				ModuleIDs.ReportManagement,
				ModuleIDs.ProcessHeader,
				ModuleIDs.BMSystems,
				ModuleIDs.ProcessTasks,
				ModuleIDs.GlbCompany,
				ModuleIDs.GlbStaff,
				ModuleIDs.ScheduledReports,
				ModuleIDs.UniversalCopySchedule,
			};
			CombineAssertions("All modules in list should be verified", () =>
				{
					foreach (var moduleId in verifiedModuleList)
					{
						Assert(GlowModuleToCW1ModuleConverter.CheckIsVerifiedModule(moduleId));
					}
				}
			);

			var notVerifiedModuleIDs = allModuleIDs.Except(verifiedModuleList);
			CombineAssertions("Others modules not in list should not be verified", () =>
				{
					foreach (var moduleId in notVerifiedModuleIDs)
					{
						Assert(!GlowModuleToCW1ModuleConverter.CheckIsVerifiedModule(moduleId));
					}
				}
			);

			Assert(!GlowModuleToCW1ModuleConverter.CheckIsVerifiedModule(null));
			Assert(!GlowModuleToCW1ModuleConverter.CheckIsVerifiedModule(DummyModuleIDs.Dummy));
		}
	}
}
