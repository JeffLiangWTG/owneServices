using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class ProfessionalServicesQuoteController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return Modules.ClientModuleRegistration.ProfessionalServicesQuote; }
		}

		public override ControllerID ID
		{
			get { return Modules.ClientControllerRegistration.ProfessionalServicesQuote; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ProfessionalServicesQuote); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ProfessionalServicesQuoteForm((ProfessionalServicesQuote)businessEntity);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var bizo = (ProfessionalServicesQuote)base.GetNewBusinessEntityInLocalFactory();
			bizo.SetTeamFromLastQuoteAddedByCurrentUser();
			return bizo;
		}

		#region Security Check Point

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return EDISecurityCheckpoints.ProfessionalServicesQuoteNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return EDISecurityCheckpoints.ProfessionalServicesQuote; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return EDISecurityCheckpoints.ProfessionalServicesQuoteEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return EDISecurityCheckpoints.ProfessionalServicesQuoteDelete; }
		}

		#endregion
	}
}
