using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XtCredential;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	[TestedType(typeof(CLSMSMessageSending))]
	partial class CLSMSMessageSendingTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestStaticGetDefault()
		{
			var @default = CLSMSMessageSending.GetDefault();
			AssertEquals("Default.RunningIntervalInSeconds", 60, @default.RunningIntervalInSeconds);
			AssertEquals("Default.EnableSMSMessageSending", false, @default.EnableSMSMessageSending);
			AssertEquals("Default.XtCredentialStatus", CLSMSMessageXtCredentialStatusList.Codes.New, @default.XtCredentialStatus);
		}

		public void TestEnableSMSMessageSending_Readonly()
		{
			BizObj.XtCredentialStatus = CLSMSMessageXtCredentialStatusList.Codes.New;
			AssertEquals("Status New, EnableSMSMessageSending writable.", false, BizObj.EnableSMSMessageSendingInfo.ReadOnly);

			BizObj.XtCredentialStatus = CLSMSMessageXtCredentialStatusList.Codes.Awaiting;
			AssertEquals("Status Awaiting, EnableSMSMessageSending read only.", true, BizObj.EnableSMSMessageSendingInfo.ReadOnly);

			BizObj.XtCredentialStatus = CLSMSMessageXtCredentialStatusList.Codes.Registered;
			AssertEquals("Status Registered, EnableSMSMessageSending writable.", false, BizObj.EnableSMSMessageSendingInfo.ReadOnly);

			BizObj.XtCredentialStatus = CLSMSMessageXtCredentialStatusList.Codes.Error;
			AssertEquals("Status Registered, EnableSMSMessageSending writable.", false, BizObj.EnableSMSMessageSendingInfo.ReadOnly);
		}

		public void TestXtCredentialStatus_Readonly()
		{
			var originalIsDeveloper = GlbStaff.CurrentUser.GS_IsDeveloper;

			GlbStaff.CurrentUser.GS_IsDeveloper = true;
			AssertEquals("IsDeveloper XtCredentialStatus writable.", false, BizObj.XtCredentialStatusInfo.ReadOnly);

			GlbStaff.CurrentUser.GS_IsDeveloper = false;
			AssertEquals("IsDeveloper XtCredentialStatus read only.", true, BizObj.XtCredentialStatusInfo.ReadOnly);

			GlbStaff.CurrentUser.GS_IsDeveloper = originalIsDeveloper;
		}

		public void TestRequiredXtCredentialAction()
		{
			AssertEquals("Nothing changed, RequiredXtCredentialAction returns None.", XtCredentialAction.None, BizObj.RequiredXtCredentialAction);
			BizObj.XtCredentialStatus = CLSMSMessageXtCredentialStatusList.Codes.New;

			BizObj.MachineName = "NODE2";
			AssertEquals("ApplicationNodeName changed, Status empty, EnableSMSMessageSending false: RequiredXtCredentialAction returns None.", XtCredentialAction.None, BizObj.RequiredXtCredentialAction);
			BizObj.EnableSMSMessageSending = true;
			AssertEquals("ApplicationNodeName changed, Status empty, EnableSMSMessageSending true: RequiredXtCredentialAction returns New.", XtCredentialAction.Update, BizObj.RequiredXtCredentialAction);

			BizObj.XtCredentialStatus = CLSMSMessageXtCredentialStatusList.Codes.Awaiting;
			AssertEquals("ApplicationNodeName changed, Status Awaiting: RequiredXtCredentialAction returns None.", XtCredentialAction.None, BizObj.RequiredXtCredentialAction);

			BizObj.XtCredentialStatus = CLSMSMessageXtCredentialStatusList.Codes.Registered;
			AssertEquals("ApplicationNodeName changed, Status Registered: RequiredXtCredentialAction returns Update.", XtCredentialAction.Update, BizObj.RequiredXtCredentialAction);
			BizObj.EnableSMSMessageSending = false;
			AssertEquals("ApplicationNodeName changed, Status Registered, EnableSMSMessageSending false: RequiredXtCredentialAction returns Delete.", XtCredentialAction.Delete, BizObj.RequiredXtCredentialAction);
		}

		public void TestIxTCredentialProviderMembers()
		{
			var sending = new CLSMSMessageSending(new FallbackLevel(GlbCompany.CurrentCompany, null, null), Factory);

			sending.MachineName = "NODE2";
			var interfaceInstance = sending as IxTCredentialProvider;
			AssertEquals("EnterpriseCode: <LicenceKeyIdentifier>_CLSMS", GlbCompany.CurrentCompany.LicenceKeyIdentifier + "_CLSMS", interfaceInstance.EnterpriseCode);
			AssertEquals(XtCredentialConstants.IdentifierList.CLC, interfaceInstance.Identifier);
		}

		public void TestLookups()
		{
			AssertType<CLSMSMessageSendingLookups>(BizObj.Lookups);
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone() => GetBusinessObjectToSerialise();

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise() => BizObj;

		protected new CLSMSMessageSending BizObj => (CLSMSMessageSending)base.BizObj;
	}
}
