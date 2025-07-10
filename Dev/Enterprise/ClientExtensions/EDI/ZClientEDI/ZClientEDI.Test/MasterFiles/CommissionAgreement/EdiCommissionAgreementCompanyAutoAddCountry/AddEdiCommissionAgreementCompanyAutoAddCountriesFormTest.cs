using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AddEdiCommissionAgreementCompanyAutoAddCountriesForm))]
	class AddEdiCommissionAgreementCompanyAutoAddCountriesFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var customization = Factory.New<EdiCommissionAgreementCustomization>();
			var action = new AddEdiCommissionAgreementCompanyAutoAddCountriesAction(customization, ZGuid.Empty);
			return new AddEdiCommissionAgreementCompanyAutoAddCountriesForm(action);
		}
	}
}
