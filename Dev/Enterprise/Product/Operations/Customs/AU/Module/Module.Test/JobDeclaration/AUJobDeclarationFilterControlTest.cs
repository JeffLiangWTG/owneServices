using System;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Module.Testing
{
	sealed class AUJobDeclarationFilterControlTest : Customs.Module.Testing.JobDeclarationFilterStripControlTest
	{
		public void TestInitializeAdditionalColumns()
		{
			using (ZForm form = new ZForm())
			using (var filterControl = new AUJobDeclarationFilterControl(null, DeclarationCollection, FilterBO))
			{
				form.Controls.Add(filterControl);
				form.Show();
				var grid = filterControl.FilteredGrid;
				AssertNotNull(grid.Columns[JobDeclaration.Schema.JE_ConsolidatedCargoStatus]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.JE_ScreeningStatus]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.ConsolidatedCargoStatusDescription]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.BrokerName]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.ImporterName]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.SupplierName]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.ForwarderName]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.AuditDate]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.AuditReference]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.AuditLogUser]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.AuditLogUserName]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.FreightContainerMode]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.JE_DateAtFinalDestination]);
				AssertEquals(JobDeclaration.Schema.JE_ConsolidatedCargoStatus, ((ZGridColumnInfo)grid.ColumnStyles[43]).ColumnName);
				AssertEquals(JobDeclaration.Schema.ConsolidatedCargoStatusDescription, ((ZGridColumnInfo)grid.ColumnStyles[44]).ColumnName);
				AssertEquals(JobDeclaration.Schema.WarehouseTransactionStatusDescription, ((ZGridColumnInfo)grid.ColumnStyles[77]).ColumnName);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.BillingBranch]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.BillingDepartment]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.BillingOperator]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.JE_ETAOfDischarge]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.JE_ETDOfLoading]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.JE_SystemCreateUser]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.JE_SystemCreateTimeUtc]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.JE_SystemLastEditUser]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.JE_SystemLastEditTimeUtc]);
				AssertNotNull(grid.Columns["WorkflowItems+MilestonesIncludingRelated+CurrentCompanyLastMilestone+P9_SE_NKMilestoneEvent"]);
				AssertNotNull(grid.Columns["WorkflowItems+MilestonesIncludingRelated+CurrentCompanyLastMilestone+DescriptionWithReference"]);
				AssertNotNull(grid.Columns["WorkflowItems+MilestonesIncludingRelated+CurrentCompanyLastMilestone+P9_ActualDate"]);
				AssertNotNull(grid.Columns["WorkflowItems+Milestones+CurrentCompanyLastMilestone+P9_SE_NKMilestoneEvent"]);
				AssertNotNull(grid.Columns["WorkflowItems+Milestones+CurrentCompanyLastMilestone+DescriptionWithReference"]);
				AssertNotNull(grid.Columns["WorkflowItems+Milestones+CurrentCompanyLastMilestone+P9_ActualDate"]);
				AssertNotNull(grid.Columns["WorkflowItems+MilestonesIncludingRelated+CurrentCompanyNextMilestone+P9_SE_NKMilestoneEvent"]);
				AssertNotNull(grid.Columns["WorkflowItems+MilestonesIncludingRelated+CurrentCompanyNextMilestone+DescriptionWithReference"]);
				AssertNotNull(grid.Columns["WorkflowItems+MilestonesIncludingRelated+CurrentCompanyNextMilestone+P9_ScheduledDate"]);
				AssertNotNull(grid.Columns["WorkflowItems+Milestones+CurrentCompanyNextMilestone+P9_SE_NKMilestoneEvent"]);
				AssertNotNull(grid.Columns["WorkflowItems+Milestones+CurrentCompanyNextMilestone+DescriptionWithReference"]);
				AssertNotNull(grid.Columns["WorkflowItems+Milestones+CurrentCompanyNextMilestone+P9_ScheduledDate"]);
				AssertNotNull(grid.Columns["Job+JH_Status"]);
				AssertNotNull(grid.Columns["Job+JH_HoldReason"]);
				AssertNotNull(grid.Columns["Job+JH_ProfitLossReasonCode"]);
				AssertNotNull(grid.Columns["Job+JH_TotalProfitRevenueMargin"]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.JE_OH_ControllingAgent]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.JE_OH_ControllingCustomer]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.JE_OH_ExternalBroker]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.LocalClientCode]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.LocalClientName]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.LocalClientAddressAsString]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.LocalClientAddressShortCode]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.LocalClientAddress1]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.LocalClientAddress2]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.LocalClientCity]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.LocalClientState]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.LocalClientCountry]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.RelatedTransportBookingsJobNumbers]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.JE_FCLDeliveryOrPickupEquipmentNeeded]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.DeliveryOrPickupCartageCoPK]);
			}
		}

		public void TestInitializeAdditionalColumns_TaxBranch()
		{
			using (var form = new ZForm())
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var filterControl = new AUJobDeclarationFilterControl(null, DeclarationCollection, FilterBO))
			{
				form.Controls.Add(filterControl);
				form.Show();
				var grid = filterControl.FilteredGrid;
				AssertNotNull(grid.Columns["BillingTaxBranch"]);
			}

			using (var form = new ZForm())
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var filterControl = new AUJobDeclarationFilterControl(null, DeclarationCollection, FilterBO))
			{
				form.Controls.Add(filterControl);
				form.Show();
				var grid = filterControl.FilteredGrid;
				AssertNull(grid.Columns["BillingTaxBranch"]);
			}
		}

		public new void TestCustomColumn()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.Brokerage.Code;
			template.P0_GC = GlbCompany.CurrentCompany.PK;
			template.P0_GB = GlbBranch.CurrentBranch.PK;
			template.P0_GE = GlbDepartment.CurrentDepartment.PK;
			var def = template.GenCustomColumnDefinitions.AddNew();
			def.XC_Name = "custom";
			def.XC_Type = Enterprise.MasterFiles.Business.CustomValues.AddOnColumnDataType.Codes.String;
			Factory.Save();
			using (ZForm form = new ZForm())
			using (AUJobDeclarationFilterControl filterControl = new AUJobDeclarationFilterControl(null, DeclarationCollection, FilterBO))
			{
				form.Controls.Add(filterControl);
				form.Show();
				AssertNotNull(filterControl.FilteredGrid.Columns[CustomPropertyHelper.GeneratePropertyIdentifier("custom", typeof(ZString))]);
			}
		}

		JobDeclarationCollection declarationCollection;
		JobDeclarationCollection DeclarationCollection => declarationCollection ?? (declarationCollection = new JobDeclarationCollection(Factory));

		JobDeclarationFilterBusinessObject filterBO;
		JobDeclarationFilterBusinessObject FilterBO => filterBO ?? (filterBO = new JobDeclarationFilterBusinessObject());
	}
}
