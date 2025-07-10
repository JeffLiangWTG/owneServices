using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.AirCargo.GUI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.GUI
{
	public class UPEAirCargoShipmentMenu : AirCargoShipmentMenu
	{
		public UPEAirCargoShipmentMenu(CusHAWBMessageManager manager)
			: base(manager)
		{
		}

		#region Factory Method Overriding

		public static void RegisterThisTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(OverriddenNew);
		}

		static AirCargoShipmentMenu OverriddenNew(CusHAWBMessageManager manager)
		{
			return new UPEAirCargoShipmentMenu(manager);
		}

		internal static bool IsSubTypeRegistered
		{
			get { return OverridableNewDelegate.IsOverriden; }
		}

		#endregion

		protected override void InitializeMenu()
		{
			base.InitializeMenu();
			MenuItems.Add("-");
			CreateFormalDecMenuItem = new ZMenuItem("Create Formal Declaration", new EventHandler(OnCreateFormalDec_Click));
			MenuItems.Add(CreateFormalDecMenuItem);
			MatchAndCreateFormalDecMenuItem = new ZMenuItem("Requires Formal Declaration", new EventHandler(OnMatchAndCreateFormalDec_Click));
			MenuItems.Add(MatchAndCreateFormalDecMenuItem);
		}

		#region CreateFormalDecMenuItem

		MenuItem CreateFormalDecMenuItem;

		void OnCreateFormalDec_Click(object sender, EventArgs e)
		{
			if (Manager.HAWB.HasChanges)
			{
				Globals.Message.ShowError("You must save the form before you can create a formal declaration.");
			}
			else if (((UPECusHAWB)Manager.HAWB).HasFormalDec)
			{
				Globals.Message.ShowError("A formal declaration has already been created for this shipment.");
			}
			else if (ConfirmFormalDecRequired())
			{
				CreateDeclarationFromAirCargoAndShowNewUnsavedDeclarationForm();
			}
		}

		void CreateDeclarationFromAirCargoAndShowNewUnsavedDeclarationForm()
		{
			ZController controller = NewJobDeclarationController();
			ZForm form = (ZForm)controller.ShowNewForm();
			JobDeclaration newDeclaration = (JobDeclaration)form.BusinessEntity;
			NotificationBuffer notify = new NotificationBuffer();
			NewDeclarationFromAirCargoCreator(Manager.HAWB).Create(newDeclaration, notify);
			((UPEJobDeclaration)newDeclaration).ResetRelatedCusHAWBs(); // Reset Related HAWBs because the collection is cached and empty
			new FreightRateCalculator().CalculateFreightRateOnDeclaration(newDeclaration);

			if (notify.Events.Length > 0)
			{
				Globals.Message.ShowWarning(notify.AsString);
			}
		}

		protected virtual UPEDeclarationFromAirCargoCreator NewDeclarationFromAirCargoCreator(CusHAWB houseAirCargo)
		{
			return new UPEDeclarationFromAirCargoCreator(houseAirCargo);
		}

		protected virtual ZController NewJobDeclarationController()
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration);
		}

		#endregion

		#region MatchAndCreateFormalDecMenuItem

		MenuItem MatchAndCreateFormalDecMenuItem;

		void OnMatchAndCreateFormalDec_Click(object sender, EventArgs e)
		{
			UPECusHAWB hAWB = (UPECusHAWB)Manager.HAWB;
			if (hAWB.HasChanges)
			{
				Globals.Message.ShowWarning("You must save the form before you can perform this action");
			}
			else if (hAWB.HasFormalDec)
			{
				Globals.Message.ShowWarning("A formal declaration has already been created for this shipment.");
			}
			else if (hAWB.IsOnOrgMatchingQueueOrMatchingCompleted)
			{
				Globals.Message.ShowWarning("Shipment consignee/importer and consignor are already on the matching queue, or matching has been completed");
			}
			else if (ConfirmFormalDecRequired())
			{
				Cursor.Current = Cursors.WaitCursor;
				try
				{
					hAWB.CreateFormalDecAndMatch();
					FactorySave(hAWB.Factory);
					Globals.Message.ShowInformation("Shipment queued for matching, or match completed and declaration will be created");
				}
				catch (ZSaveException ex)
				{
					Globals.Message.ShowError(ex.Message);
				}
				finally
				{
					Cursor.Current = Cursors.Default;
				}
			}
		}

		protected virtual void FactorySave(BusinessObjectFactory factory)
		{
			factory.Save();
		}

		#endregion

		#region Implementation

		bool ConfirmFormalDecRequired()
		{
			bool proceed = true;
			if (!((UPECusHAWB)Manager.HAWB).IsFormalDecRequired)
			{
				if (Globals.Message.Show("A formal declaration is not required for this shipment. Proceed anyway?", "", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.No)
				{
					proceed = false;
				}
			}
			return proceed;
		}

		#endregion
	}
}
