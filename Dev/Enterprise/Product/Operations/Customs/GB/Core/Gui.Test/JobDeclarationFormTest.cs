using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief.CusDec.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Eu = Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.GB.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	public class FormBasherTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var cei1 = declaration.CustomsEntryInstructions.AddNew();
			cei1.CEI_Description = "1";
			var cei2 = declaration.CustomsEntryInstructions.AddNew();
			cei2.CEI_Description = "2";
			var invoiceheader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceheader.InvoiceLines.AddNew();
			Factory.Save();// Without this, we get some really retarded test failures regarding loading of lists. The tedium of such unit tests has led me to this. 
			return new JobDeclarationForm(declaration)
			{
				ControllerID = ControllerIDs.Customs.JobDeclaration
			};
		}

		public override void TestMinimumSizeNotTooBig()
		{
			var declaration = Factory.New<JobDeclaration>();
			var minScreenWidthSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1366);
			var minScreenHeightSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(900);
			using (var form = new JobDeclarationForm(declaration))
			{
				Assert("GB Declaration Form min size too wide (" + form.MinimumSize.Width + ") for the screen. Should be less than or equal to " + minScreenWidthSupported, form.MinimumSize.Width <= minScreenWidthSupported);
				Assert("GB Declaration Form min size too high (" + form.MinimumSize.Height + ") for the screen. Should be less than or equal to " + minScreenHeightSupported, form.MinimumSize.Height <= minScreenHeightSupported);
			}
		}
	}

	[TestedType(typeof(JobDeclarationForm))]
	public class ImportJobDeclarationFormTest : EU.GUI.Testing.JobDeclarationFormTest<JobDeclaration>
	{
		public override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;

		protected override JobDeclaration GetPopulatedDeclarationForFormBashingCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cei1 = declaration.CustomsEntryInstructions.AddNew();
			cei1.CEI_Description = "1";
			var cei2 = declaration.CustomsEntryInstructions.AddNew();
			cei2.CEI_Description = "2";
			var invoiceheader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceheader.InvoiceLines.AddNew();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_MessageType = MessageTypeForFormBashing;
			return declaration;
		}

		public void TestBox18DynamicVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			using (var form = new ZForm(declaration))
			using (var control = new JobDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				AssertEquals("Box18TransportIDTextBox should not be shown", false, control.Box18TransportIDTextBox.Visible);
				AssertEquals("Box18TrNationalityFindBox should not be shown", false, control.Box18TrNationalityFindBox.Visible);

				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				AssertEquals("Box18TransportIDTextBox should not be shown", false, control.Box18TransportIDTextBox.Visible);
				AssertEquals("Box18TrNationalityFindBox should not be shown", false, control.Box18TrNationalityFindBox.Visible);

				declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
				AssertEquals("Box18TransportIDTextBox should be shown", true, control.Box18TransportIDTextBox.Visible);
				AssertEquals("Box18TrNationalityFindBox should be shown", false, control.Box18TrNationalityFindBox.Visible);

				declaration.JE_TransportMode = TransportTypeList.Codes.Road;
				AssertEquals("Box18TransportIDTextBox should not be shown", true, control.Box18TransportIDTextBox.Visible);
				AssertEquals("Box18TrNationalityFindBox should not be shown", false, control.Box18TrNationalityFindBox.Visible);

				declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
				AssertEquals("Box18TransportIDTextBox should not be shown", false, control.Box18TransportIDTextBox.Visible);
				AssertEquals("Box18TrNationalityFindBox should not be shown", false, control.Box18TrNationalityFindBox.Visible);

				declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
				AssertEquals("Box18TransportIDTextBox should not be shown", false, control.Box18TransportIDTextBox.Visible);
				AssertEquals("Box18TrNationalityFindBox should not be shown", false, control.Box18TrNationalityFindBox.Visible);

				declaration.JE_TransportMode = TransportTypeList.Codes.OwnPropulsion;
				AssertEquals("Box18TransportIDTextBox should not be shown", true, control.Box18TransportIDTextBox.Visible);
				AssertEquals("Box18TrNationalityFindBox should not be shown", false, control.Box18TrNationalityFindBox.Visible);

				declaration.JE_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
				AssertEquals("Box18TransportIDTextBox should not be shown", true, control.Box18TransportIDTextBox.Visible);
				AssertEquals("Box18TrNationalityFindBox should not be shown", false, control.Box18TrNationalityFindBox.Visible);
			}
		}

		public void TestLocationOfGoodsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (var form = new ZForm(declaration))
			using (var control = new JobDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Port");

				var code = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "AU_AIRPORT", "AU AIRPORT1", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
				var attribute = helper.CreateCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Facility, "AU");
				Factory.Save();
				var subLocationDropEditControl = (ZDropEdit)control.Controls.Find("SubLocationDropEdit", true).Single();

				declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				AssertEquals("GoodsLocationDropEdit should not be shown", false, control.GoodsLocationDropEdit.Visible);
				AssertEquals("SubLocationDropEdit should not be shown", false, subLocationDropEditControl.Visible);
				AssertEquals("LocationOfGoodsUserControl should be shown", true, control.LocationOfGoodsUserControl.Visible);

				declaration.JE_Calc_LocationOtherInformationType = "BU";
				AssertEquals("LocationTextBox should be shown", true, control.LocationOfGoodsUserControl.LocationTextBox.Visible);
				AssertEquals("CDSGoodsLocationDropEdit should not be shown", false, control.LocationOfGoodsUserControl.CDSGoodsLocationDropEdit.Visible);

				declaration.JE_Calc_LocationOtherInformationType = "AU";
				AssertEquals("LocationTextBox should not be shown", false, control.LocationOfGoodsUserControl.LocationTextBox.Visible);
				AssertEquals("CDSGoodsLocationDropEdit should be shown", true, control.LocationOfGoodsUserControl.CDSGoodsLocationDropEdit.Visible);

				declaration.JE_LocationQualifier = "CW";
				AssertEquals("LocationTextBox should be shown", true, control.LocationOfGoodsUserControl.LocationTextBox.Visible);
				AssertEquals("CDSGoodsLocationDropEdit should not be shown", false, control.LocationOfGoodsUserControl.CDSGoodsLocationDropEdit.Visible);
			}
		}

		public void TestGbMessageChangedStatusDeterminerToDictateWhetherSavingAllowed()
		{
			string expectedPopupMessagePulledFromEuGui = @"The changes cannot be saved. The changes you made would affect messaging and would mean that the pending response is out-of-date.
To ensure that the response can be applied to the declaration in the same state in which it was at time of sending, the changes you have made have been denied.
Either close without saving and await the response, or if you really wish to modify the data first set the entry as 'failed from transmission'.
Note that if the original outgoing message was accepted you may be unable to re-transmit without changing reference numbers.";

			var dec = DeclarationChosererTester.CreateGemsDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory); // In Chief.Test

			dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			//dec.Invoices.AddNew().JobComInvoiceLines.AddNew();
			dec.DoMerge();
			var entry = dec.CustomsEntryHeaders[0];
			entry.CH_JE = dec.PK;
			entry.CH_EntryStatus = "RTH";
			// All this below just because CusEntryHeader.EntryNumber = "something" doesn't work 'properly'!
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryNum = "something";
			entryNumber.Parent = entry;
			entryNumber.CE_EntryType = "EXP";
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			var sentMessage = entry.Messages.AddNew();
			sentMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			sentMessage.MessageNumberStrategy = new Business.GbMessageNumberStrategy(sentMessage.Factory, "AGI");  // Bah, this is such a pain. Reference to GB.Chief needed
			sentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sentMessage.EM_Status = EDIMessage.Status.Sent;
			var receivedMessage = entry.Messages.AddNew();
			receivedMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			receivedMessage.MessageNumberStrategy = new Business.GbMessageNumberStrategy(sentMessage.Factory, "DAN");  // Bah, this is such a pain. Reference to GB.Chief needed
			receivedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			receivedMessage.EM_Status = EDIMessage.Status.Received;
			Factory.Save();

			using (var decForm = new JobDeclarationFormTest(dec))
			{
				decForm.ControllerID = ControllerIDs.Customs.JobDeclaration;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				decForm.PressSaveButton();
				AssertEquals("Should be no popup as saving is allowed", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedPopupMessagePulledFromEuGui));
			}

			// Remove the received message so it looks like we're waiting for a reply
			receivedMessage.EM_Status = "QUE";
			receivedMessage.EM_LinkedObject = null;
			Factory.Save();
			// Now reload in a new facotry to avoid caching
			var factory2 = new BusinessObjectFactory();
			var receivedMessageReloaded = factory2.Load<EDIMessage>(receivedMessage.PK);
			var entryReloaded = factory2.Load<Eu.CusEntryHeader>(entry.PK);
			var decReloaded = factory2.Load<JobDeclaration>(dec.PK);
			entryReloaded.Messages.Load(); // damn caching

			using (var decForm = new JobDeclarationFormTest(decReloaded))
			{
				decForm.ControllerID = ControllerIDs.Customs.JobDeclaration;
				decReloaded.SubLocation = "XXX"; // a change sufficient to deny saving
				decReloaded.JE_DeclarationType = "YYY";  // a fairly serious change, but it should not cause an explosion
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				decForm.PressSaveButton(); // should have popup
				AssertEquals("Should be a popup as saving is not allowed", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedPopupMessagePulledFromEuGui));
			}

			entryReloaded.CH_EntryStatus = MessageStatusList.Codes.FailedFromTransmission;
			entryReloaded.Messages.Load(); // damn caching
			using (var decForm = new JobDeclarationFormTest(decReloaded))
			{
				decForm.ControllerID = ControllerIDs.Customs.JobDeclaration;
				decReloaded.JE_DeclarationType = "YYX";  // a fairly serious change, but it should not cause an explosion
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				decForm.PressSaveButton(); // should have popup
				AssertEquals("Should be no popup as saving is allowed", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedPopupMessagePulledFromEuGui));
			}
		}

		public void TestMAWBContainsAlpha()
		{
			var consolParent = Factory.New<ForwardingConsol>();
			consolParent.JK_TransportMode = "AIR";
			consolParent.JK_RL_NKLoadPort = "AUSYD";
			consolParent.JK_RL_NKDischargePort = "GBLHR";
			consolParent.JK_MasterBillNum = "DRT12345678";

			var importShipment = consolParent.Shipments.AddNew();
			importShipment.JS_RL_NKOrigin = "AUSYD";
			importShipment.JS_RL_NKDestination = "GBLHR";
			importShipment.JS_TransportMode = "AIR";
			var importDeclaration = Factory.New<JobDeclaration>();
			importDeclaration.JE_JS = importShipment.PK;
			importDeclaration.JE_MessageType = "IMP";
			importDeclaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			using (var decForm = new JobDeclarationFormTest(importDeclaration))
			{
				decForm.Show();
				decForm.ControllerID = ControllerIDs.Customs.JobDeclaration;
				var control = decForm.CustomsBrokerageUserControl;
				var data = control.DeclarationUserControlForTesting.JE_MasterBillForAirBoundTextBox.MasterBillText;

				AssertEquals("JE_MasterBillForAirBoundTextBox control should allow alpha-numeric data", "DRT1234 5678", data);
			}
		}

		public void TestDeclarationTypeVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();

			Factory.Save();
			using (var form = new ZForm(declaration))
			using (var control = new JobDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();
				declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				AssertEquals("Declaration Type should not be shown", false, control.DeclarationTypeDropEdit.Visible);
			}
		}

		class JobDeclarationFormTest : JobDeclarationForm
		{
			public JobDeclarationFormTest(JobDeclaration dec)
				: base(dec)
			{
			}

			public void PressSaveButton()
			{
				base.ShowPreSaveDialogs();
			}
		}
	}

	[TestedType(typeof(JobDeclarationForm))]
	public class ImportJobDeclarationFormTest_ForWhenDeclarationCancelled : EU.GUI.Testing.JobDeclarationFormTest_ForWhenDeclarationCancelled<JobDeclaration>
	{
		public override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;
	}

	public class JobDeclarationFormPerformanceTest : EU.GUI.Testing.JobDeclarationFormPerformanceTest
	{
		protected override ZForm GetForm(BusinessObject bizO) => new JobDeclarationForm((JobDeclaration)bizO);

		Dictionary<string, int> GBBaseLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>();
		Dictionary<string, int> GBBaseValidateAllExpectedHits => new Dictionary<string, int>
		{
			{ OrgCusCodeSchema.Constants.TableName, 9 },
			{ RefPacksSchema.Constants.TableName, 6 },
			{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 5 }
		};
		Dictionary<string, int> GBBaseLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>
		{
			{ OrgCusCodeSchema.Constants.TableName, 9 },
			{ CusSupportingInfoSchema.Constants.TableName, 19 },
			{ RefPacksSchema.Constants.TableName, 6 }
		};
		Dictionary<string, int> GBBaseFormMergeExpectedHits => new Dictionary<string, int>();
		Dictionary<string, int> GBBaseUniversalXMLExportExpectedHits => new Dictionary<string, int>();

		Dictionary<string, int> GBBaseUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>
		{
			{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 7 },
		};
		Dictionary<string, int> GBBaseUniversalXMLAddExpectedHits => new Dictionary<string, int>()
		{
			{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 5 }
		};
		Dictionary<string, int> GBBaseDeleteExpectedHits => new Dictionary<string, int>()
		{
			{ StmDocDataOverrideSchema.Constants.TableName, 8 }
		};

		protected virtual Dictionary<string, int> GBLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> GBValidateAllExpectedHits => new Dictionary<string, int>
		{
			{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 6 },
			{ "NonPersitentTable DBO.ZZREFCUSCODELISTCOMBINED LEFT JOIN REFDATABASE_REFCUSCODELISTLANGUAGE ON ZXA_ZZD_CODELIST = ZZD_PK AND ZXA_ZX6_NKLANGUAGE = @LANGUAGE", 5 }
		};
		protected virtual Dictionary<string, int> GBLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>
		{
			{ "NonPersitentTable DBO.ZZREFCUSCODELISTCOMBINED LEFT JOIN REFDATABASE_REFCUSCODELISTLANGUAGE ON ZXA_ZZD_CODELIST = ZZD_PK AND ZXA_ZX6_NKLANGUAGE = @LANGUAGE", 5 }
		};
		protected virtual Dictionary<string, int> GBFormMergeExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> GBUniversalXMLExportExpectedHits => new Dictionary<string, int>()
		{
			{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 5 },
			{ CusAuthorizationUsageSchema.Constants.TableName, 11 },
			{ "NonPersitentTable DBO.ZZREFCUSCODELISTCOMBINED LEFT JOIN REFDATABASE_REFCUSCODELISTLANGUAGE ON ZXA_ZZD_CODELIST = ZZD_PK AND ZXA_ZX6_NKLANGUAGE = @LANGUAGE", 7 }
		};

		protected virtual Dictionary<string, int> GBUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> GBUniversalXMLAddExpectedHits => new Dictionary<string, int>()
		{
			{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 7 },
			{ RefCusProcedureSchema.Constants.TableName, 5 },
			{ "NonPersitentTable DBO.ZZREFCUSCODELISTCOMBINED LEFT JOIN REFDATABASE_REFCUSCODELISTLANGUAGE ON ZXA_ZZD_CODELIST = ZZD_PK AND ZXA_ZX6_NKLANGUAGE = @LANGUAGE", 7 }
		};
		protected virtual Dictionary<string, int> GBDeleteExpectedHits => new Dictionary<string, int>();

		protected override Dictionary<string, int> EULoadEditableChildObjectsExpectedHits => ZipDictionaries(GBBaseLoadEditableChildObjectsExpectedHits, GBLoadEditableChildObjectsExpectedHits);
		protected override Dictionary<string, int> EUValidateAllExpectedHits => ZipDictionaries(GBBaseValidateAllExpectedHits, GBValidateAllExpectedHits);
		protected override Dictionary<string, int> EULightFormValidationAndSaveExpectedHits => ZipDictionaries(GBBaseLightFormValidationAndSaveExpectedHits, GBLightFormValidationAndSaveExpectedHits);
		protected override Dictionary<string, int> EUFormMergeExpectedHits => ZipDictionaries(GBBaseFormMergeExpectedHits, GBFormMergeExpectedHits);
		protected override Dictionary<string, int> EUUniversalXMLExportExpectedHits => ZipDictionaries(GBBaseUniversalXMLExportExpectedHits, GBUniversalXMLExportExpectedHits);
		protected override Dictionary<string, int> EUUniversalXMLImportUpdateExpectedHits => ZipDictionaries(GBBaseUniversalXMLImportUpdateExpectedHits, GBUniversalXMLImportUpdateExpectedHits);
		protected override Dictionary<string, int> EUUniversalXMLAddExpectedHits => ZipDictionaries(GBBaseUniversalXMLAddExpectedHits, GBUniversalXMLAddExpectedHits);
		protected override Dictionary<string, int> EUDeleteExpectedHits => ZipDictionaries(GBBaseDeleteExpectedHits, GBDeleteExpectedHits);

		protected override void DecorateDeclaration(BaseJobDeclaration declaration)
		{
			base.DecorateDeclaration(declaration);
			var gbDeclaration = (JobDeclaration)declaration;
			gbDeclaration.JE_ApplicationCode = "CDS";
			gbDeclaration.ZG_ManualCalc = true;
		}
	}

	public class ImportJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;
		protected override Dictionary<string, int> GBUniversalXMLExportExpectedHits => new Dictionary<string, int>()
		{
			{ CusAuthorizationUsageSchema.Constants.TableName, 11 },
			{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 5 },
			{ "NonPersitentTable DBO.ZZREFCUSCODELISTCOMBINED LEFT JOIN REFDATABASE_REFCUSCODELISTLANGUAGE ON ZXA_ZZD_CODELIST = ZZD_PK AND ZXA_ZX6_NKLANGUAGE = @LANGUAGE", 7 }
		};

		protected override Dictionary<string, int> GBLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>()
		{
			{ "NonPersitentTable DBO.ZZREFCUSCODELISTCOMBINED LEFT JOIN REFDATABASE_REFCUSCODELISTLANGUAGE ON ZXA_ZZD_CODELIST = ZZD_PK AND ZXA_ZX6_NKLANGUAGE = @LANGUAGE", 5 }
		};

		protected override Dictionary<string, int> GBValidateAllExpectedHits => new Dictionary<string, int>()
		{
			{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 6 },
			{ "NonPersitentTable DBO.ZZREFCUSCODELISTCOMBINED LEFT JOIN REFDATABASE_REFCUSCODELISTLANGUAGE ON ZXA_ZZD_CODELIST = ZZD_PK AND ZXA_ZX6_NKLANGUAGE = @LANGUAGE", 5 }
		};

		protected override Dictionary<string, int> GBUniversalXMLAddExpectedHits => new Dictionary<string, int>()
		{
			{ RefCusProcedureSchema.Constants.TableName, 5 },
			{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 9 }
		};
	}

	public class CancelledImportJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;

		protected override bool DeclarationIsCancelled => true;

		protected override Dictionary<string, int> GBUniversalXMLExportExpectedHits => new Dictionary<string, int>()
		{
			{ CusAuthorizationUsageSchema.Constants.TableName, 11 },
			{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 5 },
			{ "NonPersitentTable DBO.ZZREFCUSCODELISTCOMBINED LEFT JOIN REFDATABASE_REFCUSCODELISTLANGUAGE ON ZXA_ZZD_CODELIST = ZZD_PK AND ZXA_ZX6_NKLANGUAGE = @LANGUAGE", 7 }
		};

		protected override Dictionary<string, int> GBValidateAllExpectedHits => new Dictionary<string, int>()
		{
			{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 6 },
			{ "NonPersitentTable DBO.ZZREFCUSCODELISTCOMBINED LEFT JOIN REFDATABASE_REFCUSCODELISTLANGUAGE ON ZXA_ZZD_CODELIST = ZZD_PK AND ZXA_ZX6_NKLANGUAGE = @LANGUAGE", 5 }
		};

		protected override Dictionary<string, int> GBLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>()
		{
			{ "NonPersitentTable DBO.ZZREFCUSCODELISTCOMBINED LEFT JOIN REFDATABASE_REFCUSCODELISTLANGUAGE ON ZXA_ZZD_CODELIST = ZZD_PK AND ZXA_ZX6_NKLANGUAGE = @LANGUAGE", 5 }
		};
	}
}
