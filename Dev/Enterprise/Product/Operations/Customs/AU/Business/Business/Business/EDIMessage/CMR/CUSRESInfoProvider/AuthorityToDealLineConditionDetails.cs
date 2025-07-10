using CargoWise.Types;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AuthorityToDealLineConditionDetails
	{
		public AuthorityToDealLineConditionDetails(SegmentGroup13 group13)
		{
			this.group13 = group13;
		}
		readonly SegmentGroup13 group13;

		#region ID

		public ZString ID
		{
			get
			{
				if (group13 != null && fID.IsEmpty)
				{
					foreach (ERPSegment eRP in group13.ERP)
					{
						fID = eRP.ErrorPointDetails.MessageSubItemNumber;
						break;
					}
				}

				return fID;
			}
		}
		ZString fID;

		#endregion

		#region Location

		public ZString[] Locations
		{
			get
			{
				if (group13 != null && fLocations == null)
				{
					fLocations = new ZString[group13.ERC.Count];

					int i = 0;
					foreach (ERCSegment eRC in group13.ERC)
					{
						fLocations[i] = eRC.ApplicationErrorDetail.ApplicationErrorIdentification;
						i++;
					}
				}

				return fLocations;
			}
		}
		ZString[] fLocations;

		#endregion

		#region Description

		public ZString Description
		{
			get
			{
				if (group13 != null && fDescription.IsEmpty)
				{
					foreach (FTXSegment fTX in group13.FTX)
					{
						fDescription = fTX.TextLiteral.FreeTextValue1;
						break;
					}
				}

				return fDescription;
			}
		}
		ZString fDescription;

		#endregion
	}
}
