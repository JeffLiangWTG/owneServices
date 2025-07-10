using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public class EDIOpportunityManagementControl : OpportunityManagementControl
	{
		#region Construction

		public static new EDIOpportunityManagementControl New()
		{
			return new EDIOpportunityManagementControl();
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		#endregion

		public EDIOpportunityManagementControl()
			: base()
		{
			AddGlobalReachColumnToGrid();

			if (!DesignModeFinder.IsDesigning)
			{
				SetEDISpecificColumnCaptions();
			}
		}

		void SetEDISpecificColumnCaptions()
		{
			foreach (var columnStyle in OpportunitiesGrid.ColumnStyles)
			{
				var textBoxColumnStyle = columnStyle as ZTextBoxColumnStyleInfo;
				if (textBoxColumnStyle != null)
				{
					if (textBoxColumnStyle.ColumnName == OrgOpportunitySchema.P8_DiscountAmount.Name)
					{
						textBoxColumnStyle.CaptionResourceString = null;
						textBoxColumnStyle.Caption = OrganisationsDataRegistry.Instance.CurrentLabel.Value;
					}
					else if (textBoxColumnStyle.ColumnName == OrgOpportunitySchema.P8_RentalMultiplier.Name)
					{
						textBoxColumnStyle.CaptionResourceString = null;
						textBoxColumnStyle.Caption = OrganisationsDataRegistry.Instance.PotentialLabel.Value;
					}
				}
			}
		}

		void AddGlobalReachColumnToGrid()
		{
			var zCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			zCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo.CaptionResourceString = Res.GetData("b500ec71-0a4d-4114-860b-cd8de2ed619f", "Global Reach");
			zCalcEditColumnStyleInfo.ColumnName = "OrgOpportunityEx+EOM_GlobalPotential";
			zCalcEditColumnStyleInfo.Decimals = 0;
			this.OpportunitiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo);
		}
	}
}
