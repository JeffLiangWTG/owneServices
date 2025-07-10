using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CustomsQuantityConverter : BaseCustomsQuantityConverter
	{
		public CustomsQuantityConverter(BaseJobComInvoiceLine invoiceLine, ZPropertyInfo customsQuantityInfo, ZPropertyInfo customsUnitOfQuantityInfo)
			: base(invoiceLine, customsQuantityInfo, customsUnitOfQuantityInfo)
		{
		}

		protected override void CalculateCountrySpecificQuantity()
		{
			base.CalculateCountrySpecificQuantity();

			var customsUnitOfQuantity = (ZString)customsUnitOfQuantityInfo.Value;
			if (!customsUnitOfQuantity.IsEmpty)
			{
				var declaration = InvoiceLine?.Declaration as JobDeclaration;
				var isIID = declaration?.IsIID ?? false;
				var stockUnitOfQuantity = CustomsUnitOfMeasureList.ConvertCustomsUnitsToStockUnits(customsUnitOfQuantity, InvoiceLine.Factory, isIID);
				if (customsUnitOfQuantity != stockUnitOfQuantity)
				{
					CustomsConversionFactor = UnitConverter.ConversionFactor(InvoiceLine.JI_InvoiceUQ, stockUnitOfQuantity);
					CalculateCustomsQty();

					if (isIID)
					{
						using (InvoiceLine.ForceUpdateInvoiceQuantityWithoutDefaulting())
						{
							InvoiceLine.JI_CustomsQuantity = Enterprise.ZArchitecture.Core.Utilities.Round(InvoiceLine.JI_CustomsQuantity, 4);
						}
					}
				}
			}
		}
	}
}
