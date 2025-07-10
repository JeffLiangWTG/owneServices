using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class HouseBillPrintInstructionsTest : TestCaseWithFactory
	{
		#region TestAttachmentFilename

		public void TestAttachmentFilename()
		{
			var pivot = Factory.NewWithValidTestData<StmMenuTemplatePivotBase>();

			var menu = Factory.NewWithValidTestData<StmMenuItemBase>();
			pivot.SI_SU = menu.PK;

			var documentPivot = DocumentPivot.Create(new[] { pivot }).Single();

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			shipment.JS_ShipmentType = "STD";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";

			shipment.JS_ReleaseType = "EBL";
			shipment.JS_HouseBillOfLadingType = "IAU";

			var parameters = new Dictionary<string, string>
			{
				["AttachmentFilename"] = "attachment file name for test"
			};

			var instructions = new HouseBillPrintInstructions(documentPivot, parameters, shipment);

			AssertEquals(nameof(instructions.AttachmentFilename), "attachment file name for test", instructions.AttachmentFilename);
		}

		#endregion

		#region TestDocumentName

		public void TestDocumentName()
		{
			var pivot = Factory.NewWithValidTestData<StmMenuTemplatePivotBase>();

			var menu = Factory.NewWithValidTestData<StmMenuItemBase>();
			pivot.SI_SU = menu.PK;

			var documentPivot = DocumentPivot.Create(new[] { pivot }).Single();

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			shipment.JS_ShipmentType = "STD";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";

			shipment.JS_ReleaseType = "EBL";
			shipment.JS_HouseBillOfLadingType = "IAU";

			var parameters = new Dictionary<string, string>
			{
				["DocumentName"] = "document name for test"
			};

			var instructions = new HouseBillPrintInstructions(documentPivot, parameters, shipment);

			AssertEquals(nameof(instructions.DocumentName), "document name for test", instructions.DocumentName);
		}

		#endregion

		#region TestGetDeliveryTitle

		public void TestGetDeliveryTitle_Original()
		{
			var menu = Factory.NewWithValidTestData<StmMenuItemBase>();
			menu.SU_MenuName = "Bill Of Lading";

			var template = Factory.NewWithValidTestData<StmTemplateBase>();

			var pivot = Factory.NewWithValidTestData<StmMenuTemplatePivotBase>();
			pivot.SI_DocumentTitle = "Original";
			pivot.SI_PrintCopyType = nameof(PrintCopyType.PRN);
			pivot.SI_SU = menu.PK;
			pivot.SI_SO = template.PK;

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_ReleaseType = ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_HouseBillOfLadingType = "IAU";
			shipment.JS_NoOriginalBills = 2;
			shipment.JS_NoCopyBills = 3;

			var documentPivot = DocumentPivot.Create(new[] { pivot }).Single();
			var instructions = new HouseBillPrintInstructions(documentPivot, null, shipment);

			AssertEquals("SWB - Delivery Title", "ORIGINAL", instructions.GetDeliveryTitle(nameof(PrintCopyType.PRN)));

			AssertEquals("SWB - Delivery Title (Original can opnly be delivered to Print)", "COPY", instructions.GetDeliveryTitle(nameof(PrintCopyType.EML)));
			AssertEquals("SWB - Delivery Title (Original can opnly be delivered to Print)", "COPY", instructions.GetDeliveryTitle(nameof(PrintCopyType.FAX)));

			shipment.JS_ReleaseType = ShipmentReleaseTypes.ExpressBofL;
			AssertEquals("EBL - Delivery Title", "EXPRESS", instructions.GetDeliveryTitle(nameof(PrintCopyType.PRN)));
		}

		public void TestGetDeliveryTitle_Copy()
		{
			var menu = Factory.NewWithValidTestData<StmMenuItemBase>();
			menu.SU_MenuName = "Bill Of Lading";

			var template = Factory.NewWithValidTestData<StmTemplateBase>();

			var pivot1 = Factory.NewWithValidTestData<StmMenuTemplatePivotBase>();
			pivot1.SI_DocumentTitle = "Copy";
			pivot1.SI_PrintCopyType = nameof(PrintCopyType.PRN);
			pivot1.SI_SU = menu.PK;
			pivot1.SI_SO = template.PK;

			var pivot2 = Factory.NewWithValidTestData<StmMenuTemplatePivotBase>();
			pivot2.SI_DocumentTitle = "Copy";
			pivot2.SI_PrintCopyType = nameof(PrintCopyType.EML);
			pivot2.SI_SU = menu.PK;
			pivot2.SI_SO = template.PK;

			var pivot3 = Factory.NewWithValidTestData<StmMenuTemplatePivotBase>();
			pivot3.SI_DocumentTitle = "Copy";
			pivot3.SI_PrintCopyType = nameof(PrintCopyType.FAX);
			pivot3.SI_SU = menu.PK;
			pivot3.SI_SO = template.PK;

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_ReleaseType = ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_HouseBillOfLadingType = "IAU";
			shipment.JS_NoOriginalBills = 2;
			shipment.JS_NoCopyBills = 3;

			var documentPivots = DocumentPivot.Create(new[]
			{
				pivot1, pivot2, pivot3
			});

			AssertEquals("Created one pivot", 1, documentPivots.Length);

			var instructions = new HouseBillPrintInstructions(documentPivots.Single(), null, shipment);

			AssertEquals("SWB - Delivery Title", "COPY", instructions.GetDeliveryTitle(nameof(PrintCopyType.PRN)));
			AssertEquals("SWB - Delivery Title", "COPY", instructions.GetDeliveryTitle(nameof(PrintCopyType.EML)));
			AssertEquals("SWB - Delivery Title", "COPY", instructions.GetDeliveryTitle(nameof(PrintCopyType.FAX)));

			shipment.JS_ReleaseType = ShipmentReleaseTypes.ExpressBofL;
			AssertEquals("EBL - Delivery Title", "EXPRESS", instructions.GetDeliveryTitle(nameof(PrintCopyType.PRN)));
			AssertEquals("EBL - Delivery Title", "EXPRESS", instructions.GetDeliveryTitle(nameof(PrintCopyType.EML)));
			AssertEquals("EBL - Delivery Title", "EXPRESS", instructions.GetDeliveryTitle(nameof(PrintCopyType.FAX)));
		}

		#endregion

		#region TestNumberOfCopies

		public void TestNumberOfCopies_Original()
		{
			var menu = Factory.NewWithValidTestData<StmMenuItemBase>();
			menu.SU_MenuName = "Bill Of Lading";

			var template = Factory.NewWithValidTestData<StmTemplateBase>();

			var pivot = Factory.NewWithValidTestData<StmMenuTemplatePivotBase>();
			pivot.SI_DocumentTitle = "Original";
			pivot.SI_PrintCopyType = nameof(PrintCopyType.PRN);
			pivot.SI_SU = menu.PK;
			pivot.SI_SO = template.PK;

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_ReleaseType = ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_HouseBillOfLadingType = "IAU";
			shipment.JS_NoOriginalBills = 2;
			shipment.JS_NoCopyBills = 3;

			var documentPivot = DocumentPivot.Create(new[] { pivot }).Single();
			var instructions = new HouseBillPrintInstructions(documentPivot, null, shipment);

			AssertEquals("Number of Copies for Original", 2, instructions.GetNumberOfCopies(nameof(PrintCopyType.PRN)));

			shipment.JS_NoOriginalBills = 0;
			AssertEquals("SWB - Number of Copies for Original", 0, instructions.GetNumberOfCopies(nameof(PrintCopyType.PRN)));

			shipment.JS_ReleaseType = ShipmentReleaseTypes.ExpressBofL;
			shipment.JS_NoOriginalBills = 2;
			AssertEquals("EBL - Number of Copies for Original (EBL doesn't have any Originals)", 0, instructions.GetNumberOfCopies(nameof(PrintCopyType.PRN)));
		}

		public void TestNumberOfCopies_Copy()
		{
			var menu = Factory.NewWithValidTestData<StmMenuItemBase>();
			menu.SU_MenuName = "Bill Of Lading";

			var template = Factory.NewWithValidTestData<StmTemplateBase>();

			var pivot1 = Factory.NewWithValidTestData<StmMenuTemplatePivotBase>();
			pivot1.SI_DocumentTitle = "Copy";
			pivot1.SI_PrintCopyType = nameof(PrintCopyType.PRN);
			pivot1.SI_SU = menu.PK;
			pivot1.SI_SO = template.PK;

			var pivot2 = Factory.NewWithValidTestData<StmMenuTemplatePivotBase>();
			pivot2.SI_DocumentTitle = "Copy";
			pivot2.SI_PrintCopyType = nameof(PrintCopyType.EML);
			pivot2.SI_SU = menu.PK;
			pivot2.SI_SO = template.PK;

			var pivot3 = Factory.NewWithValidTestData<StmMenuTemplatePivotBase>();
			pivot3.SI_DocumentTitle = "Copy";
			pivot3.SI_PrintCopyType = nameof(PrintCopyType.FAX);
			pivot3.SI_SU = menu.PK;
			pivot3.SI_SO = template.PK;

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_ReleaseType = ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_HouseBillOfLadingType = "IAU";
			shipment.JS_NoOriginalBills = 2;
			shipment.JS_NoCopyBills = 3;

			var documentPivots = DocumentPivot.Create(new[]
			{
				pivot1, pivot2, pivot3
			});

			AssertEquals("Created one pivot", 1, documentPivots.Length);

			var instructions = new HouseBillPrintInstructions(documentPivots.Single(), null, shipment);

			AssertEquals("SWB - Number of Copies for Copy (Print)", 3, instructions.GetNumberOfCopies(nameof(PrintCopyType.PRN)));

			shipment.JS_NoCopyBills = 0;
			AssertEquals("SWB - Number of Copies for Copy (Print)", 0, instructions.GetNumberOfCopies(nameof(PrintCopyType.PRN)));

			shipment.JS_NoCopyBills = 3;
			AssertEquals("SWB - Number of Copies for Copy (Email)", 1, instructions.GetNumberOfCopies(nameof(PrintCopyType.EML)));
			AssertEquals("SWB - Number of Copies for Copy (Fax)", 1, instructions.GetNumberOfCopies(nameof(PrintCopyType.FAX)));

			shipment.JS_NoCopyBills = 0;
			AssertEquals("SWB - Number of Copies for Copy (Email)", 0, instructions.GetNumberOfCopies(nameof(PrintCopyType.EML)));
			AssertEquals("SWB - Number of Copies for Copy (Fax)", 0, instructions.GetNumberOfCopies(nameof(PrintCopyType.FAX)));

			shipment.JS_ReleaseType = ShipmentReleaseTypes.ExpressBofL;
			shipment.JS_NoCopyBills = 3;
			AssertEquals("EBL - Number of Copies for Copy (Email)", 1, instructions.GetNumberOfCopies(nameof(PrintCopyType.EML)));
			AssertEquals("EBL - Number of Copies for Copy (Fax)", 1, instructions.GetNumberOfCopies(nameof(PrintCopyType.FAX)));

			shipment.JS_NoCopyBills = 0;
			AssertEquals("EBL - Number of Copies for Copy (Email)", 0, instructions.GetNumberOfCopies(nameof(PrintCopyType.EML)));
			AssertEquals("EBL - Number of Copies for Copy (Fax)", 0, instructions.GetNumberOfCopies(nameof(PrintCopyType.FAX)));
		}

		#endregion

		#region TestGetParametersForDocumentDeliveryLog

		public void TestGetParametersForDocumentDeliveryLog()
		{
			var logParent = Factory.New<DummyWithLogs>();

			var menu = Factory.NewWithValidTestData<StmMenuItemBase>();

			var pivot = Factory.NewWithValidTestData<StmMenuTemplatePivotBase>();
			pivot.SI_SU = menu.PK;
			pivot.SI_PrintCopyType = "ALL";
			pivot.SI_DocumentTitle = "EXPRESS";

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_ReleaseType = "EBL";
			shipment.JS_HouseBillOfLadingType = "IAU";

			var documentPivot = DocumentPivot.Create(new[] { pivot }).Single();

			var instructions = new HouseBillPrintInstructions(documentPivot, null, shipment);

			var parameters = instructions.GetParametersForDocumentDeliveryLog("dummy").ToArray();

			AssertEquals("Bill Of Lading", parameters.Where(x => x.Key == "NAM").Select(kvp => kvp.Value).Single());
			AssertEquals("EXPRESS", parameters.Where(x => x.Key == "TYP").Select(kvp => kvp.Value).Single());
		}

		#endregion

		#region TestDoPrePrintProcessing

		public void TestDoPrePrintProcessing_IncompatibleDataSource()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var pivot = new Mock<IDocumentPivot>();
			var document = new Mock<IDocument>();
			document.SetupGet(d => d.Data).Returns(new object().MakeDynamic());

			var deliveryFactory = new BusinessObjectFactory();
			var houseBillPrintInstructions = new HouseBillPrintInstructions(pivot.Object, null, shipment);
			AssertNoExceptionThrown(() => houseBillPrintInstructions.DoPrePrintProcessing(document.Object, deliveryFactory, false));
		}

		public void TestDoPrePrintProcessing_NullDocument()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var pivot = new Mock<IDocumentPivot>();
			var document = new Mock<IDocument>();
			document.SetupGet(d => d.Data).Returns(new object().MakeDynamic());

			var deliveryFactory = new BusinessObjectFactory();
			var houseBillPrintInstructions = new HouseBillPrintInstructions(pivot.Object, null, shipment);
			AssertNoExceptionThrown(() => houseBillPrintInstructions.DoPrePrintProcessing(null, deliveryFactory, false));
		}

		#endregion

		#region TestSetIssueDateBeforePrinting

		[TestDate(2020, 7, 25)]
		public void TestSetIssueDateBeforePrinting_NonDraft() => AssertSetIssueDateBeforePrinting(false);

		[TestDate(2020, 7, 25)]
		public void TestSetIssueDateBeforePrinting_Draft() => AssertSetIssueDateBeforePrinting(true);

		void AssertSetIssueDateBeforePrinting(bool isDraft)
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			shipment.JS_ShipmentType = "STD";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_ReleaseType = "EBL";
			shipment.JS_HouseBillOfLadingType = "IAU";
			shipment.JS_HouseBillIssueDate = new ZDateTime();

			Factory.Save();

			Assert("prerequisite: Issue Date is empty", shipment.JS_HouseBillIssueDate.IsEmpty);

			var houseBill = new DummyHouseBill();
			var documentDataSource = houseBill.MakeDocDataDynamic();

			var document = new Mock<IDocument>();
			document
				.SetupGet(d => d.Data)
				.Returns(documentDataSource);

			var documentPivot = new Mock<IDocumentPivot>();
			documentPivot
				.SetupGet(p => p.DocumentTitle)
				.Returns("COPY");

			var deliveryFactory = new BusinessObjectFactory();
			var houseBillPrintInstructions = new HouseBillPrintInstructions(documentPivot.Object, null, shipment);
			houseBillPrintInstructions.DoPrePrintProcessing(document.Object, deliveryFactory, isDraft);

			var issueDate = documentDataSource.GetDynamicProperty(nameof(houseBill.DateOfIssue));
			Assert("Issue Date (DynamicData) is not marked as overridden", !issueDate.IsOverridden);
			Assert("Issue Date (DynamicData) in not marked as having changes", !issueDate.HasChanges);

			var shipmentInDeliveryFactory = deliveryFactory.Load<Forwarding.IForwardingShipment>(shipment.PK);

			if (isDraft)
			{
				AssertEquals("Issue Date has not been set for draft (DynamicData)", ZDateTime.Empty, issueDate.Value);
				AssertEquals("Issue Date has not been set for draft (DocDataObject)", ZDateTime.Empty, houseBill.DateOfIssue);
				AssertEquals("Issue Date has not been set for draft (IForwardingShipment)", ZDateTime.Empty, shipmentInDeliveryFactory.JS_HouseBillIssueDate);
			}
			else
			{
				AssertEquals("Issue Date has been set for non draft (DynamicData)", ZDateTime.Today, issueDate.Value);
				AssertEquals("Issue Date has been set for non draft (DocDataObject)", ZDateTime.Today, houseBill.DateOfIssue);
				AssertEquals("Issue Date has been set for non draft (IForwardingShipment)", ZDateTime.Today, shipmentInDeliveryFactory.JS_HouseBillIssueDate);
			}
		}

		[TestDate(2020, 7, 25)]
		public void TestSetIssueDateBeforePrinting_UserHasOverriddenIssueDateBeforeDelivery()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			shipment.JS_ShipmentType = "STD";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_ReleaseType = "EBL";
			shipment.JS_HouseBillOfLadingType = "IAU";
			shipment.JS_HouseBillIssueDate = new ZDateTime();

			Factory.Save();

			Assert("prerequisite: Issue Date is empty", shipment.JS_HouseBillIssueDate.IsEmpty);

			var houseBill = new DummyHouseBill();
			var documentDataSource = houseBill.MakeDocDataDynamic();

			var userManuallyEnteredDateOfIssue = ZDateTime.Today.AddDays(-1);

			var issueDate = documentDataSource.GetDynamicProperty(nameof(houseBill.DateOfIssue));
			issueDate.SetValue(userManuallyEnteredDateOfIssue);

			Assert("prerequisite: Issue Date (DynamicData) is marked as overridden", issueDate.IsOverridden);

			var document = new Mock<IDocument>();
			document
				.SetupGet(d => d.Data)
				.Returns(documentDataSource);

			var documentPivot = new Mock<IDocumentPivot>();
			documentPivot
				.SetupGet(p => p.DocumentTitle)
				.Returns("COPY");

			var deliveryFactory = new BusinessObjectFactory();
			var houseBillPrintInstructions = new HouseBillPrintInstructions(documentPivot.Object, null, shipment);
			houseBillPrintInstructions.DoPrePrintProcessing(document.Object, deliveryFactory, false);

			var shipmentInDeliveryFactory = deliveryFactory.Load<Forwarding.IForwardingShipment>(shipment.PK);

			AssertEquals("Issue Date has been set for (DynamicData)", userManuallyEnteredDateOfIssue, issueDate.Value);
			Assert("Issue Date (DynamicData) is marked as overridden", issueDate.IsOverridden);

			AssertEquals("Issue Date has been set for (DocDataObject)", userManuallyEnteredDateOfIssue, houseBill.DateOfIssue);
			AssertEquals("Issue Date has been set for (IForwardingShipment)", ZDateTime.Today, shipmentInDeliveryFactory.JS_HouseBillIssueDate);
		}

		#endregion
	}
}
