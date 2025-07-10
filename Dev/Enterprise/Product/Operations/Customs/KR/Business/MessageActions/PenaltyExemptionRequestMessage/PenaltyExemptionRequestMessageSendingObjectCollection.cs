using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class PenaltyExemptionRequestMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<PenaltyExemptionRequestMessageSendingObject>
	{
		public PenaltyExemptionRequestMessageSendingObjectCollection(JobDeclaration declaration)
			: base(declaration.Factory)
		{
			this.declaration = declaration;
			PopulateElements();
		}
		readonly JobDeclaration declaration;

		void PopulateElements()
		{
			RemoveAll();
			foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
			{
				var entryNumWithMaxVersionNumber = entry.EntryNumbers.Cast<CusEntryNumber>().Where(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5UA).OrderByDescending(x => x.CE_EntryLineReference).FirstOrDefault();
				ZShort.TryParse(entryNumWithMaxVersionNumber?.CE_EntryLineReference ?? ZString.Empty, out ZShort maxVersionNumber);

				maxVersionNumber++;
				if (entry.EntryInstruction?.AmendmentSessionalDataCollection != null)
				{
					foreach (var sessionalData in entry.EntryInstruction.AmendmentSessionalDataCollection.Where(x => DutyTaxCorrectionCodeList.Is5UARelevant(x.CSI_Code) && x.PenaltyExemptionSessionalData != null))
					{
						if (sessionalData.PenaltyExemptionSessionalData.PenaltyExemptionCode != YesNoList.Codes.Yes && has5UANeverBeenSent(entry, sessionalData))
						{
							Add(new PenaltyExemptionRequestMessageSendingObject(entry, sessionalData, maxVersionNumber++));
						}
					}
				}
			}

			ZBool has5UANeverBeenSent(CusEntryHeader entry, AmendmentSessionalData sessionalData)
			{
				var message5UA = entry.Messages.GetLastMessageMatching(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5UA
																		&& x.EM_MessageOwner == sessionalData.Message5FE.EM_MessageNum);
				return message5UA == null || CustomsMessageStatusTypeList.IsOriginalMessageAllowed(message5UA?.MessageStatus);
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotImplementedException();
		protected override bool AllowNewCore => false;
	}
}
