
using CargoWise.EntityFramework;
using Enterprise.Client.JAS.Business.AWB;
using Enterprise.Client.JAS.Business.Invoicing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations
{
	public class JXCDomainValidationManager
	{
		public JXCDomainValidationManager(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		public void ManageJXCValidations(JXCExportValidationType validationType)
		{
			UnregisterJXCDomainValidations();

			switch (validationType)
			{
				case JXCExportValidationType.Air:
					RegisterAirMessageDomainValidations();
					break;

				case JXCExportValidationType.Ocean:
					RegisterOceanMessageDomainValidations();
					break;

				case JXCExportValidationType.Invoicing:
					RegisterInvoicingMessageDomainValidations();
					break;

				case JXCExportValidationType.ProfitShare:
					RegisterProfitShareMessageDomainValidations();
					break;
			}
		}

		void RegisterAirMessageDomainValidations()
		{
			MainDomainValidationGroup.RegisterValidationType<JASConsolExportAWBHeader, JXCConsolExportAWBHeaderValidation>();
			MainDomainValidationGroup.RegisterValidationType<JASShipmentExportAWBHeader, JXCShipmentExportAWBHeaderValidation>();
			MainDomainValidationGroup.RegisterValidationType<ExportAWBRateLine, JXCExportAWBRateLineValidation>();
			MainDomainValidationGroup.RegisterValidationType<ExportAWBOtherCharges, JXCExportAWBOtherChargesValidation>();
		}

		void RegisterOceanMessageDomainValidations()
		{
			MainDomainValidationGroup.RegisterValidationType<JASForwardingConsol, JXCForwardingSeaConsolValidation>();
			MainDomainValidationGroup.RegisterValidationType<JASForwardingShipment, JXCForwardingSeaShipmentValidation>();
			MainDomainValidationGroup.RegisterValidationType<Transport, JXCForwardingSeaConsolTransportValidation>();
			MainDomainValidationGroup.RegisterValidationType<ForwardingContainer, JXCForwardingSeaContainerValidation>();
			MainDomainValidationGroup.RegisterValidationType<JASForwardingPackLine, JXCForwardingSeaPackLineValidation>();
		}

		void RegisterInvoicingMessageDomainValidations()
		{
			MainDomainValidationGroup.RegisterValidationType<JASARInvoice, JXCInvoicingValidation>();
			MainDomainValidationGroup.RegisterValidationType<JASARCreditNote, JXCInvoicingValidation>();
			MainDomainValidationGroup.RegisterValidationType<JASARAdjustmentNote, JXCInvoicingValidation>();
		}

		void RegisterProfitShareMessageDomainValidations()
		{
			MainDomainValidationGroup.RegisterValidationType<JASForwardingConsol, JXCForwardingConsolProfitShareValidation>();
			MainDomainValidationGroup.RegisterValidationType<Transport, JXCForwardingConsolTransportProfitShareValidation>();
		}

		internal void UnregisterJXCDomainValidations()
		{
			MainDomainValidationGroup.UnregisterValidationType<JASConsolExportAWBHeader, JXCConsolExportAWBHeaderValidation>();
			MainDomainValidationGroup.UnregisterValidationType<JASShipmentExportAWBHeader, JXCShipmentExportAWBHeaderValidation>();
			MainDomainValidationGroup.UnregisterValidationType<ExportAWBRateLine, JXCExportAWBRateLineValidation>();
			MainDomainValidationGroup.UnregisterValidationType<ExportAWBOtherCharges, JXCExportAWBOtherChargesValidation>();

			MainDomainValidationGroup.UnregisterValidationType<JASForwardingConsol, JXCForwardingSeaConsolValidation>();
			MainDomainValidationGroup.UnregisterValidationType<JASForwardingShipment, JXCForwardingSeaShipmentValidation>();
			MainDomainValidationGroup.UnregisterValidationType<Transport, JXCForwardingSeaConsolTransportValidation>();
			MainDomainValidationGroup.UnregisterValidationType<ForwardingContainer, JXCForwardingSeaContainerValidation>();
			MainDomainValidationGroup.UnregisterValidationType<JASForwardingPackLine, JXCForwardingSeaPackLineValidation>();

			MainDomainValidationGroup.UnregisterValidationType<JASARInvoice, JXCInvoicingValidation>();
			MainDomainValidationGroup.UnregisterValidationType<JASARCreditNote, JXCInvoicingValidation>();
			MainDomainValidationGroup.UnregisterValidationType<JASARAdjustmentNote, JXCInvoicingValidation>();

			MainDomainValidationGroup.UnregisterValidationType<JASForwardingConsol, JXCForwardingConsolProfitShareValidation>();
			MainDomainValidationGroup.UnregisterValidationType<Transport, JXCForwardingConsolTransportProfitShareValidation>();
		}

		DomainValidationGroup MainDomainValidationGroup
		{
			get { return Factory.Validation.MainGroup; }
		}

		readonly BusinessObjectFactory Factory;
	}
}

