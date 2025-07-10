using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using AsycudaManifestHeader = Enterprise.Customs.ASYCUDAManifest.Business.AsycudaManifestHeader;
using C = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ASYCUDAManifest.GUI.Testing
{
	sealed class MenuBuilderTest : TestCaseWithFactory
	{
		public void TestCannotSendWithoutCustomsOfficeCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList(C.RefDataGrouping.Codes.CommonDataGrouping, C.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.SolomonIslands, Core.Constants.CountryCodes.SolomonIslands, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var sbHIRS = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SolomonIslands, C.RefCusCodeListTypes.Codes.CustomsOffice, "HIRS", "Honiara Point Cruz Seaport", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sbHIRS.PK, "SEA", "SBHIR");

			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var cusCodeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SolomonIslands, C.RefCusCodeListTypes.Codes.ManifestValidationRule, C.RefCusCodeList.ManifestValidationRuleCodes.OfficeCode, "A Customs Office Code is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(C.RefCusCodeList.ManifestValidationRuleCodes.Mandatory, "Desc.", C.RefCusCodeListTypes.Codes.ManifestValidationRule, Core.Constants.CountryCodes.SolomonIslands, C.RefCusCodeListTypes.Codes.ManifestValidationRule);
			cusCodeList.Attributes.AddNew(C.RefCusCodeList.ManifestValidationRuleCodes.Mandatory, ZString.Empty);

			var wcoDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SolomonIslands, parent: wcoDataGrouping);
			Factory.Save();

			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.Containers.AddNew();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.SolomonIslands;

			var consol = Factory.New<ForwardingConsol>();
			header.SetParent(consol);
			header.AMA_RL_NKPortOfDischarge = "SBHIR";
			header.AMA_JobReference = "I have changes now";
			header.AMA_E_ARV = ZDateTime.Now;

			header.AMA_CustomsOffice = string.Empty;
			header.AMA_ManifestType = "ASY";
			header.Bills.AddNew();
			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors

					menu.MenuItems[0].MenuItems[0].PerformClick();
					AssertContains("Customs Office in Solomon Islands is a critical field", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(0, header.Messages.Count);

					header.AMA_CustomsOffice = ((CodeDescriptionPairList)header.Lookups.CustomsOffices)[0].Code;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors

					menu.MenuItems[0].MenuItems[0].PerformClick();
					AssertEquals(1, header.Messages.Count);

					var eHubMessage = ((IEDIMessageCollectionProvider)header).Messages.LastOutgoingMessage;
					AssertContains("<UniversalShipment", eHubMessage.EM_MessageText);
				}
			}
		}

		public void TestSendManifest_OnlyUxmlDoesStuffForSolomonIslands()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList(C.RefDataGrouping.Codes.CommonDataGrouping, C.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.SolomonIslands, Core.Constants.CountryCodes.SolomonIslands, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var sbHIRS = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SolomonIslands, C.RefCusCodeListTypes.Codes.CustomsOffice, "HIRS", "Honiara Point Cruz Seaport", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sbHIRS.PK, "SEA", "SBHIR");
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.Containers.AddNew();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.SolomonIslands;

			var consol = Factory.New<ForwardingConsol>();
			header.SetParent(consol);
			header.AMA_RL_NKPortOfDischarge = "SBHIR";
			header.AMA_TransportMode = "SEA";
			header.AMA_JobReference = "I have changes now";
			header.AMA_E_ARV = ZDateTime.Now;
			header.Bills.AddNew();
			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
					menu.MenuItems[0].MenuItems[0].PerformClick();
					AssertContains("Manifest Created", UnitTestUserNotification.Instance.LastMessage.Text);
					var eHubMessage = header.Messages[0];
					AssertContains("<UniversalShipment", eHubMessage.EM_MessageText);
				}
			}
		}

		public void TestSendManifest_SaveFirst()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList(C.RefDataGrouping.Codes.CommonDataGrouping, C.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.SolomonIslands, Core.Constants.CountryCodes.SolomonIslands, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SolomonIslands, C.RefCusCodeListTypes.Codes.Facilities, "HONIARA SEAPORT", "Honiara International Seaport", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.Containers.AddNew();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.SolomonIslands;

			var consol = Factory.New<ForwardingConsol>();
			header.SetParent(consol);
			header.AMA_JobReference = "I have changes now";
			header.AMA_E_ARV = ZDateTime.Now;
			header.AMA_RL_NKPortOfDischarge = "SBAFT";
			header.AMA_CustomsOffice = "X";
			header.Bills.AddNew();
			using (var menu = new ASYCUDA.GUI.AsycudaMenu(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);
					var menuToClick = menu.MenuItems[0].MenuItems[0];
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menuToClick.PerformClick();
					AssertContains("save first", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No); // Don't save
					menuToClick.PerformClick();
					AssertContains("save first", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("not saved", true, header.HasChanges);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No); // No, don't proceed with errors
					menuToClick.PerformClick();
					AssertEquals("saved", false, header.HasChanges);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No); // No, don't proceed with errors
					menuToClick.PerformClick();
					AssertContains("Do you want to send the message(s) despite these errors", UnitTestUserNotification.Instance.LastMessage.Text); // User not asked if they want to save, as a save is no necessary
				}
			}
		}

		public void TestSendManifest_ProceedWithErrors()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			var erCountry = helper.CreateNewOrGetExistingCusCodeList(C.RefDataGrouping.Codes.CommonDataGrouping,
				C.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Eritrea,
				Core.Constants.CountryCodes.Eritrea, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(erCountry.PK, RefCusCodeListAttributeTypes.Codes.NVC, "15.3.29.001");
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "AUSYD";
			header.AMA_RL_NKPortOfDischarge = "ERASA";
			header.AMA_JobReference = "I have changes now";
			header.AMA_CustomsOffice = "Y";
			header.Bills.AddNew();
			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);
					// UXML
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
					menu.MenuItems[0].PerformClick();
					AssertContains("Manifest Created", UnitTestUserNotification.Instance.LastMessage.Text);
					var eHubMessage = header.Messages[0];
					AssertContains("<UniversalShipment", eHubMessage.EM_MessageText);
				}
			}
		}

		public void TestSendManifest_ForBangladeshManifestNoCCDAndCCCPresent()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Bangladesh))
			{
				var header = CreateHeader();
				Factory.Save();

				AssertMenuItemClickWithPresetResponse(header, new[] { DialogResult.No },
					() => { AssertEquals("Should no message generated", 0, header.Messages.Count); });

				AssertMenuItemClickWithPresetResponse(header, new[] { DialogResult.Yes }, () =>
				{
					AssertContains("Manifest Created", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertContains("<UniversalShipment", header.Messages[0].EM_MessageText);
				});
			}
		}

		public void TestSendManifest_ForBangladeshManifestNoCCDAndCCCPresent_MultiConfirmation()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Bangladesh))
			{
				var header = CreateMockHeader();
				Factory.Save();

				AssertMenuItemClickWithPresetResponse(header, new[] { DialogResult.No, DialogResult.No },
					() => { AssertEquals("Should no message generated", 0, header.Messages.Count); });

				AssertMenuItemClickWithPresetResponse(header, new[] { DialogResult.Yes, DialogResult.No },
					() => { AssertEquals("Should no message generated", 0, header.Messages.Count); });

				AssertMenuItemClickWithPresetResponse(header, new[] { DialogResult.Yes, DialogResult.Yes }, () =>
				{
					AssertContains("Manifest Created", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertContains("<UniversalShipment", header.Messages[0].EM_MessageText);
				});
			}
			ErrorReporter.Clear();
		}

		public void TestSendManifest_ForBangladeshManifestWithCCDPresent()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Bangladesh))
			{
				var header = CreateHeader();
				CreateOrgCusCode(GlbBranch.CurrentBranch.OrgProxy.PK, OrgCusCode.CodeTypes.CustomsClientCode);
				Factory.Save();

				AssertMenuItemClickWithPresetResponse(header, Array.Empty<DialogResult>(), () =>
				{
					AssertContains("Manifest Created", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertContains("<UniversalShipment", header.Messages[0].EM_MessageText);
				});
			}
		}

		public void TestSendManifest_ForBangladeshManifestWithCCCPresent()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Bangladesh))
			{
				var header = CreateHeader();
				CreateOrgCusCode(GlbBranch.CurrentBranch.OrgProxy.PK, OrgCusCode.CodeTypes.CarrierCode);
				Factory.Save();

				AssertMenuItemClickWithPresetResponse(header, Array.Empty<DialogResult>(), () =>
				{
					AssertContains("Manifest Created", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertContains("<UniversalShipment", header.Messages[0].EM_MessageText);
				});
			}
		}

		static void AssertMenuItemClickWithPresetResponse(AsycudaManifestHeader header, IEnumerable<DialogResult> presetResponse, Action action)
		{
			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				foreach (var dialogResult in presetResponse)
				{
					UnitTestUserNotification.Instance.AddAnswer(dialogResult);
				}
				menu.MenuItems[0].MenuItems[0].PerformClick();
				action();
			}
		}

		AsycudaManifestHeader CreateHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			SetupHeader(header);
			return header;
		}

		AsycudaManifestHeader CreateMockHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderForMultiConfirmationForTest>();
			SetupHeader(header);
			return header;
		}

		void SetupHeader(AsycudaManifestHeader header)
		{
			var consol = Factory.New<ForwardingConsol>();
			header.Containers.AddNew();
			header.SetParent(consol);
			header.AMA_RL_NKPortOfDischarge = "BDHIR";
			header.AMA_JobReference = "I have changes now";
			header.AMA_E_ARV = ZDateTime.Now;
			header.AMA_CustomsOffice = "101";
			header.AMA_ManifestType = "ASY";
			header.Bills.AddNew();
		}

		void CreateOrgCusCode(ZGuid orgHeaderPK, ZString codeType)
		{
			var orgCusCode = Factory.New<OrgCusCode>();
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Bangladesh;
			orgCusCode.OK_CodeType = codeType;
			orgCusCode.OK_CustomsRegNo = "testccd";
			orgCusCode.OK_OH = orgHeaderPK;
		}

		sealed class AsycudaManifestHeaderForMultiConfirmationForTest : AsycudaManifestHeader
		{
			public AsycudaManifestHeaderForMultiConfirmationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override BaseMessageSendingNotificationHelper GetMessageSendingNotificationHelper() => new MessageSendingNotificationHelperForTest(this);
		}

		sealed class MessageSendingNotificationHelperForTest : AsycudaMessageSendingNotificationHelper
		{
			public MessageSendingNotificationHelperForTest(ASYCUDA.Business.AsycudaManifestHeader header) : base(header)
			{
			}

			public override IEnumerable<ZString> GetConfirmations() => new ZString[] { "would you continue 1?", "would you continue 2?" };
		}
	}
}
