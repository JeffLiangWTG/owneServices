using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Client.UPE.Business.Testing
{
	class UPEAlternateBrokerSplitNotificationAutoDeliveryTest : UPEDocumentAutoDeliveryTest
	{
		protected override UPEDocumentAutoDelivery NewDocumentAutoDelivery()
		{
			return new UPEAlternateBrokerSplitNotificationAutoDelivery((UPECusHAWB)DocumentSupportable);
		}

		protected override IUPEDocumentSupportable NewDocumentSupportable()
		{
			UPECusHAWB result = Factory.NewWithValidTestData<UPECusHAWB>();
			result.CS_JE_CustomsFormalEntry = Factory.NewWithValidTestData(typeof(JobDeclaration)).PK;
			result.Declaration.JE_OH_Importer = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			result.Declaration.Importer.SetRelatedParty(DeliveryOrganisation, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);
			return result;
		}

		protected override DocumentCommand ExpectedDocumentCommand
		{
			get
			{
				return DocumentLoader.LoadAlternateBrokerSplitNotification();
			}
		}

		protected override ZString ExpectedPrintBatchType
		{
			get
			{
				return UPEPrintBatchTypes.Codes.AlternateBrokerSplitNotification;
			}
		}

		protected override ZString ExpectedDeliveryFailureEmailSubject
		{
			get
			{
				return "Delivery instructions incomplete for Alternate Broker Split Shipment Notification - " + DeliveryOrganisation.OH_Code;
			}
		}

		protected override ZString ExpectedDeliveryFailureEmailBody
		{
			get
			{
				return @"
Delivery instructions incomplete for Alternate Broker Split Notification; Generated 11-Nov-05 00:00:00

CargoWise One Code : DLVORG

Error: DeliveryAddress: Please enter a Fax Number.
";
			}
		}
	}
}
