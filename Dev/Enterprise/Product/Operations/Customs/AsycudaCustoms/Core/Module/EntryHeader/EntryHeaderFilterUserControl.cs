using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.Module
{
	public class EntryHeaderFilterUserControl : Customs.Module.EntryHeaderFilterUserControl
	{
		public EntryHeaderFilterUserControl(IBusinessObjectCollection gridCollection, EntryHeaderFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
		}

		public static class ColumnNames
		{
			public const string IsContinuousGuarantee = "EntryInstruction+Guarantee+IsContinuous";
			public const string LinkedGuarantee = "EntryInstruction+LinkedGuaranteeNumber";
			public const string GuaranteeActivity = "EntryInstruction+Guarantee+PW_ActivityCode";
			public const string GuaranteeStatus = "EntryInstruction+Guarantee+PW_Status";
			public const string GuaranteeAmount = "EntryInstruction+Guarantee+PW_BondAmount";
			public const string TransitPermitCount = "TransitPermitCount";
			public const string TransitPermitInTransitCount = "TransitPermitInTransitCount";
			public const string TransitExpired = "Expired"; // Binding Column Names
			public const string TransitEarliestExpiryDate = "EarliestExpiryDate";
			public const string TransitCompleted = "Completed";// Binding Column Names
			public const string RemainingCustomsValue = "EntryInstruction+RemainingCustomsValue";
			public const string RemainingWeight = "EntryInstruction+RemainingNetWeightKilograms";
			public const string RemainingCustomsQuantity = "EntryInstruction+RemainingCustomsQuantity";
		}

		protected override void InitialiseGridCore()
		{
			base.InitialiseGridCore();

			grid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
			{
				new ZArchitecture.ZCheckBoxColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.AsycudaCustoms.Module.Res.GetData("396AB45A-317F-44CD-9E06-7FB8E239F803", "Continuous Guarantee"),
					ColumnName = ColumnNames.IsContinuousGuarantee,
					IsReadOnly = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.AsycudaCustoms.Module.Res.GetData("ED08CA27-6EB6-4BDA-B627-2B9F9E0562FC", "Linked Guarantee"),
					ColumnName = ColumnNames.LinkedGuarantee,
					IsReadOnly = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.AsycudaCustoms.Module.Res.GetData("6739AC56-F771-4102-AD9D-ADAD83A1D967", "Guarantee Activity"),
					ColumnName = ColumnNames.GuaranteeActivity,
					IsReadOnly = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.AsycudaCustoms.Module.Res.GetData("C607DC49-1007-44E9-9871-286DFC8F4C41", "Guarantee Status"),
					ColumnName = ColumnNames.GuaranteeStatus,
					IsReadOnly = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
				new ZArchitecture.ZCalcEditColumnStyleInfo
				{
					BindToDecimalPlaces = null,
					CaptionResourceString = Enterprise.Customs.AsycudaCustoms.Module.Res.GetData("1E11866C-29B0-4AA4-949C-DC7987576352", "Guarantee Amount"),
					ColumnName = ColumnNames.GuaranteeAmount,
					Decimals = 2,
					IsReadOnly = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
				new ZArchitecture.ZDateEditColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.CH_BondAcquittedDate,
					DateTimeFormat = ZDateTimePickerFormat.Short,
					IsReadOnly = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZArchitecture.ZDateEditColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.CH_BondValidToDate,
					DateTimeFormat = ZDateTimePickerFormat.Short,
					IsReadOnly = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZArchitecture.ZCalcEditColumnStyleInfo
				{
					BindToDecimalPlaces = null,
					CaptionResourceString = Enterprise.Customs.AsycudaCustoms.Module.Res.GetData("a0d7f51a-3bc1-4bbf-81d1-be2187d201b8", "No. of Transit Permits"),
					ColumnName = ColumnNames.TransitPermitCount,
					Decimals = 0,
					IsReadOnly = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				},
				new ZArchitecture.ZCalcEditColumnStyleInfo
				{
					BindToDecimalPlaces = null,
					CaptionResourceString = Enterprise.Customs.AsycudaCustoms.Module.Res.GetData("dccb0a6c-82b4-40e1-bc5f-db6b2ded7649", "No. still in Transit"),
					ColumnName = ColumnNames.TransitPermitInTransitCount,
					Decimals = 0,
					IsReadOnly = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				},
				new ZArchitecture.ZCheckBoxColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.AsycudaCustoms.Module.Res.GetData("7c160c14-e14c-42f7-a3a4-ee426393829f", "Transit Expired"),
					ColumnName = ColumnNames.TransitExpired,
					IsReadOnly = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
				},
				new ZArchitecture.ZDateEditColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.AsycudaCustoms.Module.Res.GetData("c43a54f2-c372-4391-83d7-acb53ee6af0b", "Transit Earliest Expiry Date"),
					ColumnName = ColumnNames.TransitEarliestExpiryDate,
					DateTimeFormat = ZDateTimePickerFormat.Short,
					IsReadOnly = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},
				new ZArchitecture.ZCheckBoxColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.AsycudaCustoms.Module.Res.GetData("d4046c13-eb68-43bf-a850-f2eb864e07f6", "Transit Completed"),
					ColumnName = ColumnNames.TransitCompleted,
					IsReadOnly = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
		});

			if (AsycudaCustoms.Business.Extensions.IsRiskManagementEnabled(GlbCompany.CurrentCompany.Country.Code, FilterBusinessObject.Factory))
			{
				grid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
				{
					new ZArchitecture.ZCalcEditColumnStyleInfo
					{
						BindToDecimalPlaces = null,
						CaptionResourceString =
							Enterprise.Customs.AsycudaCustoms.Module.Res.GetData("4C4F5915-1199-4F80-9B3A-D3B6F8F07EAD",
								"Remaining Customs Value"),
						ColumnName = ColumnNames.RemainingCustomsValue,
						Decimals = 2,
						IsReadOnly = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140),
					},
					new ZArchitecture.ZCalcEditColumnStyleInfo
					{
						BindToDecimalPlaces = null,
						CaptionResourceString =
							Enterprise.Customs.AsycudaCustoms.Module.Res.GetData("AFD28EA2-79DE-4E45-A074-DE6BB96BB377",
								"Remaining Weight"),
						ColumnName = ColumnNames.RemainingWeight,
						Decimals = 2,
						IsReadOnly = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					},
					new ZArchitecture.ZCalcEditColumnStyleInfo
					{
						BindToDecimalPlaces = null,
						CaptionResourceString =
							Enterprise.Customs.AsycudaCustoms.Module.Res.GetData("5AE88DD7-13C0-40FE-B065-8E7A8303B823",
								"Remaining Customs Quantity"),
						ColumnName = ColumnNames.RemainingCustomsQuantity,
						Decimals = 2,
						IsReadOnly = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					},
				});
			}
		}

		protected override List<string> ColumnNamesInSortOrder
		{
			get
			{
				if (columnNamesInSortOrder == null)
				{
					List<string> columns = new List<string>();

					columns.Add(CusEntryHeader.Schema.EntryNumber);
					columns.Add(CusEntryHeader.Schema.DeclarationReference);
					columns.Add(CusEntryHeaderSchema.Constants.CH_BGMReference);
					columns.Add(CusEntryHeaderSchema.Constants.CH_EntryStatus);
					columns.Add(CusEntryHeader.Schema.EntryHeaderStatusDescription);
					columns.Add(CusEntryHeader.Schema.CH_EntrySubmittedDate);
					columns.Add(CusEntryHeader.Schema.CH_EntryReleaseDate);
					columns.Add(CusEntryHeaderSchema.Constants.CH_Status);
					columns.Add(CusEntryHeader.Schema.MessageStatusDescription);
					columns.Add(CusEntryHeaderSchema.Constants.CH_MessageType);
					columns.Add(CusEntryHeader.Schema.CH_MessageTypeDescription);
					columns.Add(Schema.BranchName);
					columns.Add(Schema.ImporterName);
					columns.Add(Schema.SupplierName);
					columns.Add(Schema.AgentsReference);
					columns.Add(Schema.DateOfArrival);
					columns.Add(CusEntryHeader.Schema.CH_TotalPaid);
					columns.Add(CusEntryHeader.Schema.CH_BondValidToDate);
					columns.Add(CusEntryHeader.Schema.CH_BondAcquittedDate);
					columns.Add(CusEntryHeader.Schema.CH_WarehouseTransactionStatus);
					columns.Add(CusEntryHeader.Schema.CH_WarehouseTransactionStatusDescription);
					columns.Add(ColumnNames.IsContinuousGuarantee);
					columns.Add(ColumnNames.LinkedGuarantee);
					columns.Add(ColumnNames.GuaranteeActivity);
					columns.Add(ColumnNames.GuaranteeStatus);
					columns.Add(ColumnNames.GuaranteeAmount);

					columnNamesInSortOrder = columns;
				}
				return columnNamesInSortOrder;
			}
		}
		List<string> columnNamesInSortOrder;
	}
}
