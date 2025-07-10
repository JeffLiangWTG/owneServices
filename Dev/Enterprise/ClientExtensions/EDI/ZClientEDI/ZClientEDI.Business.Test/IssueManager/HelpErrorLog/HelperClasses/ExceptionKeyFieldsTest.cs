using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;

namespace Enterprise.Client.EDI.IssueManager.Business.Test
{
	class ExceptionKeyFieldsTest : TestCaseWithFactory
	{
		public virtual void TestCopyToLog()
		{
			var log = Factory.New<EdiHelpErrorLog>();
			Keys.CopyToLog(log);
			AssertEquals("Source", "", log.HE_ExceptionSource);
			AssertEquals("Type", "", log.HE_ExceptionType);
			AssertEquals("Message", "", log.HE_ExceptionMessage);
			AssertEquals("Key", "", log.Keys[0].HK_Key);
			AssertEquals(ZBool.False, log.HE_IsClientVisible);
		}

		public virtual void TestCopyToLogControlCharacters()
		{
			var log = Factory.New<EdiHelpErrorLog>();
			Keys.Message = '\x1f' + "aaa" + '\x1f';
			Keys.CopyToLog(log);
			AssertEquals("Message", "?aaa?", log.HE_ExceptionMessage);
		}
		public virtual void TestCopyToLog_ChineseCharacters()
		{
			var log = Factory.New<EdiHelpErrorLog>();
			Keys.Message = '\x1f' + "无法从传输连接中读取数据: 你的主机中的软件中止了一个已建立的连接。。" + '\x1f';
			Keys.CopyToLog(log);
			AssertEquals("Message", "?无法从传输连接中读取数据: 你的主机中的软件中止了一个已建立的连接。。?", log.HE_ExceptionMessage);
		}
		public virtual void TestCopyToLog_EnglishCharacters()
		{
			var log = Factory.New<EdiHelpErrorLog>();
			Keys.Message = '\x1f' + "azAZ" + '\x1f';
			Keys.CopyToLog(log);
			AssertEquals("Message", "?azAZ?", log.HE_ExceptionMessage);
		}
		public virtual void TestCopyToLog_DigitSpecialCharacters()
		{
			var log = Factory.New<EdiHelpErrorLog>();
			Keys.Message = '\x1f' + "azAZ09!@#$%^&*()_-+=~`{}[]|\':;<,>.?/" + '\x1f';
			Keys.CopyToLog(log);
			AssertEquals("Message", "?azAZ09!@#$%^&*()_-+=~`{}[]|\':;<,>.?/?", log.HE_ExceptionMessage);
		}

		public virtual void TestMatchingLog()
		{
			var logs = new ScalableHelpErrorLogCollection(Factory);
			AssertNull(Keys.MatchingLog(logs));

			logs = new ScalableHelpErrorLogCollection(Factory);
			var log = Factory.New<EdiHelpErrorLog>();
			Keys.CopyToLog(log);
			Factory.Save();
			AssertEquals(log, Keys.MatchingLog(logs));
		}

		public void TestLogKeyIsTrimmed()
		{
			Keys.CallStack = "should be trimmed ";
			AssertEquals(Keys.LogKey.Trim(), Keys.LogKey);
		}

		public void TestMessage()
		{
			Keys = new ExceptionKeyFields();
			Keys.AddMessage("  " + ZString.Replicate('m', ExceptionKeyFields.HE_ExceptionMessageMaxLength + 10) + "  ");
			AssertEquals(ZString.Replicate('m', ExceptionKeyFields.HE_ExceptionMessageMaxLength), Keys.Messages[0]);
			AssertEquals(Keys.Message, Keys.Messages[0]);

			Keys.Message = ("   " + ZString.Replicate('k', ExceptionKeyFields.HE_ExceptionMessageMaxLength + 10) + "  ");
			AssertEquals(ZString.Replicate('k', ExceptionKeyFields.HE_ExceptionMessageMaxLength), Keys.Message);
			AssertEquals(Keys.Messages[0], Keys.Message);

			Keys.Message = "m1";
			Keys.AddMessage("m2");
			Keys.AddMessage("m3");
			AssertEquals("m3", Keys.Message);
			AssertEquals("m1", Keys.Messages[0]);
			AssertEquals("m2", Keys.Messages[1]);
			AssertEquals("m3", Keys.Messages[2]);
		}

