using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class HouseBillDocumentDescriptorTest : TestCaseWithFactory
	{
		#region TestProperties

		public void TestProperties()
		{
			var descriptor = CreateDescriptor();

			CombineAssertions(() =>
			{
				AssertEquals(nameof(descriptor.Name), "ORIGINAL", descriptor.Name);
				AssertEquals(nameof(descriptor.DataContext), Constants.HouseBillTemplateDataContext, descriptor.DataContext);
				AssertEquals(nameof(descriptor.EnableTranslation), false, descriptor.EnableTranslation);
				AssertEquals(nameof(descriptor.DocumentType), "HBL", descriptor.DocumentType);
				AssertEquals(nameof(descriptor.MenuName), "Bill Of Lading", descriptor.MenuName);
				AssertEquals(nameof(descriptor.IsSystemDefined), true, descriptor.IsSystemDefined);
			});
		}

		#endregion

		#region TestPrintInstructions

		public void TestPrintInstructions()
		{
			var descriptor = CreateDescriptor();

			CombineAssertions(() =>
			{
				AssertNotNull("PrintInstructions", descriptor.PrintInstructions);
				AssertEquals("PrintInstructions.Title", "ORIGINAL", descriptor.PrintInstructions.Title);
				AssertContainsExactElementsInAnyOrder("PrintInstructions.DeliveryModes",
					new[]
					{
						nameof(PrintCopyType.PRN)
					},
					descriptor.PrintInstructions.DeliveryModes);
				AssertEquals("PrintInstructions.NumberOfCopies", 2, descriptor.PrintInstructions.GetNumberOfCopies(nameof(PrintCopyType.PRN)));
			});
		}

		#endregion

		#region TestMessageInstructions

		public void TestMessageInstructions()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			((BusinessObject)shipment)["IsEditingElectronicBOL"] = false;
			var descriptor = CreateDescriptor(shipment);

			CombineAssertions(() =>
			{
				AssertNotNull("MessageInstructions", descriptor.MessageInstructions);
				AssertEquals("DocumentName", "Bill Of Lading", descriptor.MessageInstructions.DocumentName);
				AssertEquals("DataContext", Constants.HouseBillTemplateDataContext, descriptor.MessageInstructions.DataContext);
				AssertNullOrEmpty("Recipient", descriptor.MessageInstructions.Recipient);
				AssertNullOrEmpty("EHubClientID", descriptor.MessageInstructions.EHubClientID);
				AssertNullOrEmpty("DirectXTClientID", descriptor.MessageInstructions.DirectXTClientID);
				AssertNullOrEmpty("XmlNamespace", descriptor.MessageInstructions.XmlNamespace);
				AssertEquals("AllowSendMessage", false, descriptor.MessageInstructions.AllowSendMessage);
				AssertEquals("AllowSendMessageWithdrawal", false, descriptor.MessageInstructions.AllowSendMessageWithdrawal);
				AssertEquals("AllowResetToOriginal", false, descriptor.MessageInstructions.AllowResetToOriginal);
				AssertEquals("OrderLogsByLocalTime", false, descriptor.MessageInstructions.OrderLogsByLocalTime);
				AssertEquals("MessageInstructions.AmendmentOptions.Count", 0, descriptor.MessageInstructions.AmendmentOptions.Count);
				AssertEquals("MessageInstructions.WidthdrawalOptions.Count", 0, descriptor.MessageInstructions.WidthdrawalOptions.Count);
			});
		}

		public void TestMessageInstructions_IsEditingElectronicBOL()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			((BusinessObject)shipment)["IsEditingElectronicBOL"] = true;
			var descriptor = CreateDescriptor(shipment);

			CombineAssertions(() =>
			{
				AssertNotNull("MessageInstructions", descriptor.MessageInstructions);
				AssertEquals("DocumentName", "Electronic House Bill", descriptor.MessageInstructions.DocumentName);
				AssertEquals("DataContext", Constants.HouseBillTemplateDataContext, descriptor.MessageInstructions.DataContext);
				AssertEquals("Recipient", "Title Registry", descriptor.MessageInstructions.Recipient);
				AssertNullOrEmpty("EHubClientID", descriptor.MessageInstructions.EHubClientID);
				AssertEquals("DirectXTClientID", "ELECTRONIC_BILL_OF_LADING", descriptor.MessageInstructions.DirectXTClientID);
				AssertEquals("XmlNamespace", "/eHBL/1", descriptor.MessageInstructions.XmlNamespace);
				AssertEquals("AllowSendMessage", true, descriptor.MessageInstructions.AllowSendMessage);
				AssertEquals("AllowSendMessageWithdrawal", false, descriptor.MessageInstructions.AllowSendMessageWithdrawal);
				AssertEquals("AllowResetToOriginal", false, descriptor.MessageInstructions.AllowResetToOriginal);
				AssertEquals("OrderLogsByLocalTime", false, descriptor.MessageInstructions.OrderLogsByLocalTime);
				AssertEquals("MessageInstructions.AmendmentOptions.Count", 0, descriptor.MessageInstructions.AmendmentOptions.Count);
				AssertEquals("MessageInstructions.WidthdrawalOptions.Count", 0, descriptor.MessageInstructions.WidthdrawalOptions.Count);
			});
		}

		#endregion

		#region TestEDocsInstructions

		public void TestEDocsInstructions()
		{
			var descriptor = CreateDescriptor();

			CombineAssertions(() =>
			{
				AssertNotNull("EDocsInstructions", descriptor.EDocsInstructions);
				AssertEquals("EDocsInstructions.SaveCopyToEDocs", true, descriptor.EDocsInstructions.SaveCopyToEDocs);
				AssertEquals("EDocsInstructions.Parent", true, descriptor.EDocsInstructions.Parent is Forwarding.IForwardingShipment);
			});
		}

		#endregion

		#region TestDisplayInstructions

		public void TestDisplayInstructions()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			((BusinessObject)shipment)["IsEditingElectronicBOL"] = false;
			var descriptor = CreateDescriptor(shipment);

			CombineAssertions(() =>
			{
				AssertNotNull("DisplayInstructions", descriptor.DisplayInstructions);
				AssertEquals("DisplayInstructions.ShowEvents", false, descriptor.DisplayInstructions.ShowEvents);
				AssertEquals("DisplayInstructions.ShowLastEventDetails", false, descriptor.DisplayInstructions.ShowLastEventDetails);
				AssertEquals("DisplayInstructions.MenuItems", false, descriptor.DisplayInstructions.MenuItems.Any());
			});
		}

		public void TestDisplayInstructions_AllowSendMessage()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			((BusinessObject)shipment)["IsEditingElectronicBOL"] = true;
			var descriptor = CreateDescriptor(shipment);

			CombineAssertions(() =>
			{
				AssertNotNull("DisplayInstructions", descriptor.DisplayInstructions);
				AssertEquals("DisplayInstructions.ShowEvents", true, descriptor.DisplayInstructions.ShowEvents);
				AssertEquals("DisplayInstructions.ShowLastEventDetails", true, descriptor.DisplayInstructions.ShowLastEventDetails);
				AssertEquals("DisplayInstructions.MenuItems", false, descriptor.DisplayInstructions.MenuItems.Any());
			});
		}

		#endregion

		#region Implementation

		IDocumentDescriptor CreateDescriptor(Forwarding.IForwardingShipment shipment = null)
		{
			shipment = shipment ?? Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_NoOriginalBills = 2;
			shipment.JS_NoCopyBills = 3;

			var pivot = Factory.New<VisualizerMenuTemplatePivot>();

			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "Bill Of Lading";
			menuItem.SU_IsSystemDefined = true;

			var template = new DocumentVisualizerTestHelper().CreateTemplate(Factory,
@"!BillOfLading
#FirstPage
	<Macro>
#End
#TermsAndConditionsPage
	<Macro>
#End
#FollowOnPage
	<Macro>
#End
#Charges
	<Macro>
#End
#FollowOnContent
	<Macro>
#End");

			var query = new ZQuery();
			query.AddToFilter(RefDocTypeSchema.RT_DocType, "HBL");

			var hbl = Factory.LoadTop1<RefDocType>(query);

			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = template.PK;
			pivot.SI_DocumentTitle = "ORIGINAL";
			pivot.SI_RT_DocType = hbl.PK;
			pivot.SI_DataStoreName = "7-11";
			pivot.SI_PrintCopyType = "PRN";
			pivot.SI_IsSystemDefined = true;

			var documentPivot = DocumentPivot.Create(new[] { pivot }).Single();

			var supporter = shipment.GetSupporter();

			var parameters = new DocDataObjectParameters(pivot.SI_DocumentTitle, pivot.SI_DataStoreName);

			var dataObjectRes = supporter.GetDocDataObject(shipment, Constants.HouseBillTemplateDataContext, parameters);

			if (dataObjectRes.IsLeft)
			{
				Fail(dataObjectRes.Left);
			}

			var bizObj = shipment as BusinessObject;

			return new HouseBillDocumentDescriptor(
				bizObj,
				documentPivot,
				bizObj.LoadOrCreateDocumentData(pivot.SI_DataStoreName),
				new HouseBillTemplate(template.GetFlexCelWorksheet()),
				new ServiceContainer(),
				System.Array.Empty<ICommand>());
		}

		#endregion
	}
}
