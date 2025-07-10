using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ComplianceReport.PTRS
{
	public abstract class PtrsReportBase : AccTaxReturn
	{
		public PtrsReportBase(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ATR_ReturnType = ReturnType.PTRS;
			ATR_Status = string.Empty;
			ATR_Version = 1;
		}

		#region Load Details

		public override void OnLoaded()
		{
			base.OnLoaded();
			this.SetHeaderDetails();
			GetDataFromComplianceReportLines();
		}

		public override ZGuid ATR_ACR_ComplianceReport
		{
			get => base.ATR_ACR_ComplianceReport;
			set
			{
				base.ATR_ACR_ComplianceReport = value;
				this.SetHeaderDetails();
				GetDataFromComplianceReportLines();
			}
		}

		protected abstract void GetDataFromComplianceReportLines();

		#endregion

		internal List<PtrsReportColumnWrapperBase> ColumnWrappers => columnWrappers ?? (columnWrappers = new List<PtrsReportColumnWrapperBase>());
		List<PtrsReportColumnWrapperBase> columnWrappers;

		#region Mark as Submitted

		public virtual void SubmitReport()
		{
			if (IsSaved && !HasChanges)
			{
				Reload();
				Columns.Reload(true);

				ATR_Status = Status.Submitted;
			}
		}

		#endregion
	}
}
