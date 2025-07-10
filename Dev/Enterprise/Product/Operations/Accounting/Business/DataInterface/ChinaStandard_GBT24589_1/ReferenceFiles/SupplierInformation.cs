using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1
{
	public sealed class SupplierInformation : NonPersistentBusinessObject, IObsoleteValidation
	{
		public const string LocID = "T109";
		public ZString SupplierCode { get; set; }
		public ZString SupplierName { get; set; }
		public ZString SupplierAbbreviation { get; set; }
	}

	public sealed class SupplierInformationCollection : NonPersistentBusinessObjectCollection<SupplierInformation>	{
		public SupplierInformationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			AddDefaultElements();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SupplierInformation();
		}

		void AddDefaultElements()
		{
			OrgHeaderCollection collection = new OrgHeaderCollection(Factory, filter());
			collection.Load();
			foreach (OrgHeader supplier in collection)
			{
				SupplierInformation supplierInformation = AddNew();
				supplierInformation.SupplierCode = supplier.OH_Code;
				supplierInformation.SupplierName = LocalCompanyName.GetLocalCompanyName(supplier, OrgConstants.AddressType.Payables);
				supplierInformation.SupplierAbbreviation = supplier.OH_Code;
			}
		}

		ZQuery filter()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			query.AddSubQuery(subQuery, JoinCondition.And);
			query.OrderBy = AutoOrgHeader.Schema.OH_Code;
			return query;
		}
	}
}

