using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.eNett
{
	public class ComPayRegisteredOrganisation : AutoComPayRegisteredOrganisation
	{
		// You probably only want one of these constructors. You should delete the other.

		public ComPayRegisteredOrganisation()
		{ }

		public ComPayRegisteredOrganisation(BusinessObjectFactory factory)
			: base(factory) { }

		BusinessObjectFactory fFactory;
		new BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					if (base.Factory == null)
					{
						fFactory = new BusinessObjectFactory();
					}
					else
					{
						fFactory = base.Factory;
					}
				}
				return fFactory;
			}
		}

		OrgCusCode fCusCode;
		public OrgCusCode CusCode
		{
			get
			{
				if (fCusCode == null)
				{
					fCusCode = Factory.New<OrgCusCode>();
				}
				return fCusCode;
			}
		}

		public List<ZGuid> OrgHeaderPKs
		{
			get
			{
				if (fOrgHeaderPKs == null)
				{
					fOrgHeaderPKs = new List<ZGuid>();
				}

				return fOrgHeaderPKs;
			}
		}

		List<ZGuid> fOrgHeaderPKs;
	}
}