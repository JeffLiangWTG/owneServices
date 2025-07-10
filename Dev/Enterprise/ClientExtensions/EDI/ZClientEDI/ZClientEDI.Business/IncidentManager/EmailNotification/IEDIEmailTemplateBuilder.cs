using CargoWise.Types;

namespace Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate
{
	public interface IEDIEmailTemplateBuilder
	{
		ZString BuildSubject();

		ZString BuildBody();

		IEDIEmailTemplate GetIEDIEmailTemplate();

		bool IsEmpty {  get; }
	}
}
