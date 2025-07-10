using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Client.TNT.AirCargo
{
	public partial class INDImportForm : ZChildForm
	{
		public INDImportForm(IQDownImportManager businessEntity)
			: base(businessEntity)
		{
			Factory = new BusinessObjectFactory();
		}

		private IQDownImportManager Manager
		{
			get { return (IQDownImportManager)BusinessEntity; }
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>

		public override string FormCaption
		{
			get { return "Quantum Interface IQDown Import"; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		#region Implementation

		#region Events

		void AirCargoSearchButton_Click(object sender, System.EventArgs e)
		{
			var selectedAirCargo = MawbsGrid.ListManager.GetCurrent() as IQDownAirCargo;

			if (selectedAirCargo != null)
			{
				selectedAirCargo.LoadSearchData();
			}
		}

		void UpdateButton_Click(object sender, System.EventArgs e)
		{
			if (ExistingAirCargosGrid.SelectedElements.Length == 1)
			{
				IQDownAirCargo airCargo = (IQDownAirCargo)MawbsGrid.ListManager.GetCurrent();
				TNTAUCustomsAirCargoController airCargoController = GetNewAirCargoController();
				airCargoController.ShowImportedEditForm(ExistingAirCargosGrid.SelectedElements[0].PK, airCargo);
			}
			else
			{
				Globals.Message.ShowError("Please select an existing manifest before continue.", "Update Existing Manifest Failure");
			}
		}

		void CloseButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		void INDImportForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			DialogResult confirmationResult = DialogResult.OK;

			if (!HasAllManifestImported)
			{
				confirmationResult = Globals.Message.ShowConfirmation(
					"Are you sure you want to close without processing?" + System.Environment.NewLine + "Confirm to Close",
					"Not all manifest have been imported",
					"Y", MessageBoxIcon.Question);
			}

			e.Cancel = (confirmationResult != DialogResult.OK);
		}

		void AddButton_Click(object sender, System.EventArgs e)
		{
			IQDownAirCargo airCargo = (IQDownAirCargo)MawbsGrid.ListManager.GetCurrent();
			ZQuery mawbFilter = new ZQuery(CusMAWBSchema.CM_MAWB, airCargo.MasterBill);
			bool mawbsExists = Factory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(CusMAWB)), mawbFilter);

			if (mawbsExists)
			{
				Globals.Message.ShowError("Cannot add new manifest." + System.Environment.NewLine +
					"An existing manifest with the same MasterBill already exist.",
					"Add New Manifest Failure");
			}
			else
			{
				TNTAUCustomsAirCargoController airCargoController = GetNewAirCargoController();
				airCargoController.ShowImportedNewForm(airCargo);
			}
		}

		void UpdateIQDownButton_Click(object sender, System.EventArgs e)
		{
			IQDownAirCargo airCargo = (IQDownAirCargo)MawbsGrid.ListManager.GetCurrent();
			airCargo.CopySearchData();
		}

		void INDImportForm_Closed(object sender, System.EventArgs e)
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

		bool HasAllManifestImported
		{
			get
			{
				bool result = true;
				foreach (IQDownAirCargo airCargo in Manager.AirCargos)
				{
					if (airCargo.LinkedMasterBillRef.IsEmpty)
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
				foreach (IQDownAirCargo airCargo in Manager.AirCargos)
				{
					if (!airCargo.LinkedMasterBillRef.IsEmpty)
					{
						result = true;
						break;
					}
				}

				return result;
			}
		}

		readonly BusinessObjectFactory Factory;

		#endregion
	}
}
