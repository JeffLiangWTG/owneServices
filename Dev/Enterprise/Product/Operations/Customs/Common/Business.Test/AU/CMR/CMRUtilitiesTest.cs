using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Integration.Customs.AU;

namespace Enterprise.Customs.Common.AU.CMR.Testing
{
	public class CMRUtilitiesTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestShouldImportMessageBeSentCMRDefault()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.Default;
			CMRUtilities utilities = new CMRUtilities();
			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(preGoLiveDate, preGoLiveDate, false, false), Is.EqualTo(false), "ShouldMessageBeSentCMR");
			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(preGoLiveDate, postGoLiveDate, false, false), Is.EqualTo(false), "ShouldMessageBeSentCMR");
			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(preGoLiveDate, postCutOverDate, false, false), Is.EqualTo(false), "ShouldMessageBeSentCMR");

			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(postGoLiveDate, preGoLiveDate, false, false), Is.EqualTo(false), "ShouldMessageBeSentCMR");
			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(postGoLiveDate, postGoLiveDate, false, false), Is.EqualTo(false), "ShouldMessageBeSentCMR");
			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(postGoLiveDate, postCutOverDate, false, false), Is.EqualTo(true), "ShouldMessageBeSentCMR");

			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(postCutOverDate, preGoLiveDate, false, false), Is.EqualTo(true), "ShouldMessageBeSentCMR");
			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(postCutOverDate, postGoLiveDate, false, false), Is.EqualTo(true), "ShouldMessageBeSentCMR");
			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(postCutOverDate, postCutOverDate, false, false), Is.EqualTo(true), "ShouldMessageBeSentCMR");

			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(postCutOverDate, preGoLiveDate, false, true), Is.EqualTo(true), "ShouldMessageBeSentCMR");
			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(postCutOverDate, postGoLiveDate, false, true), Is.EqualTo(true), "ShouldMessageBeSentCMR");
			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(postCutOverDate, postCutOverDate, false, true), Is.EqualTo(true), "ShouldMessageBeSentCMR");
		}

		[ExpectNoExceptions]
		public void TestShouldImportMessageBeSentCMRForceCMR()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			CMRUtilities utilities = new CMRUtilities();
			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(preGoLiveDate, preGoLiveDate, false, false), Is.EqualTo(true), "ShouldMessageBeSentCMR");
			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(preGoLiveDate, postGoLiveDate, false, false), Is.EqualTo(true), "ShouldMessageBeSentCMR");
			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(preGoLiveDate, postCutOverDate, false, false), Is.EqualTo(true), "ShouldMessageBeSentCMR");

			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(postGoLiveDate, preGoLiveDate, false, false), Is.EqualTo(true), "ShouldMessageBeSentCMR");
			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(postGoLiveDate, postGoLiveDate, false, false), Is.EqualTo(true), "ShouldMessageBeSentCMR");
			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(postGoLiveDate, postCutOverDate, false, false), Is.EqualTo(true), "ShouldMessageBeSentCMR");

			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(postCutOverDate, preGoLiveDate, false, false), Is.EqualTo(true), "ShouldMessageBeSentCMR");
			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(postCutOverDate, postGoLiveDate, false, false), Is.EqualTo(true), "ShouldMessageBeSentCMR");
			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(postCutOverDate, postCutOverDate, false, false), Is.EqualTo(true), "ShouldMessageBeSentCMR");

			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(postCutOverDate, preGoLiveDate, false, true), Is.EqualTo(true), "ShouldMessageBeSentCMR");
			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(postCutOverDate, postGoLiveDate, false, true), Is.EqualTo(true), "ShouldMessageBeSentCMR");
			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(postCutOverDate, postCutOverDate, false, true), Is.EqualTo(true), "ShouldMessageBeSentCMR");
		}

		[ExpectNoExceptions]
		public void TestShouldImportMessageBeSentCMRForceExit()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;

			CMRUtilities utilities = new CMRUtilities();
			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(preGoLiveDate, preGoLiveDate, false, false), Is.EqualTo(false), "ShouldMessageBeSentCMR");
			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(preGoLiveDate, postGoLiveDate, false, false), Is.EqualTo(false), "ShouldMessageBeSentCMR");
			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(preGoLiveDate, postCutOverDate, false, false), Is.EqualTo(false), "ShouldMessageBeSentCMR");

			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(postGoLiveDate, preGoLiveDate, false, false), Is.EqualTo(false), "ShouldMessageBeSentCMR");
			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(postGoLiveDate, postGoLiveDate, false, false), Is.EqualTo(false), "ShouldMessageBeSentCMR");
			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(postGoLiveDate, postCutOverDate, false, false), Is.EqualTo(false), "ShouldMessageBeSentCMR");

			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(postCutOverDate, preGoLiveDate, false, false), Is.EqualTo(false), "ShouldMessageBeSentCMR");
			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(postCutOverDate, postGoLiveDate, false, false), Is.EqualTo(false), "ShouldMessageBeSentCMR");
			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(postCutOverDate, postCutOverDate, false, false), Is.EqualTo(false), "ShouldMessageBeSentCMR");

			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(postCutOverDate, preGoLiveDate, false, true), Is.EqualTo(true), "ShouldMessageBeSentCMR");
			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(postCutOverDate, postGoLiveDate, false, true), Is.EqualTo(true), "ShouldMessageBeSentCMR");
			NUnit.Framework.Assert.That(utilities.ShouldImportMessageBeSentCMR(postCutOverDate, postCutOverDate, false, true), Is.EqualTo(true), "ShouldMessageBeSentCMR");
		}

		[ExpectNoExceptions]
		public void TestAreWeRunningInCMRDefault()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.Default;
			CMRUtilities utilities = new CMRUtilities();
			NUnit.Framework.Assert.That(utilities.AreWeRunningInCMR(preGoLiveDate), Is.EqualTo(false), "AreWeRunningInCMR");
			NUnit.Framework.Assert.That(utilities.AreWeRunningInCMR(postGoLiveDate), Is.EqualTo(true), "AreWeRunningInCMR");
			NUnit.Framework.Assert.That(utilities.AreWeRunningInCMR(postCutOverDate), Is.EqualTo(true), "AreWeRunningInCMR");
		}

		[ExpectNoExceptions]
		public void TestAreWeRunningInCMRForceCMR()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			CMRUtilities utilities = new CMRUtilities();
			NUnit.Framework.Assert.That(utilities.AreWeRunningInCMR(preGoLiveDate), Is.EqualTo(true), "AreWeRunningInCMR");
			NUnit.Framework.Assert.That(utilities.AreWeRunningInCMR(postGoLiveDate), Is.EqualTo(true), "AreWeRunningInCMR");
			NUnit.Framework.Assert.That(utilities.AreWeRunningInCMR(postCutOverDate), Is.EqualTo(true), "AreWeRunningInCMR");
		}

		[ExpectNoExceptions]
		public void TestAreWeRunningInCMRForceExit()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			CMRUtilities utilities = new CMRUtilities();
			NUnit.Framework.Assert.That(utilities.AreWeRunningInCMR(preGoLiveDate), Is.EqualTo(false), "AreWeRunningInCMR");
			NUnit.Framework.Assert.That(utilities.AreWeRunningInCMR(postGoLiveDate), Is.EqualTo(false), "AreWeRunningInCMR");
			NUnit.Framework.Assert.That(utilities.AreWeRunningInCMR(postCutOverDate), Is.EqualTo(false), "AreWeRunningInCMR");
		}

		[ExpectNoExceptions]
		public void TestGetCertificateErrorsNoErrors()
		{
			SetupCertificates();
			string[] result = new CMRUtilities().GetCertificateErrors(Factory);
			NUnit.Framework.Assert.That(result.Length, Is.EqualTo(0));
		}

		[ExpectNoExceptions]
		public void TestGetCertificateErrorsMissingPassword()
		{
			SetupCertificates();
			string[] result = new CMRUtilities().GetCertificateErrors(Factory);
			NUnit.Framework.Assert.That(result.Length, Is.EqualTo(0));
			Env.Registry.AUCCompanyCertificatePassword = ZString.Empty;
			result = new CMRUtilities().GetCertificateErrors(Factory);
			NUnit.Framework.Assert.That(result.Length, Is.EqualTo(1));
			NUnit.Framework.Assert.That(result[0], Is.EqualTo("There is no password for the Company Private Key File."), "Certificate Missing Password");
		}

		[ExpectNoExceptions]
		public void TestGetCertificateErrorsMissingCertificate()
		{
			SetupCertificates();
			string[] result = new CMRUtilities().GetCertificateErrors(Factory);
			NUnit.Framework.Assert.That(result.Length, Is.EqualTo(0));
			Env.Registry.AUCCompanyCertificateData = Array.Empty<byte>();
			result = new CMRUtilities().GetCertificateErrors(Factory);
			NUnit.Framework.Assert.That(result.Length, Is.EqualTo(1));
			NUnit.Framework.Assert.That(result[0], Is.EqualTo("There is no Company Private Key File."), "Certificate Missing Certificate File");
		}

		[ExpectNoExceptions]
		public void TestConvertOldAirCargoPaymentType()
		{
			NUnit.Framework.Assert.That(new CMRUtilities().ConvertOldAirCargoPaymentType("CCX"), Is.EqualTo(CMRMethodsOfPayment.Codes.Collect).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(new CMRUtilities().ConvertOldAirCargoPaymentType("PPD"), Is.EqualTo(CMRMethodsOfPayment.Codes.PrepaidOnly).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestMessageStatusChanges()
		{
			RunOneMessageStatusTest(CMRBaseStatuses.Codes.OriginalAccepted, true, false, false, false);
			RunOneMessageStatusTest(CMRBaseStatuses.Codes.AmendmentAccepted, true, false, false, false);
			RunOneMessageStatusTest(CMRBaseStatuses.Codes.WithdrawalAccepted, true, false, false, false);
			RunOneMessageStatusTest(CMRBaseStatuses.Codes.OriginalRejected, false, true, false, false);
			RunOneMessageStatusTest(CMRBaseStatuses.Codes.AmendmentRejected, false, true, false, false);
			RunOneMessageStatusTest(CMRBaseStatuses.Codes.WithdrawalRejected, false, true, false, false);
			RunOneMessageStatusTest(CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal, false, false, true, false);
			RunOneMessageStatusTest(CMRBaseStatuses.Codes.AwaitingResponseToOriginal, false, false, false, true);
			RunOneMessageStatusTest(CMRBaseStatuses.Codes.AwaitingResponseToAmendment, false, false, false, true);
		}

		[ExpectNoExceptions]
		void RunOneMessageStatusTest(ZString statusCode, bool expectedAcceptedResult, bool expectedRejectedResult, bool expectedWithdrawResult, bool expectedWaitingResult)
		{
			CusEntryNumber dummyBizo = Factory.New<CusEntryNumber>();
			dummyBizo.CE_EntryStatus = CMRBaseStatuses.Codes.NotSent;
			dummyBizo.CE_ParentTable = "JobDeclaration";
			Factory.Save();
			dummyBizo.CE_EntryStatus = statusCode;
			NUnit.Framework.Assert.That(CMRUtilities.MessageStatusChangedToAccepted(dummyBizo.CE_EntryStatusInfo), Is.EqualTo(expectedAcceptedResult), "Accepted " + statusCode);
			NUnit.Framework.Assert.That(CMRUtilities.MessageStatusChangedToRejected(dummyBizo.CE_EntryStatusInfo), Is.EqualTo(expectedRejectedResult), "Rejected " + statusCode);
			NUnit.Framework.Assert.That(CMRUtilities.MessageStatusChangedToWithdrawWaiting(dummyBizo.CE_EntryStatusInfo), Is.EqualTo(expectedWithdrawResult), "Withdraw " + statusCode);
			NUnit.Framework.Assert.That(CMRUtilities.MessageStatusChangedToWaiting(dummyBizo.CE_EntryStatusInfo), Is.EqualTo(expectedWaitingResult), "Waiting " + statusCode);
			dummyBizo.Delete();
			Factory.Save();
		}

		[ExpectNoExceptions]
		public void TestGetCertificateErrorsFactoryHits()
		{
			var otherFactory = new BusinessObjectFactory();
			var cmrUtilities = new CMRUtilities();

			var errors = cmrUtilities.GetCertificateErrors(otherFactory);
			var initialHitcount = otherFactory.GetTableHitCount(RefSysConfigSchema.Constants.TableName);
			NUnit.Framework.Assert.That(initialHitcount, Is.GreaterThan(0));

			var errors2 = cmrUtilities.GetCertificateErrors(otherFactory);
			var updatedHitcount = otherFactory.GetTableHitCount(RefSysConfigSchema.Constants.TableName);

			NUnit.Framework.Assert.That(updatedHitcount, Is.EqualTo(initialHitcount));
		}

		#region Implementation

		public static void SetupCertificates()
		{
			var factory = new BusinessObjectFactory();
			var certificatesHelper = ObjectFactory.New<ICertificateManagerHelper>(factory);
			certificatesHelper.CreateCustomsCertificates2004();

			Env.Registry.AUCCompanyCertificateData = new byte[] { 1, 2, 3 };
			using (Env.Registry.RawRegistry.AUCCompanyCertificatePassword.DataType.SuspendValidation())
			{
				Env.Registry.AUCCompanyCertificatePassword = certificatesHelper.AUCCompanyCertificatePasswordForTest;
			}

			factory.Save();
		}

		#endregion

		protected ZDateTime preGoLiveDate = Core.Constants.AUCustoms.CMRImportsGoLiveDate.AddDays(-1);
		protected ZDateTime postGoLiveDate = Core.Constants.AUCustoms.CMRImportsGoLiveDate.AddDays(1);
		protected ZDateTime postCutOverDate = Core.Constants.AUCustoms.CMRImportsCutOverDate.AddDays(1);
	}
}
