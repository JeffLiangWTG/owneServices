using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;

namespace Enterprise.Customs.DE.Business
{
	public abstract class StatusRequestSender
	{
		public static StatusRequestSender New(StatusRequest statusRequest) => statusRequest.Module == ExportStatusRequestModuleCodeList.Codes.NCTS
			? (StatusRequestSender)Activator.CreateInstance(ObjectFactory.GetType("DENCTSStatusRequestSender"), statusRequest)
			: new AesStatusRequestSender(statusRequest);

		protected readonly StatusRequest statusRequest;

		protected StatusRequestSender(StatusRequest statusRequest)
		{
			this.statusRequest = Argument.NotNull(statusRequest, nameof(statusRequest));
		}

		public void Send()
		{
			var builder = MessageBuilder;
			var provider = Provider;
			if (builder != null && provider != null)
			{
				var messageBuilder = builder.Invoke(provider);
				statusRequest.SetEM_MessageTextOrDataSource(messageBuilder.GetXMLMessage());
				statusRequest.EM_ApplicationReference = messageBuilder.MessageTechnicalName;

				statusRequest.SetLogbookRegistrationNumber(provider.MRN);
				statusRequest.SetLogbookEORIBranchSuffix(provider.InterchangeSender.EoriBranchSuffix);
			}
		}

		protected abstract IStatusRequestHeader Provider { get; }

		protected abstract Func<IStatusRequestHeader, IProduceMessageXml> MessageBuilder { get; }
	}
}
