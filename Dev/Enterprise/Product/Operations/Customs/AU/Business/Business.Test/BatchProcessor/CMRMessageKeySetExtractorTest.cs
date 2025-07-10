using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRMessageKeySetExtractorTest : TestCaseWithFactory
	{
		public void TestNormalMessageWithNoKeys()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_MessageText = @"
UNH+000002+CUSRES:D:99B:UN
BGM+34:::CARST+28G7 2ACB C2AA:0001+8
DTM+9:20200416152044499127:ZZZ
DTM+132:20200416:102
FTX+AHN+++CONSOLIDATED STATUS:CLEAR
FTX+AHN+++CARGO REPORT SAC:NO
TDT+20+16++6+PW::3
LOC+12+AUSYD::6
LOC+4+9914N::95
NAD+MR+AAA374M::95
NAD+UD+41065894724::95
RFF+MWB:
RFF+HWB:PWS16604
UNT+15+000002".Trim().Replace("\r\n", "'").Replace('\n', '\'');

			var keySetExtractor = new CMRMessageKeySetExtractor();
			AssertEquals(
				$"Message (PK={message.PK}, Type='') has no associated keys.",
				AssertExceptionThrown<InvalidOperationException>(() => keySetExtractor.ExtractKeys(message)).Message
			);
		}

		public void TestSamples_CARST()
		{
			TestSamples("CARST-Message-Keys.txt");
		}

		public void TestSamples_CUSRES()
		{
			TestSamples("CUSRES-Message-Keys.txt");
		}

		protected override void SetUp()
		{
			base.SetUp();
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
		}
		EmbeddedResourceRetriever embeddedResourceRetriever;

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever.Dispose();
		}

		void TestSamples(string filename)
		{
			var keySetExtractor = new CMRMessageKeySetExtractor();

			CombineAssertions(() =>
			{
				foreach (var testBlock in ReadTestBlocks(filename))
				{
					var testName = testBlock.Sections[0].Single();
					var messageType = testBlock.Sections[1].Single();
					var expectedKeys = testBlock.Sections[2].ToArray();
					var messageText = string.Join("'", testBlock.Sections[3]);

					var messagePK = ZGuid.NewZGuid();
					Db.Connection.ExecuteNonQuery(@"
						insert into dbo.EDIMessage (EM_PK, EM_GB, EM_GE, EM_ApplicationCode, EM_MessageType, EM_ReceiveTransmit, EM_Status, EM_MessageText, EM_SystemCreateTimeUtc, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser)
						values (@messagePK, @branchPK, @departmentPK, 'CMR', @messageType, 'RCV', 'QUE', @messageText, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
					", c =>
					{
						c.AddParameter("@messagePK", SqlDbType.UniqueIdentifier, messagePK.ToGuid());
						c.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, GlbBranch.GetCurrentBranch(Factory).PK.ToGuid());
						c.AddParameter("@departmentPK", SqlDbType.UniqueIdentifier, GlbDepartment.GetCurrentDepartment(Factory).PK.ToGuid());
						c.AddParameter("@messageType", SqlDbType.VarChar, messageType);
						c.AddParameter("@messageText", SqlDbType.VarChar, messageText);
					});
					var message = Factory.Load<EDIMessage>(messagePK);
					message.EM_MessageText = messageText;

					var keySet = keySetExtractor.ExtractKeys(message);
					AssertContainsExactElementsInAnyOrder($"{testName} {nameof(keySet.DependsOn)}", expectedKeys, keySet.DependsOn);
					AssertContainsExactElementsInAnyOrder($"{testName} {nameof(keySet.Affects)}", expectedKeys, keySet.Affects);
				}
			});
		}

		IReadOnlyList<TestBlock> ReadTestBlocks(string filename)
		{
			const string blockDelimiter = "====";
			const string sectionDelimiter = "----";

			var blocks = new List<TestBlock>();
			TestBlock currentBlock = null;
			List<string> currentSection = null;

			string path = embeddedResourceRetriever.SaveResourceToFile("Enterprise.Customs.AU.Declaration.Business.Testing.BatchProcessor.TestFiles." + filename);
			foreach (var line in File.ReadAllLines(path))
			{
				if (line == blockDelimiter)
				{
					currentBlock = new TestBlock();
					blocks.Add(currentBlock);
					currentSection = new List<string>();
					currentBlock.Sections.Add(currentSection);
				}
				else if (currentBlock == null)
				{
					throw new IOException($"{nameof(TestBlock)} should start with a line '{blockDelimiter}'.");
				}
				else if (line == sectionDelimiter)
				{
					currentSection = new List<string>();
					currentBlock.Sections.Add(currentSection);
				}
				else
				{
					currentSection.Add(line);
				}
			}

			return blocks;
		}

		sealed class TestBlock
		{
			public List<List<string>> Sections { get; } = new List<List<string>>();
		}
	}
}
