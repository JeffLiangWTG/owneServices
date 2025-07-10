using System;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class EDIJobInvoicingConsumerTypes : JobInvoicingConsumerTypes
	{
		public static readonly JobInvoicingConsumerType Incident = new IncidentConsumerType("INC", "Incident");
		public static readonly JobInvoicingConsumerType PSQuote = new ProfServicesQuoteConsumerType("PSQ", "Professional Services Quote");

		protected EDIJobInvoicingConsumerTypes()
			: base()
		{
			Add(Incident);
			Add(PSQuote);
		}

		public new static JobInvoicingConsumerTypes New()
		{
			return new EDIJobInvoicingConsumerTypes();
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}
	}

	public abstract class EDIJobInvoicingConsumerType : JobInvoicingConsumerType
	{
		protected EDIJobInvoicingConsumerType(string code, string description)
			: base(code, (NoResString)description)
		{
		}

		public override bool ValidateJobForMiscellaneousDepartment
		{
			get { return false; }
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.None; }
		}
	}

	#region Incident

	public class IncidentConsumerType : EDIJobInvoicingConsumerType
	{
		public IncidentConsumerType(string code, string description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID
		{
			get { return Modules.ClientControllerRegistration.SupportIncident; }
		}

		public override Type BizoType
		{
			get { return typeof(SupportIncident); }
		}
	}

	#endregion

	#region Prof Services Quote

	public class ProfServicesQuoteConsumerType : EDIJobInvoicingConsumerType
	{
		public ProfServicesQuoteConsumerType(string code, string description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID
		{
			get { return Modules.ClientControllerRegistration.ProfessionalServicesQuote; }
		}

		public override Type BizoType
		{
			get { return typeof(ProfessionalServicesQuote); }
		}
	}

	#endregion
}

