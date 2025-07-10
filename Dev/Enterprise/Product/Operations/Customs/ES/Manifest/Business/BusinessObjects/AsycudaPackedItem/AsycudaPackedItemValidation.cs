using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Manifest.Business
{
	public class AsycudaPackedItemValidation : ASYCUDA.Business.AsycudaPackedItemValidation
	{
		public AsycudaPackedItemValidation(AsycudaPackedItem parent) : base(parent)
		{
		}

		protected new AsycudaPackedItem Parent => (AsycudaPackedItem)base.Parent;

		protected override void TariffListValidationCore()
		{
			if (!Parent.API_Tariff.IsEmpty)
			{
				var length = Parent.API_Tariff.Length;

				if (length != 4 && length != 6 && length != 8)
				{
					Parent.API_TariffInfo.AddMessageError(Res.GetString("93EBC8F9-F1CF-4A4D-8EF0-8DB8AB43D0AB", "Only 4, 6 or 8 digits are allowed"));
				}
				else
				{
					var tariffList = Parent.Lookups.TariffList;
					var filter = tariffList.CompleteFilter;
					filter.AddToFilter(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.StartsWith, Parent.API_Tariff);
					var result = Parent.Factory.LoadTop1<TariffView>(filter);

					if (result == null)
					{
						Parent.API_TariffInfo.AddMessageError(Res.GetString("355BB2BF-FB11-4C8E-80A0-440038CE28E7", "The code you have selected is not in the list."));
					}
				}
			}
		}
	}
}
