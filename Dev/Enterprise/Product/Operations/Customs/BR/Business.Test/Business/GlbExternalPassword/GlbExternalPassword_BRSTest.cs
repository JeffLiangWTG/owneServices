using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(GlbExternalPassword_BRS))]
	public class GlbExternalPassword_BRSTest : GlbExternalPasswordTest<GlbExternalPassword_BRS>
	{
		public void TestHumanReadableNameCore()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ABC";
			var wrapper = BRGlbStaffWrapper.Get(staff);
			var externalPasswordBr = wrapper.EventSubscriptions.AddNew();
			AssertEquals("HumanReadableNameCore for BRS", "Event Subscription", externalPasswordBr.HumanReadableName);
		}

		public void TestEventIdCodeDescriptionPairList()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ABC";
			var wrapper = BRGlbStaffWrapper.Get(staff);
			var externalPasswordBr = wrapper.EventSubscriptions.AddNew();
			AssertContainsExactElementsInAnyOrder("Items on EventIdCodeDescriptionPairList", new string[] { EventIdList.Codes.CctReleasedCargo, EventIdList.Codes.CctBlockedCargo, EventIdList.Codes.CctRedChannel, EventIdList.Codes.DuexHistoric, EventIdList.Codes.ProductCatalog,
			EventIdList.Codes.LpcoStatusChange, EventIdList.Codes.LpcoExigencyInclusion, EventIdList.Codes.LpcoExigencyCancelation, EventIdList.Codes.DuimpDiagnosis, EventIdList.Codes.DuimpRegister, EventIdList.Codes.DuimpStatus }, externalPasswordBr.Lookups.EventIdCodeDescriptionPairList.GetAllCodes());
		}

		public void TestSetDefaultValues()
		{
			var password = Factory.New<GlbExternalPassword_BRS>();
			AssertEquals("BRS", password.GP_PasswordType);
			AssertEquals("", password.GP_PasswordStatus);
		}

		public void TestReadOnly()
		{
			AssertEquals("GP_UserIDInfo.ReadOnly", false, GlbExternalPassword.GP_UserIDInfo.ReadOnly);
			AssertEquals("GP_PasswordStatusInfo.ReadOnly", true, GlbExternalPassword.GP_PasswordStatusInfo.ReadOnly);
		}

		public void TestGP_MailBoxID()
		{
			var password = Factory.NewWithValidTestData<GlbExternalPassword_BRS>();
			AssertEquals(10, password.GP_MailBoxIDInfo.MaxLength);
		}

		public void TestIMessageAttachee()
		{
			var password = Factory.New<GlbExternalPassword_BRS>();
			AssertEquals(GlbCompany.CurrentCompany.FirstActiveBranch.PK, ((IMessageAttachee)password).BranchPK);

			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();
			password.GP_GC = company.PK;
			AssertEquals(branch.PK, ((IMessageAttachee)password).BranchPK);
		}

		public override void TestOnlySendCredentialIfNeeded()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ABC";
			var wrapper = BRGlbStaffWrapper.Get(staff);
			var password = wrapper.CCTPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			Factory.Save();
			var subscriptions = wrapper.EventSubscriptions;
			AssertEquals("GP_UserID", EventIdList.Codes.DuexHistoric, subscriptions[0].GP_UserID);
			AssertEquals("ConfigurationName is empty", ZString.Empty, subscriptions[0].ConfigurationName);
			var interchange = GetLatestEHubConfigurationInterchanges();
			AssertXMLEquals("Send message when saving", ExpectedXmlWithOneGroup, Regex.Replace(interchange.EI_BodyText, @"(?<=<Passphrase>).*(?=</Passphrase>)", "CaRgOw1sE"));
		}

		public void TestStatusDescription()
		{
			var password = Factory.NewWithValidTestData<GlbExternalPassword_BRS>();

			password.GP_StatusReason = GlbExternalPassword_BRS.StatusReasons.Subscribed;
			AssertEquals(GlbExternalPassword_BRS.StatusReasons.Subscribed, password.StatusDescription);

			password.GP_StatusReason = ZString.Empty;
			AssertEquals(GlbExternalPassword_BRS.StatusReasons.NotSent, password.StatusDescription);
		}

		public void TestSubmittedDate()
		{
			var password = Factory.NewWithValidTestData<GlbExternalPassword_BRS>();
			AssertEquals("SubmittedDate", ZDateTime.Empty, password.SubmittedDate);

			var dateTime = new ZDateTime(2023, 06, 16, 0, 0, 0);

			CreateMessage(password, dateTime).EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			CreateMessage(password, dateTime).EM_Status = EDIMessageStatusList.Codes.Failed;
			CreateMessage(password, dateTime).EM_MessageType = EDIMessageTypeList.Codes.XDC;
			CreateMessage(password, dateTime).EM_MessageSubType = EDIMessageSubTypeList.Codes.Rectification;
			CreateMessage(password, dateTime).EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			CreateMessage(password, dateTime).EM_SystemCreateTimeUtc = dateTime.AddHours(1);
			CreateMessage(password, dateTime).EM_SystemCreateTimeUtc = dateTime.AddHours(2);

			AssertEquals("SubmittedDate", dateTime.AddHours(1).ToLocalBranchTime(), password.SubmittedDate);
		}

		BREDIMessage CreateMessage(GlbExternalPassword_BRS password, ZDateTime dateTime, string receiveTransmit = EDIMessage.Direction.Transmit)
		{
			var message = Factory.NewWithValidTestData<BREDIMessage>();
			message.EM_LinkTable = password.TableName;
			message.EM_LinkUniqueID = password.PK;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.BRCustoms;
			message.EM_Status = EDIMessageStatusList.Codes.Sent;
			message.EM_MessageType = MessageTypeList.Codes.SUB;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.Original;
			message.EM_ReceiveTransmit = receiveTransmit;
			message.EM_SystemCreateTimeUtc = dateTime;
			return message;
		}

		EDIInterchange GetLatestEHubConfigurationInterchanges()
		{
			var query = new ZDBOnlyQuery(typeof(EDIInterchange));
			query.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.eHub);
			query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, EDIInterchangeTypeList.Codes.Configuration);
			query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit);
			query.AddToFilter(EDIInterchangeSchema.EI_From, ((IGlbCompany)GlbCompany.CurrentCompany).LicenceKeyIdentifier);
			query.AddToFilter(EDIInterchangeSchema.EI_To, MasterFiles.Business.Customs.XmlCredential.Constants.Configuration.EHubRecipient);
			query.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchangeStatusList.Codes.eHubQueued);
			query.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.eHub);
			query.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " DESC";
			return Factory.LoadTop1<EDIInterchange>(query);
		}

		const string ExpectedXmlWithOneGroup = @"<Configuration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Name=""Advanced Air Cargo Report - Brazil"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""EDI"">
      <Group Type=""Staff"" Reference=""ABC"">
        <Certificate Name=""Certificate"">
          <File>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</File>
          <Passphrase>CaRgOw1sE</Passphrase>
        </Certificate>
      </Group>
    </Group>
  </Group>
</Configuration>";
	}
}
