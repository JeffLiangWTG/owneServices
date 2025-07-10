using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	[TestedType(typeof(ImportFromSailingForm))]
	sealed class ImportFromSailingFormTest : ZFormBasherTest
	{
		protected override System.Windows.Forms.Form GetFormToBashCore()
		{
			var bills = Factory.New<AsycudaManifestHeader>().Bills;
			var collection = new BillImportActionCollection(bills);

			return new ImportFromSailingForm(collection);
		}
	}
}
