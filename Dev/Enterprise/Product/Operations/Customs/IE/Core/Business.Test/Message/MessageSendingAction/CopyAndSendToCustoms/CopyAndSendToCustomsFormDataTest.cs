using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(CopyAndSendToCustomsFormData))]
	sealed class CopyAndSendToCustomsFormDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestItineraryCountries()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<EU.Business.ItineraryCountryCollection>(declaration.ItineraryCountries);
		}

		public void TestValidation()
		{
			AssertType<CopyAndSendToCustomsFormDataValidation>(copyAndSendToCustomsFormData.Validation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return copyAndSendToCustomsFormData;
		}

		protected override void SetUp()
		{
			base.SetUp();
			copyAndSendToCustomsFormData = new CopyAndSendToCustomsFormData();
		}

		CopyAndSendToCustomsFormData copyAndSendToCustomsFormData;
	}
}
