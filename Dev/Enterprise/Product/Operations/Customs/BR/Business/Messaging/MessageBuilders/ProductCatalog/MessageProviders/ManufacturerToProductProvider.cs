using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.ProductCatalog.Outgoing;

namespace Enterprise.Customs.BR.Business.ProductCatalog
{
	public class ManufacturerToProductProvider : IManufacturerToProduct
	{
		ManufacturerToProductProvider(ForeignOperator foreignOperator)
		{
			this.foreignOperator = Argument.NotNull(foreignOperator, nameof(foreignOperator));
		}
		readonly ForeignOperator foreignOperator;

		public static ManufacturerToProductProvider New(ForeignOperator foreignOperator) => foreignOperator == null ? null : new ManufacturerToProductProvider(foreignOperator);

		public int Sequence => 0;

		public string RootCpfCnpj => foreignOperator.GoodsCatalog?.Owner.GetRootCNPJ() ?? string.Empty;

		public string ForeignOperatorCode => foreignOperator.AuthorityCode.ReturnNullIfEmpty();

		public string ManufacturerCpfCnpj => string.Empty;

		public bool Known => foreignOperator.IsKnow;

		public long ProductCode => long.TryParse(foreignOperator.GoodsCatalog?.CGC_AuthorityIdentifier, out var id) ? id : 0;

		public bool Link => foreignOperator.IsActive;

		public string CountryCode => foreignOperator.CountryCode;
	}
}
