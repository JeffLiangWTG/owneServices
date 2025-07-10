using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Dash.Business;
using Enterprise.Dash.Business.Extensions;
using Enterprise.Dash.Integration.Services;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Dash.ServiceTasks.MessageProcessors
{
	public class OrganizationMatchingMessageProcessor : DashMessageProcessor
	{
		readonly IDashGlowService dashGlowService;

		public OrganizationMatchingMessageProcessor(
			IFactory factory,
			ILogger serviceLogger,
			IDashGlowService dashGlowService,
			IShipamaxService shipamaxService,
			IDashErrorReporter dashErrorReporter)
			: base(factory, serviceLogger, shipamaxService, dashErrorReporter)
		{
			this.dashGlowService = dashGlowService;
		}

		protected override int BatchSize => 1;

		protected override byte MaxRetryCount => 5;

		protected override void AddServiceTaskSpecificQueryFilters(ZQuery query)
			=> query.AddToFilter(EDIMessageSchema.EM_MessageType, WTG.Shared.Dash.Common.Constants.DataProcessingType.Code.OrganizationMatching);

		protected override void ProcessMessageCore(IFactory factory, DashDocument dashDocument, DashDocumentDataMessage dashDocumentDataMessage, CancellationToken token)
		{
			var response = dashGlowService.MatchOrganisations(dashDocument.PK.ToGuid());

			if (!response.IsSuccessStatusCode)
			{
				var responseContent = response.Content.ReadAsString();
				ServiceLogger.Error($"Endpoint unable to process DashDocumentDataMessage record '{dashDocumentDataMessage.PK}': {response.StatusCode}: {responseContent}");
			}
		}
	}
}
