using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.DocumentWrappers.Freight
{
	public class AdsParticipantFromDeclarationHelper : IAdsParticipant
	{
		public AdsParticipantFromDeclarationHelper(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		ZDecimal IAdsParticipant.TotalWeightAds
		{
			get { return new ZWeight(declaration.JE_TotalWeight, declaration.JE_TotalWeightUnit).InKilogramsSafe; }
		}

		ZString IAdsParticipant.CtStatus
		{
			get { return declaration.ZG_CTStatusID; }
		}

		ZString IAdsParticipant.HouseBill
		{
			get { return declaration.JE_HouseBill; }
		}

		ZString IAdsParticipant.Origin
		{
			get { return declaration.Origin.ToIataSafe(); }
		}

		ZString IAdsParticipant.FinalDestination
		{
			get { return declaration.FinalDestination.ToIataSafe(); }
		}

		OrgHeader IAdsParticipant.Consignor
		{
			get { return declaration.Consignor; }
		}

		OrgHeader IAdsParticipant.Consignee
		{
			get { return declaration.Consignee; }
		}

		ZInt IAdsParticipant.TotalNoOfPacks
		{
			get { return declaration.JE_TotalNoOfPacks; }
		}

		ZString IAdsParticipant.GoodsDescription
		{
			get { return declaration.JE_GoodsDescription; }
		}

		int IAdsParticipant.DeclarationsCount
		{
			get { return declaration.CustomsEntryHeaders.Count; }
		}

		ZString IAdsParticipant.DeclarationUCRs
		{
			get
			{
				var ducrs = string.Empty;
				if (declaration.ZG_CTStatusID == ExportCommunityTransitStatusList.Codes.C)
				{
					ducrs = Helpers.CStatusGoods;
				}
				else
				{
					var sb = new ZStringBuilder();
					var sortedEntries = declaration.CustomsEntryHeaders;
					sortedEntries.Sort(CusEntryHeaderSchema.CH_BGMReference.Name);
					foreach (CusEntryHeader entry in sortedEntries)
					{
						sb.AppendLine(entry.CH_BGMReference);
					}
					ducrs = sb.ToString();
				}
				return ducrs;
			}
		}

		ZString IAdsParticipant.ChiefEntryReferences
		{
			get
			{
				var sb = new ZStringBuilder();
				if (declaration.ZG_CTStatusID != ExportCommunityTransitStatusList.Codes.C)
				{
					var sortedEntries = declaration.CustomsEntryHeaders;
					sortedEntries.Sort(CusEntryHeaderSchema.CH_BGMReference.Name);
					foreach (CusEntryHeader entry in sortedEntries)
					{
						sb.AppendLine(Helpers.GetEntryNumberAndDate(entry.CusEntryNumber));
					}
				}
				return sb.ToString();
			}
		}

		readonly JobDeclaration declaration;
	}
}
