using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Malaysia.Testing
{
	public class MalaysiaEInvoicingAdditionalDataItemsProviderTest : TestCaseWithFactory
	{
		public void TestAdditionaDataItemAR_ContainsISOAlpha3Code()
		{
			AssertAdditionaDataItem_ContainsISOAlpha3Code
			(
				() =>
				{
					var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor);
					TestObjectCreator.CreateInvoiceLine(invoice, 110m, invoice.TransactionCurrency, 1m);

					Factory.Save();
					return invoice.PK;
				}
			);
		}

		public void TestAdditionaDataItemAP_ContainsISOAlpha3Code()
		{
			AssertAdditionaDataItem_ContainsISOAlpha3Code
			(
				() =>
				{
					var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.Debtor);
					var apInvLine = (APInvoiceLine)invoice.Lines.AddNew();
					apInvLine.AL_AG = TestObjectCreator.GLHeader1.PK;

					Factory.Save();
					return invoice.PK;
				}
			);
		}

		public void AssertAdditionaDataItem_ContainsISOAlpha3Code(Func<ZGuid> createInvoice)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			{
				var invoicePK = createInvoice();

				var (branch, batch, countryFactoryMock, accEInvoiceBatch) = SetDataForTest();
				var warnings = new Common.Logger();
				var additionalDataItemProvider = new MalaysiaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
				var pivot = accEInvoiceBatch.TransactionPivots.AddNew();
				pivot.SetCompanyAndCountryCode(accEInvoiceBatch.Company);
				pivot.AIP_ParentID = invoicePK;

				var additionalDataItem = additionalDataItemProvider.GetAdditionalHeaderDataItems(accEInvoiceBatch, branch, batch, countryFactoryMock.Object, warnings);

				AssertEquals("OrgISOAlpha3Code", additionalDataItem[0].Key);
				AssertEquals("MYS", additionalDataItem[0].Value);
				AssertEquals("BraISOAlpha3Code", additionalDataItem[1].Key);
				AssertEquals("AUS", additionalDataItem[1].Value);
			}
		}

		public void TestAdditionaDataItem_ContainsDebtorCategoryAndUNLOCO()
		{
			AssertAdditionaDataItem_ContainsDebtorCategoryAndUNLOCO
			(
				() =>
				{
					var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.Debtor);
					return invoice;
				}
			);

			AssertAdditionaDataItem_ContainsDebtorCategoryAndUNLOCO
			(
				() =>
				{
					var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor);
					return invoice;
				}
			);
		}

		public void AssertAdditionaDataItem_ContainsDebtorCategoryAndUNLOCO(Func<TransactionHeader> createInvoice)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			{
				var invoice = createInvoice();
				invoice.AH_OH = ZGuid.Empty;

				var (branch, batch, countryFactoryMock, accEInvoiceBatch) = SetDataForTest();
				var warnings = new Common.Logger();
				var additionalDataItemProvider = new MalaysiaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
				var pivot = accEInvoiceBatch.TransactionPivots.AddNew();
				pivot.SetCompanyAndCountryCode(accEInvoiceBatch.Company);
				pivot.AIP_ParentID = invoice.PK;

				var additionalDataItem = additionalDataItemProvider.GetAdditionalHeaderDataItems(accEInvoiceBatch, branch, batch, countryFactoryMock.Object, warnings);

				var category = invoice is ARInvoice ? "DebtorCategory" : invoice is APInvoice ? "CreditorCategory" : string.Empty;

				AssertEquals(category, additionalDataItem[2].Key);
				AssertEquals(OrgConstants.Category.Business, additionalDataItem[2].Value);
				AssertEquals("UNLOCOCountry", additionalDataItem[3].Key);
				AssertEquals(string.Empty, additionalDataItem[3].Value);

				invoice.AH_OH = TestObjectCreator.Debtor.PK;
				TestObjectCreator.Debtor.OH_Category = OrgConstants.Category.Government;
				TestObjectCreator.Debtor.OH_RL_NKClosestPort = "AUSYD";
				Factory.Save();
				additionalDataItem = additionalDataItemProvider.GetAdditionalHeaderDataItems(accEInvoiceBatch, branch, batch, countryFactoryMock.Object, warnings);
				AssertEquals(OrgConstants.Category.Government, additionalDataItem[2].Value);
				AssertEquals("AU", additionalDataItem[3].Value);
			}
		}

		public void TestAdditionaDataItem_ContainsEInvoicingGovernmentAllocatedNumber()
		{
			AssertAdditionaDataItem_ContainsEInvoicingGovernmentAllocatedNumber
			(
				() =>
				{
					var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.Debtor);
					return invoice;
				}
			);

			AssertAdditionaDataItem_ContainsEInvoicingGovernmentAllocatedNumber
			(
				() =>
				{
					var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor);
					return invoice;
				}
			);
		}

		public void AssertAdditionaDataItem_ContainsEInvoicingGovernmentAllocatedNumber(Func<TransactionHeader> createInvoice)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			{
				var invoice = createInvoice();
				invoice.AH_GovernmentAllocatedID = "F9D425P6DS7D8IU";

				var amendInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR002", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor);
				amendInvoice.AH_TransactionBelongsToGroup = invoice.PK;

				var (branch, batch, countryFactoryMock, accEInvoiceBatch) = SetDataForTest();
				var warnings = new Common.Logger();
				var pivot = accEInvoiceBatch.TransactionPivots.AddNew();
				pivot.SetCompanyAndCountryCode(accEInvoiceBatch.Company);
				pivot.AIP_ParentID = amendInvoice.PK;

				var additionalDataItemProvider = new MalaysiaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
				var additionalDataItem = additionalDataItemProvider.GetAdditionalHeaderDataItems(accEInvoiceBatch, branch, batch, countryFactoryMock.Object, warnings);

				AssertEquals("OriginalGovt", additionalDataItem[4].Key);
				AssertEquals(invoice.AH_GovernmentAllocatedID, additionalDataItem[4].Value);
			}
		}

		public void TestAdditionaDataItem_ContainsTransactionUUID()
		{
			AssertAdditionaDataItem_ContainsTransactionUUID
			(
				() =>
				{
					var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.Debtor);
					return invoice;
				}
			);

			AssertAdditionaDataItem_ContainsTransactionUUID
			(
				() =>
				{
					var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor);
					return invoice;
				}
			);
		}

		public void AssertAdditionaDataItem_ContainsTransactionUUID(Func<TransactionHeader> createInvoice)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			{
				var invoice = createInvoice();
				invoice.AH_GovernmentAllocatedID = "F9D425P6DS7D8IU";

				var (branch, batch, countryFactoryMock, accEInvoiceBatch) = SetDataForTest();
				var warnings = new Common.Logger();

				var pivot = accEInvoiceBatch.TransactionPivots.AddNew();
				pivot.SetCompanyAndCountryCode(accEInvoiceBatch.Company);
				pivot.AIP_ParentID = invoice.PK;

				var additionalDataItemProvider = new MalaysiaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
				var additionalDataItem = additionalDataItemProvider.GetAdditionalHeaderDataItems(accEInvoiceBatch, branch, batch, countryFactoryMock.Object, warnings);

				AssertEquals("UUID", additionalDataItem[4].Key);
				AssertEquals(invoice.AH_GovernmentAllocatedID, additionalDataItem[4].Value);
			}
		}

		public void TestAdditionaDataItem_ContainsStaffPhone()
		{
			AssertAdditionaDataItem_ContainsStaffPhone
			(
				() =>
				{
					var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.Debtor);
					return invoice;
				}
			);

			AssertAdditionaDataItem_ContainsStaffPhone
			(
				() =>
				{
					var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor);
					return invoice;
				}
			);
		}

		public void AssertAdditionaDataItem_ContainsStaffPhone(Func<TransactionHeader> createInvoice)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			{
				var invoice = createInvoice();
				invoice.AH_SystemCreateUser = "E";

				var query = new ZQuery(GlbStaffSchema.GS_Code, invoice.AH_SystemCreateUser);
				var staff = Factory.LoadTop1<GlbStaff>(query);
				staff.GS_WorkPhone = "123";

				var (branch, batch, countryFactoryMock, accEInvoiceBatch) = SetDataForTest();
				var warnings = new Common.Logger();
				var pivot = accEInvoiceBatch.TransactionPivots.AddNew();
				pivot.SetCompanyAndCountryCode(accEInvoiceBatch.Company);
				pivot.AIP_ParentID = invoice.PK;

				var additionalDataItemProvider = new MalaysiaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
				var additionalDataItem = additionalDataItemProvider.GetAdditionalHeaderDataItems(accEInvoiceBatch, branch, batch, countryFactoryMock.Object, warnings);

				var contactName = string.Empty;

				if (invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					contactName = "SupplierPhone";
				}
				else if (invoice.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					contactName = "BuyerPhone";
				}

				AssertEquals(contactName, additionalDataItem[5].Key);
				AssertEquals(staff.GS_WorkPhone, additionalDataItem[5].Value);
			}
		}

		public void TestAdditionaDataItemAP_ContainsContactPhone()
		{
			AssertAdditionaDataItem_ContainsContactPhone
			(
				() =>
				{
					var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.Debtor);
					return invoice;
				}
			);
		}

		public void TestAdditionaDataItemAR_ContainsContactPhone()
		{
			AssertAdditionaDataItem_ContainsContactPhone
			(
				() =>
				{
					var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor);
					return invoice;
				}
			);
		}

		public void AssertAdditionaDataItem_ContainsContactPhone(Func<TransactionHeader> createInvoice)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			{
				var invoice = createInvoice();
				invoice.AH_OC_InvoiceContactOverride = ZGuid.Empty;

				var contact1 = TestObjectCreator.CreateContact(invoice.Header, "first contact");
				contact1.OC_Phone = "123";
				var contact2 = TestObjectCreator.CreateContact(invoice.Header, "second contact");
				contact2.OC_Phone = "456";

				var documentGroup = invoice is ARInvoice ? ContactType.Receivables.Code : invoice is APInvoice ? ContactType.Payables.Code : string.Empty;

				var orgDocument1 = contact1.Documents.AddNew();
				orgDocument1.OD_DocumentGroup = documentGroup;

				var (branch, batch, countryFactoryMock, accEInvoiceBatch) = SetDataForTest();
				var warnings = new Common.Logger();
				var pivot = accEInvoiceBatch.TransactionPivots.AddNew();
				pivot.SetCompanyAndCountryCode(accEInvoiceBatch.Company);
				pivot.AIP_ParentID = invoice.PK;

				var additionalDataItemProvider = new MalaysiaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
				var additionalDataItem = additionalDataItemProvider.GetAdditionalHeaderDataItems(accEInvoiceBatch, branch, batch, countryFactoryMock.Object, warnings);

				var contactName = string.Empty;
				if (invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					contactName = "BuyerPhone";
				}
				else if (invoice.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					contactName = "SupplierPhone";
				}

				AssertEquals(contactName, additionalDataItem[6].Key);
				AssertEquals(contact1.OC_Phone, additionalDataItem[6].Value);

				var orgDocument2 = contact2.Documents.AddNew();
				orgDocument2.OD_DocumentGroup = documentGroup;

				additionalDataItem = additionalDataItemProvider.GetAdditionalHeaderDataItems(accEInvoiceBatch, branch, batch, countryFactoryMock.Object, warnings);
				AssertEquals(contactName, additionalDataItem[6].Key);
				AssertEquals("Phone Should be Offical Document Group", contact1.OC_Phone, additionalDataItem[6].Value);

				contact1.OC_IsActive = false;

				additionalDataItem = additionalDataItemProvider.GetAdditionalHeaderDataItems(accEInvoiceBatch, branch, batch, countryFactoryMock.Object, warnings);
				AssertEquals(contactName, additionalDataItem[6].Key);
				AssertEquals("Phone Should be Active Offical Document Group", ZString.Empty, additionalDataItem[6].Value);

				invoice.AH_OC_InvoiceContactOverride = contact2.PK;

				additionalDataItem = additionalDataItemProvider.GetAdditionalHeaderDataItems(accEInvoiceBatch, branch, batch, countryFactoryMock.Object, warnings);
				AssertEquals(contactName, additionalDataItem[6].Key);
				AssertEquals(contact2.OC_Phone, additionalDataItem[6].Value);
			}
		}

		public void TestAdditionaDataItem_CotainsEINVDocumentTypeAndDesc()
		{
			AssertAdditionaDataItem_CotainsEINVDocumentTypeAndDesc
			(
				() =>
				{
					var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.Debtor);
					return invoice;
				}
			);

			AssertAdditionaDataItem_CotainsEINVDocumentTypeAndDesc
			(
				() =>
				{
					var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor);
					return invoice;
				}
			);
		}

		void AssertAdditionaDataItem_CotainsEINVDocumentTypeAndDesc(Func<TransactionHeader> createInvoice)
		{
			RegistryItemDictionary.Instance.PurgeAll();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			{
				var invoice = createInvoice();

				var docType = MasterFactory.NewWithValidTestData<RefDocType>();
				docType.RT_DocType = "XXXX";
				docType.RT_Desc = "Test";
				docType.RT_ReferenceType = "ALL";
				docType.RT_IsPublished = true;
				MasterFactory.Save();

				var codeDescriptionpairList = ((CodePairRegistryDataType)AccountingElectronicMessagingRegistry.Instance.ElectronicInvoiceDocumentType.DataType).LookUpList;

				foreach (CodeDescriptionPair desc in codeDescriptionpairList)
				{
					using (AccountingElectronicMessagingRegistry.Instance.ElectronicInvoiceDocumentType.SetTemporaryValue(invoice.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, desc.Code))
					{
						var (branch, batch, countryFactoryMock, accEInvoiceBatch) = SetDataForTest();
						var warnings = new Common.Logger();
						var additionalDataItemProvider = new MalaysiaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
						var pivot = accEInvoiceBatch.TransactionPivots.AddNew();
						pivot.SetCompanyAndCountryCode(accEInvoiceBatch.Company);
						pivot.AIP_ParentID = invoice.PK;

						var additionalDataItem = additionalDataItemProvider.GetAdditionalHeaderDataItems(accEInvoiceBatch, branch, batch, countryFactoryMock.Object, warnings);

						AssertEquals("EINVDocumentType", additionalDataItem[7].Key);
						AssertEquals(desc.Code, additionalDataItem[7].Value);
						AssertEquals("EINVDocumentDesc", additionalDataItem[8].Key);
						AssertEquals(desc.Description, additionalDataItem[8].Value);
					}
				}

				using (AccountingElectronicMessagingRegistry.Instance.ElectronicInvoiceDocumentType.SetTemporaryValue(invoice.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, docType.RT_DocType.ToString()))
				{
					var docTypeCode = docType.RT_DocType;
					docType.Delete();
					MasterFactory.Save();

					var (branch, batch, countryFactoryMock, accEInvoiceBatch) = SetDataForTest();
					var warnings = new Common.Logger();
					var additionalDataItemProvider = new MalaysiaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
					var pivot = accEInvoiceBatch.TransactionPivots.AddNew();
					pivot.SetCompanyAndCountryCode(accEInvoiceBatch.Company);
					pivot.AIP_ParentID = invoice.PK;

					RegistryItemDictionary.Instance.PurgeAll();
					var additionalDataItem = additionalDataItemProvider.GetAdditionalHeaderDataItems(accEInvoiceBatch, branch, batch, countryFactoryMock.Object, warnings);

					AssertEquals("EINVDocumentType", additionalDataItem[7].Key);
					AssertEquals(docTypeCode, additionalDataItem[7].Value);
					AssertEquals("EINVDocumentDesc", additionalDataItem[8].Key);
					AssertEquals(codeDescriptionpairList?.GetDescriptionFromCode(Constants.RefDocTypes.Invoice), additionalDataItem[8].Value);
				}
			}
		}

		public void TestAdditionaDataItem_ContainsMSICDesc()
		{
			AssertAdditionaDataItem_ContainsMSICDesc(MYMSICCodeDescriptionPairList.Descriptions.ForwardingOfFreight, () =>
			{
				var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.Debtor);
				invoice.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.StandardIndustrialClassification, MYMSICCodeDescriptionPairList.Codes.ForwardingOfFreight, CountryCodes.Malaysia);
				return invoice;
			});

			AssertAdditionaDataItem_ContainsMSICDesc(MYMSICCodeDescriptionPairList.Descriptions.AirportAndAirTrafficControl, () =>
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor);
				invoice.Branch.GB_OH_OrgProxy = TestObjectCreator.Creditor1.PK;
				invoice.Branch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.StandardIndustrialClassification, MYMSICCodeDescriptionPairList.Codes.AirportAndAirTrafficControl, CountryCodes.Malaysia);
				return invoice;
			});

			AssertAdditionaDataItem_ContainsMSICDesc(MYMSICCodeDescriptionPairList.Descriptions.CityBusServices, () =>
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor);
				invoice.Branch.GB_OH_OrgProxy = Guid.Empty;
				invoice.Company.GC_OH_OrgProxy = TestObjectCreator.Creditor2.PK;
				invoice.Company.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.StandardIndustrialClassification, MYMSICCodeDescriptionPairList.Codes.CityBusServices, CountryCodes.Malaysia);
				return invoice;
			});

			AssertAdditionaDataItem_ContainsMSICDescInFeatureControl("12345", "Description1");
			AssertAdditionaDataItem_ContainsMSICDescInFeatureControl(MYMSICCodeDescriptionPairList.Codes.ForwardingOfFreight, "Description2");
		}

		void AssertAdditionaDataItem_ContainsMSICDesc(string expect, Func<InvoicingBase> action)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			{
				var invoice = action();

				var (branch, batch, countryFactoryMock, accEInvoiceBatch) = SetDataForTest();
				var pivot = accEInvoiceBatch.TransactionPivots.AddNew();
				pivot.SetCompanyAndCountryCode(accEInvoiceBatch.Company);
				pivot.AIP_ParentID = invoice.PK;
				var warnings = new Common.Logger();

				var additionalDataItemProvider = new MalaysiaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
				var additionalDataItem = additionalDataItemProvider.GetAdditionalHeaderDataItems(accEInvoiceBatch, branch, batch, countryFactoryMock.Object, warnings);

				AssertEquals("SupplierMSICDesc", additionalDataItem[9].Key);
				AssertEquals(expect, additionalDataItem[9].Value);
			}
		}

		void AssertAdditionaDataItem_ContainsMSICDescInFeatureControl(string number, string desc)
		{
			var mockIFeatureData = new Mock<IFeatureData>();

			var reportingBookFeatureControlData = new AccRegistrationNumberFeatureControlData
			{
				RegistrationNumbers =
				[
					new()
					{
						Country = "MY",
						Type = "SIC",
						Numbers = new List<NumberTuple> { new NumberTuple { Number = number, Description = desc } }
					}
				]
			};

			mockIFeatureData.Setup(x => x.TryDeserializeParameterAsJson(out reportingBookFeatureControlData)).Returns(true);
			var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.AccountingRegistrationNumberFeature, CancellationToken.None)).Returns(Task.FromResult(mockIFeatureData.Object));

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			{
				var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.Debtor);
				invoice.Header.CustomsCodes.DeleteAll();
				invoice.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.StandardIndustrialClassification, number, CountryCodes.Malaysia);

				var (branch, batch, countryFactoryMock, accEInvoiceBatch) = SetDataForTest();
				var pivot = accEInvoiceBatch.TransactionPivots.AddNew();
				pivot.SetCompanyAndCountryCode(accEInvoiceBatch.Company);
				pivot.AIP_ParentID = invoice.PK;
				var warnings = new Common.Logger();

				var additionalDataItemProvider = new MalaysiaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
				var additionalDataItem = additionalDataItemProvider.GetAdditionalHeaderDataItems(accEInvoiceBatch, branch, batch, countryFactoryMock.Object, warnings);

				AssertEquals("SupplierMSICDesc", additionalDataItem[9].Key);
				AssertEquals(desc, additionalDataItem[9].Value);
			}
		}

		[TestDate(2024, 11, 25, 12, 0, 0)]
		public void TestAdditionaDataItem_ContainsGEIDateTime()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			{
				var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.Debtor);
				invoice.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.StandardIndustrialClassification, MYMSICCodeDescriptionPairList.Codes.ForwardingOfFreight, CountryCodes.Malaysia);
				MasterFactory.Save();

				var (branch, batch, countryFactoryMock, accEInvoiceBatch) = SetDataForTest();
				var pivot = accEInvoiceBatch.TransactionPivots.AddNew();
				pivot.SetCompanyAndCountryCode(accEInvoiceBatch.Company);
				pivot.AIP_ParentID = invoice.PK;
				var warnings = new Common.Logger();

				var additionalDataItemProvider = new MalaysiaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
				var additionalDataItem = additionalDataItemProvider.GetAdditionalHeaderDataItems(accEInvoiceBatch, branch, batch, countryFactoryMock.Object, warnings);

				AssertEquals("GEIDateTime", additionalDataItem[10].Key);
				AssertContains("2024-11-25T12:00:00", additionalDataItem[10].Value);
			}
		}

		DocumentFactory MasterFactory
		{
			get
			{
				if (fMasterFactory == null)
				{
					fMasterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				}
				return fMasterFactory;
			}
		}
		DocumentFactory fMasterFactory;

		(GlbBranch branch, UniversalTransactionBatch batch, Mock<ICountryEInvoicingObjectFactory> countryFactoryMock, AccEInvoicingBatch accEInvoiceBatch) SetDataForTest()
		{
			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();

			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;

			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			batch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Branch = new Branch() { Code = branch.GB_Code }
			});

			return (branch, batch, countryFactoryMock, accBatch);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator = testObjectCreator ?? new TestObjectCreator(Factory);
		TestObjectCreator testObjectCreator;
	}
}
