using CargoWise.Types;
using Enterprise.Client.EDI.Modules;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate
{
	public sealed class AssignedStaffNotificationEmailContentBuilder : IEDIEmailTemplate, IEDIEmailTemplateBuilder
	{
		public AssignedStaffNotificationEmailContentBuilder(SupportIncident dataSource)
		{
			this.dataSource = dataSource;
		}
		readonly SupportIncident dataSource;

		public ZString TemplateCode => SupportIncidentEmailTemplateConstants.Codes.AssignedStaffNotificationEmail;

		public ZString TemplateDescription => SupportIncidentEmailTemplateConstants.Descriptions.AssignedStaffNotificationEmail;

		string IncidentTypeDescription => IncidentConstants.GetIncidentTypeDescription(dataSource.IM_IncidentType);

		public ZString SubjectTemplate => $"(*IncidentTypeDescription*): (*IncidentNumber*) has been assigned to you";

		public ZString BodyTemplate
		{
			get
			{
				if (bodyTemplate.IsEmpty)
				{
					bodyTemplate = IncidentConstants.GetTextFromResource(SupportIncidentEmailExternalResources.AssignedStaffEmailNotificationLetterFilePath);
				}
				return bodyTemplate;
			}
		}
		ZString bodyTemplate;

		public ZString BuildBody()
		{
			IncidentTemplatedTextGenerator bodyTextGenarator = new IncidentTemplatedTextGenerator(dataSource);
			string footer = "";
			if (dataSource is SupportIncident incident)
			{
				footer = string.IsNullOrEmpty(incident.AssignmentComment) ? string.Empty : $"<BR /><BR />Comment:<BR />{incident.AssignmentComment}";
			}

			return bodyTextGenarator.GenerateTemplatedText(
					textTemplate: BodyTemplate,
					htmlLink: SupportIncidentEmailBodyGeneralControls.GetIncidentFormHyperlink(dataSource, ClientControllerRegistration.SupportIncident, dataSource.IM_IncidentNumber),
					textHeader: IncidentTypeDescription,
					textFooter: $"has been assigned to you by {GlbStaff.CurrentUser.GS_FullName}.{(string.IsNullOrEmpty(footer) ? string.Empty : footer)}");
		}

		public ZString BuildSubject()
		{
			return new SupportIncidentParser(dataSource.Factory).Parse(dataSource, this.SubjectTemplate);
		}

		public IEDIEmailTemplate GetIEDIEmailTemplate()
		{
			return this;
		}

		public bool IsEmpty => SubjectTemplate.IsEmpty && BodyTemplate.IsEmpty;
	}
}
