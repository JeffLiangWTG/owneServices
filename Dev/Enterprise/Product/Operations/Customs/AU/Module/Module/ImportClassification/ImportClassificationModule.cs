using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Module
{
	/// <summary>
	/// Module Controller for ImportClassification.
	/// </summary>
	public class ImportClassificationModule : Customs.Module.ImportClassificationModule
	{
		public ImportClassificationModule()
		{
			AddImportFromCSVDataMenuItem(CreateImportFromCSVForm, false);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AUImportClassificationFilterControl(GridCollection, (AUImportClassificationFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ImportClassificationCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AUImportClassificationFilterBusinessObject();
		}

		KForm CreateImportFromCSVForm()
		{
			return new ImportClassificationsFromCSVForm();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewActionMenuItems());
			MenuItem bulkLookupChangeMenuItem = new ZMenuItem(ResString.GetMultilingualString("AUImportClassificationModule.Action.BulkLookupChange", "Bulk Lookup Change"));
			bulkLookupChangeMenuItem.Click += BulkLookupChangeMenuItem_Click;
			result.Add(bulkLookupChangeMenuItem);
			return result.ToArray();
		}

		protected void BulkLookupChangeMenuItem_Click(object sender, EventArgs e)
		{
			if (!Env.Security.AUImportLookupBulkChange.IsAllowed)
			{
				Env.Security.AUImportLookupBulkChange.ShowError();
			}
			else
			{
				BusinessObjectFactory isolatedFactory = new BusinessObjectFactory();
				BulkLookupChanger changer = new BulkLookupChanger(isolatedFactory);
				changer.EstimatedLookupCount = ((BusinessObjectCollection)GridCollection).GetEstimatedLoadCount(FilterBusinessObject.Filter);
				if (ZFormModaliser.ShowDialogAndDispose(new BulkLookupChangeForm(changer)) == System.Windows.Forms.DialogResult.OK)
				{
					ZQuery filter = FilterBusinessObject.Filter;
					filter.AddToFilter(((BusinessObjectCollection)GridCollection).CompleteFilter);
					changer.ChangeLookups(filter);
					if (changer.TreatmentsChanged == 0 && changer.TreatmentsRemoved == 0 && changer.PartsAdjusted == 0)
					{
						Globals.Message.Show("No changes have been made.");
					}
					else
					{
						string message = Res.GetString("7f2d6215-e9c7-499b-91c4-c3cce97c1a74", @"Number of Treatment Codes changed  = {0}
Number of Treatment Codes removed = {1},
Number of Parts with Treatment Code Override adjusted = {2}

Are you sure you wish to save the changes?"
							, changer.TreatmentsChanged.ToString().Trim(), changer.TreatmentsRemoved.ToString().Trim(), changer.PartsAdjusted.ToString().Trim());
						DialogResult result = Globals.Message.Show(message, Res.GetString("eb6668ce-1792-45a9-957b-ab41708546d6", "Save Changes?"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question, DialogResult.Cancel);
						if (result == DialogResult.OK)
						{
							isolatedFactory.Save();
						}
					}
				}
			}
		}
	}
}
