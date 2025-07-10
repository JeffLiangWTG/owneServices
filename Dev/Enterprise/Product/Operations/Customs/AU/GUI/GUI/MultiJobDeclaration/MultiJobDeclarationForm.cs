using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public class MultiJobDeclarationForm : Customs.GUI.MultiJobDeclarationForm
	{
		public MultiJobDeclarationForm(MultiJobDeclarationHeader topLevelBizO)
			: base(topLevelBizO)
		{
		}

		BaseJobDeclaration Declaration => ((MultiJobDeclarationHeader)BusinessEntity).Declaration;

		protected override void AddConsignorConsignee(ZGrid grid)
		{
			base.AddConsignorConsignee(grid);

			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo("Declaration+ZA_GoodsOwnerPartyIDHidden", 80);
			zTextBoxColumnStyleInfo4.Caption = "Goods Owner ID";
			grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);

			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo("Declaration+ZA_ConsigneeNameHidden", 80);
			zTextBoxColumnStyleInfo2.Caption = "Consignee Name";
			grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);

			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo("Declaration+ZA_ConsigneeCityHidden", 80);
			zTextBoxColumnStyleInfo3.Caption = "Consignee City";
			grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
		}

		protected override Core.Forms.ZGridColumnInfo GetTariffColumnStyleInfo()
		{
			Universal.GUI.TariffColumnStyleInfo CreateAustraliaColumn(string tariffType)
			{
				return new Universal.GUI.TariffColumnStyleInfo
				{
					GetCountryCode = () => Core.Constants.CountryCodes.Australia,
					GetDataGrouping = () => Core.Constants.CountryCodes.Australia,
					GetTariffType = () => tariffType,
					GetEffectiveDate = () => Declaration.DateOfValuation
				};
			}

			Core.Forms.ZGridColumnInfo tariffColumnInfo;
			bool isExport = Declaration?.IsExport == true && AUCAHECCWrapper.EnableCWRefForAHECC;
			bool isImport = Declaration?.IsImport == true && AUCClassWrapper.UseCustomsReferenceData;

			if (isExport)
			{
				tariffColumnInfo = CreateAustraliaColumn(Universal.Constants.TariffTypes.Export);
			}
			else if (isImport)
			{
				tariffColumnInfo = CreateAustraliaColumn(Universal.Constants.TariffTypes.Import);
			}
			else
			{
				tariffColumnInfo = new AHECCTariffColumnStyleInfo(() => Declaration.IsImport);
			}

			tariffColumnInfo.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUOrgSupplierPartFormCustomsControl|662ed62c-169f-4c52-9864-4e01a17b5c19", "Tariff", "Tariff Code");
			tariffColumnInfo.ColumnName = Business.CusClassPartPivot.Schema.CI_TariffNum;
			tariffColumnInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			return tariffColumnInfo;
		}
	}
}
