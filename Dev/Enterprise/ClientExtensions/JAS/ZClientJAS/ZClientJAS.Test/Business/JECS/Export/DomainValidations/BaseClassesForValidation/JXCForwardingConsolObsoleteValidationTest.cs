using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations.Testing
{
	internal class JXCForwardingConsolObsoleteValidationTest : JXCValidationTestCase
	{
		public void TestValidateJK_OA_SendingForwarderAddress_NotEntered()
		{
			Consol.SetDefaultSendingForwarderAddress(ZGuid.Empty);
			Consol.Validation.ValidateJK_OA_SendingForwarderAddress();
			AssertHasNotEnteredJXCWarning(Consol.JK_OA_SendingForwarderAddressInfo);
		}

		public void TestValidateJK_OA_SendingForwarderAddress_NoNettingAndOfficeCode()
		{
			JASOrgHeader sendingForwarder = Factory.New<JASOrgHeader>();
			sendingForwarder.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			Consol.SetDefaultSendingForwarderAddress(sendingForwarder);
			string expectedNettingCodeWarningMessage = JXCConstants.JXCWarningPrefix + "Forwarder does not have JAS Netting Code";
			string expectedOfficeCodeWarningMessage = JXCConstants.JXCWarningPrefix + "Forwarder does not have JAS Office Code";
			AssertHasJXCWarning(Consol.JK_OA_SendingForwarderAddressInfo, expectedNettingCodeWarningMessage, expectedOfficeCodeWarningMessage);
		}

		public void TestValidateJK_OA_SendingForwarderAddress_NotControlledOrProxyToTheCurrentBranch()
		{
			Consol.SetDefaultSendingForwarderAddress(Factory.New<JASOrgHeader>());
			Consol.SendingForwarder.OfficeCode = "AUSYD";
			Consol.SendingForwarder.NettingCode = "AUCOR";
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			Consol.Validation.ValidateJK_OA_SendingForwarderAddress();
			AssertHasJXCWarning(Consol.JK_OA_SendingForwarderAddressInfo, JXCConstants.JXCWarningPrefix + "The Sending Forwarder is not controlled by or a proxy to the current Branch. Please configure the Controlling Branch from the Organisation screen or relogin");
			GlbBranch otherBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK));
			Consol.SendingForwarder.CompanyData.OB_GB_ControllingBranch = otherBranch.PK;
			Consol.Validation.ValidateJK_OA_SendingForwarderAddress();
			AssertHasJXCWarning(Consol.JK_OA_SendingForwarderAddressInfo, JXCConstants.JXCWarningPrefix + "The Sending Forwarder is not controlled by or a proxy to the current Branch. Please configure the Controlling Branch from the Organisation screen or relogin");
			Consol.SendingForwarder.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			Consol.Validation.ValidateJK_OA_SendingForwarderAddress();
			AssertHasNoJXCWarnings(Consol.JK_OA_SendingForwarderAddressInfo);
			Consol.SendingForwarder.CompanyData.OB_GB_ControllingBranch = ZGuid.Empty;
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = Consol.SendingForwarder.PK;
			Consol.Validation.ValidateJK_OA_SendingForwarderAddress();
			AssertHasNoJXCWarnings(Consol.JK_OA_SendingForwarderAddressInfo);
		}

		public void TestValidateJK_OA_ReceivingForwarderAddress()
		{
			Consol.SetDefaultSendingForwarderAddress(ZGuid.Empty);
			Consol.Validation.ValidateJK_OA_ReceivingForwarderAddress();
			AssertHasNotEnteredJXCWarning(Consol.JK_OA_ReceivingForwarderAddressInfo);
			Consol.SetDefaultReceivingForwarderAddress(Factory.New<JASOrgHeader>());
			Consol.Validation.ValidateJK_OA_ReceivingForwarderAddress();
			string expectedNettingCodeWarningMessage = JXCConstants.JXCWarningPrefix + "Forwarder does not have JAS Netting Code";
			string expectedOfficeCodeWarningMessage = JXCConstants.JXCWarningPrefix + "Forwarder does not have JAS Office Code";
			AssertHasJXCWarning(Consol.JK_OA_ReceivingForwarderAddressInfo, expectedNettingCodeWarningMessage, expectedOfficeCodeWarningMessage);
			Consol.ReceivingForwarder.OfficeCode = "AUSYD";
			Consol.ReceivingForwarder.NettingCode = "AUCOR";
			Consol.Validation.ValidateJK_OA_ReceivingForwarderAddress();
			AssertHasNoJXCWarnings(Consol.JK_OA_ReceivingForwarderAddressInfo);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			Factory.Validation.MainGroup.RegisterValidationType(typeof(JASForwardingConsol), ValidationTypeToTest);
		}

		protected virtual Type ValidationTypeToTest
		{
			get
			{
				return typeof(JXCForwardingConsolValidation);
			}
		}

		protected JASForwardingConsol Consol
		{
			get
			{
				if (fConsol == null)
				{
					fConsol = Factory.New<JASForwardingConsol>();
				}

				return fConsol;
			}
		}

		JASForwardingConsol fConsol;
		#endregion
	}
}
