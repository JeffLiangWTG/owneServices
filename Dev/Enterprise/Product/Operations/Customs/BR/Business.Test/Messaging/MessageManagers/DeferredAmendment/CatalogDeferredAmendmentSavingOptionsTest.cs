using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CatalogDeferredAmendmentSavingOptions))]
	public class CatalogDeferredAmendmentSavingOptionsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIDeferredAmendmentSavingOptions()
		{
			var savingOptions = new CatalogDeferredAmendmentSavingOptions() as IDeferredAmendmentSavingOptions;
			Assert("ShouldTakeReasonForSavingWithoutSendingSeparately", !savingOptions.ShouldTakeReasonForSavingWithoutSendingSeparately);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CatalogDeferredAmendmentSavingOptions();
		}
	}
}
