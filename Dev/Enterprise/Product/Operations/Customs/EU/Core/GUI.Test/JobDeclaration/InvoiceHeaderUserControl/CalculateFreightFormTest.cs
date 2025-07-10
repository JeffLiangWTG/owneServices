using System.Windows.Forms;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(CalculateFreightForm))]
	class CalculateFreightFormBasher : ZFormBasherTest
	{
		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.UnitedKingdom; }
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
