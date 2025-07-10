using CargoWise.EntityFramework;
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
	/// Module for CACFIAEndUseCodes.
	/// </summary>
	[SuppressFormsLocalizedTest]
	public class CACFIAEndUseCodesModule : CACFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.CA.CFIAEndUseCodes; }
		}

		protected override IFilterControl GetNewFilterControl()
		{
			var result = new ZFilterStripControl(GridCollection, FilterBusinessObject);

			var newStyleInfo = new ZTextBoxColumnStyleInfo { Caption = CodeCaption, ColumnName = CACFIAEndUseCodesSchema.FE_Code.Name };
			ControlDpiScalingHelper.SetWidth(newStyleInfo, 100, true);
			result.FilteredGrid.ColumnStyles.Add(newStyleInfo);

			newStyleInfo = new ZTextBoxColumnStyleInfo { Caption = DescCaption, ColumnName = CACFIAEndUseCodesSchema.FE_Desc.Name };
			ControlDpiScalingHelper.SetWidth(newStyleInfo, 400, true);
			result.FilteredGrid.ColumnStyles.Add(newStyleInfo);

			newStyleInfo = new ZTextBoxColumnStyleInfo { Caption = FrenchDescCaption, ColumnName = CACFIAEndUseCodesSchema.FE_DescFrench.Name };
			ControlDpiScalingHelper.SetWidth(newStyleInfo, 400, true);
			result.FilteredGrid.ColumnStyles.Add(newStyleInfo);
			return result;
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CACFIAEndUseCodesCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CACFilterStripBusinessObject((moduleFilters) =>
			{
				moduleFilters.AddTextFilter(CodeCaption, CACFIAEndUseCodesSchema.FE_Code);
				moduleFilters.AddTextFilter(DescCaption, CACFIAEndUseCodesSchema.FE_Desc);
				moduleFilters.AddTextFilter(FrenchDescCaption, CACFIAEndUseCodesSchema.FE_DescFrench);
			});
		}

		#region Captions

		static string CodeCaption
		{
			get { return Res.GetString("6708843E-CDAC-4d3a-B264-E8959DA0B5D2", "End Use Code"); }
		}

		static string DescCaption
		{
			get { return Res.GetString("EF673820-A630-45ef-838E-56BA1062E207", "Description"); }
		}

		static string FrenchDescCaption
		{
			get { return "Français"; }
		}

		#endregion
	}
}
