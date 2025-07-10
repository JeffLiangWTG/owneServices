using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Universal.Module;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	/// <summary>
	/// Module for CACSubLocation.
	/// </summary>
	[SuppressFormsLocalizedTest]
	public class CACSubLocationModule : ZZRefCusCodeListWrapperModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.CA.SubLocation; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override IFilterControl GetNewFilterControl()
		{
			var result = base.GetNewFilterControl();

			var newStyleInfo = new ZTextBoxColumnStyleInfo { Caption = CityCaption, ColumnName = CACSubLocation.Schema.SL_City };
			ControlDpiScalingHelper.SetWidth(newStyleInfo, 100, true);
			result.FilteredGrid.ColumnStyles.Add(newStyleInfo);

			newStyleInfo = new ZTextBoxColumnStyleInfo { Caption = PortCaption, ColumnName = CACSubLocation.Schema.SL_Port };
			ControlDpiScalingHelper.SetWidth(newStyleInfo, 40, true);
			result.FilteredGrid.ColumnStyles.Add(newStyleInfo);

			newStyleInfo = new ZTextBoxColumnStyleInfo { Caption = TypeCaption, ColumnName = CACSubLocation.Schema.SL_Type };
			ControlDpiScalingHelper.SetWidth(newStyleInfo, 40, true);
			result.FilteredGrid.ColumnStyles.Add(newStyleInfo);

			newStyleInfo = new ZTextBoxColumnStyleInfo { Caption = AddressCaption, ColumnName = CACSubLocation.Schema.SL_Address };
			ControlDpiScalingHelper.SetWidth(newStyleInfo, 300, true);
			result.FilteredGrid.ColumnStyles.Add(newStyleInfo);

			newStyleInfo = new ZTextBoxColumnStyleInfo { Caption = ProvinceCaption, ColumnName = CACSubLocation.Schema.SL_Province };
			ControlDpiScalingHelper.SetWidth(newStyleInfo, 60, true);
			result.FilteredGrid.ColumnStyles.Add(newStyleInfo);

			newStyleInfo = new ZTextBoxColumnStyleInfo { Caption = PostCodeCaption, ColumnName = CACSubLocation.Schema.SL_PostCode };
			ControlDpiScalingHelper.SetWidth(newStyleInfo, 60, true);
			result.FilteredGrid.ColumnStyles.Add(newStyleInfo);
			return result;
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CACSubLocationCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CACSubLocationFilterStripBusinessObject();
		}

		#region Captions

		protected override string CodeCaption => Res.GetString("68ae38e9-a866-42af-a928-633c71ddab5d", "Sub-Location Code");
		protected override string DescriptionCaption => Res.GetString("16673f56-09a3-4639-9f9c-d9bf78ec0e7c", "Warehouse Name");

		static string CityCaption => Res.GetString("72BD3275-164D-4297-BEE3-DB756263386A", "City");
		static string PortCaption => Res.GetString("05061094-B481-4DEA-B855-A3704AF839BA", "Port");
		static string TypeCaption => Res.GetString("7352643B-26B7-43AB-B90A-459C15B210BC", "Type");
		static string AddressCaption => Res.GetString("BF0E0AF0-1E4E-4A38-B056-9D9CC7663B97", "Address");
		static string ProvinceCaption => Res.GetString("64B79149-8517-4674-B7D7-9955A7CE89B0", "Province");
		static string PostCodeCaption => Res.GetString("DF209B2A-6141-4115-8249-18DE686DF580", "Post Code");

		#endregion
	}
}
