using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.PlugIn;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public abstract class CAConsolPlugIn : ZMutexedPlugIn
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public CAConsolPlugIn(ForwardingConsol consol)
			: base(consol)
		{
			Argument.NotNull(consol, nameof(consol));
			this.consol = consol;
			CustomsManifestVisibilityChanged -= OnChangeTheVisibilityRequired;
			CustomsManifestVisibilityChanged += OnChangeTheVisibilityRequired;
			consol.Transports.CountChanged -= new CollectionCountChangedEventHandler(Transports_CountChanged);
			consol.Transports.CountChanged += new CollectionCountChangedEventHandler(Transports_CountChanged);
			ChangeTheVisibility();
			SetCoveringLabelText();
		}

		public CAConsolPlugIn(ForwardingConsol consol, bool isCalledFromOtherPlugin) : this(consol)
		{
			this.isCalledFromOtherPlugin = isCalledFromOtherPlugin;
		}

		readonly bool isCalledFromOtherPlugin;
		protected readonly ForwardingConsol consol;
		protected BusinessObject bizObj;

		protected void OnChangeTheVisibilityRequired(object sender, EventArgs e)
		{
			ChangeTheVisibility();
		}

		protected void ChangeTheVisibility()
		{
			Enabled = ShouldEnablePlugIn;
		}

		protected virtual bool ShouldEnablePlugIn => !consol.IsDeleted && !Factory.CanadianCarrierCode().IsEmpty && consol.IsGoingViaIgnoringDomesticRoute(Core.Constants.CountryCodes.Canada);

		void Transports_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			OnChangeTheVisibilityRequired(sender, e);
		}
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				CustomsManifestVisibilityChanged -= new EventHandler(OnChangeTheVisibilityRequired);
				consol.Transports.CountChanged -= new CollectionCountChangedEventHandler(Transports_CountChanged);
			}
		}

		protected event EventHandler CustomsManifestVisibilityChanged
		{
			add
			{
				consol.JK_TransportModeInfo.ValueChanged += value;
				consol.JK_RL_NKFirstForeignPortInfo.ValueChanged += value;
				consol.JK_RL_NKLastForeignPortInfo.ValueChanged += value;
				consol.JK_RL_NKPortOfFirstArrivalInfo.ValueChanged += value;
				consol.JK_RL_NKDischargePortInfo.ValueChanged += value;
				if (transportsHasChangedChangedWrapper == null)
				{
					consol.Transports.HasChangesChanged += new EventHandler<HasChangesChangedEventArgs>(Transports_HasChangesChanged);
				}
				transportsHasChangedChangedWrapper += value;
			}
			remove
			{
				consol.JK_TransportModeInfo.ValueChanged -= value;
				consol.JK_RL_NKFirstForeignPortInfo.ValueChanged -= value;
				consol.JK_RL_NKLastForeignPortInfo.ValueChanged -= value;
				consol.JK_RL_NKPortOfFirstArrivalInfo.ValueChanged -= value;
				consol.JK_RL_NKDischargePortInfo.ValueChanged -= value;
				transportsHasChangedChangedWrapper -= value;
				if (transportsHasChangedChangedWrapper == null)
				{
					consol.Transports.HasChangesChanged -= new EventHandler<HasChangesChangedEventArgs>(Transports_HasChangesChanged);
				}
			}
		}

		void Transports_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			if (transportsHasChangedChangedWrapper != null)
			{
				transportsHasChangedChangedWrapper(sender, e);
			}
		}

		event EventHandler transportsHasChangedChangedWrapper;

		internal ZString CoveringLabelText { get; set; }

		void SetCoveringLabelText()
		{
			CoveringLabelText = Res.GetString("3cecd0ab-4c07-4e80-aa4d-60e98fb3062c", "Someone else is already in the process of creating {0} jobs for this consol.\r\nYou cannot process {0} jobs on this consol until they save their data. Please try later.", Name);
		}

		public override ZString PlugInNotDisplayedMessage
		{
			get { return CoveringLabelText; }
		}

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return bizObj != null;
		}

		protected override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			InitialiseBusinessObject();
			return base.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated();
		}

#if DEBUG
		internal ZInt DelayPluginForTestingMutex { get; set; }
#endif

		protected internal virtual void InitialiseBusinessObject()
		{
			ChangeTheVisibility();
			if (Enabled && bizObj == null)
			{
				bizObj = FindBusinessObject();
				if (bizObj == null)
				{
#if DEBUG
					if (DelayPluginForTestingMutex > 0)
					{
						System.Threading.Thread.Sleep(DelayPluginForTestingMutex);
					}
#endif
					var result = isCalledFromOtherPlugin ?
						DialogResult.Yes :
						Globals.Message.Show(Res.GetString("C0941A58-9CB1-494C-BE77-FEEB635B842A", "Do you want to create an {0} Job at this time?", Name), "", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

					if (result == DialogResult.Yes)
					{
						if (Mutex.Lock())
						{
							bizObj = FindBusinessObject();
							if (bizObj == null)
							{
								bizObj = CreateBusinessObject();
							}
						}
					}
					else
					{
						CoveringLabelText = Res.GetString("0A58D2A9-511F-433B-BB0E-E42AC86280AF", "You have chosen not to create an {0} Job now.\r\nPlease change to another tab, then click back to this tab to create an {0} Job for this Consol.", Name);
					}
				}
			}
		}

		protected abstract BusinessObject FindBusinessObject();
		protected abstract BusinessObject CreateBusinessObject();

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return bizObj;
		}

		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			var result = base.ShowPreSaveDialogsCore();
			if (result == ContinueWithSave.Yes && BusinessEntity is CusCAeMHMaster master)
			{
				result = master.ShowAutoSendingWithdrawalMessageDialog(CloseSender);
			}
			return result;
		}

		ISendsMessagesToCustoms CloseSender
		{
			get { return fCloseSender ?? (fCloseSender = new SendsMessagesToCustomsGUI()); }
		}
		ISendsMessagesToCustoms fCloseSender;
	}
}
