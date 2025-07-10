using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.DE.Business
{
	public class ProductSupportingInfo : CusSupportingInfo, ITariffFormatProvider
	{
		public ProductSupportingInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public const string CusSupportingInfoType = "PRD";

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const string FormattedTariff = "FormattedTariff";
		}

		protected override ZString HumanReadableNameCore => Res.GetString("bb0554e2-0452-4dde-9d23-83ff7cc726fa", "Product");

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoType;
		}

		#region Properties

		[BusinessObjectTestExclude]
		public override ZString CSI_Tariff
		{
			get => base.CSI_Tariff;
			set => base.CSI_Tariff = TariffFormatter.Format(value).Left(CSI_TariffInfo.MaxLength);
		}

		public ZString FormattedTariff
		{
			get => TariffFormatter.DisplayFormat(CSI_Tariff);
			set => CSI_Tariff = value;
		}

		public ZPropertyInfo FormattedTariffInfo
		{
			get { return GetWrappedZPropertyInfo(AutoCusSupportingInfo.Schema.CSI_Tariff, x => CSI_TariffInfo); }
		}

		TariffFormatter TariffFormatter => EU.Business.TariffFormatter.New(Core.Constants.CountryCodes.Germany);

		ITariffFormatter ITariffFormatProvider.TariffFormatter => TariffFormatter;

		#endregion

		#region Validation

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			return new ProductSupportingInfoValidation(this);
		}

		public new ProductSupportingInfoValidation Validation => (ProductSupportingInfoValidation)base.Validation;

		#endregion
	}
}
