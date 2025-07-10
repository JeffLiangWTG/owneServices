using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.ExitControl.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.ExitControl.GUI
{
	public class ConsignmentsGridUserControlMenuProvider : EU.ExitControl.GUI.ConsignmentsGridUserControlMenuProvider
	{
		public ConsignmentsGridUserControlMenuProvider(IConsignmentsGridUserControlProvider provider) : base(provider)
		{
		}

		protected override ZMenuItem[] GetAdditionalConsignmentsGridMenuItems()
		{
			var result = base.GetAdditionalConsignmentsGridMenuItems();
			var parent = Provider?.ExitHeader?.Parent;
			if (parent is JobDeclaration || parent is ForwardingShipment)
			{
				result = result.Append(new ZMenuItem(ResString.GetMultilingualString("EFF7BA4C-D40C-4DC4-A855-24BA6CBCB205", "&Import Goods Items/Entry Lines"), ImportGoodsItemsMenuItem_Click)).ToArray();
			}
			return result;
		}

		void ImportGoodsItemsMenuItem_Click(object sender, EventArgs e)
		{
			var parent = Provider?.ExitHeader?.Parent;
			JobDeclaration declaration = null;
			if (parent is JobDeclaration parentDeclaration)
			{
				declaration = parentDeclaration;
			}
			else if (parent is ForwardingShipment shipment)
			{
				declaration = shipment.DeclarationForDocuments as JobDeclaration;
			}

			if (declaration != null)
			{
				if (Provider.UserControl is IConsignmentsGridUserControl userControl && userControl.ConsignmentsGrid is ZGrid consignmentsGrid && consignmentsGrid.SelectedRowCount == 1)
				{
					if (userControl.CurrentDataItem is CusExitConsignment consignment && !consignment.CXC_MovementReference.IsEmpty)
					{
						consignment.ImportGoodsItemData(declaration);
						Globals.Message.ShowInformation(Res.GetString("76685F53-F39E-4AD0-B615-211F9DFCAE0C", "Import completed."), Res.GetString("76685F53-F39E-4AD0-B615-211F9DFCAE0D", "Succeeded"));
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("76685F53-F39E-4AD0-B615-211F9DFCAE0E", "Empty MRN is invalid for this operation."), Res.GetString("76685F53-F39E-4AD0-B615-211F9DFCAE0F", "Invalid MRN"));
					}
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("76685F53-F39E-4AD0-B615-211F9DFCAE00", "Please select a single consignment."), Res.GetString("76685F53-F39E-4AD0-B615-211F9DFCAE01", "Single Select"));
				}
			}
		}
	}
}
