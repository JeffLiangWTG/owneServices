using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Business
{
	[ModuleID(ModuleId.CACusRuling)]
	public class CACusRulingFindBoxCollection : CusRulingFindBoxCollection
	{
		public CACusRulingFindBoxCollection(BusinessObjectFactory factory, ZString rulingNumber)
			: base(factory, rulingNumber)
		{
		}

		public CACusRulingFindBoxCollection(BusinessObjectFactory factory, ZString rulingNumber, OrgHeader org, IEnumerable<ZGuid> validOrganizations)
			: base(factory, rulingNumber, org, validOrganizations)
		{
		}

		public CACusRulingFindBoxCollection(BusinessObjectFactory factory, ZString rulingType, ZString rulingNumber, OrgHeader org, IEnumerable<ZGuid> validOrganizations)
			: base(factory, rulingType == CalculationMethods.Codes.NoRemission ? ZString.Empty : rulingType, rulingNumber, org, validOrganizations)
		{
		}
	}
}
