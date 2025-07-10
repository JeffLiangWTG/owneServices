using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Module.AirCargo;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.TNT.AirCargo
{
	public class TNTAUCustomsAirCargoController : AUCustomsAirCargoController
	{
		public TNTAUCustomsAirCargoController()
		{
			AirCargo = null;
		}

		#region ShowImportedNewForm

		public IZForm ShowImportedNewForm(IAirCargo airCargo)
		{
			this.AirCargo = airCargo;
			ZForm result = (ZForm)ShowNewForm();
			ValidateLoadedData(result);

			return result;
		}

		#endregion

		#region ShowImportedEditForm

		public IZForm ShowImportedEditForm(ZGuid masterBillPK, IAirCargo airCargo)
		{
			CusMAWB masterBill = (CusMAWB)Factory.Load(typeof(CusMAWB), masterBillPK);
			ZForm result = null;

			if (masterBill != null)
			{
				if (masterBill.ChildBills.Count > 0)
				{
					this.AirCargo = airCargo;
					result = (ZForm)ShowEditForm(masterBill);
					ValidateLoadedData(result);
				}
				else
				{
					Globals.Message.ShowError(string.Format("{0} hasn't got any Housebill.{1}System cannot load it.", masterBill.UnderbondHumanReadableName, System.Environment.NewLine));
				}
			}
			else
			{
				throw new ArgumentException("Cannot load existing Masterbill information", nameof(masterBillPK));
			}

			return result;
		}

		#endregion

		#region GetNewBusinessEntityInLocalFactory

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			IBusiness result = base.GetNewBusinessEntityInLocalFactory();
			if (Importing)
			{
				LoadFromIQDownAirCargo((CusMAWB)result, true);
			}

			return result;
		}

		#endregion

		#region GetLoadedBusinessEntityInLocalFactory

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			IBusiness result = base.GetLoadedBusinessEntityInLocalFactory(sourceEntity);
			if (Importing)
			{
				LoadFromIQDownAirCargo((CusMAWB)result, false);
			}

			return result;
		}

		#endregion

		#region LoadFromIQDownAirCargo

		void LoadFromIQDownAirCargo(CusMAWB masterBill, bool importNew)
		{
			if (Importing)
			{
				using (LoadProgressForm = new ProgressForm())
				{
					LoadProgressForm.ShowCancelButton = false;
					LoadProgressForm.Text = "Processing Housebills";
					TNTProgressEventHandler eventHandler = new TNTProgressEventHandler(AirCargo_Progress);
					try
					{
						AirCargo.Progress += eventHandler;
						LoadProgressForm.Show();
						if (importNew)
						{
							AirCargo.PopulateNewData(masterBill);
						}
						else
						{
							AirCargo.PopulateData(masterBill);
						}
						LoadProgressForm.Close();
					}
					finally
					{
						AirCargo.Progress -= eventHandler;
					}
				}
			}
		}

		void AirCargo_Progress(object sender, TNTProgressEventArgs e)
		{
			LoadProgressForm.Status = e.Message;
			LoadProgressForm.PercentComplete = e.PercentComplete;
		}

		#endregion

		#region ValidateLoadedData & Events Handling

		void ValidateLoadedData(ZForm form)
		{
			if (OkToShow)
			{
				form.DisableNewAction();
				form.BusinessEntity.HasChanges = true;
				form.BusinessEntity.Factory.Saved += new BusinessObjectFactory.SavedEventHandler(Save);
				form.BusinessEntity.RunPreSaveValidation();
				form.Closed += new EventHandler(AirCargoImportForm_Closed);
			}
			else
			{
				form.Close();
			}
		}

		bool OkToShow
		{
			get { return !Importing || AirCargo.IsValid; }
		}

		void Save(BusinessObjectFactory factory, bool saveSuccessful)
		{
			if (saveSuccessful && Importing)
			{
				AirCargo.Save();
			}
		}

		void AirCargoImportForm_Closed(object sender, EventArgs e)
		{
			ZForm form = sender as ZForm;
			if (form != null)
			{
				form.BusinessEntity.Factory.Saved -= new BusinessObjectFactory.SavedEventHandler(Save);
				form.Closed -= new EventHandler(AirCargoImportForm_Closed);
			}
		}

		#endregion

		bool Importing
		{
			get { return AirCargo != null; }
		}

		IAirCargo AirCargo;

		ProgressForm LoadProgressForm;
	}
}
