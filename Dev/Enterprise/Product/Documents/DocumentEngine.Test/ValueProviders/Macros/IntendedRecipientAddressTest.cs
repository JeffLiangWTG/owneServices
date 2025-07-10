using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(IntendedRecipientAddress))]
	sealed class IntendedRecipientAddressTest : ValueProviderTest
	{
		[TestDate(2008, 8, 8, 8, 8, 8)]
		public void TestOfficialContactWithCoverPageConsistencyInRun()
		{
			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.OfficialContactWithCoverPage);

			var officialOrg = DocumentEngineTestHelper.GetNewOrganizationWithMainAddress("Microsoft", Factory);
			var officialContact = DocumentEngineTestHelper.GetNewDocDeliveryContact("Bill Gates", officialOrg, officialOrg.MainAddress, Factory);
			var contactOrg = DocumentEngineTestHelper.GetNewOrganizationWithMainAddress("Apple", Factory);
			Factory.Save();

			var helper = new ReportPrintSetEndToEndTestHelper(Factory);
			helper.SetTemplate(
@"{A}-[#Config]
{A}-[#SectionBody]
{B}-[<IntendedRecipientAddress>]
{A}-[#EndOfReport]");

			using (var printSet = new ReportPrintSet(helper.MenuItem))
			{
				var report = printSet[0][0] as Report;

				var deliveryInstructions = printSet[0].DeliveryInstructions;
				deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
				deliveryInstructions.Recipients.RemoveAndDeleteAll();
				deliveryInstructions.SetOfficialRecipientForTesting(officialContact);

				var recipient = deliveryInstructions.Recipients.AddNew();
				recipient.Name = "Peter Pan";
				recipient.Email = "peter.pan@cw1.com";
				recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
				recipient.AttachmentType = OrgConstants.AttachmentType.PDF;

				helper.AssertRun(
@"{C}-[Email Cover Sheet]   {S}-[DATE:]   {W}-[08-Aug-08 08:08 AM]
{C}-[ATTENTION]   {G}-[Peter Pan]
{C}-[EMAIL ADDRESS]   {G}-[peter.pan@cw1.com]
{C}-[FROM]   {G}-[CargoWise Support]
{C}-[MESSAGE]", printSet, deliveryInstructions);

				recipient.OrgHeaderPK = contactOrg.PK;
				helper.AssertRun(
@"{C}-[Email Cover Sheet]   {S}-[DATE:]   {W}-[08-Aug-08 08:08 AM]

{C}-[ATTENTION]   {G}-[Peter Pan]
{G}-[AppleCompany]
{C}-[EMAIL ADDRESS]   {G}-[org.address@test.com]

{C}-[FROM]   {G}-[CargoWise Support]

{C}-[MESSAGE]", printSet, deliveryInstructions);

				recipient.OrgHeaderPK = ZGuid.Empty;
				helper.AssertRun(
@"{C}-[Email Cover Sheet]   {S}-[DATE:]   {W}-[08-Aug-08 08:08 AM]
{C}-[ATTENTION]   {G}-[Peter Pan]
{C}-[EMAIL ADDRESS]   {G}-[org.address@test.com]
{C}-[FROM]   {G}-[CargoWise Support]
{C}-[MESSAGE]", printSet, deliveryInstructions);

				recipient.OrgHeaderPK = officialOrg.PK;
				helper.AssertRun(
@"{C}-[Email Cover Sheet]   {S}-[DATE:]   {W}-[08-Aug-08 08:08 AM]
{C}-[ATTENTION]   {G}-[Peter Pan]
{G}-[MicrosoftCompany]
{C}-[EMAIL ADDRESS]   {G}-[org.address@test.com]
{C}-[FROM]   {G}-[CargoWise Support]
{C}-[MESSAGE]", printSet, deliveryInstructions);
			}
		}

		public void TestDeliveryContactWithRedirectToConsistencyInRun()
		{
			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.DeliveryContactWithRedirectTo);

			var officialOrg = DocumentEngineTestHelper.GetNewOrganizationWithMainAddress("Microsoft", Factory);
			var officialContact = DocumentEngineTestHelper.GetNewDocDeliveryContact("Bill Gates", officialOrg, officialOrg.MainAddress, Factory);
			var contactOrg = DocumentEngineTestHelper.GetNewOrganizationWithMainAddress("Apple", Factory);
			Factory.Save();

			var helper = new ReportPrintSetEndToEndTestHelper(Factory);
			helper.SetTemplate(
@"{A}-[#Config]
{A}-[#SectionBody]
{B}-[<IntendedRecipientAddress>]
{A}-[#EndOfReport]");

			using (var printSet = new ReportPrintSet(helper.MenuItem))
			{
				var deliveryInstructions = printSet[0].DeliveryInstructions;
				deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
				deliveryInstructions.Recipients.RemoveAndDeleteAll();
				deliveryInstructions.SetOfficialRecipientForTesting(officialContact);

				var recipient = deliveryInstructions.Recipients.AddNew();
				recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
				recipient.Name = "Peter Pan";
				recipient.Email = "peter.pan@cw1.com";
				recipient.AttachmentType = OrgConstants.AttachmentType.PDF;

				helper.AssertRun(
@"{B}-[MICROSOFTCOMPANY|>MICROSOFTCOMPANY|>MICROSOFTADDRESS1|>MICROSOFTADDRESS2|>MICROSOFTVILLE PA 9MIC9|>UNITED STATES]", printSet, deliveryInstructions);

				recipient.OrgHeaderPK = contactOrg.PK;

				helper.AssertRun(
@"{B}-[MICROSOFTCOMPANY|>APPLECOMPANY|>APPLEADDRESS1|>APPLEADDRESS2|>APPLEVILLE MN 9APP9|>UNITED STATES]", printSet, deliveryInstructions);

				recipient.OrgHeaderPK = ZGuid.Empty;
				helper.AssertRun(
@"{B}-[MICROSOFTCOMPANY|>MICROSOFTCOMPANY|>MICROSOFTADDRESS1|>MICROSOFTADDRESS2|>MICROSOFTVILLE PA 9MIC9|>UNITED STATES]", printSet, deliveryInstructions);

				recipient.Name = "Peter";
				helper.AssertRun(
@"{B}-[MICROSOFTCOMPANY|>MICROSOFTCOMPANY|>MICROSOFTADDRESS1|>MICROSOFTADDRESS2|>MICROSOFTVILLE PA 9MIC9|>UNITED STATES]", printSet, deliveryInstructions);
			}
		}

		public void TestUseOfficialContactAddressWhenDeliveryContactIsFromSameOrganizationWhenUsingDeliveryContactWithRedirectTo()
		{
			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.DeliveryContactWithRedirectTo);

			OrgHeader appleOrg = DocumentEngineTestHelper.GetNewOrganizationWithMainAddress("Apple", Factory);
			DocDeliveryContact appleDeliveryContact = DocumentEngineTestHelper.GetNewDocDeliveryContact("BILL GATES", appleOrg, appleOrg.MainAddress, Factory);
			OrgAddress microsoft = DocumentEngineTestHelper.AddAddress("Microsoft", appleOrg, OrgAddressType.Residential);
			DocDeliveryContact microsoftDeliveryContact = DocumentEngineTestHelper.GetNewDocDeliveryContact("Joe Blow", appleOrg, microsoft, Factory);
			Factory.Save();

			ReportForTesting.MostOfficialContact = appleDeliveryContact;
			ReportForTesting.DeliveryContact = microsoftDeliveryContact;
			string macro = string.Format("<IntendedRecipientAddress>");

			AssertMultilineASCIIEquals("ValueProviderToTest.GetReplacement(macro, Report).ToString()",
@"APPLECOMPANY
APPLECOMPANY
APPLEADDRESS1
APPLEADDRESS2
APPLEVILLE MN 9APP9
UNITED STATES
", ValueProviderToTest.GetReplacement(macro, Report).ToString());
		}

		public void TestGetsActualAddressFromPKSpecifiedToMacro()
		{
			OrgHeader appleOrg = DocumentEngineTestHelper.GetNewOrganizationWithMainAddress("Apple", Factory);
			appleOrg.OH_RL_NKClosestPort = "CNSHA";

			DocDeliveryContact appleDeliveryContact = DocumentEngineTestHelper.GetNewDocDeliveryContact("BILL GATES", appleOrg, appleOrg.MainAddress, Factory);
			OrgAddress microsoft = DocumentEngineTestHelper.AddAddress("Microsoft", appleOrg, OrgAddressType.Residential);
			microsoft.OA_RL_NKRelatedPortCode = "USSEA";
			Factory.Save();

			ReportForTesting.DeliveryContact = appleDeliveryContact;
			string macro = string.Format("<IntendedRecipientAddress({0})>", microsoft.PK.ToString());

			AssertMultilineASCIIEquals("DeliveryContactWithRedirectTo",
@"APPLECOMPANY T/AS MICROSOFT
MICROSOFTADDRESS1
MICROSOFTADDRESS2
MICROSOFTVILLE MIC 9MIC9
UNITED STATES
", ValueProviderToTest.GetReplacement(macro, Report).ToString());
		}

		public void TestPrefixRegistryItemsAffectAddressFormatting()
		{
			DocumentsDataRegistry.Instance.AttentionPrefixForAddressContactNameLineText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ResString.GetMultilingualString("y", "OY!:", "parameter"));
			DocumentsDataRegistry.Instance.RedirectToPrefixForAddressContactNameLineText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ResString.GetMultilingualString("y", "Oy YOU!:", "parameter"));

			DocDeliveryContact billAtBillCompany = DocumentEngineTestHelper.GetNewDocDeliveryContact("Bill", Factory);
			DocDeliveryContact tedAtBillCompany = DocumentEngineTestHelper.GetNewDocDeliveryContact("Ted", billAtBillCompany.OrgHeader, Factory);
			DocDeliveryContact fredAtFredCompany = DocumentEngineTestHelper.GetNewDocDeliveryContact("Fred", Factory);
			Factory.Save();

			string macro = "<IntendedRecipientAddress>";
			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.DeliveryContactWithRedirectTo);

			ReportForTesting.MostOfficialContact = billAtBillCompany;
			ReportForTesting.DeliveryContact = fredAtFredCompany;

			AssertMultilineASCIIEquals("DeliveryContactWithRedirectTo",
@"BILLCOMPANY
FREDCOMPANY
FREDADDRESS1
FREDADDRESS2
FREDVILLE FRE 9FRE9
UNITED STATES
", ValueProviderToTest.GetReplacement(macro, Report).ToString());

			ReportForTesting.DeliveryContact = tedAtBillCompany;

			AssertMultilineASCIIEquals("DeliveryContactWithRedirectTo",
@"BILLCOMPANY
BILLCOMPANY
BILLADDRESS1
BILLADDRESS2
BILLVILLE BIL 9BIL9
UNITED STATES
", ValueProviderToTest.GetReplacement(macro, Report).ToString());
		}

		public void TestPrefixRegistryItemsAffectAddressFormatting_UsingSuffix()
		{
			DocumentsDataRegistry.Instance.AttentionPrefixForAddressContactNameLineText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ResString.GetMultilingualString("y", "{0}: OY!", "parameter"));
			DocumentsDataRegistry.Instance.RedirectToPrefixForAddressContactNameLineText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ResString.GetMultilingualString("y", "{0}: Oy YOU!", "parameter"));

			DocDeliveryContact billAtBillCompany = DocumentEngineTestHelper.GetNewDocDeliveryContact("Bill", Factory);
			DocDeliveryContact tedAtBillCompany = DocumentEngineTestHelper.GetNewDocDeliveryContact("Ted", billAtBillCompany.OrgHeader, Factory);
			DocDeliveryContact fredAtFredCompany = DocumentEngineTestHelper.GetNewDocDeliveryContact("Fred", Factory);
			Factory.Save();

			string macro = "<IntendedRecipientAddress>";
			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.DeliveryContactWithRedirectTo);

			ReportForTesting.MostOfficialContact = billAtBillCompany;
			ReportForTesting.DeliveryContact = fredAtFredCompany;

			AssertMultilineASCIIEquals("DeliveryContactWithRedirectTo",
@"BILLCOMPANY
FREDCOMPANY
FREDADDRESS1
FREDADDRESS2
FREDVILLE FRE 9FRE9
UNITED STATES
", ValueProviderToTest.GetReplacement(macro, Report).ToString());

			ReportForTesting.DeliveryContact = tedAtBillCompany;

			AssertMultilineASCIIEquals("DeliveryContactWithRedirectTo",
@"BILLCOMPANY
BILLCOMPANY
BILLADDRESS1
BILLADDRESS2
BILLVILLE BIL 9BIL9
UNITED STATES
", ValueProviderToTest.GetReplacement(macro, Report).ToString());
		}

		public void TestDoNotRedirectWhenOfficialAndDeliveryOrganizationIsTheSameButAddressIsDifferent()
		{
			DocDeliveryContact billAtBillCompany = DocumentEngineTestHelper.GetNewDocDeliveryContact("Bill", Factory);
			DocDeliveryContact tedAtBillCompany = DocumentEngineTestHelper.GetNewDocDeliveryContact("Ted", billAtBillCompany.OrgHeader, Factory);
			DocDeliveryContact fredAtFredCompany = DocumentEngineTestHelper.GetNewDocDeliveryContact("Fred", Factory);
			Factory.Save();

			string macro = "<IntendedRecipientAddress>";
			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.DeliveryContactWithRedirectTo);

			ReportForTesting.MostOfficialContact = billAtBillCompany;
			ReportForTesting.DeliveryContact = fredAtFredCompany;

			AssertMultilineASCIIEquals("DeliveryContactWithRedirectTo",
@"BILLCOMPANY
FREDCOMPANY
FREDADDRESS1
FREDADDRESS2
FREDVILLE FRE 9FRE9
UNITED STATES
", ValueProviderToTest.GetReplacement(macro, Report).ToString());

			ReportForTesting.DeliveryContact = tedAtBillCompany;

			AssertMultilineASCIIEquals("DeliveryContactWithRedirectTo",
@"BILLCOMPANY
BILLCOMPANY
BILLADDRESS1
BILLADDRESS2
BILLVILLE BIL 9BIL9
UNITED STATES
", ValueProviderToTest.GetReplacement(macro, Report).ToString());
		}

		public void TestUseOfficialContactAddressWhenRegistrySetToOfficialContactWithCoverPage()
		{
			DocDeliveryContact billAtBillCompany = DocumentEngineTestHelper.GetNewDocDeliveryContact("Bill", Factory);
			DocDeliveryContact tedAtBillCompany = DocumentEngineTestHelper.GetNewDocDeliveryContact("Ted", billAtBillCompany.OrgHeader, Factory);
			DocDeliveryContact fredAtFredCompany = DocumentEngineTestHelper.GetNewDocDeliveryContact("Fred", Factory);
			Factory.Save();

			string macro = "<IntendedRecipientAddress>";
			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.OfficialContactWithCoverPage);

			ReportForTesting.MostOfficialContact = billAtBillCompany;
			ReportForTesting.DeliveryContact = fredAtFredCompany;

			AssertMultilineASCIIEquals("DeliveryContactWithRedirectTo",
@"BILLCOMPANY
BILLADDRESS1
BILLADDRESS2
BILLVILLE BIL 9BIL9
UNITED STATES", ValueProviderToTest.GetReplacement(macro, Report).ToString());

			ReportForTesting.DeliveryContact = tedAtBillCompany;

			AssertMultilineASCIIEquals("DeliveryContactWithRedirectTo",
@"BILLCOMPANY
BILLADDRESS1
BILLADDRESS2
BILLVILLE BIL 9BIL9
UNITED STATES", ValueProviderToTest.GetReplacement(macro, Report).ToString());
		}

		public void TestNoOfficialAndNoDeliveryContactWithRedirectTo()
		{
			string macro = "<IntendedRecipientAddress>";

			ReportForTesting.MostOfficialContact = null;
			ReportForTesting.DeliveryContact = null;

			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.OfficialContactOnly);
			AssertMultilineASCIIEquals("OfficialContactOnly",
@"", ValueProviderToTest.GetReplacement(macro, Report).ToString());

			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.OfficialContactWithCoverPage);
			AssertMultilineASCIIEquals("OfficialContactWithCoverPage",
@"", ValueProviderToTest.GetReplacement(macro, Report).ToString());

			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.DeliveryContactWithRedirectTo);
			AssertMultilineASCIIEquals("DeliveryContactWithRedirectTo",
@"", ValueProviderToTest.GetReplacement(macro, Report).ToString());

			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.DeliveryContactOnly);
			AssertMultilineASCIIEquals("DeliveryContactOnly",
@"", ValueProviderToTest.GetReplacement(macro, Report).ToString());
		}

		public void TestNoOfficialAndNoDeliveryContactWithRedirectToAndOrgAddressPK()
		{
			OrgAddress address = DocumentEngineTestHelper.GetNewOrganizationWithMainAddress("Stan", Factory).MainAddress;
			Factory.Save();
			string macro = "<IntendedRecipientAddress(" + address.PK + ")>";

			ReportForTesting.MostOfficialContact = null;
			ReportForTesting.DeliveryContact = null;

			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.OfficialContactOnly);
			AssertMultilineASCIIEquals("OfficialContactOnly",
@"STANCOMPANY
STANADDRESS1
STANADDRESS2
STANVILLE VT 9STA9
UNITED STATES", ValueProviderToTest.GetReplacement(macro, Report).ToString());

			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.OfficialContactWithCoverPage);
			AssertMultilineASCIIEquals("OfficialContactWithCoverPage",
@"STANCOMPANY
STANADDRESS1
STANADDRESS2
STANVILLE VT 9STA9
UNITED STATES", ValueProviderToTest.GetReplacement(macro, Report).ToString());

			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.DeliveryContactWithRedirectTo);
			AssertMultilineASCIIEquals("DeliveryContactWithRedirectTo",
@"STANCOMPANY
STANADDRESS1
STANADDRESS2
STANVILLE VT 9STA9
UNITED STATES", ValueProviderToTest.GetReplacement(macro, Report).ToString());

			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.DeliveryContactOnly);
			AssertMultilineASCIIEquals("DeliveryContactOnly",
@"STANCOMPANY
STANADDRESS1
STANADDRESS2
STANVILLE VT 9STA9
UNITED STATES", ValueProviderToTest.GetReplacement(macro, Report).ToString());
		}

		public void TestNoOfficialAndDeliveryContactNameOnlyWithRedirectTo()
		{
			string macro = "<IntendedRecipientAddress>";

			ReportForTesting.MostOfficialContact = null;
			ReportForTesting.DeliveryContact = new DocDeliveryContact(Factory);
			ReportForTesting.DeliveryContact.Name = "Mr I Have No House";

			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.OfficialContactOnly);
			AssertMultilineASCIIEquals("OfficialContactOnly",
@"", ValueProviderToTest.GetReplacement(macro, Report).ToString());

			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.OfficialContactWithCoverPage);
			AssertMultilineASCIIEquals("OfficialContactWithCoverPage",
@"", ValueProviderToTest.GetReplacement(macro, Report).ToString());

			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.DeliveryContactWithRedirectTo);
			AssertMultilineASCIIEquals("DeliveryContactWithRedirectTo",
@"", ValueProviderToTest.GetReplacement(macro, Report).ToString());

			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.DeliveryContactOnly);
			AssertMultilineASCIIEquals("DeliveryContactOnly",
@"", ValueProviderToTest.GetReplacement(macro, Report).ToString());
		}

		public void TestNoOfficialAndDeliveryContactNameOnlyWithRedirectToAndOrgAddressPK()
		{
			OrgAddress address = DocumentEngineTestHelper.GetNewOrganizationWithMainAddress("Stan", Factory).MainAddress;
			Factory.Save();
			string macro = "<IntendedRecipientAddress(" + address.PK + ")>";

			ReportForTesting.MostOfficialContact = null;
			ReportForTesting.DeliveryContact = new DocDeliveryContact(Factory);
			ReportForTesting.DeliveryContact.Name = "Mr I Have No House";

			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.OfficialContactOnly);
			AssertMultilineASCIIEquals("OfficialContactOnly",
@"STANCOMPANY
STANADDRESS1
STANADDRESS2
STANVILLE VT 9STA9
UNITED STATES", ValueProviderToTest.GetReplacement(macro, Report).ToString());

			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.OfficialContactWithCoverPage);
			AssertMultilineASCIIEquals("OfficialContactWithCoverPage",
@"STANCOMPANY
STANADDRESS1
STANADDRESS2
STANVILLE VT 9STA9
UNITED STATES", ValueProviderToTest.GetReplacement(macro, Report).ToString());

			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.DeliveryContactWithRedirectTo);
			AssertMultilineASCIIEquals("DeliveryContactWithRedirectTo",
@"STANCOMPANY
STANCOMPANY
STANADDRESS1
STANADDRESS2
STANVILLE VT 9STA9
UNITED STATES", ValueProviderToTest.GetReplacement(macro, Report).ToString());

			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.DeliveryContactOnly);
			AssertMultilineASCIIEquals("DeliveryContactOnly",
@"STANCOMPANY
STANADDRESS1
STANADDRESS2
STANVILLE VT 9STA9
UNITED STATES", ValueProviderToTest.GetReplacement(macro, Report).ToString());
		}

		public void TestNoOfficialAndDeliveryContactWithRedirectTo()
		{
			string macro = "<IntendedRecipientAddress>";

			ReportForTesting.MostOfficialContact = null;
			ReportForTesting.DeliveryContact = DocumentEngineTestHelper.GetNewDocDeliveryContact("Fred", Factory);

			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.OfficialContactOnly);
			AssertMultilineASCIIEquals("OfficialContactOnly",
@"FREDCOMPANY
FREDADDRESS1
FREDADDRESS2
FREDVILLE FRE 9FRE9
UNITED STATES", ValueProviderToTest.GetReplacement(macro, Report).ToString());

			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.OfficialContactWithCoverPage);
			AssertMultilineASCIIEquals("OfficialContactWithCoverPage",
@"FREDCOMPANY
FREDADDRESS1
FREDADDRESS2
FREDVILLE FRE 9FRE9
UNITED STATES", ValueProviderToTest.GetReplacement(macro, Report).ToString());

			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.DeliveryContactWithRedirectTo);
			AssertMultilineASCIIEquals("DeliveryContactWithRedirectTo",
@"FREDCOMPANY
FREDADDRESS1
FREDADDRESS2
FREDVILLE FRE 9FRE9
UNITED STATES", ValueProviderToTest.GetReplacement(macro, Report).ToString());

			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.DeliveryContactOnly);
			AssertMultilineASCIIEquals("DeliveryContactOnly",
@"FREDCOMPANY
FREDADDRESS1
FREDADDRESS2
FREDVILLE FRE 9FRE9
UNITED STATES", ValueProviderToTest.GetReplacement(macro, Report).ToString());
		}

		public void TestMacroReplacementWhenRegistryIsOfficialContactOnly()
		{
			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.OfficialContactOnly);

			DocDeliveryContact bill = DocumentEngineTestHelper.GetNewDocDeliveryContact("Bill", Factory);
			ReportForTesting.DeliveryContact = bill;
			ReportForTesting.MostOfficialContact = bill;

			ValueProvider macro = new IntendedRecipientAddress();
			string macroNoGuid = "<IntendedRecipientAddress>";
			AssertEquals("Precondition: Should be responsible to replace:- " + macroNoGuid, true, macro.IsResponsibleForReplacing(macroNoGuid, Passes.FirstPass));

			AssertMultilineASCIIEquals("Resulting replacement using no GUID when delivery contact the same as official contact.", @"
BILLCOMPANY
BILLADDRESS1
BILLADDRESS2
BILLVILLE BIL 9BIL9
UNITED STATES
".Trim(), ValueProviderToTest.GetReplacement(macroNoGuid, Report).ToString());

			DocDeliveryContact fred = DocumentEngineTestHelper.GetNewDocDeliveryContact("Fred", Factory);
			ReportForTesting.DeliveryContact = fred;

			AssertMultilineASCIIEquals("Resulting replacement using no GUID when delivery contact not the same as official contact.", @"
BILLCOMPANY
BILLADDRESS1
BILLADDRESS2
BILLVILLE BIL 9BIL9
UNITED STATES
".Trim(), ValueProviderToTest.GetReplacement(macroNoGuid, Report).ToString());
		}

		public void TestMacroReplacementWhenRegistryIsOfficialContactWithCoverPage()
		{
			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.OfficialContactWithCoverPage);

			DocDeliveryContact bill = DocumentEngineTestHelper.GetNewDocDeliveryContact("Bill", Factory);
			ReportForTesting.DeliveryContact = bill;
			ReportForTesting.MostOfficialContact = bill;

			ValueProvider macro = new IntendedRecipientAddress();
			string macroNoGuid = "<IntendedRecipientAddress>";
			AssertEquals("Precondition: Should be responsible to replace:- " + macroNoGuid, true, macro.IsResponsibleForReplacing(macroNoGuid, Passes.FirstPass));

			AssertMultilineASCIIEquals("Resulting replacement using no GUID when delivery contact the same as official contact.", @"
BILLCOMPANY
BILLADDRESS1
BILLADDRESS2
BILLVILLE BIL 9BIL9
UNITED STATES
".Trim(), ValueProviderToTest.GetReplacement(macroNoGuid, Report).ToString());

			DocDeliveryContact fred = DocumentEngineTestHelper.GetNewDocDeliveryContact("Fred", Factory);
			ReportForTesting.DeliveryContact = fred;

			AssertMultilineASCIIEquals("Resulting replacement using no GUID when delivery contact not the same as official contact.", @"
BILLCOMPANY
BILLADDRESS1
BILLADDRESS2
BILLVILLE BIL 9BIL9
UNITED STATES
".Trim(), ValueProviderToTest.GetReplacement(macroNoGuid, Report).ToString());
		}

		public void TestMacroReplacementWhenRegistryIsDeliveryContactWithRedirectTo()
		{
			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.DeliveryContactWithRedirectTo);

			ValueProvider macro = new IntendedRecipientAddress();
			string macroNoGuid = "<IntendedRecipientAddress>";
			AssertEquals("Precondition: Should be responsible to replace:- " + macroNoGuid, true, macro.IsResponsibleForReplacing(macroNoGuid, Passes.FirstPass));

			DocDeliveryContact bill = DocumentEngineTestHelper.GetNewDocDeliveryContact("Bill", Factory);
			ReportForTesting.DeliveryContact = bill;
			ReportForTesting.MostOfficialContact = bill;

			AssertMultilineASCIIEquals("Resulting replacement using no GUID when delivery contact the same as official contact.", @"
BILLCOMPANY
BILLADDRESS1
BILLADDRESS2
BILLVILLE BIL 9BIL9
UNITED STATES
".Trim(), ValueProviderToTest.GetReplacement(macroNoGuid, Report).ToString());

			ReportForTesting.DeliveryContact = DocumentEngineTestHelper.GetNewDocDeliveryContact("Fred", Factory);

			AssertMultilineASCIIEquals("Resulting replacement using no GUID when delivery contact not the same as official contact.", @"
BILLCOMPANY
FREDCOMPANY
FREDADDRESS1
FREDADDRESS2
FREDVILLE FRE 9FRE9
UNITED STATES
".Trim(), ValueProviderToTest.GetReplacement(macroNoGuid, Report).ToString());
		}

		public void TestMacroReplacementWhenRegistryIsDeliveryContactOnly()
		{
			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.DeliveryContactOnly);

			var bill = DocumentEngineTestHelper.GetNewDocDeliveryContact("Bill", Factory);
			ReportForTesting.DeliveryContact = bill;
			ReportForTesting.MostOfficialContact = bill;

			ValueProvider macro = new IntendedRecipientAddress();
			string macroNoGuid = "<IntendedRecipientAddress>";
			AssertEquals("Precondition: Should be responsible to replace:- " + macroNoGuid, true, macro.IsResponsibleForReplacing(macroNoGuid, Passes.FirstPass));

			AssertMultilineASCIIEquals("Resulting replacement using no GUID when delivery contact the same as official contact.", @"
BILLCOMPANY
BILLADDRESS1
BILLADDRESS2
BILLVILLE BIL 9BIL9
UNITED STATES
".Trim(), ValueProviderToTest.GetReplacement(macroNoGuid, Report).ToString());

			var fred = DocumentEngineTestHelper.GetNewDocDeliveryContact("Fred", Factory);
			ReportForTesting.DeliveryContact = fred;

			AssertMultilineASCIIEquals("Resulting replacement using no GUID when delivery contact not the same as official contact.", @"
FREDCOMPANY
FREDADDRESS1
FREDADDRESS2
FREDVILLE FRE 9FRE9
UNITED STATES
".Trim(), ValueProviderToTest.GetReplacement(macroNoGuid, Report).ToString());
		}

		public void TestMacroReplacementWithOrgAddressPKWhenRegistryIsOfficialContactOnly()
		{
			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.OfficialContactOnly);

			var stan = DocumentEngineTestHelper.GetNewOrganizationWithMainAddress("Stan", Factory);
			ValueProvider macro = new IntendedRecipientAddress();
			string macroWithGuid = "<IntendedRecipientAddress(" + stan.MainAddress.PK.ToString() + ")>";
			AssertEquals("Precondition: Should be responsible to replace:- " + macroWithGuid, true, macro.IsResponsibleForReplacing(macroWithGuid, Passes.FirstPass));
			Factory.Save();

			var bill = DocumentEngineTestHelper.GetNewDocDeliveryContact("Bill", Factory);
			ReportForTesting.DeliveryContact = bill;
			ReportForTesting.MostOfficialContact = bill;
			AssertMultilineASCIIEquals("Resulting replacement using no GUID when delivery contact the same as official contact.", @"
STANCOMPANY
STANADDRESS1
STANADDRESS2
STANVILLE VT 9STA9
UNITED STATES
".Trim(), ValueProviderToTest.GetReplacement(macroWithGuid, Report).ToString());

			var fred = DocumentEngineTestHelper.GetNewDocDeliveryContact("Fred", Factory);
			ReportForTesting.DeliveryContact = fred;
			AssertMultilineASCIIEquals("Resulting replacement using no GUID when delivery contact not the same as official contact.", @"
STANCOMPANY
STANADDRESS1
STANADDRESS2
STANVILLE VT 9STA9
UNITED STATES
".Trim(), ValueProviderToTest.GetReplacement(macroWithGuid, Report).ToString());
		}

		public void TestMacroReplacementWithOrgAddressPKWhenRegistryIsOfficialContactWithCoverPage()
		{
			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.OfficialContactWithCoverPage);

			var stan = DocumentEngineTestHelper.GetNewOrganizationWithMainAddress("Stan", Factory);
			ValueProvider macro = new IntendedRecipientAddress();
			string macroWithGuid = "<IntendedRecipientAddress(" + stan.MainAddress.PK.ToString() + ")>";
			AssertEquals("Precondition: Should be responsible to replace:- " + macroWithGuid, true, macro.IsResponsibleForReplacing(macroWithGuid, Passes.FirstPass));
			Factory.Save();

			var bill = DocumentEngineTestHelper.GetNewDocDeliveryContact("Bill", Factory);
			ReportForTesting.DeliveryContact = bill;
			ReportForTesting.MostOfficialContact = bill;
			AssertMultilineASCIIEquals("Resulting replacement using no GUID when delivery contact the same as official contact.", @"
STANCOMPANY
STANADDRESS1
STANADDRESS2
STANVILLE VT 9STA9
UNITED STATES
".Trim(), ValueProviderToTest.GetReplacement(macroWithGuid, Report).ToString());

			var fred = DocumentEngineTestHelper.GetNewDocDeliveryContact("Fred", Factory);
			ReportForTesting.DeliveryContact = fred;
			AssertMultilineASCIIEquals("Resulting replacement using no GUID when delivery contact not the same as official contact.", @"
STANCOMPANY
STANADDRESS1
STANADDRESS2
STANVILLE VT 9STA9
UNITED STATES
".Trim(), ValueProviderToTest.GetReplacement(macroWithGuid, Report).ToString());
		}

		public void TestMacroReplacementWithOrgAddressPKWhenRegistryIsDeliveryContactWithRedirectTo()
		{
			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.DeliveryContactWithRedirectTo);

			var stan = DocumentEngineTestHelper.GetNewOrganizationWithMainAddress("Stan", Factory);
			ValueProvider macro = new IntendedRecipientAddress();
			string macroWithGuid = "<IntendedRecipientAddress(" + stan.MainAddress.PK.ToString() + ")>";
			AssertEquals("Precondition: Should be responsible to replace:- " + macroWithGuid, true, macro.IsResponsibleForReplacing(macroWithGuid, Passes.FirstPass));
			Factory.Save();

			var bill = DocumentEngineTestHelper.GetNewDocDeliveryContact("Bill", Factory);
			ReportForTesting.DeliveryContact = bill;
			ReportForTesting.MostOfficialContact = bill;
			AssertMultilineASCIIEquals("Resulting replacement using GUID when delivery contact the same as official contact.", @"
STANCOMPANY
BILLCOMPANY
BILLADDRESS1
BILLADDRESS2
BILLVILLE BIL 9BIL9
UNITED STATES
".Trim(), ValueProviderToTest.GetReplacement(macroWithGuid, Report).ToString());

			var fred = DocumentEngineTestHelper.GetNewDocDeliveryContact("Fred", Factory);
			ReportForTesting.DeliveryContact = fred;
			AssertMultilineASCIIEquals("Resulting replacement using GUID when delivery contact not the same as official contact.", @"
STANCOMPANY
FREDCOMPANY
FREDADDRESS1
FREDADDRESS2
FREDVILLE FRE 9FRE9
UNITED STATES
".Trim(), ValueProviderToTest.GetReplacement(macroWithGuid, Report).ToString());
		}

		public void TestMacroReplacementWithOrgAddressPKWhenRegistryIsDeliveryContactOnly()
		{
			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(RedirectedDocumentAddressFormattingOptionList.Codes.DeliveryContactOnly);

			var stan = DocumentEngineTestHelper.GetNewOrganizationWithMainAddress("Stan", Factory);
			ValueProvider macro = new IntendedRecipientAddress();
			string macroWithGuid = "<IntendedRecipientAddress(" + stan.MainAddress.PK.ToString() + ")>";
			AssertEquals("Precondition: Should be responsible to replace:- " + macroWithGuid, true, macro.IsResponsibleForReplacing(macroWithGuid, Passes.FirstPass));
			Factory.Save();

			var bill = DocumentEngineTestHelper.GetNewDocDeliveryContact("Bill", Factory);
			ReportForTesting.DeliveryContact = bill;
			ReportForTesting.MostOfficialContact = bill;
			AssertMultilineASCIIEquals("Resulting replacement using no GUID when delivery contact the same as official contact.", @"
BILLCOMPANY
BILLADDRESS1
BILLADDRESS2
BILLVILLE BIL 9BIL9
UNITED STATES
".Trim(), ValueProviderToTest.GetReplacement(macroWithGuid, Report).ToString());

			var fred = DocumentEngineTestHelper.GetNewDocDeliveryContact("Fred", Factory);
			ReportForTesting.DeliveryContact = fred;
			AssertMultilineASCIIEquals("Resulting replacement using no GUID when delivery contact not the same as official contact.", @"
FREDCOMPANY
FREDADDRESS1
FREDADDRESS2
FREDVILLE FRE 9FRE9
UNITED STATES
".Trim(), ValueProviderToTest.GetReplacement(macroWithGuid, Report).ToString());
		}

		public void TestIsResponsibleForReplacing()
		{
			AssertIsResponsibleForReplacing("<IntendedRecipientAddress({30A35925-A453-4edb-BF27-3FE83CA2B874} )>");
			AssertIsResponsibleForReplacing("<IntendedRecipientAddress({30A35925-A453-4edb-BF27-3FE83CA2B874})>");
			AssertIsResponsibleForReplacing("<IntendedRecipientAddress(  30A35925-A453-4edb-BF27-3FE83CA2B874 )>");
			AssertIsResponsibleForReplacing("<IntendedRecipientAddress(<SomeRandomMacro>)>");
			AssertIsResponsibleForReplacing("<IntendedRecipientAddress( < Some Random Macro > )>");
			AssertIsResponsibleForReplacing("<IntendedRecipientAddress(  )>");
			AssertIsResponsibleForReplacing("<IntendedRecipientAddress()>");

			AssertIsResponsibleForReplacing("<IntendedRecipientAddress>");
			AssertIsResponsibleForReplacing("<intended recipient address>");
			AssertIsResponsibleForReplacing("<intendedrecipientaddress>");

			AssertNotResponsibleForReplacing("<MrsMiaWallace>");
			AssertNotResponsibleForReplacing("<IntendedRecipientAddressXXX(\"XXX\")>");
		}

		public void TestReplacement()
		{
			PrepareRenderer();

			DocDeliveryContact fred = new DocDeliveryContact(new BusinessObjectFactory());
			fred.Name = "Fred";
			fred.CompanyName = "Foo Corp";
			fred.Address1 = "Addr1";
			fred.Address2 = "Addr2";
			fred.City = "Sydney";
			fred.PostCode = "2000";
			fred.State = "NSW";
			fred.UNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			((IReportForUnitTesting)Report).MostOfficialContact = fred;

			DocDeliveryContact bob = new DocDeliveryContact(new BusinessObjectFactory());
			bob.Name = "Bob";
			bob.CompanyName = "Boo Corp";
			bob.Address1 = "Addr3";
			bob.Address2 = "Addr4";
			bob.City = "Sydney";
			bob.PostCode = "2000";
			bob.State = "NSW";
			bob.UNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			((IReportForUnitTesting)Report).DeliveryContact = bob;

			Report.TypeOfContact = ContactType.Consignee;
			AssertMultilineASCIIEquals("Report.TypeOfContact = ContactType.Consignee",
@"Foo Corp
Addr1
Addr2
Sydney NSW 2000".ToUpper(), ValueProviderToTest.GetReplacement("<IntendedRecipientAddress>", Report).ToString());
			Report.TypeOfContact = ContactType.Consignor;
			AssertMultilineASCIIEquals("Report.TypeOfContact = ContactType.Consignor",
@"Foo Corp
Addr1
Addr2
Sydney NSW 2000".ToUpper(), ValueProviderToTest.GetReplacement("<IntendedRecipientAddress>", Report).ToString());

			SwithToCompanyAndBranchIS();
			OrgHeader orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			OrgCusCode kennitala = Factory.NewWithValidTestData<OrgCusCode>();
			kennitala.OK_CodeType = OrgCusCode.CodeTypes.Kennitala;
			kennitala.OK_CustomsRegNo = "kennitala";
			kennitala.OK_OH = orgProxy.PK;
			fred.OrgHeaderPK = orgProxy.PK;
			Factory.Save();

			Report.DeliveryContact.Name = "Bob";
			Report.TypeOfContact = ContactType.Consignee;
			AssertMultilineASCIIEquals("",
@"BOO CORP
ADDR3
ADDR4
SYDNEY NSW 2000
AUSTRALIA
Kennitala kennitala", ValueProviderToTest.GetReplacement("<IntendedRecipientAddress>", Report).ToString());

			kennitala.OK_CodeType = OrgCusCode.CodeTypes.AccountsPayableSuppliersReference;
			kennitala.OK_CustomsRegNo = "NOT EMPTY";
			bob.Name = "THE ACCOUNTS PAYABLE MANAGER";
			orgProxy.OH_RL_NKClosestPort = "ISREY";
			Factory.Save();

			DocDeliveryContact contact = Report.MostOfficialContact ?? Report.DeliveryContact;
			contact.OrgHeader.Addresses.AddNew(OrgAddressType.PickupAndDelivery, false).OA_Address1 = "Pickup and Delivery Address";

			var replacement = ValueProviderToTest.GetReplacement("", Report).ToString();

			Assert(!replacement.Contains("b/t: bókhaldsdeildar"));
			Assert(replacement.Contains("Pickup and Delivery Address".ToUpper()));

			contact.OrgHeader.Addresses.RemoveAndDeleteAll();
			contact.OrgHeader.Addresses.MainAddress.OA_Address1 = "Office Address";

			replacement = ValueProviderToTest.GetReplacement("", Report).ToString();
			Assert(!replacement.Contains("b/t: bókhaldsdeildar"));
			Assert(replacement.Contains("Office Address".ToUpper()));

			RevertToInitialCompanyAndBranch();
		}

		public void TestReplacementWhenNull()
		{
			// When original contact is null, the delivery contact should be displayed
			PrepareRenderer();

			DocDeliveryContact bob = new DocDeliveryContact(new BusinessObjectFactory());
			bob.Name = "Bob";
			bob.CompanyName = "Boo Corp";
			bob.Address1 = "Addr1";
			bob.Address2 = "Addr2";
			bob.City = "Sydney";
			bob.PostCode = "2000";
			bob.State = "NSW";
			bob.UNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			((IReportForUnitTesting)Report).DeliveryContact = bob;

			Report.TypeOfContact = ContactType.Consignee;
			AssertMultilineASCIIEquals("Report.TypeOfContact = ContactType.Consignee",
@"Boo Corp
Addr1
Addr2
Sydney NSW 2000".ToUpper(), ValueProviderToTest.GetReplacement("", Report).ToString());
		}

		#region Implementation

		IReportForUnitTesting ReportForTesting
		{
			get
			{
				if (fReportForTesting == null)
				{
					PrepareRenderer();
					fReportForTesting = Report;
				}
				return fReportForTesting;
			}
		}

		IReportForUnitTesting fReportForTesting;

		void SwithToCompanyAndBranchIS()
		{
			RemeberCurrentUserContext();

			GlbCompany icelandCo = Factory.NewWithValidTestData<GlbCompany>(TestBusinessObjectKind.MinimumRequiredToSave);
			icelandCo.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Iceland;
			icelandCo.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Iceland;
			icelandCo.GC_Code = "ISC";
			icelandCo.GC_Name = "Iceland Company";
			icelandCo.GC_Address1 = "1 somewhere";
			icelandCo.GC_City = "Reykjavik";

			OrgHeader orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.OH_RL_NKClosestPort = "ISREY";
			icelandCo.GC_OH_OrgProxy = orgProxy.PK;

			GlbBranch iSBranch = Factory.NewWithValidTestData<GlbBranch>(TestBusinessObjectKind.MinimumRequiredToSave);
			iSBranch.GB_Code = "REY";
			iSBranch.GB_City = "Reykjavik";
			iSBranch.GB_RL_NKHomePort = "ISREY";
			iSBranch.GB_GC = icelandCo.PK;
			iSBranch.GB_BranchName = "IS Branch";
			iSBranch.GB_Address1 = "1 somewhere";

			SetNewUserContext(iSBranch.PK);
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Iceland);
		}

		void RemeberCurrentUserContext()
		{
			initialUserContext = Env.CurrentUserContext;
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Baseline")]
		void SetNewUserContext(ZGuid branchPk)
		{
			GlbDepartment bRNDepartment = FindOrCreateDepartment("BRN");
			Factory.Save();
			Env.SetUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, branchPk.ToGuid(), bRNDepartment.PK.ToGuid()));
			GlbCompany.CurrentCompany.SetCountry(initialUserContext.Company.Country.Code);
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Baseline")]
		void RevertToInitialCompanyAndBranch()
		{
			Env.SetUserContext(initialUserContext);
		}

		GlbDepartment FindOrCreateDepartment(string departmentCode)
		{
			GlbDepartment result = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, departmentCode);
			if (result == null)
			{
				result = Factory.NewWithValidTestData<GlbDepartment>();
				result.GE_Code = departmentCode;
				Factory.Save();
			}
			return result;
		}

		IUserContext initialUserContext;

		protected override ValueProvider GetNewValueProvider()
		{
			return new IntendedRecipientAddress();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			var fred = new DocDeliveryContact(new BusinessObjectFactory());
			fred.Name = "Fred";
			fred.CompanyName = "Foo Corp";
			fred.Address1 = "Addr1";
			fred.Address2 = "Addr2";
			fred.City = "Sydney";
			fred.PostCode = "2000";
			fred.State = "NSW";
			fred.UNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			((IReportForUnitTesting)Report).MostOfficialContact = fred;
			Report.TypeOfContact = ContactType.Consignee;

			var appleOrg = DocumentEngineTestHelper.GetNewOrganizationWithMainAddress("Apple", Factory);
			appleOrg.OH_RL_NKClosestPort = "CNSHA";
			var appleDeliveryContact =
				DocumentEngineTestHelper.GetNewDocDeliveryContact("BILL GATES", appleOrg, appleOrg.MainAddress, Factory);
			var microsoft = DocumentEngineTestHelper.AddAddress("Microsoft", appleOrg, OrgAddressType.Residential);
			microsoft.OA_RL_NKRelatedPortCode = "USSEA";

			ReportForTesting.DeliveryContact = appleDeliveryContact;
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("AddressPK", microsoft.PK.ToString()));
			Factory.Save();
		}

		#endregion
	}
}
