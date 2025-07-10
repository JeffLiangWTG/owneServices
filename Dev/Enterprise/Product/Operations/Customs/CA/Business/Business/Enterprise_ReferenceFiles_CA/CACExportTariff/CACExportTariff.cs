using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	[DescriptionProperty(CACExportTariffSchema.Constants.CE_Description)]
	public class CACExportTariff : AutoCACExportTariff, ITariffData, ITariff
	{
		public CACExportTariff(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCACExportTariff.Schema
		{
			public const string CE_FormattedCode = "CE_FormattedCode";
		}

		public override bool SupportsNotes
		{
			get { return false; }
		}

		public ZString CE_FormattedCode
		{
			get { return TariffFormatter.DisplayFormat(CE_Code); }
		}

		public ZPropertyInfo CE_FormattedCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CE_FormattedCode, x => CE_CodeInfo); }
		}

		protected TariffFormatter TariffFormatter
		{
			get { return new TariffFormatter(); }
		}

		#region ITariffData members
		ZString ITariffData.TariffCode
		{
			get { return CE_Code; }
		}

		ZString ITariffData.TariffDescription
		{
			get { return CE_Description; }
		}

		ZString ITariffData.TariffUnits
		{
			get { return CE_Unit; }
		}

		ZBool ITariffData.ConveyanceIDRequired
		{
			get { return CE_IsConveyanceIDRequired; }
		}
		#endregion

		#region ITariff
		ZString ITariff.Code => CE_Code;

		ZString ITariff.Description => CE_Description;

		ZString ITariff.UQ1 => CE_Unit;

		ZString ITariff.UQ2 => ZString.Empty;

		ZString ITariff.UQ3 => ZString.Empty;

		ZString ITariff.UQ4 => ZString.Empty;

		ZString ITariff.UQ5 => ZString.Empty;
		#endregion

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public CACExportTariff LoadFromCode(ZString tariffCode)
			{
				return Factory.GetCachedValue("CACExportTariff_" + tariffCode, () => Factory.LoadFromNaturalKey<CACExportTariff>(CACExportTariffSchema.CE_Code, tariffCode));
			}

			protected override System.Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CACExportTariff);
			}
		}
	}
}
