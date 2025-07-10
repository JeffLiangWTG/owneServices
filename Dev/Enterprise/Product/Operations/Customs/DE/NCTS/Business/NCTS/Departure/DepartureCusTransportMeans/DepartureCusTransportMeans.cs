using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class DepartureCusTransportMeans : EU.NCTS.Business.DepartureCusTransportMeans
	{
		public DepartureCusTransportMeans(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new DepartureCusTransportMeansValidation Validation => (DepartureCusTransportMeansValidation)base.Validation;

		protected override Customs.Business.CusTransportMeansValidation GetNewValidation()
		{
			return new DepartureCusTransportMeansValidation(this);
		}
	}
}
