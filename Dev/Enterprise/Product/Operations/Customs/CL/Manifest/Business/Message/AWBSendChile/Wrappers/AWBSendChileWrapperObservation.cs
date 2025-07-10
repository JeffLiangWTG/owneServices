using CargoWise.Customs.CL.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.CL.Manifest.Business
{
	internal class AWBSendChileWrapperObservation : IDocObservations
	{
		internal AWBSendChileWrapperObservation(ZString reasonType, ZString reason)
		{
			Name = reasonType;
			Content = reason;
		}

		public string Name { get; }

		public string Content { get; }
	}
}
