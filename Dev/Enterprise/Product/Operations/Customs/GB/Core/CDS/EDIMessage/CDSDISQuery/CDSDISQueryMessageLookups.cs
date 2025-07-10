using Enterprise.Customs.GB.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSDISQueryMessageLookups : EDIMessageLookups
	{
		public CDSDISQueryMessageLookups(AutoEDIMessage parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ProfileList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var credentials = GBExtensions.GetFullBadgeProfile();
				credentials.ForEach((y) => result.Add(new CodeDescriptionPair(y, string.Empty)));
				return result;
			}
		}

		public CodeDescriptionPairList DeclarationCategories => Factory.GetCachedValue("CDSDISQueryMessage.DeclarationCategories", () =>
		{
			return new CodeDescriptionPairList()
			{
				new CodeDescriptionPair(CDSDISQueryHelper.Constants.DeclarationCategories.IM, Res.GetString("CC7D0C1D-10A5-41E2-A1C3-975F15DBF132", "IM")),
				new CodeDescriptionPair(CDSDISQueryHelper.Constants.DeclarationCategories.EX, Res.GetString("9C9A1EAE-3C7B-4A05-AC73-10A9A2B0809D", "EX")),
				new CodeDescriptionPair(CDSDISQueryHelper.Constants.DeclarationCategories.CO, Res.GetString("8031201A-556C-468D-AA23-1E2C9A90E088", "CO")),
				new CodeDescriptionPair(CDSDISQueryHelper.Constants.DeclarationCategories.ALL, Res.GetString("F2815D38-43BC-475A-BFC5-6CA885FEE799", "ALL")),
			};
		});

		public CodeDescriptionPairList DeclarationStatuses => Factory.GetCachedValue("CDSDISQueryMessage.DeclarationStatuses", () =>
		{
			return new CodeDescriptionPairList()
			{
				new CodeDescriptionPair(CDSDISQueryHelper.Constants.DeclarationStatuses.Cleared, Res.GetString("38F87A7B-F937-45A4-96CB-644126534E16", "Cleared")),
				new CodeDescriptionPair(CDSDISQueryHelper.Constants.DeclarationStatuses.Uncleared, Res.GetString("A1716043-7EF3-46CF-B7F6-14DFD761A088", "Uncleared")),
				new CodeDescriptionPair(CDSDISQueryHelper.Constants.DeclarationStatuses.Rejected, Res.GetString("F1796269-820E-45D5-8346-336A0EAF3415", "Rejected")),
				new CodeDescriptionPair(CDSDISQueryHelper.Constants.DeclarationStatuses.All, Res.GetString("04FAEA8C-42D6-4906-AFE1-42F0D29BD67E", "All")),
			};
		});
	}
}
