using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class FreightWrapperFromProfessionalServicesQuote : FreightWrapperEDI<ProfessionalServicesQuote>
	{
		public FreightWrapperFromProfessionalServicesQuote(ProfessionalServicesQuote professionalServicesQuote, BusinessObjectFactory factory)
			: base(professionalServicesQuote, factory)
		{ }
	}
}
