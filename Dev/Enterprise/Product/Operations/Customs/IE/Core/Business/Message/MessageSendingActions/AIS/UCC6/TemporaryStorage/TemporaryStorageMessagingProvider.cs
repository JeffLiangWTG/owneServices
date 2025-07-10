using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.CusTempStorage
{
	public class TemporaryStorageMessagingProvider : EU.Business.CusTempStorage.TemporaryStorageMessagingProvider
	{
		public CodeDescriptionPairList GetDeclarationTypes()
		{
			var codeDescriptionPairList = new CodeDescriptionPairList();
			codeDescriptionPairList.AddPair(TemporaryStorageDeclarationTypeList.Codes.DeclarationAndPresentationNotification, TemporaryStorageDeclarationTypeList.Descriptions.DeclarationAndPresentationNotification);
			codeDescriptionPairList.AddPair(TemporaryStorageDeclarationTypeList.Codes.Declaration, TemporaryStorageDeclarationTypeList.Descriptions.Declaration);
			codeDescriptionPairList.AddPair(TemporaryStorageDeclarationTypeList.Codes.PresentationNotification, TemporaryStorageDeclarationTypeList.Descriptions.PresentationNotification);
			return codeDescriptionPairList;
		}

		public ZString GetDefaultDeclarationType(TemporaryStorageMessageSendingObject sendingObject)
		{
			var result = ZString.Empty;
			switch (sendingObject.Header.AMA_MessageType)
			{
				case PNTSMessageTypeList.Codes.CombinedTemporaryStorage:
					result = TemporaryStorageDeclarationTypeList.Codes.DeclarationAndPresentationNotification;
					break;
				case PNTSMessageTypeList.Codes.PreLodgedTempStorage:
					result = TemporaryStorageDeclarationTypeList.Codes.Declaration;
					break;
				case PNTSMessageTypeList.Codes.PresentationNotification:
					result = TemporaryStorageDeclarationTypeList.Codes.PresentationNotification;
					break;
			}
			return result;
		}

		protected override EU.Business.CusTempStorage.TemporaryStorageMessageBuilder GetTemporaryStorageMessageBuilder(EU.Business.CusTempStorage.TemporaryStorageMessageSendingObject messageSendingObject, EU.Business.CusTempStorage.TemporaryStorageMessageFunction messageFunction)
		{
			var header = messageSendingObject.Header;
			if (header is TemporaryStorageHeader ieHeader && ieHeader.IsUCC5)
			{
				return new UCC5.TemporaryStorageMessageBuilder((TemporaryStorageMessageSendingObject)messageSendingObject, messageFunction);
			}
			else
			{
				return new UCC6.TemporaryStorageMessageBuilder((TemporaryStorageMessageSendingObject)messageSendingObject, messageFunction);
			}
		}

		protected override IEnumerable<EU.Business.CusTempStorage.TemporaryStorageMessageFunction> GetMessageFunctions(EU.Business.CusTempStorage.TemporaryStorageMessageSendingObject messageSendingObject)
		{
			yield return new DeclarationMessageFunction();
			yield return new InvalidationMessageFunction();
			yield return new AmendmentMessageFunction();
			yield return new PresentationNotificationMessageFunction();
			yield return new GoodsStatusReportDeclarationMessageFunction();
		}

		protected override CodeDescriptionPairList GetMessageTypesCore(EU.Business.CusTempStorage.TemporaryStorageHeader header)
		{
			var messageType = header.AMA_MessageType.ToUpperInvariant();
			var messageStatus = header.AMA_MessageStatus.ToUpperInvariant();
			var customsStatus = header.CustomsStatus.ToUpperInvariant();
			return header.Factory.GetCachedValue($"IE.Business.CusTempStorage.TemporaryStorageMessagingProvider.{messageType}.{messageStatus}.{customsStatus}", () =>
			{
				var result = new CodeDescriptionPairList();
				switch (messageType)
				{
					case PNTSMessageTypeList.Codes.CombinedTemporaryStorage:
					case PNTSMessageTypeList.Codes.PreLodgedTempStorage:
						result.AddPair(IETemporaryStorageMessageTypeList.Codes.Amendment, IETemporaryStorageMessageTypeList.Descriptions.Amendment);
						result.AddPair(IETemporaryStorageMessageTypeList.Codes.Invalidation, IETemporaryStorageMessageTypeList.Descriptions.Invalidation);
						result.AddPair(IETemporaryStorageMessageTypeList.Codes.Declaration, IETemporaryStorageMessageTypeList.Descriptions.Declaration);
						break;
					case PNTSMessageTypeList.Codes.PresentationNotification:
						result.AddPair(IETemporaryStorageMessageTypeList.Codes.Amendment, IETemporaryStorageMessageTypeList.Descriptions.Amendment);
						result.AddPair(IETemporaryStorageMessageTypeList.Codes.Invalidation, IETemporaryStorageMessageTypeList.Descriptions.Invalidation);
						result.AddPair(IETemporaryStorageMessageTypeList.Codes.PresentationNotification, IETemporaryStorageMessageTypeList.Descriptions.PresentationNotification);
						break;
					default:
						result.AddPair(IETemporaryStorageMessageTypeList.Codes.Declaration, IETemporaryStorageMessageTypeList.Descriptions.Declaration);
						result.AddPair(IETemporaryStorageMessageTypeList.Codes.Invalidation, IETemporaryStorageMessageTypeList.Descriptions.Invalidation);
						result.AddPair(IETemporaryStorageMessageTypeList.Codes.Amendment, IETemporaryStorageMessageTypeList.Descriptions.Amendment);
						result.AddPair(IETemporaryStorageMessageTypeList.Codes.PresentationNotification, IETemporaryStorageMessageTypeList.Descriptions.PresentationNotification);
						result.AddPair(IETemporaryStorageMessageTypeList.Codes.GoodsStatusReportDeclaration, IETemporaryStorageMessageTypeList.Descriptions.GoodsStatusReportDeclaration);
						break;
				}
				return result;
			});
		}

		protected override ZString GetDefaultMessageTypeCore(EU.Business.CusTempStorage.TemporaryStorageMessageSendingObject sendingObject)
		{
			var result = ZString.Empty;
			var header = sendingObject.Header;

			switch (header.AMA_MessageType)
			{
				case PNTSMessageTypeList.Codes.CombinedTemporaryStorage:
				case PNTSMessageTypeList.Codes.PreLodgedTempStorage:
					result = header.CustomsStatus != AISEntryStatusList.Codes.Registered ? IETemporaryStorageMessageTypeList.Codes.Declaration : IETemporaryStorageMessageTypeList.Codes.Amendment;
					break;
				case PNTSMessageTypeList.Codes.PresentationNotification:
					result = IETemporaryStorageMessageTypeList.Codes.PresentationNotification;
					break;
			}

			return result;
		}
	}
}
