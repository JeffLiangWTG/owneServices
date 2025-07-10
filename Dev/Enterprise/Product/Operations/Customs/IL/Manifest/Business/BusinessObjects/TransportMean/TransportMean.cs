using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class TransportMean : ASYCUDA.Business.TransportMean
	{
		public TransportMean(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override JobConsolTransportLookups GetNewLookups() => new TransportMeanLookups(this);

		public new JobConsolTransportValidation Validation => GetNewValidation();

		protected new JobConsolTransportValidation GetNewValidation() => new AsycudaManifestHeaderTransportSupporter(this.Parent as AsycudaManifestHeader).GetNewTransportValidator(this);
	}
}
