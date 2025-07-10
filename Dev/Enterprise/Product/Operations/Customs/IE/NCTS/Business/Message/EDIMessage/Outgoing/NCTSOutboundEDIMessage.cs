using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class NCTSOutboundEDIMessage : OutboundEDIMessage
	{
		public NCTSOutboundEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new NCTSOutboundEDIMessageLookups Lookups => (NCTSOutboundEDIMessageLookups)base.Lookups;

		protected override EDIMessageLookups GetNewLookups() => new NCTSOutboundEDIMessageLookups(this);

		protected override string GetMessageReferenceNumber() => EM_MessageNum.IsEmpty ? Env.NumberFountains.IEMessageControlNumber(EDIInterchange.ApplicationCodes.IECustomsNCTS).GetNextFormatted(Factory) : (string)EM_MessageNum;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIInterchange.ApplicationCodes.IECustomsNCTS;
		}

		NctsHeader LinkedNctsHeader => linkedNctsHeaderCached ??= IsLinkedNctsHeader ? (NctsHeader)EM_LinkedObject : (EM_LinkedObject as NctsDepartureMovementHeader)?.Header;
		NctsHeader linkedNctsHeaderCached;

		public NctsDepartureMovementHeader LinkedDepartureMovementHeader => linkedDepartureMovementHeaderCached ??= IsLinkedNctsHeader ? ((NctsHeader)EM_LinkedObject).MovementHeader : EM_LinkedObject as NctsDepartureMovementHeader;
		NctsDepartureMovementHeader linkedDepartureMovementHeaderCached;

		bool IsLinkedNctsHeader => EM_LinkTable == CusInBondHeaderSchema.Constants.TableName;

		protected override string GetSendersReference()
		{
			if (EM_MessageType == NCTSOutgoingMessageTypeList.Codes.DeclarationData)
			{
				return LinkedNctsHeader?.GetLRNAndSetIfNeeded() ?? string.Empty;
			}
			else if (EM_MessageType == NCTSOutgoingMessageTypeList.Codes.DeclarationAmendment)
			{
				return LinkedDepartureMovementHeader?.BM_PaperlessInbondNum ?? string.Empty;
			}
			else
			{
				return string.Empty;
			}
		}

		protected override string MessageNumberPlaceHolderOverride => CargoWise.Customs.IE.MessageContracts.Constants.NCTSMessageRecipientHolder;
		protected override string UniqueBatchNumberPlaceHolderOverride => CargoWise.Customs.IE.MessageContracts.Constants.NCTSMessageSenderHolder;
		protected override string GetBatchNumber() => GlbCompanyWrapper.GetMessageSenderEORI(EM_LinkedObject as IMessageAttachee);

		protected override string SendersReferencePlaceHolderOverride => LRNPlaceHolder;

		#region Constants For Number Fountain Place Holders
		public const string LRNPlaceHolder = "NCTS__LRN_Holder";
		#endregion
	}
}
