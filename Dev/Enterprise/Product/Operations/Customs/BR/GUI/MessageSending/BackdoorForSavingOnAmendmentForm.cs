
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	public partial class BackdoorForSavingOnAmendmentForm : Customs.GUI.BackdoorForSavingOnAmendmentForm
	{
		public BackdoorForSavingOnAmendmentForm(Business.DeclarationDeferredAmendmentSavingOptions savingOptions)
			: base(savingOptions)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override string ExplanationForSaveWithEntryChanges
		{
			get { return Res.GetString("BAD174B4-76DF-42EB-8E1A-09B75E783C04",
				@"If you have made changes that affect Customs Entries and does not want to send the amendment  message now, please choose this option.
The system will take a rectification reason and lets you save without send a rectification message.
But you will be able to see the reasons entered on the Workflow> DAP event prior send the rectification message.
You can send a Rectification message later by Clicking Brokerage> Send to Customs.
Message Status will show ""Not Sent"" until you send Rectification Message."); }
		}
	}
}
