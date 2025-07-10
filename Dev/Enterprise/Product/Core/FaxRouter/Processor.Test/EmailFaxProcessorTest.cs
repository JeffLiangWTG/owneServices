using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Text;
#if NETFRAMEWORK
using System.Web;
#else
using System.Net.Http;
using System.Net.Mail;
#endif
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.FaxRouter.TypeDefinitions;
using NUnit.Framework;

namespace Enterprise.FaxRouter.Processor.Test
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClassesAnalyzer", Justification = "This assembly has no need to change SqlConnection")]
	sealed class EmailFaxProcessorTest : TransactionedTestCase
	{
		[DeveloperOnlyTest]
		[TestDate(2013, 1, 1)]
		public void TestHandleEmailToFax_RetryMultipleTimesBeforeMarkAsFail()
		{
			var emailFaxProcessor = new EmailFaxProcessorForTesting();

			try
			{
				var faxItem = InsertTestEmailToFaxMailDBItems(1)[0];

				emailFaxProcessor.SetServerIsBusyForTesting(true);
				TestDateAttribute.Date = new DateTime(2013, 1, 1, 0, 1, 0);
				emailFaxProcessor.HandleEmailToFax();
				AssertEquals("Should remain queued so that it can retry again", "QUE", GetMailItemStatus(faxItem));
				AssertEquals("Should not send email to administrator", 0, GetTotalTransmitMailItems());

				TestDateAttribute.Date = new DateTime(2013, 1, 1, 1, 0, 0);
				emailFaxProcessor.HandleEmailToFax();
				AssertEquals("Should remain queued so that it can retry again", "QUE", GetMailItemStatus(faxItem));
				AssertEquals("Should not send email to administrator", 0, GetTotalTransmitMailItems());

				TestDateAttribute.Date = new DateTime(2013, 1, 1, 1, 1, 0);
				emailFaxProcessor.HandleEmailToFax();
				AssertEquals("Should set as failed if still can't send 60 minutes after first try", "FAL", GetMailItemStatus(faxItem));
				AssertEquals("Should send email to administrator when set to fail", 1, GetTotalTransmitMailItems());
			}
			finally
			{
				emailFaxProcessor.RemoveAllGeneratedFiles();
				RemoveTestMailDBItems();
			}
		}

		[DeveloperOnlyTest]
		[TestDate(2024, 5, 1)]
		public void TestHandleEmailToFax_ErrorLogging()
		{
			var emailFaxProcessor = new EmailFaxProcessorForTesting();

			try
			{
				var faxItem = InsertTestEmailToFaxMailDBItems(1)[0];

				emailFaxProcessor.SetServerIsBusyForTesting(true);
				TestDateAttribute.Date = new DateTime(2024, 5, 1, 0, 1, 0);
				emailFaxProcessor.HandleEmailToFax();

				var logs = emailFaxProcessor.Logs.ToString();
				CombineAssertions("", () =>
				{
					Assert(logs.Contains("Checking for new faxes FaxJob"));
					Assert(logs.Contains("FAX JOB [1 of 1] Process Failed"));
				});
			}
			finally
			{
				emailFaxProcessor.RemoveAllGeneratedFiles();
				RemoveTestMailDBItems();
			}
		}

		[DeveloperOnlyTest]
		[TestDate(2024, 5, 1)]
		public void TestFaxAcknowledgements_ErrorLogging()
		{
			ConfigurationSettings.AppSettings["EDI_SEND_FAX_ACK_EMAIL_ON_SUCCESS"] = "1";
			var emailFaxProcessor = new EmailFaxProcessorForTesting();
			try
			{
				InsertTestFaxRecipient(sysId: "UPSSYD", sysFaxJobId: Guid.NewGuid().ToString());
				InsertTestFaxAcknowledgementsMailDBItems(1);
				emailFaxProcessor.HandleFaxAcknowledgements();
				
				var logs = emailFaxProcessor.Logs.ToString();
				CombineAssertions("", () =>
				{
					Assert(logs.Contains("Checking for fax acknowledgements Ack"));
					Assert(logs.Contains("ACK [1 of 1] Failed"));
				});
			}
			finally
			{
				try
				{
					RemoveTestMailDBItems();
				}
				catch { }
				ConfigurationSettings.AppSettings["EDI_SEND_FAX_ACK_EMAIL_ON_SUCCESS"] = "0";
			}
		}

		string GetMailItemStatus(Guid mI_PK)
		{
			using (var connection = BaseDataModule.GetMailDBConnection())
			using (var command = new SqlCommand("SELECT MI_Status FROM dbo.MailDBItems WHERE MI_PK = @MI_PK", connection))
			{
				command.Parameters.AddWithValue("@MI_PK", mI_PK);
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					return (string)reader["MI_Status"];
				}
			}
		}

		[DeveloperOnlyTest]
		public void TestHandleEmailToFaxIsProcessedInBatchesWhenTooMuch()
		{
			var emailBatchSizeForTesting = 10;
			var emailFaxProcessor = new EmailFaxProcessorSpy();
			emailFaxProcessor.SetMaxEmailBatchSizeForTesting(emailBatchSizeForTesting);

			try
			{
				InsertTestEmailToFaxMailDBItems(emailBatchSizeForTesting - 1);
				Assert("Precondition: TotalNewReceivedMailItemsBySubjectSubstring(EDI Fax) < testEmailBatchSize", emailFaxProcessor.GetTotalNewReceivedMailItemsBySubjectSubstring("EDI Fax") < emailBatchSizeForTesting);
				emailFaxProcessor.ResetCallsCount();
				emailFaxProcessor.HandleEmailToFax();
				AssertEquals("emailFaxProcessor.HandleEmailBatchToFaxCalls", 1, emailFaxProcessor.HandleEmailBatchToFaxCalls);

				InsertTestEmailToFaxMailDBItems(emailBatchSizeForTesting * 3);
				Assert("Precondition: GetTotalNewReceivedMailItemsBySubjectSubstring(EDI Fax) > testEmailBatchSize", emailFaxProcessor.GetTotalNewReceivedMailItemsBySubjectSubstring("EDI Fax") > emailBatchSizeForTesting);
				emailFaxProcessor.ResetCallsCount();
				emailFaxProcessor.HandleEmailToFax();
				AssertEquals("emailFaxProcessor.HandleEmailBatchToFaxCalls", 4, emailFaxProcessor.HandleEmailBatchToFaxCalls);
			}
			finally
			{
				RemoveTestMailDBItems();
			}
		}

		[DeveloperOnlyTest]
		public void TestHandleFaxAcknowledgementsIsProcessedInBatchesWhenTooMuch()
		{
			var emailBatchSizeForTesting = 10;
			var emailFaxProcessor = new EmailFaxProcessorSpy();
			emailFaxProcessor.SetMaxEmailBatchSizeForTesting(emailBatchSizeForTesting);

			try
			{
				InsertTestFaxRecipient();
				InsertTestFaxAcknowledgementsMailDBItems(emailBatchSizeForTesting - 1);
				Assert("Precondition: GetTotalNewReceivedMailItemsBySender(FAX_ACK_SENDER) < testEmailBatchSize", emailFaxProcessor.GetTotalNewReceivedMailItemsBySender(MailDataModule.FAX_ACK_SENDER) < emailBatchSizeForTesting);
				emailFaxProcessor.ResetCallsCount();
				emailFaxProcessor.HandleFaxAcknowledgements();
				AssertEquals("emailFaxProcessor.HandleFaxAcknowledgementsBatchCalls", 1, emailFaxProcessor.HandleFaxAcknowledgementsBatchCalls);

				InsertTestFaxAcknowledgementsMailDBItems(emailBatchSizeForTesting * 3);
				Assert("Precondition: GetTotalNewReceivedMailItemsBySender(FAX_ACK_SENDER) > testEmailBatchSize", emailFaxProcessor.GetTotalNewReceivedMailItemsBySender(MailDataModule.FAX_ACK_SENDER) > emailBatchSizeForTesting);
				emailFaxProcessor.ResetCallsCount();
				emailFaxProcessor.HandleFaxAcknowledgements();
				AssertEquals("emailFaxProcessor.HandleFaxAcknowledgementsBatchCalls", 4, emailFaxProcessor.HandleFaxAcknowledgementsBatchCalls);
			}
			finally
			{
				RemoveTestMailDBItems();
			}
		}

		#region Implementation

		int GetTotalTransmitMailItems()
		{
			using (var command = Db.Connection.Command(@"SELECT COUNT(*) FROM dbo.MailDBItems WHERE MI_Direction = 'TRX' AND MI_SUBJECT like '%' + @MI_SUBJECT + '%'"))
			{
				command.AddParameter("@MI_SUBJECT", SqlDbType.VarChar, mailItemSubjectForTestInserts);
				return (int)command.ExecuteScalar();
			}
		}

		List<Guid> InsertTestEmailToFaxMailDBItems(int amount)
		{
			var insertedPks = new List<Guid>();
			using (SqlConnection conn = BaseDataModule.GetMailDBConnection())
			{
				for (var i = 0; i < amount; i++)
				{
					var mI_PK = Guid.NewGuid();

					using (var sqlCmd = new SqlCommand(@"INSERT INTO dbo.MailDBItems
	(MI_PK, MI_Status, MI_DIRECTION, MI_ReceivedDateTime, MI_ContentType, MI_Encoding, MI_SUBJECT, MI_HEADER, MI_BODY, MI_FROM, MI_ReplyTo, MI_SystemCreateUser, MI_SystemCreateTimeUtc, MI_Application, MI_XMLInfo, MI_SystemLastEditUser, MI_SystemLastEditTimeUtc) VALUES
	(@MI_PK, 'QUE', 'RCV', @MI_ReceivedDateTime, 'PLN', '', @MI_SUBJECT, @MI_Header, 'Test Body', @MI_From, 'Test ReplyTo', 'E', GETUTCDATE(), 'STD', '', 'E', GETUTCDATE())", conn))
					{
						sqlCmd.Parameters.AddWithValue("@MI_PK", mI_PK);
						sqlCmd.Parameters.AddWithValue("@MI_ReceivedDateTime", DateTime.Now.Date);
						sqlCmd.Parameters.AddWithValue("@MI_SUBJECT", mailItemSubjectForTestInserts);
						sqlCmd.Parameters.AddWithValue("@MI_From", "<test.from@cargowise.com>");
						sqlCmd.Parameters.AddWithValue("@MI_Header", String.Format("From: <test.from@cargowise.com>\nSubject: {0}", mailItemSubjectForTestInserts));

						sqlCmd.ExecuteNonQuery();
					}

					using (var sqlCmd = new SqlCommand(@"INSERT INTO dbo.MailDBAttachments
	(MA_PK, MA_FileName, MA_DATA, MA_ContentType, MA_Encoding, MA_MI) VALUES
	(@MA_PK, @MA_FileName, @MA_DATA, '', '', @MA_MI)", conn))
					{
						sqlCmd.Parameters.AddWithValue("@MA_PK", Guid.NewGuid());
						sqlCmd.Parameters.AddWithValue("@MA_FileName", "FaxCommand.base64");
						sqlCmd.Parameters.AddWithValue("@MA_DATA", Encoding.UTF8.GetBytes("RkFYTlVNQkVSPSs5NzEgKDQpIDMzNS02NDQzDQpGQVhBVFRFTlRJT049DQpGQVhBVFRFTlRJT05DT01QQU5ZPQ0KU1lTRkFYSk9CSUQ9MDM1NzlhNDEtMmI0MC00NmRkLThjOWQtZDIxMWJlY2Y5YTI2DQpTWVNJRD1lZGlFbnRlcnByaXNlDQpTRU5UREFURVRJTUU9MjAxMzEwMjUxMzEwMDUNCkVOVEVSUFJJU0VDT0RFPU1MUA0KQ09NUEFOWUNPREU9TUVMDQpQSFlTSUNBTFNFUlZFUklEPU1FTA0KRkFYS0VZPTNBLUU2LUI0LUIyLTVFLTc0LTc2LTU0LTQ2LUQ4LTUwLURGLUZDLTNFLThFLUZG"));
						sqlCmd.Parameters.AddWithValue("@MA_MI", mI_PK);
						sqlCmd.ExecuteNonQuery();
					}

					using (var sqlCmd = new SqlCommand(@"INSERT INTO dbo.MailDBAttachments
	(MA_PK, MA_FileName, MA_DATA, MA_ContentType, MA_Encoding, MA_MI) VALUES
	(@MA_PK, @MA_FileName, @MA_DATA, '', 'B64', @MA_MI)", conn))
					{
						sqlCmd.Parameters.AddWithValue("@MA_PK", Guid.NewGuid());
						sqlCmd.Parameters.AddWithValue("@MA_FileName", String.Format("{0}.TIF", uniqueIdentifierForTestInserts));
						sqlCmd.Parameters.AddWithValue("@MA_DATA", Convert.FromBase64String("0x505A755845581B0AB7C40B040BC125B81482BB4329148A1597E2147728EE04B75228109C02C54A70772FD222458B5B8A5D0804F777FFEF6DDE5BFC338B3373BEB39DC599776A985864686868B868C746980ADA8F24943494686851766868D4FF6E955EABF228F1FF2F0478FEE35E2BE8F1FABBBA1CCB387BEFB47D2EF4ABFA2B5774A508318424947E90E96A72BA6880D3CAD06983EDD3FFE868B3B20A94A9D0AB26C3D510A1F73E989B6A69F6AC41F60D86F5E934E8C9CA25330EC21DB98C86474D9DE993E9AA865F8FDBD3816601A1468B923DC8DBAEDFAE8B5282CFBAA8F07C77B11113A6C4A283A303B90AEA055ABF1DABA70EA1FD249A1DCDDCDD8F2B5FDDFCDF6E0371918A5E52C25AF1C76952D7FD976033C1F36EB6032AEFD8B416208D1AF4F2B883E9D2CC40942E8D23C6F9F642DB6BA1406A111F48BCFCA68329BA63E1FC6F8B147F66D37A7F28F70691C32CEEAF6138325B1857DA829B55DCA4B352F7197069FCEB4C7DD0D85FA1825CD003EE0E03AC7CAB2E7E5E9EDC100BF40B50E22EDD02EA10C7AF62173B67FEE9E6A4C98C9CFC5D971F83BDA4107EEE103091B1E5766841196B10775A4CC4E37E1C23FB6044B6C0E6A6056D06E6AF28EC0BD1606A55669131674DA9361B3093379792B7ACB44A565C5214615E813F34D788840C8A50FA186DE6F63A606B83A657A2F77E7754A7E5541780943E2370A1AD0EAE1D820F1E7887BA7B1E460958A5972EE91DEC1A99991AB831914A6B2B649771C375D105316FDA8211D4718E8FBC46B42B6A9279C377665989B7238105867AB0D968D3F677132656B46F66298FA63164B48CAA89A5A737CDEAAEF0843110EDE69A951686A5697A92B5FA6CA9062D4A47D233995AD9A79E5D642C5E97AF78CD2BF6EA9C5B3D84D7DEDFB33BA0D8710F743ACA752D23A14D0BCBD70ABF43A64CA0CBE41E68FD610C64B3B0D93B7F053C482B6D1DBD0C26FC1B059E8C75600D801698C85484A56EF2FBDE4FFC8902CFB5AD98B7B9463A8CFAE08E54F42D718BF2E3DEC2B07D62E8ED6AD3DA54FCEB52EE1DC75DF857AE103C8FD8B717F14FEAAC18CE781AC72F752F602A033EB93865EEE7D854188AC722AE306285844A61E50E0B5C21804EEA995D0645F64750CCF73601A54F5B8E7C590690D2DA6AD2C64175D079954ED59771F2CB2EF69F4286D3E8906BCA1CB449CF72574C1119E19FB1F5EE751962743077556CD50652CC393C2665159DD9934132D610EA1AA3AFF6514D7865C34E300753F78AC64847CB295942CA820C5AC123F5AE03B9C938F05D8B1487DA42641932E9F46FEFB332411838AF67CF8641F4E304AA4823A94277719CC9488518C7AAF7C767E9847173441231755444717F3D0E40B6DCA19BEAB694B28AC7C752E6E1571FE038DBB5813E07A483C18EBED3DBE5B828C9724FA23D255FAADC46643DD3A0B8CBB6DA90329D2AFFFAE51FA1834AAC76B802EA264B12A961236311E5AF072AA47F2CBA041CEA0E90F43670D25F271E07F1A62F8A1082B07FB10D29B8E714ECBD1B64E3C516C208AA3A9AEC9AC51C9B7414579D79CC6C7B5A38DD9A2652D05894CD30013F801ABD9A7DE994B17D42A371AB45AFDA7A151F8517B56AA95597580ED65F60D1657E4ABD3FFA4D3D5B9E1A11822293EDB14B55C6B5362E4ACD715EA9B73458B42CDF7228579DF8BE3B3CF7CF9B69BB5E3EA645610C9AF42483CFC7A64CA76EE32F70E201C75753A306B53854C233CBFE932DDD51E4E69BE096C6F86A6CF1DE7A20C908F730F0E06172B0F580F2D0DFD2DF266A2311D85356B936BEBAFBE54FFA45F4F5CDE3889C11098F787D57D024CA0B2FD8965C1197CBFA886978C12B8B8726E3AAA155688EC1A6B4A412290395B4B0CC606284C6B9800D5971A396A6E9094C8AD1A9B13E75FF4C7D4ED9398986AEF605EC4F9B4D6D52DD850C627F855E624DDC3AB8B46186E7962222AAA5DFD0C0D72E8A7A391C67C215A2E4A857FE2AEEAE635B3264130E5DDA64228F0AB28E5FF581542068A954128EFB8291B5457D4F085E5EF7CC133300221514B3A935E2750930066B8A29127C2EDF14D96544EB250C69403BB4A74AA6D46F20F85B1D77FA26CE20D9D6D4C7E8F25400B9DA07D39615BCFE8A59E9FA825BDD2A2E6075EFEEAE19A6CE2F97F7D1A6D98B993BC1930F6B95E4205E490E96ED2677EA1B68B156038DEB2ED8833333197F12706AA5570558CFF479972C1F027CFBB07E4CD8099AA3C900F4C91E18E79EC868CD701B1E4C7DE7F42C66F0A0020F8F8F3F9E2715072D25A248EB84792952F83B216E9ABE1D5384E1C77F7B46F009D6AF3CF81DB77D0B2AF04A6914E4B3744F5F876793F85AEA71C470BE2E9F4157307F159ADB268ECE9CB38F2A69AEE751FF3964492F19C1123C9D1AB272D1DD971BFD9B43CA8836055CC3B392EED6C518553010DCC095A93D07D1A0F4BE680EF0C8E8C34CCE7A9D3ACEADBE04AA7FAA47C77EAF46CC32A6945E41321BE9CA983764DC470D8C1E4D52881B0168934C06A8BCE20E3A89BC2E71376D24977D041A05E50FE9020C54CA6560F12A6C07577F83943FFE456370E9842ACFD41D859EDDB03FF8E2739EE95328CB977DE96035BD81210BDC8C501C1F58E3C55838AF600D4DB900139CB23D295F2E50536EFFCB30B608962B4E14C716199813CCC3ACCF72059B2015D2E0977CD7E945209EE01A83EF99C3508BBB17C01D01E2269E2BA72D9CBC2BC04E9172C91D5DCA757E417F348733BC55FD5E9AE525ACA83627D6ECE49EACB1C8622B38A7EA943DEE4F99E8B54141EEF8F8F5748311A1E83758AB836078BDFADFF96DA101AC2143FBEBF96C7AA16F17D674E24C3DA241E232BC3257BF3E30A8A946FF54780E0DE8457EF10A8ED0DE3CE4AC7F21B88ECF40BCD4F7798EE62AECB579DB3CF82D06FBCBA9B2187DFD1AFD84ECB9D39AB177D6E967222F094E0265E0753C39EE983A33D287E6668A16EEF80A22B5076107671CDCDB63CA32B2D978C2C075653D636DD502CE04563527ED976A82638FCDA6926CC850A7EAE057E3749885FEA3947B7B686E01DF93DD6D2804ED0BD87FC8D089CA8EE6192FF9C90DC763FDAE77B12BF146E8843AC596638D0F9C3343055EAAE1DFA6839E306C9E1CC14FAFBC2083AFB96C8DDB19AD0FFAF5FAB6BEE50D581FAB73155C087DA2F57B2F7231C22C1E56143F05F72DC2B3F556C852162ED231D00F366FEA1599C72B0C19CA7FBBB777C07BD082C56B50BF08D240FF4EB2FB17B5B71F9706F9719B8D9B3451A856E776DA5B78E6A915190C03F9522E1341885CE0100DA527B3B8E2C473AE715749B32901FC04C880C2D08D77757120D5953987EC8AEEA3C18F1A0048944E6AF36478A65B51F29A1899B39BF6CE3D859E27BB74F475775B18593158953117B7ABE7082CE1C888FD7994FC8EE853C1B3C707246B1F4EFA31DD44D08AD0A2839BD4C9AAAFC54030991778E9ECF6ABC57CAEAD05F157FE36CC5FEB50B8CAB84062F0A9FF4BFC03BDCB19B52EE940EE633B12F3F2C55F3E68914491369517841E11C10483D01D07565DEA99F076C8AA1266EB5838E8909FC6D44BFBB77FD0AF7919B30B7222E6E379E09021A25867F9D0A6EDEB8EDA93796ECD5BF24A48E59426517E774974D5207AB7095A62BFC03870ECCCE876E792E6886AA7CD76BAFDFB40FE69D6452F423B93A1CB85839FC6FF450962E987BBEC562343EF2BE9FE8B0E8D1F7C8B587C96B7B67B7C7E8FE63FCE271C4A7F0DD53F948FD5F1BF103BE58EEBA346BFA3A12272225AAADE24273D26B2A896E093F8D0A2815083B7B455B50608A0FB8278FD47D2C033F746AD68062E49FD280275F2309AB384E14EDFAE37AA512EA4A98888C22D8F127EE8F43562D20E558D2421DD88E3109F7E6268FACC474CCAEF88D2E99C2F5887CB8F52045D4DF8BC23EDFEC436CC0731DF12D1533F95DBE84CC583C44A7E128DBDB35868B9EDE3528514124EB3258DB4467AC2534FF03E18D1E0DC91CFF753D10698D855BCB1727BFD86ABB09072220EF8A22F5290F4C46ABDA847668E2872E130B9F8840A9DB2FF5785AA537B19B90E8F38AE3DB1186755B1A66292BE40EC04F218854381145BF05F59A827EC42B5AD555C12F8F5D819DBFB117D5E8F7488A61A72833F11D6FEE14245273D73244BE57F847DF63DE3CD76A6EE512249B6B0B01D3599F2DEABC37E296B2538D0811D663FC41C72C3427B330898A726D0B0D864FBAC97F5AAD762B3624503165F2469B6A5CF38554B6994C3846D59C2C28711E6596F6A8E858EA728EB52154FDC193D41D24A0B634D13E0BD71D1858ADEDDD9495CE60596D4926F27F8D266B8A1D319DED0EFFFD3B1D63294207AF7CED805E37419BDF10CDBB744C0C7A15BAF7A5EA5176FCE2655F4EA95BC8E5EB3899D461711CBF0A3DC9FD0B649529DF16654E8AB2AD3AA0FFB43DD8024780BBDF43E77E6A19AA8D3CAD5D6E8F503619D6B54A2646F0A40DA52A913F5729C84211ABD70EFAD46526A0C4757B24C8A513A37A238CD56A749892C40D3874163A698FC0A255E36310FCEFCD3D8317530715D78B6C59CC752F807713FB5C4E7D37BD358F5E54B0EB3E177C4545854BB9BC6F75493AA57A5267412BECF487FDF9B8E3504F549B8716C607B05798BD572FE3AB52DE3B72A8325742769C93AB78AFAEDB909FD6FA8FA78D55138CEF1FA3983FCB915E6BE87E5BCB393EA4B96BF14742DDBE6350029D5ED0B6C574F72DAE8D193D8D7BEF5BC005ADCFD03E5C7DE7C48410E6382F3B64FF7EC51D308EB12D6D17453DFAEE5972CA947C463E735129729EC0DD4233EEB768232E115CDE30616D51A095406BF2B8F5A531EDB7435F70747D05C9D4A6B48E5682F13FCC63A50F0E2DE4FEEF142977684C1AF6F9F77B8127DE062905D6453EF28C55DC56DCEB1C99DF6EEE6CBD7E78F61B3FF46A71CEF2D59612C7EBA16EFD80B7556EB6C1B511CD7A1CE050EE8274EF2CF570012EF641F6EE8628049443176F2A9DBAAF54BB7C8DFFC106E7BEA5C219495A37473B206FF95AC4EC76F2B3276C785316B69AD08FB3EA176AC93572042F104E268BC9D2EB78E37F58B1E623F3C63D42813918201CD6FAAAB521711CFA2EE2CA30ADB6D3F29AFB7915BC4EF10B87E593313D60A35144B25C9495C4F54F4C7447DAC49D7BC3416DFE820D3C73F1A659CC62C07684772E5EEC9502E4121C3CB61916CFBFBEDC4892ACAADC4F6F1698A9E6CFB91B4CC06FB041BA9C584D309E13960F39F777526523715AFEC2F2D7B69F2946952933E4AE87ECF34AEF972AEDF0CD953F39A1FD7D26FCF117BABAB56AFAE06D7B7EF0D79EF6D0DB68AA812B90C881E470554E63D03E7B7361C783A79E32049DE360F44B606AFC425550E240E5EDFD721DA8B3139484D478A432B31C4C19F20BC20C3622BEBE0717381244A522CD158E843839F4C9C256FC8AE69510CECED0C6C32E9E156AFEC5627DB008543909D55A9646FABA4ADF78DF412D33112E0E7B208357C79B1583B4DEC2C590F5FC5EB31932C6EBCFA88619D2D7CA2A91996977052C77F652F078BBE46F6CD7D358E2A646F5B05249A550FE42065FA9EDCCBB95A9549A4037B9A5FB09A9F58EDFA279E9609869800E2900E584E5198776482C73BF07F24E928A4131306BC1C5F50F98B26F751EC822C377C8F11817F04822995AD656E0030E7C2969280A6C5A6B1137B85762100B23F16E0CF7649C15DBE6AAC13A669E60D9DE296EF598D423A5269C7E683FB07CC6D816E5D6CB6B5B176FD9C49C2FE087FDAAB93B782702CAAC6824039B29F6F1F4C228CD289E8AC7493722B87CFC7E32D9581D1FF3E4F75E6A86262F1CC46AB746B96EDEF126764B5B9A0DDFCDED171267A1C557347C43BCF109A843153EA76B7BE5F78CE207223EDE00B051EF4A10CA3DA18665F72794FDB95AF40C69E0312E04947114A94D74B4E27D98D1F839EA163B26C6D082A89E3656FA6E783A9BE175753CCE5BD4F5B16FFE4F3FA217D6B29AFB70018055C396366E49F31DAEFC12F6171C308ED23225A3C2B95323273A7F7DB82BAA183246726D6AB17AECB5AD27FB3BB4C2F36663F9219FEF0DDC7CDC13CCB113905230BD143820D443C9084F60CC3EB89CEC6D829FDD4C97B3B4A1AC66BCAA771A1F5073690C82D49BA0E1B96CDA0530A86ED389DBE1A32CF406A45A03E0E99858D1D29A120214B875D96EC6995847B38234D39F10859639CC627C001451718347CF694EF88F5459AB76262B3476C678050C6FAC5507477782BCCD4DCDAC1125F216435A06DD4F5521CB1DECDFC9D6DDD319719BB447D97E321182D825031C99236DABBD58E56DC265EE774EE843CF59F435C4B13AD364E093ACAB273326BF08AF72C7D0337852BF5530B3E5A3181CB4438B665E53B223B3FB73F68A6C72471EFB53C6A5C30E784D23E4B881E5CAD8A4CCB32B05FA86785E84CFD5EE5BB2220B249A3C7D0180C198545522D13941937F9AB8D903CCE147842DBB86D3D514AEDEA0AD26912438486255685A652BAA9FB2E383E60F9062815A6C53B2B5B66534E839EEA6C211FA78CB7B709DABC08B52A9DF145AE59EB9E0AAE1885F9EC6293E68DF5BC18B1F2AD5F8EFEDE34FF473E716A2AA665BB86578E1FC87CA4DFDC1090751B18804734D5CD116E0637645508562F447A474726B7D649160B1DEE487D34B29A5287B9468DFB7C810F27AC47E03BDB355971EBAAB1AE69E2692DA6AD16796FB8EE694E8DC6C2800664D5755E09D4DA5A4BB7A95BB1221C74F54E03A0EF656DDB98B637A44714675E781310D9AB8BF7BF87023AAB0DAA0C86F9931F5CEC28CEBF2DFF8CB2D5DF0D869B0A11FEF9625F655516C8D6D846F7A1EFEC83E0A9BF4EA5F019F53FE0CAA42CE20C9D63C8B946A3DE94FFD3C1DC5F3737219E9C8F4E122C2513EB3E6232E74D754F6B01B64B308D9307518E972C14867A065BBC9FF9D1354EBE35146CC35CBDB4D232FAAC1B378E28EE810C1ADEA4F77D9265D2DFECC6D11D359714B75BEC47CA30230673C7A67F50E21688568369EFD8D3ED7E0E57BC10B47F81555089BB4541AB76FD91EBEF1741B960EBF8474759E730BB4C7D1DE7CC1CBEAC2ABB1E3D78FEDEF7C665085F96A3464BC537CD83DB4F66AD1416AD06816B26073ED0F10ACF15AFAE70934DDAA5820CB4DB388CF13825B159E58FB2A754D63803780937A5E43CD2640B0AFB3CD07F5AF7C58755388A2EA2640D7373EA7CCE0E74311AA019362E7740911A69F8980FDFA028CB8B3A82C5B82C1D966AD3A39D4C20C45F05081577B7C42D9665B1C42A24A5262ACFDB67C844BAFBC6A139248E82455C358ADB30DEAE7BB0131910A0510FE6191964BC07AD2F78C283DE322C2BD38CD6FEF4902F3D85370DB4B297A073061F02A0743786688238ED7DEA9A5FBA93B8A57F1B6C8F375D2109ACEAD3E111873185493F1FCFEF6A566425863C0A1F12114F1B18B7ADF24F3CA96180DDFF239FA922823B670F45C2E789FBF62907C528630AB90DFAAD1C7D6B3A3105C18F09F49793302BD3B4F4E5806F8219198E3D0E338E3D7AA2490F4DEA086919393EDA3263E95675C4EEA3D467C67DA211C54C369594946BA417236377A42C3EDFA9A2F2920E56DC0E4B032975D1A03415D172C7C5C08E808B7C21502E6E0C93278AF5AB54AD9F994DD0DCA78F25B1FBFBAF59AE64EA542C5D7FC5E2466E106AFE09110550F4FF809C7125DECCD8F586BD28DEC5C397450EEEC60EEDDE1482D45AE415AA947BAAF5BEDFEAD89A8C1BC2CF4D7A9867CF73BC6AF4D486F18FBC51DCF7D4794D312C44E10EC18939261F08A95CF6104EE6BCA4D98E9910B5BA31FF5AC501E9948035692BEFE0E71A21076F9FAD9C59A7A4F762E1406E233918484CA79E94C027E1F0620A84B22F330963E7B037D2FC47CD5E982685FD31A546AC464A22867EA34850EC1F2829C7AA535ED2A388CC56CBF62EBEDF009E10C5CB1CAB9474DAD5CBDC7CE03B4C45528192D1F2F2B87BBEC8D1F78365E5453B7FB04D70D0E571FFF150EF3EAF143A4C09B3AD285136B4DFA49BBC861C78297FD1E565618221C338122CECC8B423D6632CFB441E086F949D3DB5B3A87996F2FC79F2774D89F9D26D6B7B5B3866C837E047DDC763A733DF423FA08478E28302BA5BE058D1C672BEEFEBD7A8C47208BB8566DE8BE587DA6060CC750EF6D7747F37AD18CE5AEA443DDB645F7462CE55B9AB955D1432539FFDF9D473463C35150CDAD5807052AC3C9E594BDCC89E5C1BA776BE5FE20D723F7F7324B2DEDE498CF23B60C174A532081E2EBC77A8E46EC3A2646AD528FA0E7AC00252A7F86E00FAB450C146CBBC1C538BBD414AB2C15484C8F7839DFDB09251BC20063021AE8DBD3C467108958595467EC17A00F84D28C275CBFFE94F70D0F176D7979E4A3A8D1D9DE47A2E8C23D191B78DBE127F39E6AE4DEE5F1D46D3709AD7D7EACBDCB34F12295B21C35CFF3EA3BD5343C7E041FBEF95FAFFC77F2DD8DFA961E3FCE700FD5FBEF9776A53FEC7FD0F"));
						sqlCmd.Parameters.AddWithValue("@MA_MI", mI_PK);
						sqlCmd.ExecuteNonQuery();
					}

					insertedPks.Add(mI_PK);
				}
				conn.Close();
			}

			return insertedPks;
		}

		void InsertTestFaxAcknowledgementsMailDBItems(int amount)
		{
			using (SqlConnection conn = BaseDataModule.GetMailDBConnection())
			{
				for (var i = 0; i < amount; i++)
				{
					var mI_PK = Guid.NewGuid();

					using (var sqlCmd = new SqlCommand(@"INSERT INTO dbo.MailDBItems
	(MI_PK, MI_Status, MI_DIRECTION, MI_ReceivedDateTime, MI_ContentType, MI_Encoding, MI_SUBJECT, MI_HEADER, MI_BODY, MI_FROM, MI_ReplyTo, MI_SystemCreateUser, MI_SystemCreateTimeUtc, MI_Application, MI_XMLInfo, MI_SystemLastEditUser, MI_SystemLastEditTimeUtc) VALUES
	(@MI_PK, 'QUE', 'RCV', @MI_ReceivedDateTime, 'PLN', '', @MI_SUBJECT, @MI_HEADER, 'Test Body', @MI_FROM, 'Test ReplyTo', 'E', GETUTCDATE(), 'STD', '', 'E', GETUTCDATE())", conn))
					{
						sqlCmd.Parameters.AddWithValue("@MI_PK", mI_PK);
						sqlCmd.Parameters.AddWithValue("@MI_ReceivedDateTime", DateTime.Now.Date);
						sqlCmd.Parameters.AddWithValue("@MI_SUBJECT", mailItemSubjectForTestInserts);
						sqlCmd.Parameters.AddWithValue("@MI_HEADER", String.Format("Subject: .{0}", uniqueIdentifierForTestInserts));
						sqlCmd.Parameters.AddWithValue("@MI_FROM", MailDataModule.FAX_ACK_SENDER);
						sqlCmd.ExecuteNonQuery();
					}
				}
				conn.Close();
			}
		}

		static void RemoveTestMailDBItems()
		{
			using (SqlConnection conn = MailDataModule.GetMailDBConnection())
			{
				using (var sqlCmd = new SqlCommand(@"DELETE FROM dbo.MailDBItems where MI_SUBJECT like '%' + @MI_SUBJECT + '%'", conn))
				{
					sqlCmd.Parameters.AddWithValue("@MI_SUBJECT", mailItemSubjectForTestInserts);
					sqlCmd.ExecuteNonQuery();
				}

				using (var sqlCmd = new SqlCommand(@"DELETE FROM dbo.MailDBAttachments where MA_FileName = @MA_FileName", conn))
				{
					sqlCmd.Parameters.AddWithValue("@MA_FileName", String.Format("{0}.TIF", uniqueIdentifierForTestInserts));
					sqlCmd.ExecuteNonQuery();
				}
				conn.Close();
			}
		}

		void InsertTestFaxRecipient(string sysId = "", string sysFaxJobId = "")
		{
			using (SqlConnection conn = BaseDataModule.GetEDIFaxDBConnection())
			{
				using (var sqlCmd = new SqlCommand(@"INSERT INTO FaxJobs
		(FaxJobId, ReceivedDateTime, PageCount, EnterpriseCode, CompanyCode, ServerCode, SysId, SysFaxJobId) VALUES
		(@FaxJobId, @ReceivedDateTime, 1, 'UPE', 'SYD', 'SYD', @SysId, @SysFaxJobId)", conn))
				{
					sqlCmd.Parameters.AddWithValue("FaxJobId", uniqueIdentifierForTestInserts);
					sqlCmd.Parameters.AddWithValue("ReceivedDateTime", DateTime.Now);
					sqlCmd.Parameters.AddWithValue("SysId", sysId);
					sqlCmd.Parameters.AddWithValue("SysFaxJobId", sysFaxJobId);
					sqlCmd.ExecuteNonQuery();
				}

				using (var sqlCmd = new SqlCommand(@"INSERT INTO FaxRecipients
		(FaxRecipientId, AttentionName, FaxNumber, Company, FaxJobId, IsAcknowledged, IsAckWarningSent) VALUES
		(@FaxRecipientId, 'TEST NAME', '029999999', 'EDI', @FaxJobId, 0, 0)", conn))
				{
					sqlCmd.Parameters.AddWithValue("FaxRecipientId", uniqueIdentifierForTestInserts);
					sqlCmd.Parameters.AddWithValue("FaxJobId", uniqueIdentifierForTestInserts);
					sqlCmd.ExecuteNonQuery();
				}
				conn.Close();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			faxDbTestHelper = new FaxDbTestHelper();
			faxDbTestHelper.RunFaxDbCreateScripts();

			ConfigurationSettings.AppSettings["MailDBConnectionString"] = String.Format(@"Data Source=localhost; Integrated Security=SSPI; Initial Catalog={0}; Application Name=FaxRouter; Connect Timeout=60; Pooling=false", Db.DatabaseName);
			ConfigurationSettings.AppSettings["EDIFaxDBConnectionString"] = String.Format(@"Data Source=localhost; Integrated Security=SSPI; Initial Catalog={0}; Application Name=FaxRouter; Pooling=false", FaxDbTestHelper.FaxDbName);
			ConfigurationSettings.AppSettings["ENTERPRISE_DATABASE_NAME"] = Db.DatabaseName;
			ConfigurationSettings.AppSettings["AUTO_START"] = "1";

			ConfigurationSettings.AppSettings["FAX_GATEWAY_ADMINISTRATOR_EMAIL"] = "test1@edi.com.au;test2@cargowise.com";
			ConfigurationSettings.AppSettings["FAX_ACK_SENDER"] = "test_sender@edi.com.au";
			ConfigurationSettings.AppSettings["FAX_GATEWAY_POLLING_INTERVAL"] = "30000";
			ConfigurationSettings.AppSettings["FAX_VIEWER_PAGE_HIEGHT"] = "950";
			ConfigurationSettings.AppSettings["FAX_VIEWER_PAGE_WIDTH"] = "750";
			ConfigurationSettings.AppSettings["FAX_GATEWAY_TEMP_FILE_DIRECTORY"] = @"C:\Temp\";
			ConfigurationSettings.AppSettings["FAX_GATEWAY_LOG"] = @"EventLog.txt";

			ConfigurationSettings.AppSettings["ACK_FORMAT"] = "TNZ";
			ConfigurationSettings.AppSettings["BLACKLIST"] = @"djtest@edi.com.au:\+61290251194|0421944394|294|299";
			ConfigurationSettings.AppSettings["DAILY_REPORT_DISCREPANCY_THRESHOLD"] = "0";
			ConfigurationSettings.AppSettings["NOREPLY_EMAIL"] = "noreply@cargowise.com";
		}

		protected override void TearDown()
		{
			faxDbTestHelper.DropFaxDb();

			base.TearDown();
		}

		FaxDbTestHelper faxDbTestHelper;
		const string uniqueIdentifierForTestInserts = "D621C86D-7571-4819-B61C-27F10B028C7C";
		const string mailItemSubjectForTestInserts = "Test EDI Fax D621C86D-7571-4819-B61C-27F10B028C7C";

		#region Helper Classes

		class EmailFaxProcessorForTesting : EmailFaxProcessor
		{
			public EmailFaxProcessorForTesting()
			{
				this.OnFaxProgress += new FaxProgressDelegate(
					(message, status) =>
					{
						Logs.AppendLine($"{message} {status}");
					});
			}

			public void SetServerIsBusyForTesting(bool value)
			{
				ServerShouldBeBusyForTesting = value;
			}

			protected override FaxForwarder GetNewFaxForwarder()
			{
				return new FaxForwarderForTesting(this);
			}

			public void RemoveAllGeneratedFiles()
			{
				try
				{
					foreach (var filename in GeneratedTiffFilenames)
					{
						File.Delete(filename);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}

			bool ServerShouldBeBusyForTesting;
			readonly List<string> GeneratedTiffFilenames = new List<string>();

			public StringBuilder Logs { get; private set; } = new StringBuilder();

			class FaxForwarderForTesting : FaxForwarder
			{
				public FaxForwarderForTesting(EmailFaxProcessorForTesting emailFaxProcessor)
				{
					this.emailFaxProcessor = emailFaxProcessor;
				}

				readonly EmailFaxProcessorForTesting emailFaxProcessor;

				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1086:DoNotUseSystemWebMail", Justification = "Testing")]
#if NETFRAMEWORK
				protected override void SendMail(string smtpServer, System.Web.Mail.MailMessage mailMessage)
				{
					if (emailFaxProcessor.ServerShouldBeBusyForTesting)
					{
						throw new HttpException("The message could not be sent to the SMTP server. The transport error code was 0x800ccc6a. The server response was 451 ESMTP MailEnable Service temporarily refused connection because the server is too busy.");
					}
				}
#else

				protected override void SendMail(string smtpServer, MailMessage mailMessage)
				{
					if (emailFaxProcessor.ServerShouldBeBusyForTesting)
					{
						throw new HttpRequestException("The message could not be sent to the SMTP server. The transport error code was 0x800ccc6a. The server response was 451 ESMTP MailEnable Service temporarily refused connection because the server is too busy.");
					}
				}
#endif

				protected override string GetUniqueTiffFilename()
				{
					var result = base.GetUniqueTiffFilename();
					emailFaxProcessor.GeneratedTiffFilenames.Add(result);
					return result;
				}
			}
		}

		class EmailFaxProcessorSpy : EmailFaxProcessor
		{
			public EmailFaxProcessorSpy()
			{
				AFaxAcknowledgement = new FaxAcknowledgementThatDoesNotSendMail();
			}

			public void SetMaxEmailBatchSizeForTesting(int value)
			{
				maxEmailBatchSize = value;
			}

			public int HandleEmailBatchToFaxCalls
			{
				get;
				private set;
			}

			public int HandleFaxAcknowledgementsBatchCalls
			{
				get;
				private set;
			}

			public void ResetCallsCount()
			{
				HandleEmailBatchToFaxCalls = 0;
				HandleFaxAcknowledgementsBatchCalls = 0;
			}

			protected override void HandleEmailBatchToFax(IEnumerable<MailDBItemDataLine> newMailItems, int startCount, int currentPosition)
			{
				HandleEmailBatchToFaxCalls++;
			}

			protected override void HandleFaxAcknowledgementsBatch(IEnumerable<MailDBItemDataLine> newMailItems, int startCount, int currentPosition)
			{
				HandleFaxAcknowledgementsBatchCalls++;
			}
		}

		class FaxAcknowledgementThatDoesNotSendMail : FaxAcknowledgement
		{
			public override void SendEnterpriseFaxDeliveryNotification(string sysFaxJobId, DeliveryNotificationType status, string email)
			{
			}
		}

		public class FaxDbTestHelper
		{
			public const string FaxDbName = "EDIFaxDBTest";

			public void RunFaxDbCreateScripts()
			{
				using (AdminConnection adminCon = Db.NewAdminConnection())
				{
					DoSql(adminCon, @"IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'" + FaxDbName + "') DROP DATABASE [" + FaxDbName + "]");
					adminCon.CreateDatabase(FaxDbName);
					adminCon.CloseConnection();
				}

				using (DbConnection faxCon = Db.NewAdminConnection(FaxDbName))
				{
					DoSql(faxCon,
	@"CREATE TABLE [dbo].[FaxJobs](
	[FaxJobId] [uniqueidentifier] NOT NULL,
	[ReceivedDateTime] [datetime] NULL,
	[Sender] [varchar](128) NULL,
	[TiffFile] [varbinary](max) NULL,
	[ChargeCode] [varchar](30) NULL,
	[PageCount] [int] NOT NULL,
	[SysId] [varchar](20) NULL,
	[SysFaxJobId] [uniqueidentifier] NULL,
	[ReportedLicenceHeader] [uniqueidentifier] NULL,
 	[EnterpriseCode] varchar(3) NOT NULL,
	[CompanyCode] varchar(3) NOT NULL,
	[ServerCode] varchar(3) NOT NULL
CONSTRAINT [PK_FaxJobs] PRIMARY KEY NONCLUSTERED
(
	[FaxJobId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
---------------------------------------
CREATE TABLE [dbo].[FaxRecipients](
	[FaxRecipientId] [uniqueidentifier] NOT NULL,
	[SentDateTime] [datetime] NULL,
	[AttentionName] [varchar](50) NOT NULL,
	[FaxNumber] [varchar](50) NOT NULL,
	[Company] [varchar](50) NOT NULL,
	[FaxJobId] [uniqueidentifier] NOT NULL,
	[IsAcknowledged] [int] NOT NULL,
	[AckDateTime] [datetime] NULL,
	[AckSuccess] [int] NULL,
	[IsAckWarningSent] [int] NOT NULL,
 CONSTRAINT [PK_FaxRecipients] PRIMARY KEY NONCLUSTERED
(
	[FaxJobId] ASC,
	[FaxRecipientId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY];

ALTER TABLE [dbo].[FaxRecipients] ADD  CONSTRAINT [DF_FaxRecipients_IsAckWarningSent]  DEFAULT (0) FOR [IsAckWarningSent];

---------------------------------------
CREATE TABLE [dbo].[FaxAcknowledgements](
	[AckId] [uniqueidentifier] NOT NULL,
	[AckBody] [text] NULL,
	[AckChargeCode] [nvarchar](50) NULL,
	[AckReceivedDateTime] [datetime] NULL,
	[FaxReciepientId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_FaxAcknowledgements] PRIMARY KEY NONCLUSTERED
(
	[AckId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
");
					faxCon.CloseConnection();
				}
			}

			const string TearDownSql =
	@"IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'" + FaxDbName + @"')
DROP DATABASE [" + FaxDbName + "]";

			public void DropFaxDb()
			{
				DoSql(TearDownSql);
			}

			void DoSql(DbConnection con, string sql)
			{
				con.ExecuteNonQuery(sql);
			}

			void DoSql(string sql)
			{
				using (DbConnection con = Db.NewAdminConnection())
				{
					con.ExecuteNonQuery(sql);
				}
			}
		}

#endregion

#endregion
	}
}
