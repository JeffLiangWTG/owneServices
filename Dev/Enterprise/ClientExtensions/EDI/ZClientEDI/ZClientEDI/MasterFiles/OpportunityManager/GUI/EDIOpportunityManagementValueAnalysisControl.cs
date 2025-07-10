using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public partial class EDIOpportunityManagementValueAnalysisControl : OpportunityManagementValueAnalysisControl
	{
		protected EDIOpportunityManagementValueAnalysisControl()
		{
			InitializeComponent();
			SetupCapitalisedValueColumn();
			ValueDetailsGrid.GetColumnStyle("RevenueTypeDescription").CaptionResourceString = ZClientEDI.Res.GetData("EDIOpportunityManagementValueAnalysisControl|ae5f305b-e2d9-4a34-b24b-ae96deb60e17", "Revenue Type");
		}

		#region Construction

		public static new EDIOpportunityManagementValueAnalysisControl New()
		{
			return new EDIOpportunityManagementValueAnalysisControl();
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		#endregion

		#region Columns

		void SetupCapitalisedValueColumn()
		{
			ZCalcEditColumnStyleInfo capitalisedValueColumn = new ZCalcEditColumnStyleInfo();
			capitalisedValueColumn.BindToDecimalPlaces = null;
			capitalisedValueColumn.ColumnName = "CapitalisedValue";
			capitalisedValueColumn.CaptionResourceString = ZClientEDI.Res.GetData("EDIOpportunityManagementValueAnalysisControl|74b0b9ca-b80e-45bd-9b4d-52f1db41d857", "Capitalized Value");
			ControlDpiScalingHelper.SetWidth(ref capitalisedValueColumn, 110, true);
			ValueDetailsGrid.ColumnStyles.Add(capitalisedValueColumn);

#if DEBUG
			TypeDescriptor.AddAttributes(capitalisedValueColumn, new SuppressFormsLocalizedTestAttribute());
#endif

			foreach (ZGridColumnInfo colStyle in ValueDetailsGrid.ColumnStyles)
			{
				colStyle.IsVisible = true;
			}
		}

		#endregion
	}
}
