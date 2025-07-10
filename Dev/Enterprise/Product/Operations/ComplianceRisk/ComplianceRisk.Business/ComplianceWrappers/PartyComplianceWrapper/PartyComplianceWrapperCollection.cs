using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ComplianceRisk.Business
{
	public class PartyComplianceWrapperCollection : NonPersistentBusinessObjectCollection<PartyComplianceWrapper>
	{
		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject() => new PartyComplianceWrapper(new ScreeningParty(null, "", OrgHeader.New(Factory)));

		protected override bool AllowNewCore => false;

		#endregion
	}
}
