using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Manifest.Business;

namespace Enterprise.Customs.GB.ICS.Business
{
	public class AsycudaPackSS : AsycudaPack
	{
		public AsycudaPackSS(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ManifestBase.AsycudaPackLookups GetNewLookups() => new AsycudaPackSSLookups(this);
	}
}
