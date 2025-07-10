using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Dash.Business;
using Enterprise.Dash.Integration;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using SharedConstants = WTG.Shared.Dash.Common.Constants;

namespace Enterprise.Dash.ServiceTasks.MessageProcessors
{
	public class ValidationProcessor : DashMessageProcessor
	{
		readonly IValidator<DashCommercialInvoice> validator;

		public ValidationProcessor() : this(null, null, null, null, null)
		{
		}

		public ValidationProcessor(IFactory factory, ILogger serviceLogger, IShipamaxService shipamaxService, IDashErrorReporter dashErrorReporter, IValidator<DashCommercialInvoice> validator)
			 : base(factory, serviceLogger, shipamaxService, dashErrorReporter)
		{
			this.validator = validator;
		}

		protected override int BatchSize => 5;

		protected override byte MaxRetryCount => 5;

		protected override void AddServiceTaskSpecificQueryFilters(ZQuery query)
			=> query.AddToFilter(EDIMessageSchema.EM_MessageType, SharedConstants.DataProcessingType.Code.Validation);

		protected override void ProcessMessageCore(IFactory factory, DashDocument dashDocument, DashDocumentDataMessage dashDocumentDataMessage, CancellationToken token)
		{
			if (dashDocument.DDD_ParseType == SharedConstants.ParseType.Code.CommercialInvoice)
			{
				var errorMessage = validator.Validate(dashDocument.DashCommercialInvoice);

				if (!string.IsNullOrWhiteSpace(errorMessage))
				{
					dashDocument.DDD_ValidationStatus = SharedConstants.ValidationStatus.Code.Error;
					dashDocument.DDD_ValidationMessage = errorMessage;
				}
				else
				{
					dashDocument.DDD_ValidationStatus = SharedConstants.ValidationStatus.Code.Passed;
				}
			}
		}
	}
}
