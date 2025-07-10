using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public sealed class PredefinedNoteTypesTestCase : TransactionedTestCase
	{
		#region Construction

		const string TestNoteTypeDescription = "Test Note Type";

		public void TestOverriding()
		{
			ClientPredefinedNoteTypes.Unregister(); // To clear any previous initialization

			AssertEquals("Initial PredefinedNoteTypes", typeof(PredefinedNoteTypes), PredefinedNoteTypes.Instance.GetType());
			AssertEquals("Initial PredefinedNoteTypes", false, PredefinedNoteTypes.Instance.NoteTypeByDescription(TestNoteTypeDescription) != null);

			ClientPredefinedNoteTypes.Register();
			AssertEquals("Should return the overridden PredefinedNoteTypes", typeof(ClientPredefinedNoteTypes), PredefinedNoteTypes.Instance.GetType());
			AssertEquals("Should return the overridden PredefinedNoteTypes", true, PredefinedNoteTypes.Instance.NoteTypeByDescription(TestNoteTypeDescription) != null);

			ClientPredefinedNoteTypes.Unregister();
			AssertEquals("Should revert back to the initial PredefinedNoteTypes", typeof(PredefinedNoteTypes), PredefinedNoteTypes.Instance.GetType());
			AssertEquals("Should revert back to the initial PredefinedNoteTypes", false, PredefinedNoteTypes.Instance.NoteTypeByDescription(TestNoteTypeDescription) != null);
		}

		public void TestOverrideResetAfterTest()
		{
			ClientPredefinedNoteTypes.Register();
			AssertEquals("Should return the overridden PredefinedNoteTypes", typeof(ClientPredefinedNoteTypes), PredefinedNoteTypes.Instance.GetType());
			Overridable.ResetAll();
			AssertEquals("Should revert back to the initial PredefinedNoteTypes", typeof(PredefinedNoteTypes), PredefinedNoteTypes.Instance.GetType());
		}

		class ClientPredefinedNoteTypes : PredefinedNoteTypes
		{
			protected ClientPredefinedNoteTypes()
			{
			}

			public new static ClientPredefinedNoteTypes Instance
			{
				get { return (ClientPredefinedNoteTypes)PredefinedNoteTypes.Instance; }
			}

			public static void Register()
			{
				OverrideNewDelegate(New);
			}

			public static void Unregister()
			{
				ResetNewDelegate();
			}

			static PredefinedNoteTypes New()
			{
				return new ClientPredefinedNoteTypes();
			}

			public PredefinedNoteType ClientNoteType
			{
				get
				{
					if (fClientNoteType == null)
					{
						fClientNoteType = new PredefinedNoteType((NoResString)"Test Note Type", StmNoteVisibility.PRV, IsUniqueInCollection, !IsReadOnlyAfterAdd, !IsTextOnly, false);
					}
					return fClientNoteType;
				}
			}
			PredefinedNoteType fClientNoteType;
		}

		#endregion

		#region Unique Descriptions

		public void TestNoteDescriptionsAreUnique()
		{
			Hashtable hash = new Hashtable();

			foreach (PredefinedNoteType noteType in PredefinedNoteTypes.Instance.All)
			{
				string description = noteType.Description.Trim();
				Assert(!hash.ContainsValue(description));

				hash.Add(description, description);
			}
		}

		public void TestTypesWithNonUniqueDescriptionAreSynchronizedToViewInDB()
		{
			List<string> nonUniqueNotesInDB = new List<string>();
			List<string> nonUniqueNotesInCode = new List<string>();

			using (var cmd = Db.Connection.Command("SELECT NoteDescription FROM dbo.vw_StmNoteNonUniqueDescriptions"))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					nonUniqueNotesInDB.Add(reader[0].ToString());
				}
			}

			nonUniqueNotesInCode.AddRange(GetNonUniqueNoteTypes(typeof(PredefinedNoteTypes)));
			nonUniqueNotesInCode.AddRange(ReadClientSpecificPredefinedNoteTypes());
			nonUniqueNotesInCode.AddRange(HardCodedNonUniqueNoteTypes.ToList());
			nonUniqueNotesInCode = nonUniqueNotesInCode.Distinct().ToList();

			AssertEquals(nonUniqueNotesInCode.Count, nonUniqueNotesInDB.Count);
			AssertContainsExactElementsInAnyOrder(nonUniqueNotesInCode, nonUniqueNotesInDB);
		}

		readonly string[] HardCodedNonUniqueNoteTypes = { "Conversation", "Comments", "Email Sent" };

		List<string> ReadClientSpecificPredefinedNoteTypes()
		{
			List<string> nonUniqueNotesInClientSpecificDLL = new List<string>();
			string[] files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");

			foreach (string file in files)
			{
				if (Modules.ClientHookLoader.Instance.IsAnyNonWebClientOverrideAssembly(Path.GetFileNameWithoutExtension(file), includingTestAssemblies: false))
				{
					var dll = Assembly.LoadFrom(file);
					var derivedTypes = dll.GetTypes().Where(t => t.IsSubclassOf(typeof(PredefinedNoteTypes)));
					foreach (var derivedType in derivedTypes)
					{
						nonUniqueNotesInClientSpecificDLL.AddRange(GetNonUniqueNoteTypes(derivedType));
					}
				}
			}

			return nonUniqueNotesInClientSpecificDLL;
		}

		List<string> GetNonUniqueNoteTypes(Type type)
		{
			List<string> nonUniqueNotesInCode = new List<string>();
			var constructor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Array.Empty<Type>(), null);
			var instance = constructor.Invoke(null);
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			foreach (var p in properties)
			{
				if (p.PropertyType == typeof(PredefinedNoteType) || p.PropertyType.IsSubclassOf(typeof(PredefinedNoteType)))
				{
					var note = p.GetValue(instance, null) as PredefinedNoteType;
					if (!note.IsOnlyOneAllowed)
					{
						nonUniqueNotesInCode.Add(note.Description);
					}
				}
			}

			return nonUniqueNotesInCode;
		}

		#endregion

		#region Notes

		public void TestAccessoryDescription()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.AccessoryDescription, "Accessory Description", StmNoteVisibility.PRV, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestLeadSheetComments()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.LeadSheetComments, "Lead Sheet Comments", StmNoteVisibility.INT, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestCustomsManualStatus()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.CustomsManualStatus, "Customs Manual Status", StmNoteVisibility.PRV, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestManualRelease()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.ManualRelease, "Manual Release", StmNoteVisibility.PRV, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestManualCancel()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.ManualCancel, "Manual Cancel", StmNoteVisibility.PRV, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestContainerReleaseNote()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.ContainerReleaseNote, "Container Release Note", StmNoteVisibility.PUB, IsOnlyOneAllowed, IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestDeliveryInstructionsNote()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.DeliveryInstructionsNote, "Import Delivery Instructions", StmNoteVisibility.PUB, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestPickingInstructions()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.PickingInstructions, "Picking Instructions", StmNoteVisibility.PUB, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestPickupInstructionsNote()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.PickupInstructionsNote, "Export Pickup Instructions", StmNoteVisibility.PUB, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestExtraOrderDetails()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.ExtraOrderDetails, "Extra Order Details", StmNoteVisibility.PUB, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestInvoiceDetailsNote()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.InvoiceDetails, "Invoice Details", StmNoteVisibility.PUB, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestSurveyInstruction()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.SurveyInstruction, "Survey Instruction", StmNoteVisibility.PUB, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestUnmatchedOrgDetailsNote()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.UnmatchedOrgDetails, "Unmatched Org Details", StmNoteVisibility.PRV, IsOnlyOneAllowed, IsReadOnlyAfterAdd, IsTextOnly);
			AssertEquals("Serializable note type", ObjectFactory.GetType<MasterFiles.Integration.IUnmatchOrgDetailRecords>(), PredefinedNoteTypes.Instance.UnmatchedOrgDetails.SerializableNoteType);
		}

		public void TestCartageHistoryNotes()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.CartageHistoryNotes, "Cartage History Notes", StmNoteVisibility.INT, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestGatePassNotes()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.GatePassNotes, "Gate Pass Notes", StmNoteVisibility.PUB, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestDetailedGoodsDescription()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.DetailedGoodsDescription, "Detailed Goods Description", StmNoteVisibility.PUB, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestExtendedCommercialDescription()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.ExtendedCommercialDescription, "Extended Commercial Description", StmNoteVisibility.PUB, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestManifestGoodsDescription()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.ManifestGoodsDescription, "Manifest Goods Description", StmNoteVisibility.PUB, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestStackLogDebug()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.StackLogDebug, "Business Object Creation Stack Log", StmNoteVisibility.INT, !IsOnlyOneAllowed, IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestOpportunityFollowUpNote()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.OpportunityFollowUpNote, "Opportunity Follow Up Note", StmNoteVisibility.INT, !IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestReceiveConfirmationInstructionsNote()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.ReceiveConfirmationInstructionsNote, "Receive Confirmation Instructions", StmNoteVisibility.PUB, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestRTUSRequestLogNote()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.RTUSRequestLog, "RTUS Request Log", StmNoteVisibility.PRV, !IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestFullJobRoleDescriptionNote()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.FullJobRoleDescription, "Full Job Role Description", StmNoteVisibility.PUB, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestCustomsInstructionNotes()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.CustomsInstructionNotes, "Customs Instruction Notes", StmNoteVisibility.PUB, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		[ExpectNoExceptions]
		public void TestAllNoteTypesAreValid()
		{
			foreach (PropertyInfo info in typeof(PredefinedNoteTypes).GetProperties())
			{
				if (typeof(PredefinedNoteType).IsAssignableFrom(info.PropertyType))
				{
					info.GetValue(PredefinedNoteTypes.Instance, null);
				}
			}
		}

		public void TestEXDOCLetterOfCredit()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.EXDOCLetterOfCredit, "EXDOC Letter Of Credit", StmNoteVisibility.PRV, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestEXDOCAdditionalInformation()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.EXDOCAdditionalInformation, "EXDOC Additional Information", StmNoteVisibility.PRV, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestEXDOCNotifyText()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.EXDOCNotifyText, "EXDOC Notify Text", StmNoteVisibility.PRV, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestEXDOCAmendmentReason()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.EXDOCAmendmentReason, "EXDOC Amendment Reason", StmNoteVisibility.PRV, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestNEXDOCCancellationReason()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.NEXDOCCancellationReason, "NEXDOC Cancellation Reason", StmNoteVisibility.PRV, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestCarrierBookingRequest()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.CarrierBookingRequest, "Carrier Booking Request", StmNoteVisibility.PUB, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestWebUserNote()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.WebUserNote, "Web User Note", StmNoteVisibility.PUB, IsOnlyOneAllowed, IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestCustomsMessageToPrintOn7501()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.CustomsMessageToPrintOn7501, "Message to Print on 7501", StmNoteVisibility.INT, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestCustomsMessageToPrintOnB3()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.CustomsMessageToPrintOnB3, "Message to Print on B3 Document", StmNoteVisibility.INT, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestCustomsMessageToPrintOnCAD()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.CustomsMessageToPrintOnCAD, "Message to Print on CAD Document", StmNoteVisibility.INT, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestSGTradersRemarks()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.SGTradersRemarks, "SG Traders Remarks", StmNoteVisibility.INT, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestSGMarksAndNumbers()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.SGMarksAndNumbers, "SG Marks and Numbers", StmNoteVisibility.INT, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestSGCertificateItemDesc()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.SGCertificateItemDesc, "SG Certificate Item Description", StmNoteVisibility.INT, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestTradeLaneChargeInformation()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.TradeLaneChargeInformation, "Trade Lane Charge Information", StmNoteVisibility.PUB, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestTradeLaneChargeInternalNote()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.TradeLaneChargeInternalNote, "Trade Lane Charge Internal Note", StmNoteVisibility.INT, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestAllocationLog()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.AllocationLog, "Allocation Log", StmNoteVisibility.PRV, IsOnlyOneAllowed, IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestAutoRatingAuditLog()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.AutoRatingAuditLog, "AutoRating Log", StmNoteVisibility.PRV, IsOnlyOneAllowed, IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestAutoRatingAuditLogMaxLength()
		{
			AssertEquals(Math.Pow(2, 19), (double)PredefinedNoteTypes.Instance.AutoRatingAuditLog_MaxLength);
		}

		public void TestAWBRatelineOvertypedNotes()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.AWBRatelineOvertypedNotes, "AWB Rateline Overtyped Notes", StmNoteVisibility.PUB, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestAdditionalSecurityInformation()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.AdditionalSecurityInformation, "Additional Security Information", StmNoteVisibility.PUB, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestUnrecognisedAdditionalReferenceTypes()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes, "Unrecognized Additional Reference Types", StmNoteVisibility.INT, IsOnlyOneAllowed, IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestPortMessageRemarks()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.PortMessageRemarks, "Port Message Remarks", StmNoteVisibility.INT, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestAdditionalBillClauses()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.AdditionalBillClauses, "Additional Bill Clauses", StmNoteVisibility.PUB, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestBookingNotes()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.BookingNotes, "Booking Notes", StmNoteVisibility.PUB, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, !IsTextOnly);
		}

		public void TestAIRSValidationResults()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.AIRSValidationResults, "AIRS Validation Results", StmNoteVisibility.INT, !IsOnlyOneAllowed, IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestCountryRules()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.CountryRules, "Country Rules", StmNoteVisibility.PUB, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.CountryRulesInternal, "Country Rules Internal", StmNoteVisibility.INT, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestZAEndorsement()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.ZAEndorsement, "Endorsement", StmNoteVisibility.INT, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestZAVOCReason()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.ZAVOCReason, "VOC Reason", StmNoteVisibility.INT, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestWineDetailsComments()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.WineDetailsComments, "Wine Details Comments", StmNoteVisibility.PRV, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestInwardProcessingAdditionalInformation()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.InwardProcessingAdditionalInformation, "Inward Processing Additional Information", StmNoteVisibility.PRV, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestInwardProcessingDescription()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.InwardProcessingDescription, "Inward Processing Description", StmNoteVisibility.PRV, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestNCTSEventCancellationReason()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.NCTSEventCancellationReason, "NCTS Event Cancellation Reason", StmNoteVisibility.PRV, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestOriginalBillNotes()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.OriginalBillNotes, "Original Bill Notes", StmNoteVisibility.INT, IsOnlyOneAllowed, IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestPenaltyExemptionReason()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.PenaltyExemptionReason, "KR Import Penalty Exemption Reasons", StmNoteVisibility.PRV, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestProfitShareRedistributionAuditLog()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.ProfitShareRedistributionAuditLog, "Profit Share Redistribution Log", StmNoteVisibility.PRV, IsOnlyOneAllowed, IsReadOnlyAfterAdd, IsTextOnly);
		}

		#region Pro Forma Invoice

		public void TestAdditionalInformation()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.AdditionalInformation, "Additional Information", StmNoteVisibility.PRV, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestExportersBankName()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.ExportersBankName, "Exporters Bank Name", StmNoteVisibility.PRV, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestExportersBankAccountNo()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.ExportersBankAccountNo, "Exporters Bank Account No", StmNoteVisibility.PRV, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestExportersBankSWIFTCode()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.ExportersBankSWIFTCode, "Exporters Bank SWIFT Code", StmNoteVisibility.PRV, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestMethodOfPayment()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.MethodOfPayment, "Method of Payment", StmNoteVisibility.PRV, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestPickupInstructions()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.PickupInstructions, "Pickup Instructions", StmNoteVisibility.PUB, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestDeliveryInstructions()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.DeliveryInstructions, "Delivery Instructions", StmNoteVisibility.PUB, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		public void TestMessageInterpretationmaxLength()
		{
			AssertEquals(Math.Pow(2, 30) - 1, (double)PredefinedNoteTypes.Instance.MessageInterpretation_MaxLength);
		}

		#endregion

		public void TestCustomsSpecialMentions()
		{
			AssertPredefinedNoteType(PredefinedNoteTypes.Instance.CustomsSpecialMentions, "Customs Special Mentions", StmNoteVisibility.PRV, IsOnlyOneAllowed, !IsReadOnlyAfterAdd, IsTextOnly);
		}

		#endregion

		#region Custom Notes

		public void TestAllNoteTypesContainCustomDefined()
		{
			// This XML is generated based on the sample data from CustomNoteTypeRegistryItem in Enterprise.Registry.Business. To see its structure, look in that solution.
			string xmlForCustomNoteTypes = "<?xml version=\"1.0\" encoding=\"utf-16\"?><CustomNoteTypes><NoteModuleAndCountryList xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><CustomNoteModuleAndCountry><ModuleIDName>ModuleID2</ModuleIDName><CountryCode>AU</CountryCode><CustomNoteTypesList><CustomNoteTypeItem><IsTextOnly>Y</IsTextOnly><IsAppendingNote>N</IsAppendingNote><IsReadOnlyAfterAdd>N</IsReadOnlyAfterAdd><ForceRead>Y</ForceRead><DefaultVisibility>DOC</DefaultVisibility><NoteName>item1Module2</NoteName></CustomNoteTypeItem><CustomNoteTypeItem><IsTextOnly>N</IsTextOnly><IsAppendingNote>N</IsAppendingNote><IsReadOnlyAfterAdd>Y</IsReadOnlyAfterAdd><ForceRead>Y</ForceRead><DefaultVisibility>PUB</DefaultVisibility><NoteName>item1Module1</NoteName></CustomNoteTypeItem></CustomNoteTypesList></CustomNoteModuleAndCountry><CustomNoteModuleAndCountry><ModuleIDName /><CountryCode /><CustomNoteTypesList><CustomNoteTypeItem><IsTextOnly>N</IsTextOnly><IsAppendingNote>N</IsAppendingNote><IsReadOnlyAfterAdd>N</IsReadOnlyAfterAdd><ForceRead>N</ForceRead><DefaultVisibility>PUB</DefaultVisibility><NoteName>item2Module1</NoteName></CustomNoteTypeItem></CustomNoteTypesList></CustomNoteModuleAndCountry></NoteModuleAndCountryList></CustomNoteTypes>";
			byte[] result = Encoding.Unicode.GetBytes(xmlForCustomNoteTypes);
			BinaryRegistryItem item = new BinaryRegistryItem("CustomNotes", null, null, null, RegistryStorageFlags.System);

			PredefinedNoteTypes noteTypes = PredefinedNoteTypes.Instance;

			try
			{
				noteTypes.ClearCacheOfAllNotes();
				((IRegistryItemInternals)CustomNotesProvider.Instance.RegistryItem).ClearCache();
				item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, result);

				AssertNotNull("item1Module1 is a custom defined note type", SearchForNoteTypeInListOfAll(noteTypes, "item1Module1"));
				AssertNotNull("item2Module1 is NOT a custom defined note type (not part of this country)", SearchForNoteTypeInListOfAll(noteTypes, "item2Module1"));
				AssertNotNull("item1Module2 is a custom defined note type", SearchForNoteTypeInListOfAll(noteTypes, "item1Module2"));
			}
			finally
			{
				item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, null);
				((IRegistryItemInternals)CustomNotesProvider.Instance.RegistryItem).ClearCache();
			}
		}

		PredefinedNoteType SearchForNoteTypeInListOfAll(PredefinedNoteTypes noteTypes, string itemNameToFind)
		{
			return Array.Find(noteTypes.All, delegate (PredefinedNoteType currentSearchItem)
			{
				return currentSearchItem.Description == itemNameToFind;
			}
				);
		}

		#endregion

		public void TestNoteTypeByDescriptionWorksInAnyLanguage()
		{
			CombineAssertions(() =>
			{
				foreach (var language in DataFile.GetAvailableLanguages())
				{
					using (Res.TemporarilySwitchLanguage(language))
					{
						foreach (var noteType in PredefinedNoteTypes.Instance.All)
						{
							AssertEquals(noteType.Description + " in " + language, noteType, PredefinedNoteTypes.Instance.NoteTypeByDescription(noteType.Description));
						}
					}
				}
			});
		}

		public void TestGetByDescriptionWorksAfterLanguageValuesChange()
		{
			using (var mock = Res.UseMockData())
			{
				mock.SetResourceGetter(delegate (string key)
				{ return new ResourceStringData(key, "foo"); });
				AssertNotNull(PredefinedNoteTypes.Instance.NoteTypeByDescription(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description));
				mock.SetResourceGetter(delegate (string key)
				{ return new ResourceStringData(key, "bar"); });
				AssertNotNull(PredefinedNoteTypes.Instance.NoteTypeByDescription(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description));
			}
		}

		public void TestInSyncWithCargoWiseDefinitions()
		{
			// ClearCacheOfAllNotes would remove all custom types, which helps the Instance get back to initial state and be ready for this test.
			PredefinedNoteTypes.Instance.ClearCacheOfAllNotes();
			var typesInDefinitions = NoteTypes.AllPredefinedNoteTypes.ToDictionary(x => x.Description, x => x);
			var typesInEnterprise = PredefinedNoteTypes.Instance.All.ToDictionary(x => x.Description, x => x);
			var typesNotInDefinitions = PredefinedNoteTypes.Instance.All.Where(x => !typesInDefinitions.ContainsKey(x.Description));
			var typesNotInEnterprise = NoteTypes.AllPredefinedNoteTypes.Where(x => !typesInEnterprise.ContainsKey(x.Description));
			var error =
$@"Could not find these types in CargoWise.Definitions: {string.Join(", ", typesNotInDefinitions.Select(x => x.Description))}
Could not find these types in PredefinedNoteTypes: {string.Join(", ", typesNotInEnterprise.Select(x => x.Description))}";
			Assert(error, !typesNotInEnterprise.Any() && !typesNotInDefinitions.Any());
		}

		#region Implementation

		void AssertPredefinedNoteType(PredefinedNoteType noteType, string expectedDescription, StmNoteVisibility expectedDefaultVisibility, bool expectedIsOnlyOneAllowed, bool expectedIsReadOnlyAfterAdd, bool expectedIsTextOnly)
		{
			AssertEquals("Description", expectedDescription, noteType.Description);
			AssertEquals("DefaultVisibility", expectedDefaultVisibility, noteType.DefaultVisibility);
			AssertEquals("IsOnlyOneAllowed", expectedIsOnlyOneAllowed, noteType.IsOnlyOneAllowed);
			AssertEquals("IsReadOnlyAfterAdd", expectedIsReadOnlyAfterAdd, noteType.IsReadOnlyAfterAdd);
			AssertEquals("IsTextOnly", expectedIsTextOnly, noteType.IsTextOnly);
		}

		const bool IsOnlyOneAllowed = true;
		const bool IsReadOnlyAfterAdd = true;
		const bool IsTextOnly = true;

		#endregion
	}
}
