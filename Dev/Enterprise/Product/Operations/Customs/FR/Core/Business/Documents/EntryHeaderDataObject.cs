using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.FR.Business.Documents
{
	public class EntryHeaderDataObject : EU.Business.Documents.DocDataObjects.EntryHeaderDataObject
	{
		public EntryHeaderDataObject(CusEntryHeader entry) : base(entry)
		{
			dv1Detail = entry.EntryInstruction?.DV1DetailsPivots.Cast<NonPersistentCusDV1DetailPivot>().FirstOrDefault(x => x.IsForEntryInstruction);
			var dv1DetailFound = dv1Detail != null;
			HasRelatedParty = dv1DetailFound && dv1Detail.Relationship == YesNoList.Codes.Yes;
			HasRelatedParty2 = dv1DetailFound && dv1Detail.PriceInfluence == YesNoList.Codes.Yes;
			HasRestrictions = dv1DetailFound && dv1Detail.Restrictions == YesNoList.Codes.Yes;
			HasSaleConditions = dv1DetailFound && dv1Detail.Consideration == YesNoList.Codes.Yes;
			HasRoyaltyCharges = dv1DetailFound && dv1Detail.RoyaltiesLicence == YesNoList.Codes.Yes;
			HasDisposal = dv1DetailFound && dv1Detail.Resale == YesNoList.Codes.Yes;
		}

		readonly NonPersistentCusDV1DetailPivot dv1Detail;

		protected override ZString PreviousCustomsDecisionsCore => dv1Detail?.CustomsDecisionNumber ?? ZString.Empty;

		protected override ZString Q7cDetailsCore => dv1Detail?.RelationDetails ?? ZString.Empty;

		protected override ZString Q8bDetailsCore => dv1Detail?.RestrictionConsiderationDetails ?? ZString.Empty;

		protected override ZString Q9bDetailsCore
		{
			get
			{
				var royaltiesLicenceDetails = dv1Detail?.RoyaltiesLicenceDetails ?? ZString.Empty;
				var resaleDetails = dv1Detail?.ResaleDetails ?? ZString.Empty;
				var result = new ZStringBuilder();
				if (!royaltiesLicenceDetails.IsEmpty)
				{
					if (resaleDetails.IsEmpty)
					{
						result.Append(royaltiesLicenceDetails);
					}
					else
					{
						result.AppendLine(royaltiesLicenceDetails);
					}
				}

				result.Append(resaleDetails);
				return result.ToString();
			}
		}

		protected override ZBool PrintDefaultFlagCore => dv1Detail != null;

		protected override ZBool PrintQ7CFlagCore => false;
	}
}
