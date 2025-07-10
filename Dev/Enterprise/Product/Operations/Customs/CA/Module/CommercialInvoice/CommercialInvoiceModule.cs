using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public class CommercialInvoiceModule : Customs.Module.CommercialInvoiceModule
	{
		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>();
			result.AddRange(base.GetNewActionMenuItems());
			result.Add(new ZMenuItem(Res.GetData("F9562D81-9513-4B85-BFBB-C14C6CC53DF2", "Create New CLVS Shipment"), HandleCreateNewLVXShipment));
			return result.ToArray();
		}

		void HandleCreateNewLVXShipment(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects.Length == 1 && SelectedBusinessObjects[0] is JobComInvoiceHeader commercialinvoice)
			{
				var factory = new BusinessObjectFactory();
				var newCommercialInvoice = factory.Load<JobComInvoiceHeader>(commercialinvoice.PK);
				if (newCommercialInvoice == null || !newCommercialInvoice.JZ_JE.IsEmpty)
				{
					Globals.Message.ShowWarning(Res.GetString("94A174DF-76FC-4593-B54F-5EDB3DD690D9", "Please find Commercial Invoice not attached to any job."));
				}
				else
				{
					var lvx = LVXController.CreateNewBusinessObject(factory, newCommercialInvoice);
					if (lvxController == null)
					{
						lvxController = ZControllerFactory.Create(ControllerIDs.Customs.CA.CALVXJobs);
					}
					lvxController.SetFormsModalTo(LocateMainForm());
					lvxController.ShowFormForNewEntity(lvx);
				}
			}
			else
			{
				Globals.Message.ShowWarning(Res.GetString("43A73E1E-54E0-4435-9533-00551B5EDD62", "Please select one Commercial Invoice."));
			}
		}

		internal ZController lvxController;
	}
}
