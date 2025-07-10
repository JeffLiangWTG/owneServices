using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class Licence3rdPartySoftwareLookups : AutoLicence3rdPartySoftwareLookups
	{
		public Licence3rdPartySoftwareLookups(AutoLicence3rdPartySoftware parent)
			: base(parent)
		{
		}

		public new Licence3rdPartySoftware Parent
		{
			get { return (Licence3rdPartySoftware)base.Parent; }
		}

		#region OSType List

		public OSTypeList OSType
		{
			get { return new OSTypeList(); }
		}

		#endregion

		#region LicenceType List

		public LicenceTypeList LicenceType
		{
			get { return new LicenceTypeList(); }
		}

		#endregion

		#region Products

		public EDIOrgSupplierPartCollection Products
		{
			get
			{
				if (products == null)
				{
					products = new EDIOrgSupplierPartCollection(Factory);
				}

				return products;
			}
		}
		EDIOrgSupplierPartCollection products;

		#endregion

		#region Clients

		public OrganisationsFindBoxCollection Clients
		{
			get
			{
				if (clients == null)
				{
					ZQuery filter = new ZQuery(OrgHeaderSchema.OH_IsActive, ZBool.True);
					clients = new OrganisationsFindBoxCollection(Factory, filter);
				}

				return clients;
			}
		}

		OrganisationsFindBoxCollection clients;

		#endregion
	}
}

