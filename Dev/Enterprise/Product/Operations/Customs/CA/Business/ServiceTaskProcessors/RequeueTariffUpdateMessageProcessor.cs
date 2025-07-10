using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class RequeueTariffUpdateMessageProcessor
	{
		public RequeueTariffUpdateMessageProcessor(ILogger serviceLogger)
		{
			logger = Argument.NotNull(serviceLogger, "serviceLogger");
		}
		readonly ILogger logger;

		public void Process()
		{
			using (var helper = new RefDbExtendedPropertyHelper())
			{
				var refreshRequired = helper.GetRefreshRequired();
				if (refreshRequired.HasValue && refreshRequired.Value)
				{
					var lastUpdateDate = helper.GetLastTariffDataUpdateDate();
					if (lastUpdateDate.IsValid)
					{
						var factory = new BusinessObjectFactory();

						var messages = GetEDIMessageForRequeue(factory, lastUpdateDate);
						var requeuedMessages = messages.Where(m => m.EM_Status != EDIMessage.Status.Queued && m.EM_SystemCreateTimeUtc.Date > lastUpdateDate);
						var requeuedMessageCount = requeuedMessages.Count();
						if (requeuedMessageCount > 0)
						{
							requeuedMessages.ForEach(m => m.EM_Status = EDIMessage.Status.Queued);

							try
							{
								factory.Save();
							}
							catch (ZSaveException e)
							{
								ZExceptionReporting.HandleSaveException(e);
							}

							logger.Information(string.Format(CultureInfo.CurrentCulture, "{0} tariff update messages were re-queued.", requeuedMessageCount));
						}

						if (messages.IsCountMoreThan(0))
						{
							helper.SetRefreshRequired(false);
						}
					}
				}
			}
		}

		readonly ZString[] tariffUpdateMessageSubTypes = new ZString[]
		{
			QueryMessageSubType3CharCodes.Codes.CLASSFILE,
			QueryMessageSubType3CharCodes.Codes.EXCISETAX,
			QueryMessageSubType3CharCodes.Codes.GSTFILE,
			QueryMessageSubType3CharCodes.Codes.TARIFFCODE
		};

		EDIMessage[] GetEDIMessageForRequeue(BusinessObjectFactory factory, ZDateTime lastUpdateDate)
		{
			var query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, lastUpdateDate);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.CAIMP);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.Query);
			query.AddToFilter(EDIMessageSchema.EM_MessageSubType, tariffUpdateMessageSubTypes);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);

			return factory.Load<EDIMessage>(query);
		}
	}
}
