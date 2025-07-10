using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.OperationalActions;
using Enterprise.Customs.FR.Messaging.Interfaces.COD;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.COD
{
	public class GenWrapper : IGen
	{
		public GenWrapper(CreditCODDataObject item)
		{
			this.item = item;
		}
		readonly CreditCODDataObject item;

		public ZString EntryNumber => item.PreviousEntryHeader.EntryNumber;

		public ZString Direction => item.PreviousEntryHeader.Declaration.JE_MessageType;

		public ZString Numcod => item.PreviousEntryHeader.Declaration.GetVariousOperationCreditNumberIncludingExpired();

		public ZString Opecod => item.PreviousEntryHeader.Importer?.Organisation?.GetRegoCodeOfThisOrg(OrgCusCode.FranceCodeTypes.Siret, Core.Constants.CountryCodes.France) ?? ZString.Empty;
	}
}
