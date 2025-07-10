using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Module
{
	public class AUJobDeclarationFilterControl : Customs.Module.JobDeclarationFilterStripControl
	{
		public AUJobDeclarationFilterControl()
			: base()
		{
		}

		public AUJobDeclarationFilterControl(JobDeclarationModule module, IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(module, gridCollection, filterStripBusinessObject)
		{
			InitialiseGridColumns();
			WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, gridCollection, JobInvoicingConsumerTypes.Brokerage.Code, false, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);
		}

		protected override ZBool ShouldAddWHSStatusFieldToGrid
		{
			get { return true; }
		}

		protected override ZBool ShouldAddCustomFieldsToGrid
		{
			get { return false; }
		}

		void InitialiseGridColumns()
		{
			var consolidatedCargoStatusTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			consolidatedCargoStatusTextBoxColumnStyleInfo.Caption = "Cargo Status";
			consolidatedCargoStatusTextBoxColumnStyleInfo.ColumnName = JobDeclaration.Schema.JE_ConsolidatedCargoStatus;
			consolidatedCargoStatusTextBoxColumnStyleInfo.IsVisible = false;
			ControlDpiScalingHelper.SetWidth(ref consolidatedCargoStatusTextBoxColumnStyleInfo, 50, true);
			FilteredGrid.ColumnStyles.Add(consolidatedCargoStatusTextBoxColumnStyleInfo);

			var consolidatedCargoStatusDescriptionTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			consolidatedCargoStatusDescriptionTextBoxColumnStyleInfo.Caption = "Cargo Status Description";
			consolidatedCargoStatusDescriptionTextBoxColumnStyleInfo.ColumnName = JobDeclaration.Schema.ConsolidatedCargoStatusDescription;
			consolidatedCargoStatusDescriptionTextBoxColumnStyleInfo.IsVisible = false;
			FilteredGrid.ColumnStyles.Add(consolidatedCargoStatusDescriptionTextBoxColumnStyleInfo);

			ReOrderGridColumns(FilteredGrid);
		}

		protected void ReOrderGridColumns(ZGrid grid)
		{
			ArrayList newColumnOrder = GetNewColumnOrderForGrid(grid);

			if (newColumnOrder != null && newColumnOrder.Count > 0)
			{
				grid.ColumnStyles.Clear();
				grid.ColumnStyles.AddRange(newColumnOrder);
			}
		}

		ArrayList GetNewColumnOrderForGrid(ZGrid grid)
		{
			ZString[] columnNames = ColumnNamesInSortOrder;
			ArrayList result = new ArrayList(columnNames.Length);

			foreach (string columnName in columnNames)
			{
				ZGridColumnInfo column = grid.GetColumnStyle(columnName);
				result.Add(column);
			}

			return result;
		}

		protected ZString[] ColumnNamesInSortOrder
		{
			get
			{
				if (columnNamesInSortOrder == null)
				{
					var columns = new List<ZString>();

					columns.Add(JobDeclaration.Schema.JE_GB);
					columns.Add(JobDeclaration.Schema.JE_MessageType);
					columns.Add(JobDeclaration.Schema.JE_TransportMode);
					columns.Add(JobDeclaration.Schema.JE_DeclarationReference);
					columns.Add(JobDeclaration.Schema.JE_VesselName);
					columns.Add(JobDeclaration.Schema.JE_VoyageFlightNo);
					columns.Add(JobDeclaration.Schema.JE_DateOfArrival);
					columns.Add(JobDeclaration.Schema.JE_RL_NKOrigin);
					columns.Add(JobDeclaration.Schema.JE_RL_NKFinalDestination);
					columns.Add(JobDeclaration.Schema.JE_DateAtFinalDestination);
					columns.Add(JobDeclaration.Schema.JE_HouseBill);
					columns.Add(JobDeclaration.Schema.JE_OH_Importer);
					columns.Add(JobDeclaration.Schema.JE_OH_Supplier);
					columns.Add(JobDeclaration.Schema.JE_AgentsReference);
					columns.Add(JobDeclaration.Schema.JE_ContainerMode);
					columns.Add(JobDeclaration.Schema.JE_ContainerCount);
					columns.Add(JobDeclaration.Schema.JE_DateOfFirstArrival);
					columns.Add(JobDeclaration.Schema.JE_EntryAuthorisationDate);
					columns.Add(JobDeclaration.Schema.JE_EntryStatus);
					columns.Add(JobDeclaration.Schema.JE_EntrySubmittedDate);
					columns.Add(JobDeclaration.Schema.JE_ExportDate);
					columns.Add(JobDeclaration.Schema.JE_ExportGoodsType);
					columns.Add(JobDeclaration.Schema.JE_GoodsDescription);
					columns.Add(JobDeclaration.Schema.JE_MasterBill);
					columns.Add(JobDeclaration.Schema.JE_MessageSubType);
					columns.Add(JobDeclaration.Schema.JE_OwnerRef);
					columns.Add(JobDeclaration.Schema.JE_ScreeningStatus);
					columns.Add(JobDeclaration.Schema.JE_RL_NKPortOfArrival);
					columns.Add(JobDeclaration.Schema.JE_RL_NKPortOfFirstArrival);
					columns.Add(JobDeclaration.Schema.JE_RL_NKPortOfLoading);
					columns.Add(JobDeclaration.Schema.JE_ETAOfDischarge);
					columns.Add(JobDeclaration.Schema.JE_ETDOfLoading);
					columns.Add(JobDeclaration.Schema.JE_TotalNoOfPacks);
					columns.Add(JobDeclaration.Schema.JE_TotalNoOfPacksPackType);
					columns.Add(JobDeclaration.Schema.DeclarationNumber);
					columns.Add(JobDeclaration.Schema.EarliestCustomsEntryIssueDate);
					columns.Add(JobDeclaration.Schema.JE_EntryStatusDescription);
					columns.Add(JobDeclaration.Schema.OrderNumbers);
					columns.Add(JobDeclaration.Schema.JE_TotalVolume);
					columns.Add(JobDeclaration.Schema.JE_TotalVolumeUnit);
					columns.Add(JobDeclaration.Schema.JE_TotalWeight);
					columns.Add(JobDeclaration.Schema.JE_TotalWeightUnit);
					columns.Add(JobDeclaration.Schema.JE_MessageStatus);
					columns.Add(JobDeclaration.Schema.JE_ConsolidatedCargoStatus);
					columns.Add(JobDeclaration.Schema.ConsolidatedCargoStatusDescription);
					columns.Add(JobDeclaration.Schema.JE_SystemCreateTimeUtc);
					columns.Add(JobDeclaration.Schema.JE_GS_NKCusAgent);
					columns.Add("WorkflowItems+Milestones+LastMilestone+P9_SE_NKMilestoneEvent");
					columns.Add("WorkflowItems+Milestones+LastMilestone+P9_Description");
					columns.Add("WorkflowItems+Milestones+LastMilestone+P9_ActualDateForBinding");
					columns.Add("WorkflowItems+Milestones+NextMilestone+P9_SE_NKMilestoneEvent");
					columns.Add("WorkflowItems+Milestones+NextMilestone+P9_Description");
					columns.Add("WorkflowItems+Milestones+NextMilestone+P9_ScheduledDateForBinding");
					columns.Add("WorkflowItems+MilestonesIncludingRelated+CurrentCompanyLastMilestone+P9_SE_NKMilestoneEvent");
					columns.Add("WorkflowItems+MilestonesIncludingRelated+CurrentCompanyLastMilestone+DescriptionWithReference");
					columns.Add("WorkflowItems+MilestonesIncludingRelated+CurrentCompanyLastMilestone+P9_ActualDate");
					columns.Add("WorkflowItems+Milestones+CurrentCompanyLastMilestone+P9_SE_NKMilestoneEvent");
					columns.Add("WorkflowItems+Milestones+CurrentCompanyLastMilestone+DescriptionWithReference");
					columns.Add("WorkflowItems+Milestones+CurrentCompanyLastMilestone+P9_ActualDate");
					columns.Add("WorkflowItems+MilestonesIncludingRelated+CurrentCompanyNextMilestone+P9_SE_NKMilestoneEvent");
					columns.Add("WorkflowItems+MilestonesIncludingRelated+CurrentCompanyNextMilestone+DescriptionWithReference");
					columns.Add("WorkflowItems+MilestonesIncludingRelated+CurrentCompanyNextMilestone+P9_ScheduledDate");
					columns.Add("WorkflowItems+Milestones+CurrentCompanyNextMilestone+P9_SE_NKMilestoneEvent");
					columns.Add("WorkflowItems+Milestones+CurrentCompanyNextMilestone+DescriptionWithReference");
					columns.Add("WorkflowItems+Milestones+CurrentCompanyNextMilestone+P9_ScheduledDate");
					columns.Add((ZString)JobDeclaration.Schema.BrokerName);
					columns.Add((ZString)JobDeclaration.Schema.ImporterName);
					columns.Add((ZString)JobDeclaration.Schema.SupplierName);
					columns.Add((ZString)JobDeclaration.Schema.ForwarderName);
					columns.Add(JobDeclaration.Schema.JE_OH_ControllingAgent);
					columns.Add(JobDeclaration.Schema.JE_OH_ControllingCustomer);
					columns.Add(JobDeclaration.Schema.JE_OH_ExternalBroker);

					columns.Add((ZString)JobDeclaration.Schema.AuditDate);
					columns.Add((ZString)JobDeclaration.Schema.AuditReference);
					columns.Add((ZString)JobDeclaration.Schema.AuditLogUser);
					columns.Add((ZString)JobDeclaration.Schema.AuditLogUserName);
					columns.Add((ZString)JobDeclaration.Schema.FreightContainerMode);
					columns.Add(JobDeclaration.Schema.WarehouseTransactionStatusDescription);
					columns.Add(JobDeclaration.Schema.BillingBranch);
					columns.Add(JobDeclaration.Schema.BillingDepartment);
					columns.Add(JobDeclaration.Schema.BillingOperator);
					columns.Add(JobDeclaration.Schema.BillingTaxBranch);
					columns.Add(JobDeclaration.Schema.JE_ApplicationCode);
					columns.Add((ZString)JobDeclaration.Schema.JE_FCLDeliveryOrPickupEquipmentNeeded);
					columns.Add((ZString)JobDeclaration.Schema.DeliveryOrPickupCartageCoPK);
					columns.Add("Job+JH_Status");
					columns.Add("Job+JH_HoldReason");
					columns.Add("Job+JH_ProfitLossReasonCode");
					columns.Add("Job+JH_TotalProfitRevenueMargin");

					AddColumnByColumnStyle(columns, JobDeclaration.Schema.JE_SystemCreateUser);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.JE_SystemCreateTimeUtc);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.JE_SystemLastEditUser);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.JE_SystemLastEditTimeUtc);

					AddColumnByColumnStyle(columns, JobDeclaration.Schema.LocalClientCode);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.LocalClientName);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.LocalClientAddressAsString);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.LocalClientAddressShortCode);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.LocalClientAddress1);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.LocalClientAddress2);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.LocalClientCity);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.LocalClientState);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.LocalClientCountry);

					AddColumnByColumnStyle(columns, JobDeclaration.Schema.RelatedTransportBookingsJobNumbers);

					columnNamesInSortOrder = columns.ToArray();
				}

				return columnNamesInSortOrder;
			}
		}

		void AddColumnByColumnStyle(List<ZString> columns, string columnName)
		{
			if (grid.GetColumnStyle(columnName) != null)
			{
				columns.Add(columnName);
			}
		}

		ZString[] columnNamesInSortOrder;
	}
}
