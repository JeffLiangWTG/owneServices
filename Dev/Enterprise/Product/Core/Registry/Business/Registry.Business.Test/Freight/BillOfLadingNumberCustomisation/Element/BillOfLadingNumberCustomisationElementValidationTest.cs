using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business.BillCustomisationStrategies;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.Testing
{
	sealed class BillOfLadingNumberCustomisationElementValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateOrder()
		{
			Element1.Include = false;
			Element1.Order = 0;
			Element2.Include = false;
			Element2.Order = 0;

			AssertNoNotifications(Element2.OrderInfo);

			Element1.Include = true;
			Element1.Order = 1;
			Element2.Include = true;
			Element2.Order = 1;

			AssertHasError(Element2.OrderInfo, "The Order has been duplicated and must be unique.");

			Element2.Order = 2;
			AssertNoNotifications(Element2.OrderInfo);
		}

		public void TestValidateFountain()
		{
			Customisation.UseShipmentSequenceNumber = true;
			Element1.Fountain = false;
			AssertNoErrors(Element1.FountainInfo);

			Element1.Fountain = true;
			AssertHasError(Element1.FountainInfo, "This option is not valid when 'Use Shipment Sequence Number' is enabled");

			Customisation.UseShipmentSequenceNumber = false;
			Element1.Validation.ValidateFountain();
			AssertNoErrors(Element1.FountainInfo);
		}

		public void TestValidateCheckDigit()
		{
			Customisation.CheckDigitAlgorithm = CheckDigitAlgorithmList.Codes.None;
			Element1.Include = true;
			Element1.CheckDigit = false;
			Element2.Include = true;
			Element2.CheckDigit = false;
			AssertNoErrors(Element1.CheckDigitInfo);
			AssertNoErrors(Element2.CheckDigitInfo);

			Customisation.CheckDigitAlgorithm = CheckDigitAlgorithmList.Codes.MAWB;
			Element1.Validation.ValidateCheckDigit();
			Element2.Validation.ValidateCheckDigit();
			AssertHasError(Element1.CheckDigitInfo, string.Format("The Check Digit Algorithm is {0}. At least one element needs to be included in the check digit algorithm.", Customisation.CheckDigitAlgorithm));
			AssertHasError(Element2.CheckDigitInfo, string.Format("The Check Digit Algorithm is {0}. At least one element needs to be included in the check digit algorithm.", Customisation.CheckDigitAlgorithm));

			Element2.CheckDigit = true;
			Element1.Validation.ValidateCheckDigit();
			AssertNoErrors(Element1.CheckDigitInfo);
			AssertNoErrors(Element2.CheckDigitInfo);
		}

		#region Implementation

		#region Customisation

		BillOfLadingNumberCustomisation Customisation
		{
			get
			{
				if (customisation == null)
				{
					customisation = new BillOfLadingNumberCustomisation();
					customisation.UnFilteredElements.RemoveAll();
					customisation.UnFilteredElements.Add(new BillOfLadingNumberCustomisationElement(customisation, Strategy1));
					customisation.UnFilteredElements.Add(new BillOfLadingNumberCustomisationElement(customisation, Strategy2));
				}
				return customisation;
			}
		}
		BillOfLadingNumberCustomisation customisation;

		#endregion

		#region Collection

		BillOfLadingNumberCustomisationElementCollection Collection
		{
			get { return Customisation.UnFilteredElements; }
		}

		#endregion

		#region Strategy1

		IElementStrategy Strategy1
		{
			get { return strategy1 ?? (strategy1 = new CommonElementStrategy("key1", "name1", string.Empty, NumberCustomisationElementCategories.Standard, 1, null)); }
		}
		IElementStrategy strategy1;

		#endregion

		#region Strategy2

		IElementStrategy Strategy2
		{
			get { return strategy2 ?? (strategy2 = new CommonElementStrategy("key2", "name2", string.Empty, NumberCustomisationElementCategories.Standard, 2, null)); }
		}
		IElementStrategy strategy2;

		#endregion

		#region Element1

		BillOfLadingNumberCustomisationElement Element1
		{
			get { return element1 ?? (element1 = Collection[Strategy1.Key]); }
		}
		BillOfLadingNumberCustomisationElement element1;

		#endregion

		#region Element2

		BillOfLadingNumberCustomisationElement Element2
		{
			get { return element2 ?? (element2 = Collection[Strategy2.Key]); }
		}
		BillOfLadingNumberCustomisationElement element2;

		#endregion

		#endregion
	}
}
