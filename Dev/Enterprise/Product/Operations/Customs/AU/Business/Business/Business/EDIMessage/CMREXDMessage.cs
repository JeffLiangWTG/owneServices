using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMREXDMessage : CMRMessage
	{
		public CMREXDMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.EXD;
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (!IsInDatabase && !IsDeleted)
			{
				var declaration = EM_LinkedObject as JobDeclaration;
				if (declaration != null)
				{
					oldJE_EntryStatus = declaration.JE_EntryStatus;
					EM_ApplicationReference = EM_MessageNum;

					switch (EM_MessageSubType)
					{
						case MessageSubTypes.Original:
							declaration.JE_EntryStatus = CustomsEntryStatus.AwaitingOriginal.Code;
							Logs.AddNew(Events.DeclarationQueued);
							break;
						case "REP":
						case CMRMessage.MessageSubTypes.Amendment:
							declaration.JE_EntryStatus = CustomsEntryStatus.AwaitingReplacement.Code;
							Logs.AddNew(Events.DeclarationAmendmentQueued);
							break;
						case MessageSubTypes.Withdraw:
							declaration.JE_EntryStatus = CustomsEntryStatus.AwaitingWithdrawal.Code;
							Logs.AddNew(Events.DeclarationCancellationQueued);
							break;
					}
				}

				var entryHeader = EM_LinkedObject as CusEntryHeader;
				if (entryHeader != null)
				{
					oldJE_EntryStatus = entryHeader.Declaration.JE_EntryStatus;
					EM_ApplicationReference = EM_MessageNum;

					switch (EM_MessageSubType)
					{
						case MessageSubTypes.Original:
							entryHeader.Declaration.JE_EntryStatus = CustomsEntryStatus.AwaitingOriginal.Code;
							entryHeader.CH_Status = CustomsEntryStatus.AwaitingOriginal.Code;
							Logs.AddNew(Events.DeclarationQueued);
							break;
						case "REP":
						case CMRMessage.MessageSubTypes.Amendment:
							entryHeader.Declaration.JE_EntryStatus = CustomsEntryStatus.AwaitingReplacement.Code;
							entryHeader.CH_Status = CustomsEntryStatus.AwaitingReplacement.Code;
							Logs.AddNew(Events.DeclarationAmendmentQueued);
							break;
						case MessageSubTypes.Withdraw:
							entryHeader.Declaration.JE_EntryStatus = CustomsEntryStatus.AwaitingWithdrawal.Code;
							entryHeader.CH_Status = CustomsEntryStatus.AwaitingWithdrawal.Code;
							Logs.AddNew(Events.DeclarationCancellationQueued);
							break;
					}
				}
			}
		}

		string oldJE_EntryStatus;

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded)
			{
				ResetEntryStatusOfJobDeclaration();
			}
			oldJE_EntryStatus = null;
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				ResetEntryStatusOfJobDeclaration();
			}
			oldJE_EntryStatus = null;

			base.Delete();
		}

		void ResetEntryStatusOfJobDeclaration()
		{
			if (EM_LinkedObject is JobDeclaration declaration && oldJE_EntryStatus != null)
			{
				declaration.JE_EntryStatus = oldJE_EntryStatus;
			}

			if (EM_LinkedObject is CusEntryHeader cusEntryHeader && oldJE_EntryStatus != null)
			{
				cusEntryHeader.Declaration.JE_EntryStatus = oldJE_EntryStatus;
			}
		}

		#endregion

	}
}
