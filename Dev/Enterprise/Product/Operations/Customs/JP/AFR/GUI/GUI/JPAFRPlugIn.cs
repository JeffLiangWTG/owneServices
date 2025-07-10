using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.JP.AFR.GUI
{
	public class JPAFRPlugIn : ZMutexedPlugIn, IInBondDetailInitiator
	{
		public JPAFRPlugIn(ForwardingConsol consol)
			: base(consol)
		{
			if (HostBusinessEntity != null)
			{
				CustomsManifestVisibilityChanged -= OnChangeTheVisibilityRequired;
				CustomsManifestVisibilityChanged += OnChangeTheVisibilityRequired;
				HostBusinessEntity.Transports.CountChanged -= Transports_CountChanged;
				HostBusinessEntity.Transports.CountChanged += Transports_CountChanged;
				ChangeTheVisibility();
			}
		}

		public JPAFRHeader Header
		{
			get { return InternalHeader; }
		}

		public override string Name
		{
			get { return "AFR"; }
		}

		#region Implementation

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					if (HostBusinessEntity != null)
					{
						CustomsManifestVisibilityChanged -= OnChangeTheVisibilityRequired;
						HostBusinessEntity.Transports.CountChanged -= Transports_CountChanged;
						if (onDisposing != null)
						{
							onDisposing(this, EventArgs.Empty);
						}
					}
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		event EventHandler CustomsManifestVisibilityChanged
		{
			add
			{
				HostBusinessEntity.JK_TransportModeInfo.ValueChanged += value;
				HostBusinessEntity.JK_RL_NKFirstForeignPortInfo.ValueChanged += value;
				HostBusinessEntity.JK_RL_NKLastForeignPortInfo.ValueChanged += value;
				HostBusinessEntity.JK_RL_NKPortOfFirstArrivalInfo.ValueChanged += value;
				HostBusinessEntity.JK_RL_NKDischargePortInfo.ValueChanged += value;
				if (transportsHasChangedChangedWrapper == null)
				{
					HostBusinessEntity.Transports.HasChangesChanged += Transports_HasChangesChanged;
				}
				transportsHasChangedChangedWrapper += value;
			}
			remove
			{
				HostBusinessEntity.JK_TransportModeInfo.ValueChanged -= value;
				HostBusinessEntity.JK_RL_NKFirstForeignPortInfo.ValueChanged -= value;
				HostBusinessEntity.JK_RL_NKLastForeignPortInfo.ValueChanged -= value;
				HostBusinessEntity.JK_RL_NKPortOfFirstArrivalInfo.ValueChanged -= value;
				HostBusinessEntity.JK_RL_NKDischargePortInfo.ValueChanged -= value;
				transportsHasChangedChangedWrapper -= value;
				if (transportsHasChangedChangedWrapper == null)
				{
					HostBusinessEntity.Transports.HasChangesChanged -= Transports_HasChangesChanged;
				}
			}
		}

		void ChangeTheVisibility()
		{
			if (HostBusinessEntity != null)
			{
				Enabled = HostBusinessEntity.IsEligibleForJPAFR();
			}
		}

		void OnChangeTheVisibilityRequired(object sender, EventArgs e)
		{
			ChangeTheVisibility();
		}

		void Transports_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			OnChangeTheVisibilityRequired(sender, e);
		}

		void Transports_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			if (transportsHasChangedChangedWrapper != null)
			{
				transportsHasChangedChangedWrapper(sender, e);
			}
		}

		event EventHandler transportsHasChangedChangedWrapper;

		protected new ForwardingConsol HostBusinessEntity
		{
			get { return (ForwardingConsol)base.HostBusinessEntity; }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected ForwardingConsol Consol
		{
			get { return HostBusinessEntity; }
		}

		protected new JPAFRHeader BusinessEntity
		{
			get { return (JPAFRHeader)base.BusinessEntity; }
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Forwarder; }
		}

		#region Plugin Overrides

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return Header;
		}

		protected override Control GetNewUserControl()
		{
			return new JPAFRConsolManifestUserControl(InternalHeader);
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			var header = Header;
			if (afrMainMenuItem == null)
			{
				afrMainMenuItem = new AFRMainMenuItem(header);
			}
			return afrMainMenuItem;
		}
		AFRMainMenuItem afrMainMenuItem;

		#endregion

		bool QueryUserToCreateAFR()
		{
			bool result = false;
			if (Env.Security.AdvanceFilingRules.IsAllowed)
			{
				string queryText = CreateAFRQuery;

				if (queryText != null)
				{
					result = QueryUser(queryText, Res.GetString("JPAFRlugIn|CreateAFRQueryCaption", "Create AFR for Consol"), false);
				}
			}

			return result;
		}

		bool QueryUser(string question, string caption, bool showInTest)
		{
			var result = DialogResult.Yes;
			if (!Globals.IsTest || showInTest)
			{
				result = Globals.Message.Show(
					question,
					caption,
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Information);
			}
			return (result == DialogResult.Yes);
		}

		#region Validity

		public override ZString PlugInNotDisplayedMessage
		{
			get { return coveringLabelText; }
		}

		ZString coveringLabelText;

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return InternalHeader != null;
		}

		protected override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			QueryAddAFR();
			return InternalHeader != null;
		}

		protected void QueryAddAFR()
		{
			coveringLabelText = string.Empty;
			if (InternalHeader == null)
			{
				if (Consol != null && Consol.IsCancelled)
				{
					coveringLabelText = ConsolInactivated;
				}
				else if (!Mutex.IsLocked)
				{
					if (QueryUserToCreateAFR())
					{
						if (!CreateAFR())
						{
							coveringLabelText = MutexLockText;
						}
					}
					else
					{
						coveringLabelText = NotToCreateJobText;
					}
				}
				else
				{
					coveringLabelText = MutexLockText;
				}
			}
		}

		#endregion

		JPAFRHeader InternalHeader
		{
			get
			{
				if (fHeader == null)
				{
					InternalHeader = GetAFRFromConsol();
					if (fHeader != null)
					{
						StartPlugInSynchroniser();
					}
				}

				return fHeader;
			}
			set
			{
				if (fHeader != value && afrMainMenuItem != null && value != null)
				{
					afrMainMenuItem.Header = value;
				}

				fHeader = value;
				if (fHeader != null)
				{
					fHeader.JPH_ParentId = Consol.PK;
					fHeader.JPH_ParentTableCode = Consol.TablePrefix;
					fHeader.InBondDetailInitiator = this;
				}
			}
		}
		JPAFRHeader fHeader;

		JPAFRHeader GetAFRFromConsol()
		{
			return (Consol.GetAFRHeader(false) as JPAFRHeader);
		}

		ZBool CreateAFR()
		{
			ZBool result = Mutex.Lock();
			if (result)
			{
				CreateNewAFR();
				StartPlugInSynchroniser();
				if (!Globals.IsTest)
				{
					Header.HasChanges = true;
					if (Header.DocAddresses != null)
					{
						Header.DocAddresses.HasChanges = true;
					}
				}
			}
			return result;
		}

		void CreateNewAFR()
		{
			InternalHeader = Factory.New<JPAFRHeader>();
		}

		void StartPlugInSynchroniser()
		{
			if (Enabled)
			{
				if (InternalHeader != null)
				{
					((Integration.Customs.JP.AFR.IJPAFRHeaderWithConsolSynchonisation)InternalHeader).SynchroniseWithConsolIfNeeded();
				}
			}
		}

		public override ZGlobalMutex Mutex
		{
			get
			{
				if (fMutex == null)
				{
					fMutex = new ZGlobalMutex(MutexIDs.AFRJobBeingCreated, Consol.PK.ToString());
				}
				return fMutex;
			}
		}
		ZGlobalMutex fMutex;

		static string ConsolInactivated
		{
			get { return Res.GetString("AFRPlugIn|ConsolInactivated", "You can not create an AFR while the consolidation is inactive, to reactivate go to Actions->Make Active"); }
		}

		static string NotToCreateJobText
		{
			get { return Res.GetString("AFRPlugIn|NotToCreateJobText", "You have chosen not to create an AFR now.\r\nPlease change to another tab. You can click back to this tab when/if you need to create an AFR for this Consolidation."); }
		}

		static string MutexLockText
		{
			get { return Res.GetString("AFRPlugIn|MutexLockText", "Someone else is already in the process of creating an AFR for this Consolidation.\r\nYou should be able to access the AFR when the person has saved the record. Please try later."); }
		}

		static string CreateAFRQuery
		{
			get { return Res.GetString("AFRPlugIn|CreateAFRQuery", "Are you sure you want to create an AFR now?"); }
		}

		#endregion

		#region IInBondDetailInitiator Members

		void IInBondDetailInitiator.NotifyUserOfAnInvalidOperation(string text)
		{
			Globals.Message.ShowWarning(text);
		}

		event EventHandler IInBondDetailInitiator.OnDisposing
		{
			add { onDisposing += value; }
			remove { onDisposing -= value; }
		}
		event EventHandler onDisposing;

		#endregion
	}
}
