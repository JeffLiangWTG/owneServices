using System;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using CusEntryInstruction = Enterprise.Customs.IL.Business.CusEntryInstruction;

namespace Enterprise.Customs.IL.GUI
{
	public partial class EntryInstructionDetailsUserControl : BaseCustomsEntryUserControl
	{
		public EntryInstructionDetailsUserControl()
		{
			InitializeComponent();
			InitializeEntryInstructionsGrid();
			PreviousDocumentsTabPage.RunWhenBindingOrFirstShown(InitPreviousDocumentsUserControl);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			BindingSource.SetBindingMember(DetailsUserControl, "CustomsEntryInstructions");
			var layout = InstructionDetailsLayoutProvider;
			DetailsUserControl.UpdateLayout(layout);
		}

		protected IDeclarationFormLayoutProvider DeclarationFormLayoutProvider
		{
			get
			{
				declarationFormLayoutProvider ??= Customs.GUI.DeclarationFormLayoutProvider.GetLayoutProvider(JobDeclaration);
				return declarationFormLayoutProvider;
			}
		}

		IPanelLayoutProvider InstructionDetailsLayoutProvider
		{
			get
			{
				if (declaration != JobDeclaration)
				{
					instructionDetailsLayoutProvider = DeclarationFormLayoutProvider.GetInstructionDetailsLayoutProvider(JobDeclaration);
					declaration = JobDeclaration;
				}
				return instructionDetailsLayoutProvider;
			}
		}

		void InitializeEntryInstructionsGrid()
		{
			var groupFromWarehouse = Res.GetData("91FE115C-AD44-407D-BE3F-D9B588E94B88", "From Warehouse");
			var groupToWarehouse = Res.GetData("50E30723-1ADF-4CFF-80F1-69376A728034", "To Warehouse");
			EntryInstructionsGrid.ColumnStyles.AddRange(new ZGridColumnInfo[]
			{
				new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo()
				{
					GroupName = groupToWarehouse,
					ColumnName = CusEntryInstruction.Schema.ToWarehouseOrgPK,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115),
					IsVisible = false,
				},
				new ZAddressDropEditColumnStyleInfo()
				{
					GroupName = groupToWarehouse,
					ColumnName = CusEntryInstruction.Schema.CEI_OA_Warehouse2,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsVisible = false,
				},

				new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo()
				{
					GroupName = groupFromWarehouse,
					ColumnName = CusEntryInstruction.Schema.FromWarehouseOrgPK,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115),
					IsVisible = false,
				},
				new ZAddressDropEditColumnStyleInfo()
				{
					GroupName = groupFromWarehouse,
					ColumnName = CusEntryInstruction.Schema.CEI_OA_Warehouse,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsVisible = false,
				},
			});
		}

		void InitPreviousDocumentsUserControl(object sender, EventArgs args)
		{
			PreviousDocumentsUserControl.UserControlType = typeof(LayoutPreviousDocumentsUserControl);
			PreviousDocumentsUserControl.HostedControlCreated += (sender, args) =>
			{
				if (PreviousDocumentsUserControl.HostedControl is ISupportingInfoUserControls)
				{
					SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(PreviousDocumentsUserControl.HostedControl, "CustomsEntryInstructions", SupportingInfoColumnLayoutContext);
				}
			};
		}

		protected const string SupportingInfoColumnLayoutContext = "DEC";

		IDeclarationFormLayoutProvider declarationFormLayoutProvider;
		IPanelLayoutProvider instructionDetailsLayoutProvider;
		BaseJobDeclaration declaration;
	}
}
