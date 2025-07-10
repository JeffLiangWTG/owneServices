using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class DocBaseWrapperNonInheritedTest : TestCaseWithFactory
	{
		public void TestGetDocDataValueOnlyOverride()
		{
			// Tested using BaseJobDeclaration as that's got real (and tested) DocData Fields.
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			var wrapper = DocBaseJobDeclaration.New(declaration, Factory);
			declaration.JE_GoodsDescription = "YEEEEEHA!!!";
			AssertEquals("YEEEEEHA!!!", wrapper.GetDocDataValue(DocBaseJobDeclaration.SDFields.AdditionalPaymentTerms, "{GoodsDescription}"));

			var docNote = DocumentNote.LoadNote(declaration);
			docNote.SetSystemDefinedFieldValue(DocBaseJobDeclaration.SDFields.AdditionalPaymentTerms, "YEEEEEHOOOO!!!");
			AssertEquals("YEEEEEHA!!!", wrapper.GetDocDataValue(DocBaseJobDeclaration.SDFields.AdditionalPaymentTerms, "{GoodsDescription}"));

			wrapper = DocBaseJobDeclaration.New(declaration, Factory);
			AssertEquals("YEEEEEHOOOO!!!", wrapper.GetDocDataValue(DocBaseJobDeclaration.SDFields.AdditionalPaymentTerms, "{GoodsDescription}"));
		}

		public void TestPrintStandardAndPrintClientSpecific()
		{
			Assert("PrintStandard should return true by default", BaseWrapper.PrintStandard);
			Assert("PrintSpecific should return false by default", !BaseWrapper.PrintClientSpecific);
		}

		public void TestSetTemplateConstants()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);

			AssertEquals("ReportName", ZString.Empty, BaseWrapper.ReportName);
			AssertEquals("DocumentDirection", ZString.Empty, BaseWrapper.DocumentDirection);
			AssertEquals("MenuTitle", ZString.Empty, BaseWrapper.MenuTitle);

			OrgHeader contact = OrgHeader.New(Factory);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ReportName, "Test Document");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.DocumentDirection, nameof(DocumentDirection.DEP));
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Menu Title");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContactType, ContactType.Consignee.Code);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContactOrganisationPK, contact.PK.ToString());
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.DeliveryMode, "EML");

			BaseWrapper.SetTemplateConstants(constants);
			AssertEquals("ReportName", "Test Document", BaseWrapper.ReportName);
			AssertEquals("DocumentDirection", nameof(DocumentDirection.DEP), BaseWrapper.DocumentDirection);
			AssertEquals("MenuTitle", "Menu Title", BaseWrapper.MenuTitle);
			AssertEquals("DeliveryMode", "EML", BaseWrapper.DocumentDeliveryMode);
			AssertEquals("ContactType", ContactType.Consignee, BaseWrapper.DocumentContactType);
			AssertEquals("ContactOrganisationPK", contact.PK.ToString(), BaseWrapper.ContactOrganisation.PK.ToString());
			AssertEquals("ContactOrganisation", contact, BaseWrapper.ContactOrganisation);
		}

		public void TestFormatNumber()
		{
			AssertEquals("FormatNumber", "0", BaseWrapper.FormatNumber(0M));
			AssertEquals("FormatNumber", "0", BaseWrapper.FormatNumber(0.0M));
			AssertEquals("FormatNumber", "0", BaseWrapper.FormatNumber(0.00M));
			AssertEquals("FormatNumber", "0", BaseWrapper.FormatNumber(0.000M));
			AssertEquals("FormatNumber", "0", BaseWrapper.FormatNumber(0.0000M));

			AssertEquals("FormatNumber", "1234", BaseWrapper.FormatNumber(1234M));
			AssertEquals("FormatNumber", "1234", BaseWrapper.FormatNumber(1234.0M));
			AssertEquals("FormatNumber", "1234", BaseWrapper.FormatNumber(1234.00M));
			AssertEquals("FormatNumber", "1234", BaseWrapper.FormatNumber(1234.000M));
			AssertEquals("FormatNumber", "1234", BaseWrapper.FormatNumber(1234.0000M));

			AssertEquals("FormatNumber", "1234", BaseWrapper.FormatNumber(1234M));
			AssertEquals("FormatNumber", "1234.1", BaseWrapper.FormatNumber(1234.1M));
			AssertEquals("FormatNumber", "1234.12", BaseWrapper.FormatNumber(1234.12M));
			AssertEquals("FormatNumber", "1234.123", BaseWrapper.FormatNumber(1234.123M));
			AssertEquals("FormatNumber", "1234.123", BaseWrapper.FormatNumber(1234.1234M));
			AssertEquals("FormatNumber", "1234.124", BaseWrapper.FormatNumber(1234.1239M));

			AssertEquals("FormatNumber", "0.1", BaseWrapper.FormatNumber(0.1000M));
			AssertEquals("FormatNumber", "0.01", BaseWrapper.FormatNumber(0.0100M));
			AssertEquals("FormatNumber", "0.011", BaseWrapper.FormatNumber(0.0110M));
			AssertEquals("FormatNumber", "1", BaseWrapper.FormatNumber(1.0000M));
			AssertEquals("FormatNumber", "1.001", BaseWrapper.FormatNumber(1.0010M));
			AssertEquals("FormatNumber", "1.01", BaseWrapper.FormatNumber(1.0100M));
			AssertEquals("FormatNumber", "1.011", BaseWrapper.FormatNumber(1.0110M));
			AssertEquals("FormatNumber", "1.1", BaseWrapper.FormatNumber(1.1000M));
			AssertEquals("FormatNumber", "1.101", BaseWrapper.FormatNumber(1.1010M));
			AssertEquals("FormatNumber", "1.111", BaseWrapper.FormatNumber(1.1110M));

			AssertEquals("FormatNumber", "1234", BaseWrapper.FormatNumber(1234M, 0));
			AssertEquals("FormatNumber", "1234.0", BaseWrapper.FormatNumber(1234M, 1));
			AssertEquals("FormatNumber", "1234.00", BaseWrapper.FormatNumber(1234M, 2));
			AssertEquals("FormatNumber", "1234.000", BaseWrapper.FormatNumber(1234M, 3));

			AssertEquals("FormatNumber", "1234", BaseWrapper.FormatNumber(1234M, 0));
			AssertEquals("FormatNumber", "1234.1", BaseWrapper.FormatNumber(1234.1M, 0));
			AssertEquals("FormatNumber", "1234.12", BaseWrapper.FormatNumber(1234.12M, 0));
			AssertEquals("FormatNumber", "1234.123", BaseWrapper.FormatNumber(1234.123M, 0));

			AssertEquals("FormatNumber", "1234.0", BaseWrapper.FormatNumber(1234M, 1));
			AssertEquals("FormatNumber", "1234.1", BaseWrapper.FormatNumber(1234.1M, 1));
			AssertEquals("FormatNumber", "1234.12", BaseWrapper.FormatNumber(1234.12M, 1));
			AssertEquals("FormatNumber", "1234.123", BaseWrapper.FormatNumber(1234.123M, 1));

			AssertEquals("FormatNumber", "1234.00", BaseWrapper.FormatNumber(1234M, 2));
			AssertEquals("FormatNumber", "1234.10", BaseWrapper.FormatNumber(1234.1M, 2));
			AssertEquals("FormatNumber", "1234.12", BaseWrapper.FormatNumber(1234.12M, 2));
			AssertEquals("FormatNumber", "1234.123", BaseWrapper.FormatNumber(1234.123M, 2));

			AssertEquals("FormatNumber", "1234.000", BaseWrapper.FormatNumber(1234M, 3));
			AssertEquals("FormatNumber", "1234.100", BaseWrapper.FormatNumber(1234.1M, 3));
			AssertEquals("FormatNumber", "1234.120", BaseWrapper.FormatNumber(1234.12M, 3));
			AssertEquals("FormatNumber", "1234.123", BaseWrapper.FormatNumber(1234.123M, 3));
		}

		#region Wrapper Fields

		public void TestCurrentCompany()
		{
			AssertNotNull("CurrentCompany", BaseWrapper.CurrentCompany);
			AssertEquals("CurrentCompany is of type DocComany", typeof(DocCompany), BaseWrapper.CurrentCompany.GetType());
		}

		public void TestCurrentBranch()
		{
			AssertNotNull("CurrentBranch", BaseWrapper.CurrentBranch);
			AssertEquals("CurrentBranch is of type DocBranch", typeof(DocBranch), BaseWrapper.CurrentBranch.GetType());
		}

		public void TestCurrentBranchDefaultBankAccount()
		{
			AccBankAccount bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_IsDefaultReceiptBankAccount = true;
			AssertNotNull("CurrentBranchDefaultBankAccount", BaseWrapper.CurrentBranchDefaultBankAccount);
			AssertEquals("CurrentBranchDefaultBankAccount is of type AccBankAcc", typeof(AccBankAccount), BaseWrapper.CurrentBranchDefaultBankAccount.GetType());
		}

		public void TestCurrentDepartment()
		{
			AssertNotNull("CurrentDepartment", BaseWrapper.CurrentDepartment);
			AssertEquals("CurrentDepartment is of type DocDepartment", typeof(DocDepartment), BaseWrapper.CurrentDepartment.GetType());
		}

		public void TestCurrentUser()
		{
			AssertNotNull("Current User", BaseWrapper.CurrentUser);
			AssertEquals("Current User name", GlbStaff.CurrentUser.GS_FullName, BaseWrapper.CurrentUser.FullName);
		}

		public void TestIsExportDocument()
		{
			BaseWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("IsExportDocument", ZBool.True, BaseWrapper.IsExportDocument);

			BaseWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("IsExportDocument", ZBool.False, BaseWrapper.IsExportDocument);

			BaseWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ANY));
			AssertEquals("IsExportDocument", ZBool.False, BaseWrapper.IsExportDocument);
		}

		public void TestIsImportDocument()
		{
			BaseWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("IsImportDocument", ZBool.True, BaseWrapper.IsImportDocument);

			BaseWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("IsImportDocument", ZBool.False, BaseWrapper.IsImportDocument);

			BaseWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ANY));
			AssertEquals("IsImportDocument", ZBool.False, BaseWrapper.IsImportDocument);
		}

		public void TestIsAnyDocument()
		{
			BaseWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("IsAnyDocument", ZBool.False, BaseWrapper.IsAnyDocument);

			BaseWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("IsAnyDocument", ZBool.False, BaseWrapper.IsAnyDocument);

			BaseWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ANY));
			AssertEquals("IsAnyDocument", ZBool.True, BaseWrapper.IsAnyDocument);
		}

		#endregion

		#region Document Weight Volume Display Tests

		public void TestWeightVolumeDisplayOptionNoDocumentName()
		{
			BaseWrapper.SetReportNameForTesting("NON SYSTEM DEFINED DOCUMENT");
			BaseWrapper.SetDocumentDirectionForTesting("ANY");
			AssertEquals(Core.WeightAndVolumeDisplayTypes.Codes.Actual, BaseWrapper.GetWeightVolumeDisplayOptionTestMethod());
		}

		public void TestPreAlertWeightVolumeDisplay()
		{
			AssertWeightVolumeDisplay("Pre-Alert", DocumentDirection.ARV, BusinessContext.Shipment,
				"Pre-Alert", Env.Registry.PreAlertWeightAndVolumeDisplay);
		}

		public void TestArrivalNoticeWeightVolumeDisplay()
		{
			AssertWeightVolumeDisplay("Arrival Notice", DocumentDirection.ARV, BusinessContext.Shipment,
				"Arrival Notice", Env.Registry.ArrivalNoticeWeightAndVolumeDisplay);
		}

		public void TestShippingAdviceWeightVolumeDisplay()
		{
			AssertWeightVolumeDisplay("Shipping Advice", DocumentDirection.ARV, BusinessContext.Shipment,
				"Shipping Advice", Env.Registry.ShippingAdviceWeightAndVolumeDisplay);
		}

		public void TestDeliveryOrderWeightVolumeDisplay()
		{
			AssertWeightVolumeDisplay("Delivery Order", DocumentDirection.ARV, BusinessContext.Shipment,
				"Delivery Order", Env.Registry.DeliveryOrderWeightAndVolumeDisplay);
		}

		public void TestOutturnReportWeightVolumeDisplay()
		{
			AssertWeightVolumeDisplay("Outturn Report", DocumentDirection.ARV, BusinessContext.Shipment,
				"Outturn Report", Env.Registry.OutturnReportWeightAndVolumeDisplay);
		}

		public void TestAgentsInstructionWeightVolumeDisplay()
		{
			AssertWeightVolumeDisplay("Agents Instruction", DocumentDirection.DEP, BusinessContext.Shipment,
				"Agents Instruction", Env.Registry.AgentsInstructionNoticeWeightAndVolumeDisplay);
		}

		public void TestBookingConfirmationWeightVolumeDisplay()
		{
			AssertWeightVolumeDisplay("Booking Confirmation", DocumentDirection.DEP, BusinessContext.Shipment,
				"Booking Confirmation", Env.Registry.BookingConfirmationWeightAndVolumeDisplay);
		}

		public void TestShipperDepartureNoticeWeightVolumeDisplay()
		{
			AssertWeightVolumeDisplay("Shipper Departure Notice", DocumentDirection.DEP, BusinessContext.Shipment,
				"Shipper Departure Notice", Env.Registry.ConsolShipperDepartureNoticeWeightAndVolumeDisplay);
		}

		public void TestAgentDepartureNoticeWeightVolumeDisplay()
		{
			AssertWeightVolumeDisplay("Agent Departure Notice", DocumentDirection.DEP, BusinessContext.Consol,
				"Agent Departure Notice", Env.Registry.ConsolAgentDepartureNoticeWeightAndVolumeDisplay);
		}

		public void TestLetterToOverseasAgentWeightVolumeDisplay()
		{
			AssertWeightVolumeDisplay("Letter To Overseas Agent", DocumentDirection.DEP, BusinessContext.Consol,
				"Letter To Overseas Agent", Env.Registry.ConsolLetterToOverseasAgentWeightAndVolumeDisplay);
		}

		public void TestForwardingInstructionsWeightVolumeDisplay()
		{
			AssertWeightVolumeDisplay("Forwarding Instruction Standard", DocumentDirection.DEP, BusinessContext.Consol,
				"Forwarding Instruction", Env.Registry.ConsolForwardingInstructionWeightAndVolumeDisplay);

			AssertWeightVolumeDisplay("Forwarding Instruction Detailed", DocumentDirection.DEP, BusinessContext.Consol,
				"Forwarding Instruction", Env.Registry.ConsolForwardingInstructionWeightAndVolumeDisplay);
		}

		public void TestCargoLoadListWeightVolumeDisplay()
		{
			AssertWeightVolumeDisplay("Cargo Load List", DocumentDirection.DEP, BusinessContext.Consol,
				"Cargo Load List", Env.Registry.ConsolCargoLoadListWeightAndVolumeDisplay);
		}

		public void TestExportCartageAdviceWeightVolumeDisplay()
		{
			AssertWeightVolumeDisplay("Cartage Advice", DocumentDirection.DEP, BusinessContext.Shipment,
				"Shipment Cartage Advice", Env.Registry.CartageAdviceExportWeightAndVolumeDisplay);
		}

		public void TestImportCartageWithReceiptAdviceWeightVolumeDisplay()
		{
			AssertWeightVolumeDisplay("Cartage Advice with Receipt", DocumentDirection.ARV, BusinessContext.Shipment,
				"Shipment Cartage Advice with Receipt", Env.Registry.CartageAdviceImportWeightAndVolumeDisplay);
		}

		public void TestExportCartageWithReceiptAdviceWeightVolumeDisplay()
		{
			AssertWeightVolumeDisplay("Cartage Advice with Receipt", DocumentDirection.DEP, BusinessContext.Shipment,
				"Shipment Cartage Advice with Receipt", Env.Registry.CartageAdviceExportWeightAndVolumeDisplay);
		}

		public void TestExportManifestWeightVolumeDisplay()
		{
			AssertWeightVolumeDisplay("Manifest (Portrait)", DocumentDirection.DEP, BusinessContext.Consol,
										"Manifest", Env.Registry.ConsolManifestConsolExportWeightAndVolumeDisplay);

			AssertWeightVolumeDisplay("Manifest Standard (Landscape)", DocumentDirection.DEP, BusinessContext.Consol,
										"Manifest (Landscape)", Env.Registry.ConsolManifestConsolExportWeightAndVolumeDisplay);

			AssertWeightVolumeDisplay("Manifest Detailed (Landscape)", DocumentDirection.DEP, BusinessContext.Consol,
										"Manifest Detailed (Landscape)", Env.Registry.ConsolManifestConsolExportWeightAndVolumeDisplay);
		}

		public void TestImportManifestWeightVolumeDisplay()
		{
			AssertWeightVolumeDisplay("Manifest (Portrait)", DocumentDirection.ARV, BusinessContext.Consol,
										"Manifest", Env.Registry.ConsolManifestConsolImportWeightAndVolumeDisplay);

			AssertWeightVolumeDisplay("Manifest Standard (Landscape)", DocumentDirection.ARV, BusinessContext.Consol,
										"Manifest (Landscape)", Env.Registry.ConsolManifestConsolImportWeightAndVolumeDisplay);

			AssertWeightVolumeDisplay("Manifest Detailed (Landscape)", DocumentDirection.ARV, BusinessContext.Consol,
										"Manifest Detailed (Landscape)", Env.Registry.ConsolManifestConsolImportWeightAndVolumeDisplay);
		}

		public void TestImportColoadMasterManifestWeightVolumeDisplay()
		{
			AssertWeightVolumeDisplay("Master Shipment Manifest", DocumentDirection.ARV, BusinessContext.Shipment,
				"Master Shipment Manifest", Env.Registry.CoLoadMasterManifestWeightAndVolumeDisplay);
		}

		public void TestExportColoadMasterManifestWeightVolumeDisplay()
		{
			AssertWeightVolumeDisplay("Master Shipment Manifest", DocumentDirection.DEP, BusinessContext.Shipment,
				"Master Shipment Manifest", Env.Registry.CoLoadMasterManifestWeightAndVolumeDisplay);
		}

		#endregion

		#region GetNotes Utilities

		#region GetNotes
		void TestGetNotes_BizObjOrNotesOfBizObj(bool useBizObject)
		{
			var shipment = Factory.New<ForwardingShipment>();
			if (useBizObject)
			{
				AssertEquals("", BaseWrapper.GetNotesTestMethod(PredefinedNoteTypes.Instance.MarksAndNumbers.Description, shipment));
			}
			else
			{
				AssertEquals("", BaseWrapper.GetNotesTestMethod(PredefinedNoteTypes.Instance.MarksAndNumbers.Description, shipment.Notes));
			}

			AddNote(shipment, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, "PUB", "Marks and numbers Line One\nLine Two");
			AddNote(shipment, PredefinedNoteTypes.Instance.BookingNotes.Description, "PUB", "This is other notes and should not be included.");
			Factory.Save();

			if (useBizObject)
			{
				AssertEquals("Marks and numbers", "Marks and numbers Line One\nLine Two", BaseWrapper.GetNotesTestMethod(PredefinedNoteTypes.Instance.MarksAndNumbers.Description, shipment));
			}
			else
			{
				AssertEquals("Marks and numbers", "Marks and numbers Line One\nLine Two", BaseWrapper.GetNotesTestMethod(PredefinedNoteTypes.Instance.MarksAndNumbers.Description, shipment.Notes));
			}
		}

		public void TestGetNotes()
		{
			AssertEquals("", BaseWrapper.GetNotesTestMethod(PredefinedNoteTypes.Instance.MarksAndNumbers.Description, (BusinessObject)null));

			TestGetNotes_BizObjOrNotesOfBizObj(true);
		}

		public void TestGetNotesByContext()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			AddNote(shipment, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, "INT", "Testing Context - AIR (AAI in new 3-part Module/Direction/FreightMode NoteContext format)", nameof(StmNoteContextModule.A) + nameof(StmNoteContextDirection.A) + nameof(StmNoteContextFreightMode.I));
			AddNote(shipment, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, "INT", "Testing Context - SEA (AAS in new 3-part Module/Direction/FreightMode NoteContext format)", nameof(StmNoteContextModule.A) + nameof(StmNoteContextDirection.A) + nameof(StmNoteContextFreightMode.S));
			AddNote(shipment, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, "INT", "Testing Context - ALL (AAA in new 3-part Module/Direction/FreightMode NoteContext format)", StmNoteContextUtils.StmNoteContextsAllToString);
			AddNote(shipment, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, "INT", "Testing Context - FCL (AAF in new 3-part Module/Direction/FreightMode NoteContext format)", nameof(StmNoteContextModule.A) + nameof(StmNoteContextDirection.A) + nameof(StmNoteContextFreightMode.F));
			AddNote(shipment, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, "INT", "Testing Context - LCL (AAL in new 3-part Module/Direction/FreightMode NoteContext format)", nameof(StmNoteContextModule.A) + nameof(StmNoteContextDirection.A) + nameof(StmNoteContextFreightMode.L));
			AddNote(shipment, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, "INT", "Testing Context - No Context");

			string airNotesText = BaseWrapper.GetNotesTestMethod(PredefinedNoteTypes.Instance.MarksAndNumbers.Description, Core.Constants.TransportModes.Air, shipment.Notes);
			AssertEquals(true, airNotesText.Contains("Testing Context - ALL (AAA in new 3-part Module/Direction/FreightMode NoteContext format)"));
			AssertEquals(true, airNotesText.Contains("Testing Context - AIR (AAI in new 3-part Module/Direction/FreightMode NoteContext format)"));
			AssertEquals(false, airNotesText.Contains("Testing Context - SEA (AAS in new 3-part Module/Direction/FreightMode NoteContext format)"));
			AssertEquals(true, airNotesText.Contains("Testing Context - No Context"));
			AssertEquals(false, airNotesText.Contains("Testing Context - FCL (AAS in new 3-part Module/Direction/FreightMode NoteContext format)"));
			AssertEquals(false, airNotesText.Contains("Testing Context - LCL (AAS in new 3-part Module/Direction/FreightMode NoteContext format)"));

			string seaNotesText = BaseWrapper.GetNotesTestMethod(PredefinedNoteTypes.Instance.MarksAndNumbers.Description, Core.Constants.TransportModes.Sea, shipment.Notes);
			AssertEquals(true, seaNotesText.Contains("Testing Context - ALL (AAA in new 3-part Module/Direction/FreightMode NoteContext format)"));
			AssertEquals(true, seaNotesText.Contains("Testing Context - SEA (AAS in new 3-part Module/Direction/FreightMode NoteContext format)"));
			AssertEquals(false, seaNotesText.Contains("Testing Context - AIR (AAI in new 3-part Module/Direction/FreightMode NoteContext format)"));
			AssertEquals(true, seaNotesText.Contains("Testing Context - No Context"));
			AssertEquals(false, seaNotesText.Contains("Testing Context - FCL (AAS in new 3-part Module/Direction/FreightMode NoteContext format)"));
			AssertEquals(false, seaNotesText.Contains("Testing Context - LCL (AAS in new 3-part Module/Direction/FreightMode NoteContext format)"));

			string emptyNotesText = BaseWrapper.GetNotesTestMethod(PredefinedNoteTypes.Instance.MarksAndNumbers.Description, "", shipment.Notes);
			AssertEquals(true, emptyNotesText.Contains("Testing Context - ALL (AAA in new 3-part Module/Direction/FreightMode NoteContext format)"));
			AssertEquals(true, emptyNotesText.Contains("Testing Context - SEA (AAS in new 3-part Module/Direction/FreightMode NoteContext format)"));
			AssertEquals(true, emptyNotesText.Contains("Testing Context - AIR (AAI in new 3-part Module/Direction/FreightMode NoteContext format)"));
			AssertEquals(true, emptyNotesText.Contains("Testing Context - No Context"));
			AssertEquals(false, emptyNotesText.Contains("Testing Context - FCL (AAS in new 3-part Module/Direction/FreightMode NoteContext format)"));
			AssertEquals(false, emptyNotesText.Contains("Testing Context - LCL (AAS in new 3-part Module/Direction/FreightMode NoteContext format)"));

			string lclNotesText = BaseWrapper.GetNotesTestMethod(PredefinedNoteTypes.Instance.MarksAndNumbers.Description, Core.Constants.ContainerModes.LCL, shipment.Notes);
			AssertEquals(true, lclNotesText.Contains("Testing Context - ALL (AAA in new 3-part Module/Direction/FreightMode NoteContext format)"));
			AssertEquals(false, lclNotesText.Contains("Testing Context - AIR (AAI in new 3-part Module/Direction/FreightMode NoteContext format)"));
			AssertEquals(false, lclNotesText.Contains("Testing Context - SEA (AAS in new 3-part Module/Direction/FreightMode NoteContext format)"));
			AssertEquals(false, lclNotesText.Contains("Testing Context - FCL (AAF in new 3-part Module/Direction/FreightMode NoteContext format)"));
			AssertEquals(true, lclNotesText.Contains("Testing Context - LCL (AAL in new 3-part Module/Direction/FreightMode NoteContext format)"));
			AssertEquals(true, airNotesText.Contains("Testing Context - No Context"));

			string fclNotesText = BaseWrapper.GetNotesTestMethod(PredefinedNoteTypes.Instance.MarksAndNumbers.Description, Core.Constants.ContainerModes.FCL, shipment.Notes);
			AssertEquals(true, fclNotesText.Contains("Testing Context - ALL (AAA in new 3-part Module/Direction/FreightMode NoteContext format)"));
			AssertEquals(false, fclNotesText.Contains("Testing Context - AIR (AAI in new 3-part Module/Direction/FreightMode NoteContext format)"));
			AssertEquals(false, fclNotesText.Contains("Testing Context - SEA (AAS in new 3-part Module/Direction/FreightMode NoteContext format)"));
			AssertEquals(true, fclNotesText.Contains("Testing Context - FCL (AAF in new 3-part Module/Direction/FreightMode NoteContext format)"));
			AssertEquals(false, fclNotesText.Contains("Testing Context - LCL (AAL in new 3-part Module/Direction/FreightMode NoteContext format)"));
			AssertEquals(true, airNotesText.Contains("Testing Context - No Context"));
		}

		public void TestGetNotesByContextDirection()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			string handlingDesc = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			AddNote(shipment, handlingDesc, "INT", "Testing Context Direction - ALL/AIR (AAI)", nameof(StmNoteContextModule.A) + nameof(StmNoteContextDirection.A) + nameof(StmNoteContextFreightMode.I));
			AddNote(shipment, handlingDesc, "INT", "Testing Context Direction - ALL/SEA (AAS)", nameof(StmNoteContextModule.A) + nameof(StmNoteContextDirection.A) + nameof(StmNoteContextFreightMode.S));
			AddNote(shipment, handlingDesc, "INT", "Testing Context Direction - ALL (AAA)", StmNoteContextUtils.StmNoteContextsAllToString);
			AddNote(shipment, handlingDesc, "INT", "Testing Context Direction - ALL/FCL (AAF)", nameof(StmNoteContextModule.A) + nameof(StmNoteContextDirection.A) + nameof(StmNoteContextFreightMode.F));
			AddNote(shipment, handlingDesc, "INT", "Testing Context Direction - ALL/LCL (AAL)", nameof(StmNoteContextModule.A) + nameof(StmNoteContextDirection.A) + nameof(StmNoteContextFreightMode.L));
			AddNote(shipment, handlingDesc, "INT", "Testing Context Direction - No Context");

			string airNotesText = BaseWrapper.GetNotesTestMethod(handlingDesc, Core.Constants.TransportModes.Air, shipment.Notes, nameof(StmNoteContextDirection.A));
			AssertEquals(true, airNotesText.Contains("Testing Context Direction - ALL (AAA)"));
			AssertEquals(true, airNotesText.Contains("Testing Context Direction - ALL/AIR (AAI)"));
			AssertEquals(false, airNotesText.Contains("Testing Context Direction - ALL/SEA (AAS)"));
			AssertEquals(true, airNotesText.Contains("Testing Context Direction - No Context"));
			AssertEquals(false, airNotesText.Contains("Testing Context Direction - ALL/FCL (AAF)"));
			AssertEquals(false, airNotesText.Contains("Testing Context Direction - ALL/LCL (AAL)"));

			string seaNotesText = BaseWrapper.GetNotesTestMethod(handlingDesc, Core.Constants.TransportModes.Sea, shipment.Notes, nameof(StmNoteContextDirection.A));
			AssertEquals(true, seaNotesText.Contains("Testing Context Direction - ALL (AAA)"));
			AssertEquals(true, seaNotesText.Contains("Testing Context Direction - ALL/SEA (AAS)"));
			AssertEquals(false, seaNotesText.Contains("Testing Context Direction - ALL/AIR (AAI)"));
			AssertEquals(true, seaNotesText.Contains("Testing Context Direction - No Context"));
			AssertEquals(false, seaNotesText.Contains("Testing Context Direction - ALL/FCL (AAF)"));
			AssertEquals(false, seaNotesText.Contains("Testing Context Direction - ALL/LCL (AAL)"));

			string emptyNotesText = BaseWrapper.GetNotesTestMethod(handlingDesc, "", shipment.Notes, nameof(StmNoteContextDirection.A));
			AssertEquals(true, emptyNotesText.Contains("Testing Context Direction - ALL (AAA)"));
			AssertEquals(true, emptyNotesText.Contains("Testing Context Direction - ALL/SEA (AAS)"));
			AssertEquals(true, emptyNotesText.Contains("Testing Context Direction - ALL/AIR (AAI)"));
			AssertEquals(true, emptyNotesText.Contains("Testing Context Direction - No Context"));
			AssertEquals(true, emptyNotesText.Contains("Testing Context Direction - ALL/FCL (AAF)"));
			AssertEquals(true, emptyNotesText.Contains("Testing Context Direction - ALL/LCL (AAL)"));

			string lclNotesText = BaseWrapper.GetNotesTestMethod(handlingDesc, Core.Constants.ContainerModes.LCL, shipment.Notes, nameof(StmNoteContextDirection.A));
			AssertEquals(true, lclNotesText.Contains("Testing Context Direction - ALL (AAA)"));
			AssertEquals(false, lclNotesText.Contains("Testing Context Direction - ALL/AIR (AAI)"));
			AssertEquals(false, lclNotesText.Contains("Testing Context Direction - ALL/SEA (AAS)"));
			AssertEquals(false, lclNotesText.Contains("Testing Context Direction - ALL/FCL (AAF)"));
			AssertEquals(true, lclNotesText.Contains("Testing Context Direction - ALL/LCL (AAL)"));
			AssertEquals(true, airNotesText.Contains("Testing Context Direction - No Context"));

			string fclNotesText = BaseWrapper.GetNotesTestMethod(handlingDesc, Core.Constants.ContainerModes.FCL, shipment.Notes, nameof(StmNoteContextDirection.A));
			AssertEquals(true, fclNotesText.Contains("Testing Context Direction - ALL (AAA)"));
			AssertEquals(false, fclNotesText.Contains("Testing Context Direction - ALL/AIR (AAI)"));
			AssertEquals(false, fclNotesText.Contains("Testing Context Direction - ALL/SEA (AAS)"));
			AssertEquals(true, fclNotesText.Contains("Testing Context Direction - ALL/FCL (AAF)"));
			AssertEquals(false, fclNotesText.Contains("Testing Context Direction - ALL/LCL (AAL)"));
			AssertEquals(true, airNotesText.Contains("Testing Context Direction - No Context"));

			//Add Import Notes
			AddNote(shipment, handlingDesc, "INT", "Import/AIR (AII)", nameof(StmNoteContextModule.A) + nameof(StmNoteContextDirection.I) + nameof(StmNoteContextFreightMode.I));
			AddNote(shipment, handlingDesc, "INT", "Import/SEA (AIS)", nameof(StmNoteContextModule.A) + nameof(StmNoteContextDirection.I) + nameof(StmNoteContextFreightMode.S));
			AddNote(shipment, handlingDesc, "INT", "Import/FCL (AIF)", nameof(StmNoteContextModule.A) + nameof(StmNoteContextDirection.I) + nameof(StmNoteContextFreightMode.F));
			AddNote(shipment, handlingDesc, "INT", "Import/LCL (AIL)", nameof(StmNoteContextModule.A) + nameof(StmNoteContextDirection.I) + nameof(StmNoteContextFreightMode.L));

			//Add Export Notes
			AddNote(shipment, handlingDesc, "INT", "Export/AIR (AEI)", nameof(StmNoteContextModule.A) + nameof(StmNoteContextDirection.E) + nameof(StmNoteContextFreightMode.I));
			AddNote(shipment, handlingDesc, "INT", "Export/SEA (AES)", nameof(StmNoteContextModule.A) + nameof(StmNoteContextDirection.E) + nameof(StmNoteContextFreightMode.S));
			AddNote(shipment, handlingDesc, "INT", "Export/FCL (AEF)", nameof(StmNoteContextModule.A) + nameof(StmNoteContextDirection.E) + nameof(StmNoteContextFreightMode.F));
			AddNote(shipment, handlingDesc, "INT", "Export/LCL (AEL)", nameof(StmNoteContextModule.A) + nameof(StmNoteContextDirection.E) + nameof(StmNoteContextFreightMode.L));

			//Add Both Import & Export Notes
			AddNote(shipment, handlingDesc, "INT", "Both Import & Export/AIR (ABI)", nameof(StmNoteContextModule.A) + nameof(StmNoteContextDirection.B) + nameof(StmNoteContextFreightMode.I));
			AddNote(shipment, handlingDesc, "INT", "Both Import & Export/SEA (ABS)", nameof(StmNoteContextModule.A) + nameof(StmNoteContextDirection.B) + nameof(StmNoteContextFreightMode.S));
			AddNote(shipment, handlingDesc, "INT", "Both Import & Export/FCL (ABF)", nameof(StmNoteContextModule.A) + nameof(StmNoteContextDirection.B) + nameof(StmNoteContextFreightMode.F));
			AddNote(shipment, handlingDesc, "INT", "Both Import & Export/LCL (ABL)", nameof(StmNoteContextModule.A) + nameof(StmNoteContextDirection.B) + nameof(StmNoteContextFreightMode.L));

			airNotesText = BaseWrapper.GetNotesTestMethod(handlingDesc, Core.Constants.TransportModes.Air, shipment.Notes, nameof(StmNoteContextDirection.A));
			AssertEquals(true, airNotesText.Contains("Testing Context Direction - ALL (AAA)"));
			AssertEquals(true, airNotesText.Contains("Testing Context Direction - ALL/AIR (AAI)"));
			AssertEquals(false, airNotesText.Contains("Testing Context Direction - ALL/SEA (AAS)"));
			AssertEquals(true, airNotesText.Contains("Testing Context Direction - No Context"));
			AssertEquals(false, airNotesText.Contains("Testing Context Direction - ALL/FCL (AAF)"));
			AssertEquals(false, airNotesText.Contains("Testing Context Direction - ALL/LCL (AAL)"));

			string importNotesText = BaseWrapper.GetNotesTestMethod(handlingDesc, "", shipment.Notes, nameof(StmNoteContextDirection.I));
			AssertEquals(true, importNotesText.Contains("Import/AIR (AII)"));
			AssertEquals(true, importNotesText.Contains("Import/SEA (AIS)"));
			AssertEquals(true, importNotesText.Contains("Import/FCL (AIF)"));
			AssertEquals(true, importNotesText.Contains("Import/LCL (AIL)"));
			AssertEquals(true, importNotesText.Contains("Testing Context Direction - ALL (AAA)"));
			AssertEquals(true, importNotesText.Contains("Testing Context Direction - ALL/AIR (AAI)"));
			AssertEquals(true, importNotesText.Contains("Testing Context Direction - ALL/SEA (AAS)"));
			AssertEquals(true, importNotesText.Contains("Testing Context Direction - No Context"));
			AssertEquals(true, importNotesText.Contains("Testing Context Direction - ALL/FCL (AAF)"));
			AssertEquals(true, importNotesText.Contains("Testing Context Direction - ALL/LCL (AAL)"));
			AssertEquals(false, importNotesText.Contains("Export/AIR (AEI)"));
			AssertEquals(false, importNotesText.Contains("Export/SEA (AES)"));
			AssertEquals(false, importNotesText.Contains("Export/FCL (AEF)"));
			AssertEquals(false, importNotesText.Contains("Export/LCL (AEL)"));
			AssertEquals(true, importNotesText.Contains("Both Import & Export/AIR (ABI)"));
			AssertEquals(true, importNotesText.Contains("Both Import & Export/SEA (ABS)"));
			AssertEquals(true, importNotesText.Contains("Both Import & Export/FCL (ABF)"));
			AssertEquals(true, importNotesText.Contains("Both Import & Export/LCL (ABL)"));

			string importAirNotesText = BaseWrapper.GetNotesTestMethod(handlingDesc, Core.Constants.TransportModes.Air, shipment.Notes, nameof(StmNoteContextDirection.I));
			AssertEquals(true, importAirNotesText.Contains("Import/AIR (AII)"));
			AssertEquals(false, importAirNotesText.Contains("Import/SEA (AIS)"));
			AssertEquals(false, importAirNotesText.Contains("Import/FCL (AIF)"));
			AssertEquals(false, importAirNotesText.Contains("Import/LCL (AIL)"));
			AssertEquals(true, importAirNotesText.Contains("Testing Context Direction - ALL (AAA)"));
			AssertEquals(true, importAirNotesText.Contains("Testing Context Direction - ALL/AIR (AAI)"));
			AssertEquals(false, importAirNotesText.Contains("Testing Context Direction - ALL/SEA (AAS)"));
			AssertEquals(true, importAirNotesText.Contains("Testing Context Direction - No Context"));
			AssertEquals(false, importAirNotesText.Contains("Testing Context Direction - ALL/FCL (AAF)"));
			AssertEquals(false, importAirNotesText.Contains("Testing Context Direction - ALL/LCL (AAL)"));
			AssertEquals(false, importAirNotesText.Contains("Export/AIR (AEI)"));
			AssertEquals(false, importAirNotesText.Contains("Export/SEA (AES)"));
			AssertEquals(false, importAirNotesText.Contains("Export/FCL (AEF)"));
			AssertEquals(false, importAirNotesText.Contains("Export/LCL (AEL)"));
			AssertEquals(true, importAirNotesText.Contains("Both Import & Export/AIR (ABI)"));
			AssertEquals(false, importAirNotesText.Contains("Both Import & Export/SEA (ABS)"));
			AssertEquals(false, importAirNotesText.Contains("Both Import & Export/FCL (ABF)"));
			AssertEquals(false, importAirNotesText.Contains("Both Import & Export/LCL (ABL)"));

			string importSeaNotesText = BaseWrapper.GetNotesTestMethod(handlingDesc, Core.Constants.TransportModes.Sea, shipment.Notes, nameof(StmNoteContextDirection.I));
			AssertEquals(false, importSeaNotesText.Contains("Import/AIR (AII)"));
			AssertEquals(true, importSeaNotesText.Contains("Import/SEA (AIS)"));
			AssertEquals(false, importSeaNotesText.Contains("Import/FCL (AIF)"));
			AssertEquals(false, importSeaNotesText.Contains("Import/LCL (AIL)"));
			AssertEquals(true, importSeaNotesText.Contains("Testing Context Direction - ALL (AAA)"));
			AssertEquals(false, importSeaNotesText.Contains("Testing Context Direction - ALL/AIR (AAI)"));
			AssertEquals(true, importSeaNotesText.Contains("Testing Context Direction - ALL/SEA (AAS)"));
			AssertEquals(true, importSeaNotesText.Contains("Testing Context Direction - No Context"));
			AssertEquals(false, importSeaNotesText.Contains("Testing Context Direction - ALL/FCL (AAF)"));
			AssertEquals(false, importSeaNotesText.Contains("Testing Context Direction - ALL/LCL (AAL)"));
			AssertEquals(false, importSeaNotesText.Contains("Export/AIR (AEI)"));
			AssertEquals(false, importSeaNotesText.Contains("Export/SEA (AES)"));
			AssertEquals(false, importSeaNotesText.Contains("Export/FCL (AEF)"));
			AssertEquals(false, importSeaNotesText.Contains("Export/LCL (AEL)"));
			AssertEquals(false, importSeaNotesText.Contains("Both Import & Export/AIR (ABI)"));
			AssertEquals(true, importSeaNotesText.Contains("Both Import & Export/SEA (ABS)"));
			AssertEquals(false, importSeaNotesText.Contains("Both Import & Export/FCL (ABF)"));
			AssertEquals(false, importSeaNotesText.Contains("Both Import & Export/LCL (ABL)"));

			string importFCLNotesText = BaseWrapper.GetNotesTestMethod(handlingDesc, "FCL", shipment.Notes, nameof(StmNoteContextDirection.I));
			AssertEquals(false, importFCLNotesText.Contains("Import/AIR (AII)"));
			AssertEquals(false, importFCLNotesText.Contains("Import/SEA (AIS)"));
			AssertEquals(true, importFCLNotesText.Contains("Import/FCL (AIF)"));
			AssertEquals(false, importFCLNotesText.Contains("Import/LCL (AIL)"));
			AssertEquals(true, importFCLNotesText.Contains("Testing Context Direction - ALL (AAA)"));
			AssertEquals(false, importFCLNotesText.Contains("Testing Context Direction - ALL/AIR (AAI)"));
			AssertEquals(false, importFCLNotesText.Contains("Testing Context Direction - ALL/SEA (AAS)"));
			AssertEquals(true, importFCLNotesText.Contains("Testing Context Direction - No Context"));
			AssertEquals(true, importFCLNotesText.Contains("Testing Context Direction - ALL/FCL (AAF)"));
			AssertEquals(false, importFCLNotesText.Contains("Testing Context Direction - ALL/LCL (AAL)"));
			AssertEquals(false, importFCLNotesText.Contains("Export/AIR (AEI)"));
			AssertEquals(false, importFCLNotesText.Contains("Export/SEA (AES)"));
			AssertEquals(false, importFCLNotesText.Contains("Export/FCL (AEF)"));
			AssertEquals(false, importFCLNotesText.Contains("Export/LCL (AEL)"));
			AssertEquals(false, importFCLNotesText.Contains("Both Import & Export/AIR (ABI)"));
			AssertEquals(false, importFCLNotesText.Contains("Both Import & Export/SEA (ABS)"));
			AssertEquals(true, importFCLNotesText.Contains("Both Import & Export/FCL (ABF)"));
			AssertEquals(false, importFCLNotesText.Contains("Both Import & Export/LCL (ABL)"));

			string importLCLNotesText = BaseWrapper.GetNotesTestMethod(handlingDesc, "LCL", shipment.Notes, nameof(StmNoteContextDirection.I));
			AssertEquals(false, importLCLNotesText.Contains("Import/AIR (AII)"));
			AssertEquals(false, importLCLNotesText.Contains("Import/SEA (AIS)"));
			AssertEquals(false, importLCLNotesText.Contains("Import/FCL (AIF)"));
			AssertEquals(true, importLCLNotesText.Contains("Import/LCL (AIL)"));
			AssertEquals(true, importLCLNotesText.Contains("Testing Context Direction - ALL (AAA)"));
			AssertEquals(false, importLCLNotesText.Contains("Testing Context Direction - ALL/AIR (AAI)"));
			AssertEquals(false, importLCLNotesText.Contains("Testing Context Direction - ALL/SEA (AAS)"));
			AssertEquals(true, importLCLNotesText.Contains("Testing Context Direction - No Context"));
			AssertEquals(false, importLCLNotesText.Contains("Testing Context Direction - ALL/FCL (AAF)"));
			AssertEquals(true, importLCLNotesText.Contains("Testing Context Direction - ALL/LCL (AAL)"));
			AssertEquals(false, importLCLNotesText.Contains("Export/AIR (AEI)"));
			AssertEquals(false, importLCLNotesText.Contains("Export/SEA (AES)"));
			AssertEquals(false, importLCLNotesText.Contains("Export/FCL (AEF)"));
			AssertEquals(false, importLCLNotesText.Contains("Export/LCL (AEL)"));
			AssertEquals(false, importLCLNotesText.Contains("Both Import & Export/AIR (ABI)"));
			AssertEquals(false, importLCLNotesText.Contains("Both Import & Export/SEA (ABS)"));
			AssertEquals(false, importLCLNotesText.Contains("Both Import & Export/FCL (ABF)"));
			AssertEquals(true, importLCLNotesText.Contains("Both Import & Export/LCL (ABL)"));

			string exportNotesText = BaseWrapper.GetNotesTestMethod(handlingDesc, "", shipment.Notes, nameof(StmNoteContextDirection.E));
			AssertEquals(false, exportNotesText.Contains("Import/AIR (AII)"));
			AssertEquals(false, exportNotesText.Contains("Import/SEA (AIS)"));
			AssertEquals(false, exportNotesText.Contains("Import/FCL (AIF)"));
			AssertEquals(false, exportNotesText.Contains("Import/LCL (AIL)"));
			AssertEquals(true, exportNotesText.Contains("Testing Context Direction - ALL (AAA)"));
			AssertEquals(true, exportNotesText.Contains("Testing Context Direction - ALL/AIR (AAI)"));
			AssertEquals(true, exportNotesText.Contains("Testing Context Direction - ALL/SEA (AAS)"));
			AssertEquals(true, exportNotesText.Contains("Testing Context Direction - No Context"));
			AssertEquals(true, exportNotesText.Contains("Testing Context Direction - ALL/FCL (AAF)"));
			AssertEquals(true, exportNotesText.Contains("Testing Context Direction - ALL/LCL (AAL)"));
			AssertEquals(true, exportNotesText.Contains("Export/AIR (AEI)"));
			AssertEquals(true, exportNotesText.Contains("Export/SEA (AES)"));
			AssertEquals(true, exportNotesText.Contains("Export/FCL (AEF)"));
			AssertEquals(true, exportNotesText.Contains("Export/LCL (AEL)"));
			AssertEquals(true, exportNotesText.Contains("Both Import & Export/AIR (ABI)"));
			AssertEquals(true, exportNotesText.Contains("Both Import & Export/SEA (ABS)"));
			AssertEquals(true, exportNotesText.Contains("Both Import & Export/FCL (ABF)"));
			AssertEquals(true, exportNotesText.Contains("Both Import & Export/LCL (ABL)"));

			string exportAirNotesText = BaseWrapper.GetNotesTestMethod(handlingDesc, Core.Constants.TransportModes.Air, shipment.Notes, nameof(StmNoteContextDirection.E));
			AssertEquals(false, exportAirNotesText.Contains("Import/AIR (AII)"));
			AssertEquals(false, exportAirNotesText.Contains("Import/SEA (AIS)"));
			AssertEquals(false, exportAirNotesText.Contains("Import/FCL (AIF)"));
			AssertEquals(false, exportAirNotesText.Contains("Import/LCL (AIL)"));
			AssertEquals(true, exportAirNotesText.Contains("Testing Context Direction - ALL (AAA)"));
			AssertEquals(true, exportAirNotesText.Contains("Testing Context Direction - ALL/AIR (AAI)"));
			AssertEquals(false, exportAirNotesText.Contains("Testing Context Direction - ALL/SEA (AAS)"));
			AssertEquals(true, exportAirNotesText.Contains("Testing Context Direction - No Context"));
			AssertEquals(false, exportAirNotesText.Contains("Testing Context Direction - ALL/FCL (AAF)"));
			AssertEquals(false, exportAirNotesText.Contains("Testing Context Direction - ALL/LCL (AAL)"));
			AssertEquals(true, exportAirNotesText.Contains("Export/AIR (AEI)"));
			AssertEquals(false, exportAirNotesText.Contains("Export/SEA (AES)"));
			AssertEquals(false, exportAirNotesText.Contains("Export/FCL (AEF)"));
			AssertEquals(false, exportAirNotesText.Contains("Export/LCL (AEL)"));
			AssertEquals(true, exportAirNotesText.Contains("Both Import & Export/AIR (ABI)"));
			AssertEquals(false, exportAirNotesText.Contains("Both Import & Export/SEA (ABS)"));
			AssertEquals(false, exportAirNotesText.Contains("Both Import & Export/FCL (ABF)"));
			AssertEquals(false, exportAirNotesText.Contains("Both Import & Export/LCL (ABL)"));

			string exportSeaNotesText = BaseWrapper.GetNotesTestMethod(handlingDesc, Core.Constants.TransportModes.Sea, shipment.Notes, nameof(StmNoteContextDirection.E));
			AssertEquals(false, exportSeaNotesText.Contains("Import/AIR (AII)"));
			AssertEquals(false, exportSeaNotesText.Contains("Import/SEA (AIS)"));
			AssertEquals(false, exportSeaNotesText.Contains("Import/FCL (AIF)"));
			AssertEquals(false, exportSeaNotesText.Contains("Import/LCL (AIL)"));
			AssertEquals(true, exportSeaNotesText.Contains("Testing Context Direction - ALL (AAA)"));
			AssertEquals(false, exportSeaNotesText.Contains("Testing Context Direction - ALL/AIR (AAI)"));
			AssertEquals(true, exportSeaNotesText.Contains("Testing Context Direction - ALL/SEA (AAS)"));
			AssertEquals(true, exportSeaNotesText.Contains("Testing Context Direction - No Context"));
			AssertEquals(false, exportSeaNotesText.Contains("Testing Context Direction - ALL/FCL (AAF)"));
			AssertEquals(false, exportSeaNotesText.Contains("Testing Context Direction - ALL/LCL (AAL)"));
			AssertEquals(false, exportSeaNotesText.Contains("Export/AIR (AEI)"));
			AssertEquals(true, exportSeaNotesText.Contains("Export/SEA (AES)"));
			AssertEquals(false, exportSeaNotesText.Contains("Export/FCL (AEF)"));
			AssertEquals(false, exportSeaNotesText.Contains("Export/LCL (AEL)"));
			AssertEquals(false, exportSeaNotesText.Contains("Both Import & Export/AIR (ABI)"));
			AssertEquals(true, exportSeaNotesText.Contains("Both Import & Export/SEA (ABS)"));
			AssertEquals(false, exportSeaNotesText.Contains("Both Import & Export/FCL (ABF)"));
			AssertEquals(false, exportSeaNotesText.Contains("Both Import & Export/LCL (ABL)"));

			string exportFCLNotesText = BaseWrapper.GetNotesTestMethod(handlingDesc, "FCL", shipment.Notes, nameof(StmNoteContextDirection.E));
			AssertEquals(false, exportFCLNotesText.Contains("Import/AIR (AII)"));
			AssertEquals(false, exportFCLNotesText.Contains("Import/SEA (AIS)"));
			AssertEquals(false, exportFCLNotesText.Contains("Import/FCL (AIF)"));
			AssertEquals(false, exportFCLNotesText.Contains("Import/LCL (AIL)"));
			AssertEquals(true, exportFCLNotesText.Contains("Testing Context Direction - ALL (AAA)"));
			AssertEquals(false, exportFCLNotesText.Contains("Testing Context Direction - ALL/AIR (AAI)"));
			AssertEquals(false, exportFCLNotesText.Contains("Testing Context Direction - ALL/SEA (AAS)"));
			AssertEquals(true, exportFCLNotesText.Contains("Testing Context Direction - No Context"));
			AssertEquals(true, exportFCLNotesText.Contains("Testing Context Direction - ALL/FCL (AAF)"));
			AssertEquals(false, exportFCLNotesText.Contains("Testing Context Direction - ALL/LCL (AAL)"));
			AssertEquals(false, exportFCLNotesText.Contains("Export/AIR (AEI)"));
			AssertEquals(false, exportFCLNotesText.Contains("Export/SEA (AES)"));
			AssertEquals(true, exportFCLNotesText.Contains("Export/FCL (AEF)"));
			AssertEquals(false, exportFCLNotesText.Contains("Export/LCL (AEL)"));
			AssertEquals(false, exportFCLNotesText.Contains("Both Import & Export/AIR (ABI)"));
			AssertEquals(false, exportFCLNotesText.Contains("Both Import & Export/SEA (ABS)"));
			AssertEquals(true, exportFCLNotesText.Contains("Both Import & Export/FCL (ABF)"));
			AssertEquals(false, exportFCLNotesText.Contains("Both Import & Export/LCL (ABL)"));

			string exportLCLNotesText = BaseWrapper.GetNotesTestMethod(handlingDesc, "LCL", shipment.Notes, nameof(StmNoteContextDirection.E));
			AssertEquals(false, exportLCLNotesText.Contains("Import/AIR (AII)"));
			AssertEquals(false, exportLCLNotesText.Contains("Import/SEA (AIS)"));
			AssertEquals(false, exportLCLNotesText.Contains("Import/FCL (AIF)"));
			AssertEquals(false, exportLCLNotesText.Contains("Import/LCL (AIL)"));
			AssertEquals(true, exportLCLNotesText.Contains("Testing Context Direction - ALL (AAA)"));
			AssertEquals(false, exportLCLNotesText.Contains("Testing Context Direction - ALL/AIR (AAI)"));
			AssertEquals(false, exportLCLNotesText.Contains("Testing Context Direction - ALL/SEA (AAS)"));
			AssertEquals(true, exportLCLNotesText.Contains("Testing Context Direction - No Context"));
			AssertEquals(false, exportLCLNotesText.Contains("Testing Context Direction - ALL/FCL (AAF)"));
			AssertEquals(true, exportLCLNotesText.Contains("Testing Context Direction - ALL/LCL (AAL)"));
			AssertEquals(false, exportLCLNotesText.Contains("Export/AIR (AEI)"));
			AssertEquals(false, exportLCLNotesText.Contains("Export/SEA (AES)"));
			AssertEquals(false, exportLCLNotesText.Contains("Export/FCL (AEF)"));
			AssertEquals(true, exportLCLNotesText.Contains("Export/LCL (AEL)"));
			AssertEquals(false, exportLCLNotesText.Contains("Both Import & Export/AIR (ABI)"));
			AssertEquals(false, exportLCLNotesText.Contains("Both Import & Export/SEA (ABS)"));
			AssertEquals(false, exportLCLNotesText.Contains("Both Import & Export/FCL (ABF)"));
			AssertEquals(true, exportLCLNotesText.Contains("Both Import & Export/LCL (ABL)"));
		}

		public void TestGetNotesPassingStmNotes()
		{
			AssertEquals("", BaseWrapper.GetNotesTestMethod(PredefinedNoteTypes.Instance.MarksAndNumbers.Description, null));

			TestGetNotes_BizObjOrNotesOfBizObj(false);
		}

		[ExpectNoExceptions]
		public void TestGetNotesIfNoteContextIsEmpty()
		{
			Db.Connection.ExecuteNonQuery(@"IF (OBJECT_ID('Constraint_ST_NoteContext_NoCheck', 'C') IS NOT NULL)
										BEGIN
											ALTER TABLE dbo.StmNote NOCHECK CONSTRAINT Constraint_ST_NoteContext_NoCheck
										END");
			var dummy = Factory.New<DummyWithRelatedDummy>();
			var stmNote = dummy.GetNotes().AddNew(true, "Desc", "Main Note");
			Factory.Save();
			Db.Connection.ExecuteNonQuery($"UPDATE dbo.StmNote SET ST_NoteContext = '' WHERE ST_PK = '{stmNote.PK}'");
			stmNote.Reload();

			var wrapper = new DocBaseWrapperForTesting(dummy, Factory);
			AssertEquals("Main Note", wrapper.GetNotesTestMethod("Desc", dummy));
		}

		public void TestGetNotesUsesRelatedNotes()
		{
			DummyWithRelatedDummy dummy = Factory.New<DummyWithRelatedDummy>();
			StmNote mainNote = dummy.GetNotes().AddNew(true, "Desc", "Main Note");
			StmNote innerNote = dummy.InnerDummy.GetNotes().AddNew(true, "Desc", "Inner Note");

			DocBaseWrapperForTesting wrapper = new DocBaseWrapperForTesting(dummy, Factory);

			AssertEquals("Main Note", wrapper.GetNotesTestMethod("Desc", dummy));
			dummy.GetNotes().RemoveAndDeleteAll();

			AssertEquals("Inner Note", wrapper.GetNotesTestMethod("Desc", dummy));

			dummy.InnerDummy.GetNotes().RemoveAndDeleteAll();
			AssertEquals("", wrapper.GetNotesTestMethod("Desc", dummy));
		}

		class DummyWithRelatedDummy : DummyEnterpriseBusinessObject, IStmNoteParent
		{
			public DummyWithRelatedDummy(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public DummyEnterpriseBusinessObject InnerDummy
			{
				get { return fInnerDummy ?? (fInnerDummy = Factory.New<DummyEnterpriseBusinessObject>()); }
			}
			DummyEnterpriseBusinessObject fInnerDummy;

			#region IStmNoteParent Members

			Notes IStmNoteParent.Notes
			{
				get { return notes ?? (notes = new Notes(this)); }
			}
			Notes notes;

			ZGuid IStmNoteParent.NotesParentPK
			{
				get { return PK; }
			}

			string IStmNoteParent.NotesParentTableName
			{
				get { return TableName; }
			}

			BusinessObjectFactory IStmNoteParent.NotesFactory
			{
				get { return Factory; }
			}

			bool IStmNoteParent.SupportsNotes
			{
				get { return false; }
			}

			BusinessObject[] IStmNoteParent.BusinessObjectsWithRelatedNotes
			{
				get { return new BusinessObject[] { InnerDummy }; }
			}

			StmNoteContexts IStmNoteParent.NoteContextsForRelatedNotes
			{
				get { return StmNoteContexts.Default; }
			}

			NoteTypeCollection IStmNoteParent.NoteTypes
			{
				get { return new NoteTypeCollection(); }
			}

			#endregion
		}

		#endregion

		#region TestGetAllNotes
		void TestGetAllNotes_BizObjectOrNotesOfBizObj(bool useBizObject)
		{
			var shipment = Factory.New<ForwardingShipment>();
			if (useBizObject)
			{
				AssertEquals("", BaseWrapper.GetNotesTestMethod(PredefinedNoteTypes.Instance.MarksAndNumbers.Description, shipment));
			}
			else
			{
				AssertEquals("", BaseWrapper.GetNotesTestMethod(PredefinedNoteTypes.Instance.MarksAndNumbers.Description, shipment.Notes));
			}

			AddNote(shipment, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, "INT", "Marks and numbers Line One\nLine Two");
			AddNote(shipment, PredefinedNoteTypes.Instance.BookingNotes.Description, "PUB", "Order refereces. Reference 1, Reference 2, Reference 3\nMore references.");
			Factory.Save();

			ZString expected = PredefinedNoteTypes.Instance.MarksAndNumbers.Description.ToUpper();
			expected += "  (INTERNAL)\n";
			expected += "Marks and numbers Line One\nLine Two" + "\n\n";
			expected += PredefinedNoteTypes.Instance.BookingNotes.Description.ToUpper();
			expected += "  (CLIENT-VISIBLE)\n";
			expected += "Order refereces. Reference 1, Reference 2, Reference 3\nMore references.";
			if (useBizObject)
			{
				AssertEquals("All Notes", expected, BaseWrapper.GetAllNotesTestMethod(shipment));
			}
			else
			{
				AssertEquals("All Notes", expected, BaseWrapper.GetAllNotesTestMethod(shipment.Notes));
			}
		}

		public void TestGetAllNotes()
		{
			AssertEquals("", BaseWrapper.GetNotesTestMethod(PredefinedNoteTypes.Instance.MarksAndNumbers.Description, (BusinessObject)null));
			TestGetAllNotes_BizObjectOrNotesOfBizObj(true);
		}

		public void TestGetAllNotesPassingStmNotes()
		{
			AssertEquals("", BaseWrapper.GetNotesTestMethod(PredefinedNoteTypes.Instance.MarksAndNumbers.Description, null));
			TestGetAllNotes_BizObjectOrNotesOfBizObj(false);
		}

		#endregion

		#region TestGetAllNotesInStringArray
		void TestGetAllNotesInStringArray_BizObjectOrNotesOfBizObj(bool useBizObject)
		{
			var shipment = Factory.New<ForwardingShipment>();
			if (useBizObject)
			{
				AssertEquals("", BaseWrapper.GetNotesTestMethod(PredefinedNoteTypes.Instance.MarksAndNumbers.Description, shipment));
			}
			else
			{
				AssertEquals("", BaseWrapper.GetNotesTestMethod(PredefinedNoteTypes.Instance.MarksAndNumbers.Description, shipment.Notes));
			}
			AddNote(shipment, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, "INT", "Marks and numbers Line One\nLine Two");
			AddNote(shipment, PredefinedNoteTypes.Instance.BookingNotes.Description, "PRV", "Order refereces. Reference 1, Reference 2, Reference 3\nMore references.");
			Factory.Save();

			ZString[] result;
			if (useBizObject)
			{
				result = BaseWrapper.GetAllNotesInStringArrayTestMethod(shipment);
			}
			else
			{
				result = BaseWrapper.GetAllNotesInStringArrayTestMethod(shipment.Notes);
			}
			AssertEquals(PredefinedNoteTypes.Instance.MarksAndNumbers.Description.ToUpper() + "  (INTERNAL)", result[0]);
			AssertEquals("Marks and numbers Line One", result[1]);
			AssertEquals("Line Two", result[2]);
			AssertEquals("", result[3]);
			AssertEquals(PredefinedNoteTypes.Instance.BookingNotes.Description.ToUpper() + "  (PRIVATE)", result[4]);
			AssertEquals("Order refereces. Reference 1, Reference 2, Reference 3", result[5]);
			AssertEquals("More references.", result[6]);
		}

		public void TestGetAllNotesInStringArray()
		{
			TestGetAllNotesInStringArray_BizObjectOrNotesOfBizObj(true);
		}

		public void TestGetAllNotesInStringArray_PassingNotes()
		{
			TestGetAllNotesInStringArray_BizObjectOrNotesOfBizObj(false);
		}
		#endregion

		#endregion

		#region DocManager barcode properties

		public void TestDocManagerCode()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();
			var mockWrapper = new Mock<DocBaseWrapperForTesting>(dec, Factory) { CallBase = true };
			var wrapper = mockWrapper.Object;
			AssertEquals("Code-[DEC]\r\nUniqueID-[]", wrapper.BarcodePrerequisitesForTestingONLY);
			mockWrapper.Protected().Setup<BusinessObject>("GetParentBOForNoteStorageEDocsAndDocData").Returns(shipment);
			AssertEquals("Code-[SHP]\r\nUniqueID-[]", wrapper.BarcodePrerequisitesForTestingONLY);
		}

		public void TestBarcodeTextForFontPlaceholder()
		{
			ZString barcodeText = ((IDocManagerPlaceholderBarcode)BaseWrapper).BarcodeTextForFontPlaceholder;

			AssertEquals(
@"Default value should just be empty string (property will be overridden in
subclass) and both the things following should be empty:
" + BaseWrapper.BarcodePrerequisitesForTestingONLY
					, ZString.Empty, barcodeText);
		}

		public void TestBarcodeTextPlaceholder()
		{
			ZString barcodeText = ((IDocManagerPlaceholderBarcode)BaseWrapper).BarcodeTextPlaceholder;
			AssertEquals("Default value should just be empty string (property will be overridden in subclass", ZString.Empty, barcodeText);
		}

		public void TestBarcodeTextForFont()
		{
			ZString barcodeText = ((IDocManagerBarcode)BaseWrapper).BarcodeTextForFont;
			AssertEquals("Default value should just be empty string (Barcode will be overridden in subclass)", ZString.Empty, barcodeText);
		}

		public void TestBarcodeText()
		{
			ZString barcodeText = ((IDocManagerBarcode)BaseWrapper).BarcodeText;
			AssertEquals("Default value should just be empty string (Barcode will be overridden in subclass)", ZString.Empty, barcodeText);
		}

		public void TestDocManagerBarcodeText()
		{
			var barcodeDetails = BaseWrapper;
			AssertNotNull("Since this is the default value for subclasses, this property should not return null", barcodeDetails.BarcodeText);
			AssertEquals("This property should be empty string by default", ZString.Empty, barcodeDetails.BarcodeText);
		}

		public void TestDocManagerBarcodeTextForFont()
		{
			var barcodeDetails = BaseWrapper;
			AssertNotNull("Since this is the default value for subclasses, this property should not return null", barcodeDetails.BarcodeTextForFont);
			AssertEquals("This property should be empty string by default", ZString.Empty, barcodeDetails.BarcodeTextForFont);
		}

		#endregion

		#region Customised Client Logo
		public void TestCustomisedLogo()
		{
			AssertNull("Base should not return anything", BaseWrapper.CustomisedLogo);
		}
		#endregion

		#region Air and Sea Watermarks

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAirWatermark()
		{
			var resourceRetriever = new EmbeddedResourceRetriever();
			var bytes = resourceRetriever.GetBytes("Enterprise.DocumentWrappers.Testing.AirWatermark.png");
			Image testImage = Image.FromStream(new MemoryStream(bytes));
			AssertImageEquals("Air watermark image is not coming from the expected file", testImage, BaseWrapper.AirWatermark);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSeaWatermark()
		{
			var resourceRetriever = new EmbeddedResourceRetriever();
			var bytes = resourceRetriever.GetBytes("Enterprise.DocumentWrappers.Testing.SeaWatermark.png");
			Image testImage = Image.FromStream(new MemoryStream(bytes));
			AssertImageEquals("Sea watermark image is not coming from the expected file", testImage, BaseWrapper.SeaWatermark);
		}

		#endregion

		#region Implementation

		DocBaseWrapperForTesting BaseWrapper;

		protected override void SetUp()
		{
			var consol = Factory.New<ForwardingConsol>();
			BaseWrapper = new DocBaseWrapperForTesting(consol, Factory);
			base.SetUp();
		}

		void AssertWeightVolumeDisplay(ZString menuName, DocumentDirection direction, BusinessContext businessContext, ZString documentTitle, string displayType)
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_BusinessContext, businessContext.ToString());
			filter.AddToFilter(StmMenuItemSchema.SU_MenuName, menuName);
			filter.AddToFilter(StmMenuItemSchema.SU_DocumentDirection, direction.ToString());
			var menuItems = Factory.Load<StmMenuItem>(filter);
			if (menuItems.Length > 1)
			{
				filter.AddToFilter(StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.StartsWith, Enterprise.Core.Constants.DocumentEngine.MenuPaths.LegacyDocuments);
				menuItems = Factory.Load<StmMenuItem>(filter);
			}
			AssertEquals("Should have found 1 Menu item for: [" + menuName + "].", 1, menuItems.Length);
			StmMenuItem menuItem = menuItems[0];

			ZQuery pivotFilter = new ZQuery(StmMenuTemplatePivotSchema.SI_SU, menuItem.PK);
			pivotFilter.AddToFilter(StmMenuTemplatePivotSchema.SI_DocumentTitle, SQLComparisonOperator.Contains, documentTitle);
			var pivot = Factory.LoadTop1<StmMenuTemplatePivot>(pivotFilter);
			AssertNotNull("StmMenuTemplatePivot for " + menuName + " with SI_DocumentTitle " + documentTitle + " could not be found", pivot);

			BaseWrapper.SetReportNameForTesting(pivot.SI_DocumentTitle);
			BaseWrapper.SetDocumentDirectionForTesting(menuItem.SU_DocumentDirection);
			AssertEquals("Wrong display type", displayType, BaseWrapper.GetWeightVolumeDisplayOptionTestMethod());
		}

		void AddNote(BusinessObject bizo, ZString noteDesc, ZString noteType, ZString noteData)
		{
			AddNote(bizo, noteDesc, noteType, noteData, StmNoteContextUtils.StmNoteContextsAllToString);
		}

		void AddNote(BusinessObject bizo, ZString noteDesc, ZString noteType, ZString noteData, ZString context)
		{
			var note = bizo.GetNotes().AddNew();
			note.ST_Description = noteDesc;
			note.ST_ParentID = bizo.PK;
			note.ST_Table = bizo.TableName;
			note.ST_NoteDataAsText = noteData;
			note.ST_NoteType = noteType;
			note.ST_NoteContext = context;
		}
		#endregion
	}
}
