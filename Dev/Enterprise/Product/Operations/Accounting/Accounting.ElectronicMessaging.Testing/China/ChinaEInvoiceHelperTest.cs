using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.ElectronicMessaging.China.Testing
{
	class ChinaEInvoiceHelperTest : TestCaseWithFactory
	{
		public void TestUpdateDiscountedLineRowType()
		{
			var normalLine = new OrderDetail();
			normalLine.RowType = "0";

			var discountedLine = new OrderDetail();
			discountedLine.RowType = "0";

			var discountLine = new OrderDetail();
			discountLine.RowType = "1";

			var items = new List<OrderDetail>(3);
			items.Add(normalLine);
			items.Add(discountedLine);
			items.Add(discountLine);

			ChinaEInvoiceHelper.UpdateDiscountedLineRowType(items);

			AssertEquals("rowType of normal line should be \"0\"", "0", items[0].RowType);
			AssertEquals("rowType of discounted line should be \"2\"", "2", items[1].RowType);
			AssertEquals("rowType of discount line should be \"1\"", "1", items[2].RowType);
		}

		public void TestChangeEInvoiceFileNameWhenHasSameNameAsExisted()
		{
			using (var memoryStream = (SubStreamableStream)new MemoryStream(Encoding.ASCII.GetBytes("test")))
			{
				var attachedDocuments = new List<AttachedDocument>();

				var documentType = new DocumentType()
				{
					Code = "INV",
					Description = "China eInvoice file"
				};
				var attachedDocument = new AttachedDocument()
				{
					Type = documentType,
					FileName = "a.pdf",
					IsPublished = true,
					ImageData = memoryStream
				};
				attachedDocuments.Add(attachedDocument);
				attachedDocument = new AttachedDocument()
				{
					Type = documentType,
					FileName = "b.ofd",
					IsPublished = true,
					ImageData = memoryStream
				};
				attachedDocuments.Add(attachedDocument);
				attachedDocument = new AttachedDocument()
				{
					Type = documentType,
					FileName = "c.xml",
					IsPublished = true,
					ImageData = memoryStream
				};
				attachedDocuments.Add(attachedDocument);

				var invoice = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", ObjectCreator.CNY,
					1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
				invoice.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "a", "INV");
				invoice.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "b", "INV");
				invoice.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "c", "INV");
				invoice.DocManagerInfo.MasterFactory.Save();
				invoice.Logs.AddNew(Events.DocumentImported);
				invoice.Logs.AddNew(Events.DocumentImported);
				invoice.Logs.AddNew(Events.DocumentImported);
				invoice.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "a", "INV");
				invoice.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "b", "INV");
				invoice.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "c", "INV");

				var pdfFileName = "VAT_" + invoice.AH_TransactionNum + "_a[2]";
				var ofdFileName = "VAT_" + invoice.AH_TransactionNum + "_b[2]";
				var xmlFileName = "VAT_" + invoice.AH_TransactionNum + "_c[2]";

				ChinaEInvoiceHelper.UpdateDDIReferenceAndFileName(invoice, attachedDocuments);

				AssertEquals(6, invoice.DocManagerInfo.AllEDocs.Count);

				var eDocs = invoice.DocManagerInfo.AllEDocs.OfType<StorageDocsBase>();
				Assert(eDocs.Any(x => x.SC_FileName == pdfFileName));
				Assert(eDocs.Any(x => x.SC_FileName == ofdFileName));
				Assert(eDocs.Any(x => x.SC_FileName == xmlFileName));
			}
		}
		public void TestUpdateDDIReferenceAndFileName_RemoveDuplicatedChinaEInvoiceFileEDocWithSameFileNameAndDesc()
		{
			using (var memoryStream = (SubStreamableStream)new MemoryStream(Encoding.ASCII.GetBytes("test")))
			{
				var attachedDocuments = new List<AttachedDocument>();

				var documentType = new DocumentType()
				{
					Code = "INV",
					Description = "China eInvoice file"
				};
				var attachedDocument = new AttachedDocument()
				{
					Type = documentType,
					FileName = "a.pdf",
					IsPublished = true,
					ImageData = memoryStream
				};
				attachedDocuments.Add(attachedDocument);

				var invoice = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", ObjectCreator.CNY,
					1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
				Factory.Save();
				AssertEquals(1, invoice.Logs.GetAllLogs().Count);

				AddAndUpdateLogAndFile(invoice, attachedDocuments, "a.pdf", "INV", "China eInvoice file");
				invoice.DocManagerInfo.Save();
				AssertEquals(1, invoice.DocManagerInfo.AllEDocs.Count);
				AssertEquals(2, invoice.Logs.GetAllLogs().Count);

				AddAndUpdateLogAndFile(invoice, attachedDocuments, "a.pdf", "INV", "China eInvoice file");
				AssertEquals(1, invoice.DocManagerInfo.AllEDocs.Count);
				AssertEquals(2, invoice.Logs.GetAllLogs().Count);

				attachedDocuments.Clear();
				attachedDocument = new AttachedDocument()
				{
					Type = documentType,
					FileName = "b.pdf",
					IsPublished = true,
					ImageData = memoryStream
				};
				attachedDocuments.Add(attachedDocument);

				AddAndUpdateLogAndFile(invoice, attachedDocuments, "b.pdf", "INV", "China eInvoice file");
				AssertEquals(2, invoice.DocManagerInfo.AllEDocs.Count);
				AssertEquals(3, invoice.Logs.GetAllLogs().Count);
				invoice.DocManagerInfo.Save();

				attachedDocuments.Clear();
				attachedDocument = new AttachedDocument()
				{
					Type = documentType,
					FileName = "a.ofd",
					IsPublished = true,
					ImageData = memoryStream
				};
				attachedDocuments.Add(attachedDocument);

				AddAndUpdateLogAndFile(invoice, attachedDocuments, "a.ofd", "INV", "China eInvoice file");
				AssertEquals(3, invoice.DocManagerInfo.AllEDocs.Count);
				AssertEquals(4, invoice.Logs.GetAllLogs().Count);
			}
		}

		void AddAndUpdateLogAndFile(InvoicingBase invoice, List<AttachedDocument> attachedDocuments, string fileName, string documentType, string description)
		{
			invoice.Logs.AddNew(Events.DocumentImported);
			invoice.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, fileName, documentType, description: description);
			ChinaEInvoiceHelper.UpdateDDIReferenceAndFileName(invoice, attachedDocuments);
		}

		#region GetReceivingFileType

		public void TestGetReceivingFileType_PDF()
		{
			var extend = ChinaEInvoiceHelper.GetExtend(ARInvoice);
			AssertEquals("PDF", extend);
		}

		public void TestGetReceivingFileType_OFD()
		{
			var configs = new EInvoicingReceivingFileTypeConfigurationCollection
			{
				new EInvoicingReceivingFileTypeConfiguration
				{
					FileFormat = "OFD",
					DebtorType = Core.Constants.DebtorTypes.Code.DebtorOrganisation,
					DebtorCode = ObjectCreator.Debtor.PK
				},
				new EInvoicingReceivingFileTypeConfiguration
				{
					FileFormat = "XML",
					DebtorType = Core.Constants.DebtorTypes.Code.DebtorOrganisation,
					DebtorCode = ObjectCreator.Debtor1.PK
				}
			};

			AssertGetReceivingFileType(configs, "OFD,PDF");
		}

		public void TestGetReceivingFileType_XML()
		{
			var configs = new EInvoicingReceivingFileTypeConfigurationCollection
			{
				new EInvoicingReceivingFileTypeConfiguration
				{
					FileFormat = "OFD",
					DebtorType = Core.Constants.DebtorTypes.Code.DebtorOrganisation,
					DebtorCode = ObjectCreator.Debtor1.PK
				},
				new EInvoicingReceivingFileTypeConfiguration
				{
					FileFormat = "XML",
					DebtorType = Core.Constants.DebtorTypes.Code.DebtorOrganisation,
					DebtorCode = ObjectCreator.Debtor.PK
				}
			};

			AssertGetReceivingFileType(configs, "PDF,XML");
		}

		public void TestGetReceivingFileType_BothALL()
		{
			var configs = new EInvoicingReceivingFileTypeConfigurationCollection
			{
				new EInvoicingReceivingFileTypeConfiguration { FileFormat = "OFD", DebtorType = "ALL" },
				new EInvoicingReceivingFileTypeConfiguration { FileFormat = "XML", DebtorType = "ALL" }
			};

			AssertGetReceivingFileType(configs, "OFD,PDF,XML");
		}

		public void TestGetReceivingFileType_BothDebtor()
		{
			var configs = new EInvoicingReceivingFileTypeConfigurationCollection
			{
				new EInvoicingReceivingFileTypeConfiguration
				{
					FileFormat = "OFD",
					DebtorType = Core.Constants.DebtorTypes.Code.DebtorOrganisation,
					DebtorCode = ObjectCreator.Debtor.PK
				},
				new EInvoicingReceivingFileTypeConfiguration
				{
					FileFormat = "XML",
					DebtorType = Core.Constants.DebtorTypes.Code.DebtorOrganisation,
					DebtorCode = ObjectCreator.Debtor.PK
				}
			};

			AssertGetReceivingFileType(configs, "OFD,PDF,XML");
		}

		public void TestGetReceivingFileType_BothDebtorGroup()
		{
			var configs = new EInvoicingReceivingFileTypeConfigurationCollection
			{
				new EInvoicingReceivingFileTypeConfiguration
				{
					FileFormat = "OFD",
					DebtorType = Core.Constants.DebtorTypes.Code.DebtorOrganisation,
					DebtorCode = ObjectCreator.Debtor.PK
				},
				new EInvoicingReceivingFileTypeConfiguration
				{
					FileFormat = "XML",
					DebtorType = Core.Constants.DebtorTypes.Code.DebtorOrganisation,
					DebtorCode = ObjectCreator.Debtor.PK
				}
			};

			AssertGetReceivingFileType(configs, "OFD,PDF,XML");
		}

		public void TestGetReceivingFileType_AllAndDebtor()
		{
			var configs = new EInvoicingReceivingFileTypeConfigurationCollection
			{
				new EInvoicingReceivingFileTypeConfiguration
				{
					FileFormat = "OFD",
					DebtorType = Core.Constants.DebtorTypes.Code.DebtorGroup,
					DebtorCode = ObjectCreator.Debtor.CompanyData.OB_OJ_ARDebtorGroup
				},
				new EInvoicingReceivingFileTypeConfiguration
				{
					FileFormat = "XML",
					DebtorType = Core.Constants.DebtorTypes.Code.DebtorGroup,
					DebtorCode = ObjectCreator.Debtor.CompanyData.OB_OJ_ARDebtorGroup
				}
			};

			AssertGetReceivingFileType(configs, "OFD,PDF,XML");
		}

		public void TestGetReceivingFileType_AllAndDebtorGroup()
		{
			var configs = new EInvoicingReceivingFileTypeConfigurationCollection
			{
				new EInvoicingReceivingFileTypeConfiguration
				{
					FileFormat = "OFD",
					DebtorType = "ALL"
				},
				new EInvoicingReceivingFileTypeConfiguration
				{
					FileFormat = "XML",
					DebtorType = Core.Constants.DebtorTypes.Code.DebtorGroup,
					DebtorCode = ObjectCreator.Debtor.CompanyData.OB_OJ_ARDebtorGroup
				}
			};

			AssertGetReceivingFileType(configs, "OFD,PDF,XML");
		}

		public void TestGetReceivingFileType_DebtorAndDebtorGroup()
		{
			var configs = new EInvoicingReceivingFileTypeConfigurationCollection
			{
				new EInvoicingReceivingFileTypeConfiguration
				{
					FileFormat = "OFD",
					DebtorType = Core.Constants.DebtorTypes.Code.DebtorOrganisation,
					DebtorCode = ObjectCreator.Debtor.PK
				},
				new EInvoicingReceivingFileTypeConfiguration
				{
					FileFormat = "XML",
					DebtorType = Core.Constants.DebtorTypes.Code.DebtorGroup,
					DebtorCode = ObjectCreator.Debtor.CompanyData.OB_OJ_ARDebtorGroup
				}
			};

			AssertGetReceivingFileType(configs, "OFD,PDF,XML");
		}

		void AssertGetReceivingFileType(EInvoicingReceivingFileTypeConfigurationCollection configs, string expectExtend)
		{
			AccountingConfigurationRegistry.Instance.ChinaEInvoicingReceivingFileType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configs);

			var extend = ChinaEInvoiceHelper.GetExtend(ARInvoice);
			AssertEquals(expectExtend, extend);
		}

		#endregion

		public void TestGetDefaultTaxBankAccount()
		{
			var account = ObjectCreator.Debtor.CompanyData.ARAccountDetailsCollection.AddNew();
			account.A1_IsDefaultAccount = true;
			account.A1_PaymentMethod = AccARAccountDetails.ARBankAccPayment;
			account.A1_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			account.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.China;
			account.A1_BankName = "Bank name1";
			account.A1_BankAccount = "Bank account1";

			var accountWithCurrencyUSD = ObjectCreator.Debtor.CompanyData.ARAccountDetailsCollection.AddNew();
			accountWithCurrencyUSD.A1_IsDefaultAccount = true;
			accountWithCurrencyUSD.A1_PaymentMethod = AccARAccountDetails.ARBankAccPayment;
			accountWithCurrencyUSD.A1_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			accountWithCurrencyUSD.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountWithCurrencyUSD.A1_BankName = "Bank name2";
			accountWithCurrencyUSD.A1_BankAccount = "Bank account2";

			AssertEquals(accountWithCurrencyUSD.PK, ChinaEInvoiceHelper.GetDefaultTaxBankAccount(ObjectCreator.Debtor, Core.Constants.CurrencyCodes.UnitedStates).PK);

			ObjectCreator.Debtor.CompanyData.ARAccountDetailsCollection.RemoveAndDelete(accountWithCurrencyUSD);

			AssertEquals(account.PK, ChinaEInvoiceHelper.GetDefaultTaxBankAccount(ObjectCreator.Debtor, Core.Constants.CurrencyCodes.UnitedStates).PK);
		}

		protected readonly string[] eReportingGEIMessageSystemTypeCodes =
		{
			AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.AlwaysProductionSystem,
			AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.AlwaysTestSystem, AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.Default,
		};

		protected override void SetUp()
		{
			base.SetUp();

			ObjectCreator = new TestObjectCreator(Factory);

			var debtorGroup = ObjectCreator.CreateDebtorGroup();
			ObjectCreator.Debtor.CompanyData.OB_OJ_ARDebtorGroup = debtorGroup.PK;
			ARInvoice = ObjectCreator.CreateARInvoice<ARInvoice>("AR001", ObjectCreator.CNY, 1m, ObjectCreator.Debtor);
			Factory.Save();
		}

		ARInvoice ARInvoice;
		TestObjectCreator ObjectCreator;
	}
}
