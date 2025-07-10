
namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusClassPartPivotValidation : Customs.Business.BaseCusClassPartPivotValidation
	{
		public CusClassPartPivotValidation(CusClassPartPivot parent)
			: base(parent)
		{
		}

		protected override void CheckCI_TariffNum()
		{
			base.CheckCI_TariffNum();

			var parent = (CusClassPartPivot)Parent;
			var value = parent.CI_TariffNum.Trim();

			if (!value.IsEmpty)
			{
				var exists = false;
				var isPartial = false;

				if (parent.IsExport)
				{
					var tariff = AUCAHECCWrapper.Load(parent.Factory, value, parent.DateOfValuation);
					exists = tariff != null;
					isPartial = tariff?.HasChildren ?? (value.Length < AUExportTariffUniversalFormatter.MaxTarifflength);
				}
				else
				{
					var tariff = AUCClassWrapper.LoadPartialCode(Parent.Factory, value, parent.DateOfValuation);
					exists = tariff != null;
					isPartial = tariff?.HasChildren ?? (value.Length < AUImportTariffUniversalFormatter.MaxTarifflength);
				}

				if (isPartial)
				{
					parent.CI_TariffNumInfo.AddMessageError(TariffIsPartial);
				}
				else if (!exists)
				{
					parent.CI_TariffNumInfo.AddMessageError(TariffDoesNotExist);
				}
			}
		}

		public static string TariffDoesNotExist => Res.GetString("59FF4202-E19C-46D4-B163-091922EC5B8F", "The tariff code cannot be found in the customs tariff code list.");
		public static string TariffIsPartial => Res.GetString("F6564526-6FB4-4172-8F1E-DF29D8C6052C", "The selected tariff is a partial tariff used only for navigation - Please select a complete tariff number.");
	}
}