		public void TestBuildReadableMessage()
		{
			var log = Factory.New<EdiHelpErrorLog>();
			Keys.Message = null;
			Keys.AddMessage("Error from Data layer: TableName: TestTable PK: ect ect ect");
			Keys.AddMessage("ConcurrencyError");
			Keys.CopyToLog(log);
			AssertEquals("Message", "ConcurrencyError TestTable", log.HE_ExceptionMessage);

			Keys.Message = null;
			Keys.AddMessage("Error from Data layer: TableName:      TestTable PK: ect ect ect");
			Keys.AddMessage("ConcurrencyError");
			Keys.CopyToLog(log);
			AssertEquals("Message", "ConcurrencyError TestTable", log.HE_ExceptionMessage);

			Keys.Message = null;
			Keys.AddMessage("Error from Data layer:\n\rTableName:\rTestTable PK: ect ect ect");
			Keys.AddMessage("ConcurrencyError");
			Keys.CopyToLog(log);
			AssertEquals("Message", "ConcurrencyError TestTable", log.HE_ExceptionMessage);

			Keys.Message = null;
			Keys.AddMessage("Error from Data layer: TestTable PK: ect ect ect");
			Keys.AddMessage(" ConcurrencyError ");
			Keys.CopyToLog(log);
			AssertEquals("Message", "ConcurrencyError", log.HE_ExceptionMessage);

			Keys.Message = null;
			Keys.AddMessage("Error from Data layer: TableName: TestTable PK: ect ect ect");
			Keys.AddMessage("!~ConcurrencyError~!");
			Keys.CopyToLog(log);
			AssertEquals("Message", "ConcurrencyError TestTable", log.HE_ExceptionMessage);
		}

		protected ExceptionKeyFields Keys;

		protected override void SetUp()
		{
			base.SetUp();
			Keys = new ExceptionKeyFields();
		}
	}

	class TestWithValues : ExceptionKeyFieldsTest
	{
		public override void TestCopyToLog()
		{
			var log = Factory.New<EdiHelpErrorLog>();
			Keys.CopyToLog(log);
			AssertEquals(Keys.Type, log.HE_ExceptionType);
			AssertEquals(Keys.Source, log.HE_ExceptionSource);
			AssertEquals(Keys.Message, log.HE_ExceptionMessage);
			AssertEquals(GetKeyOfSetUpLog(), log.Keys[0].HK_Key);
			AssertEquals(HelpErrorLogKey.GetKeyHashCode(GetKeyOfSetUpLog()), log.Keys[0].HK_HashCode);
		}

		public override void TestMatchingLog()
		{
			var logs = new ScalableHelpErrorLogCollection(Factory);
			AssertNull(Keys.MatchingLog(logs));

			logs = new ScalableHelpErrorLogCollection(Factory);
			Factory.Save();
			AssertNull(Keys.MatchingLog(logs));

			logs = new ScalableHelpErrorLogCollection(Factory);
			var log = Factory.New<EdiHelpErrorLog>();
			Keys.CopyToLog(log);
			Factory.Save();
			AssertEquals(log, Keys.MatchingLog(logs));

			logs = new ScalableHelpErrorLogCollection(Factory);
			log.Keys[0].HK_HashCode = HelpErrorLogKey.GetKeyHashCode(GetKeyOfSetUpLog());
			Factory.Save();
			AssertEquals("Matching HK_HashCode (and Key)", log, Keys.MatchingLog(logs));

			logs = new ScalableHelpErrorLogCollection(Factory);
			log.Keys[0].HK_HashCode = HelpErrorLogKey.GetKeyHashCode("different" + GetKeyOfSetUpLog());
			Factory.Save();
			AssertNull("Non matching HK_HashCode", Keys.MatchingLog(logs));

			logs = new ScalableHelpErrorLogCollection(Factory);
			log.Keys[0].HK_HashCode = HelpErrorLogKey.GetKeyHashCode(GetKeyOfSetUpLog());
			log.HE_IsClientVisible = ZBool.True;
			Factory.Save();
			AssertEquals("Still a match because client visibility is ignored", log, Keys.MatchingLog(logs));
		}

