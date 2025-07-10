using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public class AirCargoConsolPlugIn : AirCargoPlugIn
	{
		public AirCargoConsolPlugIn(ForwardingConsol hostBusinessEntity)
			: base(hostBusinessEntity)
		{
			hostBusinessEntity.SetAirCargoSynchroniserRetriever(() => MasterBill == null ? null : new MAWBToConsolBridge(MasterBill));
		}

		protected override ForwardingConsol Consol
		{
			get { return (ForwardingConsol)ManifestProvider; }
		}

		CusMAWBMessageManager fManager;
		protected internal CusMAWBMessageManager Manager
		{
			get
			{
				if (fManager == null)
				{
					fManager = new CusMAWBMessageManager(() => MasterBill);
				}
				return fManager;
			}
		}

		public override void OnUserControlShown()
		{
			base.OnUserControlShown();
			if (HasUserControl)
			{
				((BaseACAStandAloneUserControl)UserControl).SetupPlugins();
			}
		}

		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			ContinueWithSave result = base.ShowPreSaveDialogsCore();
			if (result == ContinueWithSave.Yes && MasterBillFromLocalCache != null && MasterBillFromLocalCache.HasChanges)
			{
				Business.IMessageManager manager = ((Business.IMessageManageableBizObj)MasterBill).GetMessageManagerForAmendmentDetection();
				result = new SendsMessagesToCustomsGUI().DetermineRequiredMessagesAndSendThem(manager);
			}
			return result;
		}

		#region IZPlugIn Members

		protected bool CreateAirCargoJobsIfRequired()
		{
			bool result = true;

			var mawb = MasterBill;
			if (mawb != null && !mawb.IsDeleted)
			{
				var shipmentPKs = Consol.Shipments.GetPKs();
				var cusHAWBs = CusHAWB.Load(Factory, shipmentPKs);

				if (Consol.Shipments.Count != cusHAWBs.Length)
				{
					CusHAWB.Load(Factory, shipmentPKs, true);
				}

				foreach (var shipment in Consol.Shipments)
				{
					Factory.AddFetchHint(JobShipmentSchema.Instance, new ZQuery(JobShipmentSchema.JS_JS_ColoadMasterShipment, shipment.PK));
				}

				foreach (ForwardingShipment shipment in Consol.Shipments)
				{
					var loader = new CusHAWBLoaderWithMutexManagement(shipment);
					var cusHAWB = loader.Load(masterBill);

					if (cusHAWB == null && Env.Registry.AutoPopulateHAWBsOnConsol)
					{
						Mutexes.Add(loader);
						cusHAWB = loader.CreateWithMutexLock(masterBill);

						if (cusHAWB == null)
						{
							result = false;
							break;
						}
					}
				}
			}

			return result;
		}

		public override void OnMenuShown()
		{
			base.OnMenuShown();
			CreateAirCargoJobsIfRequired();
		}

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return MasterBill != null;
		}

		protected override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			bool result = false;

			if (Consol != null)
			{
				if (MasterBill == null && CheckForInvalidAirCargo() && MutexForConsol != null)
				{
					if (QueryUserToCreateAirCargoJob())
					{
						if (FormPreSaved(Consol, Form))
						{
							if (MutexForConsol.Lock())
							{
								var newFactory = new BusinessObjectFactory();
								var consolInNewFactory = newFactory.Load<ForwardingConsol>(Consol.PK);
								var mawbInNewFactory = CusMAWB.Load(consolInNewFactory);
								if (mawbInNewFactory == null)
								{
									mawbInNewFactory = CusMAWB.CreateNew(consolInNewFactory);
								}
								else
								{
									mawbInNewFactory.SynchroniseData();
								}
								try
								{
									newFactory.Save();
									result = true;
								}
								catch (Exception e) when (!e.IsCriticalException())
								{
									if (!mawbInNewFactory.IsInDatabase)
									{
										mawbInNewFactory.Delete();
									}

									ZExceptionReporting.HandleSaveException(e);
								}
								finally
								{
									MutexForConsol.Unlock();
								}
							}
							else
							{
								plugInNotDisplayedMessage = SomeoneIsEditingTheConsol;
							}
						}
						else
						{
							plugInNotDisplayedMessage = YouHaveToSaveBeforeCreateAnAirCargo;
						}
					}
					else
					{
						plugInNotDisplayedMessage = YouHaveChosenNotToCreateAnAirCargo;
					}
				}

				if (MasterBill != null)
				{
					result = CreateAirCargoJobsIfRequired();
					if (!result)
					{
						plugInNotDisplayedMessage = SomeoneIsEditingOneOfTheShipments;
					}
				}
			}

			return result;
		}

		bool CheckForInvalidAirCargo()
		{
			var filter = ForwardingConsol.GetMAWBsFilter(Consol.PK, reloadExistingRows: true, new ZString[] { string.Empty, Core.Constants.Customs.CusSCAOceanBillApplicationCodes.AustraliaLegacy });
			var legacyMAWB = Consol.Factory.LoadTop1<Business.CusMAWB>(filter);
			if (legacyMAWB != null)
			{
				plugInNotDisplayedMessage = Res.GetString("5C9BCD95-988C-4308-A6C7-78FD9F32C186", "Air Cargo Data is Invalid.");
				return false;
			}

			return true;
		}

		bool QueryUserToCreateAirCargoJob()
		{
			return Globals.Message.Show(DoYouWantToCreateAnAirCargo, string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes;
		}

		public override ZString PlugInNotDisplayedMessage => plugInNotDisplayedMessage;
		ZString plugInNotDisplayedMessage;

		public const string SomeoneIsEditingTheConsol = "Someone else is already in the process of creating an air cargo master bill for this consol.\r\nYou cannot process this job until they save their data. Please try later.";
		public const string SomeoneIsEditingOneOfTheShipments = "Someone else is already in the process of creating an air cargo house bill for one of the attached shipments.\r\nYou cannot process this job until they save their data. Please try later.";
		public const string YouHaveChosenNotToCreateAnAirCargo = "You have chosen not to create an air cargo job now.\r\nPlease change to another tab. Click back to this tab later if you want to create an air cargo job for this consol at another time.";
		public const string DoYouWantToCreateAnAirCargo = "Do you want to create an air cargo job at this time?";
		public const string YouHaveToSaveBeforeCreateAnAirCargo = "You have to save the data first before creating an air cargo job.\r\nPlease change to another tab and save the job, then click back to this tab to create an air cargo job for this consol.";

		public override string Name
		{
			get { return "Air Cargo"; }
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			AirCargoMasterMenu result = AirCargoMasterMenu.New(Consol, Manager);
			result.Enabled = IsFormEditable;
			result.ParentForm = Form;
			return result;
		}

		public override bool CanDelete
		{
			get { return MasterBill == null || MasterBill.CanDelete; }
		}

		public override string CannotDeleteMessage
		{
			get { return "You cannot delete this record as there are air cargo messages sent to the Customs."; }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return MasterBill;
		}

		protected override void OnIsFormEditableChanged()
		{
			base.OnIsFormEditableChanged();
			TopLevelMenu.Enabled = IsFormEditable;
		}

		protected override Control GetNewUserControl()
		{
			return new CMRACAStandAloneUserControl();
		}

		#endregion

		List<CusHAWBLoaderWithMutexManagement> Mutexes
		{
			get
			{
				return mutexes ?? (mutexes = new List<CusHAWBLoaderWithMutexManagement>());
			}
		}
		List<CusHAWBLoaderWithMutexManagement> mutexes;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnlockMutexIfNecessary();
			}

			base.Dispose(disposing);
		}

		protected void UnlockMutexIfNecessary()
		{
			Mutexes.ForEach(x => x.UnlockMutexIfNecessary());
			Mutexes.Clear();

			if (MutexForConsol != null && MutexForConsol.HasLock)
			{
				MutexForConsol.Unlock();
			}
		}

		#region Implementation

		public override void OnSaving()
		{
			base.OnSaving();
			if (Enabled && MasterBill != null)
			{
				MasterBill.SynchroniseData();
			}
		}

		protected internal CusMAWB MasterBill
		{
			get
			{
				if (masterBill == null || masterBill.IsDeleted)
				{
					masterBill = CusMAWB.Load(Consol);
				}
				return masterBill;
			}
		}
		CusMAWB masterBill;

		CusMAWB MasterBillFromLocalCache
		{
			get
			{
				CusMAWB result = null;
				if (Consol != null)
				{
					ZQuery filter = new ZQuery(CusMAWBSchema.CM_JK, Consol.PK);
					filter.AddToFilter(CusMAWBSchema.CM_ApplicationCode, Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages);
					filter.FetchOnlyFromLocalCache = true;
					CusMAWB[] mAWBs = Consol.Factory.Load<CusMAWB>(filter);
					result = mAWBs.Length == 0 ? null : mAWBs[0];
				}
				return result;
			}
		}

		protected override void ChangeTheVisibilityCore()
		{
			Enabled = IsValidConsolForAirCargo;
		}

		protected bool IsValidConsolForAirCargo
		{
			get
			{
				return Consol != null
				&& Consol.JK_RL_NKDischargePort.StartsWith("AU")
				&& Consol.JK_TransportMode == Core.Constants.TransportModes.Air;
			}
		}

		#endregion

	}
}
