using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business.BusinessObjects.CusPermit;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Business
{
	public class GBCusPermitCusDecProcessor : CusPermitCusDecProcessorForMessage
	{
		public GBCusPermitCusDecProcessor(IAllowPermitProcessing header, ZString messageType) : base(header, messageType)
		{
		}

		protected override IList<Customs.Business.PermitRecord> GetPermitRecords() => header.GetPermitRecords();

		protected override IList<Customs.Business.PermitRecord> GetPermitRecords(EDIMessage message)
		{
			var entryHeader = message?.EM_LinkedObject as CusEntryHeader;
			return entryHeader.GetPermitRecords();
		}

		protected override bool AllowNegativeAdjustments => true;
	}
}
