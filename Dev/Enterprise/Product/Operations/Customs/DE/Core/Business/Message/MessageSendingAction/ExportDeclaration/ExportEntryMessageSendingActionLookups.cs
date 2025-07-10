using System.Collections.Immutable;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ExportEntryMessageSendingActionLookups : ZLookups
	{
		public ExportEntryMessageSendingActionLookups(ExportEntryMessageSendingAction action) : base(action)
		{
		}

		new ExportEntryMessageSendingAction Parent => (ExportEntryMessageSendingAction)base.Parent;

		public CodeDescriptionPairList EntryTypeList
		{
			get
			{
				var parent = Parent;
				bool isMRNEmpty = parent.MovementReferenceNumber.IsEmpty;
				bool isEntryStatusEmpty = parent.EntryStatus.IsEmpty;
				var isEXTAvailable = EntryStatusValidForEXT.Contains(parent.EntryStatus) || (isEntryStatusEmpty && !isMRNEmpty);
				var isDATNotAvailable = isEntryStatusEmpty && !isMRNEmpty;

				return Factory.GetCachedValue("DE.ExportEntryMessageSendingActionLookups.ExportEntryTypeList_" + isEXTAvailable + isDATNotAvailable, () =>
				{
					var result = new ExportEntryTypeList();
					if (!isEXTAvailable)
					{
						result.RemoveCode(ExportEntryTypeList.Codes.ExitToExport);
					}
					if (isDATNotAvailable)
					{
						result.RemoveCode(ExportEntryTypeList.Codes.ExportDeclaration);
					}
					return result;
				});
			}
		}

		public CodeDescriptionPairList ExitTypeList => Factory.GetCachedValue<ExportExitTypeList>();

		public CustomsOfficeCodeCollection ExitCustomsOfficeList => EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRoles(Factory, EuOfficeCodesTypes.Codes.OfficeOfExit, EuOfficeCodesTypes.Codes.OfficeOfExitInland);

		public CodeDescriptionPairList SecurityTypeList
		{
			get
			{
				var isExportToSpecialTerritory = (Parent.MessagingObject?.Declaration?.JE_EntryStyle ?? ZString.Empty) == EntryStyleListExport.Codes.ExportToSpecialTerritory;
				return Factory.GetCachedValue("SecurityTypeList_" + isExportToSpecialTerritory, () =>
				{
					var result = new ExportSecurityTypeList();
					if (isExportToSpecialTerritory)
					{
						result.RemoveCode(ExportSecurityTypeList.Codes.EXS);
					}
					return result;
				});
			}
		}

		ImmutableHashSet<string> EntryStatusValidForEXT => Factory.GetCachedValue("DE.ExportEntryMessageSendingActionLookups.EntryStatusValidForEXT", // Cache Key
			() => ImmutableHashSet.Create(
				"500", "501", "502"//Change this when a codedescriptionpairlist is created
			)
		);
	}
}
