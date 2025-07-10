using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.eServices.Business
{
	public class DeniedPartyList : AutoDeniedPartyList
	{
		public DeniedPartyList() { }

		public DeniedPartyList(BusinessObjectFactory factory)
			: base(factory) { }
	}

	public class DeniedPartyListValidation : AutoDeniedPartyListValidation
	{
		public DeniedPartyListValidation(AutoDeniedPartyList parent)
			: base(parent) { }

		#region Implementation

		public new DeniedPartyList Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (DeniedPartyList)base.Parent; }
		}

		#endregion
	}
}
