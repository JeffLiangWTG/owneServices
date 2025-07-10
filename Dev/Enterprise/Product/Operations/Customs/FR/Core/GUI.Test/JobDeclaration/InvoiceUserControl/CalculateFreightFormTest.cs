using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing
{
	[TestedType(typeof(CalculateFreightForm))]
	class CalculateFreightFormTest : ZFormBasherTest
	{
		public void TestControls()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var invoice = declaration.Invoices.AddNew();
			var charge1 = invoice.Charges.AddNew();
			var calculateFreightbizO = new CalculateFreightBizObj(declaration, invoice.Charges, new EU.Business.EUIncoTermAndCustomsChargeFactory(), declaration.LocalCurrencyCode, declaration.CountryCode, declaration.DateOfValuation, declaration.JE_IATALoadPort);
			using (var form = new CalculateFreightForm(calculateFreightbizO))
			{
				var isFreightIncludedInLinesCheckBox = form.Controls.Find("zCheckBoxIsFreightIncludedInLines", true).First();
				AssertNotNull("Form should show an Included in Invoice Lines checkbox", isFreightIncludedInLinesCheckBox);
				var isInsuranceIncludedInLinesCheckBox = form.Controls.Find("zCheckBoxIsInsuranceIncludedInLines", true).First();
				AssertNotNull("Form should show an Included in Invoice Lines checkbox", isInsuranceIncludedInLinesCheckBox);
			}
		}
		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.France; }
		}

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var bizobj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			bizobj.HasChanges = false;

			return new CalculateFreightForm(bizobj);
		}
	}
}
