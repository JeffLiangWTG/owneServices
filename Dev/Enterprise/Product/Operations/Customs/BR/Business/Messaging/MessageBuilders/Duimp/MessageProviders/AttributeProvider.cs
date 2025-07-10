using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Duimp.Outgoing;

namespace Enterprise.Customs.BR.Business.Duimp
{
	public class AttributeProvider : IAttributeItem
	{
		AttributeProvider(AttributeCusCodeData attribute)
		{
			this.attribute = Argument.NotNull(attribute, nameof(attribute));
		}
		readonly AttributeCusCodeData attribute;

		public static AttributeProvider New(AttributeCusCodeData attribute) => attribute == null ? null : new AttributeProvider(attribute);

		public string Code => attribute.CY_Code;

		public string Value => attribute.CY_Data;
	}
}
