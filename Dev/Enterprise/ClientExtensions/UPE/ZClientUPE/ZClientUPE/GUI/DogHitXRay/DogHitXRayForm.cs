using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public partial class DogHitXRayForm : ZChildForm
	{
		public DogHitXRayForm()
		{
		}

		public DogHitXRayForm(DogHitXRay dogHitXRay)
			: base(dogHitXRay)
		{
		}

		public DogHitXRay DogHitXRay
		{
			get { return (DogHitXRay)BusinessEntity; }
		}

		public override string FormHeading
		{
			get { return "Dog Hit or X-Ray Hold"; }
		}

		void Process()
		{
			DogHitXRay.RunPreSaveValidation();

			if (DogHitXRay.HasErrors)
			{
				Globals.Message.ShowError("Please fix the errors before proceeding.");
			}
			else
			{
				try
				{
					Cursor.Current = Cursors.WaitCursor;
					if (DogHitXRay.SelectedUPECusHAWB != null)
					{
						DogHitXRay.SetProcessQueueOnSelectedUPECusHAWB();
						try
						{
							DogHitXRay.SelectedUPECusHAWB.Factory.Saved += new BusinessObjectFactory.SavedEventHandler(SelectedUPECusHAWB_FactorySaved);
							ShowDogHitEDocsForm();
						}
						finally
						{
							DogHitXRay.SelectedUPECusHAWB.Factory.Saved -= new BusinessObjectFactory.SavedEventHandler(SelectedUPECusHAWB_FactorySaved);
						}
						DogHitXRay.Clear();
					}
				}
				finally
				{
					Cursor.Current = Cursors.Default;
				}
			}

			TrackingNumberTextBox.Focus();
		}

		protected virtual void ShowDogHitEDocsForm()
		{
			ZFormModaliser.ShowDialogAndDispose(new DogHitXRayeDocsForm(DogHitXRay.SelectedUPECusHAWB));
		}

		void SelectedUPECusHAWB_FactorySaved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			DogHitXRay.SendNotificationEmail();
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void OkButton_Click(object sender, EventArgs e)
		{
			Process();
		}
	}
}
