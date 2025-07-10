using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.ProductCatalog.Outgoing;

namespace Enterprise.Customs.BR.Business.ProductCatalog
{
	public class CompositeAttributeProvider : ICompositeAttribute
	{
		CompositeAttributeProvider(AttributeCusCodeData attribute, AttributeCusCodeData[] childAttributes)
		{
			this.attribute = Argument.NotNull(attribute, nameof(attribute));
			this.childAttributes = Argument.NotNull(childAttributes, nameof(childAttributes));
		}
		readonly AttributeCusCodeData attribute;
		readonly AttributeCusCodeData[] childAttributes;

		public static CompositeAttributeProvider New(AttributeCusCodeData attribute)
		{
			var childAttributes = attribute?.ChildAttributes?.Where(a => !a.CY_Data.IsEmpty && !a.IsEffectiveInFuture).ToArray();
			return childAttributes == null || childAttributes.Length == 0 ? null : new CompositeAttributeProvider(attribute, childAttributes);
		}

		public string Attribute => attribute.CY_Code;

		public IEnumerable<IAttribute> Values => childAttributes.Select(AttributeProvider.New);
	}
}
