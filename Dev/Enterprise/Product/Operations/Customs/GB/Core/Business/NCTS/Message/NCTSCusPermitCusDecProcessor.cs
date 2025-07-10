using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.BusinessObjects.CusPermit;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class NCTSCusPermitCusDecProcessor : GBCusPermitCusDecProcessor
	{
		public NCTSCusPermitCusDecProcessor(IAllowPermitProcessing header, EDIMessage message) : this(header, message.EM_MessageType) { }

		public NCTSCusPermitCusDecProcessor(IAllowPermitProcessing header, ZString messageType) : base(header, messageType) { }

		protected override IList<PermitRecord> GetPermitRecords() => header.GetPermitRecords();

		protected override IList<PermitRecord> GetPermitRecords(EDIMessage message)
		{
			IAllowPermitProcessing allowPermitProcessing = message?.EM_LinkedObject as IAllowPermitProcessing;
			return allowPermitProcessing.GetPermitRecords();
		}

		protected override bool AllowNegativeAdjustments => true;

		protected override ISet<ZString> MessageTypes => new HashSet<ZString>(new ZString[2] { GB_NCTS5DeparturePhaseList.Codes.Amendment, GB_NCTS5DeparturePhaseList.Codes.DepartureDeclaration });
	}
}
