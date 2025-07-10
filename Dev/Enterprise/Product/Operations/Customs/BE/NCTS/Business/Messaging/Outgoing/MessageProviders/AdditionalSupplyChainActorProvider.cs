using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class AdditionalSupplyChainActorProvider : IAdditionalSupplyChainActor
	{
		readonly CusReference reference;

		public AdditionalSupplyChainActorProvider(CusReference reference, ZInt sequenceNumber)
		{
			this.reference = Argument.NotNull(reference, nameof(reference));
			SequenceNumber = sequenceNumber;
		}

		public int SequenceNumber { get; }

		public string Role => reference.CFR_Code;

		public string IdentificationNumber => reference.CFR_Reference;
	}
}
