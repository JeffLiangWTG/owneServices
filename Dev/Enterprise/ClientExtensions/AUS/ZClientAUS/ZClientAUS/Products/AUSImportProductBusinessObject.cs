using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.AUS.Products
{
	public class AUSImportProductBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		public AUSImportProductBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ZString DummyStringField
		{
			get;
			set;
		}

		public OrgHeader Importer
		{
			get { return Factory.Load<OrgHeader>(ImporterPK); }
		}

		[List("Organisations")]
		public ZGuid ImporterPK
		{
			get;
			set;
		}

		public OrgHeader Supplier
		{
			get { return Factory.Load<OrgHeader>(SupplierPK); }
		}

		[List("Organisations")]
		public ZGuid SupplierPK
		{
			get;
			set;
		}

		public OrgHeaderCollection Organisations
		{
			get { return organisations ?? (organisations = new OrgHeaderCollection(Factory, new ZQuery())); }
		}

		OrgHeaderCollection organisations;
	}
}
