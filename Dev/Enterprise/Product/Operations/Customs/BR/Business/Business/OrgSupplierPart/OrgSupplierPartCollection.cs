using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class OrgSupplierPartCollection : Customs.Business.OrgSupplierPartCollection
	{
		public OrgSupplierPartCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgSupplierPartCollection(BusinessObjectFactory factory, BaseJobComInvoiceLine invoiceLine, bool isExport)
			: base(factory, invoiceLine, isExport)
		{
		}

		public new OrgSupplierPart AddNew()
		{
			return (OrgSupplierPart)base.AddNew();
		}

		public new OrgSupplierPart this[int index]
		{
			get { return (OrgSupplierPart)Elements[index]; }
		}

		protected override void AddPivotWithAdditionalLineDetailsCore(Customs.Business.OrgSupplierPart part, BaseJobComInvoiceLine invoiceLine)
		{
			base.AddPivotWithAdditionalLineDetailsCore(part, invoiceLine);
			CopyValuesFromInvoiceLine(part, invoiceLine);
		}

		void CopyValuesFromInvoiceLine(Customs.Business.OrgSupplierPart part, BaseJobComInvoiceLine invoiceLine)
		{
			var orgSupplierPart = (OrgSupplierPart)part;
			var jobComInvoiceLine = (JobComInvoiceLine)invoiceLine;
			var pivot = orgSupplierPart.GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.Brazil).FirstOrDefault();

			if (pivot != null)
			{
				if (pivot.ComplementaryDescription.IsEmpty)
				{
					pivot.ComplementaryDescription = jobComInvoiceLine.ComplementaryDescription;
				}

				if (jobComInvoiceLine.GoodsCatalog != null)
				{
					pivot.CI_CGC_Catalog = jobComInvoiceLine.JI_CGC_Catalog;
				}
			}
		}
	}
}
