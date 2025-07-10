using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business
{
	public class Guarantee : NctsGuarantee
	{
		public Guarantee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Lookups

		public new GuaranteeLookups Lookups => (GuaranteeLookups)base.Lookups;

		protected override Enterprise.MasterFiles.Business.CusBondDetailLookups GetNewLookups() => new GuaranteeLookups(this);

		#endregion
	}
}
