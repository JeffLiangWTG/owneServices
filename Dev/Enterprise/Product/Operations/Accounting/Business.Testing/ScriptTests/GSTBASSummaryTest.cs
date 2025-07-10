using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class GSTBASSummaryTest : ScriptTest
	{
		public void TestQSTandQCTMixedQSTQCTWithARandAP()
		{
			SetupInvoice("001", "AR", TestObjectCreator.GSTANDQSTBASEDONQCT.PK, 100M, 0M);
			SetupInvoice("002", "AP", TestObjectCreator.GSTANDQSTBASEDONQCT.PK, 120M, 0M);
			SetupInvoice("003", "AR", TestObjectCreator.GSTANDQST1.PK, 70M, 70M);
			SetupInvoice("004", "AP", TestObjectCreator.GSTANDQST1.PK, 160M, 0M);
			AssertData(RunScript(), new Dictionary<string, decimal>()
				{
					{ "GSTANDQSTOut", 240 },
					{ "GSTANDQSTSalesOut", 12M },
					{ "QSTTaxOut", 23.94M },
					{ "GSTANDQSTIn", -280 },
					{ "GSTANDQSTSalesIn", -14M },
					{ "QSTTaxIn", -27.93M }
				}
					);
		}

		public void TestQSTandQCTRounding()
		{
			SetupInvoice("001", "AR", TestObjectCreator.GSTANDQSTBASEDONQCT.PK, 100M, 0M);
			SetupInvoice("002", "AP", TestObjectCreator.GSTANDQST1.PK, 100M, 0M);

			AssertData(RunScript(), new Dictionary<string, decimal>()
				{
					{ "GSTANDQSTOut", 100 },
					{ "GSTANDQSTSalesOut", 5M },
					{ "QSTTaxOut", 9.98M },
					{ "GSTANDQSTIn", -100 },
					{ "GSTANDQSTSalesIn", -5M },
					{ "QSTTaxIn", -9.98M }
				}
					);
		}

		public void TestGSTANDQST_WithRecoverableVAT()
		{
			SetupInvoice("003", "AR", TestObjectCreator.GSTANDQST1.PK, 100M, 0M, true);
			SetupInvoice("004", "AP", TestObjectCreator.GSTANDQST1.PK, 100M, 0M, true);

			AssertData(RunScript(), new Dictionary<string, decimal>()
				{
					{ "GSTANDQSTOut", 100m },
					{ "GSTANDQSTSalesOut", 5M },
					{ "QSTTaxOut", 9.98M },
					{ "GSTANDQSTIn", -100m },
					{ "GSTANDQSTSalesIn", -5M },
					{ "GSTANDQSTSalesInNotRecoverable", -1M },
					{ "QSTTaxIn", -9.98M },
					{ "QSTTaxInNotRecoverable", -2M },
				});
		}

		public void TestGSTANDQST_WithRecoverableVAT_WithCashVAT()
		{
			SetupInvoiceWithCashVATLine("AR", TestObjectCreator.GSTANDQST1.PK, 150M, 100M, true);
			SetupInvoiceWithCashVATLine("AP", TestObjectCreator.GSTANDQST1.PK, 150M, 100M, true);

			AssertData(RunScript(), new Dictionary<string, decimal>()
				{
					{ "GSTANDQSTOut", 86.98M },
					{ "GSTANDQSTSalesOut", 4.35M },
					{ "QSTTaxOut", 8.67M },
					{ "GSTANDQSTIn", -86.98M },
					{ "GSTANDQSTSalesIn", -4.35M },
					{ "GSTANDQSTSalesInNotRecoverable", -0.87M },
					{ "QSTTaxIn", -8.67M },
					{ "QSTTaxInNotRecoverable", -1.73M },
				});
		}

		public void TestGSTANDQST_WithRecoverableVAT_CorrectCalculationSequence()
		{
			SetupInvoice("001", "AP", TestObjectCreator.GSTANDQSTBASEDONQCT.PK, 200M, 0M, true, 0.7777M);

			AssertData(RunScript(), new Dictionary<string, decimal>()
				{
					{ "GSTANDQSTIn", -200M },
					{ "GSTANDQSTSalesIn", -10M },
					{ "GSTANDQSTSalesInNotRecoverable", -2.22M },
					{ "QSTTaxIn", -19.95M },
					{ "QSTTaxInNotRecoverable", -4.43M }  //If recoverable from Total tax calculated first then -15.51. If split GST and Extra Tax from Total first, then -15.52.
				});
		}

		public void TestGSTANDQSTBASEDONQCT_WithRecoverableVAT()
		{
			SetupInvoice("001", "AR", TestObjectCreator.GSTANDQSTBASEDONQCT.PK, 100M, 0M, true);
			SetupInvoice("002", "AP", TestObjectCreator.GSTANDQSTBASEDONQCT.PK, 100M, 0M, true);

			AssertData(RunScript(), new Dictionary<string, decimal>()
				{
					{ "GSTANDQSTOut", 100m },
					{ "GSTANDQSTSalesOut", 5M },
					{ "QSTTaxOut", 9.98M },
					{ "GSTANDQSTIn", -100m },
					{ "GSTANDQSTSalesIn", -5M },
					{ "GSTANDQSTSalesInNotRecoverable", -1M },
					{ "QSTTaxIn", -9.98M },
					{ "QSTTaxInNotRecoverable", -2M },
				});
		}

		public void TestGSTANDQSTBASEDONQCT_WithRecoverableVAT_WithCashVAT()
		{
			SetupInvoiceWithCashVATLine("AR", TestObjectCreator.GSTANDQSTBASEDONQCT.PK, 150M, 100M, true);
			SetupInvoiceWithCashVATLine("AP", TestObjectCreator.GSTANDQSTBASEDONQCT.PK, 150M, 100M, true);

			AssertData(RunScript(), new Dictionary<string, decimal>()
				{
					{ "GSTANDQSTOut", 86.98M },
					{ "GSTANDQSTSalesOut", 4.35M },
					{ "QSTTaxOut", 8.67M },
					{ "GSTANDQSTIn", -86.98M },
					{ "GSTANDQSTSalesIn", -4.35M },
					{ "GSTANDQSTSalesInNotRecoverable", -0.87M },
					{ "QSTTaxIn", -8.67M },
					{ "QSTTaxInNotRecoverable", -1.73M },
				});
		}

		public void TestSERANDEDU()
		{
			SetupInvoice("001", "AR", TestObjectCreator.SERANDEDU1.PK, 100M, 0M);
			SetupInvoice("002", "AP", TestObjectCreator.SERANDEDU1.PK, 100M, 0M);

			AssertDataForSERAndEDU();
		}

		public void TestSERANDEDU_SER()
		{
			using (TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var sERANDEDU1 = TestObjectCreator.SERANDEDU1;
				sERANDEDU1.AT_Type = AccTaxRate.Types.ServiceTax;

				SetupInvoice("001", "AR", TestObjectCreator.SERANDEDU1.PK, 100M, 0M);
				SetupInvoice("002", "AP", TestObjectCreator.SERANDEDU1.PK, 100M, 0M);

				AssertDataForSERAndEDU();
			}
		}

		void AssertDataForSERAndEDU()
		{
			AssertData(RunScript(), new Dictionary<string, decimal>()
				{
					{ "GSTANDEDUOut", 100M },
					{ "GSTANDEDUSalesOut", 10M },
					{ "EDUTaxOut", 0.3M },
					{ "GSTANDEDUIn", -100M },
					{ "GSTANDEDUSalesIn", -10M },
					{ "EDUTaxIn", -0.3M },
				});
		}

		public void TestSERANDEDU_WithRecoverableVAT()
		{
			SetupInvoice("001", "AR", TestObjectCreator.SERANDEDU1.PK, 100M, 0M, true);
			SetupInvoice("002", "AP", TestObjectCreator.SERANDEDU1.PK, 100M, 0M, true);

			AssertData(RunScript(), new Dictionary<string, decimal>()
				{
					{ "GSTANDEDUOut", 100M },
					{ "GSTANDEDUSalesOut", 10M },
					{ "EDUTaxOut", 0.3M },
					{ "GSTANDEDUIn", -100M },
					{ "GSTANDEDUSalesIn", -10M },
					{ "GSTANDEDUSalesInNotRecoverable", -2M },
					{ "EDUTaxIn", -0.3M },
					{ "EDUTaxInNotRecoverable", -0.06M },
				});
		}

		public void TestSERANDEDU_WithRecoverableVAT_WithCashVAT()
		{
			SetupInvoiceWithCashVATLine("AR", TestObjectCreator.SERANDEDU1.PK, 150M, 100M, true);
			SetupInvoiceWithCashVATLine("AP", TestObjectCreator.SERANDEDU1.PK, 150M, 100M, true);

			AssertData(RunScript(), new Dictionary<string, decimal>()
				{
					{ "GSTANDEDUOut", 90.66M },
					{ "GSTANDEDUSalesOut", 9.07M },
					{ "EDUTaxOut", 0.27M },
					{ "GSTANDEDUIn", -90.66M },
					{ "GSTANDEDUSalesIn", -9.07M },
					{ "GSTANDEDUSalesInNotRecoverable", -1.81M },
					{ "EDUTaxIn", -0.27M },
					{ "EDUTaxInNotRecoverable", -0.05M },
				});
		}

		public void TestRAX()
		{
			SetupInvoice("001", "AR", TestObjectCreator.RAX.PK, 100M, 0M);
			SetupInvoice("002", "AP", TestObjectCreator.RAX.PK, 100M, 0M);

			AssertData(RunScript(), new Dictionary<string, decimal>()
				{
					{ "RAXGSTOut", 100M },
					{ "RAXGSTSalesOut", 17M },
					{ "RAXGSTIn", -100M },
					{ "RAXGSTSalesIn", -17M }
				});
		}

		public void TestRAX_WithRecoverableVAT()
		{
			SetupInvoice("001", "AR", TestObjectCreator.RAX.PK, 100M, 0M, true);
			SetupInvoice("002", "AP", TestObjectCreator.RAX.PK, 100M, 0M, true);

			AssertData(RunScript(), new Dictionary<string, decimal>()
				{
					{ "RAXGSTOut", 100M },
					{ "RAXGSTSalesOut", 17M },
					{ "RAXGSTIn", -100M },
					{ "RAXGSTSalesIn", -17M }
				});
		}

		public void TestRAX_WithRecoverableVAT_WithCashVAT()
		{
			SetupInvoiceWithCashVATLine("AR", TestObjectCreator.RAX.PK, 150M, 100M, true);
			SetupInvoiceWithCashVATLine("AP", TestObjectCreator.RAX.PK, 150M, 100M, true);

			AssertData(RunScript(), new Dictionary<string, decimal>()
				{
					{ "RAXGSTOut", 85.47M },
					{ "RAXGSTSalesOut", 14.53M },
					{ "RAXGSTIn", -85.47M },
					{ "RAXGSTSalesIn", -14.53M }
				});
		}

		public void TestCAP()
		{
			SetupInvoice("001", "AR", TestObjectCreator.CAP.PK, 100M, 0M);
			SetupInvoice("002", "AP", TestObjectCreator.CAP.PK, 100M, 0M);

			AssertData(RunScript(), new Dictionary<string, decimal>()
				{
					{ "GSTCAPOutWithGST", 118M },
					{ "GSTCAPOut", 100M },
					{ "GSTCAPSalesOut", 18M },
					{ "GSTCAPInWithGST", -118M },
					{ "GSTCAPIn", -100M },
					{ "GSTCAPSalesIn", -18M },
				});
		}

		public void TestFREECAPGST()
		{
			SetupInvoice("001", "AR", TestObjectCreator.CAP.PK, 100M, 0M);
			SetupInvoice("001A", "AR", TestObjectCreator.FREECAPGST.PK, 100M, 0M);
			SetupInvoice("002", "AP", TestObjectCreator.CAP.PK, 100M, 0M);
			SetupInvoice("002A", "AP", TestObjectCreator.FREECAPGST.PK, 100M, 0M);

			AssertData(RunScript(), new Dictionary<string, decimal>()
				{
					{ "GSTCAPOutWithGST", 118M },
					{ "GSTCAPOut", 100M },
					{ "FREECAPGSTOut", 100M },
					{ "GSTCAPSalesOut", 18M },
					{ "GSTCAPInWithGST", -118M },
					{ "GSTCAPIn", -100M },
					{ "FREECAPGSTIn", -100M },
					{ "GSTCAPSalesIn", -18M },
				});
		}

		public void TestCAP_WithRecoverableVAT()
		{
			SetupInvoice("001", "AR", TestObjectCreator.CAP.PK, 100M, 0M, true);
			SetupInvoice("002", "AP", TestObjectCreator.CAP.PK, 100M, 0M, true);

			AssertData(RunScript(), new Dictionary<string, decimal>()
				{
					{ "GSTCAPOutWithGST", 118M },
					{ "GSTCAPOut", 100M },
					{ "GSTCAPSalesOut", 18M },
					{ "GSTCAPInWithGST", -118M },
					{ "GSTCAPInWithGSTNotRecoverable", -23.6M },
					{ "GSTCAPIn", -100M },
					{ "GSTCAPSalesIn", -18M },
					{ "GSTCAPSalesInNotRecoverable", -3.6M },
				});
		}

		public void TestCAP_WithRecoverableVAT_WithCashVAT()
		{
			SetupInvoiceWithCashVATLine("AR", TestObjectCreator.CAP.PK, 150M, 100M, true);
			SetupInvoiceWithCashVATLine("AP", TestObjectCreator.CAP.PK, 150M, 100M, true);

			AssertData(RunScript(), new Dictionary<string, decimal>()
				{
					{ "GSTCAPOutWithGST", 100M },
					{ "GSTCAPOut", 84.75M },
					{ "GSTCAPSalesOut", 15.25M },
					{ "GSTCAPInWithGST", -100M },
					{ "GSTCAPInWithGSTNotRecoverable", -20M },
					{ "GSTCAPIn", -84.75M },
					{ "GSTCAPSalesIn", -15.25M },
					{ "GSTCAPSalesInNotRecoverable", -3.05M },
				});
		}

		public void TestGST()
		{
			SetupInvoice("001", "AR", TestObjectCreator.GST1.PK, 100M, 0M);
			SetupInvoice("002", "AP", TestObjectCreator.GST1.PK, 100M, 0M);

			AssertData(RunScript(), new Dictionary<string, decimal>()
				{
					{ "GSTOutWithGST", 110M },
					{ "GSTOut", 100M },
					{ "GSTSalesOut", 10M },
					{ "GSTInWithGST", -110M },
					{ "GSTIn", -100M },
					{ "GSTSalesIn", -10M },
				});
		}

		public void TestGST_WithRecoverableVAT()
		{
			SetupInvoice("001", "AR", TestObjectCreator.GST1.PK, 100M, 0M, true);
			SetupInvoice("002", "AP", TestObjectCreator.GST1.PK, 100M, 0M, true);

			AssertData(RunScript(), new Dictionary<string, decimal>()
				{
					{ "GSTOutWithGST", 110M },
					{ "GSTOut", 100M },
					{ "GSTSalesOut", 10M },
					{ "GSTInWithGST", -110M },
					{ "GSTInWithGSTNotRecoverable", -22M },
					{ "GSTIn", -100M },
					{ "GSTSalesIn", -10M },
					{ "GSTSalesInNotRecoverable", -2M },
				});
		}

		public void TestGST_WithRecoverableVAT_WithCashVAT()
		{
			SetupInvoiceWithCashVATLine("AR", TestObjectCreator.GST1.PK, 150M, 100M, true);
			SetupInvoiceWithCashVATLine("AP", TestObjectCreator.GST1.PK, 150M, 100M, true);

			AssertData(RunScript(), new Dictionary<string, decimal>()
				{
					{ "GSTOutWithGST", 100M },
					{ "GSTOut", 90.91M },
					{ "GSTSalesOut", 9.09M },
					{ "GSTInWithGST", -100M },
					{ "GSTInWithGSTNotRecoverable", -20M },
					{ "GSTIn", -90.91M },
					{ "GSTSalesIn", -9.09M },
					{ "GSTSalesInNotRecoverable", -1.82M },
				});
		}

		public void TestGetTaxRateFromLine()
		{
			var invoice1 = SetupInvoice("INV001", "AR", TestObjectCreator.GST1.PK, 100M, 0M);
			var invoice2 = SetupInvoice("INV002", "AP", TestObjectCreator.GST1.PK, 100M, 0M);
			AssertResult();

			TestObjectCreator.GST1.SetRate_ForTestOnly(0, 10);
			TestObjectCreator.GST1.SetExtraRate_ForTestOnly(125, 10);
			Factory.Save();
			AssertResult();

			void AssertResult(decimal gSTOutWithGST = 110M, decimal gSTOut = 100M, decimal gSTSalesOut = 10M, decimal gSTInWithGST = -110M, decimal gSTIn = -100M, decimal gSTSalesIn = -10M)
			{
				AssertLineData(RunScript(), new Dictionary<string, decimal>()
				{
					{ "GSTOutWithGST", gSTOutWithGST },
					{ "GSTOut", gSTOut },
					{ "GSTSalesOut", gSTSalesOut },
					{ "GSTInWithGST", gSTInWithGST },
					{ "GSTIn", gSTIn },
					{ "GSTSalesIn", gSTSalesIn },
				});
			}
		}

		#region Implementation

		InvoicingBase SetupInvoice(string invoiceNumber, string invoiceType, ZGuid taxCodePK, ZDecimal lineAmount1, ZDecimal lineAmount2, bool withRecoverableVAT = false, decimal? inputVATRecoverable = null)
		{
			InvoicingBase invoice;

			if (invoiceType == "AR")
			{
				invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), invoiceNumber, TestObjectCreator.AUD, 1M);
				invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			}
			else
			{
				invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), invoiceNumber, TestObjectCreator.AUD, 1M);
				invoice.AH_OH = TestObjectCreator.ABIGAS.PK;
			}

			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, lineAmount1, GlbBranch.CurrentBranch.PK, false);
			line.AL_AT = taxCodePK;
			if (withRecoverableVAT)
			{
				line.AL_InputGSTVATRecoverable = inputVATRecoverable != null ? inputVATRecoverable.Value : 0.8M;
			}

			if (lineAmount2 != 0M)
			{
				line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, lineAmount2, GlbBranch.CurrentBranch.PK, false);
				line.AL_AT = taxCodePK;
			}
			Factory.Save();
			return invoice;
		}

		void SetupInvoiceWithCashVATLine(string invoiceType, ZGuid taxCodePK, ZDecimal lineAmount, ZDecimal paidAmount, bool withRecoverableVAT = false, decimal? inputVATRecoverable = null)
		{
			InvoicingBase invoice = TestObjectCreator.CreateInvoiceWithCashVATLine(invoiceType == "AR" ? typeof(ARInvoice) : typeof(APInvoice), lineAmount, 0, false);
			var line = invoice.Lines[0];
			line.AL_AT = ZGuid.Empty;
			line.AL_AT = taxCodePK;
			if (withRecoverableVAT)
			{
				line.AL_InputGSTVATRecoverable = inputVATRecoverable != null ? inputVATRecoverable.Value : 0.8M;
			}
			Factory.Save();

			if ((invoice.AH_Ledger == "AP" && invoice.AH_TransactionType != "CRD") || (invoice.AH_Ledger == "AR" && invoice.AH_TransactionType == "CRD"))
			{
				TestObjectCreator.CreateMatchLinkToPayAPInvoice(invoice, ZDateTime.Now, -paidAmount);
			}
			else
			{
				TestObjectCreator.CreateMatchLinkToPayARInvoice(invoice, ZDateTime.Now, paidAmount);
			}

			Factory.Save();
		}

		void AssertData(DataTable table, Dictionary<string, decimal> expectedValues)
		{
			AssertEquals("One row of data returned", 1, table.Rows.Count);
			var row = table.Rows[0];

			foreach (DataColumn col in table.Columns)
			{
				var testMessage = String.Format("Testing column {0} set to correct value.", col.ColumnName);
				if (expectedValues.ContainsKey(col.ColumnName))
				{
					AssertEquals(testMessage, expectedValues[col.ColumnName], row[col]);
				}
				else
				{
					AssertEquals(testMessage, 0M, row[col]);
				}
			}
		}

		void AssertLineData(DataTable table, Dictionary<string, decimal> expectedValues)
		{
			AssertEquals("One row of data returned", 1, table.Rows.Count);
			var row = table.Rows[0];

			foreach (DataColumn col in table.Columns)
			{
				var testMessage = $"Testing column {col.ColumnName} set to correct value.";
				if (expectedValues.ContainsKey(col.ColumnName))
				{
					AssertEquals(testMessage, expectedValues[col.ColumnName], row[col]);
				}
			}
		}

		DataTable RunScript()
		{
			return RunScript(ZDateTime.Empty, ZDateTime.Empty);
		}

		DataTable RunScript(ZDateTime startDate, ZDateTime endDate)
		{
			string sql = string.Format(@"Select * From GSTBASSummary (
			'{0}', --@StartDate
			'{1}', --@EndDate
			'{2}' --@CompanyCode
			)",
			 GetMinDateTimeString(startDate),
			 GetMaxDateTimeString(endDate),
			 GlbCompany.CurrentCompany.PK.ToString());

			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		#endregion

	}
}


