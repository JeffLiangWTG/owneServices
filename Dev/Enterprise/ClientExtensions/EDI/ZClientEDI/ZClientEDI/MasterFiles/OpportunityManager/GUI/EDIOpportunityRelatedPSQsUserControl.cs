using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class EDIOpportunityRelatedPSQsUserControl : ZUserControl
	{
		public EDIOpportunityRelatedPSQsUserControl()
		{
			InitializeComponent();
		}

		void AddNewPSQ()
		{
			LastController = ZControllerFactory.Create(ClientControllerRegistration.ProfessionalServicesQuote);
			ZForm form = (ZForm)LastController.ShowNewForm();
			using (((BusinessObject)form.BusinessEntity).SuspendSettingHasChanges())
			{
				PopulateNewPSQ(form);
			}
			new NewPSQTracker(form.BusinessEntity, RelatedPSQs);
		}

		class NewPSQTracker
		{
			public NewPSQTracker(IBusiness bizo, ProfessionalServicesQuoteCollection relatedPSQs)
			{
				psqPK = bizo.Identifier;
				relatedPSQRef = new WeakReference(relatedPSQs);

				bizo.Factory.Saved += new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
			}

			void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
			{
				ProfessionalServicesQuoteCollection relatedPSQs;
				if (relatedPSQRef != null && (relatedPSQs = relatedPSQRef.Target as ProfessionalServicesQuoteCollection) != null)
				{
					relatedPSQs.Add(relatedPSQs.Factory.Load<ProfessionalServicesQuote>(psqPK));
					relatedPSQRef = null;
				}

				factory.Saved -= Factory_Saved;
			}

			readonly ZGuid psqPK;
			WeakReference relatedPSQRef;
		}

		void PopulateNewPSQ(ZForm form)
		{
			ProfessionalServicesQuote newPSQ = form.BusinessEntity as ProfessionalServicesQuote;
			if (newPSQ != null)
			{
				newPSQ.InitializeFromOpportunity(CurrentDataItem);
			}
		}

		void PSQsGrid_DoubleClick(object sender, EventArgs e)
		{
			OpenRelatedItem();
		}

		void NewButton_Click(object sender, EventArgs e)
		{
			AddNewPSQ();
		}

		void AttachButton_Click(object sender, EventArgs e)
		{
			ShowRecordAttacher();
		}

		void DettachButton_Click(object sender, EventArgs e)
		{
			if (PSQsGrid.ListManager.Position > -1)
			{
				DialogResult dialogResult = Globals.Message.Show("Are you sure you want to detach the selected PSQ?", "Confirm Detach...", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				if (dialogResult == DialogResult.Yes)
				{
					ProfessionalServicesQuote psq = (ProfessionalServicesQuote)PSQsGrid.ListManager.GetCurrent();
					RelatedPSQs.Remove(psq);
				}
			}
		}

		void EditButton_Click(object sender, EventArgs e)
		{
			OpenRelatedItem();
		}

		void OpenRelatedItem()
		{
			if (PSQsGrid.ListManager.Position > -1)
			{
				var relatedItem = (ProfessionalServicesQuote)PSQsGrid.ListManager.GetCurrent();
				LastController = ZControllerFactory.Create(ClientControllerRegistration.ProfessionalServicesQuote);
				LastController.ShowEditForm(relatedItem);
			}
		}

		#region Implementation

		public new EDIOrgOpportunity CurrentDataItem
		{
			get { return (EDIOrgOpportunity)base.CurrentDataItem; }
		}

		ProfessionalServicesQuoteCollection RelatedPSQs
		{
			get { return CurrentDataItem.RelatedPSQs; }
		}

		protected void ShowRecordAttacher()
		{
			LastAttacher = new ZRecordAttacher(RelatedPSQs, new ProfessionalServicesQuoteCollection(CurrentDataItem.Factory), ClientModuleRegistration.ProfessionalServicesQuote);
			LastAttacher.Show((IZForm)FindForm());
		}

		internal ZRecordAttacher LastAttacher;

		internal ZController LastController;

		#endregion
	}
}
