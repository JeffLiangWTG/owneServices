using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IL.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Logging;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Business.Message.MessageBuilder
{
	public class ILGEN910MessageBuilder : ILMessageBuilderBase<OutgoingMessageRequestParams>
	{
		public ILGEN910MessageBuilder(GlbCompany company, ICommonLogger logger) : base(null)
		{
			this.company = Argument.NotNull(company, nameof(company));
			this.logger = Argument.NotNull(logger, nameof(logger));
			this.factory = company.Factory;
			this.dcaParameters = ILCustomsDataRegistry.Instance.DCAParametersForASyncMessage.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
		}

		protected override ILEDIMessage GetMessage()
		{
			var message = factory.New<ILGEN910RequestMessage>();
			message.EM_GB = company.Branches[0].PK;

			return message;
		}

		protected override IBusinessObjectCollection GetMessageOwnerCollection() => null;

		protected override string GetMessageText(OutgoingMessageRequestParams input)
		{
			var messageBuilder = new OutgoingMessageRequestMessageBuilder(OutgoingMessageRequestWrapper.New(int.Parse(PeekWayList.Codes._3), input.Name, dcaParameters.MaxMessagesPerIteration, input.FromDate, input.ToDate)) as IXmlMessageBuilder;
			return messageBuilder?.GenerateXmlMessage().GetSerializedString() ?? ZString.Empty;
		}

		protected override string GetMessageText()
		{
			var messageBuilder = new OutgoingMessageRequestMessageBuilder(OutgoingMessageRequestWrapper.New(int.Parse(PeekWayList.Codes._2), null, dcaParameters.MaxMessagesPerIteration, ZDateTime.Empty, ZDateTime.Empty)) as IXmlMessageBuilder;
			return messageBuilder?.GenerateXmlMessage().GetSerializedString() ?? ZString.Empty;
		}

		protected override IEnumerable<OutgoingMessageRequestParams> GetMessageGenerationInputs()
		{
			switch (dcaParameters.PeekWay)
			{
				case PeekWayList.Codes._2:
					{
						logger.Log(LogType.Information, $"Company {company.GC_Code}, Generate A-Sync Message - {PeekWayList.Descriptions._2}");
						yield return null;
					}
					break;
				case PeekWayList.Codes._3:
					{
						var currentUtcTime = ZDateTime.UtcNow;
						foreach (var service in dcaParameters.Services.Cast<DCAService>())
						{
							var fromDateUtc = CalculateFromDateUtc(currentUtcTime, service.NextRunDateTime);
							logger.Log(LogType.Information, $"Company {company.GC_Code}, Generate A-Sync Message - {PeekWayList.Descriptions._3} {service.Name} from date {fromDateUtc} to date {currentUtcTime}");
							yield return new OutgoingMessageRequestParams { Name = service.Name, FromDate = fromDateUtc, ToDate = currentUtcTime };
							UpdateRegistry(service, currentUtcTime);
						}
					}
					break;
				default:
					break;
			}
		}

		ZDateTime CalculateFromDateUtc(ZDateTime currentUtcTime, ZDateTime nextRunDateTime)
		{
			var fromDate = nextRunDateTime.IsEmpty
				? currentUtcTime.AddMinutes(-10)
				: nextRunDateTime;
			return fromDate;
		}

		void UpdateRegistry(DCAService service, ZDateTime now)
		{
			service.NextRunDateTime = now;
			ILCustomsDataRegistry.Instance.DCAParametersForASyncMessage.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, dcaParameters);
		}

		readonly BusinessObjectFactory factory;
		readonly GlbCompany company;
		readonly ICommonLogger logger;
		readonly DCAParameters dcaParameters;
	}

	public sealed class OutgoingMessageRequestParams
	{
		public string Name { get; internal set; }

		public ZDateTime FromDate { get; internal set; }

		public ZDateTime ToDate { get; internal set; }
	}
}
