using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class EcoRegimeAuthorizationWrapper : IEcoRegimeAuthorization
	{
		public EcoRegimeAuthorizationWrapper(Declaration.CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		}
		readonly Declaration.CusEntryHeader entryHeader;

		public ZString EcoRegimeAuthorizationNumber => entryHeader.EntryInstruction?.SpecificRegimeNumber ?? ZString.Empty;

		public ZString EcoRegimeCountryCode => entryHeader.EntryInstruction?.SpecificRegimeCountryCode ?? ZString.Empty;
	}
}
