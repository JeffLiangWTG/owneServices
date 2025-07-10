using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.JXC.Export.Validations.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class PreShipmentWrapperValidationTest : JXCValidationTestCase
	{
		public void TestConstructor()
		{
			AssertEquals("Should be assigned in the constructor", PreShipmentWrapper, Validation.Parent);
		}

		public void TestValidateAll()
		{
			Validation.ValidateAll();
			AssertHasNotEnteredJXCWarning(PreShipmentWrapper.SendingForwarderPKInfo);
			AssertMandatoryValidationError(PreShipmentWrapper.ReceivingForwarderPKInfo, true);
			JASOrgHeader testOrg1 = (JASOrgHeader)PreShipmentWrapper.SendingForwarders.AddNew(typeof(JASOrgHeader));
			JASOrgHeader testOrg2 = (JASOrgHeader)PreShipmentWrapper.ReceivingForwarders.AddNew(typeof(JASOrgHeader));
			testOrg1.OfficeCode = "AUBNE";
			testOrg1.NettingCode = "AUCOR";
			testOrg1.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			testOrg2.OfficeCode = "ITMIL";
			testOrg2.NettingCode = "ITMIL";
			PreShipmentWrapper.SendingForwarderPK = testOrg1.PK;
			PreShipmentWrapper.ReceivingForwarderPK = testOrg2.PK;
			Validation.ValidateAll();
			Assert(!PreShipmentWrapper.HasNotifications());
		}

		public void TestValidateSendingForwarderPK_NotEnteredOrInvalid()
		{
			PreShipmentWrapper.SendingForwarderPK = ZGuid.Empty;
			PreShipmentWrapper.Validation.ValidateSendingForwarderPK();
			AssertHasNotEnteredJXCWarning(PreShipmentWrapper.SendingForwarderPKInfo);
			PreShipmentWrapper.SendingForwarderPK = ZGuid.NewZGuid();
			AssertListValidationInvalidCodeError(PreShipmentWrapper.SendingForwarderPKInfo, true);
		}

		public void TestValidateSendingForwarderPK_SameAsReceivingForwarderPK()
		{
			JASOrgHeader newOrg = (JASOrgHeader)PreShipmentWrapper.SendingForwarders.AddNew(typeof(JASOrgHeader));
			PreShipmentWrapper.ReceivingForwarderPK = newOrg.PK;
			PreShipmentWrapper.SendingForwarderPK = newOrg.PK;
			Assert("Should not be the same as ReceivingForwarder", PreShipmentWrapper.SendingForwarderPKInfo.HasErrors());
		}

		public void TestValidateSendingForwarderPK_NoNettingAndOfficeCode()
		{
			JASOrgHeader newOrg = (JASOrgHeader)PreShipmentWrapper.SendingForwarders.AddNew(typeof(JASOrgHeader));
			newOrg.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			PreShipmentWrapper.SendingForwarderPK = newOrg.PK;
			Assert("Should not have any errors", !PreShipmentWrapper.SendingForwarderPKInfo.HasErrors());
			string expectedWarning1 = JXCConstants.JXCWarningPrefix + "Forwarder does not have JAS Netting Code";
			string expectedWarning2 = JXCConstants.JXCWarningPrefix + "Forwarder does not have JAS Office Code";
			AssertHasJXCWarning(PreShipmentWrapper.SendingForwarderPKInfo, expectedWarning1, expectedWarning2);
			newOrg.OfficeCode = "AUSYD";
			PreShipmentWrapper.Validation.ValidateSendingForwarderPK();
			AssertHasJXCWarning(PreShipmentWrapper.SendingForwarderPKInfo, expectedWarning1);
			newOrg.NettingCode = "AUCOR";
			PreShipmentWrapper.Validation.ValidateSendingForwarderPK();
			AssertHasNoJXCWarnings(PreShipmentWrapper.SendingForwarderPKInfo);
		}

		public void TestValidateSendingForwarderPK_NotControlledOrProxyToTheCurrentBranch()
		{
			JASOrgHeader newOrg = (JASOrgHeader)PreShipmentWrapper.SendingForwarders.AddNew(typeof(JASOrgHeader));
			newOrg.NettingCode = "AUCOR";
			newOrg.OfficeCode = "AUSYD";
			PreShipmentWrapper.SendingForwarderPK = newOrg.PK;
			Assert("Should not have any errors", !PreShipmentWrapper.SendingForwarderPKInfo.HasErrors());
			AssertHasJXCWarning(PreShipmentWrapper.SendingForwarderPKInfo, JXCConstants.JXCWarningPrefix + "The Sending Forwarder is not controlled by or a proxy to the current Branch. Please configure the Controlling Branch from the Organisation screen or relogin");
			PreShipmentWrapper.SendingForwarder.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			PreShipmentWrapper.Validation.ValidateSendingForwarderPK();
			AssertHasNoJXCWarnings("Current branch is the controlling branch, should not have warnings", PreShipmentWrapper.SendingForwarderPKInfo);
			PreShipmentWrapper.SendingForwarder.CompanyData.OB_GB_ControllingBranch = ZGuid.Empty;
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = PreShipmentWrapper.SendingForwarderPK;
			PreShipmentWrapper.Validation.ValidateSendingForwarderPK();
			AssertHasNoJXCWarnings("Organisation is a proxy to the current branch, should not have warnings", PreShipmentWrapper.SendingForwarderPKInfo);
		}

		public void TestValidateReceivingForwarderPK()
		{
			Validation.ValidateReceivingForwarderPK();
			AssertMandatoryValidationError(PreShipmentWrapper.ReceivingForwarderPKInfo, true);
			PreShipmentWrapper.ReceivingForwarderPK = ZGuid.NewZGuid();
			AssertMandatoryValidationError(PreShipmentWrapper.ReceivingForwarderPKInfo, false);
			AssertListValidationInvalidCodeError(PreShipmentWrapper.ReceivingForwarderPKInfo, true);
			JASOrgHeader newOrg = (JASOrgHeader)PreShipmentWrapper.ReceivingForwarders.AddNew(typeof(JASOrgHeader));
			PreShipmentWrapper.SendingForwarderPK = newOrg.PK;
			PreShipmentWrapper.ReceivingForwarderPK = newOrg.PK;
			AssertListValidationInvalidCodeError(PreShipmentWrapper.ReceivingForwarderPKInfo, false);
			Assert("Should not be the same as Sending Forwarder", PreShipmentWrapper.ReceivingForwarderPKInfo.HasErrors());
			PreShipmentWrapper.SendingForwarderPK = ZGuid.Empty;
			PreShipmentWrapper.Validation.ValidateReceivingForwarderPK();
			string expectedWarning1 = JXCConstants.JXCWarningPrefix + "Forwarder does not have JAS Netting Code";
			string expectedWarning2 = JXCConstants.JXCWarningPrefix + "Forwarder does not have JAS Office Code";
			AssertHasJXCWarning(PreShipmentWrapper.ReceivingForwarderPKInfo, expectedWarning1, expectedWarning2);
			newOrg.OfficeCode = "AUSYD";
			PreShipmentWrapper.Validation.ValidateReceivingForwarderPK();
			AssertHasJXCWarning(PreShipmentWrapper.ReceivingForwarderPKInfo, expectedWarning1);
			newOrg.NettingCode = "AUCOR";
			PreShipmentWrapper.Validation.ValidateReceivingForwarderPK();
			AssertHasNoJXCWarnings(PreShipmentWrapper.ReceivingForwarderPKInfo);
		}

		PreShipmentWrapperValidation Validation
		{
			get
			{
				if (fValidation == null)
				{
					fValidation = new PreShipmentWrapperValidation(PreShipmentWrapper);
				}

				return fValidation;
			}
		}

		PreShipmentWrapper PreShipmentWrapper
		{
			get
			{
				if (fPreShipmentWrapper == null)
				{
					JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
					fPreShipmentWrapper = new PreShipmentWrapper(shipment);
				}

				return fPreShipmentWrapper;
			}
		}

		PreShipmentWrapperValidation fValidation;
		PreShipmentWrapper fPreShipmentWrapper;
	}
}
