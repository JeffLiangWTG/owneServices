using System;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	sealed class ParseDocumentMenuItemTest : eDocMenuItemTestCase
	{
		protected override void RunAssertionsForGetEnabledStatus(eDocMenuItem menuItem, StorageDocsBase bizO)
		{
			if (bizO.ParentMain == null)
			{
				// Don't test for unallocated eDocs.
				return;
			}

			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var newMessage = MasterFactory.New<EDocsShipamaxMessage>();
				newMessage.EM_LinkUniqueID = bizO.PK;
				newMessage.EM_LinkTable = AutoStorageDocs.Schema.TableName;
				newMessage.EM_ApplicationReference = bizO.SC_SM.ToString();
				bizO.ParentMain.SM_ParentFK = ZGuid.NewZGuid();
				bizO.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
				MasterFactory.Save();
				var textOriginal = "&Parse";

				// Case 1: Document does not have a parse type
				bizO.SC_DocType = ZString.Empty;
				AssertEquals(false, menuItem.GetEnabledStatus(bizO, false));
				AssertEquals(textOriginal, menuItem.Caption);

				// Case 2: Document has a parse type, but message status is Unparsed
				bizO.SC_DocType = "CIV";
				bizO.SC_DataType = "PDF";
				MasterFactory.Save();
				var textReparse = "&Re-parse as Comercial Invoice";
				var textParse = "&Parse as Comercial Invoice";
				bizO.ActiveShipamaxMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
				AssertEquals(false, menuItem.GetEnabledStatus(bizO, false));
				AssertEquals(textOriginal, menuItem.Caption);

				// Case 3: Document has a parse type, but message status is Failed
				bizO.ActiveShipamaxMessage.EM_Status = EDIMessageStatusList.Codes.Failed;
				AssertEquals(false, menuItem.GetEnabledStatus(bizO, false));
				AssertEquals(textOriginal, menuItem.Caption);

				// Case 4: Document has a parse type, but message status is Processing
				bizO.ActiveShipamaxMessage.EM_Status = EDIMessageStatusList.Codes.Sent;
				AssertEquals(true, menuItem.GetEnabledStatus(bizO, false));
				AssertEquals(textReparse, menuItem.Caption);

				// Case 5: Document has a parse type, but message status is Complete
				bizO.ActiveShipamaxMessage.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
				AssertEquals(true, menuItem.GetEnabledStatus(bizO, false));
				AssertEquals(textReparse, menuItem.Caption);

				// Case 6: Document has a parse type, but message status is NeedReview
				bizO.ActiveShipamaxMessage.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;
				AssertEquals(true, menuItem.GetEnabledStatus(bizO, false));
				AssertEquals(textReparse, menuItem.Caption);

				// Case 7: Document has a parse type, but message status is Error
				bizO.ActiveShipamaxMessage.EM_Status = EDIMessageStatusList.Codes.Error;
				AssertEquals(true, menuItem.GetEnabledStatus(bizO, false));
				AssertEquals(textParse, menuItem.Caption);

				// Case 8: Document has a parse type, but message status is Cancelled
				bizO.ActiveShipamaxMessage.EM_Status = EDIMessageStatusList.Codes.Cancelled;
				AssertEquals(true, menuItem.GetEnabledStatus(bizO, false));
				AssertEquals(textParse, menuItem.Caption);
			}
		}

		protected override eDocMenuItem GetMenuItem()
		{
			return new ParseDocumentMenuItem();
		}
	}
}
