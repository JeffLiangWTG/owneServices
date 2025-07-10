using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.AESVersion3_0
{
	public class EXPEXTHeaderProvider : AESHeaderProvider, IEXPEXTHeader
	{
		public EXPEXTHeaderProvider(ExportEntryMessageSendingAction action) : base(action?.MessagingObject)
		{
			this.action = action;
		}
		readonly ExportEntryMessageSendingAction action;

		public DateTime? ExitDate => action.ExitDate.ToNullableDateTime()?.Date;

		public DateTime? IntendedExitDate => ExportExitTypeList.Is2(action.ExitType) ? action.ExitDate.ToNullableDateTime()?.Date : null;

		public string Annotation => action.Annotation;

		public string ExitType => action.ExitType;

		public string ActualExitCustomsOffice => ExportExitTypeList.Is4(action.ExitType) ? action.ExitCustomsOffice : null;

		public bool DeclarantSpecified
		{
			get
			{
				var result = EntryInstruction.Constellation3rdDigitIs0();
				if (!result)
				{
					result = EntryInstruction.Invoices.SelectMany(x => x.AdditionalInfos.Cast<AdditionalInfo>()).Any(x => x.CSI_Code == UniversalReferenceConstants.AdditionalInfoCodes.X0001 && x.CSI_SubType == AdditionalDocTypeList.Codes.AdditionalInformation);
				}
				return result;
			}
		}

		public bool DeclarantContactPersonSpecified => EntryInstruction.Constellation3rdDigitIs1();

		IAESParty IAESHeader.Declarant => CachedValueHelper.GetValue(ref declarantCached, () => !Declaration.Declarant.Header.GetEUEoriDetails().IsEmpty ? PartyProvider.NewOrNull(Declaration.Declarant, GlbStaff.CurrentUser) : null);
		CachedValue<IAESParty> declarantCached;

		public IAESParty ExitCarrier => CachedValueHelper.GetValue(ref exitCarrierCached, () => ExportExitTypeList.Is4(action.ExitType) ? PartyProvider.NewOrNull(Declaration.CarrierEUBorderDocAddress.Address) : null);
		CachedValue<IAESParty> exitCarrierCached;

		public IReadOnlyCollection<IAlternativeEvidence> AlternativeEvidences => alternativeEvidences ?? (alternativeEvidences = ExportExitTypeList.Is4(action.ExitType)
			? action.AlternativeEvidences.Cast<AlternativeEvidence>().GroupBy(x => x.EvidenceType, x => x, (type, evidences) => new AlternativeEvidenceProvider(evidences)).ToArray()
			: Array.Empty<IAlternativeEvidence>());
		IReadOnlyCollection<IAlternativeEvidence> alternativeEvidences;
	}
}
