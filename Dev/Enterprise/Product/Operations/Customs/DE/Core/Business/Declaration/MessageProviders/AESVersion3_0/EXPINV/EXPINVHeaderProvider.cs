using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.AESVersion3_0
{
	public class EXPINVHeaderProvider : AESHeaderProvider, IEXPINVHeader
	{
		public EXPINVHeaderProvider(ExportEntryMessageSendingAction action) : base(action?.MessagingObject)
		{
			this.action = Argument.NotNull(action, nameof(action));
		}
		readonly ExportEntryMessageSendingAction action;

		string IAESHeader.LocalReferenceNumber => MRN.IsNullOrEmpty() ? EntryHeader.LocalReferenceNumber : null;

		public string InvalidationReason => action.Annotation;

		public IAESParty Exporter => null;

		IAESParty IAESHeader.Declarant => CachedValueHelper.GetValue(ref declarant, () =>
		{
			IAESParty result = null;
			var declarationDeclarant = Declaration.Declarant;
			if (declarationDeclarant != null && declarationDeclarant.Header.HasEUEoriRegNo() && EntryInstruction.Constellation3rdDigitIs0())
			{
				result = PartyProvider.NewOrNull(declarationDeclarant);
			}
			return result;
		});
		CachedValue<IAESParty> declarant;
	}
}
