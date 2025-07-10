using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.OpportunityManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class PSQRelatedOpportunitiesUserControl : ZUserControl
	{
		public PSQRelatedOpportunitiesUserControl()
		{
			InitializeComponent();
		}

		void AddNewOpportunity()
		{
			LastController = ZControllerFactory.Create(ControllerIDs.Opportunity);
			ZForm form = (ZForm)LastController.ShowNewForm();
			using (((BusinessObject)form.BusinessEntity).SuspendSettingHasChanges())
			{
				PopulateNewOpportunity(form);
			}
			new NewOppTracker(form.BusinessEntity, (EDIOrgOpportunityCollection)RelatedOpportunities);
		}

		class NewOppTracker
		{
			public NewOppTracker(IBusiness bizo, EDIOrgOpportunityCollection relatedOpps)
			{
				oppPK = bizo.Identifier;
				relatedOppRef = new WeakReference(relatedOpps);

				bizo.Factory.Saved += new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
			}

			void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
			{
				EDIOrgOpportunityCollection relatedOpps;
				if (relatedOppRef != null && (relatedOpps = relatedOppRef.Target as EDIOrgOpportunityCollection) != null)
				{
					relatedOpps.Add(relatedOpps.Factory.Load<EDIOrgOpportunity>(oppPK));
					relatedOppRef = null;
				}

				factory.Saved -= Factory_Saved;
			}

			readonly ZGuid oppPK;
			WeakReference relatedOppRef;
		}

		void PopulateNewOpportunity(ZForm form)
		{
			EDIOrgOpportunity newOpportunity = form.BusinessEntity as EDIOrgOpportunity;
			if (newOpportunity != null)
			{
				newOpportunity.InitializeFromPSQ(CurrentDataItem);
			}
		}

		void OpportunitiesGrid_DoubleClick(object sender, EventArgs e)
		{
			OpenRelatedItem();
		}

		void NewButton_Click(object sender, EventArgs e)
		{
			AddNewOpportunity();
		}

		void AttachButton_Click(object sender, EventArgs e)
		{
			ShowRecordAttacher();
		}

		void DettachButton_Click(object sender, EventArgs e)
		{
			if (OpportunitiesGrid.ListManager.Position > -1)
			{
				DialogResult dialogResult = Globals.Message.Show("Are you sure you want to detach the selected opportunity?", "Confirm Detach...", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				if (dialogResult == DialogResult.Yes)
				{
					EDIOrgOpportunity opportunity = (EDIOrgOpportunity)OpportunitiesGrid.ListManager.GetCurrent();
					RelatedOpportunities.Remove(opportunity);
				}
			}
		}

		void EditButton_Click(object sender, EventArgs e)
		{
			OpenRelatedItem();
		}

		void OpenRelatedItem()
		{
			if (OpportunitiesGrid.ListManager.Position > -1)
			{
				var relatedItem = (EDIOrgOpportunity)OpportunitiesGrid.ListManager.GetCurrent();
				LastController = ZControllerFactory.Create(ControllerIDs.Opportunity);
				LastController.ShowEditForm(relatedItem);
			}
		}

		protected void ShowRecordAttacher()
		{
			LastAttacher = new ZRecordAttacher(RelatedOpportunities, new OrgOpportunityCollection(CurrentDataItem.Factory),
				ModuleIDs.Opportunity);
			LastAttacher.Show((IZForm)FindForm());
		}

		#region Implementation

		public new ProfessionalServicesQuote CurrentDataItem
		{
			get { return (ProfessionalServicesQuote)base.CurrentDataItem; }
		}

		OrgOpportunityCollection RelatedOpportunities
		{
			get { return CurrentDataItem.RelatedOpportunities; }
		}

		internal ZRecordAttacher LastAttacher;

		internal ZController LastController;

		#endregion
	}
}
