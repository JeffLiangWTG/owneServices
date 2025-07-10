
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	public class EDIRefZoneHeaderModule : InternationalZonesModule
	{
		#region Construction

		public EDIRefZoneHeaderModule() : base() { }

		#endregion

		#region Standard Module overrides

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new EDIRefZoneHeaderFilterBusinessObject();
		}
		internal FilterBusinessObject InternalGetNewFilterBusinessObject() => GetNewFilterBusinessObject();

#endregion

	}
}
