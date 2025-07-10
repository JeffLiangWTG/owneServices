using System.Windows.Forms;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(CalculateInsuranceForm))]
	sealed class CalculateInsuranceFormBasherTest : ZFormBasherTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Latvia;

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var chargeFactory = (EUIncoTermAndCustomsChargeFactory)declaration.IncoTermAndChargeFactory;
			var bizObj = new CalculateInsuranceBizObj(chargeFactory, invoice);
			bizObj.HasChanges = false;

			return new CalculateInsuranceForm(bizObj);
		}
	}
}
