using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentMainLookups : AutoIncidentMainLookups
	{
		IncidentListHelper listHelper;

		public IncidentMainLookups(AutoIncidentMain parent)
			: base(parent)
		{
		}

		public IncidentListHelper ListHelper
		{
			get { return listHelper ?? (listHelper = new IncidentListHelper(Parent)); }
		}

		protected new IncidentMainBase Parent
		{
			get { return (IncidentMainBase)base.Parent; }
		}

		#region Status

		public CodeDescriptionPairList StatusList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				PopulateStatusList(result);
				return result;
			}
		}

		protected virtual void PopulateStatusList(CodeDescriptionPairList statusList)
		{
			statusList.AddPair(Status.Open, "Open");
			statusList.AddPair(Status.Working, "Working");
			statusList.AddPair(Status.Suspended, "Suspended");
			statusList.AddPair(Status.Closed, "Closed");
		}

		public static class Status
		{
			public const string Open = "OPN";
			public const string Working = "WRK";
			public const string Closed = "CLS";
			public const string NotClosed = "NCL";
			public const string ClosedDirectlyInSupport = "CCS";
			public const string Suspended = "SUS";
		}

		#endregion
	}
}


