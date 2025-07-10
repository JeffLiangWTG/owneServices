using CargoWise.Types;

namespace Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate
{
	public interface IEDIEmailTemplate
	{
		ZString TemplateCode { get; }

		ZString TemplateDescription { get; }

		ZString SubjectTemplate { get; }

		ZString BodyTemplate { get; }

		bool IsEmpty { get; }
	}
}
