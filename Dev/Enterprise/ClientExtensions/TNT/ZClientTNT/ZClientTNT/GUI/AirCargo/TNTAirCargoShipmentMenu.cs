using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.AirCargo.GUI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TNT.GUI
{
	public class TNTAirCargoShipmentMenu : AirCargoShipmentMenu
	{
		protected TNTAirCargoShipmentMenu(CusHAWBMessageManager manager)
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
			return new TNTAirCargoShipmentMenu(manager);
		}

		#endregion

		protected override void InitializeMenu()
		{
			base.InitializeMenu();
			MenuItems.Add("-");
			CreateFormalDecMenuItem = new ZMenuItem(CreateFormalMenuHeading, new EventHandler(OnCreateFormalDec_Click));
			MenuItems.Add(CreateFormalDecMenuItem);
		}

		internal const string CreateFormalMenuHeading = "Create Formal Declaration";

		MenuItem CreateFormalDecMenuItem;

		void OnCreateFormalDec_Click(object sender, EventArgs e)
		{
			if (Manager.HAWB.HasChanges)
			{
				Globals.Message.ShowError("You must save the form before you can create a Formal Declaration.");
			}
			else if (Manager.HAWB.Declaration != null)
			{
				Globals.Message.ShowError(string.Format("Formal Declaration ({0}) is already exist for this {1}.", Manager.HAWB.Declaration[JobDeclarationSchema.JE_DeclarationReference.Name].ToString(), Manager.HAWB.UnderbondHumanReadableName), "Formal Declaration Already Exist");
			}
			else
			{
				CreateDeclarationFromAirCargo();
			}
		}

		void CreateDeclarationFromAirCargo()
		{
			NotificationBuffer buffer = new NotificationBuffer();
			TNTDeclarationFromAirCargoCreator declarationCreator = new TNTDeclarationFromAirCargoCreator(Manager.HAWB);
			Customs.Business.BaseJobDeclaration declaration = declarationCreator.Create(buffer);

			try
			{
				Manager.HAWB.Factory.Save();
			}
			catch (ZSaveConcurrencyException)
			{
				buffer.Notify(new ErrorNotification(ErrorType.Error, "Air Cargo Record cannot be saved as another user or process is currently modifying it. Please try again later"));
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				buffer.Notify(new ErrorNotification(ErrorType.Error, ex.Message));
			}

			if (buffer.HasErrors)
			{
				Globals.Message.ShowError(GetErrorMessage(buffer), "Error(s) occurred while creating Formal Declaration");
			}
			else if (buffer.HasWarnings)
			{
				Globals.Message.ShowWarning(buffer.AsString, string.Format("Formal Declaration ({0}} was created with warnings", declaration.JE_DeclarationReference));
			}
			else
			{
				Globals.Message.Show(string.Format("Formal Declaration ({0}) was successfully created.", declaration.JE_DeclarationReference.ToString()), "Formal Declaration Created", MessageBoxButtons.OK, MessageBoxIcon.None);
			}
		}

		ZString GetErrorMessage(NotificationBuffer buffer)
		{
			ZStringBuilder builder = new ZStringBuilder();

			foreach (INotification current in buffer.GetEventsByType(ErrorType.Error))
			{
				builder.AppendIfNotEmpty(current.Message);
			}
			return builder.ToStringWithNewLineBetweenAppends();
		}
	}
}
