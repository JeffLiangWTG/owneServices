using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public class PreValidateTraderUniversalEventBuilder
	{
		public PreValidateTraderUniversalEventBuilder(IPreValidateTraderDataProvider dataProvider)
		{
			this.dataProvider = Argument.NotNull(dataProvider, nameof(dataProvider));
		}

		public UniversalEvent BuildUniversalEvent()
		{
			var universalEvent = new UniversalEvent
			{
				EventTime = ZDateTimeOffset.Now,
				EventType = Events.ExternalValidationNotCompletedCode,
				EventReference = CreateEventReference(),
				DataContext = CreateDataContext(),
				ContextCollection = GetContextCollection()
			};

			return universalEvent;
		}

		ZString CreateEventReference()
		{
			var parameters = new Dictionary<string, string>
			{
				{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, PreValidateTraderHelper.Constants.MessageType },
				{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Service, PreValidateTraderHelper.Constants.Service }
			};
			return StmALog.GenerateEventReference(ZString.Empty, parameters);
		}

		IDataContextDataObject CreateDataContext() => dataProvider.GetDataContext();
		List<Context> GetContextCollection() => dataProvider.GetContextCollection();

		readonly IPreValidateTraderDataProvider dataProvider;
	}
}

