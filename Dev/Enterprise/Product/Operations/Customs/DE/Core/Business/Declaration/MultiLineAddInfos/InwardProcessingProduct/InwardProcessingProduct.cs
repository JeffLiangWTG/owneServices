using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public class InwardProcessingProduct : CusSupportingInfo, ITariffFormatProvider
	{
		public InwardProcessingProduct(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public const string CusSupportingInfoType = "IWP";

		public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

		#region CN Code

		[BusinessObjectTestExclude]
		public override ZString CSI_Tariff
		{
			get => base.CSI_Tariff;
			set => base.CSI_Tariff = TariffFormatter.Format(value).Left(CSI_TariffInfo.MaxLength);
		}

		[ResourceStringData("DDD5342F-1EED-40B8-99F1-CA4C1953399A", Caption = "CN Code")]
		[ReadOnlyMember(nameof(FormattedTariff_ReadOnly))]
		[MaxLength(10)]
		public ZString FormattedTariff
		{
			get => TariffFormatter.DisplayFormat(CSI_Tariff);
			set
			{
				var oldValue = CSI_Tariff;
				CSI_Tariff = value;
				if (oldValue != CSI_Tariff)
				{
					ClearProductDetailsIfNeeded();
				}
			}
		}

		public ZPropertyInfo FormattedTariffInfo => GetWrappedZPropertyInfo(nameof(FormattedTariff), x => CSI_TariffInfo);

		void ClearProductDetailsIfNeeded()
		{
			if (CSI_Tariff.IsEmpty)
			{
				CSI_Description = ZString.Empty;
				CSI_SubType = ZString.Empty;
				CSI_AdditionalDescription = ZString.Empty;
			}
		}

		ZBool FormattedTariff_ReadOnly
		{
			get
			{
				var instruction = Parent?.EntryInstruction;
				return !(instruction != null && instruction.EnabledInwardProcessing && instruction.CEI_SimplifiedGrantAuthorization == SimplifiedGrantAuthorizationList.Codes.J);
			}
		}

		TariffFormatter TariffFormatter => tariffFormatter ?? (tariffFormatter = EU.Business.TariffFormatter.New(Core.Constants.CountryCodes.Germany));
		TariffFormatter tariffFormatter;

		ITariffFormatter ITariffFormatProvider.TariffFormatter => TariffFormatter;

		#endregion

		[ResourceStringData("0AC4B84B-E58C-430A-B31F-E76564C2C669", Caption = "Goods Description")]
		[ReadOnlyMember(nameof(ProductDetails_ReadOnly))]
		public override ZString CSI_Description
		{
			get => base.CSI_Description;
			set => base.CSI_Description = value;
		}

		[ResourceStringData("EB1873AE-C533-4A32-87A1-16218103859E", Caption = "Yield Type")]
		[ReadOnlyMember(nameof(ProductDetails_ReadOnly))]
		[MaxLength(1)]
		public override ZString CSI_SubType
		{
			get => base.CSI_SubType;
			set => base.CSI_SubType = value;
		}

		[ResourceStringData("AFBC1057-B9DC-4F01-A0D8-18FAB27EEAA9", Caption = "Yield Rate / Method of calculation")]
		[ReadOnlyMember(nameof(ProductDetails_ReadOnly))]
		public override ZString CSI_AdditionalDescription
		{
			get => base.CSI_AdditionalDescription;
			set => base.CSI_AdditionalDescription = value;
		}

		ZBool ProductDetails_ReadOnly => FormattedTariff.IsEmpty;

		protected override CusSupportingInfoValidation GetNewValidation() => new InwardProcessingProductValidation(this);

		public new InwardProcessingProductValidation Validation => (InwardProcessingProductValidation)base.Validation;

		protected override CusSupportingInfoLookups GetNewLookups() => new InwardProcessingProductLookups(this);

		public new InwardProcessingProductLookups Lookups => (InwardProcessingProductLookups)base.Lookups;

		protected override ZString HumanReadableNameCore => Res.GetString("D7D705D6-BE27-4956-AF7B-4A96CF243CF0", "Product");

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoType;
		}
	}
}
