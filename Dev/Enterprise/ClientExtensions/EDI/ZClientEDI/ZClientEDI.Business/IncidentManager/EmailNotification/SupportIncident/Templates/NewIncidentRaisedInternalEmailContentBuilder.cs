using System.Globalization;
using System.Net;
using CargoWise.Types;
using Enterprise.Client.EDI.Modules;
using Enterprise.ProcessManagement.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate
{
	public sealed class NewIncidentRaisedInternalEmailContentBuilder : IEDIEmailTemplate, IEDIEmailTemplateBuilder
	{
		public NewIncidentRaisedInternalEmailContentBuilder(SupportIncident dataSource, string reportedByUserName)
		{
			this.dataSource = dataSource;
			this.reportedByUserName = reportedByUserName;
		}
		readonly SupportIncident dataSource;
		readonly string reportedByUserName;

		public ZString TemplateCode => SupportIncidentEmailTemplateConstants.Codes.NewIncidentRaisedInternalEmail;

		public ZString TemplateDescription => SupportIncidentEmailTemplateConstants.Descriptions.NewIncidentRaisedInternalEmail;

		public ZString SubjectTemplate => "[{0}] A {1} incident raised by client {2}";

		public ZString BodyTemplate
		{
			get
			{
				return @"{0}
<p><p>Product : {1}
<p><p>Incident number  : <a href=""{2}"">{3}</a>
<p><p>Incident summary : {4}
<p><p>Incident details : {5}";
			}
		}

		public bool IsEmpty => dataSource == null;

		public ZString BuildBody()
		{
			return string.Format(CultureInfo.InvariantCulture, BodyTemplate,
				WebUtility.HtmlEncode($"{reportedByUserName} raised a {dataSource.IM_Priority} incident ({dataSource.IM_Product}) for client: {dataSource.ClientName}"),
				WebUtility.HtmlEncode(dataSource.ProductDescription),
				SupportIncidentEmailBodyGeneralControls.GetIncidentFormUrl(dataSource, ClientControllerRegistration.SupportIncident),
				WebUtility.HtmlEncode(((IWorkItemRelatedItem)dataSource).Number.ToString()),
				WebUtility.HtmlEncode(((IWorkItemRelatedItem)dataSource).ItemDescription),
				WebUtility.HtmlEncode(dataSource.DetailNoteText));
		}

		public ZString BuildSubject()
		{
			return string.Format(CultureInfo.InvariantCulture, SubjectTemplate,
				dataSource.IM_Product,
				dataSource.IM_Priority,
				dataSource.ClientName);
		}

		public IEDIEmailTemplate GetIEDIEmailTemplate()
		{
			return this;
		}
	}
}
