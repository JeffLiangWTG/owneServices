using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.MasterFiles;
using Enterprise.Customs.GB.GUI.JobDeclarationForms;
using Enterprise.Customs.GB.GUI.Plugin;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using OrgSupplierPart = Enterprise.Customs.GB.Business.OrgSupplierPart;

namespace Enterprise.Customs.GB.GUI
{
	public class GBOrgSupplierPartFormCustomsControl : EU.GUI.EUOrgSupplierPartFormCustomsControl
	{
		public GBOrgSupplierPartFormCustomsControl()
		{
			RemoveFixedCaptions();
		}

		protected override Type GetSupportingDocumentsUserControlType()
		{
			return typeof(GBSupportingDocumentsUserControl);
		}

		protected override void InitializeForm()
		{
			base.InitializeForm();

			vatDropEdit = new ZDropEdit();
			DetailsTabPage.SuspendLayout();

			foreach (Control control in DetailsTabPage.Controls)
			{
				CargoWise.Windows.UI.ControlDpiScalingHelper.SetLeft(control, control.Left + CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100), false);
			}

			BindingSource.SetBindingMember(vatDropEdit, "PivotsForBinding.CI_ZZF_NKTaxType");
			vatDropEdit.AllowDrop = true;
			vatDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 93, true);
			vatDropEdit.Name = "vatDropEdit";
			vatDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(274, 20, true);
			vatDropEdit.TabIndex = 8;

			DetailsTabPage.Controls.Add(vatDropEdit);

			MoveThirdQtyCalcEdit();
			AddQuantityCalcEditControls();
			ReOrderTabIndexes();

			DetailsTabPage.ResumeLayout(false);
			DetailsTabPage.PerformLayout();

