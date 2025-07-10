
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CARSTandDSAStatusCalculator : CMRStatusCalculator<PackingGroup>
	{
		public CARSTandDSAStatusCalculator(PackingGroup pivot)
			: base(pivot)
		{
		}

		protected internal override ZString[] InterestedMessageTypes
		{
			get
			{
				return new ZString[]
					{
						CMRMessage.CMRMessageTypes.CARST,
						CMRMessage.CMRMessageTypes.DSA
					};
			}
		}

		protected internal override ZPropertyInfo StatusInfo => Parent.CR_CargoStatusInfo;

		protected internal override ZString StatusChangedEventLogPrefix => "Consolidated Cargo Status - ";

		protected override ZString GetDefaultStatus() => ZString.Empty;

		protected override void DeriveStatusCore()
		{
			base.DeriveStatusCore();
			JobDeclaration declaration = Parent.Declaration;
			if (declaration != null)
			{
				declaration.JE_ConsolidatedCargoStatus = Parent.CR_CargoStatus;
				ZString abbreviatedCargoStatusDescription = ZString.Empty;
				foreach (PackingGroup pack in declaration.PackingGroups)
				{
					if (abbreviatedCargoStatusDescription.IsEmpty)
					{
						abbreviatedCargoStatusDescription = pack.AbbreviatedCargoStatusDescription;
					}
					else if (abbreviatedCargoStatusDescription != pack.AbbreviatedCargoStatusDescription)
					{
						declaration.JE_ConsolidatedCargoStatus = CMRConsolidatedCargoStatuses.Codes.SeePackingDetails;
						break;
					}
				}

				var entryHeaders = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().ToArray();
				foreach (var entryHeader in entryHeaders)
				{
					if (entryHeader.Packages.Count > 0)
					{
						bool allPackingLinesAreCargoCleared = true;
						foreach (Package package in entryHeader.Packages)
						{
							if (package.PackingGroup == null || !package.PackingGroup.IsCargoStatusAvailableAndCargoClear)
							{
								allPackingLinesAreCargoCleared = false;
								break;
							}
						}

						if (allPackingLinesAreCargoCleared)
						{
							entryHeader.AddInfo.ZA_IsSubjectToRedLine_Hidden = false;
							if (entryHeader.AuthorityToDealMessage != null)
							{
								entryHeader.CH_EntryStatus = CMRImportEntryAdvice.ATDReceived.Code;
								declaration.ResetDeclarationStatus();
							}
						}
					}
				}
			}
		}

		protected override IEnumerable<EDIMessage> Messages
		{
			get
			{
				var result = base.Messages.Cast<EDIMessage>().ToList();

				var declaration = Parent.Declaration;
				if (declaration != null && declaration.EntryHeader != null)
				{
					result.AddRange(declaration.EntryHeader.GetRelatedDSAMessages());
				}

				return result;
			}
		}

		protected internal override ZString GetStatusFromInboundMessage(EDIMessage message)
		{
			return message is CMRCUSRESMessage ? GetCargoStatusCodeFromDescription(Parent.GetCargoStatusFromLatestMessage())
					: base.GetStatusFromInboundMessage(message);
		}
	}
}
