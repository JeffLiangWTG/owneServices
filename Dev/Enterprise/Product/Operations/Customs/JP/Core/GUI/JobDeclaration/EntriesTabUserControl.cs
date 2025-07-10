using System;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.GUI;
using Enterprise.Customs.JP.Business;

namespace Enterprise.Customs.JP.GUI
{
	public partial class EntriesTabUserControl : ImportMessageUserControl
	{
		public EntriesTabUserControl()
		{
			EntriesBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().SingleOrDefault(s => s.ColumnName == nameof(CusEntryHeader.CH_BGMReference)).CaptionResourceString = null;
			EntriesBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().SingleOrDefault(s => s.ColumnName == nameof(CusEntryHeader.EntryNumber)).CaptionResourceString = null;
			SetupEntryHeaderColumns();
			SetupEntryLineGridColumns();
		}

		protected override Type GetBaseMessagesTabUserControlType() => typeof(MessagesTabUserControl);

		protected override string MessagesUserControlBindingPath => "CustomsEntryHeaders";

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();
			if (JobDeclaration != null)
			{
				EntryLineGrid.SetAvailability(JobDeclaration.IsImport, "RandomLine+" + JobComInvoiceLine.Schema.JI_PrimaryPreference);
				EntryLineGrid.SetAvailability(JobDeclaration.IsExport, "RandomLine+" + JobComInvoiceLine.Schema.JI_FEFTAArticle48);
				EntryLineGrid.SetAvailability(JobDeclaration.IsImport, "RandomLine+" + JobComInvoiceLine.Schema.JI_StorageType);
				EntryLineGrid.SetAvailability(JobDeclaration.IsImport, "RandomLine+" + JobComInvoiceLine.Schema.JI_AdvanceRulingOnClassification);
				EntryLineGrid.SetAvailability(JobDeclaration.IsImport, "RandomLine+" + JobComInvoiceLine.Schema.JI_AdvanceRulingOnOrigin);
				EntryLineGrid.SetAvailability(JobDeclaration.IsExport, "RandomLine+" + JobComInvoiceLine.Schema.JI_DomesticConsumptionTaxExemptionCode);
				EntryLineGrid.SetAvailability(JobDeclaration.IsExport, "RandomLine+" + nameof(JobComInvoiceLine.DomesticConsumptionTaxExemptionType));
				EntryLineGrid.SetAvailability(JobDeclaration.IsImport, nameof(CusEntryLine.DutyReductionAmount));
			}
		}

