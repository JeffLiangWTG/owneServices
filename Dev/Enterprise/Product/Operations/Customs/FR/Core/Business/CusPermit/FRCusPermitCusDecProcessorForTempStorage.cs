using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.BusinessObjects.CusPermit;
using Enterprise.Customs.FR.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business
{
	public class FRCusPermitCusDecProcessorForTempStorage : CusPermitCusDecProcessor<CusTempStorageDec>
	{
		public FRCusPermitCusDecProcessorForTempStorage(IAllowPermitProcessing header) : base(header)
		{
		}

		protected override IList<PermitRecord> GetPermitRecords() => header.GetPermitRecords();

		protected override bool AllowNegativeAdjustments => true;
	}
}
