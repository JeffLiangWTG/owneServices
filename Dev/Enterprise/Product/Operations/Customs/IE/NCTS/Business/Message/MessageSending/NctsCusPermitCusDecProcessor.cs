using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business.BusinessObjects.CusPermit;

namespace Enterprise.Customs.IE.NCTS.Business
{
	class NctsCusPermitCusDecProcessor : EU.NCTS.Business.NCTSCusPermitCusDecProcessor
	{
		public NctsCusPermitCusDecProcessor(IAllowPermitProcessing header, ZString messageType) : base(header, messageType)
		{
		}

		protected override IList<Customs.Business.PermitRecord> GetPermitRecords()
		{
			var permitRecords = base.GetPermitRecords();
			if (messageType == Messaging.NCTSOutgoingMessageTypeList.Codes.DeclarationInvalidationRequest)
			{
				foreach (var record in permitRecords)
				{
					record.Value = 0;
					record.Quantity = 0;
				}
			}
			return permitRecords;
		}

		protected override bool AllowNegativeAdjustments => true;
	}
}
