using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Registry;
using Enterprise.Integration;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.IE.Business
{
	public abstract class SendingNotificationCommonProcessor : BranchCustomsApplicationTypeMessageProcessor
	{
		protected SendingNotificationCommonProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected IDisposable SetApplicationCodeForEmail(ZString applicationCode) => new DisposableAction(() =>
		{
			if (!applicationCode.IsEmpty)
			{
				currentApplicationCode = applicationCode;
			}
		}, () => { currentApplicationCode = null; });
		string currentApplicationCode;

		protected override IRegistryItem GetEmailGroupRegistryItem() => currentApplicationCode switch
		{
			EDIMessage.ApplicationCodes.IECustomsExport => EUCustomsDataRegistry.Instance.SendExportAcknowledgements,
			EDIMessage.ApplicationCodes.IECustomsUCC5Import or EDIMessage.ApplicationCodes.IECustomsImport => IECustomsDataRegistry.Instance.SendImportAcknowledgements,
			EDIMessage.ApplicationCodes.IECustomsNCTS => EUCustomsDataRegistry.Instance.SendNctsAcknowledgements,
			_ => base.GetEmailGroupRegistryItem(),
		};
	}
}
