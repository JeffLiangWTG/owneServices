using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public abstract class AISCommonOutboundEDIMessage : OutboundEDIMessage, EU.Business.CusTempStorage.IMessageTypeProvider
	{
		protected AISCommonOutboundEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override string GetSendersReference()
		{
			switch (EM_MessageType.ToUpperInvariant())
			{
				case AISOutgoingMessageTypeList.Codes.ApplicationForRemissionOfCustomsDebtF15:
					return RefundMessageProviderHelper.GetNewRFApplicationReferenceId(Factory);
				case AISOutgoingMessageTypeList.Codes.ApplicationForRemissionOfCustomsDebtR15:
					return RefundMessageProviderHelper.GetNewRDApplicationReferenceId(Factory);
				case AISOutgoingMessageTypeList.Codes.CustomsDeclaration:
					return (EM_LinkedObject as ILRNProvider)?.GetLRNAndSetIfNeeded() ?? string.Empty;
				default:
					return string.Empty;
			}
		}

		protected override string SendersReferencePlaceHolderOverride
		{
			get
			{
				switch (EM_MessageType.ToUpperInvariant())
				{
					case AISOutgoingMessageTypeList.Codes.ApplicationForRemissionOfCustomsDebtF15:
						return RF415ApplicationReferenceIdPlaceHolder;
					case AISOutgoingMessageTypeList.Codes.ApplicationForRemissionOfCustomsDebtR15:
						return RD415ApplicationReferenceIdPlaceHolder;
					default:
						return LRNPlaceHolder;
				}
			}
		}
		public const string RF415ApplicationReferenceIdPlaceHolder = "_RF415_ARI_Holder_";
		public const string RD415ApplicationReferenceIdPlaceHolder = "_RD415_ARI_Holder_";
		public const string LRNPlaceHolder = "AIS__LRN_Holder";

		ZString EU.Business.CusTempStorage.IMessageTypeProvider.MessageType => EM_MessageType;
	}
}
