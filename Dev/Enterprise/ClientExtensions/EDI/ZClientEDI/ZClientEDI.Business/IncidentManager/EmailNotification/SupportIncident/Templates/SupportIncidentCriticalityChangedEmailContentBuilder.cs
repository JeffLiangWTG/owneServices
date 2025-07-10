using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate
{
	public sealed class SupportIncidentCriticalityChangedEmailContentBuilder : BaseEDIRegistryCodeDescriptionEmailTemplate
	{
		public SupportIncidentCriticalityChangedEmailContentBuilder(SupportIncident dataSource, bool eRequestV2, string product, string registryTemplateCode = "", bool needUpgrade = false) : base(eRequestV2, product, registryTemplateCode, needUpgrade)
		{
			this.dataSource = dataSource;
		}
		readonly SupportIncident dataSource;

		protected override CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItem RegistryItem => EDIDataRegistry.Instance.IncidentCriticalityChangedRaw;

		public override ZString TemplateCode => SupportIncidentEmailTemplateConstants.Codes.CriticalityChangedEmail;

		public override ZString TemplateDescription => SupportIncidentEmailTemplateConstants.Descriptions.CriticalityChangedEmail;

		public override ZString BuildBody()
		{
			var bodyBuilder = new ZStringBuilder();
			bodyBuilder.Append($"This is an update to {SupportIncidentEmailBodyGeneralControls.GetIncidentGlowHyperlink(dataSource)}<br/>");
			bodyBuilder.Append($"{new SupportIncidentHtmlParser(dataSource.Factory).Parse(dataSource, BodyTemplate)}<br/>");
			return bodyBuilder.ToString();
		}

		public override ZString BuildSubject()
		{
			return new SupportIncidentParser(dataSource.Factory).Parse(dataSource, SubjectTemplate);
		}
	}
}
