using System;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class CC170CProvider : NctsDepartureHeaderProvider, ICC170C
	{
		public CC170CProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public override string HolderOfTheTransitProcedureIdentificationNumber => !IsTIRInbondEntryType ? base.HolderOfTheTransitProcedureIdentificationNumber : null;

		public DateTime LimitDate => nctsHeader.MovementHeader.BM_ExportDate.IsValid ? DataProviderHelper.GetProviderDateTime(nctsHeader.MovementHeader.BM_ExportDate) : DateTime.MinValue;

		public IConsignmentType08 Consignment => consignment ?? (consignment = new ConsignmentType08Provider(nctsHeader));
		IConsignmentType08 consignment;

		public override string MessageType => Constants.MessageTypes.CC170C;
	}
}
