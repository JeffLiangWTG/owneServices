using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ZZDBValidationHelper
	{
		public ZZDBValidationHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		internal IEnumerable<ZString> GetProductTypeValues(ZString fieldValue, ZString headerType)
		{
			return GetAttributesCore(fieldValue, headerType, Constants.InvoiceLine.Keys.ProductType);
		}

		internal IEnumerable<ZString> GetAHECCValues(ZString fieldValue)
		{
			return GetAttributesCore(fieldValue, "A", Constants.InvoiceLine.Keys.AHECCCode);
		}

		IEnumerable<ZString> GetAttributesCore(ZString fieldValue, ZString headerType, ZString attributeName)
		{
			var typeCode = FormattableString.Invariant($"NPRC{headerType}");

			var code = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, fieldValue, Core.Constants.CountryCodes.Australia, typeCode, ZDateTime.Today, null, new[] { attributeName });
			return code?.GetAttributesValues(attributeName) ?? Enumerable.Empty<ZString>();
		}
	}
}
