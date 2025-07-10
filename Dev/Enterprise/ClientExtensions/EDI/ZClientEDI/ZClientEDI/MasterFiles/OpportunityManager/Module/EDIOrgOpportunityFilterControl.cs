using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	public class EDIOrgOpportunityFilterControl : OrgOpportunityFilterControl
	{
		public EDIOrgOpportunityFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			AddGlobalReachColumnToGrid();
			AddLifetimeColumnToGrid();

			if (!DesignModeFinder.IsDesigning)
			{
				SetEDISpecificColumnCaptions();
			}
		}

		void SetEDISpecificColumnCaptions()
		{
			foreach (var columnStyle in grid.ColumnStyles)
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
					else if (textBoxColumnStyle.ColumnName == OrgOpportunitySchema.P8_EstimatedValue.Name)
					{
						textBoxColumnStyle.CaptionResourceString = Res.GetData("858a2fb7-37e8-458d-ac5e-492a6b03becd", "Contract (p.a)");
						textBoxColumnStyle.Caption = "Contract (p.a)";
						textBoxColumnStyle.GroupName = Res.GetData("3fc616c3-91ba-4d3d-b719-aa0cfb119437", "Contract");
					}
					else if (textBoxColumnStyle.ColumnName == OrgOpportunitySchema.P8_RX_NKEstimatedValueCurrency.Name)
					{
						textBoxColumnStyle.CaptionResourceString = Res.GetData("edc012c6-caea-4d78-bb56-13ba5da137ec", "Contract Currency");
						textBoxColumnStyle.Caption = "Contract Currency";
						textBoxColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
						textBoxColumnStyle.GroupName = Res.GetData("3fc616c3-91ba-4d3d-b719-aa0cfb119437", "Contract");
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
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo);
		}

		void AddLifetimeColumnToGrid()
		{
			var zCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			zCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo.CaptionResourceString = Res.GetData("e497a4db-c6fb-41e5-ab66-ee47b8b3394c", "Lifetime (3 CLV)");
			zCalcEditColumnStyleInfo.ColumnName = "OrgOpportunityEx.EOM_LifetimeValueOver3Years";
			zCalcEditColumnStyleInfo.Decimals = 0;
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo);
		}
	}
}
