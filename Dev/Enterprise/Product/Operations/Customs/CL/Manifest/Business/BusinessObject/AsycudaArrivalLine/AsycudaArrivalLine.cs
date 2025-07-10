using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class AsycudaArrivalLine : ASYCUDA.Business.AsycudaArrivalLine
	{
		public AsycudaArrivalLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ManifestBase.AsycudaArrivalLineValidation GetNewValidation() => new AsycudaArrivalLineValidation(this);

		public bool ATL_APA_AsycudaPack_ReadOnly
		{
			get { return ATL_ABL_AsycudaBill.IsEmpty; }
		}
	}
}
