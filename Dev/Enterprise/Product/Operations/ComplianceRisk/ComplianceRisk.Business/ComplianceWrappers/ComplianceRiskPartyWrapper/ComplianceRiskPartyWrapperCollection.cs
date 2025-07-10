using System;
using CargoWise.EntityFramework;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceRiskPartyWrapperCollection : NonPersistentBusinessObjectCollection<ComplianceRiskPartyWrapper>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotImplementedException();

		protected override bool AllowNewCore => false;
	}
}
