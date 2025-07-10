using System;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CusAuthorizationUsage = Enterprise.Customs.EU.Business.CusAuthorizationUsage;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC170CProvider : NctsDepartureHeaderProvider, ICC170C
	{
		public CC170CProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public override string HolderOfTheTransitProcedureIdentificationNumber => !IsTIRInbondEntryType ? base.HolderOfTheTransitProcedureIdentificationNumber : null;

		public bool ReducedDatasetIndicator => nctsHeader.BH_FTZMove;

		public DateTime? LimitDate => CachedValueHelper.GetValue(ref limitDateCached, GetLimitDate);
		CachedValue<DateTime?> limitDateCached;

		public IConsignmentType08 Consignment => consignment ?? (consignment = new ConsignmentType08Provider(nctsHeader));
		IConsignmentType08 consignment;

		public override string MessageType => Constants.MessageTypes.CC170C;

		DateTime? GetLimitDate()
		{
			DateTime? result = null;
			var exportDate = nctsHeader.MovementHeader.BM_ExportDate;
			var cusAuthorizationUsages = nctsHeader.IsPhase5Departure ? (IBusinessObjectCollection<CusAuthorizationUsage>)nctsHeader.MovementHeader.CusAuthorizationUsages : nctsHeader.CusAuthorizationUsages;
			if (exportDate.IsValid && !cusAuthorizationUsages.Any(x => x.AGC_Code == Constants.CusPermitHeaderTypes.ACR))
			{
				result = exportDate.ToDateTime();
			}
			return result;
		}
	}
}
