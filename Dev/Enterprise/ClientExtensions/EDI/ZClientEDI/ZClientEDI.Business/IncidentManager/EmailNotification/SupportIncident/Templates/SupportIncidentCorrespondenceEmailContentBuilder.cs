using System.Globalization;
using System.Net;
using CargoWise.Types;

namespace Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate
{
	public sealed class SupportIncidentCorrespondenceEmailContentBuilder : IEDIEmailTemplate, IEDIEmailTemplateBuilder
	{
		public SupportIncidentCorrespondenceEmailContentBuilder(SupportIncident dataSource)
		{
			this.dataSource = dataSource;
		}
		readonly SupportIncident dataSource;

		public ZString TemplateCode => SupportIncidentEmailTemplateConstants.Codes.SupportIncidentCorrespondenceEmail;

		public ZString TemplateDescription => SupportIncidentEmailTemplateConstants.Descriptions.SupportIncidentCorrespondenceEmail;

		public ZString SubjectTemplate => SupportIncidentEmailBodyGeneralControls.GetCommonEmailSubject("Update");

		public ZString BodyTemplate
		{
			get
			{
				return
@"This email is regarding Customer Service Incident {0}.
		            
(Enter correspondence details here)
		            
<b> Incident Details </b>
<i>{1}</i>
		            
If any details are incorrect or there are any concerns please let us know by directly replying to this email.";
			}
		}

		public ZString BuildBody()
		{
			return string.Format(CultureInfo.InvariantCulture, string.Format(CultureInfo.InvariantCulture, BodyTemplate, dataSource.Number, WebUtility.HtmlEncode(dataSource.DetailNoteText)));
		}

		public ZString BuildSubject()
		{
			return new SupportIncidentParser(dataSource.Factory).Parse(dataSource, SubjectTemplate);
		}

		public IEDIEmailTemplate GetIEDIEmailTemplate()
		{
			return this;
		}

		public bool IsEmpty => SubjectTemplate.IsEmpty && BodyTemplate.IsEmpty;
	}
}
