using System;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public partial class GuaranteeUserControl : ZUserControl
	{
		public GuaranteeUserControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			UnhookCurrentInstruction();
			base.OnCurrentDataItemChanging(e);
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			HookCurrentInstruction();
		}

		void UnhookCurrentInstruction()
		{
			if (Instruction?.ReleaseGuarantees is SecondCusBondDetailCollection releaseGuarantees)
			{
				releaseGuarantees.CountChanged -= ReleaseGuarantees_CountChanged;
			}

			if (Guarantee is CusBondDetail guarantee)
			{
				guarantee.PW_BondTypeInfo.ValueChanged -= PW_BondTypeInfo_ValueChanged;
				guarantee.PW_CPH_GuaranteeInfo.ValueChanged -= PW_CPH_GuaranteeInfo_ValueChanged;
				guarantee.PW_BondNumber2Info.ValueChanged -= PW_BondNumber2Info_ValueChanged;
				guarantee.PW_BondEffectiveDateInfo.ValueChanged -= PW_BondEffectiveDateInfo_ValueChanged;
				guarantee.PW_BondAmountInfo.ValueChanged -= PW_BondAmountInfo_ValueChanged;
				guarantee.PW_StatusInfo.ValueChanged -= PW_StatusInfo_ValueChanged;
				guarantee.OnLinkOrUnlinkAskingSaveJobFirst -= GuaranteeOnLinkOrUnlinkAskingSaveJobFirst;
			}
		}

		void HookCurrentInstruction()
		{
			if (Instruction?.ReleaseGuarantees is SecondCusBondDetailCollection releaseGuarantees)
			{
				releaseGuarantees.CountChanged += ReleaseGuarantees_CountChanged;
			}

			if (Guarantee is CusBondDetail guarantee)
			{
				guarantee.PW_BondTypeInfo.ValueChanged += PW_BondTypeInfo_ValueChanged;
				guarantee.PW_CPH_GuaranteeInfo.ValueChanged += PW_CPH_GuaranteeInfo_ValueChanged;
				guarantee.PW_BondNumber2Info.ValueChanged += PW_BondNumber2Info_ValueChanged;
				guarantee.PW_BondEffectiveDateInfo.ValueChanged += PW_BondEffectiveDateInfo_ValueChanged;
				guarantee.PW_BondAmountInfo.ValueChanged += PW_BondAmountInfo_ValueChanged;
				guarantee.PW_StatusInfo.ValueChanged += PW_StatusInfo_ValueChanged;
				guarantee.OnLinkOrUnlinkAskingSaveJobFirst += GuaranteeOnLinkOrUnlinkAskingSaveJobFirst;
			}

			ControlLinkButtonEnabled();
		}

		void PW_StatusInfo_ValueChanged(object sender, EventArgs e)
		{
			ControlLinkButtonEnabled();
		}

		void PW_BondAmountInfo_ValueChanged(object sender, EventArgs e)
		{
			ControlLinkButtonEnabled();
		}

		void PW_BondEffectiveDateInfo_ValueChanged(object sender, EventArgs e)
		{
			ControlLinkButtonEnabled();
		}

		void PW_BondNumber2Info_ValueChanged(object sender, EventArgs e)
		{
			ControlLinkButtonEnabled();
		}

		void PW_CPH_GuaranteeInfo_ValueChanged(object sender, EventArgs e)
		{
			ControlLinkButtonEnabled();
		}

		void ReleaseGuarantees_CountChanged(object sender, EventArgs e)
		{
			ControlLinkButtonEnabled();
		}

		void PW_BondTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ControlLinkButtonEnabled();
		}

		void ControlLinkButtonEnabled()
		{
			LinkButton.Enabled = Guarantee is CusBondDetail guarantee && guarantee.IsLinkButtonEnabled;
		}

		void LinkButton_Click(object sender, EventArgs e)
		{
			Guarantee?.LinkOrUnlink();
		}

		CusEntryInstruction Instruction => (CusEntryInstruction)CurrentDataItem;

		CusBondDetail Guarantee => Instruction?.GetGuarantee(false);

		ZForm MainForm => (ZForm)FindForm();

		void GuaranteeOnLinkOrUnlinkAskingSaveJobFirst(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = Instruction?.JobDeclaration is JobDeclaration declaration && !SaveDataFirst.Confirm(declaration, MainForm);
		}
	}
}
