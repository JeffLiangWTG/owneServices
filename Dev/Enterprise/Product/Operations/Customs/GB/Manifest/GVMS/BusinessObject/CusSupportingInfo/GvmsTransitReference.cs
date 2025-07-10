using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.GVMS
{
	public class GvmsTransitReference : GvmsItemReference
	{
		public GvmsTransitReference(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override CusSupportingInfoLookups GetNewLookups() => new GvmsTransitReferenceLookups(this);
	}
}
