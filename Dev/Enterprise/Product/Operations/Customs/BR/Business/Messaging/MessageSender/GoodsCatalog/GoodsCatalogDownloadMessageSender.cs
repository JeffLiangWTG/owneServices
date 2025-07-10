using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BR.Business
{
	public class GoodsCatalogDownloadMessageSender
	{
		public GoodsCatalogDownloadMessageSender(GoodsCatalogDownloadObject goodsCatalogDownload)
		{
			this.goodsCatalogDownload = Argument.NotNull(goodsCatalogDownload, nameof(goodsCatalogDownload));
		}

		readonly GoodsCatalogDownloadObject goodsCatalogDownload;

		BusinessObjectFactory factory => goodsCatalogDownload.Factory;

		public int SendMessagesAndSave()
		{
			var messages = new List<EDIMessage>();

			if (goodsCatalogDownload != null)
			{
				if (goodsCatalogDownload.DownloadCatalog)
				{
					messages.Add(CreateCustomsMessage(EDIMessageSubTypeList.Codes.CatalogZipFile));
					messages.Add(CreateCustomsMessage(EDIMessageSubTypeList.Codes.ManufacturerZipFile));

					goodsCatalogDownload.Owner?.Logs.AddNew(Events.DownloadCatalogRequestSent);
				}
				if (goodsCatalogDownload.DownloadForeignOperator)
				{
					messages.Add(CreateCustomsMessage(EDIMessageSubTypeList.Codes.OperatorZipFile));
				}

				try
				{
					factory.Save();
				}
				catch (ZSaveException ex)
				{
					messages.DeleteAll();
					ZExceptionReporting.HandleSaveException(ex);

					return 0;
				}
			}
			return messages.Count;
		}

		EDIMessage CreateCustomsMessage(string messageSubType)
		{
			goodsCatalogDownload.MessageType = messageSubType;
			return goodsCatalogDownload.CreateCustomsMessage();
		}

		public bool CanSendMessage() => goodsCatalogDownload.OwnerRootCNPJ is ZString ownerRootCNPJ && !ownerRootCNPJ.IsEmpty
			&& BRMessageHelper.AllMessagesHaveResponseAndBeenProcessed(factory,
				BRMessageHelper.GetOutgoingMessagesByApplicationReference(factory, ownerRootCNPJ,
					MessageTypeList.Codes.CAT,
					new[]
					{
						EDIMessageSubTypeList.Codes.CatalogZipFile,
						EDIMessageSubTypeList.Codes.ManufacturerZipFile,
						EDIMessageSubTypeList.Codes.OperatorZipFile
					}));
	}
}
