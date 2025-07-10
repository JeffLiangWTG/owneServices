using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonTransitOperationWrapper : INCTSCommonTransitOperation
	{
		public NCTS5CommonTransitOperationWrapper(NctsHeader header)
		{
			nctsHeader = Argument.NotNull(header, nameof(header));
			departureMovement = Argument.NotNull(nctsHeader.MovementHeader, nameof(nctsHeader.MovementHeader));
		}
		protected readonly NctsHeader nctsHeader;
		protected readonly NctsDepartureMovementHeader departureMovement;

		public ZString DeclarationType => departureMovement.BM_InBondEntryType;

		public ZString TIRCarnetNumber => departureMovement.BM_InBondEntryType == NctsPhase5DeclarationTypeList.Codes.TIR ? departureMovement.TirCarnetNumber : ZString.Empty;

		public ZString Security => GetSecurityCode();

		ZString GetSecurityCode()
		{
			const string noSafetyCode = "0";
			const string safetyCode = "2";

			var code = ZString.Empty;
			switch (departureMovement.BM_TypeOfSecurity)
			{
				case NctsTypeOfSecurityList.Codes.NON:
					code = noSafetyCode;
					break;
				case NctsTypeOfSecurityList.Codes.EXI:
					code = safetyCode;
					break;
			}

			return code;
		}
	}
}
