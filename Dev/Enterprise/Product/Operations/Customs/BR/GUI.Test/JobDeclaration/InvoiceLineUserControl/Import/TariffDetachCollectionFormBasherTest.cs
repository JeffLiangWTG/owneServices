using System.Windows.Forms;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(TariffDetachCollectionForm))]
	public class TariffDetachCollectionFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var collection = new TariffDetachCollection(Factory.New<JobComInvoiceLine>());
			return new TariffDetachCollectionForm(collection);
		}
	}
}
