using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.JXC.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations.Testing
{
	internal class JXCExportHeaderValidationHelperTest : JXCValidationTestCase
	{
		[ExpectNoExceptions]
		public void TestValidateSendingForwarder_NullParams()
		{
			Helper.ValidateSendingForwarder(null, null);
			Helper.ValidateSendingForwarder(null, HeaderData);
			Helper.ValidateSendingForwarder(Dummy.Z0_GuidInfo, null);
		}

		public void TestValidateSendingForwarder_NotEntered()
		{
			Helper.ValidateSendingForwarder(Dummy.Z0_GuidInfo, HeaderData);
			AssertHasNotEnteredJXCWarning(Dummy.Z0_GuidInfo);
		}

		public void TestValidateSendingForwarder_NoNettingAndOfficeCode()
		{
			HeaderData.SendingForwarder = Factory.New<JASOrgHeader>();
			HeaderData.SendingForwarder.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			Dummy.Z0_Guid = HeaderData.SendingForwarder.PK;
			Helper.ValidateSendingForwarder(Dummy.Z0_GuidInfo, HeaderData);
			string expectedNettingCodeWarning = JXCConstants.JXCWarningPrefix + "Forwarder does not have JAS Netting Code";
			string expectedOfficeCodeWarning = JXCConstants.JXCWarningPrefix + "Forwarder does not have JAS Office Code";
			AssertHasJXCWarning(Dummy.Z0_GuidInfo, expectedNettingCodeWarning, expectedOfficeCodeWarning);
		}

		public void TestValidateSendingForwarder_NotControlledOrProxyToTheCurrentBranch()
		{
			HeaderData.SendingForwarder = Factory.New<JASOrgHeader>();
			HeaderData.SendingForwarder.OfficeCode = "AUSYD";
			HeaderData.SendingForwarder.NettingCode = "AUCOR";
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			Dummy.Z0_Guid = HeaderData.SendingForwarder.PK;
			Dummy.Z0_GuidInfo.ClearAllNotifications();
			Helper.ValidateSendingForwarder(Dummy.Z0_GuidInfo, HeaderData);
			AssertHasJXCWarning(Dummy.Z0_GuidInfo, JXCConstants.JXCWarningPrefix + "The Sending Forwarder is not controlled by or a proxy to the current Branch. Please configure the Controlling Branch from the Organisation screen or relogin");
			Dummy.Z0_GuidInfo.ClearAllNotifications();
			GlbBranch otherBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK));
			HeaderData.SendingForwarder.CompanyData.OB_GB_ControllingBranch = otherBranch.PK;
			Helper.ValidateSendingForwarder(Dummy.Z0_GuidInfo, HeaderData);
			AssertHasJXCWarning(Dummy.Z0_GuidInfo, JXCConstants.JXCWarningPrefix + "The Sending Forwarder is not controlled by or a proxy to the current Branch. Please configure the Controlling Branch from the Organisation screen or relogin");
			Dummy.Z0_GuidInfo.ClearAllNotifications();
			HeaderData.SendingForwarder.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			Helper.ValidateSendingForwarder(Dummy.Z0_GuidInfo, HeaderData);
			AssertHasNoJXCWarnings(Dummy.Z0_GuidInfo);
			Dummy.Z0_GuidInfo.ClearAllNotifications();
			HeaderData.SendingForwarder.CompanyData.OB_GB_ControllingBranch = ZGuid.Empty;
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = HeaderData.SendingForwarder.PK;
			Helper.ValidateSendingForwarder(Dummy.Z0_GuidInfo, HeaderData);
			AssertHasNoJXCWarnings(Dummy.Z0_GuidInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			SuspendValidationTestingDisposable = Dummy.SuspendValidationTesting();
		}

		protected override void TearDown()
		{
			SuspendValidationTestingDisposable.Dispose();
			base.TearDown();
		}

		JXCHeaderForTest HeaderData
		{
			get
			{
				if (fHeaderData == null)
				{
					fHeaderData = new JXCHeaderForTest();
				}

				return fHeaderData;
			}
		}

		DummyBusinessObject Dummy
		{
			get
			{
				if (fDummy == null)
				{
					fDummy = Factory.New<DummyBusinessObject>();
				}

				return fDummy;
			}
		}

		JXCExportHeaderValidationHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new JXCExportHeaderValidationHelper();
				}

				return fHelper;
			}
		}

		JXCHeaderForTest fHeaderData;
		JXCExportHeaderValidationHelper fHelper;
		DummyBusinessObject fDummy;
		IDisposable SuspendValidationTestingDisposable;
	}
}
