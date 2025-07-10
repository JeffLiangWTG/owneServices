using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.JAS.Business.AWB;
using Enterprise.Client.JAS.Business.Invoicing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations.Testing
{
	internal class JXCDomainValidationManagerTest : TestCaseWithFactory
	{
		#region TestManageJXCValidations_None
		public void TestManageJXCValidations_None()
		{
			AssertDomainValidationContainerIsNotLoaded();
			ManageJXCValidations_None(Factory);
			AssertManageJXCValidations_None();
		}

		void AssertManageJXCValidations_None()
		{
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASConsolExportAWBHeader)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASShipmentExportAWBHeader)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(ExportAWBRateLine)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(ExportAWBOtherCharges)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASForwardingConsol)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASForwardingShipment)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(Transport)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(ForwardingContainer)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASForwardingPackLine)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASARInvoice)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASARCreditNote)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASARAdjustmentNote)));
		}

		#endregion
		#region TestManageJXCValidations_Air
		public void TestManageJXCValidations_Air()
		{
			AssertDomainValidationContainerIsNotLoaded();
			DomainValidationManager.ManageJXCValidations(JXCExportValidationType.Air);
			AssertManageJXCValidations_Air();
		}

		void AssertManageJXCValidations_Air()
		{
			Assert("Should be registered", MainDomainValidationGroup.ContainsDomainValidation(typeof(JASConsolExportAWBHeader), typeof(JXCConsolExportAWBHeaderValidation)));
			Assert("Should be registered", MainDomainValidationGroup.ContainsDomainValidation(typeof(JASShipmentExportAWBHeader), typeof(JXCShipmentExportAWBHeaderValidation)));
			Assert("Should be registered", MainDomainValidationGroup.ContainsDomainValidation(typeof(ExportAWBRateLine), typeof(JXCExportAWBRateLineValidation)));
			Assert("Should be registered", MainDomainValidationGroup.ContainsDomainValidation(typeof(ExportAWBOtherCharges), typeof(JXCExportAWBOtherChargesValidation)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASForwardingConsol)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASForwardingShipment)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(Transport)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(ForwardingContainer)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASForwardingPackLine)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASARInvoice)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASARCreditNote)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASARAdjustmentNote)));
		}

		#endregion
		#region TestManageJXCValidations_Ocean
		public void TestManageJXCValidations_Ocean()
		{
			AssertDomainValidationContainerIsNotLoaded();
			DomainValidationManager.ManageJXCValidations(JXCExportValidationType.Ocean);
			AssertManageJXCValidations_Ocean();
		}

		void AssertManageJXCValidations_Ocean()
		{
			Assert("Should be registered", MainDomainValidationGroup.ContainsDomainValidation(typeof(JASForwardingConsol), typeof(JXCForwardingSeaConsolValidation)));
			Assert("Should be registered", MainDomainValidationGroup.ContainsDomainValidation(typeof(JASForwardingShipment), typeof(JXCForwardingSeaShipmentValidation)));
			Assert("Should be registered", MainDomainValidationGroup.ContainsDomainValidation(typeof(Transport), typeof(JXCForwardingSeaConsolTransportValidation)));
			Assert("Should be registered", MainDomainValidationGroup.ContainsDomainValidation(typeof(ForwardingContainer), typeof(JXCForwardingSeaContainerValidation)));
			Assert("Should be registered", MainDomainValidationGroup.ContainsDomainValidation(typeof(JASForwardingPackLine), typeof(JXCForwardingSeaPackLineValidation)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASConsolExportAWBHeader)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASShipmentExportAWBHeader)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(ExportAWBRateLine)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(ExportAWBOtherCharges)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASARInvoice)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASARCreditNote)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASARAdjustmentNote)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASForwardingConsol), typeof(JXCForwardingConsolProfitShareValidation)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASForwardingConsol), typeof(JXCForwardingConsolProfitShareValidation)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASForwardingConsol), typeof(JXCForwardingConsolProfitShareValidation)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(Transport), typeof(JXCForwardingConsolTransportProfitShareValidation)));
		}

		#endregion
		#region TestManageJXCValidations_Invoicing
		public void TestManageJXCValidations_Invoicing()
		{
			AssertDomainValidationContainerIsNotLoaded();
			DomainValidationManager.ManageJXCValidations(JXCExportValidationType.Invoicing);
			AssertManageJXCValidations_Invoicing();
		}

		void AssertManageJXCValidations_Invoicing()
		{
			Assert("Should be registered", MainDomainValidationGroup.ContainsDomainValidation(typeof(JASARInvoice), typeof(JXCInvoicingValidation)));
			Assert("Should be registered", MainDomainValidationGroup.ContainsDomainValidation(typeof(JASARCreditNote), typeof(JXCInvoicingValidation)));
			Assert("Should be registered", MainDomainValidationGroup.ContainsDomainValidation(typeof(JASARAdjustmentNote), typeof(JXCInvoicingValidation)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASConsolExportAWBHeader)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASShipmentExportAWBHeader)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(ExportAWBRateLine)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(ExportAWBOtherCharges)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASForwardingConsol)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASForwardingShipment)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(Transport)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(ForwardingContainer)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASForwardingPackLine)));
		}

		#endregion
		#region TestManageJXCValidations_ProfitShare
		public void TestManageJXCValidations_ProfitShare()
		{
			AssertDomainValidationContainerIsNotLoaded();
			DomainValidationManager.ManageJXCValidations(JXCExportValidationType.ProfitShare);
			AssertManageJXCValidations_ProfitShare();
		}

		void AssertManageJXCValidations_ProfitShare()
		{
			Assert("Should be registered", MainDomainValidationGroup.ContainsDomainValidation(typeof(JASForwardingConsol), typeof(JXCForwardingConsolProfitShareValidation)));
			Assert("Should be registered", MainDomainValidationGroup.ContainsDomainValidation(typeof(Transport), typeof(JXCForwardingConsolTransportProfitShareValidation)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASConsolExportAWBHeader)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASShipmentExportAWBHeader)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(ExportAWBRateLine)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(ExportAWBOtherCharges)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASForwardingShipment)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASForwardingConsol), typeof(JXCForwardingSeaConsolValidation)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(Transport), typeof(JXCForwardingSeaConsolTransportValidation)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(ForwardingContainer)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASForwardingPackLine)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASARInvoice)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASARCreditNote)));
			Assert("Should not be registered", !MainDomainValidationGroup.ContainsDomainValidation(typeof(JASARAdjustmentNote)));
		}

		#endregion
		public void TestManageJXCValidations_ChangingJXCExportValidationType()
		{
			AssertDomainValidationContainerIsNotLoaded();
			DomainValidationManager.ManageJXCValidations(JXCExportValidationType.Air);
			AssertManageJXCValidations_Air();
			DomainValidationManager.ManageJXCValidations(JXCExportValidationType.Ocean);
			AssertManageJXCValidations_Ocean();
			DomainValidationManager.ManageJXCValidations(JXCExportValidationType.Invoicing);
			AssertManageJXCValidations_Invoicing();
			DomainValidationManager.ManageJXCValidations(JXCExportValidationType.ProfitShare);
			AssertManageJXCValidations_ProfitShare();
			ManageJXCValidations_None(Factory);
			AssertManageJXCValidations_None();
		}

		internal void ManageJXCValidations_None(BusinessObjectFactory factory)
		{
			JXCDomainValidationManager testManager = new JXCDomainValidationManager(factory);
			testManager.UnregisterJXCDomainValidations();

			factory.Validation.MainGroup.RegisterValidationType<DummyBusinessObject, DummyBusinessObjectDomainValidationForTest>();
			factory.Validation.MainGroup.RegisterValidationType<DummyChildBusinessObject, DummyChildBusinessObjectDomainValidationForTest>();
		}

		#region Implementation
		void AssertDomainValidationContainerIsNotLoaded()
		{
			Assert("Pre-condition; Should return false if no domain validation is registered and DomainValidationContainer is not accessed", !Factory.HasDomainValidation);
		}

		JXCDomainValidationManager DomainValidationManager
		{
			get
			{
				if (fDomainValidationManager == null)
				{
					fDomainValidationManager = new JXCDomainValidationManager(Factory);
				}

				return fDomainValidationManager;
			}
		}

		DomainValidationGroup MainDomainValidationGroup
		{
			get
			{
				return Factory.Validation.MainGroup;
			}
		}

		JXCDomainValidationManager fDomainValidationManager;
		#endregion
	}
}
