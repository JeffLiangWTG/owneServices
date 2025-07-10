using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class ProjectLookups : ProcessManagement.Business.WorkProjectLookups
	{
		public ProjectLookups(EDIProject parent) : base(parent) { }

		public ProjectLookups(BusinessObjectFactory factory) : base(factory) { }

		public new EDIProject Parent
		{
			get { return (EDIProject)base.Parent; }
		}

		#region Licence Headers

		public LicenceHeaderCollection LicenceHeaderList
		{
			get
			{
				LicenceHeaderCollection result;
				EDIOrgHeader header;

				if ((header = (EDIOrgHeader)Parent.ClientOrganisation) != null && header.LicCompany != null)
				{
					result = header.LicCompany.LicHeadersForAllDatabases;
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation", "Property", header.PK));
				}
				else
				{
					if (licenceHeaderList == null)
					{
						licenceHeaderList = new LicenceHeaderCollection(Factory);
					}
					result = licenceHeaderList;
				}

				return result;
			}
		}

		LicenceHeaderCollection licenceHeaderList;

		#endregion

		protected IModuleListBuilder GetNewModuleListBuilder()
		{
			return new ProjectModuleListBuilder();
		}
	}
}

