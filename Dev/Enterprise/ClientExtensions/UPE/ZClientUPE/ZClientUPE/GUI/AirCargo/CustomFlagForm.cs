using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.GUI
{
	public partial class CustomFlagForm : ZArchitecture.GUI.ZChildForm
	{
		#region Static New

		public static CustomFlagForm New(RebillFlags rebillFlag, UPECusHAWB uPECusHAWB)
		{
			CustomFlagForm result;

			switch (rebillFlag)
			{
				case RebillFlags.IsChangedToFreeDomicile:
					result = new CustomFlagForm(new FreeDomicileDetails(uPECusHAWB));
					break;

				case RebillFlags.IsTranshipment:
					result = new RTSTranshipmentForm(new TranshipmentDetails(uPECusHAWB));
					break;

				case RebillFlags.IsAbandoned:
					result = new CustomFlagForm(new AbandonDetails(uPECusHAWB));
					break;

				case RebillFlags.IsRTS:
					result = new RTSTranshipmentForm(new RTSDetails(uPECusHAWB));
					break;

				default:
					result = null;
					break;
			}

			return result;
		}

		#endregion

		[Obsolete("For design-time only", true)]
		public CustomFlagForm()
		{
		}

		protected CustomFlagForm(UPECusHAWBFlagDetails flagDetails)
			: base(flagDetails)
		{
		}

		public override string FormHeading
		{
			get { return "Enter " + FlagDetails.FlagName + " details:"; }
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			HandleClosing();
			e.Cancel = DialogResult == DialogResult.OK && FlagDetails.HasErrors;
		}

		protected virtual void OnOKDialogResult()
		{
		}

		UPECusHAWBFlagDetails FlagDetails
		{
			get { return (UPECusHAWBFlagDetails)BusinessEntity; }
		}

		void HandleClosing()
		{
			FlagDetails.ValidateAll();
			if (!FlagDetails.HasErrors)
			{
				FlagDetails.CreateNote();
				OnOKDialogResult();
			}
		}
	}
}
