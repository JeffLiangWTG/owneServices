using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.BusinessObjects.CusPermit;

namespace Enterprise.Customs.FR.Business
{
	class FRCusPermitCusDecProcessorForMessage : CusPermitCusDecProcessorForMessage
	{
		public FRCusPermitCusDecProcessorForMessage(IAllowPermitProcessing header, ZString messageType, bool rectifying, CusGuaranteeHeader ai2Permit) : base(header, messageType)
		{
			this.rectifying = rectifying;
			this.ai2Permit = ai2Permit;
		}

		protected override IList<Customs.Business.PermitRecord> GetPermitRecords() => PermitRecordsWithoutAI2IfRectifying(header.GetPermitRecords());

		protected override IList<Customs.Business.PermitRecord> GetPermitRecords(Enterprise.Messaging.Business.EDIMessage message)
		{
			var businessObject = message?.EM_LinkedObject as IAllowPermitProcessing;
			return PermitRecordsWithoutAI2IfRectifying(businessObject.GetPermitRecords());
		}

		IList<Customs.Business.PermitRecord> PermitRecordsWithoutAI2IfRectifying(IList<Customs.Business.PermitRecord> permitRecords)
		{
			var result = permitRecords;

			if (this.rectifying && this.ai2Permit != null)
			{
				result = result.Where(x => x.PermitHeader.PK != ai2Permit.PK).ToList();
			}

			return result;
		}

		protected override bool AllowNegativeAdjustments => true;

		protected override ISet<ZString> MessageTypes => new HashSet<ZString>(new ZString[] { MessageTypeList.Codes.IMC, MessageTypeList.Codes.IMD, MessageTypeList.Codes.EXC, MessageTypeList.Codes.EXD });

		readonly bool rectifying;
		readonly CusGuaranteeHeader ai2Permit;
	}
}
