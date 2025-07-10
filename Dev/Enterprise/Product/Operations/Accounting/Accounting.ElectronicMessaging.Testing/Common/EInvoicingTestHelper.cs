using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class EInvoicingTestHelper
	{
		public EInvoicingTestHelper(TestObjectCreator testObjectCreator)
		{
			this.ObjectCreator = testObjectCreator;
		}

		public void SetupControlAccounts()
		{
			AccGLHeader aRSuspenseControlAccount = ObjectCreator.CreateARSuspenseControlAccount();
			AccGLHeader aPSuspenseControlAccount = ObjectCreator.CreateAPSuspenseControlAccount();
			AccGLHeader jobRevenueJournalControlAccount = ObjectCreator.CreateJobRevenueJournalControlAccount();
			AccGLHeader cFXAccount = ObjectCreator.CreateCFXAccount();
			AccGLHeader pendingInputTaxAccount = ObjectCreator.CreateInputTaxReceivablePendingAccount();
			AccGLHeader pendingOutputTaxAccount = ObjectCreator.CreateOutputTaxPayablePendingAccount();
			Factory.Save();

			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aRSuspenseControlAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aPSuspenseControlAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, jobRevenueJournalControlAccount.PK.ToGuid());
			GlbDepartment department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CES"));
			AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(Guid.Empty, Guid.Empty, department.PK.ToGuid(), cFXAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pendingInputTaxAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pendingOutputTaxAccount.PK.ToGuid());
		}

		public static GlobalElectronicInvoicing GetEInvoice(byte[] payload = null)
		{
			return new GlobalElectronicInvoicing()
			{
				Header = new GlobalElectronicInvoicingHeader()
				{
					ElectronicInvoiceBatchRequest = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest()
					{
						MessagingSystem = "Test Electronic Invoicing System",
						BatchNumber = "125896",
						MessageType = EInvoiceAPICommandList.Codes.GenerateInvoiceRequest
					}
				},

				Payload = payload != null ? MessageEncoding.UTF8WithoutBOM.GetString(payload) : "Test Payload",
				TransactionBatchSpecified = false,
			};
		}

		public static GlobalElectronicInvoicing GetEInvoiceWithComapnyAndBranch(byte[] payload = null)
		{
			var eInvoice = GetEInvoice(payload);
			eInvoice.Header.ElectronicInvoiceBatchRequest.CompanyCode = "COM";
			eInvoice.Header.ElectronicInvoiceBatchRequest.BranchCode = "BRN";
			return eInvoice;
		}

		public static DeliveryContext GetDeliveryContext(BusinessObjectFactory factory, BusinessObject parentBizO, string applicationCode, string messageType, string messageSubType, INotifications logger)
		{
			var context = new DeliveryContext(factory)
			{
				PurposeCode = ZString.Empty,
				ApplicationCode = applicationCode,
				MessageTypeCode = messageType,
				MessageSubTypeCode = messageSubType,
				Notifications = logger
			};

			if (parentBizO != null)
			{
				context.ParentInfo = EntityInfo.New(parentBizO);
			}

			return context;
		}

		public static NonPersistentEDICommunicationMode GetEHubMode()
		{
			return new NonPersistentEDICommunicationMode
			{
				EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML,
				EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService,
				EK_Destination = "TSTCompliance"
			};
		}

		public GlbCompany CreateCompanyAndBranch(string companyCode, string branchCode, string countryCode, bool enableEInvoicing)
		{
			var companyOrgProxy = ObjectCreator.CreateOrgHeader(companyCode + "PROXY", true, true);
			var company = ObjectCreator.CreateNewCompany(companyCode, countryCode, orgProxy: companyOrgProxy);
			company.GC_Name = companyCode + " Company";
			company.GC_BusinessRegNo = companyCode + "12345";

			var cusCode = companyOrgProxy.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = company.GC_RN_NKCountryCode;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			cusCode.OK_CustomsRegNo = "32323329";

			ObjectCreator.CreateNewBranch(company, branchCode);
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, enableEInvoicing);
			AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-7).ToDateTime());
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, enableEInvoicing);
			AccountingMasterFilesRegistry.Instance.EReportingComplianceDateForPayables.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-7).ToDateTime());
			Factory.Save();

			return company;
		}

		public string AddCustomsCodeForCountryIfMissing(OrgHeader orgHeader, ZString countryCode, ZString orgCusCode, string regNumber = null)
		{
			var result = string.IsNullOrEmpty(regNumber) ? TestObjectCreator.GetRandomInt(1000, 9999).ToString() : regNumber;

			var cusCode = orgHeader.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_RN_NKCodeCountry == countryCode && x.OK_CodeType == orgCusCode);
			if (cusCode == null)
			{
				cusCode = orgHeader.CustomsCodes.AddNew(orgCusCode, result);
				cusCode.OK_RN_NKCodeCountry = countryCode;
			}
			else
			{
				result = cusCode.OK_CustomsRegNo;
			}

			return result;
		}

		public void UpdateCustomsCodeForCountry(OrgHeader orgHeader, ZString countryCode, ZString orgCusCode, string regNumber)
		{
			var cusCode = orgHeader.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_RN_NKCodeCountry == countryCode && x.OK_CodeType == orgCusCode);
			if (cusCode == null)
			{
				cusCode = orgHeader.CustomsCodes.AddNew(orgCusCode, regNumber);
				cusCode.OK_RN_NKCodeCountry = countryCode;
			}
			else
			{
				cusCode.OK_CustomsRegNo = regNumber;
			}
		}

		public void CreateARAPINVCRDADJTransactions(GlbBranch branch, OrgHeader debtor, OrgHeader creditor, string transactionReference = null, RefCurrency currency = null, Action<ARInvoice, ARCreditNote, ARAdjustmentNote, APInvoice, APCreditNote, APAdjustmentNote> beforeSave = null)
		{
			currency = currency ?? ObjectCreator.EUR;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var job1 = ObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge1 = ObjectCreator.CreateCharge(job1, ObjectCreator.CC1, "charge1", currency, 10m, creditor, currency, 10m, debtor);
				charge1.JR_AT_SellGSTRate = ObjectCreator.ServiceTax.PK;

				var job2 = ObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge2 = ObjectCreator.CreateCharge(job2, ObjectCreator.CC1, "charge3", currency, 10m, creditor, currency, 10m, debtor);
				charge2.JR_AT_SellGSTRate = ObjectCreator.ServiceTax.PK;

				ObjectCreator.Factory.Save();

				var arInvoice = (ARInvoice)ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(5), currency, 1m, 10m, 0M, 10m, 0m, debtor, ObjectCreator.CC1.PK);
				arInvoice.AH_TransactionReference = transactionReference;
				arInvoice.Lines[0].AL_AT = charge1.JR_AT_SellGSTRate;
				var complianceSubTypeForEInvoicingInfo = CountryComplianceFactory.GetICountryEligibleComplianceSubTypeForEInvoice(arInvoice.Company.GC_RN_NKCountryCode);
				var complianceSubType = complianceSubTypeForEInvoicingInfo?.GetEligibleComplianceSubTypeListForEInvoicing().FirstOrDefault() ?? "01";
				arInvoice.AH_ComplianceSubType = complianceSubType;

				var taxTransaction = ObjectCreator.Factory.NewWithValidTestData<AccTaxTransaction>();
				taxTransaction.ATT_AH = arInvoice.PK;
				taxTransaction.ATT_TaxSystemCode = "ISS";

				var taxTransactionLinePivot = ObjectCreator.Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxTransaction.PK)).FirstOrDefault();
				taxTransactionLinePivot.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(arInvoice.Lines[0]));

				var arCreditNote = ObjectCreator.CreateARCreditNoteWithLine(TestObjectCreator.GetRandomString(3), debtor, currency, 1.0m, "Desc", job2, ObjectCreator.CC1, 10m, ZDateTime.Today, false);
				arCreditNote.AH_TransactionReference = transactionReference;
				var arCrediNoteLine = arCreditNote.Lines[0];
				var arCrediNoteCharge = ObjectCreator.CreateJobCharge(arCrediNoteLine, job2, ObjectCreator.CC1, currency);
				arCrediNoteLine.AL_AT = arCrediNoteCharge.JR_AT_SellGSTRate = ObjectCreator.ServiceTax.PK;
				arCrediNoteLine.AL_LocalExTaxAmount = arCrediNoteLine.AL_OSExTaxAmount = arCreditNote.AH_OSExTaxAmount;
				arCrediNoteLine.AL_LocalTaxAmount = arCrediNoteLine.AL_OSTaxAmount = 0m;

				taxTransaction = ObjectCreator.Factory.NewWithValidTestData<AccTaxTransaction>();
				taxTransaction.ATT_AH = arCreditNote.PK;
				taxTransaction.ATT_TaxSystemCode = "ISS";

				taxTransactionLinePivot = ObjectCreator.Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxTransaction.PK)).FirstOrDefault();
				taxTransactionLinePivot.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(arCreditNote.Lines[0]));

				var arAdjNote = ObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>(TestObjectCreator.GetRandomString(5), 100m, 0m, ZDateTime.Today, debtor.PK);
				arAdjNote.Lines.Add(ObjectCreator.CreateRevenueLine(charge2, arAdjNote.PK));

				taxTransaction = ObjectCreator.Factory.NewWithValidTestData<AccTaxTransaction>();
				taxTransaction.ATT_AH = arAdjNote.PK;
				taxTransaction.ATT_TaxSystemCode = "ISS";

				taxTransactionLinePivot = ObjectCreator.Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxTransaction.PK)).FirstOrDefault();
				taxTransactionLinePivot.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(arAdjNote.Lines[0]));

				var apInvoice = ObjectCreator.CreateAPInvoice<APInvoice>(TestObjectCreator.GetRandomString(5), currency, 1m, 10m, 0m, 0m, 10m, 0m, 0m, creditor);
				apInvoice.Lines.Add(ObjectCreator.CreateCostLine(charge1, apInvoice.PK));

				taxTransaction = ObjectCreator.Factory.NewWithValidTestData<AccTaxTransaction>();
				taxTransaction.ATT_AH = apInvoice.PK;
				taxTransaction.ATT_TaxSystemCode = "ISS";

				taxTransactionLinePivot = ObjectCreator.Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxTransaction.PK)).FirstOrDefault();
				taxTransactionLinePivot.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(apInvoice.Lines[0]));

				var apCreditNote = ObjectCreator.CreateAPCreditNote(TestObjectCreator.GetRandomString(3), creditor, currency, 1.0m, "Desc", ZDateTime.Today, false);

				var apAdjNote = ObjectCreator.CreateAdjustmentNote<APAdjustmentNote>(TestObjectCreator.GetRandomString(5), 10m, 0m, ZDateTime.Today, creditor.PK);
				apAdjNote.Lines.Add(ObjectCreator.CreateCostLine(charge2, apAdjNote.PK));

				taxTransaction = ObjectCreator.Factory.NewWithValidTestData<AccTaxTransaction>();
				taxTransaction.ATT_AH = apAdjNote.PK;
				taxTransaction.ATT_TaxSystemCode = "ISS";

				taxTransactionLinePivot = ObjectCreator.Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxTransaction.PK)).FirstOrDefault();
				taxTransactionLinePivot.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(apAdjNote.Lines[0]));

				if (debtor.MainAddress != null)
				{
					arInvoice.AH_OA_InvoiceAddressOverride = debtor.MainAddress.PK;
					arCreditNote.AH_OA_InvoiceAddressOverride = debtor.MainAddress.PK;
					arAdjNote.AH_OA_InvoiceAddressOverride = debtor.MainAddress.PK;
				}
				beforeSave?.Invoke(arInvoice, arCreditNote, arAdjNote, apInvoice, apCreditNote, apAdjNote);
				ObjectCreator.Factory.Save();
			}
		}

		public AccComplianceDocumentHeader CreateARComplianceDocumentWithQueuedStatus(GlbBranch branch, string transactionType = "INV", string subType = "TXE", string documentNumber = null)
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Type invoiceType;
				switch (transactionType)
				{
					case TransactionTypes.Invoice:
						invoiceType = typeof(ARInvoice);
						break;
					case TransactionTypes.CreditNote:
						invoiceType = typeof(ARCreditNote);
						break;
					default:
						return null;
				}

				var arInvoice = ObjectCreator.CreateInvoiceWithLine(invoiceType, TestObjectCreator.GetRandomString(5), ObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
				arInvoice.Lines[0].AL_AT = ObjectCreator.GST1.PK;
				var invoiceDocumentHeader = ObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", documentNumber ?? TestObjectCreator.GetRandomString(8), subType, "desc", arInvoice.Lines[0], ObjectCreator.Debtor);
				invoiceDocumentHeader.ADH_VATRegistrationNumberOverride = "96944490";
				var pivot1 = ObjectCreator.CreateEInvoicingTransactionPivot(invoiceDocumentHeader, Core.Constants.EInvoicingPivotState.Queued);
				Factory.Save();
				return invoiceDocumentHeader;
			}
		}

		public void CreateARComplianceDocumentForSpecialVoidingWithQueuedStatus(GlbBranch branch, string voidingReason, string transactionType = "INV", string subType = "TXE", string documentNumber = null)
		{
			var invoiceDocumentHeader = CreateARComplianceDocumentWithQueuedStatus(branch,transactionType,subType,documentNumber);
			invoiceDocumentHeader.ADH_VoidingReason = voidingReason;
			invoiceDocumentHeader.Void();
			Factory.Save();
		}

		public static AccEInvoicingBatch[] LoadInvoiceBatchesForCompany(ZGuid companyPk, ZString expectedBatchStatus)
		{
			var batchFilterQuery = new ZQuery(AccEInvoicingBatchSchema.AIB_GC, companyPk);
			batchFilterQuery.AddToFilter(AccEInvoicingBatchSchema.AIB_Status, expectedBatchStatus);
			return new BusinessObjectFactory().Load<AccEInvoicingBatch>(batchFilterQuery);
		}

		public static AccEInvoicingTransactionPivot[] LoadTransactionPivotsForCompany(ZGuid companyPk, ZString expectedPivotStatus, ZGuid batchPk)
		{
			var pivotFilterQuery = new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_GC, companyPk);
			pivotFilterQuery.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, expectedPivotStatus);
			if (batchPk.IsEmpty)
			{
				pivotFilterQuery.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_AIB, DBNull.Value);
			}
			else
			{
				pivotFilterQuery.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_AIB, batchPk);
			}
			return new BusinessObjectFactory().Load<AccEInvoicingTransactionPivot>(pivotFilterQuery);
		}

		public static AccEInvoicingTransactionPivot[] LoadTransactionPivotsForCompany(ZGuid companyPk, ZString expectedPivotStatus)
		{
			var pivotFilterQuery = new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_GC, companyPk);
			pivotFilterQuery.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, expectedPivotStatus);
			return new BusinessObjectFactory().Load<AccEInvoicingTransactionPivot>(pivotFilterQuery);
		}

		public ZGuid CreateNotificationGroup(ZString userName, ZString userEmail)
		{
			var newUser = Factory.New<GlbStaff>();
			newUser.GS_LoginName = userName;
			newUser.GS_EmailAddress = userEmail;
			var group = Factory.New<GlbGroup>();
			group.GG_Code = TestObjectCreator.GetRandomString(3);
			group.Staff.Add(newUser);
			Factory.Save();
			return group.PK;
		}

		public static GEIDeliveryModeAndContextProvider GetProvider(DeliveryContext context, bool deliverEDIMessageEvenIfThereIsError, params IEDICommunicationsMode[] modes) =>
			new GEIDeliveryModeAndContextProvider()
			{
				Serializer = new DefaultGlobalElectronicInvoiceSerializer(),
				Context = context,
				DeliverEDIMessageEvenIfThereIsError = false,
				CreateEDIMessageEvenIfThereIsError = true,
				Modes = modes
			};

		public static IEDICommunicationsMode CreateCommunicaitonMode(string transportMode, string destination, string fileFormat = "XML")
		{
			return new NonPersistentEDICommunicationMode()
			{
				EK_CommunicationsTransport = transportMode,
				EK_Destination = destination,
				EK_FileFormat = fileFormat
			};
		}

		public static string DecodePassword(string encryptedPassword)
		{
			using (var rsa = new RSACryptoServiceProvider(2048))
			{
				var keyXml = "<RSAKeyValue><Modulus>1XmSsYj8fup2QDmo2Yp0nKVzNsonnECcN2OcZXjF+eIEgQyLydnwngBcdW7UOwqQ0E+ffFcdoW44IaTVZ/A7yNC/pRJQy42BBcXteSo7SqgHt7yvPUph+oFpEWLIe0WnPJDmZCRFNoZnMUoOzK89oA/v0I3neveYHjM0bRgtVBE=</Modulus><Exponent>AQAB</Exponent><P>5sXEF2kBcvgWJ1FSo1rahov1KiqogxqBOJio3FmsLsjafwV9O1xKGmjuL1gdQbgd7rPI7x8b42PTfGctlAIPdQ==</P><Q>7M+6YC9HcPeNvr6/S+jX7tP+OV6Xljqe/U9/j7fisEG/9cfFz9lh+H+oItB1MpOD/dKB3RNW5/ODA5TSsHsarQ==</Q><DP>PQtja63jLD5j3dKtQXjvBVhQae8O1F9Wf1oikOdHnLiU07ToA6POFl5bYzqzwoappFL6fAaGogfuEaJZdCV3YQ==</DP><DQ>5iFYtXA8tQNdtCgaLuKwNV++hnHuTgfZycEf7cJ9gVvj+C2ThlFya9NiybJasjO46UlQ+k54/iAfCbPuq6J2YQ==</DQ><InverseQ>k3hB5HGpUKFMYBO+Dh7dkJJ6z7TGKevKWHw3Rhv6aYFOqVvDBKQyW52pbh4aHv5eX15QrBJNPnXYmovSYj47oQ==</InverseQ><D>A/CjvA+bcMCP5f+6cFNtc2OxWe+cELaiO3p6QpGFU+dE7oMmWazM/lmNhfmsRJpdZwmFLR9oKQMsBDZIMwwnH5/WD+gso3VmPRsg5BIdoOs/F0ZkR1qeUlwbjmaD+fPV4b8Tg/3JVeSWT7K8xtKGMLWDcdLMA3vaRo/1/g+KfD0=</D></RSAKeyValue>";
				var text = Convert.FromBase64String(encryptedPassword);
				rsa.FromXmlString(keyXml);
				return new string(System.Text.Encoding.UTF8.GetChars(rsa.Decrypt(text, true)));
			}
		}

		public static void ForceErrorWhileCreatingEDIInterchange(EDIInterchangeCreatorForEInvoicingBatchBase.TransactionBatchProcessContext batchProcessContext, INotifications notifications, ExceptionTypes? forceThisError, AccEInvoicingBatch batchForWhichErrorWillBeForced)
		{
			if (forceThisError.HasValue && (batchForWhichErrorWillBeForced == null || batchForWhichErrorWillBeForced.PK == batchProcessContext.Batch.PK))
			{
				if (forceThisError == ExceptionTypes.NotifcationErrorDuringEDIMessageCreation)
				{
					notifications.AddError("Forced Error generated for testing while creating EDI Message");
				}
				else if (forceThisError == ExceptionTypes.UnhandledExceptionDuringEDIMessageCreation)
				{
					_ = 1M / ZDecimal.Zero;
				}
			}
		}

		public static Stream GetEmbeddedResourceAsStream(string resourceName, string embeddedLocation)
		{
			var list = Assembly.GetExecutingAssembly().GetManifestResourceNames();

			var fileName = embeddedLocation + resourceName;
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName);

			return stream ?? throw new Exception($"Cannot find resource '{resourceName}'. Full name: {fileName}");
		}

		public static string GetEmbeddedResourceAsUtf8String(string resourceName, string embeddedLocation)
		{
			using (var stream = GetEmbeddedResourceAsStream(resourceName, embeddedLocation))
			{
				using (var reader = new StreamReader(stream, MessageEncoding.UTF8WithoutBOM))
				{
					return reader.ReadToEnd();
				}
			}
		}

		public const string CommonXmlFilesEmbeddedLocation = "Enterprise.Accounting.ElectronicMessaging.Testing.Common.TestFile.";

		#region Country Specific Functions

		#region Italy

		public static void AssertGEIEInvoicingEDIMessage(EDIMessage message, ZGuid expectedBranchPK, ZGuid expectedDepartmentPK, string expectedStatus = "SNT", string expectedMessageType = EInvoiceAPICommandList.Codes.GenerateInvoiceRequest)
		{
			Assertion.AssertEquals("EM_IsActive", ZBool.True, message.EM_IsActive);
			Assertion.AssertEquals("EM_IsActive", ZString.Empty, message.EM_MessageOwner);
			Assertion.AssertEquals("EM_ApplicationCode", "GEI", message.EM_ApplicationCode);
			Assertion.AssertEquals("EM_MessageType", expectedMessageType, message.EM_MessageType);
			Assertion.AssertEquals("EM_MessageType", expectedMessageType, message.EM_MessageSubType);
			Assertion.AssertEquals("EM_ReceiveTransmit", "TRX", message.EM_ReceiveTransmit);
			Assertion.AssertEquals("EM_Status", expectedStatus, message.EM_Status);
			Assertion.AssertEquals("EM_GB", expectedBranchPK, message.EM_GB);
			Assertion.AssertEquals("EM_GE", expectedDepartmentPK, message.EM_GE);
			Assertion.AssertEquals("EM_LinkTable", AccEInvoicingBatchSchema.Constants.TableName, message.EM_LinkTable);
		}

		public static void AssertGEIEInvoicingEDIInterchange(IXmlEDIInterchange interchange, string expectedStatus = "HQU", string expectedMessageType = EInvoiceAPICommandList.Codes.GenerateInvoiceRequest, string expectedTo = "GLB_ELEC_INVOICING")
		{
			Assertion.AssertEquals("EI_IsActive", ZBool.True, interchange.EI_IsActive);
			Assertion.AssertEquals("EI_ApplicationCode", "GEI", interchange.EI_ApplicationCode);
			Assertion.AssertEquals("EI_InterchangeType", expectedMessageType, interchange.EI_InterchangeType);
			Assertion.AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
			Assertion.AssertEquals("EI_Status", expectedStatus, interchange.EI_Status);
			Assertion.AssertEquals("EI_To", expectedTo, interchange.EI_To);
		}

		#endregion

		#endregion

		public TestObjectCreator ObjectCreator { get; }

		BusinessObjectFactory Factory => ObjectCreator.Factory;
	}
}
