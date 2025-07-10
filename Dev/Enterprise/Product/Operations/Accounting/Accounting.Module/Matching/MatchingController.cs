using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Unmatching;
using Enterprise.Accounting.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public abstract partial class MatchingController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true; public event EventHandler RefreshGrid;

		#region GetLoadedBusinessEntityInLocalFactory

		// Source entity should be a UnmatchingRow
		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			var unmatchingRow = sourceEntity as UnmatchingRow;
			if (unmatchingRow != null)
			{
				// Note: UnmatchingRow does not exist in the database since it is an aggregation
				// Force DB reload of lazy properties
				UnmatchingRow loadedEntity = new UnmatchingRow(Factory);
				loadedEntity.Initialize(unmatchingRow);
				return loadedEntity;
			}
			else
			{
				return base.GetLoadedBusinessEntityInLocalFactory(sourceEntity);
			}
		}

		protected override void DeleteMultipleCore(BusinessObject[] selectedBusinessObjects)
		{
			Globals.Message.ShowInformation(Res.GetString("1b816ed9-8b71-4ec4-b3f6-af10ea2208b2", "Please select only one Match Session. Match Sessions must be Unmatched individually."));
		}

		#endregion

		#region GetForm

		protected override ZArchitecture.GUI.IZForm GetForm(IBusiness businessEntity)
		{
			if (businessEntity is UnmatchingRow)
			{
				ViewMatchGroupForm viewForm = new ViewMatchGroupForm((UnmatchingRow)businessEntity);
				businessEntity.Factory.Saved += new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
				return viewForm;
			}
			else
			{
				return new NewMatchGroupForm((MatchingBase)businessEntity);
			}
		}

		#endregion

		// Refresh the list of Match Groups at the module level
		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully && RefreshGrid != null)
			{
				RefreshGrid(this, EventArgs.Empty);
			}
		}
	}
}
