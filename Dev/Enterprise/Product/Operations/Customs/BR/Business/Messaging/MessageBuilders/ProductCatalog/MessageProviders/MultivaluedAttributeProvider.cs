using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.ProductCatalog.Outgoing;

namespace Enterprise.Customs.BR.Business.ProductCatalog
{
	public class MultivaluedAttributeProvider : IMultivaluedAttribute
	{
		MultivaluedAttributeProvider(AttributeCusCodeData attribute)
		{
			this.attribute = Argument.NotNull(attribute, nameof(attribute));
		}
		readonly AttributeCusCodeData attribute;

		public static MultivaluedAttributeProvider New(AttributeCusCodeData attribute)
		{
			return attribute == null ? null : new MultivaluedAttributeProvider(attribute);
		}

		public string Attribute => attribute.CY_Code;

		public IEnumerable<string> Values => attribute.Answers.Where(x => !x.IsEmpty).Select(x => x.ToString());
	}
}
