using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AIS
{
	public sealed class ItemControlResultsProvider
	{
		readonly IItemControlResultsType itemControlResultsObject;

		public ItemControlResultsProvider(IItemControlResultsType itemControlResultsObject)
		{
			this.itemControlResultsObject = itemControlResultsObject;
		}

		public ZString SequenceNumber => itemControlResultsObject.SequenceNumber;

		public ZString DeclarationGoodsItemNumber => itemControlResultsObject.DeclarationGoodsItemNumber;

		public ZString ControlResultCode => itemControlResultsObject.ControlResultCode;

		public IReadOnlyCollection<ControlResultProvider> ResultsOfControl => resultsOfControl ?? (resultsOfControl = itemControlResultsObject.ResultsOfControl?.Select(x => new ControlResultProvider(x)).ToArray() ?? Array.Empty<ControlResultProvider>());
		IReadOnlyCollection<ControlResultProvider> resultsOfControl;
	}
}
