using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EdiCommissionAgreementCompanyAutoAddCountryCollection))]
	class EdiCommissionAgreementCompanyAutoAddCountryCollectionTest : ActiveBusinessObjectCollectionTestCase<EdiCommissionAgreementCompanyAutoAddCountryCollection>
	{
		#region Overrides

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			newElementIndex++;

			var result = (EdiCommissionAgreementCompanyAutoAddCountry)base.GetNewElementToAddToTheCollection();
			result.EPC_RN_NKCountry = newElementIndex.ToString();

			return result;
		}

		int newElementIndex;

		protected override EdiCommissionAgreementCompanyAutoAddCountryCollection GetCollectionToTest()
		{
			var customization = Factory.New<EdiCommissionAgreementCustomization>();
			return new EdiCommissionAgreementCompanyAutoAddCountryCollection(customization);
		}

		#endregion
	}
}
