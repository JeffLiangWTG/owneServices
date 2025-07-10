using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class DeltaIEJobDeclarationMessageSendingObjectLookups : ZLookups
	{
		public DeltaIEJobDeclarationMessageSendingObjectLookups(DeltaIEJobDeclarationMessageSendingObject parent)
			: base(parent)
		{
		}

		public new DeltaIEJobDeclarationMessageSendingObject Parent => (DeltaIEJobDeclarationMessageSendingObject)base.Parent;

		public CodeDescriptionPairList MessageSubTypeList
		{
			get
			{
				var entryStatus = Parent.Header.CH_EntryStatus;

				return Factory.GetCachedValue("Enterprise.Customs.FR.Business.MessageSending.MessageSubTypeList." + entryStatus, () =>
				{
					var result = new CodeDescriptionPairList();

					switch (entryStatus)
					{
						case "":
							result.Add(new CodeDescriptionPair(DeltaIESendMessageSubTypeList.Codes.ImportDeclaration, DeltaIESendMessageSubTypeList.Descriptions.ImportDeclaration));
							break;
						case DeltaIEImportCusEntryStatusList.Codes.DeclarationRegistered:
							result.AddRange(
								new CodeDescriptionPairList
								{
									new CodeDescriptionPair(DeltaIESendMessageSubTypeList.Codes.PresentationNotification, DeltaIESendMessageSubTypeList.Descriptions.PresentationNotification),
									new CodeDescriptionPair(DeltaIESendMessageSubTypeList.Codes.AmendmentRequest, DeltaIESendMessageSubTypeList.Descriptions.AmendmentRequest),
									new CodeDescriptionPair(DeltaIESendMessageSubTypeList.Codes.Invalidation, DeltaIESendMessageSubTypeList.Descriptions.Invalidation)
								});
							break;
						case DeltaIEImportCusEntryStatusList.Codes.Amending:
							result.Add(new CodeDescriptionPair(DeltaIESendMessageSubTypeList.Codes.AmendmentRequest, DeltaIESendMessageSubTypeList.Descriptions.AmendmentRequest));
							break;
						case DeltaIEImportCusEntryStatusList.Codes.DeclarationRejected:
							result.AddRange(
								new CodeDescriptionPairList
								{
									new CodeDescriptionPair(DeltaIESendMessageSubTypeList.Codes.ImportDeclaration, DeltaIESendMessageSubTypeList.Descriptions.ImportDeclaration),
									new CodeDescriptionPair(DeltaIESendMessageSubTypeList.Codes.AmendmentRequest, DeltaIESendMessageSubTypeList.Descriptions.AmendmentRequest)
								});
							break;
						default:
							result.AddRange(new DeltaIESendMessageSubTypeList());
							break;
					}

					return result;
				});
			}
		}

		public CodeDescriptionPairList MotivationForInvalidationList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, UniversalReferenceConstants.RefCusCodeListTypes.Codes.MotivationForInvalidationRequest, ZDate.Today, includeParentDataGrouping: false);
		public CodeDescriptionPairList MotivationForRectificationList
		{
			get
			{
				var resultFromDb = RefCusCodeListTypes.GetCachedList(
					Factory,
					Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE,
					UniversalReferenceConstants.RefCusCodeListTypes.Codes.MotivationForRectificationRequest,
					ZDate.Today,
					includeParentDataGrouping: false
				);

				var result = new CodeDescriptionPairList();
				var entryHeader = Parent.Header;
				var entryInstruction = entryHeader.EntryInstruction;

				if (entryHeader.CH_EntryStatus == DeltaIEImportCusEntryStatusList.Codes.DeclarationRegistered)
				{
					result.AddPair(FRConstants.RectificationMotivationTypes.PrelodgeRectification, resultFromDb.GetDescriptionFromCode(FRConstants.RectificationMotivationTypes.PrelodgeRectification));
				}
				else if (entryHeader.CH_EntryStatus == DeltaIEImportCusEntryStatusList.Codes.Released
						&& (entryInstruction.CEI_SubStyle == EntrySubstyleCodePairList.Codes.C || entryInstruction.CEI_SubStyle == EntrySubstyleCodePairList.Codes.F)
						&& entryInstruction.CEI_Procedure == FRConstants.ProcedureTypes.IntoBondedWarehouseProcedure)
				{
					foreach(CodeDescriptionPair pair in resultFromDb)
					{
						if (pair.Code != FRConstants.RectificationMotivationTypes.MissingDocuments)
						{
							result.AddPair(pair.Code, pair.Description);
						}
					}
				}
				else
				{
					result = resultFromDb;
				}

				return result;
			}
		}

		public CodeDescriptionPairList MotivationList => Parent.MessageType == DeltaIESendMessageSubTypeList.Codes.AmendmentRequest ? MotivationForRectificationList : MotivationForInvalidationList;
	}
}
