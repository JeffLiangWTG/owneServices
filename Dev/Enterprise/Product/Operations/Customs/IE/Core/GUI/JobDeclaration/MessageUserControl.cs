using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public partial class MessageUserControl : EU.GUI.MessageUserControl
	{
		public MessageUserControl() : this(null)
		{
		}

		public MessageUserControl(JobDeclaration declaration) : base(declaration)
		{
			InitializeComponent();

			UpdateEntriesBoundGridColumns();
			SplitEntryLinesTabPage();
			originalEntryLineGridColumnStyles = EntryLineGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
			EntriesBoundGrid.AfterBind += EntriesBoundGrid_AfterBind;
		}

		public new JobDeclaration JobDeclaration => (JobDeclaration)EUDeclaration;

		protected override EntryLineAdditionalDataUserControl GetEntryLineAdditionalData() => EUDeclaration is JobDeclaration declaration && declaration.IsUCC5
			? new ImportEntryLineAdditionalDataUserControl()
			: base.GetEntryLineAdditionalData();

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();

			if (EntriesBoundGrid != null && EntryLineGrid != null)
			{
				if (JobDeclaration.IsExport)
				{
					EntriesBoundGrid.SetColumnCaption(CusEntryHeader.Schema.CH_BGMReference, Res.GetData("087C0854-3B3A-4EE8-898F-93BEC5836A52", "LRN").Caption);
					EntriesBoundGrid.SetAvailability(false, CusEntryHeader.Schema.VAT);
					EntriesBoundGrid.SetAvailability(false, CusEntryHeader.Schema.Duty);
					var issueDateColumnStyleName = $"{nameof(CusEntryNumber)}+{CusEntryNumber.Schema.CE_IssueDate}";
					EntriesBoundGrid.SetAvailability(false, issueDateColumnStyleName);
					EntryLineGrid.SetAvailability(false, CusEntryLine.Schema.DutyAmount);
					EntryLineGrid.SetAvailability(false, CusEntryLine.Schema.CL_DutyPercent);
					EntryLineGrid.SetAvailability(false, CusEntryLine.Schema.GSTVATAmount);
					EntryLineGrid.SetAvailability(false, CusEntryLine.Schema.GSTVATDeferred);
					EntryLineGrid.SetAvailability(false, CusEntryLine.Schema.CL_CustomsValue);
					EntryLineGrid.SetAvailability(false, CusEntryHeader.Schema.CRN);
				}

				EntriesBoundGrid.SetColumnVisible(true, CusEntryHeader.Schema.MessageStatusDescription);

				EntriesBoundGrid.SetAvailability(false, CusEntryHeader.Schema.EntryNumber);
				EntriesBoundGrid.SetAvailability(false, CusEntryHeader.Schema.DeclarationUCR);
				if (JobDeclaration.IsImport)
				{
					EntriesBoundGrid.SetAvailability(true, CusEntryHeader.Schema.CRN);
					EntriesBoundGrid.SetColumnVisible(true, CusEntryHeader.Schema.CRN);
				}
			}
		}

		void EntriesBoundGrid_AfterBind(object sender, EventArgs e)
		{
			if (EntriesBoundGrid.ListManager is CurrencyManager listManager)
			{
				listManager.CurrentChanged += EntriesBoundGrid_ListManager_CurrentChanged;
			}
		}

		void EntriesBoundGrid_ListManager_CurrentChanged(object sender, EventArgs e)
		{
			SetEntryLineGridAndTaxOrFeeTabPageVisibilities();
		}

		#region EntryLineGrid & TaxOrFeeTabPage Visibilities

		readonly ZGridColumnInfo[] originalEntryLineGridColumnStyles;

		void SetEntryLineGridAndTaxOrFeeTabPageVisibilities()
		{
			ChangeTaxOrFeeTabPageVisibility();
			if (TryGetSelectedInstruction(out var instruction))
			{
				ChangeEntryLineGridColumnsVisibility(instruction);
			}
		}

		bool TryGetSelectedInstruction(out CusEntryInstruction instruction)
		{
			instruction =
				EntriesBoundGrid?.ListManager is CurrencyManager listManager
				&& listManager.GetCurrent() is CusEntryHeader entryHeader
				&& entryHeader.EntryInstruction is CusEntryInstruction cusEntryInstruction
					? cusEntryInstruction
					: null;
			return instruction != null;
		}

		void ChangeEntryLineGridColumnsVisibility(CusEntryInstruction instruction)
		{
			var statisticalColumnsOnly = instruction.IsH2;

			using (EntryLineGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				foreach (var column in originalEntryLineGridColumnStyles)
				{
					EntryLineGrid.Columns.Remove(column.ColumnName);
				}

				var newColumns = originalEntryLineGridColumnStyles
					.Where(info => !statisticalColumnsOnly || EntryLineStatisticalColumns.Contains(info.ColumnName))
					.Select(Clone)
					.ToArray();

				foreach (var newColumn in newColumns)
				{
					if (!EntryLineGrid.Columns.Contains(newColumn.ColumnName))
					{
						EntryLineGrid.Columns.Add(newColumn);
					}
				}
			}

			static ZGridColumnInfo Clone(ZGridColumnInfo original)
			{
				var originalType = original.GetType();
				var result = Activator.CreateInstance(originalType);
				foreach (var property in originalType.GetProperties())
				{
					if (property.CanWrite)
					{
						property.SetValue(result, property.GetValue(original));
					}
				}
				return (ZGridColumnInfo)result;
			}
		}

		static HashSet<string> EntryLineStatisticalColumns => entryLineStatisticalColumnsLazy.Value;
		static Lazy<HashSet<string>> entryLineStatisticalColumnsLazy { get; } = new Lazy<HashSet<string>>(() => new HashSet<string>
		{
			CusEntryLine.Schema.CL_LineNumber,
			CusEntryLine.Schema.FormattedTariff,
			CusEntryLine.Schema.EffectiveDescription,
			CusEntryLine.Schema.CL_StatisticalValue,
			CusEntryLine.Schema.DutyAmount
		});

		void ChangeTaxOrFeeTabPageVisibility()
		{
			var shouldHideTaxOrFeeTabPage =
				JobDeclaration is JobDeclaration jobDeclaration && jobDeclaration.IsExport
				|| TryGetSelectedInstruction(out var instruction) && instruction.IsH2;
			EntryLineAdditionalDataUserControl.TaxOrFeeTabPage.TabVisible = !shouldHideTaxOrFeeTabPage;
		}

		#endregion

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			SetEntryLineGridAndTaxOrFeeTabPageVisibilities();
		}

		void SplitEntryLinesTabPage()
		{
			if (JobDeclaration != null && JobDeclaration.IsUCC5)
			{
				EntryLinesTabPage.Controls.Clear();

				var entryLinesSplitContainer = new KSplitContainer
				{
					Dock = DockStyle.Fill,
					Orientation = Orientation.Horizontal,
					SplitterDistance = 200,
				};
				EntryLineGrid.Dock = DockStyle.Fill;
				entryLinesSplitContainer.Panel1.Controls.Add(EntryLineGrid);

				var entryLineCalculatedDutyAndTaxUserControl = EntryLineAdditionalDataUserControl.FindSingleOrDefault<Control>("EntryLineCalculatedDutyAndTaxUserControl");
				var entryLineConfirmedDutyAndTaxGroupBox = EntryLineAdditionalDataUserControl.FindSingleOrDefault<Control>("EntryLineConfirmedDutyAndTaxGroupBox");

				if (entryLineCalculatedDutyAndTaxUserControl != null && entryLineConfirmedDutyAndTaxGroupBox != null)
				{
					entryLineCalculatedDutyAndTaxUserControl.AllowOverlap(entryLineConfirmedDutyAndTaxGroupBox);
				}
				EntryLineAdditionalDataUserControl.Dock = DockStyle.Fill;
				entryLinesSplitContainer.Panel2.Controls.Add(EntryLineAdditionalDataUserControl);

				EntryLinesTabPage.Controls.Add(entryLinesSplitContainer);
			}
		}

		void UpdateEntriesBoundGridColumns()
		{
			var mrnColumn = EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.MovementReferenceNumber);
			if (mrnColumn != null)
			{
				mrnColumn.GroupName = MovementReferenceNumberGroupName;
			}
			EntriesBoundGrid.ColumnStyles.AddRange(new ZGridColumnInfo[]
			{
				new ZDateEditColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("DD667947-5DEA-417E-84BB-B32B4A2D3A9C", "Accepted  Date"),
					GroupName = MovementReferenceNumberGroupName,
					ColumnName = CusEntryHeader.Schema.MovementReferenceNumberIssueDate,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short
				},
			});

			var releaseDateColumn = EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_EntryReleaseDate) as ZDateEditColumnStyleInfo;
			if (releaseDateColumn != null)
			{
				releaseDateColumn.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			}

			EntriesBoundGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo()
			{
				CaptionResourceString = Res.GetData("124C8D00-CE3E-4FC9-A933-41E7E1BCDDE2", "CRN", "Customs Registration Number", "CRN - Customs Registration Number"),
				ColumnName = nameof(CusEntryHeader.Schema.CRN),
				IsVisible = false,
				IsReadOnly = true,
			});
		}

		protected override Type GetBaseMessagesTabUserControlType() => typeof(MessagesTabUserControl);

		ResourceStringData MovementReferenceNumberGroupName => Res.GetData("98FB1013-5F48-44C7-A3D5-C2DDDA4B6E0C", englishCaption: "MRN", englishFullDescription: "Movement Reference Number");
	}
}
