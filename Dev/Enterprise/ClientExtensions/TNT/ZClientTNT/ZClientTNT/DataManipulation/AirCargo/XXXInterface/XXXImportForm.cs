using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Client.TNT.AirCargo
{
	public partial class XXXImportForm : ZChildForm
	{
		public XXXImportForm(XXXImportManager businessEntity)
			: base(businessEntity)
		{
		}

		XXXImportManager Manager
		{
			get { return (XXXImportManager)BusinessEntity; }
		}

		public override string FormCaption
		{
			get { return "Quantum Interface XXX Import"; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		#region Implementation

		#region Events

		void AirCargoSearchButton_Click(object sender, System.EventArgs e)
		{
			Manager.MasterAirCargo.LoadSearchData((XXXAirCargo)MawbsGrid.ListManager.GetCurrent());
		}

		void AirCargoClearButton_Click(object sender, System.EventArgs e)
		{
			((XXXAirCargo)MawbsGrid.ListManager.GetCurrent()).ClearSearchData();
		}

		void UpdateButton_Click(object sender, System.EventArgs e)
		{
			if (ExistingAirCargosGrid.SelectedElements.Length == 1 && MawbsGrid.SelectedElements.Length > 0)
			{
				Manager.MasterAirCargo.SetSelectedElements(SelectedAirCargos);
				TNTAUCustomsAirCargoController airCargoController = GetNewAirCargoController();
				airCargoController.ShowImportedEditForm(ExistingAirCargosGrid.SelectedElements[0].PK, Manager.MasterAirCargo);
			}
			else
			{
				Globals.Message.ShowError("Please select an existing manifest before continue.", "Update Existing Manifest Failure");
			}
		}

		XXXAirCargo[] SelectedAirCargos
		{
			get
			{
				BusinessObject[] selectedElements = MawbsGrid.SelectedElements;
				XXXAirCargo[] result = new XXXAirCargo[selectedElements.Length];
				for (int index = 0; index < selectedElements.Length; index++)
				{
					result[index] = (XXXAirCargo)selectedElements[index];
				}
				return result;
			}
		}

		void CloseButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		void XXXImportForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			DialogResult confirmationResult = DialogResult.OK;

			if (!HasImportedAllHouseBills)
			{
				confirmationResult = Globals.Message.ShowConfirmation(
					"Are you sure you want to close without processing?" + System.Environment.NewLine + "Confirm to Close",
					"Not all Housebills have been imported",
					"Y", MessageBoxIcon.Question);
			}

			e.Cancel = (confirmationResult != DialogResult.OK);
		}

		void XXXImportForm_Closed(object sender, System.EventArgs e)
		{
			if (HasFileBeenProcessed)
			{
				Manager.MoveFileToProcessedDirectory();
			}
		}

		#endregion

		TNTAUCustomsAirCargoController GetNewAirCargoController()
		{
			TNTAUCustomsAirCargoController result = (TNTAUCustomsAirCargoController)ZControllerFactory.Create(ControllerIDs.Customs.AU.AirCargo) ?? throw new ModuleCannotFindControllerException("GetNewController() returns null");

			result.EnablePreviousNextSupport = false;
			result.SetFormsModalTo(this);

			return result;
		}

		bool HasImportedAllHouseBills
		{
			get
			{
				bool result = true;
				foreach (XXXAirCargo airCargo in Manager.MasterAirCargo.AirCargos)
				{
					if (airCargo.LinkedHouseBillRef.IsEmpty)
					{
						result = false;
						break;
					}
				}

				return result;
			}
		}

		bool HasFileBeenProcessed
		{
			get
			{
				bool result = false;
				foreach (XXXAirCargo airCargo in Manager.MasterAirCargo.AirCargos)
				{
					if (!airCargo.LinkedHouseBillRef.IsEmpty)
					{
						result = true;
						break;
					}
				}

				return result;
			}
		}

		#endregion
	}
}
