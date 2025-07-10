using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using CusEntryHeader = Enterprise.Customs.EU.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	public class CusExitHeader : EU.ExitControl.Business.CusExitHeader
		, Integration.Customs.DEExitControl.ICusExitHeader
	{
		public CusExitHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusExitHeaderValidation Validation => (CusExitHeaderValidation)base.Validation;

		[LightValidationTestExempt]
		public override ZGuid CXH_ParentID { get => base.CXH_ParentID; set => base.CXH_ParentID = value; }

		[LightValidationTestExempt]
		public override ZString CXH_ParentTableCode { get => base.CXH_ParentTableCode; set => base.CXH_ParentTableCode = value; }

		protected override ExitControlBase.Business.CusExitHeaderValidation GetNewValidation() => new CusExitHeaderValidation(this);

		protected override ICusExitConsignmentCollection<ExitControlBase.Business.CusExitConsignment> CreateNewCusExitConsignmentCollection() => new CusExitConsignmentCollection<CusExitConsignment>(this);

		public new ICusExitConsignmentCollection<CusExitConsignment> CusExitConsignments => (ICusExitConsignmentCollection<CusExitConsignment>)base.CusExitConsignments;

		protected override ICusExitReportCollection<ExitControlBase.Business.CusExitReport> CreateNewCusExitReportCollection() => new CusExitReportCollection<CusExitReport>(this);

		public new ICusExitReportCollection<CusExitReport> CusExitReports => (ICusExitReportCollection<CusExitReport>)base.CusExitReports;

		#region IsAutomatedValidationEnabledTra

		public bool IsAutomatedValidationEnabledTRA => Factory.GetValue(ref isAutomatedValidationEnabledTRA, GetIsAutomatedValidationEnabledTRA);

		CachedProperty<bool> isAutomatedValidationEnabledTRA;

		bool GetIsAutomatedValidationEnabledTRA()
		{
			return (this as IWorkflowProvider).WorkflowItems.Cast<ProcessTask>().Any(t =>
				t.IsWorkflowTrigger
				&& t.TriggerConditions.TriggerEventCode == AutoEvents.CustomsEntryStatusCode
				&& t.TriggerConditions.TriggerConditionValue == DE.Business.UniversalReferenceConstants.CusExitDetailStatus._310
				&& t.TriggerConditions.TriggerCondition == EventReferenceConditionList.Codes.EventReference
				&& t.P9_LineTriggerType == WorkflowDescriptors.CusExitReportWorkflowDescriptorCode
				&& t.ProcessTaskNotifications.Any(n => n.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendExitReportTransferMessage));
		}

		#endregion

		protected override EU.ExitControl.Business.CusExitConsignment CreateConsignmentFromEntryHeader(CusEntryHeader entryHeader)
		{
			var consignment = base.CreateConsignmentFromEntryHeader(entryHeader);
			if (consignment == null && entryHeader.MovementReferenceNumber.IsEmpty)
			{
				var lrn = CusEntryNumber.Load(entryHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, entryHeader.CountryCode)?.CE_EntryNum ?? ZString.Empty;
				if (!lrn.IsEmpty && !CusExitConsignments.Any(x => x.CXC_LocalReference == lrn))
				{
					consignment = CusExitConsignments.AddNew();
					consignment.CXC_LocalReference = lrn;
				}
			}

			if (consignment != null && entryHeader.Declaration.Shipment is ForwardingShipment shipment)
			{
				SetReferenceNumberFromMasterBillNum(consignment, shipment);
			}
			return consignment;
		}

		protected override void DefaultHeaderDataFromDeclaration(JobDeclaration declaration)
		{
			base.DefaultHeaderDataFromDeclaration(declaration);
			CXH_OA_Carrier = declaration.Branch?.OrgProxy?.MainAddress?.PK ?? ZGuid.Empty;
		}

		protected override void DefaultHeaderDataFromShipment(ForwardingShipment shipment)
		{
			base.DefaultHeaderDataFromShipment(shipment);
			CXH_OH_Exporter = shipment.ConsignorDocumentaryAddress.OrganisationPK;
			CXH_OA_Carrier = GlbBranch.CurrentBranch.OrgProxy?.MainAddress.PK ?? ZGuid.Empty;
		}

		protected override EU.ExitControl.Business.CusExitConsignment CreateConsignmentFromShipment(ForwardingShipment shipment)
		{
			EU.ExitControl.Business.CusExitConsignment exitConsignment = null;
			var mrnCollection = shipment.CusEntryNumbers.Where(n => n.CE_EntryType == CusEntryNumberTypes.Standard.MovementReferenceNumber).Select(n => n.CE_EntryNum);

			foreach (var mrn in mrnCollection)
			{
				exitConsignment = CreateConsignmentFromMRN(mrn);
				SetReferenceNumberFromMasterBillNum(exitConsignment, shipment);
			}

			return exitConsignment;
		}

		void SetReferenceNumberFromMasterBillNum(EU.ExitControl.Business.CusExitConsignment consignment, ForwardingShipment shipment)
		{
			if (consignment != null && DefaultDataFromShipmentHelper.GetConsol(shipment) is ForwardingConsol consol)
			{
				consignment.CXC_ReferenceNumber = consol.JK_MasterBillNum;
			}
		}
	}
}
