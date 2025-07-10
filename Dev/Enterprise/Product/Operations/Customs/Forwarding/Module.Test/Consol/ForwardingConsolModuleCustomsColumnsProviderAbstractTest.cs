using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.Forwarding.GUI;
using Enterprise.Customs.Forwarding.GUI.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Forwarding.Module.Testing
{
	abstract class ForwardingConsolModuleCustomsColumnsProviderAbstractTest : GridCustomColumnsProviderAbstractTest<ForwardingConsol>
	{
		protected ZGridColumnInfo FindColumnByName(DumyZGrid grid, string columnName)
		{
			return (from columnStyleInfo in grid.ColumnStyles.Cast<ZGridColumnInfo>()
					where columnStyleInfo.ColumnName == columnName
					select columnStyleInfo).FirstOrDefault();
		}

		protected override SchemaPKColumn PkColumn => JobConsolSchema.PK;

		protected override GridCustomColumnsProvider GetGridCustomColumnsProvider()
		{
			return new ForwardingConsolModuleCustomsColumnsProvider();
		}

		protected override ZGuid[] CreateKeysForTest()
		{
			var result = new List<ZGuid>();
			var factory = new BusinessObjectFactory();

			var refContainer = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			for (var i = 0; i < 12; i++)
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "vessel" + i;

				var voyage = factory.New<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "voyage" + i;
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
				voyage.GenerateSailings();

				var consol = factory.New<ForwardingConsol>();

				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "TEST2017";
				container.JC_RC = refContainer.PK;

				var transport = consol.Transports.AddNew();
				transport.JW_IsLinked = true;
				transport.JW_JX = voyage.Sailings[0].PK;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_UniqueConsignRef = "shipment" + i;

				var job = new JobHeader.Loader(shipment).TryCreate();
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_GB = GlbBranch.CurrentBranch.PK;

				var outerPackline = shipment.OuterPackLines.AddNew();
				outerPackline.SetContainer(consol, container);

				var milestone = consol.WorkflowItems.Milestones.AddNew();
				milestone.SetMilestoneActualDateForTest(ZDateTime.Now);
				milestone.P9_GC = GlbCompany.CurrentCompany.PK;

				milestone = consol.WorkflowItems.Milestones.AddNew();
				milestone.SetMilestoneActualDateForTest(ZDateTime.Invalid);
				milestone.P9_GC = GlbCompany.CurrentCompany.PK;

				result.Add(consol.PK);
			}

			factory.Save();

			return result.ToArray();
		}
	}
}
