using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class EDIGlbTimeAllocationLookups : GlbTimeAllocationLookups
	{
		protected EDIGlbTimeAllocationLookups(AutoGlbStaffHoliday parent)
			: base(parent)
		{
		}

		public new static EDIGlbTimeAllocationLookups New(AutoGlbStaffHoliday parent)
		{
			return new EDIGlbTimeAllocationLookups(parent);
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		#region Types

		protected override CodeDescriptionBoolCollection GetTypes()
		{
			CodeDescriptionBoolCollection result = new CodeDescriptionBoolCollection(base.GetTypes());
			result.Add(TrainingSession, (NoResString)"Training Session", false);
			return result;
		}

		public const string TrainingSession = "TRN";

		#endregion
	}
}

