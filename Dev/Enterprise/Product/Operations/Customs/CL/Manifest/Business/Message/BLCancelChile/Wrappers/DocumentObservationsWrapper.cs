using CargoWise.Customs.CL.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.CL.Manifest.Business
{
	internal class DocumentObservationsWrapper : IDocumentObservation
	{
		internal DocumentObservationsWrapper(ZString reason)
		{
			Description = reason;
		}

		public string Description { get; }
	}
}
