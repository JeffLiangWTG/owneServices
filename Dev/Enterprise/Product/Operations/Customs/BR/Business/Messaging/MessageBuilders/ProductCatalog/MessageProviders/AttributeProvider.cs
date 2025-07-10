using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.ProductCatalog.Outgoing;

namespace Enterprise.Customs.BR.Business.ProductCatalog
{
	public class AttributeProvider : IAttribute
	{
		AttributeProvider(AttributeCusCodeData attribute)
		{
			this.attribute = Argument.NotNull(attribute, nameof(attribute));
		}
		readonly AttributeCusCodeData attribute;

		public static AttributeProvider New(AttributeCusCodeData attribute) => attribute == null ? null : new AttributeProvider(attribute);

		public string Attribute => attribute.CY_Code;

		public string Value => attribute.CY_Data;
	}
}
