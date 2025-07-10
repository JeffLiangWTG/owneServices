using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Matching.Testing
{
	[TestedType(typeof(PreMatchedDataExporter))]
	sealed class PreMatchedDataExporterTest : NonPersistentBusinessObjectTestCase
	{
		#region Export to Directory
		public void TestExport_ToDirectory()
		{
			Exporter.DeliveryMethod = PreMatchedDataExporter.DirectoryDeliveryMethodCode;
			string testDir = Path.Combine(Env.TempPath, "PreMatchedDatExporterTestDir_");
			Directory.CreateDirectory(testDir);
			Exporter.ExportDirectory = testDir;
			Exporter.NettingCycle = Exporter.NettingCycleList[0].Code;
			try
			{
				ZString nettingCode = CurrentOrg.CustomsCodes.GetUNC();
				ZString aPNettingCode = nettingCode.EndsWith("COR") ? "COR" + nettingCode.Replace("COR", "") : nettingCode.Right(3) + ZArchitecture.Core.LedgerTypes.AccountsPayable;
				string expectedARFilename = string.Format("{0}{1:yMM}.txt", nettingCode, Exporter.NettingCycleDate);
				string expectedAPFilename = string.Format("{0}{1:yMM}.txt", aPNettingCode, Exporter.NettingCycleDate);
				string aRFileFullPath = Path.Combine(Exporter.ExportDirectory, expectedARFilename);
				string aPFileFullPath = Path.Combine(Exporter.ExportDirectory, expectedAPFilename);
				AssertEquals("Is ready", true, Exporter.HasMinimumRequirements);
				Exporter.Export();
				AssertEquals(true, File.Exists(aRFileFullPath));
				AssertEquals(true, File.Exists(aPFileFullPath));
				string generated = GetStringInFile(aRFileFullPath);
				AssertEquals("", generated);
				generated = GetStringInFile(aRFileFullPath);
				AssertEquals("", generated);
				InsertDummyTransactions_TestExport();
				AssertEquals("Is ready", true, Exporter.HasMinimumRequirements);
				Exporter.Export();
				AssertEquals(true, File.Exists(aRFileFullPath));
				AssertEquals(true, File.Exists(aPFileFullPath));
				generated = GetStringInFile(aRFileFullPath);
				string expected = GenerateARString_TestExport();
				AssertEquals(expected, generated);
				generated = GetStringInFile(aPFileFullPath);
				expected = GenerateAPString_TestExport();
				AssertEquals(expected, generated);
			}
			finally
			{
				TempDirectory.DeleteDirectory(testDir);
			}
		}

		void InsertDummyTransactions_TestExport()
		{
			ZDateTime invoiceDate1 = new ZDateTime(2004, 11, 15);
			ZDateTime invoiceDate2 = new ZDateTime(2004, 10, 15);
			ZDateTime dueDate = new ZDateTime(2004, 12, 30);
			ZString desc = "Transaction Description";
			OrgHeader counterpart = Factory.NewWithValidTestData<OrgHeader>();
			counterpart.OH_Code = "TSJAS";
			SetNettingCode(counterpart, "TSJAS");
			JASForwardingConsol consol = Generator.GenerateConsol("MB001", Constants.TransportModes.Sea);
			CommonShipment shipment1 = Generator.GenerateShipment("HB001", Constants.TransportModes.Sea, "AUSYD", "IDJKT");
			shipment1.JS_E_DEP = invoiceDate1;
			shipment1.Consols.Add(consol);
			JobHeader job = Generator.GenerateJob(shipment1.PK, "J001");
			Generator.GenerateAccTransactionHeader(job.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, dueDate, invoiceDate1, "T1", "Desc1", AUDCode, 41, counterpart.PK);
			consol = Generator.GenerateConsol("MB002", Constants.TransportModes.Air);
			shipment1 = Generator.GenerateShipment("HB002", Constants.TransportModes.Air, "AUSYD", "IDJKT");
			shipment1.JS_E_DEP = invoiceDate1;
			shipment1.Consols.Add(consol);
			job = Generator.GenerateJob(shipment1.PK, "J002");
			Generator.GenerateAccTransactionHeader(job.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, dueDate, invoiceDate1, "T2", "Desc2", AUDCode, -42, counterpart.PK);
			consol = Generator.GenerateConsol("MB003", Constants.TransportModes.Air);
			consol.JK_UniqueConsignRef = "TESTCONSOL";
			consol.Shipments.Add(Generator.GenerateShipment("HB003A", Constants.TransportModes.SeaAir, "AUSYD", "IDJKT"));
			consol.Shipments.Add(Generator.GenerateShipment("HB003B", Constants.TransportModes.AirSea, "AUSYD", "IDJKT"));
			consol.Shipments.Add(Generator.GenerateShipment("HB003C", Constants.TransportModes.Other, "AUSYD", "IDJKT"));
			foreach (JASForwardingShipment shipment in consol.Shipments)
			{
				shipment.JS_E_DEP = invoiceDate1;
			}

			Generator.GenerateAccTransactionHeader(consol.JK_UniqueConsignRef, ZArchitecture.Core.LedgerTypes.AccountsPayable, TransactionTypes.AdjustmentNote, dueDate, invoiceDate1, "T3", "Desc3", AUDCode, -43, counterpart.PK);
			shipment1 = Generator.GenerateShipment("HB004", Constants.TransportModes.AirSea, "AUSYD", "IDJKT");
			shipment1.JS_E_DEP = invoiceDate2;
			shipment1.Consols.Add(Generator.GenerateConsol("MB004A", Constants.TransportModes.Sea));
			shipment1.Consols.Add(Generator.GenerateConsol("MB004B", Constants.TransportModes.Air));
			shipment1.Consols.Add(Generator.GenerateConsol("MB004C", Constants.TransportModes.Sea));
			job = Generator.GenerateJob(shipment1.PK, "J004");
			Generator.GenerateAccTransactionHeader(job.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, dueDate, invoiceDate2, "T4", "Desc4", AUDCode, 44, counterpart.PK);
			OrgHeader aPCounterpart = Factory.NewWithValidTestData<OrgHeader>();
			aPCounterpart.OH_Code = "APONLY";
			SetNettingCode(aPCounterpart, "APONLY");
			consol = Generator.GenerateConsol("MB005", Constants.TransportModes.Air);
			shipment1 = Generator.GenerateShipment("HB005", Constants.TransportModes.Air, "AUSYD", "IDJKT");
			shipment1.JS_E_DEP = invoiceDate2;
			shipment1.Consols.Add(consol);
			job = Generator.GenerateJob(shipment1.PK, "J005");
			Generator.GenerateAccTransactionHeader(job.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, dueDate, invoiceDate2, "T5", "Desc5", AUDCode, 45, aPCounterpart.PK);
			consol = Generator.GenerateConsol("MB006", Constants.TransportModes.Sea);
			shipment1 = Generator.GenerateShipment("HB006", Constants.TransportModes.Sea, "AUSYD", "IDJKT");
			shipment1.JS_E_DEP = invoiceDate2;
			shipment1.Consols.Add(consol);
			job = Generator.GenerateJob(shipment1.PK, "J006");
			Generator.GenerateAccTransactionHeader(job.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, dueDate, invoiceDate2, "T6", "Desc6", AUDCode, 46, aPCounterpart.PK);
			OrgHeader aRCounterpart = Factory.NewWithValidTestData<OrgHeader>();
			aRCounterpart.OH_Code = "ARONLY";
			SetNettingCode(aRCounterpart, "ARONLY");
			consol = Generator.GenerateConsol("MB007", Constants.TransportModes.Air);
			shipment1 = Generator.GenerateShipment("HB007", Constants.TransportModes.Air, "AUSYD", "IDJKT");
			shipment1.JS_E_DEP = invoiceDate1;
			shipment1.Consols.Add(consol);
			job = Generator.GenerateJob(shipment1.PK, "J007");
			Generator.GenerateAccTransactionHeader(job.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, dueDate, invoiceDate1, "T7", "Desc7", AUDCode, 47, aRCounterpart.PK);
			consol = Generator.GenerateConsol("MB008", Constants.TransportModes.Sea);
			shipment1 = Generator.GenerateShipment("HB008", Constants.TransportModes.Sea, "AUSYD", "IDJKT");
			shipment1.JS_E_DEP = invoiceDate2;
			shipment1.Consols.Add(consol);
			job = Generator.GenerateJob(shipment1.PK, "J008");
			Generator.GenerateAccTransactionHeader(job.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, dueDate, invoiceDate2, "T8", "Desc8", AUDCode, 48, aRCounterpart.PK);
			OrgHeader nPCounterpart = Factory.NewWithValidTestData<OrgHeader>();
			nPCounterpart.OH_Code = "NONPRT";
			SetNettingCode(nPCounterpart, "NONPRT");
			consol = Generator.GenerateConsol("MB009", Constants.TransportModes.Sea);
			shipment1 = Generator.GenerateShipment("HB009", Constants.TransportModes.Sea, "AUSYD", "IDJKT");
			shipment1.JS_E_DEP = invoiceDate1;
			shipment1.Consols.Add(consol);
			job = Generator.GenerateJob(shipment1.PK, "J009");
			Generator.GenerateAccTransactionHeader(job.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, dueDate, invoiceDate1, "T9", "Desc9", AUDCode, 49, nPCounterpart.PK);
			OrgHeader nonJASCounterpart = Factory.NewWithValidTestData<OrgHeader>();
			nonJASCounterpart.OH_Code = "NONJAS";
			consol = Generator.GenerateConsol("MB010", Constants.TransportModes.Sea);
			shipment1 = Generator.GenerateShipment("HB010", Constants.TransportModes.Sea, "AUSYD", "IDJKT");
			shipment1.JS_E_DEP = invoiceDate2;
			shipment1.Consols.Add(consol);
			job = Generator.GenerateJob(shipment1.PK, "J010");
			Generator.GenerateAccTransactionHeader(job.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, dueDate, invoiceDate2, "T10", "Desc10", AUDCode, 50, nonJASCounterpart.PK);
			consol = Generator.GenerateConsol("MB011", Constants.TransportModes.Air);
			shipment1 = Generator.GenerateShipment("HB011", Constants.TransportModes.Air, "AUSYD", "IDJKT");
			shipment1.JS_E_DEP = invoiceDate2;
			shipment1.Consols.Add(consol);
			job = Generator.GenerateJob(shipment1.PK, "J011");
			Generator.GenerateAccTransactionHeader(job.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable, TransactionTypes.Invoice, dueDate, invoiceDate2, "T11", "Desc11", AUDCode, 51, nonJASCounterpart.PK);
			Factory.Save();
		}

		string GenerateARString_TestExport()
		{
			ArrayList aRStrings = new ArrayList(4);
			StringBuilder builder = new StringBuilder();
			builder.Append("TSJASCRJAS          42.00+01/14/05AUDT2    A AUSYDIDJKT                        INV           ");
			builder.Append("11/15/04MB002         HB002         T2            R");
			builder.Append(System.Environment.NewLine);
			aRStrings.Add(builder.ToString());
			builder.Remove(0, builder.Length);
			builder.Append("TSJASCRJAS          44.00-12/15/04AUDT4    S AUSYDIDJKT                        CRD           ");
			builder.Append("10/15/04MB004A        HB004         T4            R");
			builder.Append(System.Environment.NewLine);
			aRStrings.Add(builder.ToString());
			builder.Remove(0, builder.Length);
			builder.Append("APONLCRJAS          46.00-12/15/04AUDT6    M AUSYDIDJKT                        CRD           ");
			builder.Append("10/15/04MB006         HB006         T6            R");
			builder.Append(System.Environment.NewLine);
			aRStrings.Add(builder.ToString());
			builder.Remove(0, builder.Length);
			builder.Append("ARONLCRJAS          48.00-12/15/04AUDT8    M AUSYDIDJKT                        CRD           ");
			builder.Append("10/15/04MB008         HB008         T8            R");
			builder.Append(System.Environment.NewLine);
			aRStrings.Add(builder.ToString());
			builder.Remove(0, builder.Length);
			builder.Append("NONPRCRJAS          49.00-01/14/05AUDT9    M AUSYDIDJKT                        CRD           ");
			builder.Append("11/15/04MB009         HB009         T9            R");
			builder.Append(System.Environment.NewLine);
			aRStrings.Add(builder.ToString());
			aRStrings.Sort();
			return string.Concat((string[])aRStrings.ToArray(typeof(string))).Trim();
		}

		string GenerateAPString_TestExport()
		{
			ArrayList aPStrings = new ArrayList(2);
			StringBuilder builder = new StringBuilder();
			builder.AppendFormat("CRJAS   TSJAS   2004111520050114{0:yyyyMMdd}MB003               HB003A              T3                  T3                  S", Exporter.NettingCycleDate);
			builder.Append(new string(' ', 19));
			builder.Append("          43.000AUDFDesc3                             ADJ             ");
			builder.Append(System.Environment.NewLine);
			aPStrings.Add(builder.ToString());
			builder.Remove(0, builder.Length);
			builder.AppendFormat("CRJAS   APONLY  2004101520041215{0:yyyyMMdd}MB005               HB005               T5                  T5                  A", Exporter.NettingCycleDate);
			builder.Append(new string(' ', 19));
			builder.Append("          45.000AUDTDesc5                             CRD             ");
			builder.Append(System.Environment.NewLine);
			aPStrings.Add(builder.ToString());
			builder.Remove(0, builder.Length);
			builder.AppendFormat("CRJAS   ARONLY  2004111520050114{0:yyyyMMdd}MB007               HB007               T7                  T7                  A", Exporter.NettingCycleDate);
			builder.Append(new string(' ', 19));
			builder.Append("          47.000AUDTDesc7                             CRD             ");
			builder.Append(System.Environment.NewLine);
			aPStrings.Add(builder.ToString());
			builder.Remove(0, builder.Length);
			builder.AppendFormat("CRJAS   TSJAS   2004111520050114{0:yyyyMMdd}MB001               HB001               T1                  T1                  M", Exporter.NettingCycleDate);
			builder.Append(new string(' ', 19));
			builder.Append("          41.000AUDTDesc1                             CRD             ");
			builder.Append(System.Environment.NewLine);
			aPStrings.Add(builder.ToString());
			aPStrings.Sort();
			return string.Concat((string[])aPStrings.ToArray(typeof(string))).Trim();
		}

		#endregion
		#region Export to Email
		public void TestExport_EmailDeliveryMethod_IndividualRecipient()
		{
			Exporter.DeliveryMethod = PreMatchedDataExporter.EmailDeliveryMethodCode;
			Exporter.IsIndividualEmailRecipient = true;
			Exporter.EmailAddress = "prematcheddataexportertest@edi.com.au";
			Exporter.NettingCycle = Exporter.NettingCycleList[0].Code;
			Exporter.ExportDirectory = Env.TempPath;
			ZString nettingCode = CurrentOrg.CustomsCodes.GetUNC();
			ZString aPNettingCode = nettingCode.EndsWith("COR") ? "COR" + nettingCode.Replace("COR", "") : nettingCode.Right(3) + ZArchitecture.Core.LedgerTypes.AccountsPayable;
			string expectedARFilename = string.Format("{0}{1:yMM}.txt", nettingCode, Exporter.NettingCycleDate);
			string expectedAPFilename = string.Format("{0}{1:yMM}.txt", aPNettingCode, Exporter.NettingCycleDate);
			string aRFileFullPath = Exporter.ExportDirectory + expectedARFilename;
			string aPFileFullPath = Exporter.ExportDirectory + expectedAPFilename;
			try
			{
				AssertEquals("Pre-condition", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				InsertDummyTransactions_TestExport();
				AssertEquals("Is ready", true, Exporter.HasMinimumRequirements);
				Exporter.Export();
				Assert("Should not be exporting it to the export directory", !File.Exists(aRFileFullPath));
				Assert("Should not be exporting it to the export directory", !File.Exists(aPFileFullPath));
				AssertEquals("There should be an email sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef emailSent = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("prematcheddataexportertest@edi.com.au", emailSent.Recipients[0]);
				AssertEquals("AR and AP Pre-Matching files from CargoWise One", emailSent.Subject);
				AssertEquals("AR file should be sent as an attachment", expectedARFilename, emailSent.Attachments[0].DisplayName);
				AssertEquals("AP file should be sent as an attachment", expectedAPFilename, emailSent.Attachments[1].DisplayName);
				AssertMultilineEquals("AR File", GenerateARString_TestExport(), GetStringFromByte(emailSent.Attachments[0].Data), '\n');
				AssertMultilineEquals("AP File", GenerateAPString_TestExport(), GetStringFromByte(emailSent.Attachments[1].Data), '\n');
			}
			finally
			{
				if (File.Exists(aRFileFullPath))
				{
					File.Delete(aRFileFullPath);
				}

				if (File.Exists(aPFileFullPath))
				{
					File.Delete(aPFileFullPath);
				}
			}
		}

		public void TestExport_EmailDeliveryMethod_GroupRecipient()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			group.GG_Code = "MEH";
			GlbStaff newStaff = Factory.New<GlbStaff>();
			newStaff.GS_EmailAddress = "prematcheddataexportertest@edi.com.au";
			newStaff.Groups.Add(group);
			newStaff.GS_Code = "ZAC";
			Factory.Save();
			Exporter.EmailGroups.Add(group);
			Exporter.DeliveryMethod = PreMatchedDataExporter.EmailDeliveryMethodCode;
			Exporter.IsGroupEmailRecipient = true;
			Exporter.EmailGroupPK = group.PK;
			Exporter.NettingCycle = Exporter.NettingCycleList[0].Code;
			Exporter.ExportDirectory = Env.TempPath;
			ZString nettingCode = CurrentOrg.CustomsCodes.GetUNC();
			ZString aPNettingCode = nettingCode.EndsWith("COR") ? "COR" + nettingCode.Replace("COR", "") : nettingCode.Right(3) + ZArchitecture.Core.LedgerTypes.AccountsPayable;
			string expectedARFilename = string.Format("{0}{1:yMM}.txt", nettingCode, Exporter.NettingCycleDate);
			string expectedAPFilename = string.Format("{0}{1:yMM}.txt", aPNettingCode, Exporter.NettingCycleDate);
			string aRFileFullPath = Exporter.ExportDirectory + expectedARFilename;
			string aPFileFullPath = Exporter.ExportDirectory + expectedAPFilename;
			try
			{
				AssertEquals("Pre-condition", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				InsertDummyTransactions_TestExport();
				AssertEquals("Is ready", true, Exporter.HasMinimumRequirements);
				Exporter.Export();
				Assert("Should not be exporting it to the export directory", !File.Exists(aRFileFullPath));
				Assert("Should not be exporting it to the export directory", !File.Exists(aPFileFullPath));
				AssertEquals("There should be an email sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef emailSent = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("prematcheddataexportertest@edi.com.au", emailSent.Recipients[0]);
				AssertEquals("AR and AP Pre-Matching files from CargoWise One", emailSent.Subject);
				AssertEquals("AR file should be sent as an attachment", expectedARFilename, emailSent.Attachments[0].DisplayName);
				AssertEquals("AP file should be sent as an attachment", expectedAPFilename, emailSent.Attachments[1].DisplayName);
				AssertMultilineEquals("AR File", GenerateARString_TestExport(), GetStringFromByte(emailSent.Attachments[0].Data), '\n');
				AssertMultilineEquals("AP File", GenerateAPString_TestExport(), GetStringFromByte(emailSent.Attachments[1].Data), '\n');
			}
			finally
			{
				if (File.Exists(aRFileFullPath))
				{
					File.Delete(aRFileFullPath);
				}

				if (File.Exists(aPFileFullPath))
				{
					File.Delete(aPFileFullPath);
				}
			}
		}

		#endregion
		#region TestExport_ShouldNotIncludeTransactionsFromOtherCompanies
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestExport_ShouldNotIncludedTransactionsFromOtherCompanies()
		{
			Exporter.DeliveryMethod = PreMatchedDataExporter.DirectoryDeliveryMethodCode;
			string testDir = Path.Combine(Env.TempPath, "TestDirPrematchedexport");
			Directory.CreateDirectory(testDir);
			Exporter.ExportDirectory = testDir;
			Exporter.NettingCycle = Exporter.NettingCycleList[0].Code;
			ZString nettingCode = CurrentOrg.CustomsCodes.GetUNC();
			ZString aPNettingCode = nettingCode.EndsWith("COR") ? "COR" + nettingCode.Replace("COR", "") : nettingCode.Right(3) + ZArchitecture.Core.LedgerTypes.AccountsPayable;
			string expectedARFileFullPath = Path.Combine(Exporter.ExportDirectory, string.Format("{0}{1:yMM}.txt", nettingCode, Exporter.NettingCycleDate));
			string expectedAPFileFullPath = Path.Combine(Exporter.ExportDirectory, string.Format("{0}{1:yMM}.txt", aPNettingCode, Exporter.NettingCycleDate));
			var initialUserContext = Env.CurrentUserContext;
			try
			{
				GlbCompany[] testCompanies = SetupNewCompaniesWithBranches();
				TestExport_ShouldNotIncludedTransactionsFromOtherCompanies_InsertDummyTransactions(testCompanies);
				string expectedARFile = GenerateARStringForCompany1();
				string expectedAPFile = GenerateAPStringForCompany1();
				GlbCompany.CurrentCompany.Factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, testCompanies[0].Branches[0].PK.ToGuid(), Env.CurrentDepartment.PK));
				JASDataRegistry.Instance.UseJobInvoiceNumberAsPreMatchingInvoiceRef = false;
				AssertEquals("Is ready", true, Exporter.HasMinimumRequirements);
				Exporter.Export();
				string generatedARFile = GetStringInFile(expectedARFileFullPath);
				string generatedAPFile = GetStringInFile(expectedAPFileFullPath);
				AssertMultilineEquals("AR File", expectedARFile, generatedARFile, '\n');
				AssertMultilineEquals("AP File", expectedAPFile, generatedAPFile, '\n');
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, testCompanies[0].Branches[1].PK.ToGuid(), Env.CurrentDepartment.PK, factory: GlbCompany.CurrentCompany.Factory));
				AssertEquals("Is ready", true, Exporter.HasMinimumRequirements);
				Exporter.Export();
				generatedARFile = GetStringInFile(expectedARFileFullPath);
				generatedAPFile = GetStringInFile(expectedAPFileFullPath);
				AssertEquals(expectedARFile, generatedARFile);
				AssertEquals(expectedAPFile, generatedAPFile);
				expectedARFile = GenerateARStringForCompany2();
				expectedAPFile = GenerateAPStringForCompany2();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, testCompanies[1].Branches[0].PK.ToGuid(), Env.CurrentDepartment.PK, factory: GlbCompany.CurrentCompany.Factory));
				AssertEquals("Is ready", true, Exporter.HasMinimumRequirements);
				Exporter.Export();
				generatedARFile = GetStringInFile(expectedARFileFullPath);
				generatedAPFile = GetStringInFile(expectedAPFileFullPath);
				AssertMultilineEquals("AR File", expectedARFile, generatedARFile, '\n');
				AssertMultilineEquals("AP File", expectedAPFile, generatedAPFile, '\n');
			}
			finally
			{
				Env.SetUserContext(initialUserContext);
				TempDirectory.DeleteDirectory(testDir);
			}
		}

		void CreateTransactionsForBranch1(GlbBranch branch, ZDateTime dueDate, OrgHeader counterpart)
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				JASForwardingConsol consol = Generator.GenerateConsol("MB001", Constants.TransportModes.Sea);
				CommonShipment shipment = Generator.GenerateShipment("HB001", Constants.TransportModes.Sea, "AUSYD", "IDJKT");
				shipment.Consols.Add(consol);
				JobHeader job = Generator.GenerateJob(shipment.PK, "J001");
				Generator.GenerateAccTransactionHeader(job.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, dueDate, new ZDateTime(2004, 11, 15), "T1", "Desc1", AUDCode, 41, counterpart.PK);
				shipment.JS_E_DEP = new ZDateTime(2004, 11, 30);
				consol = Generator.GenerateConsol("MB002", Constants.TransportModes.Air);
				shipment = Generator.GenerateShipment("HB002", Constants.TransportModes.Air, "AUSYD", "IDJKT");
				shipment.Consols.Add(consol);
				job = Generator.GenerateJob(shipment.PK, "J002");
				Generator.GenerateAccTransactionHeader(job.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, dueDate, new ZDateTime(2004, 11, 15), "T2", "Desc2", AUDCode, -42, counterpart.PK);
				shipment.JS_E_DEP = new ZDateTime(2004, 11, 1);
			}
		}

		void CreateTransactionsForBranch2(GlbBranch branch, ZDateTime dueDate, OrgHeader counterpart)
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				JASForwardingConsol consol = Generator.GenerateConsol("MB003", Constants.TransportModes.Air);
				consol.JK_UniqueConsignRef = "TESTCONSOL";
				consol.Shipments.Add(Generator.GenerateShipment("HB003A", Constants.TransportModes.SeaAir, "AUSYD", "IDJKT"));
				consol.Shipments.Add(Generator.GenerateShipment("HB003B", Constants.TransportModes.AirSea, "AUSYD", "IDJKT"));
				consol.Shipments.Add(Generator.GenerateShipment("HB003C", Constants.TransportModes.Other, "AUSYD", "IDJKT"));
				foreach (JASForwardingShipment shipment in consol.Shipments)
				{
					shipment.JS_E_DEP = new ZDateTime(2004, 11, 2);
				}

				Generator.GenerateAccTransactionHeader(consol.JK_UniqueConsignRef, ZArchitecture.Core.LedgerTypes.AccountsPayable, TransactionTypes.AdjustmentNote, dueDate, new ZDateTime(2004, 11, 15), "T3", "Desc3", AUDCode, -43, counterpart.PK);
				JASForwardingShipment shipment1 = Generator.GenerateShipment("HB004", Constants.TransportModes.AirSea, "AUSYD", "IDJKT");
				shipment1.JS_E_DEP = new ZDateTime(2004, 10, 31);
				shipment1.Consols.Add(Generator.GenerateConsol("MB004A", Constants.TransportModes.Sea));
				shipment1.Consols.Add(Generator.GenerateConsol("MB004B", Constants.TransportModes.Air));
				shipment1.Consols.Add(Generator.GenerateConsol("MB004C", Constants.TransportModes.Sea));
				JobHeader job = Generator.GenerateJob(shipment1.PK, "J004");
				Generator.GenerateAccTransactionHeader(job.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, dueDate, new ZDateTime(2004, 10, 15), "T4", "Desc4", AUDCode, 44, counterpart.PK);
			}
		}

		void CreateTransactionsForBranch3(GlbBranch branch, ZDateTime dueDate)
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				OrgHeader aPCounterpart = Factory.NewWithValidTestData<OrgHeader>();
				aPCounterpart.OH_Code = "APONLY";
				SetNettingCode(aPCounterpart, "APONLY");
				JASForwardingConsol consol = Generator.GenerateConsol("MB005", Constants.TransportModes.Air);
				JASForwardingShipment shipment = Generator.GenerateShipment("HB005", Constants.TransportModes.Air, "AUSYD", "IDJKT");
				shipment.Consols.Add(consol);
				JobHeader job = Generator.GenerateJob(shipment.PK, "J005");
				Generator.GenerateAccTransactionHeader(job.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, dueDate, new ZDateTime(2004, 10, 15), "T5", "Desc5", AUDCode, 45, aPCounterpart.PK);
				shipment.JS_E_DEP = new ZDateTime(2004, 10, 15);
				consol = Generator.GenerateConsol("MB006", Constants.TransportModes.Sea);
				shipment = Generator.GenerateShipment("HB006", Constants.TransportModes.Sea, "AUSYD", "IDJKT");
				shipment.Consols.Add(consol);
				job = Generator.GenerateJob(shipment.PK, "J006");
				Generator.GenerateAccTransactionHeader(job.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, dueDate, new ZDateTime(2004, 10, 15), "T6", "Desc6", AUDCode, 46, aPCounterpart.PK);
				shipment.JS_E_DEP = new ZDateTime(2004, 10, 14);
			}
		}

		void CreateTransactionsForBranch4(GlbBranch branch, ZDateTime dueDate)
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				OrgHeader aRCounterpart = Factory.NewWithValidTestData<OrgHeader>();
				aRCounterpart.OH_Code = "ARONLY";
				SetNettingCode(aRCounterpart, "ARONLY");
				JASForwardingConsol consol = Generator.GenerateConsol("MB007", Constants.TransportModes.Air);
				JASForwardingShipment shipment = Generator.GenerateShipment("HB007", Constants.TransportModes.Air, "AUSYD", "IDJKT");
				shipment.Consols.Add(consol);
				JobHeader job = Generator.GenerateJob(shipment.PK, "J007");
				Generator.GenerateAccTransactionHeader(job.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, dueDate, new ZDateTime(2004, 11, 15), "T7", "Desc7", AUDCode, 47, aRCounterpart.PK);
				shipment.JS_E_DEP = new ZDateTime(2004, 11, 15);
				consol = Generator.GenerateConsol("MB008", Constants.TransportModes.Sea);
				shipment = Generator.GenerateShipment("HB008", Constants.TransportModes.Sea, "AUSYD", "IDJKT");
				shipment.Consols.Add(consol);
				job = Generator.GenerateJob(shipment.PK, "J008");
				Generator.GenerateAccTransactionHeader(job.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, dueDate, new ZDateTime(2004, 10, 15), "T8", "Desc8", AUDCode, 48, aRCounterpart.PK);
				shipment.JS_E_DEP = new ZDateTime(2004, 10, 15);
				OrgHeader nPCounterpart = Factory.NewWithValidTestData<OrgHeader>();
				nPCounterpart.OH_Code = "NONPRT";
				SetNettingCode(nPCounterpart, "NONPRT");
				consol = Generator.GenerateConsol("MB009", Constants.TransportModes.Sea);
				shipment = Generator.GenerateShipment("HB009", Constants.TransportModes.Sea, "AUSYD", "IDJKT");
				shipment.Consols.Add(consol);
				job = Generator.GenerateJob(shipment.PK, "J009");
				Generator.GenerateAccTransactionHeader(job.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, dueDate, new ZDateTime(2004, 11, 15), "T9", "Desc9", AUDCode, 49, nPCounterpart.PK);
				shipment.JS_E_DEP = new ZDateTime(2004, 11, 15);
			}
		}

		void TestExport_ShouldNotIncludedTransactionsFromOtherCompanies_InsertDummyTransactions(GlbCompany[] testCompanies)
		{
			ZDateTime dueDate = new ZDateTime(2004, 11, 30);
			OrgHeader counterpart = Factory.NewWithValidTestData<OrgHeader>();
			counterpart.OH_Code = "TSJAS";
			SetNettingCode(counterpart, "TSJAS");
			CreateTransactionsForBranch1(testCompanies[0].Branches[0], dueDate, counterpart);
			CreateTransactionsForBranch2(testCompanies[0].Branches[1], dueDate, counterpart);
			CreateTransactionsForBranch3(testCompanies[0].Branches[2], dueDate);
			CreateTransactionsForBranch4(testCompanies[1].Branches[0], dueDate);
			Factory.Save();
		}

		string GenerateARStringForCompany1()
		{
			ArrayList aRStrings = new ArrayList(2);
			StringBuilder builder = new StringBuilder();
			builder.Append("TSJASCRJAS          42.00+01/14/05AUDT2    A AUSYDIDJKT                        INV           ");
			builder.Append("11/15/04MB002         HB002         T2            R");
			builder.Append(System.Environment.NewLine);
			aRStrings.Add(builder.ToString());
			builder.Remove(0, builder.Length);
			builder.Append("TSJASCRJAS          44.00-12/15/04AUDT4    S AUSYDIDJKT                        CRD           ");
			builder.Append("10/15/04MB004A        HB004         T4            R");
			builder.Append(System.Environment.NewLine);
			aRStrings.Add(builder.ToString());
			builder.Remove(0, builder.Length);
			builder.Append("APONLCRJAS          46.00-12/15/04AUDT6    M AUSYDIDJKT                        CRD           ");
			builder.Append("10/15/04MB006         HB006         T6            R");
			builder.Append(System.Environment.NewLine);
			aRStrings.Add(builder.ToString());
			aRStrings.Sort();
			return string.Concat((string[])aRStrings.ToArray(typeof(string))).Trim();
		}

		string GenerateAPStringForCompany1()
		{
			ArrayList aPStrings = new ArrayList(2);
			StringBuilder builder = new StringBuilder();
			builder.AppendFormat("CRJAS   TSJAS   2004111520050114{0:yyyyMMdd}MB003               HB003A              T3                  T3                  S", Exporter.NettingCycleDate);
			builder.Append(new string(' ', 19));
			builder.Append("          43.000AUDFDesc3                             ADJ             ");
			builder.Append(System.Environment.NewLine);
			aPStrings.Add(builder.ToString());
			builder.Remove(0, builder.Length);
			builder.AppendFormat("CRJAS   APONLY  2004101520041215{0:yyyyMMdd}MB005               HB005               T5                  T5                  A", Exporter.NettingCycleDate);
			builder.Append(new string(' ', 19));
			builder.Append("          45.000AUDTDesc5                             CRD             ");
			builder.Append(System.Environment.NewLine);
			aPStrings.Add(builder.ToString());
			builder.Remove(0, builder.Length);
			builder.AppendFormat("CRJAS   TSJAS   2004111520050114{0:yyyyMMdd}MB001               HB001               T1                  T1                  M", Exporter.NettingCycleDate);
			builder.Append(new string(' ', 19));
			builder.Append("          41.000AUDTDesc1                             CRD             ");
			builder.Append(System.Environment.NewLine);
			aPStrings.Add(builder.ToString());
			aPStrings.Sort();
			return string.Concat((string[])aPStrings.ToArray(typeof(string))).Trim();
		}

		string GenerateARStringForCompany2()
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("ARONLCRJAS          48.00-12/15/04AUDT8    M AUSYDIDJKT                        CRD           ");
			builder.Append("10/15/04MB008         HB008         T8            R");
			builder.Append(System.Environment.NewLine);
			builder.Append("NONPRCRJAS          49.00-01/14/05AUDT9    M AUSYDIDJKT                        CRD           ");
			builder.Append("11/15/04MB009         HB009         T9            R");
			builder.Append(System.Environment.NewLine);
			return builder.ToString().Trim();
		}

		string GenerateAPStringForCompany2()
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendFormat("CRJAS   ARONLY  2004111520050114{0:yyyyMMdd}MB007               HB007               T7                  T7                  A", Exporter.NettingCycleDate);
			builder.Append(new string(' ', 19));
			builder.Append("          47.000AUDTDesc7                             CRD             ");
			builder.Append(System.Environment.NewLine);
			return builder.ToString().Trim();
		}

		GlbCompany[] SetupNewCompaniesWithBranches()
		{
			GlbCompany company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			GlbBranch branch1C1 = company1.Branches.AddNew();
			branch1C1.FillWithValidTestData();
			branch1C1.GB_RL_NKHomePort = "AUSYD";
			GlbBranch branch2C1 = company1.Branches.AddNew();
			branch2C1.FillWithValidTestData();
			branch2C1.GB_RL_NKHomePort = "AUSYD";
			GlbBranch branch3C1 = company1.Branches.AddNew();
			branch3C1.FillWithValidTestData();
			branch3C1.GB_RL_NKHomePort = "AUSYD";
			GlbCompany company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			GlbBranch branch1C2 = company2.Branches.AddNew();
			branch1C2.FillWithValidTestData();
			branch1C2.GB_RL_NKHomePort = "AUSYD";
			Factory.Save();
			return new GlbCompany[] { company1, company2 };
		}

		#endregion
		#region Validation
		public void TestValidation()
		{
			AssertEquals(typeof(PreMatchedDataExporterValidation), Exporter.Validation.GetType());
		}

		public void TestRunPreSaveValidation()
		{
			Exporter.DeliveryMethod = "";
			Exporter.RunPreSaveValidation();
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(Exporter.NettingCycleInfo, true);
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(Exporter.DeliveryMethodInfo, true);
			Exporter.DeliveryMethod = PreMatchedDataExporter.EmailDeliveryMethodCode;
			Exporter.IsIndividualEmailRecipient = true;
			Exporter.RunPreSaveValidation();
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(Exporter.NettingCycleInfo, true);
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(Exporter.DeliveryMethodInfo, false);
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(Exporter.EmailAddressInfo, true);
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(Exporter.EmailGroupPKInfo, false);
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(Exporter.ExportDirectoryInfo, false);
			Exporter.IsGroupEmailRecipient = true;
			Exporter.RunPreSaveValidation();
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(Exporter.NettingCycleInfo, true);
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(Exporter.DeliveryMethodInfo, false);
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(Exporter.EmailAddressInfo, false);
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(Exporter.EmailGroupPKInfo, true);
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(Exporter.ExportDirectoryInfo, false);
			Exporter.DeliveryMethod = PreMatchedDataExporter.DirectoryDeliveryMethodCode;
			Exporter.RunPreSaveValidation();
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(Exporter.NettingCycleInfo, true);
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(Exporter.DeliveryMethodInfo, false);
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(Exporter.EmailAddressInfo, false);
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(Exporter.EmailGroupPKInfo, false);
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(Exporter.ExportDirectoryInfo, true);
		}

		#endregion
		public void TestCancelExport()
		{
			AssertEquals(false, Exporter.ExposedIsCancelled);
			Exporter.CancelExport();
			AssertEquals(true, Exporter.ExposedIsCancelled);
		}

		public void TestGetAPNettingCodeForFileName()
		{
			string generated = Exporter.ExposedGetAPNettingCodeForFileName("TEST");
			AssertEquals("ESTAP", generated);
			generated = Exporter.ExposedGetAPNettingCodeForFileName("");
			AssertEquals(ZArchitecture.Core.LedgerTypes.AccountsPayable, generated);
			generated = Exporter.ExposedGetAPNettingCodeForFileName("USCOR");
			AssertEquals("CORUS", generated);
		}

		#region Property Testing
		public void TestDefaultValues()
		{
			AssertEquals("Default should be email", PreMatchedDataExporter.EmailDeliveryMethodCode, Exporter.DeliveryMethod);
			Assert("Default should be individual email recipient type", Exporter.IsIndividualEmailRecipient);
		}

		public void TestNettingCycleInfo()
		{
			AssertEquals(9, Exporter.NettingCycleInfo.MaxLength);
			AssertEquals("NettingCycle", Exporter.NettingCycleInfo.Name);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1122:DoNotUseDateTimeParse", Justification = "not an issue with Globalization as InvariantCulture is used")]
		public void TestNettingCycleAndCycleClosingDate()
		{
			Exporter.NettingCycle = "";
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(Exporter.NettingCycleInfo, true);
			AssertEquals(ZDateTime.Empty, Exporter.NettingCycleDate);
			AssertEquals(ZDateTime.Empty, Exporter.CycleClosingDate);
			Exporter.NettingCycle = "asdfasdf";
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(Exporter.NettingCycleInfo, false);
			BusinessObjectValidationTestCase.AssertListValidationInvalidCodeError(Exporter.NettingCycleInfo, true);
			AssertEquals(ZDateTime.Empty, Exporter.NettingCycleDate);
			AssertEquals(ZDateTime.Empty, Exporter.CycleClosingDate);
			Exporter.NettingCycle = "11-JAN-11";
			BusinessObjectValidationTestCase.AssertListValidationInvalidCodeError(Exporter.NettingCycleInfo, true);
			Exporter.NettingCycle = Exporter.NettingCycleList[0].Code;
			BusinessObjectValidationTestCase.AssertListValidationInvalidCodeError(Exporter.NettingCycleInfo, false);
			AssertEquals(false, Exporter.NettingCycleInfo.HasErrors());
			ZDateTime expected = new ZDateTime(DateTime.Parse(Exporter.NettingCycleList[0].Code, CultureInfo.InvariantCulture));
			ZDateTime expectedClosing = expected.AddDays(-expected.Day + JASDataRegistry.Instance.NettingPaymentTerms);
			AssertEquals(expected, Exporter.NettingCycleDate);
			AssertEquals(expectedClosing, Exporter.CycleClosingDate);
			using (Exporter.GetValidationSuspender())
			{
				Exporter.NettingCycle = "28-FEB-08";
				AssertEquals(new ZDateTime(2008, 2, 28), Exporter.NettingCycleDate);
				AssertEquals(new ZDateTime(2008, 3, 16), Exporter.CycleClosingDate);
			}
		}

		public void TestExportDirectoryInfo()
		{
			AssertEquals(300, Exporter.ExportDirectoryInfo.MaxLength);
			AssertEquals("ExportDirectory", Exporter.ExportDirectoryInfo.Name);
		}

		public void TestExportDirectory()
		{
			Exporter.DeliveryMethod = PreMatchedDataExporter.DirectoryDeliveryMethodCode;
			var tempPath = Env.TempPath;
			AssertEquals(true, Directory.Exists(tempPath));
			Exporter.ExportDirectory = tempPath;
			AssertEquals(false, Exporter.ExportDirectoryInfo.HasErrors());
			var dir = @"\ThisIsA\Clients\JAS\TestDirectoryThatDoesNotExist_";
			Exporter.ExportDirectory = dir;
			Assert(Exporter.ExportDirectoryInfo.HasErrors());
			AssertEquals(1, Exporter.ExportDirectoryInfo.GetErrors().Count());
			var expectedText = string.Format("Directory \"{0}\" does not exist. Please select a different directory.", dir);
			AssertEquals(expectedText, Exporter.ExportDirectoryInfo.GetErrors().GetFirstMessage());
		}

		public void TestDeliveryMethod()
		{
			Exporter.DeliveryMethod = "ASD";
			AssertEquals("ASD", Exporter.DeliveryMethod);
			BusinessObjectValidationTestCase.AssertListValidationInvalidCodeError(Exporter.DeliveryMethodInfo, true);
			Exporter.DeliveryMethod = PreMatchedDataExporter.EmailDeliveryMethodCode;
			AssertNoErrors("Valid delivery method", Exporter.DeliveryMethodInfo);
			AssertEquals(3, Exporter.DeliveryMethodInfo.MaxLength);
		}

		public void TestIsEmailDeliveryMethod()
		{
			Exporter.DeliveryMethod = PreMatchedDataExporter.DirectoryDeliveryMethodCode;
			Assert("Not email delivery method", !Exporter.IsEmailDeliveryMethod);
			Exporter.DeliveryMethod = PreMatchedDataExporter.EmailDeliveryMethodCode;
			Assert("Should be true", Exporter.IsEmailDeliveryMethod);
		}

		public void TestIsDirectoryDeliveryMethod()
		{
			Exporter.DeliveryMethod = PreMatchedDataExporter.EmailDeliveryMethodCode;
			Assert("Not Directory delivery method", !Exporter.IsDirectoryDeliveryMethod);
			Exporter.DeliveryMethod = PreMatchedDataExporter.DirectoryDeliveryMethodCode;
			Assert("Should be true", Exporter.IsDirectoryDeliveryMethod);
		}

		public void TestEmailAddress()
		{
			Exporter.DeliveryMethod = PreMatchedDataExporter.EmailDeliveryMethodCode;
			Exporter.IsIndividualEmailRecipient = true;
			Exporter.EmailAddress = "asdfsdaf";
			AssertHasErrorContaining(Exporter.EmailAddressInfo, "Email Address is not valid");
			Exporter.EmailAddress = "test@edi.com.au";
			AssertNoErrors("VAlid email address", Exporter.EmailAddressInfo);
		}

		public void TestEmailGroup()
		{
			Exporter.DeliveryMethod = PreMatchedDataExporter.EmailDeliveryMethodCode;
			Exporter.IsGroupEmailRecipient = true;
			GlbGroup group = Exporter.EmailGroups.AddNew();
			group.GG_Code = "MEH";
			Exporter.EmailGroupPK = ZGuid.NewZGuid();
			BusinessObjectValidationTestCase.AssertListValidationInvalidCodeError(Exporter.EmailGroupPKInfo, true);
			Exporter.EmailGroupPK = group.PK;
			AssertNoErrors("Valid email group", Exporter.EmailGroupPKInfo);
		}

		#endregion
		#region Implementation
		OrgHeader CurrentOrg
		{
			get
			{
				return GlbCompany.CurrentCompany.OrgProxy;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			JASDataRegistry.Instance.UseJobInvoiceNumberAsPreMatchingInvoiceRef = false;
			TestCaseHelper.RunClientDbCreateScripts();
			Exporter = new PreMatchedDataExporterForTest();
			OldOrgProxy = TestUNCAndUOCSetter.SetCurrentOrgProxy(Factory, "CRJAS", "CRJAS", "CRJAS");
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = (OldOrgProxy == null) ? ZGuid.Empty : OldOrgProxy.PK;
		}

		string GetStringInFile(string path)
		{
			using (StreamReader reader = new StreamReader(path))
			{
				ArrayList lines = new ArrayList();
				while (reader.Peek() > -1)
				{
					lines.Add(reader.ReadLine() + System.Environment.NewLine);
				}

				lines.Sort();
				return string.Concat((string[])lines.ToArray(typeof(string))).Trim();
			}
		}

		string GetStringFromByte(byte[] data)
		{
			string dataAsString = Encoding.ASCII.GetString(data);
			string[] lines = dataAsString.Split('\n');
			Array.Sort(lines);
			return string.Join("\n", lines).Trim();
		}

		ZString AUDCode
		{
			get
			{
				if (fAUDCode.IsEmpty)
				{
					fAUDCode = Core.Constants.CurrencyCodes.Australia;
				}

				return fAUDCode;
			}
		}

		void Exporter_Processed(object sender, ProcessedEventArgs e)
		{
			// Do nothing
		}

		TestTransactionGenerator Generator
		{
			get
			{
				if (fGenerator == null)
				{
					fGenerator = new TestTransactionGenerator(Factory);
				}

				return fGenerator;
			}
		}

		void SetNettingCode(OrgHeader org, ZString nettingCode)
		{
			TestUNCAndUOCSetter.SetNettingCode(Factory, org, nettingCode);
			AssertEquals(nettingCode, org.CustomsCodes.GetUNC());
		}

		#region ExposedPreMatchedDataExporter class
		class PreMatchedDataExporterForTest : PreMatchedDataExporter
		{
			public bool ExposedIsCancelled
			{
				get
				{
					return IsCancelled;
				}
			}

			public string ExposedGetAPNettingCodeForFileName(string code)
			{
				return GetAPNettingCodeForFileName(code);
			}
		}

		#endregion
		PreMatchedDataExporterForTest Exporter;
		OrgHeader OldOrgProxy;
		ZString fAUDCode;
		TestTransactionGenerator fGenerator;
		#endregion
	}
}
