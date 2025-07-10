using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public partial class DeclarationDetailsTabUserControl : EU.NCTS.GUI.DeclarationDetailsTabUserControl, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public DeclarationDetailsTabUserControl()
		{
			InitializeComponent();
			ClearanceInfoGroupBox.AllowOutsideOfParent();
		}

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return (control.Name == "AgreedLocationOfGoodsCodeNormalDropEdit" && previousControl.Name == "PreLodgedForAgreedLocationOfGoodsCodeTickBox")
				|| (control.Name == "PreLodgedForAgreedLocationOfGoodsCodeTickBox" && previousControl.Name == "AgreedLocationOfGoodsCodeNormalDropEdit");
		}

		NctsHeader Header => CurrentDataItem as NctsHeader;

		NctsHeader currentHeader;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			CertificateDropEdit.DataBindings.RemoveBinding(IsVisibleForBindingString);

			if (Header != null)
			{
				CertificateDropEdit.DataBindings.Add(new KBinding(IsVisibleForBindingString, Header, nameof(NctsHeader.IsBrokerNeeded), false, DataSourceUpdateMode.Never));
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			UnhookEvents(currentHeader);
			currentHeader = Header;
			HookEvents(currentHeader);
		}

		#region Hook / Unhook Events

		void HookEvents(NctsHeader header)
		{
			if (header != null)
			{
				header.BH_FTZMoveInfo.ValueChanged += BH_FTZMoveInfo_ValueChanged;
			}
		}

		void UnhookEvents(NctsHeader header)
		{
			if (header != null)
			{
				header.BH_FTZMoveInfo.ValueChanged -= BH_FTZMoveInfo_ValueChanged;
			}
		}

		#endregion

		void BH_FTZMoveInfo_ValueChanged(object sender, EventArgs e)
		{
			SetSafetyAndSecurityAsTrueIfContainsSecurityData(Header);
		}

		protected void SetSafetyAndSecurityAsTrueIfContainsSecurityData(NctsHeader header)
		{
			if (header.IsSafetyAndSecurityUncheckedWithExistingSecurityData)
			{
				if (!ShouldAllowUncheckingSafetyAndSecurity(header))
				{
					header.BH_FTZMove = true;
				}
			}
		}

		bool ShouldAllowUncheckingSafetyAndSecurity(NctsHeader header)
		{
			return Globals.Message.Show(
				Res.GetString("D4CDF5B6-C3D8-4CB7-9AA8-2B442F12CA42", "There is security data already filled in. If you uncheck the \"Safety and Security\" box, this data will not be sent to Customs."),
				Res.GetString("A866C9F4-42D1-411F-A826-3D18C4E96E6C", "Do you want to uncheck the box?"),
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning,
				DialogResult.Yes) == DialogResult.Yes;
		}
	}
}
