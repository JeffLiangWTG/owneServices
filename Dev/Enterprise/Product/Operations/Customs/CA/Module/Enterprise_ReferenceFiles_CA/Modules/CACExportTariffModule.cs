using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	/// <summary>
	/// Module for CACExportTariff.
	/// </summary>
	[SuppressFormsLocalizedTest]
	public class CACExportTariffModule : CACFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.CA.ExportTariff; }
		}

		protected override IFilterControl GetNewFilterControl()
		{
			var result = new ZFilterStripControl(GridCollection, FilterBusinessObject);

			var newStyleInfo = new ZTextBoxColumnStyleInfo { Caption = TariffCaption, ColumnName = CACExportTariffSchema.CE_Code.Name };
			ControlDpiScalingHelper.SetWidth(newStyleInfo, 100, true);
			result.FilteredGrid.ColumnStyles.Add(newStyleInfo);

			newStyleInfo = new ZTextBoxColumnStyleInfo { Caption = DescriptionCaption, ColumnName = CACExportTariffSchema.CE_Description.Name };
			ControlDpiScalingHelper.SetWidth(newStyleInfo, 300, true);
			result.FilteredGrid.ColumnStyles.Add(newStyleInfo);

			newStyleInfo = new ZTextBoxColumnStyleInfo { Caption = UnitCaption, ColumnName = CACExportTariffSchema.CE_Unit.Name };
			ControlDpiScalingHelper.SetWidth(newStyleInfo, 80, true);
			result.FilteredGrid.ColumnStyles.Add(newStyleInfo);
			return result;
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CACExportTariffCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CACFilterStripBusinessObject((moduleFilters) =>
			{
				var tariffFilter = moduleFilters.AddTextFilter(TariffCaption, GetTariffQuery);
				tariffFilter.MaxLength = CACClassSchema.CT_Tariff.MaxLength + 3;
				moduleFilters.AddTextFilter(DescriptionCaption, CACExportTariffSchema.CE_Description);
				moduleFilters.AddTextFilter(UnitCaption, CACExportTariffSchema.CE_Unit);
			});
		}

		ZQuery GetTariffQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(CACExportTariffSchema.CE_Code, comparisonOperator, value.Replace(".", "").Replace(" ", "").SubstringSafe(0, CACExportTariffSchema.CE_Code.MaxLength));
		}

		#region Captions

		public static string TariffCaption
		{
			get { return Res.GetString("a59508fd-7b9f-4ac9-a4d5-fde909f8b371", "Tariff"); }
		}

		static string DescriptionCaption
		{
			get { return Res.GetString("434c4179-9eb2-443a-8a1c-1d9197504c2e", "Description"); }
		}

		static string UnitCaption
		{
			get { return Res.GetString("a3b0cc4d-3838-4b8a-b650-9fcd02c4fe33", "Unit"); }
		}

		#endregion
	}
}
