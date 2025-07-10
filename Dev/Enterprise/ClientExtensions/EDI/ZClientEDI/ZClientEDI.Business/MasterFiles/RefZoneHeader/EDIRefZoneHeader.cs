using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIRefZoneHeader : RefZoneHeader
	{
		public EDIRefZoneHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		protected override RefZoneHeaderLookups GetNewLookups()
		{
			return new EDIRefZoneHeaderLookups(this);
		}
	}
}

