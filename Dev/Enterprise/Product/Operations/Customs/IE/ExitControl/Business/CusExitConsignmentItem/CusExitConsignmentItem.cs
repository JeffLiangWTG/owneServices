using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class CusExitConsignmentItem : EU.ExitControl.Business.CusExitConsignmentItem
		, EU.Business.ICusAuthorizationUsageMaster
		, Integration.Customs.IEExitControl.ICusExitConsignmentItem
	{
		public CusExitConsignmentItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new CusExitConsignmentItemValidation Validation => (CusExitConsignmentItemValidation)base.Validation;

		protected override ExitControlBase.Business.CusExitConsignmentItemValidation GetNewValidation() => new CusExitConsignmentItemValidation(this);

		protected override ExitControlBase.Business.ICusExitReportItemCollection<ExitControlBase.Business.CusExitReportItem> CreateNewCusExitReportItemCollection() => new ExitControlBase.Business.CusExitReportItemCollection<CusExitReportItem>(this);

		protected override ExitControlBase.Business.ICusExitConsignmentPivotCollection<ExitControlBase.Business.CusExitConsignmentPivot> CreateNewCusExitConsignmentPivotCollection() => new ExitControlBase.Business.CusExitConsignmentPivotCollection<CusExitConsignmentPivot>(this);

		protected override ExitControlBase.Business.ICusExitConsignmentPivotCollection<ExitControlBase.Business.CusExitConsignmentPivot> CreateNewCusExitConsignmentPackagePivotCollection() => new ExitControlBase.Business.CusExitConsignmentPivotCollection<CusExitConsignmentPivot>(this, ExitControlBase.Business.ConsignmentItemPivotType.Package);

		protected override ExitControlBase.Business.ICusExitConsignmentPivotCollection<ExitControlBase.Business.CusExitConsignmentPivot> CreateNewCusExitConsignmentContainerPivotCollection() => new ExitControlBase.Business.CusExitConsignmentPivotCollection<CusExitConsignmentPivot>(this, ExitControlBase.Business.ConsignmentItemPivotType.Container);

		public override ZShort CCI_LineNumber
		{
			get => base.CCI_LineNumber;
			set
			{
				var oldValue = CCI_LineNumber;
				base.CCI_LineNumber = value;

				if (!IsValidationSuspended && !IsCopying && oldValue != CCI_LineNumber)
				{
					CusExitConsignmentPackagePivots.MarkAsNeedingValidation();
				}
			}
		}

		[ChildEditable]
		public EU.Business.ICusAuthorizationUsageCollection<CusAuthorizationUsage, CusExitConsignmentItem> CusAuthorizationUsages
		{
			get
			{
				if (cusAuthorizationUsages == null)
				{
					cusAuthorizationUsages = GetNewCusAuthorizationUsages();
					cusAuthorizationUsages.Load();
					cusAuthorizationUsages.IsManagedForDataRefresh = true;
					RegisterEditableChildObject(cusAuthorizationUsages);
				}
				return cusAuthorizationUsages;
			}
		}
		EU.Business.ICusAuthorizationUsageCollection<CusAuthorizationUsage, CusExitConsignmentItem> cusAuthorizationUsages;

		protected virtual EU.Business.ICusAuthorizationUsageCollection<CusAuthorizationUsage, CusExitConsignmentItem> GetNewCusAuthorizationUsages() => new CusAuthorizationUsageCollection<CusAuthorizationUsage, CusExitConsignmentItem>(this, Factory);

		public override bool CanDelete
		{
			get
			{
				return !(bool)Consignment.Header?.CusExitReports?.Any(r => r.CusExitReportItems.Any(i => i.ERI_CCI_ConsignmentItem.Equals(PK)));
			}
		}
		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("DBE1069A-7EEA-4401-A638-5A0651002899", "Item cannot be deleted as it is part of an entry referenced in an Exit Report.");

		public override void Delete()
		{
			if (!IsDeleted)
			{
				CusAuthorizationUsages.RemoveAndDeleteAll();
			}
			base.Delete();
		}

		SchemaGuidColumn EU.Business.ICusAuthorizationUsageMaster.FKSchemaColumnInDependent => CusAuthorizationUsageSchema.AGC_ParentID;
	}
}
