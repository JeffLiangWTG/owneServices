using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public class IE815WineProduct
{
	public IE815WineProduct(IWineProduct wineProduct)
	{
		this.wineProduct = Argument.NotNull(wineProduct, "wineProduct");
	}
	readonly IWineProduct wineProduct;

	[MessageLayout(Order = 0)]
	[MessageFieldIntegerRepresentation(1, true)]
	[MessageFieldRules("C", "D010")]
	public ZInt WineProductCategory => wineProduct.WineProductCategory;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 7, false)]
	[MessageFieldRules("C", "D011")]
	public ZString WineGrowingZoneCode => wineProduct.WineGrowingZoneCode;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, true)]
	[MessageFieldRules("C", "C029")]
	public ZString ThirdCountryOfOrigin => wineProduct.ThirdCountryOfOrigin;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 350, false)]
	[MessageFieldRules("O", "C029")]
	public ZString OtherInformation => wineProduct.OtherInformation;

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldRules("C", "C012")]
	public ZString OtherInformationLanguage => wineProduct.OtherInformationLanguage;

	[MessageLayout(Order = 5)]
	[MessageFieldIntegerRepresentation(3, true)]
	[MessageFieldRules("R")]
	public ZInt TotalWineOperationsIterations => wineProduct.WineOperations.Count();

	[MessageLayout(Order = 6)]
	public IEnumerable<IE815WineOperation> WineOperations
	{
		get
		{
			int i = 1;
			foreach (var wineOperation in wineProduct.WineOperations)
			{
				yield return new IE815WineOperation(wineOperation, i++);
			}
		}
	}
}
