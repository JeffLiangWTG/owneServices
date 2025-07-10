using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Modules;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class PSQuoteProcessTask : CRMProcessTask
	{
		public PSQuoteProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ControllerID ParentControllerID
		{
			get { return ClientControllerRegistration.ProfessionalServicesQuote; }
		}

		protected override Type ParentType
		{
			get { return typeof(ProfessionalServicesQuote); }
		}

		public new ProfessionalServicesQuote Parent
		{
			get { return (ProfessionalServicesQuote)base.Parent; }
		}
	}
}

