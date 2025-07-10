using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.Testing
{
	[TestedType(typeof(InBondMoveLineItem))]
	public class InBondMoveLineItemTest : DataObjectTestCase<InBondMoveLineItem>
	{
		protected override List<string> ExpectedAllowLineControlWhiteSpaceAttributePropertiesCore() => new List<string>()
		{
			nameof(InBondMoveLineItem.DescriptionAndQuantityOfMerchandise),
			nameof(InBondMoveLineItem.MarksAndNumbers)
		};

		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>()
			{
				{ nameof(InBondMoveLineItem.MarksAndNumbers), 512 },
				{ nameof(InBondMoveLineItem.DescriptionAndQuantityOfMerchandise), 1000 },
				{ nameof(InBondMoveLineItem.RateComment), 100 },
				{ nameof(InBondMoveLineItem.DutyComment), 100 },
				{ nameof(InBondMoveLineItem.TariffCode), CusInBondCargoDescSchema.BY_HarmonisedTariff.MaxLength },
				{ nameof(InBondMoveLineItem.ReferenceNumber), CusInBondCargoDescSchema.BY_CommercialReferenceNumber.MaxLength },
			};
		}
	}
}
