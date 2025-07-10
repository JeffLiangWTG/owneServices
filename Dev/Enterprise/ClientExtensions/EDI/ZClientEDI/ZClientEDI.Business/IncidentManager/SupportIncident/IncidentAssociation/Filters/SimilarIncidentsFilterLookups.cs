using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	public class SimilarIncidentsFilterLookups : ZLookups
	{
		public SimilarIncidentsFilterLookups(SimilarIncidentsFilter parent) : base(parent) { }

		#region IncidentStatus

		public CodeDescriptionPairList IncidentStatus
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(IncidentStatusCodes.All, IncidentStatusDescriptions.All);
				list.AddPair(IncidentStatusCodes.Closed, IncidentStatusDescriptions.Closed);
				list.AddPair(IncidentStatusCodes.Open, IncidentStatusDescriptions.Open);
				return list;
			}
		}

		public static class IncidentStatusCodes
		{
			public static MultilingualString All => ResString.GetMultilingualString("391d825e-fe51-4e52-b9d7-6869f1163a14", "All");
			public static MultilingualString Closed => ResString.GetMultilingualString("f4f234e5-355f-4e1a-b5c2-6027a4cbc317", "Closed");
			public static MultilingualString Open => ResString.GetMultilingualString("b7155423-c728-4ade-88e6-d20000a0e3e9", "Open");
		}

		public static class IncidentStatusCodeStrings
		{
			public static string All => IncidentStatusCodes.All.GetUnresolvedString();
			public static string Closed => IncidentStatusCodes.Closed.GetUnresolvedString();
			public static string Open => IncidentStatusCodes.Open.GetUnresolvedString();
		}

		public static class IncidentStatusDescriptions
		{
			public static MultilingualString All => ResString.GetMultilingualString("ca61f68c-6db6-4790-b338-46ff2b9ba78f", "All Incidents");
			public static MultilingualString Closed => ResString.GetMultilingualString("d9453c7a-1d5b-4de4-ab52-68f8068cd6b1", "Closed Incidents");
			public static MultilingualString Open => ResString.GetMultilingualString("ca9e342f-e9f7-4948-99d7-a114314b5805", "Open Incidents");
		}

		#endregion
	}
}