		protected virtual string GetKeyOfSetUpLog()
		{
			return Keys.CallStack.Trim();
		}

		protected override void SetUp()
		{
			base.SetUp();
			Keys.Type = " type ";
			Keys.Source = " source ";
			Keys.Message = " message ";
			Keys.CallStack = " stack ";
			Keys.IsClientVisible = ZBool.False;
		}
	}

	class TestWithValuesIncludingKey : TestWithValues
	{
		public void TestCopyToLogWithALongKey()
		{
			var log = Factory.New<EdiHelpErrorLog>();
			Keys.Key = new ZString('K', 600);
			Keys.CallStack = new ZString('C', 5000);
			string expectedKey = Keys.Key;

			Keys.CopyToLog(log);
			Factory.Save();

			AssertEquals(Keys.Type, log.HE_ExceptionType);
			AssertEquals(Keys.Source, log.HE_ExceptionSource);
			AssertEquals(Keys.Message, log.HE_ExceptionMessage);
			AssertEquals(expectedKey, log.Keys[0].HK_Key);
		}

		public void TestHResultIsAddedToLogKey()
		{
			Keys.Key = new ZString('K', 16);
			Keys.CallStack = new ZString('C', 50);

			AssertEquals(Keys.Key, Keys.LogKey);

			Keys.Key = "";
			AssertEquals(Keys.CallStack, Keys.LogKey);

			Keys.Message = "dsfhsdf HResult: 0x23432 dsfsdf";
			AssertEquals(Keys.CallStack + " (HResult: 0x23432)", Keys.LogKey);

			Keys.Message = "dsfhsdf (HResult: 0x23432) dsfsdf";
			AssertEquals(Keys.CallStack + " (HResult: 0x23432)", Keys.LogKey);

			Keys.Key = new ZString('K', 16);

			Keys.Message = "dsfhsdf HResult: 0x23432 dsfsdf";
			AssertEquals(Keys.Key + " (HResult: 0x23432)", Keys.LogKey);

			Keys.Message = "dsfhsdf (HResult: 0x23432) dsfsdf";
			AssertEquals(Keys.Key + " (HResult: 0x23432)", Keys.LogKey);
		}

		public void TestLogKeyIsReplacedFromRegistryList()
		{
			Keys.Key = "KEYBLAH1234";

			var collection = new ExceptionKeyRegexCollection();
			collection.Add(new ExceptionKeyRegex() { Regex = @"ABCD", Description = "" });
			collection.Add(new ExceptionKeyRegex() { Regex = @"KEYBLAH", Description = "" });
			EDIDataRegistry.Instance.ExceptionKeyMatchingRegexes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			AssertEquals("KEYBLAH", Keys.LogKey);

			EDIDataRegistry.Instance.ExceptionKeyMatchingRegexes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ExceptionKeyRegexCollection());
		}

		public void TestExceptionKeyStacktraceIsTrimmedWhenMappedInRegistry()
		{
			var collection = new ExceptionKeyStacktraceDepthCollection
			{
				new ExceptionKeyStacktraceDepth { ExceptionType = "System.DontCareAboutFullStackException", StackDepth = 3 }
			};
			EDIDataRegistry.Instance.ExceptionKeyStacktraceDepths.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			Keys.Type = "System.DontCareAboutFullStackException";
			Keys.CallStack = @"at MaiMethodOne()
at MaiMethodTwo()
at MaiMethodThree()
at MaiMethodFour()";

			AssertEquals(@"at MaiMethodOne()
at MaiMethodTwo()
at MaiMethodThree()", Keys.CallStack);

			Keys.Type = "System.IDoCareAboutFullStackException";
			Keys.CallStack = @"at MaiMethodOne()
at MaiMethodTwo()
at MaiMethodThree()
at MaiMethodFour()";

			AssertEquals(@"at MaiMethodOne()
at MaiMethodTwo()
at MaiMethodThree()
at MaiMethodFour()", Keys.CallStack);
		}

		protected override string GetKeyOfSetUpLog()
		{
			return Keys.Key.Trim();
		}

		protected override void SetUp()
		{
			base.SetUp();
			Keys.Key = " my key ";
		}
	}
}
