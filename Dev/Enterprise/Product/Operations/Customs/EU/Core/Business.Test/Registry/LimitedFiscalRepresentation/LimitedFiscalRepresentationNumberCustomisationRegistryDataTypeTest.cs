using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Registry.Testing
{
	[TestedType(typeof(LimitedFiscalRepresentationNumberCustomisationRegistryDataType))]
	sealed class LimitedFiscalRepresentationNumberCustomisationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<LimitedFiscalRepresentationNumberCustomisationRegistryDataType>
	{
		protected override string ExpectedEditorName => "BillOfLadingNumberCustomisationRegistryItemEditor";

		protected override LimitedFiscalRepresentationNumberCustomisationRegistryDataType GetNewDataType()
		{
			return new LimitedFiscalRepresentationNumberCustomisationRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var customisation1 = new LimitedFiscalRepresentationNumberCustomisation
			{
				RemoveFountainPrefix = false,
				PrefixLength = 3,
				UseShipmentSequenceNumber = false,
				CheckDigitAlgorithm = CheckDigitAlgorithmList.Codes.None
			};

			customisation1.UnFilteredElements.Sort(BillOfLadingNumberCustomisationElement.Schema.Key);
			for (var i = 0; i < customisation1.UnFilteredElements.Count; i++)
			{
				var element = customisation1.UnFilteredElements[i];
				element.Order = (byte)i;

				switch (element.Key)
				{
					case BillOfLadingNumberCustomisationElement.Keys.YearAsDigit:
						element.Detail = "4";
						element.Include = true;
						break;

					case BillOfLadingNumberCustomisationElement.Keys.SequenceNumber:
						element.Detail = "8";
						element.Include = true;
						break;

					case BillOfLadingNumberCustomisationElement.Keys.MonthAs2Digits:
						element.Detail = "1";
						element.Include = true;
						break;

					default:
						element.Detail = "";
						element.Include = false;
						break;
				}
			}

			var customisation2 = new LimitedFiscalRepresentationNumberCustomisation
			{
				RemoveFountainPrefix = true,
				PrefixLength = 3,
				UseShipmentSequenceNumber = true,
				CheckDigitAlgorithm = CheckDigitAlgorithmList.Codes.Standard
			};

			return
			[
				new ValidSampleAndBinaryValueInDB(customisation1, new LimitedFiscalRepresentationNumberCustomisationRegistryDataType().Serialise(customisation1)),
				new ValidSampleAndBinaryValueInDB(customisation2, new LimitedFiscalRepresentationNumberCustomisationRegistryDataType().Serialise(customisation2))
			];
		}
	}
}
