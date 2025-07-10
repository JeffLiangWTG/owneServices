using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	public abstract class EdiCommissionAgreementTreeBizObjWrapperTestCase<T> : NonPersistentBusinessObjectTestCase where T : EdiCommissionAgreementTreeBizObjWrapper
	{
		#region Implementation
		protected abstract T GetNewWrapper(EdiCommissionAgreementCustomization customization, IEnumerable<EdiCommissionAgreementTreeBizObjWrapper> children);
		protected abstract EdiCommissionAgreementTreeBizObjWrapper GetNewChildWrapper(EdiCommissionAgreementCustomization customization);
		protected override BusinessObject GetNewBusinessObject()
		{
			var customization = Factory.New<EdiCommissionAgreementCustomization>();
			var childWrapper = GetNewChildWrapper(customization);
			var children = childWrapper != null ? new[] { childWrapper } : null;
			return GetNewWrapper(customization, children);
		}
		#endregion
	}
}
