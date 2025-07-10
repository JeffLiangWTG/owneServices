using System;
using CargoWise.EntityFramework;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceRiskLocationWrapperCollection : NonPersistentBusinessObjectCollection<ComplianceRiskLocationWrapper>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotImplementedException();

		protected override bool AllowNewCore => false;
	}
}
