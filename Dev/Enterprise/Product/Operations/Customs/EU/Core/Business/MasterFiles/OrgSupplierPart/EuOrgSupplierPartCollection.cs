using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business.MasterFiles
{
	public class EuOrgSupplierPartCollection : OrgSupplierPartCollection
	{
		public EuOrgSupplierPartCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public EuOrgSupplierPartCollection(BusinessObjectFactory factory, JobComInvoiceLine invoiceLine, bool isExport)
			: base(factory, invoiceLine, isExport)
		{
		}

		public new OrgSupplierPart AddNew()
		{
			var newPart = (OrgSupplierPart)OrgSupplierPart.New(Factory);
			this.Add(newPart);
			SetDefaultsForNewChild(newPart);
			return newPart;
		}

		protected override BusinessObject AddNewCore()
		{
			return AddNew();
		}

		protected override BusinessObject AddNewCore(Type bizoType)
		{
			var newPart = Factory.New(bizoType);
			this.Add(newPart);
			SetDefaultsForNewChild(newPart);
			return newPart;
		}

		public new OrgSupplierPart this[int index]
		{
			get { return (OrgSupplierPart)Elements[index]; }
		}
	}
}
