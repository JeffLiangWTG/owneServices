using CargoWise.Types;
using Enterprise.Customs.GB.Business;

namespace Enterprise.Customs.GB.CDS
{
	public abstract partial class CDSInventoryLinkingRequestMessageBuilder
	{
		public static ZString NewMessageText(IUkCinvWrapper messageDataProvider, GbDes242MessageFunction how)
		{
			return GetBuilder(messageDataProvider, how)?.Build() ?? ZString.Empty;
		}

		static CDSInventoryLinkingRequestMessageBuilder GetBuilder(IUkCinvWrapper messageDataProvider, GbDes242MessageFunction how)
		{
			switch (how)
			{
				case GbDes242MessageFunction.MucrAssociate mucrAssociate:
					return new AssociateRequestMessageBuilder(messageDataProvider, mucrAssociate);
				case GbDes242MessageFunction.MucrDisAssociate mucrDisAssociate:
					return new DisAssociateRequestMessageBuilder(messageDataProvider, mucrDisAssociate);
				case GbDes242MessageFunction.MucrClose _:
					return new CloseRequestMessageBuilder(messageDataProvider);
				case GbInventoryManagementMessageFunction.ArrivalActual arrivalActual:
					return new ArrivalActualRequestMessageBuilder(messageDataProvider, arrivalActual.Level);
				case GbInventoryManagementMessageFunction.ArrivalAnticipated arrivalAnticipated:
					return new ArrivalAnticipatedRequestMessageBuilder(messageDataProvider, arrivalAnticipated.Level);
				case GbInventoryManagementMessageFunction.Departure departure:
					return new DepartureRequestMessageBuilder(messageDataProvider, departure.Level);
				case GbDes242MessageFunction.QueryMasterDEC _:
					return new MUCRQueryDeclarationRequestMessageBuilder(messageDataProvider);
				default:
					return null;
			}
		}
	}
}
