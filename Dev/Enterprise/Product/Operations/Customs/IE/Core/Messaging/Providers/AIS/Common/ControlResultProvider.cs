using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Messaging.AIS
{
	public sealed class ControlResultProvider
	{
		readonly IControlResultType controlResult;

		public ControlResultProvider(IControlResultType controlResult)
		{
			this.controlResult = controlResult;
		}

		public ZString SequenceNumber => controlResult.SequenceNumber;

		public ZString RiskAreaCode => controlResult.RiskAreaCode;

		public ZString ControlType => controlResult.ControlType;

		public ZDate ControlDate => (controlResult.ControlDate).ConvertToZDate();

		public ZString Remarks => controlResult.Remarks;

		public IReadOnlyCollection<ControlDetailsProvider> ControlDetails => controlDetails ?? (controlDetails = controlResult.ControlDetails?.Select(x => new ControlDetailsProvider(x)).ToArray() ?? Array.Empty<ControlDetailsProvider>());
		IReadOnlyCollection<ControlDetailsProvider> controlDetails;
	}
}
