using System;
using System.Collections;
using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Orders.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Client.Wow
{
	public partial class WoolworthsOrdersUserControl : OrdersUserControl
	{
		public WoolworthsOrdersUserControl()
		{
			InitializeComponent();

			TypeDescriptor.AddAttributes(JD_RSLabel, new SuppressFormsLocalizedTestAttribute());

			this.JD_JSBoundFindBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("WoolworthsOrdersUserControl|eb5f7258-bc8d-46f1-8502-35a08c748e82", "Shipment");
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (CurrentDataItem != null)
			{
				foreach (CustomLabelControlRenamer renamer in fRenamers)
				{
					renamer.Dispose();
				}
				fRenamers.Clear();
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				JD_RSBoundFindBox.Visible = false;
				ControlDpiScalingHelper.SetTop(ref JD_PaymentTypeBoundDropEdit, JD_RSBoundFindBox.Top, false);
				JD_PaymentTypeBoundDropEdit.TabIndex = JD_RSBoundFindBox.TabIndex;
				fRenamers.Add(new CustomLabelControlRenamer(
					JD_RSLabel, JD_PaymentTypeBoundDropEdit, new Order.CustomLabelsProvider(Order, false), WoolworthsOrder.JD_PaymentType_CustomPropertyName));
			}
		}

		#region Implementation

		protected ArrayList fRenamers = new ArrayList();

		protected override string[] GetPropertiesToExcludeFromAdditionalDetailControl()
		{
			ArrayList result = new ArrayList(base.GetPropertiesToExcludeFromAdditionalDetailControl());
			result.Add(WoolworthsOrder.JD_PaymentType_CustomPropertyName);
			return (string[])result.ToArray(typeof(string));
		}

		#endregion
	}
}
