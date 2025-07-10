using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.TNT.AirCargo
{
	public class XXXMasterAirCargo : NonPersistentBusinessObject, IObsoleteValidation, IAirCargo
	{
		public XXXMasterAirCargo(BusinessObjectFactory factory)
			: base(factory)
		{
			SelectedAirCargosList = System.Array.Empty<ZGuid>();
		}

		#region MatchingMasterbills

		public ReadOnlyCusMAWBCollection MatchingMasterbills
		{
			get
			{
				if (fMatchingMasterbills == null)
				{
					fMatchingMasterbills = new ReadOnlyCusMAWBCollection(Factory);
				}

				return fMatchingMasterbills;
			}
		}

		ReadOnlyCusMAWBCollection fMatchingMasterbills;

		#endregion

		#region LoadSearchData

		public void LoadSearchData(XXXAirCargo airCargo)
		{
			MatchingMasterbills.Load(airCargo.SearchFilter);
		}

		#endregion

		#region AirCargos

		public XXXAirCargoCollection AirCargos
		{
			get
			{
				if (fAirCargos == null)
				{
					fAirCargos = new XXXAirCargoCollection(Factory);
				}
				return fAirCargos;
			}
		}

		XXXAirCargoCollection fAirCargos;

		#endregion

		#region SetSelectedElements

		public void SetSelectedElements(XXXAirCargo[] selectedAirCargos)
		{
			SelectedAirCargosList = new ZGuid[selectedAirCargos.Length];
			for (int i = 0; i < selectedAirCargos.Length; i++)
			{
				SelectedAirCargosList[i] = selectedAirCargos[i].PK;
			}
		}

		#endregion

		#region IAirCargo Members

		public void PopulateNewData(CusMAWB masterBill)
		{
			PopulateFromAirCargos(masterBill);
		}

		public void PopulateData(CusMAWB masterBill)
		{
			PopulateFromAirCargos(masterBill);
		}

		public event TNTProgressEventHandler Progress;

		#region IsValid

		public bool IsValid
		{
			get
			{
				bool result = true;
				if (AirCargos.Count > 0)
				{
					ZString errorMessage = AirCargosErrorMessages;
					if (!errorMessage.IsEmpty)
					{
						DialogResult answer = Globals.Message.Show("The following errors were encoutered" + errorMessage, "Error Encoutered", MessageBoxButtons.YesNo, MessageBoxIcon.Error, DialogResult.No);
						result = (answer == DialogResult.Yes);
					}
				}
				else
				{
					Globals.Message.ShowError("No Air Cargo Consignment to import");
					result = false;
				}
				return result;
			}
		}

		#endregion

		#region Save

		public void Save()
		{
			foreach (ZGuid airCargoPK in SelectedAirCargosList)
			{
				XXXAirCargo airCargo = (XXXAirCargo)AirCargos.FindByPK(airCargoPK);
				if (airCargo != null)
				{
					airCargo.Save();
				}
			}
		}

		#endregion

		#endregion

		#region Implementation

		void PopulateFromAirCargos(CusMAWB masterBill)
		{
			int total = AirCargos.Count;
			int i = 0;
			foreach (ZGuid airCargoPK in SelectedAirCargosList)
			{
				XXXAirCargo airCargo = (XXXAirCargo)AirCargos.FindByPK(airCargoPK);
				if (airCargo != null)
				{
					ShowProgress(++i, SelectedAirCargosList.Length, "Loading Housebill '" + airCargo.HouseBill + "'");
					airCargo.PopulateData(masterBill);
				}
			}
		}

		ZString AirCargosErrorMessages
		{
			get
			{
				ZString errorMessage = "";
				foreach (XXXAirCargo airCargo in AirCargos)
				{
					if (airCargo.Buffer.HasErrors)
					{
						errorMessage += System.Environment.NewLine + airCargo.Buffer.AsString;
					}
				}
				return errorMessage;
			}
		}

		void ShowProgress(int current, int total, string message)
		{
			if (Progress != null)
			{
				Progress(this, new TNTProgressEventArgs(current, total, message));
			}
		}

		#endregion

		ZGuid[] SelectedAirCargosList;

		public IReadOnlyList<ZGuid> SelectedAirCargosListTest
		{
			get { return SelectedAirCargosList; }
		}
	}
}
