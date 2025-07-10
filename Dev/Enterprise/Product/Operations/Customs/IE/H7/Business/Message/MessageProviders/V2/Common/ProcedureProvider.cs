using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.H7.Business
{
	public class ProcedureProvider : IProcedure
	{
		public ProcedureProvider(AsycudaPackedItem packedItem)
		{
			this.packedItem = packedItem;
		}

		readonly AsycudaPackedItem packedItem;

		public string RequestedProcedure => null;

		public string PreviousProcedure => null;

		public IReadOnlyCollection<ICcQualifierAdditionalProcedure> AdditionalProcedure => additionalProcedure ?? (additionalProcedure =
			MessageProviderHelper.GetAdditionalProcedures(packedItem).Select((x, i) => new AdditionalProcedureProvider(i + 1, x)).ToArray());
		IReadOnlyCollection<ICcQualifierAdditionalProcedure> additionalProcedure;
	}
}
