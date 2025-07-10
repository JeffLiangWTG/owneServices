using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public class UPERefLocoMap : RefLocoMap
	{
		public UPERefLocoMap(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override RefLocoMapLookups GetNewLookups()
		{
			return new UPERefLocoMapLookups(this);
		}

		protected override RefLocoMapValidation GetNewValidation()
		{
			return new UPERefLocoMapValidation(this);
		}
	}
}
