using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business.BusinessObjects.CusPermit;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NCTSCusPermitCusDecProcessor : CusPermitCusDecProcessorForMessage
	{
		public NCTSCusPermitCusDecProcessor(IAllowPermitProcessing header, EDIMessage message)
			: this(header, message.EM_MessageType)
		{
		}

		public NCTSCusPermitCusDecProcessor(IAllowPermitProcessing header, ZString messageType)
			: base(header, messageType)
		{
		}

		protected override IList<Customs.Business.PermitRecord> GetPermitRecords() => header.GetPermitRecords();

		protected override IList<Customs.Business.PermitRecord> GetPermitRecords(EDIMessage message)
		{
			var nctsHeader = message?.EM_LinkedObject as IAllowPermitProcessing;
			return nctsHeader.GetPermitRecords();
		}

		protected override bool AllowNegativeAdjustments => false;

		protected override ISet<ZString> MessageTypes => new HashSet<ZString>(new ZString[] { Core.Constants.CountryCodes.UnitedKingdom } );
	}
}
