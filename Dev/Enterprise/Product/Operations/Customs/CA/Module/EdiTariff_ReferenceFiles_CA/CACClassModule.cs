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
	/// Module for CACClass.
	/// </summary>
	[SuppressFormsLocalizedTest]
	public class CACClassModule : CACFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.CA.ClassTariff; }
		}

		protected override IFilterControl GetNewFilterControl()
		{
			var result = new ZFilterStripControl(GridCollection, FilterBusinessObject);

			var newStyleInfo = new ZTextBoxColumnStyleInfo { Caption = TariffCaption, ColumnName = CACClassSchema.CT_Tariff.Name };
			ControlDpiScalingHelper.SetWidth(newStyleInfo, 100, true);
			result.FilteredGrid.ColumnStyles.Add(newStyleInfo);

			newStyleInfo = new ZTextBoxColumnStyleInfo { Caption = DescriptionCaption, ColumnName = CACClassSchema.CT_LongDescription.Name };
			ControlDpiScalingHelper.SetWidth(newStyleInfo, 300, true);
			result.FilteredGrid.ColumnStyles.Add(newStyleInfo);

			newStyleInfo = new ZTextBoxColumnStyleInfo { Caption = UnitCaption, ColumnName = CACClassSchema.CT_UQ.Name };
			ControlDpiScalingHelper.SetWidth(newStyleInfo, 80, true);
			result.FilteredGrid.ColumnStyles.Add(newStyleInfo);
			return result;
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CACClassCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CACFilterStripBusinessObject((moduleFilters) =>
			{
				var tariff = moduleFilters.AddTextFilter(TariffCaption, GetTariffQuery);
				tariff.MaxLength = CACClassSchema.CT_Tariff.MaxLength + 3;
				moduleFilters.AddTextFilter(DescriptionCaption, CACClassSchema.CT_LongDescription);
				moduleFilters.AddTextFilter(UnitCaption, CACClassSchema.CT_UQ);
			});
		}

		ZQuery GetTariffQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery result = new ZQuery(CACClassSchema.CT_Tariff, comparisonOperator, value.ExcludeChars(".").ExcludeChars(" ").SubstringSafe(0, CACClassSchema.CT_Tariff.MaxLength));
			return result;
		}

		#region Captions

		public static string TariffCaption
		{
			get { return Res.GetString("1fe158f6-ae00-46a7-a3f4-aaf808142383", "Classification"); }
		}

		static string DescriptionCaption
		{
			get { return Res.GetString("41805fee-1182-4f24-b928-e37476a0dc03", "Description"); }
		}

		static string UnitCaption
		{
			get { return Res.GetString("c1989cdb-260b-49d2-92ef-714d3b86eeb7", "Unit"); }
		}

		#endregion
	}
}
