using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public static class ExciseProductHelper
	{
		public static ZZRefCusCodeListCombined GetExciseProduct(this EMCSJobComInvoiceLine line)
		{
			ZZRefCusCodeListCombined result = null;
			if (line != null)
			{
				var exciseProductCode = line.ZG_ExciseProductCode;
				if (!exciseProductCode.IsEmpty)
				{
					result = line.Factory.GetExciseProductCodes(line.GetDefaultDataGroupingCode()).FirstOrDefault(c => c.ZZD_Code == exciseProductCode);
				}
			}
			return result;
		}

		public static ZBool HasExciseProductAttribute(this EMCSJobComInvoiceLine line, ExciseProductCodeAttribute attribute, ZString? value = null)
		{
			return line.GetExciseProduct()?.HasAttribute(attribute.ToString(), value) ?? ZBool.False;
		}

		public static IEnumerable<ZString> GetExciseProductAttributeValues(this EMCSJobComInvoiceLine line, ExciseProductCodeAttribute attribute)
		{
			return line.GetExciseProduct()?.GetAttributesValues(attribute.ToString()) ?? Enumerable.Empty<ZString>();
		}
	}
}
