using System.Globalization;
using System.Net;
using CargoWise.Types;
using Enterprise.Client.EDI.Modules;

namespace Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate
{
	public sealed class ProductManagerCustomizeRequestEmailContentBuilder : IEDIEmailTemplate, IEDIEmailTemplateBuilder
	{
		public ProductManagerCustomizeRequestEmailContentBuilder(SupportIncident dataSource)
		{
			this.dataSource = dataSource;
		}
		readonly SupportIncident dataSource;

		public ZString TemplateCode => SupportIncidentEmailTemplateConstants.Codes.ProductManagerCustomizeRequestEmail;

		public ZString TemplateDescription => SupportIncidentEmailTemplateConstants.Descriptions.NewIncidentRaisedInternalEmail;

		public ZString SubjectTemplate => "A development estimate request raised by client {0}";

		public ZString BodyTemplate
		{
			get
			{
				return @"A development estimate request raised by client: {0}
<br /><br />Incident number  : <a href=""{1}"">{2}</a>
<br /><br />Incident summary : {3}
<br /><br />Incident details : {4}";
			}
		}

		public bool IsEmpty => dataSource == null;

		public ZString BuildBody()
		{
			return string.Format(CultureInfo.InvariantCulture, BodyTemplate,
				dataSource.ClientName,
				SupportIncidentEmailBodyGeneralControls.GetIncidentFormUrl(dataSource, ClientControllerRegistration.SupportIncident),
				WebUtility.HtmlEncode(((ProcessManagement.Business.IWorkItemRelatedItem)dataSource).Number.ToString()),
				WebUtility.HtmlEncode(dataSource.IM_Description),
				WebUtility.HtmlEncode(dataSource.DetailNoteText));
		}

		public ZString BuildSubject()
		{
			return string.Format(CultureInfo.InvariantCulture, SubjectTemplate, dataSource.ClientName);
		}

		public IEDIEmailTemplate GetIEDIEmailTemplate()
		{
			return this;
		}
	}
}
