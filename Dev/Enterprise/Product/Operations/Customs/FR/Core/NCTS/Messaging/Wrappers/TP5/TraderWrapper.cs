using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class TraderWrapper : ITrader
	{
		TraderWrapper(JobDocAddress address)
		{
			this.address = Argument.NotNull(address, nameof(address));
		}
		readonly JobDocAddress address;

		public string IdentificationNumber => identificationNumber ?? (identificationNumber = address.Address?.GetEORI().Left(17) ?? ZString.Empty);
		string identificationNumber;

		public string CommunicationLanguageAtDestination => communicationLanguageAtDestination ?? (communicationLanguageAtDestination = address.Organisation?.OH_Language ?? ZString.Empty);
		string communicationLanguageAtDestination;

		public static TraderWrapper New(JobDocAddress address) => address == null ? null : new TraderWrapper(address);
	}
}
