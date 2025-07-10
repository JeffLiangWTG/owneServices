using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusContainerInvoiceLinePivot : Customs.Business.CusContainerInvoiceLinePivot, Integration.Customs.AU.ICusContainerInvoiceLinePivot
	{
		public CusContainerInvoiceLinePivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related BO's
		public new CusContainer Container
		{
			get { return base.Container as CusContainer; }
		}

		public new JobComInvoiceLine InvoiceLine
		{
			get { return (JobComInvoiceLine)base.InvoiceLine; }
		}
		#endregion

		public override bool IsSavedByFactory
		{
			get { return base.IsSavedByFactory && IsDeclarationPersistent; }
		}

		public bool IsDeclarationPersistent
		{
			get
			{
				var container = IsDeleted ? null : Container;
				return container == null || container.IsDeleted || container.IsDeclarationPersistent;
			}
		}

		public new CusContainerInvoiceLinePivotValidation Validation
		{
			get { return (CusContainerInvoiceLinePivotValidation)base.Validation; }
		}

		protected override Customs.Business.CusContainerInvoiceLinePivotValidation GetNewValidation()
		{
			return new CusContainerInvoiceLinePivotValidation(this);
		}
	}
}