			PivotGrid.GetColumnStyle(CusClassPartPivot.Schema.CI_FormattedTariffNum).Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			PivotGrid.GetColumnStyle(nameof(CusClassPartPivot.CI_CPC)).Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			AddQuantityGridColumns();
		}

		void AddQuantityGridColumns()
		{
			PivotGrid.ColumnStyles.AddRange(columnsToAdd);
			PivotGrid.ReOrderColumns(reorderedColumnsSequence);
		}

		string[] reorderedColumnsSequence => new[]
		{
			CusClassPartPivot.Schema.CI_FormattedTariffNum,
			CusClassPartPivot.Schema.CI_ChildType,
			CusClassPartPivot.Schema.CI_ChildListOrder,
			CusClassPartPivot.Schema.CI_OH,
			nameof(CusClassPartPivot.CI_CPC),
			nameof(CusClassPartPivot.CI_Supplement1),
			nameof(CusClassPartPivot.CI_Supplement2),
			nameof(CusClassPartPivot.CI_SecondQty),
			nameof(CusClassPartPivot.CI_ThirdQty),
			nameof(CusClassPartPivot.CI_FourthQty),
			nameof(CusClassPartPivot.CI_FifthQty),
			CusClassPartPivot.Schema.CI_RN_NKCountryOfOrigin,
			CusClassPartPivot.Schema.CI_CC,
			nameof(CusClassPartPivot.PreferenceCode),
			CusClassPartPivot.Schema.CI_ConcessionOrder,
			CusClassPartPivot.Schema.CI_UsageComment,
			CusClassPartPivot.Schema.CI_Description,
			nameof(CusClassPartPivot.CI_GoodsCategory),
			CusClassPartPivot.Schema.CI_LastAuditedDate,
			nameof(CusClassPartPivot.LastAuditedUserFullName),
			nameof(CusClassPartPivot.Classification) + "+" + nameof(BaseCusClassification.CC_TariffNum),
			nameof(CusClassPartPivot.Classification) + "+" + nameof(BaseCusClassification.CC_Description),
			nameof(CusClassPartPivot.Classification) + "+" + nameof(BaseCusClassification.CC_ClassificationType)
		};

		readonly ZGridColumnInfo[] columnsToAdd = {
			new ZCalcEditColumnStyleInfo
			{
				ColumnName = nameof(CusClassPartPivot.CI_SecondQty),
				CaptionResourceString = Res.GetData("8B963DA0-8FFA-49D7-B4D0-534BCEC6D99A", "2nd Q.", "2nd Qty.", "Second Quantity")
			},
			new ZCalcEditColumnStyleInfo
			{
				ColumnName = nameof(CusClassPartPivot.CI_FourthQty),
				CaptionResourceString = Res.GetData("D2BF6DB3-3D70-4342-A8E9-299625207C30", "4th Q.", "4th Qty.", "Fourth Quantity")
			},
			new ZCalcEditColumnStyleInfo
			{
				ColumnName = nameof(CusClassPartPivot.CI_FifthQty),
				CaptionResourceString = Res.GetData("D215F3BC-FC9A-46F5-81C7-FF640A4E77FF", "5th Q.", "5th Qty.", "Fifth Quantity")
			},
			new ZDropEditColumnStyleInfo
			{
				ColumnName = nameof(CusClassPartPivot.CI_GoodsCategory),
				CaptionResourceString = Res.GetData("E83847F1-682D-4063-AA0A-0F44F66E2B7E", "Category", "Category", "SPIMM Category")
			}
		};

		void AddQuantityCalcEditControls()
		{
			secondQtyCalcEdit = new ZCalcEdit();
			BindingSource.SetBindingMember(secondQtyCalcEdit, "PivotsForBinding.CI_SecondQty");
			secondQtyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(235, 119, true);
			secondQtyCalcEdit.Name = "secondQtyCalcEdit";
			secondQtyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			secondQtyCalcEdit.TabIndex = 9;
			DetailsTabPage.Controls.Add(secondQtyCalcEdit);

			fourthQtyCalcEdit = new ZCalcEdit();
			BindingSource.SetBindingMember(fourthQtyCalcEdit, "PivotsForBinding.CI_FourthQty");
			fourthQtyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(540, 119, true);
			fourthQtyCalcEdit.Name = "fourthQtyCalcEdit";
			fourthQtyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			fourthQtyCalcEdit.TabIndex = 11;
			DetailsTabPage.Controls.Add(fourthQtyCalcEdit);

			fifthQtyCalcEdit = new ZCalcEdit();
			BindingSource.SetBindingMember(fifthQtyCalcEdit, "PivotsForBinding.CI_FifthQty");
			fifthQtyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(680, 119, true);
			fifthQtyCalcEdit.Name = "fifthQtyCalcEdit";
			fifthQtyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			fifthQtyCalcEdit.TabIndex = 12;
			DetailsTabPage.Controls.Add(fifthQtyCalcEdit);
		}

		void MoveThirdQtyCalcEdit()
		{
			var thirdQtyCalcEdit = this.FindSingleOrDefault<ZCalcEdit>("ThirdQtyCalcEdit");
			if (thirdQtyCalcEdit != null)
			{
				ControlDpiScalingHelper.SetLeft(ref thirdQtyCalcEdit, thirdQtyCalcEdit.Left + ControlDpiScalingHelper.ScaleToCurrentDpiX(165), false);
				thirdQtyCalcEdit.TabIndex = 10;
			}
		}

		void ReOrderTabIndexes()
		{
			var countryTextBox = this.FindSingleOrDefault<ZTextBox>("CountryOfOriginTextBox");
			if (countryTextBox != null)
			{
				countryTextBox.TabIndex = 13;
			}
			var preferenceDropEdit = this.FindSingleOrDefault<ZDropEdit>("PreferenceCodeDropEdit");
			if (preferenceDropEdit != null)
			{
				preferenceDropEdit.TabIndex = 14;
			}
			var classificationTextBox = this.FindSingleOrDefault<LongTextControl>("ClassificationDescriptionTextBox");
			if (classificationTextBox != null)
			{
				classificationTextBox.TabIndex = 15;
			}
			var goodsCategoryDropEdit = this.FindSingleOrDefault<ZDropEdit>("GoodsCategoryDropEdit");
			if (goodsCategoryDropEdit != null)
			{
				goodsCategoryDropEdit.TabIndex = 16;
			}
		}

		void RemoveFixedCaptions()
		{
			this.FindSingle<ZCodeFindBox>("CpcTextBox").CaptionResourceString = null;
			this.FindSingle<ZCalcEdit>("ThirdQtyCalcEdit").CaptionResourceString = null;
			this.FindSingle<ZTextBox>("QuotaTextBox").CaptionResourceString = null;
			this.FindSingle<ZTextBox>("CountryOfOriginTextBox").CaptionResourceString = null;
			this.FindSingle<ZDropEdit>("PreferenceCodeDropEdit").CaptionResourceString = null;
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();

			var taxTabPage = this.FindSingleOrDefault<ZTabPage>("taxTabPage");
			if (taxTabPage != null)
			{
				taxTabPage.TabVisible = false;
			}
			UpdateCaptions();
			UpdateGoodsCategoryDropEdit();
		}

		void UpdateCaptions()
		{
			var pivot = PivotGrid.GetCurrent() as CusClassPartPivot;
			if (pivot != null)
			{
				((OrgSupplierPart)DataSource).SelectedPivot = pivot;
			}
			var multipleKeysToUse = (DataSource as OrgSupplierPart)?.MultipleKeysToUse;
			PivotGrid.RefreshColumnCaptions(typeof(CusClassPartPivot), multipleKeysToUse);

			var manualUpdateCaptions = new (string controlName, string captionPropertyName)[]
			{
				("Supplement1TextBox", nameof(OrgSupplierPart.SupplementsCaption)),
				("supportingDocsTabPage", nameof(OrgSupplierPart.SupportingDocumentsCaption)),
				("additionalInfosTabPage", nameof(OrgSupplierPart.AdditionalInfosCaption)),
				("previousDocsTabPage", nameof(OrgSupplierPart.PreviousDocumentsCaption))
			};

			foreach (var (controlName, captionPropertyName) in manualUpdateCaptions)
			{
				if (this.FindSingleOrDefault<Control>(controlName) is IResCaptionedControl control)
				{
					control.CaptionResourceString = DataBoundResourceStrings.GetDataForProperty(typeof(OrgSupplierPart), captionPropertyName, multipleKeysToUse);
				}
			}

			this.RefreshControlCaptions();
		}

		void UpdateGoodsCategoryDropEdit()
		{
			var goodsCategoryDropEdit = this.FindSingleOrDefault<ZDropEdit>("GoodsCategoryDropEdit");
			if (goodsCategoryDropEdit != null)
			{
				goodsCategoryDropEdit.Visible = true;
				var pivot = PivotGrid.GetCurrent() as CusClassPartPivot;
				if (pivot != null && string.IsNullOrEmpty(goodsCategoryDropEdit.DescriptionBox.Text))
				{
					goodsCategoryDropEdit.DescriptionBox.Text = pivot.Lookups.GoodsCategoryList.GetDescriptionFromCode(pivot.CI_GoodsCategory);
				}
			}
		}

		protected override ZCodeFindBox GetTariffFindBox() => CreateUniversalTariffFindBox();

		ZCodeFindBox CreateUniversalTariffFindBox()
		{
			var unTariffFindBox = new Universal.GUI.TariffFindBox();
			unTariffFindBox.TariffType = "IMP";
			unTariffFindBox.GetCountryCode = () => Core.Constants.CountryCodes.UnitedKingdom;
			unTariffFindBox.GetDataGrouping = () => Core.Constants.CountryCodes.UnitedKingdom;
			unTariffFindBox.GetEffectiveDate = () => ZDateTime.Today;
			return unTariffFindBox;
		}

		protected override ZTextBoxColumnStyleInfo GetTariffColumnStyleInfo()
		{
			var gbTariffColumnStyleInfo = new Universal.GUI.TariffColumnStyleInfo();
			gbTariffColumnStyleInfo.GetCountryCode = () => Core.Constants.CountryCodes.UnitedKingdom;
			gbTariffColumnStyleInfo.GetDataGrouping = () => Core.Constants.CountryCodes.UnitedKingdom;
			gbTariffColumnStyleInfo.TariffType = "IMP";
			return gbTariffColumnStyleInfo;
		}

		ZDropEdit vatDropEdit;
		ZCalcEdit secondQtyCalcEdit;
		ZCalcEdit fourthQtyCalcEdit;
		ZCalcEdit fifthQtyCalcEdit;
	}
}
