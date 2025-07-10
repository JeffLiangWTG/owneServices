using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIOrgCodeLists : OrgCodeLists
	{
		public static void RegisterThisSubTypeOverride()
		{
			OverridableContactTypeListDelegate.Value = (list) =>
			{
				list.AddRange(new EDIOrgDocumentGroupTypes());
				return list;
			};
		}
	}
}
