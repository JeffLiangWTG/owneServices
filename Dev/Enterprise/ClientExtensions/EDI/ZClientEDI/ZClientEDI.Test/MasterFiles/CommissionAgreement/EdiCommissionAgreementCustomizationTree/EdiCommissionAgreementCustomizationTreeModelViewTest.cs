using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.GUI.Testing
{
	[TestedType(typeof(EdiCommissionAgreementCustomizationTreeModelView))]
	class EdiCommissionAgreementCustomizationTreeModelViewTest : NonPersistentBusinessObjectTestCase
	{
		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
		{
			var customization = Factory.New<EdiCommissionAgreementCustomization>();
			var model = new EdiCommissionAgreementCustomizationTreeModel(customization);
			return new EdiCommissionAgreementCustomizationTreeModelView(model);
		}
	}
}