		void SetupEntryLineGridColumns()
		{
			EntryLineGrid.ColumnStyles.AddRange(new ZGridColumnInfo[]
			{
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = "RandomLine." + JobComInvoiceLine.Schema.JI_FormattedTariff,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsReadOnly = true
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = "RandomLine." + JobComInvoiceLine.Schema.JI_NACCSCode,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsReadOnly = true
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = CusEntryLine.Schema.EffectiveDescription,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsReadOnly = true
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = "RandomLine." + JobComInvoiceLine.Schema.JI_CountryOfOrigin,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsReadOnly = true
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = "RandomLine." + nameof(JobComInvoiceLine.JI_PrimaryPreference),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsReadOnly = true,
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.CustomsQuantity1),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsReadOnly = true
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.CustomsQuantityUnit1),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsReadOnly = true
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.CustomsQuantity2),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsReadOnly = true
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.CustomsQuantityUnit2),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsReadOnly = true
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = CusEntryLine.Schema.BasicPrice,
					GroupName = Res.GetData("F84C1C26-EE42-4DEB-8CCF-A6C2D93E1F0C", "Basic Price"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsReadOnly = true
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = CusEntryLine.Schema.BasicPriceCurrencyCode,
					GroupName = Res.GetData("F84C1C26-EE42-4DEB-8CCF-A6C2D93E1F0C", "Basic Price"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsReadOnly = true
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = CusEntryLine.Schema.CL_CustomsValue,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsReadOnly = true,
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = "RandomLine." + JobComInvoiceLine.Schema.JI_TradeControlOrderAppendix,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsReadOnly = true
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = "RandomLine." + JobComInvoiceLine.Schema.JI_FEFTAArticle48,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsReadOnly = true,
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = "RandomLine." + JobComInvoiceLine.Schema.JI_StorageType,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsReadOnly = true,
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = "RandomLine." + JobComInvoiceLine.Schema.JI_AdvanceRulingOnClassification,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240),
					IsReadOnly = true,
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = "RandomLine." + JobComInvoiceLine.Schema.JI_AdvanceRulingOnOrigin,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160),
					IsReadOnly = true,
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = "RandomLine." + JobComInvoiceLine.Schema.JI_DomesticConsumptionTaxExemptionCode,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240),
					IsReadOnly = true,
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = "RandomLine." + nameof(JobComInvoiceLine.DomesticConsumptionTaxExemptionType),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(320),
					IsReadOnly = true,
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.DutyReductionAmount),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsReadOnly = true,
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.CL_ConfirmedCustomsValue),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					IsReadOnly = true,
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.CL_ParentLineNumber),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsReadOnly = true,
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.CL_MergedCustomsValue),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					IsReadOnly = true,
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.CL_PriceCheck),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					GroupName = Res.GetData("94689568-0AF3-49BD-991B-1164C7742D22", "Price Check"),
					IsReadOnly = true,
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.CL_PriceCheckDescription),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200),
					GroupName = Res.GetData("94689568-0AF3-49BD-991B-1164C7742D22", "Price Check"),
					IsReadOnly = true,
				},
			});
		}

		void SetupEntryHeaderColumns()
		{
			EntriesBoundGrid.ColumnStyles.AddRange(new ZGridColumnInfo[]
			{
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.CH_Status,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					GroupName = Enterprise.Customs.JP.GUI.Res.GetData("444e6e07-992c-43ea-9cca-431bddc12345", "Message Status")
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.CH_StatusDescription),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180),
					GroupName = Enterprise.Customs.JP.GUI.Res.GetData("444e6e07-992c-43ea-9cca-431bddc12345", "Message Status")
				},

				new ZArchitecture.GUI.ZDropEditColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.CH_PhaseStatus,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					GroupName = Enterprise.Customs.JP.GUI.Res.GetData("5BEE99D4-B45A-463A-9E79-6260158A84FF", "Phase"),
					IsReadOnly = true
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.PhaseDescription),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180),
					GroupName = Enterprise.Customs.JP.GUI.Res.GetData("5BEE99D4-B45A-463A-9E79-6260158A84FF", "Phase"),
					IsReadOnly = true
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.CH_InspectionStatus),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					GroupName = Enterprise.Customs.JP.GUI.Res.GetData("84CC727A-1BA6-4F70-837A-1E34C9F03A68", "Inspection Status"),
					IsReadOnly = true
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.InspectionStatusDescription),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					GroupName = Enterprise.Customs.JP.GUI.Res.GetData("84CC727A-1BA6-4F70-837A-1E34C9F03A68", "Inspection Status"),
					IsReadOnly = true
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.CH_CargoType),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					GroupName = Enterprise.Customs.JP.GUI.Res.GetData("DD1CECA3-617C-4B5F-A687-08B2B91370CF", "Cargo Type"),
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.CargoTypeDescription),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					GroupName = Enterprise.Customs.JP.GUI.Res.GetData("DD1CECA3-617C-4B5F-A687-08B2B91370CF", "Cargo Type"),
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.CH_InspectionType),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110),
					GroupName = Enterprise.Customs.JP.GUI.Res.GetData("60716775-258C-46A3-98A4-2118A206C2C5", "Inspection Type"),
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.InspectionTypeDescription),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					GroupName = Enterprise.Customs.JP.GUI.Res.GetData("60716775-258C-46A3-98A4-2118A206C2C5", "Inspection Type"),
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.CH_InspectionSubType),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130),
					GroupName = Enterprise.Customs.JP.GUI.Res.GetData("F4B4CEB0-A0B9-46B8-A0AF-3817584BE158", "Inspection Sub Type"),
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.InspectionSubTypeDescription),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					GroupName = Enterprise.Customs.JP.GUI.Res.GetData("F4B4CEB0-A0B9-46B8-A0AF-3817584BE158", "Inspection Sub Type"),
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.CH_DocumentRequestType),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145),
					GroupName = Enterprise.Customs.JP.GUI.Res.GetData("250BC386-112B-4F0D-8276-FC1783E171A4", "Document Request Type"),
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.DocumentRequestTypeDescription),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					GroupName = Enterprise.Customs.JP.GUI.Res.GetData("250BC386-112B-4F0D-8276-FC1783E171A4", "Document Request Type"),
				},

				new ZArchitecture.GUI.ZDropEditColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.CH_EntryStatus,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					GroupName = Enterprise.Customs.JP.GUI.Res.GetData("75900DE4-701B-4F5F-A6DA-3B31D2C52BD9", "Entry Status")
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.CH_EntryStatusDescription),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					GroupName = Enterprise.Customs.JP.GUI.Res.GetData("75900DE4-701B-4F5F-A6DA-3B31D2C52BD9", "Entry Status"),
					IsReadOnly = true
				}
			});
		}
	}
}
