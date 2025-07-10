using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CARMDailyNoticeDocumentWrapperTest : TestCaseWithFactory
	{
		#region TestSourceIdentifierProvider

		public void TestISourceIdentifierProvider()
		{
			var header = CreateCusStatementHeaderWithMessages(Factory, CusStatementHeaderTypes.Codes.Importer);
			var wrapper = new CARMDailyNoticeDocumentWrapper(header);

			var supporter = wrapper as ISourceIdentifierProvider;
			AssertNotNull("CARMDailyNoticeDocumentWrapper should implement ISourceIdentifierProvider", supporter);
			AssertEquals("supporter.SourceIdentifier", header.PK, supporter?.SourceIdentifier);
		}

		#endregion

		public void TestNoExceptionThrownWhenProgTypeIsNull()
		{
			var header = Factory.New<CusStatementHeader>();
			header.B2_StatementType = CusStatementHeaderTypes.Codes.Importer;
			header.B2_ImporterCustomsID = "100023258";
			header.B2_PrintDate = new ZDateTime(2022, 01, 01, 00, 00, 00);
			header.B2_StatementAmount = 337837.53;
			var message = header.Messages.AddNew();
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				message.EM_MessageText = resourceRetriever.GetString("Enterprise.Customs.CA.Business.Test.DocWrappers.CARM.DailyNotice.TestFiles.ZCARMDNOTICEAH_03.xml");
			}
			AssertNoExceptionThrown(() => new CARMDailyNoticeDocumentWrapper(header));
		}

		public void TestPropertiesWhenStatementTypeIsImporter()
		{
			var header = CreateCusStatementHeaderWithMessages(Factory, CusStatementHeaderTypes.Codes.Importer);
			var wrapper = new CARMDailyNoticeDocumentWrapper(header);
			CombineAssertions(() =>
			{
				AssertEquals("IsImporterHeader", ZBool.True, wrapper.IsImporterHeader);
				AssertEquals("IsBrokererHeader", ZBool.False, wrapper.IsBrokererHeader);
				AssertEquals("ProcessDate", new ZDateTime(2022, 01, 01), wrapper.ProcessDate);
				AssertEquals("BusinessNumber", "100023258", wrapper.BusinessNumber);
				AssertEquals("LegalName", ZString.Empty, wrapper.LegalName);
				AssertEquals("OperatingName", ZString.Empty, wrapper.OperatingName);
				AssertEquals("ProgramName", ZString.Empty, wrapper.ProgramName);
				AssertEquals("ProgramAccountNum", "100023258RM0001", wrapper.ProgramAccountNum);
				AssertEquals("StatementAmount", 337837.53m, wrapper.StatementAmount);
				AssertEquals("PaidAmount", 0m, wrapper.PaidAmount);
				AssertEquals("RefundAmount", 0m, wrapper.RefundAmount);
				AssertEquals("InterestAmount", 0m, wrapper.InterestAmount);
				AssertEquals("OthersAmount", 0m, wrapper.OthersAmount);
				AssertEquals("Duties", 252408m, wrapper.Duties);
				AssertEquals("ExciseTax", 0m, wrapper.ExciseTax);
				AssertEquals("ExciseDuties", 1242.03m, wrapper.ExciseDuties);
				AssertEquals("SIMA", 0m, wrapper.SIMA);
				AssertEquals("GST", 84187.5m, wrapper.GST);
				AssertEquals("HST", 0m, wrapper.HST);
				AssertEquals("PST", 0m, wrapper.PST);
				AssertEquals("Interest", 0m, wrapper.Interest);
				AssertEquals("Penalties", 0m, wrapper.Penalties);
				AssertEquals("Payments", 0m, wrapper.Payments);
				AssertEquals("Others", 0m, wrapper.Others);
			});

			var details = wrapper.ImporterDailyNoticeDetails;
			CombineAssertions(() =>
			{
				AssertEquals("ImporterDailyNoticeDetails.Count", 2, details.Count);
				AssertEquals("CADVersion", "00001", details[0].CADVersion);
				AssertEquals("CADVersion", "00002", details[1].CADVersion);

				AssertEquals("ReleaseDate", new ZDate(2020, 02, 29), details[0].ReleaseDate);
				AssertEquals("AccountingDate", new ZDate(2022, 02, 03), details[0].AccountingDate);
				AssertEquals("TransactionType", "B2", details[0].TransactionType);
				AssertEquals("TransactionDescription", "Reassessment (B2-1)", details[0].TransactionDescription);
				AssertEquals("ReferenceNumber", "010000053358", details[0].ReferenceNumber);
				AssertEquals("CADNumber", "99999999991234", details[0].CADNumber);
				AssertEquals("SubmittedBy", "178234732", details[0].SubmittedBy);
				AssertEquals("AccountingStatus", "ABC", details[0].AccountingStatus);
				AssertEquals("PortNumber", "Port1", details[0].PortNumber);
				AssertEquals("CustomsDuty", 100000m, details[0].CustomsDuty);
				AssertEquals("ExciseTax", 0m, details[0].ExciseTax);
				AssertEquals("ExciseDuties", 0m, details[0].ExciseDuties);
				AssertEquals("SIMA", 0m, details[0].SIMA);
				AssertEquals("GST", 10000m, details[0].GST);
				AssertEquals("HST", 0m, details[0].HST);
				AssertEquals("PST", 0m, details[0].PST);
				AssertEquals("Interest", 0m, details[0].Interest);
				AssertEquals("Penalties", 0m, details[0].Penalties);
				AssertEquals("Payments", 0m, details[0].Payments);
				AssertEquals("PaymentDueDate", new ZDate(2022, 03, 31), details[0].PaymentDueDate);
				AssertEquals("Others", 0m, details[0].Others);
				AssertEquals("TotalAmount", 10m, details[0].TotalAmount);
			});

			var notes = wrapper.DailyNoticeNotes;
			AssertEquals("Message", "DNAH - The CARM Client Portal is now live. Check the CBSA Website for more information.", notes);
		}

		public void TestPropertiesWhenStatementTypeIsBroker()
		{
			var header = CreateCusStatementHeaderWithMessages(Factory, CusStatementHeaderTypes.Codes.Broker);
			var wrapper = new CARMDailyNoticeDocumentWrapper(header);
			CombineAssertions(() =>
			{
				AssertEquals("IsImporterHeader", ZBool.False, wrapper.IsImporterHeader);
				AssertEquals("IsBrokererHeader", ZBool.True, wrapper.IsBrokererHeader);
				AssertEquals("ProcessDate", new ZDateTime(2022, 01, 01), wrapper.ProcessDate);
				AssertEquals("BusinessNumber", "100023258", wrapper.BusinessNumber);
				AssertEquals("OperatingName", "Broker 178234732", wrapper.OperatingName);
				AssertEquals("LicenseNumber", "9999", wrapper.LicenseNumber);
				AssertEquals("TotalImportationAmount", 110000m, wrapper.TotalImportationAmount);
			});

			var details = wrapper.BrokerDailyNoticeDetails;
			CombineAssertions(() =>
			{
				AssertEquals("BrokerDailyNoticeDetails.Count", 1, details.Count);
				AssertEquals("LegalName", "Legal Name", details[0].LegalName);
				AssertEquals("ProgramAccountNum", "123456789123457", details[0].ProgramAccountNum);
				AssertEquals("DailyNoticeDetails.Count", 2, details[0].DailyNoticeDetails.Count);
				AssertEquals("DailyNoticeDetails.CADVersion", "00001", details[0].DailyNoticeDetails[0].CADVersion);
				AssertEquals("DailyNoticeDetails.CADVersion", "00002", details[0].DailyNoticeDetails[1].CADVersion);

				AssertEquals("DailyNoticeDetails.ReleaseDate", new ZDate(2020, 02, 29), details[0].DailyNoticeDetails[1].ReleaseDate);
				AssertEquals("DailyNoticeDetails.AccountingDate", new ZDate(2022, 02, 03), details[0].DailyNoticeDetails[1].AccountingDate);
				AssertEquals("DailyNoticeDetails.TransactionType", "B2", details[0].DailyNoticeDetails[1].TransactionType);
				AssertEquals("DailyNoticeDetails.TransactionDescription", ZString.Empty, details[0].DailyNoticeDetails[1].TransactionDescription);
				AssertEquals("DailyNoticeDetails.ReferenceNumber", "010000053358", details[0].DailyNoticeDetails[1].ReferenceNumber);
				AssertEquals("DailyNoticeDetails.CADNumber", "99999999991234", details[0].DailyNoticeDetails[1].CADNumber);
				AssertEquals("DailyNoticeDetails.SubmittedBy", "178234732", details[0].DailyNoticeDetails[1].SubmittedBy);
				AssertEquals("DailyNoticeDetails.AccountingStatus", "BCD", details[0].DailyNoticeDetails[1].AccountingStatus);
				AssertEquals("DailyNoticeDetails.PortNumber", "Port2", details[0].DailyNoticeDetails[1].PortNumber);
				AssertEquals("DailyNoticeDetails.CustomsDuty", 0m, details[0].DailyNoticeDetails[1].CustomsDuty);
				AssertEquals("DailyNoticeDetails.ExciseTax", 0m, details[0].DailyNoticeDetails[1].ExciseTax);
				AssertEquals("DailyNoticeDetails.ExciseDuties", 0m, details[0].DailyNoticeDetails[1].ExciseDuties);
				AssertEquals("DailyNoticeDetails.SIMA", 10m, details[0].DailyNoticeDetails[1].SIMA);
				AssertEquals("DailyNoticeDetails.GST", 0m, details[0].DailyNoticeDetails[1].GST);
				AssertEquals("DailyNoticeDetails.HST", 0m, details[0].DailyNoticeDetails[1].HST);
				AssertEquals("DailyNoticeDetails.PST", 0m, details[0].DailyNoticeDetails[1].PST);
				AssertEquals("DailyNoticeDetails.Interest", 10m, details[0].DailyNoticeDetails[1].Interest);
				AssertEquals("DailyNoticeDetails.Penalties", 0m, details[0].DailyNoticeDetails[1].Penalties);
				AssertEquals("DailyNoticeDetails.Payments", 0m, details[0].DailyNoticeDetails[1].Payments);
				AssertEquals("DailyNoticeDetails.PaymentDueDate", new ZDate(2022, 03, 31), details[0].DailyNoticeDetails[1].PaymentDueDate);
				AssertEquals("DailyNoticeDetails.Others", 0m, details[0].DailyNoticeDetails[1].Others);
				AssertEquals("DailyNoticeDetails.TotalAmount", 20m, details[0].DailyNoticeDetails[1].TotalAmount);
			});

			var notes = wrapper.DailyNoticeNotes;
			AssertEquals("Message", "DNCB - The CARM Client Portal is now live. Check the CBSA Website for more information.", notes);
		}

		static CusStatementHeader CreateCusStatementHeaderWithMessages(BusinessObjectFactory factory, string statementType)
		{
			var header = factory.New<CusStatementHeader>();
			header.B2_StatementType = statementType;
			header.B2_ImporterCustomsID = "100023258";
			header.B2_PrintDate = new ZDateTime(2022, 01, 01, 00, 00, 00);
			header.B2_StatementAmount = 337837.53;

			var line1 = header.StatementLines.AddNew();
			line1.B3_EntryDate = new ZDate(2020, 02, 29);
			line1.B3_ScheduledProcessDate = new ZDate(2022, 02, 03);
			line1.B3_DueDate = new ZDate(2022, 03, 31);
			line1.B3_EntryType = "B2";
			line1.CARMTransactionDescription = "Reassessment (B2-1)";
			line1.B3_AssociatedEntry = "010000053358";
			line1.B3_EntryNum = "99999999991234";
			line1.CARMCADVersion = "00001";
			line1.CARMSubmittedBy = "178234732";
			line1.CARMPort = "Port1";
			line1.CARMStatus = "ABC";
			line1.CARMTotal = 10m;

			var chargeline1 = line1.Charges.AddNew();
			chargeline1.B4_ChargeType = CARMDailyNoticeChargeTypeList.Codes.Duties;
			chargeline1.B4_ChargeAmount = 100000m;

			var chargeline2 = line1.Charges.AddNew();
			chargeline2.B4_ChargeType = CARMDailyNoticeChargeTypeList.Codes.GoodsAndServicesTax;
			chargeline2.B4_ChargeAmount = 10000m;

			var line2 = header.StatementLines.AddNew();
			line2.B3_EntryDate = new ZDate(2020, 02, 29);
			line2.B3_ScheduledProcessDate = new ZDate(2022, 02, 03);
			line2.B3_DueDate = new ZDate(2022, 03, 31);
			line2.B3_EntryType = "B2";
			line2.CARMTransactionDescription = ZString.Empty;
			line2.B3_AssociatedEntry = "010000053358";
			line2.B3_EntryNum = "99999999991234";
			line2.CARMCADVersion = "00002";
			line2.CARMSubmittedBy = "178234732";
			line2.CARMPort = "Port2";
			line2.CARMStatus = "BCD";
			line2.CARMTotal = 20m;

			var chargeline3 = line2.Charges.AddNew();
			chargeline3.B4_ChargeType = CARMDailyNoticeChargeTypeList.Codes.SIMA;
			chargeline3.B4_ChargeAmount = 10m;

			var chargeline4 = line2.Charges.AddNew();
			chargeline4.B4_ChargeType = CARMDailyNoticeChargeTypeList.Codes.Interest;
			chargeline4.B4_ChargeAmount = 10m;

			var message1 = header.Messages.AddNew();
			var message2 = header.Messages.AddNew();
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				if (statementType == CusStatementHeaderTypes.Codes.Broker)
				{
					message1.EM_MessageText = resourceRetriever.GetString("Enterprise.Customs.CA.Business.Test.DocWrappers.CARM.DailyNotice.TestFiles.ZCARMDNOTICECB_01.xml");
					message2.EM_MessageText = resourceRetriever.GetString("Enterprise.Customs.CA.Business.Test.DocWrappers.CARM.DailyNotice.TestFiles.ZCARMDNOTICECB_02.xml");
					header.CreateOrUpdateCustomNote(StatementMessageProcessorHelper.EnglishMessageToRecipient, "DNCB - The CARM Client Portal is now live. Check the CBSA Website for more information.");
					header.B2_ImporterCustomsID = "100023258";

					line1.B3_ImporterCustomsID = "123456789123457";
					line2.B3_ImporterCustomsID = "123456789123457";

					var importer = factory.NewWithValidTestData<OrgHeader>();
					importer.OH_Code = "100023259";
					importer.OH_FullName = "Legal Name";

					var lineGroup = header.LineGroupCollection.AddNew();
					lineGroup.B10_ImporterCustomsID = "123456789123457";
					lineGroup.B10_OH_Importer = importer.PK;
				}
				else if (statementType == CusStatementHeaderTypes.Codes.Importer)
				{
					message1.EM_MessageText = resourceRetriever.GetString("Enterprise.Customs.CA.Business.Test.DocWrappers.CARM.DailyNotice.TestFiles.ZCARMDNOTICEAH_01.xml");
					message2.EM_MessageText = resourceRetriever.GetString("Enterprise.Customs.CA.Business.Test.DocWrappers.CARM.DailyNotice.TestFiles.ZCARMDNOTICEAH_02.xml");
					header.CreateOrUpdateCustomNote(StatementMessageProcessorHelper.EnglishMessageToRecipient, "DNAH - The CARM Client Portal is now live. Check the CBSA Website for more information.");
				}
			}

			return header;
		}
	}
}
