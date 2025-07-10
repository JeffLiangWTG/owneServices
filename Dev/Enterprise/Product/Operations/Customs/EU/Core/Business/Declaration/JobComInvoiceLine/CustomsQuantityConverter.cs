using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using static Enterprise.Customs.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CustomsQuantityConverter : BaseCustomsQuantityConverter
	{
		public CustomsQuantityConverter(BaseJobComInvoiceLine invoiceLine, ZPropertyInfo customsQuantityInfo, ZPropertyInfo customsUnitOfQuantityInfo)
			: base(invoiceLine, customsQuantityInfo, customsUnitOfQuantityInfo)
		{
		}

		public string GetEffectiveWeightUnit(string unit) => CustomsUnitToWeightUnitDict.TryGetValue(unit, out string mappedWeightCode) ? mappedWeightCode : unit;

		protected virtual ImmutableDictionary<string, string> CustomsUnitToWeightUnitDict => InvoiceLine.Factory.GetCachedValue("EU.Business.Declaration.CustomsQuantityConverter.CustomsUnitToWeightUnitDict", () =>
		{
			return ImmutableDictionary.CreateRange(new Dictionary<string, string>
			{
				{ RefCusCodeList.CustomsUq.Weight.Kilogram, Core.Constants.Weight.Kilograms },
				{ RefCusCodeList.CustomsUq.Weight.Tonne, Core.Constants.Weight.Tonnes },
				{ RefCusCodeList.CustomsUq.Weight.Gram, Core.Constants.Weight.Grams },
				{ RefCusCodeList.CustomsUq.Weight.Hectokilogram, Core.Constants.Weight.Decitons }
			});
		});

		public override ZDecimal CalculateFromNetWeightToCustomsQtyCore()
		{
			var invLine = InvoiceLine;
			return Core.Constants.Weight.Convert(invLine.JI_NetWeight, invLine.JI_NetWeightUQ, GetEffectiveWeightUnit(customsUnitOfQuantityInfo.Value.ToString()));
		}
	}
}
