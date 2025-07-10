using Enterprise.Customs.JP.AFR.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.GUI.Testing
{
	[TestedType(typeof(ImportFromSailingForm))]
	class ImportFromSailingFormTest : ZFormBasherTest
	{
		protected override System.Windows.Forms.Form GetFormToBashCore()
		{
			return new ImportFromSailingForm(new BillImportActionCollection(Factory.New<JPAFRHeader>().Bills));
		}
	}
}
