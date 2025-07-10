using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.Cryptoki.Common.ClientServerApi;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.DataTransfer.EInvoicing.Egypt;
using Enterprise.Accounting.GUI.EInvoicing.HardwareTokenSigning;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.GUI.Testing.EInvoicing.HardwareTokenSigning
{
	[TestedType(typeof(ESigningBusinessObject))]
	sealed class ESigningBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ESigningBusinessObject(null, null, null);
		}

		public void TestAvailableChipsetTypesList_ProductionSystem()
		{
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			var productionESigningBusinessObject = (ESigningBusinessObject)GetNewBusinessObject();
			var expectedChipsets = Enum.GetNames(typeof(Chipset));
			AssertContainsExactElementsInExactOrder("Chipset List should not include Windows Token on production licensed systems", expectedChipsets, productionESigningBusinessObject.AvailableChipsetTypes.GetAllCodes());
		}

		public void TestAvailableChipsetTypesList_NonProductionSystem()
		{
			var testingESigningBusinessObject = (ESigningBusinessObject)GetNewBusinessObject();
			var expectedChipsets = Enum.GetNames(typeof(Chipset))
				.Prepend(ESigningBusinessObject.WindowsToken);
			AssertContainsExactElementsInExactOrder("Chipset List", expectedChipsets, testingESigningBusinessObject.AvailableChipsetTypes.GetAllCodes());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAvailableCertificates()
		{
			string baseFolder = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.Module\Transaction\TestFiles\";

			var mockCryptoApi = new Moq.Mock<ICryptoApi>();
			mockCryptoApi.Setup(x => x.GetCertificatesFromToken(Moq.It.IsAny<Chipset>()))
						.Returns(new CertificateInfo[]
						{
							new CertificateInfo() { Content = System.IO.File.ReadAllBytes(baseFolder + "Root Certificate.cer") },
							new CertificateInfo() { Content = System.IO.File.ReadAllBytes(baseFolder + "2021 Self Signed Certificate.cer") },
						});
			var eSigningBusinessObject = new ESigningBusinessObject(null, null, null, mockCryptoApi.Object);
			eSigningBusinessObject.ChipsetType = nameof(Chipset.AKIS);

			AssertNull(eSigningBusinessObject.HardwareTokenErrorForCurrentChipset);

			var expectedAvailableCertificateCodes = new CodeDescriptionPairList();
			expectedAvailableCertificateCodes.AddPair("1", "CN=EEI-RootCA-PREPROD, O=ETA, C=EG");
			expectedAvailableCertificateCodes.AddPair("2", "CN=Murray Grant TEST Code Signing");
			AssertContainsExactElementsInExactOrder(expectedAvailableCertificateCodes, eSigningBusinessObject.AvailableCertificateCodesForCurrentChipset);

			var expectedSerialNumbers = new string[2];
			expectedSerialNumbers[0] = "27FACEF6632E6C8E4E086214CF6C9BE6";
			expectedSerialNumbers[1] = "4DD6A8B5815DE7B04D50C077037F9989";
			AssertContainsExactElementsInExactOrder(expectedSerialNumbers, eSigningBusinessObject.SerialNumbersForCurrentChipset);
		}

		public void TestAvailableCertificates_HandlesExpectedExceptions()
		{
			var mockCryptoApi = new Moq.Mock<ICryptoApi>();
			mockCryptoApi.Setup(x => x.GetCertificatesFromToken(Moq.It.IsAny<Chipset>()))
						.Throws<IOException>();
			var eSigningBusinessObject = new ESigningBusinessObject(null, null, null, mockCryptoApi.Object);
			eSigningBusinessObject.ChipsetType = nameof(Chipset.AKIS);
			AssertNull(eSigningBusinessObject.AvailableCertificateCodesForCurrentChipset);
			AssertNull(eSigningBusinessObject.SerialNumbersForCurrentChipset);
			AssertType(typeof(IOException), eSigningBusinessObject.HardwareTokenErrorForCurrentChipset);

			mockCryptoApi.Setup(x => x.GetCertificatesFromToken(Moq.It.IsAny<Chipset>()))
						.Throws<System.Security.Cryptography.CryptographicException>();
			eSigningBusinessObject = new ESigningBusinessObject(null, null, null, mockCryptoApi.Object);
			eSigningBusinessObject.ChipsetType = nameof(Chipset.AKIS);
			AssertNull(eSigningBusinessObject.AvailableCertificateCodesForCurrentChipset);
			AssertNull(eSigningBusinessObject.SerialNumbersForCurrentChipset);
			AssertType(typeof(System.Security.Cryptography.CryptographicException), eSigningBusinessObject.HardwareTokenErrorForCurrentChipset);

			mockCryptoApi.Setup(x => x.GetCertificatesFromToken(Moq.It.IsAny<Chipset>()))
						.Throws(new InvalidOperationException("Some XML parsing error", new InvalidOperationException("Chipset '12345' is not valid")));
			eSigningBusinessObject = new ESigningBusinessObject(null, null, null, mockCryptoApi.Object);
			eSigningBusinessObject.ChipsetType = nameof(Chipset.AKIS);
			AssertNull(eSigningBusinessObject.AvailableCertificateCodesForCurrentChipset);
			AssertNull(eSigningBusinessObject.SerialNumbersForCurrentChipset);
			AssertType(typeof(InvalidOperationException), eSigningBusinessObject.HardwareTokenErrorForCurrentChipset);
			AssertType(typeof(InvalidOperationException), eSigningBusinessObject.HardwareTokenErrorForCurrentChipset.InnerException);
			AssertEquals("Some XML parsing error", eSigningBusinessObject.HardwareTokenErrorForCurrentChipset.Message);
			AssertEquals("Chipset '12345' is not valid", eSigningBusinessObject.HardwareTokenErrorForCurrentChipset.InnerException.Message);

			mockCryptoApi.Setup(x => x.GetCertificatesFromToken(Moq.It.IsAny<Chipset>()))
						.Throws<NullReferenceException>();
			eSigningBusinessObject = new ESigningBusinessObject(null, null, null, mockCryptoApi.Object);
			AssertExceptionThrown<NullReferenceException>(() => { eSigningBusinessObject.ChipsetType = nameof(Chipset.AKIS); });
		}

		#region SignElectronicInvoices

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSignElectronicInvoicesWithHardwareToken_Success()
		{
			AssertSignElectronicInvoices_Success(nameof(Chipset.EPASS2003));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSignElectronicInvoicesWithWindows_Success()
		{
			AssertSignElectronicInvoices_Success(ESigningBusinessObject.WindowsToken);
		}

		void AssertSignElectronicInvoices_Success(string chipset)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Egypt))
			{
				SetupControlAccounts();
				GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CorporationCode, "198742", CountryCodes.Egypt);
				GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EgyptCodeTypes.CommercialRegistrationNumber, "09876223", CountryCodes.Egypt);
				GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GovBusinessCode, "7482", CountryCodes.Egypt);
				GlbBranch.CurrentBranch.OrgProxy.Factory.Save();
				TestObjectCreator.AALSHI.CustomsCodes.AddNew("GST", "123456");
				TestObjectCreator.AALSHI.Factory.Save();

				const string authorisationRecordType = AccTransactionHeaderAuthorisationRecordTypes.Egypt;

				var pendingInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
				var pendingInvoicePivot = TestObjectCreator.CreateEInvoicingTransactionPivot(pendingInvoice, status: EInvoicingPivotState.Pending);
				var pendingCreditNote = TestObjectCreator.CreateARCreditNote("AR002", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "", new ZDateTime(2021, 05, 01), true);
				var pendingCreditNotePivot = TestObjectCreator.CreateEInvoicingTransactionPivot(pendingCreditNote, status: EInvoicingPivotState.Pending);
				var pendingAdjustmentNote = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("AR003", 1m, 0m, new ZDateTime(2021, 05, 01), TestObjectCreator.AALSHI.PK);
				var pendingAdjustmentNotePivot = TestObjectCreator.CreateEInvoicingTransactionPivot(pendingAdjustmentNote, status: EInvoicingPivotState.Pending);
				var previouslyMappedInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR004", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
				var previouslyMappedInvoicePivot = TestObjectCreator.CreateEInvoicingTransactionPivot(previouslyMappedInvoice, status: EInvoicingPivotState.Pending);
				var previouslyMappedAuthRecord = previouslyMappedInvoice.Factory.New<AccTransactionHeaderAuthorisationRecord>();
				previouslyMappedAuthRecord.AHF_ParentId = previouslyMappedInvoice.PK;
				previouslyMappedAuthRecord.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
				previouslyMappedAuthRecord.AHF_RecordType = authorisationRecordType;
				previouslyMappedAuthRecord.AHF_AuthorisationData = ZBlob.FromUTF8("<document>previously mapped document</document>");

				var mockCryptoApi = new Moq.Mock<ICryptoApi>();
				var signatureData = ZBlob.FromUTF8("exactly 32 byte fake signature.");
				mockCryptoApi.Setup(x => x.SignWithToken(Moq.It.IsAny<Chipset>(), Moq.It.IsNotNull<string>(), Moq.It.IsNotNull<byte[]>(), Moq.It.IsNotNull<byte[]>()))
							.Returns(signatureData);
				string base64CertificateData;
				using (StreamReader streamReader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Business.Testing\eInvoicing\HardwareTokenSigning\TestFiles\TestCertificateData.cer"))
				{
					base64CertificateData = streamReader.ReadToEnd();
				}
				var certificateData = Convert.FromBase64String(base64CertificateData);
				mockCryptoApi.Setup(x => x.GetCertificatesFromToken(Moq.It.IsAny<Chipset>()))
							.Returns(new CertificateInfo[] { new CertificateInfo() { Content = certificateData } });
				mockCryptoApi.Setup(x => x.GetCertificatesFromWindowsCertificateStore())
							.Returns(new CertificateInfo[] { new CertificateInfo() { Content = certificateData } });
				mockCryptoApi.Setup(x => x.GetCertificateFromToken(Moq.It.IsAny<Chipset>(), Moq.It.IsNotNull<byte[]>()))
							.Returns(new CertificateInfo() { Content = certificateData });
				mockCryptoApi.Setup(x => x.GetCertificateFromWindowsCertificateStore(Moq.It.IsNotNull<byte[]>()))
							.Returns(new CertificateInfo() { Content = certificateData });

				Factory.Save();

				var transactions = new TransactionHeader[]
				{
					pendingInvoice,
					pendingCreditNote,
					pendingAdjustmentNote,
					previouslyMappedInvoice,
				};
				var mapper = new InvoicingBaseToXmlConverter();
				var eSigningBusinessObject = new ESigningBusinessObject(transactions, mapper, null, mockCryptoApi.Object);
				eSigningBusinessObject.ChipsetType = chipset;
				eSigningBusinessObject.CertificateCode = "1";
				eSigningBusinessObject.EnteredPin = "1234";
				eSigningBusinessObject.MapAndSignAndQueueElectronicInvoices();

				AssertEquals("Four transactions should be successful", 4, eSigningBusinessObject.SuccessCount);
				AssertEquals("Zero transaction should fail", 0, eSigningBusinessObject.ErrorCount);

				var newFactory = Factory.CreateNewFactory();
				AssertInvoiceQueued(pendingInvoice, "Pending Invoice");
				AssertInvoiceQueued(pendingCreditNote, "Pending Credit Note");
				AssertInvoiceQueued(pendingAdjustmentNote, "Pending Adjustment Note");
				AssertInvoiceQueued(previouslyMappedInvoice, "Previously Mapped Invoice");

				void AssertInvoiceQueued(InvoicingBase originalInvoice, string messagePrefix)
				{
					var reloadedInvoice = newFactory.Load<InvoicingBase>(originalInvoice.PK);
					AssertEquals(messagePrefix + " should now be in QUE status", EInvoicingPivotState.Queued, reloadedInvoice.EInvoicingStatus);
					AssertEquals(messagePrefix + " should now have no errors", "", reloadedInvoice.EInvoicingError);

					var reloadedAuthRecord = GetAuthorisationRecord(reloadedInvoice, authorisationRecordType);
					AssertNotNull(messagePrefix + " should have an auth record as it was successful", reloadedAuthRecord);
					Assert(messagePrefix + " auth record should be in database", reloadedAuthRecord.IsInDatabase);
					var xml = reloadedAuthRecord.AHF_AuthorisationData.ToUTF8();
					AssertNotNullOrEmpty(messagePrefix + " auth record should have some data", xml);
					var xmlDoc = new XmlDocument();
					AssertNoExceptionThrown(messagePrefix + " auth record should have mapped XML document", () => xmlDoc.LoadXml(xml));
					AssertNotEquals(messagePrefix + " auth record XML should not be previously mapped", "<document>previously mapped document</document>", xml);
					AssertContains(messagePrefix + " auth record XML should contain a signature element", "<signature>", xml);
					Assert(messagePrefix + " auth record XML should contain a signature value, which is not deterministic", Regex.IsMatch(xml, "<value>[a-zA-Z0-9=+\\/]{1,}<\\/value>"));
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSignElectronicInvoicesWithHardwareToken_Failure()
		{
			AssertSignElectronicInvoices_Failure(nameof(Chipset.EPASS2003));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSignElectronicInvoicesWithWindows_Failure()
		{
			AssertSignElectronicInvoices_Failure(ESigningBusinessObject.WindowsToken);
		}

		public void AssertSignElectronicInvoices_Failure(string chipset)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Egypt))
			{
				SetupControlAccounts();
				var pendingInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
				var pendingInvoicePivot = TestObjectCreator.CreateEInvoicingTransactionPivot(pendingInvoice, status: EInvoicingPivotState.Pending);
				var pendingCreditNote = TestObjectCreator.CreateARCreditNote("AR002", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "", new ZDateTime(2021, 05, 01), true);
				var pendingCreditNotePivot = TestObjectCreator.CreateEInvoicingTransactionPivot(pendingCreditNote, status: EInvoicingPivotState.Pending);

				var mockCryptoApi = new Moq.Mock<ICryptoApi>();
				mockCryptoApi.Setup(x => x.SignWithToken(Moq.It.IsAny<Chipset>(), Moq.It.IsNotNull<string>(), Moq.It.IsNotNull<byte[]>(), Moq.It.IsNotNull<byte[]>()))
							.Throws<InvalidOperationException>();
				string base64CertificateData;
				using (StreamReader streamReader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Business.Testing\eInvoicing\HardwareTokenSigning\TestFiles\TestCertificateData.cer"))
				{
					base64CertificateData = streamReader.ReadToEnd();
				}
				var certificateData = Convert.FromBase64String(base64CertificateData);
				mockCryptoApi.Setup(x => x.GetCertificatesFromToken(Moq.It.IsAny<Chipset>()))
							.Returns(new CertificateInfo[] { new CertificateInfo() { Content = certificateData } });
				mockCryptoApi.Setup(x => x.GetCertificatesFromWindowsCertificateStore())
							.Returns(new CertificateInfo[] { new CertificateInfo() { Content = certificateData } });

				Factory.Save();

				var transactions = new TransactionHeader[]
				{
					pendingInvoice,
					pendingCreditNote,
				};

				var mapper = new InvoicingBaseToXmlConverter();
				var eSigningBusinessObject = new ESigningBusinessObject(transactions, mapper, null, mockCryptoApi.Object);
				eSigningBusinessObject.ChipsetType = chipset;
				eSigningBusinessObject.CertificateCode = "1";
				eSigningBusinessObject.EnteredPin = "1234";
				eSigningBusinessObject.MapAndSignAndQueueElectronicInvoices();

				AssertEquals("Zero transactions should be successful", 0, eSigningBusinessObject.SuccessCount);
				AssertEquals("Two transactions should fail", 2, eSigningBusinessObject.ErrorCount);

				var newFactory = Factory.CreateNewFactory();
				AssertInvoiceFailed(pendingInvoice, "Pending Invoice");
				AssertInvoiceFailed(pendingCreditNote, "Pending Credit Note");

				void AssertInvoiceFailed(InvoicingBase originalInvoice, string messagePrefix)
				{
					var reloadedInvoice = newFactory.Load<InvoicingBase>(originalInvoice.PK);
					AssertEquals(messagePrefix + " should now be in FAL status", EInvoicingPivotState.Failed, reloadedInvoice.EInvoicingStatus);
					AssertNotNullOrEmpty(messagePrefix + " should now have errors", reloadedInvoice.EInvoicingError);
					// No assertions for auth record: it is possible an old auth record exists, but FAL status takes precedence.
					// Eventually, when validation errors are fixed, the auth record data will be overwritten.
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSignElectronicInvoicesWithHardwareToken_Skipped()
		{
			AssertSignElectronicInvoices_Skipped(nameof(Chipset.EPASS2003));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSignElectronicInvoicesWithWindows_Skipped()
		{
			AssertSignElectronicInvoices_Skipped(ESigningBusinessObject.WindowsToken);
		}

		public void AssertSignElectronicInvoices_Skipped(string chipset)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Egypt))
			{
				const string authorisationRecordType = AccTransactionHeaderAuthorisationRecordTypes.Egypt;

				var sentInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
				var sentInvoicePivot = TestObjectCreator.CreateEInvoicingTransactionPivot(sentInvoice, status: EInvoicingPivotState.Sent);
				var noPivotInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR002", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
				var failedInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR003", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
				var failedInvoicePivot = TestObjectCreator.CreateEInvoicingTransactionPivot(failedInvoice, status: EInvoicingPivotState.Failed);
				failedInvoicePivot.AIP_ErrorDescription = "Some error from a previous submission attempt";

				var mockCryptoApi = new Moq.Mock<ICryptoApi>();
				mockCryptoApi.Setup(x => x.SignWithToken(Moq.It.IsAny<Chipset>(), Moq.It.IsAny<string>(), Moq.It.IsAny<byte[]>(), Moq.It.IsAny<byte[]>()))
							.Throws<InvalidOperationException>();
				string base64CertificateData;
				using (StreamReader streamReader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Business.Testing\eInvoicing\HardwareTokenSigning\TestFiles\TestCertificateData.cer"))
				{
					base64CertificateData = streamReader.ReadToEnd();
				}
				var certificateData = Convert.FromBase64String(base64CertificateData);
				mockCryptoApi.Setup(x => x.GetCertificatesFromToken(Moq.It.IsAny<Chipset>()))
							.Returns(new CertificateInfo[] { new CertificateInfo() { Content = certificateData } });
				mockCryptoApi.Setup(x => x.GetCertificatesFromWindowsCertificateStore())
							.Returns(new CertificateInfo[] { new CertificateInfo() { Content = certificateData } });

				Factory.Save();

				var transactions = new TransactionHeader[]
				{
					sentInvoice,
					noPivotInvoice,
					failedInvoice
				};

				var mapper = new InvoicingBaseToXmlConverter();
				var eSigningBusinessObject = new ESigningBusinessObject(transactions, mapper, null, mockCryptoApi.Object);
				eSigningBusinessObject.ChipsetType = chipset;
				eSigningBusinessObject.CertificateCode = "1";
				eSigningBusinessObject.EnteredPin = "1234";
				eSigningBusinessObject.MapAndSignAndQueueElectronicInvoices();

				AssertEquals("Zero transactions should be successful", 0, eSigningBusinessObject.SuccessCount);
				AssertEquals("Zero transaction should fail", 0, eSigningBusinessObject.ErrorCount);

				var newFactory = Factory.CreateNewFactory();
				AssertInvoiceUnchanged(sentInvoice, "Sent Invoice");
				AssertInvoiceUnchanged(noPivotInvoice, "No Pivot Invoice");
				AssertInvoiceUnchanged(failedInvoice, "Failed Invoice");

				void AssertInvoiceUnchanged(InvoicingBase originalInvoice, string messagePrefix)
				{
					var reloadedInvoice = newFactory.Load<InvoicingBase>(originalInvoice.PK);
					AssertEquals(messagePrefix + " should now be in original status", originalInvoice.EInvoicingStatus, reloadedInvoice.EInvoicingStatus);
					AssertEquals(messagePrefix + " should now have original errors", originalInvoice.EInvoicingError, reloadedInvoice.EInvoicingError);

					var reloadedAuthRecord = GetAuthorisationRecord(reloadedInvoice, authorisationRecordType);
					AssertNull(messagePrefix + " should not create an auth record as it was skipped", reloadedAuthRecord);
				}
			}
		}

		void SetupControlAccounts()
		{
			var aRSuspenseControlAccount = TestObjectCreator.CreateARSuspenseControlAccount();
			var aPSuspenseControlAccount = TestObjectCreator.CreateAPSuspenseControlAccount();
			var jobRevenueJournalControlAccount = TestObjectCreator.CreateJobRevenueJournalControlAccount();
			var cFXAccount = TestObjectCreator.CreateCFXAccount();
			var pendingInputTaxAccount = TestObjectCreator.CreateInputTaxReceivablePendingAccount();
			var pendingOutputTaxAccount = TestObjectCreator.CreateOutputTaxPayablePendingAccount();
			Factory.Save();

			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aRSuspenseControlAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aPSuspenseControlAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, jobRevenueJournalControlAccount.PK.ToGuid());
			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CES"));
			AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(Guid.Empty, Guid.Empty, department.PK.ToGuid(), cFXAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pendingInputTaxAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pendingOutputTaxAccount.PK.ToGuid());
		}

		static AccTransactionHeaderAuthorisationRecord GetAuthorisationRecord(TransactionHeader transactionHeader, ZString authRecordType)
		{
			var query = new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentTableCode, AccTransactionHeaderSchema.Constants.Prefix);
			query.AddToFilter(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, transactionHeader.PK);
			query.AddToFilter(AccTransactionHeaderAuthorisationRecordSchema.AHF_RecordType, authRecordType);
			return transactionHeader.Factory.LoadTop1<AccTransactionHeaderAuthorisationRecord>(query);
		}

		#endregion

		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fTestObjectCreator;
	}
}
