using CargoWise.Customs.CL.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.CL.Manifest.Business
{
	class BLSendChileWrapperObservation : IDocObservation
	{
		internal BLSendChileWrapperObservation(ZString reasonType, ZString reason)
		{
			Name = reasonType;
			Description = reason;
		}

		public string Name { get; }

		public string Description { get; }
	}
}
