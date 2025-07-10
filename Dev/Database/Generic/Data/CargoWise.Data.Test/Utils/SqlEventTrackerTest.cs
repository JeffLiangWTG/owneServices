using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using CargoWise.Common;
using CargoWise.Data.Diagnostics;
using NUnit.Framework;
using SqlBulkCopyOptions = CargoWise.Data.Providers.Common.SqlBulkCopyOptions;

namespace CargoWise.Data.Testing
{
	sealed class SqlEventTrackerTest : TestCase
	{
		[DeveloperOnlyTest]
		public void TestRemoveNullsTiming()
		{
			//this test basically exists to prove that we don't need to strip nulls at any stage (if we start off with WriteElementString), so we can remove RemoveNulls from this test

			//simulating construction of SqlEvents with nulls
			StringWriter strWriter = new StringWriter(CultureInfo.InvariantCulture);
			XmlTextWriter xtw = new CustomXmlWriter(strWriter);
			xtw.WriteStartElement("SqlEvents");
			xtw.WriteElementString("Command", "'&#x0;&#0;' & '\\0 \0 \\u0000 \u0000 0x00 xsi:nil'");
			xtw.WriteEndElement();
			xtw.Flush();
			var result = strWriter.GetStringBuilder().ToString();
			AssertEquals("<SqlEvents><Command>'&amp;#x0;&amp;#0;' &amp; '\\0 &#x0; \\u0000 &#x0; 0x00 xsi:nil'</Command></SqlEvents>", result);

			//simulating construction of issue report xml
			StringWriter strWriter2 = new StringWriter(CultureInfo.InvariantCulture);
			XmlTextWriter xtw2 = new CustomXmlWriter(strWriter2);
			xtw2.WriteStartElement("EDI_Exception_Report");
			xtw2.WriteRaw(result);
			xtw2.WriteEndElement();
			xtw2.Flush();
			//var result2 = SqlEventTracker.RemoveNulls(strWriter2.GetStringBuilder());
			var result2 = strWriter2.GetStringBuilder().ToString();
			AssertEquals("<EDI_Exception_Report><SqlEvents><Command>'&amp;#x0;&amp;#0;' &amp; '\\0 &#x0; \\u0000 &#x0; 0x00 xsi:nil'</Command></SqlEvents></EDI_Exception_Report>", result2);

			//simulating ExceptionReportRenderer
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(result2);
			void SanitizeAllNodes(XmlNode xmlNode)
			{
				foreach (XmlNode node in xmlNode.ChildNodes)
				{
					if (node is XmlText)
					{
						node.InnerText = System.Net.WebUtility.HtmlEncode(node.InnerText);
					}
					else
					{
						SanitizeAllNodes(node);
					}
				}
			}
			SanitizeAllNodes(xmlDoc);
			var sqlEvents = xmlDoc.DocumentElement["SqlEvents"];
			AssertEquals(@"<Command>&amp;#39;&amp;amp;#x0;&amp;amp;#0;&amp;#39; &amp;amp; &amp;#39;\0 &#x0; \u0000 &#x0; 0x00 xsi:nil&amp;#39;</Command>", sqlEvents.InnerXml);
		}

		[DeveloperOnlyTest]
		public void TestRemoveNullsTiming_2()
		{
			//now testing how many different kinds of nulls cause a problem when we finally open it on the issue manager side, if we sneak them in using WriteRaw or some glitch.

			//this is the bare minimum RemoveNulls needs. (also technically raw & but that's trickier...)
			string RemoveNulls(StringBuilder input)
			{
				input = input.Replace("\0", "\\0");
				return input.ToString();
			}

			//simulating construction of SqlEvents with nulls
			StringWriter strWriter = new StringWriter(CultureInfo.InvariantCulture);
			XmlTextWriter xtw = new CustomXmlWriter(strWriter);
			xtw.WriteStartElement("SqlEvents");
			xtw.WriteRaw("<Command>'&#x0;&#0;' &amp; '\\0 \0 \\u0000 \u0000 0x00 xsi:nil'</Command>");
			xtw.WriteEndElement();
			xtw.Flush();
			var result = strWriter.GetStringBuilder().ToString();

			//simulating construction of issue report xml
			StringWriter strWriter2 = new StringWriter(CultureInfo.InvariantCulture);
			XmlTextWriter xtw2 = new CustomXmlWriter(strWriter2);
			xtw2.WriteStartElement("EDI_Exception_Report");
			xtw2.WriteRaw(result);
			xtw2.WriteEndElement();
			xtw2.Flush();
			var result2 = RemoveNulls(strWriter2.GetStringBuilder());

			//simulating ExceptionReportRenderer
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(result2);
			void SanitizeAllNodes(XmlNode xmlNode)
			{
				foreach (XmlNode node in xmlNode.ChildNodes)
				{
					if (node is XmlText)
					{
						node.InnerText = System.Net.WebUtility.HtmlEncode(node.InnerText);
					}
					else
					{
						SanitizeAllNodes(node);
					}
				}
			}
			SanitizeAllNodes(xmlDoc);
			var sqlEvents = xmlDoc.DocumentElement["SqlEvents"];
			CombineAssertions(() =>
			{
				AssertEquals("<SqlEvents><Command>'&#x0;&#0;' &amp; '\\0 \0 \\u0000 \0 0x00 xsi:nil'</Command></SqlEvents>", result);
				AssertEquals("<EDI_Exception_Report><SqlEvents><Command>'&#x0;&#0;' &amp; '\\0 \\0 \\u0000 \\0 0x00 xsi:nil'</Command></SqlEvents></EDI_Exception_Report>", result2);
				AssertEquals("<Command>&amp;#39;&#x0;&#x0;&amp;#39; &amp;amp; &amp;#39;\\0 \\0 \\u0000 \\0 0x00 xsi:nil&amp;#39;</Command>", sqlEvents.InnerXml);
			});
		}

		public void TestAddSqlServerBulkCopyEvent()
		{
			var sqlBulkCopy = Db.Connection.GetSqlBulkCopy(SqlBulkCopyOptions.TableLock);
			sqlBulkCopy.DestinationTableName = "HappyTable";
			var threadID = Thread.CurrentThread.ManagedThreadId;
			SqlEventTracker.Instance.Clear();
			SqlEventTracker.Instance.AddSqlEvent(sqlBulkCopy, new Exception("something happened"), TimeSpan.FromSeconds(56));
			AssertEquals(true, SqlEventTracker.Instance.HasQueries);
			var eventText = SqlEventTracker.Instance.SqlEventList.Single();
			AssertContains(@"[BULKCOPY FAILED: 
something happened]

================     STACK TRACE      ================", eventText);
			AssertContains(@"================  END OF STACK TRACE  ================
insert bulk HappyTable with (TableLock)", eventText);
			AssertContains($@"   [Duration=56000ms]
   [Transaction=false]
   [Current thread ID={threadID}]", eventText);
		}

		public void TestEntireStackTraceComesOut()
		{
			var sqlBulkCopy = Db.Connection.GetSqlBulkCopy(SqlBulkCopyOptions.TableLock);
			Exception testException;

			try
			{
				try
				{
					throw new Exception("Inner Exception");
				}
				catch (Exception e)
				{
					throw new Exception("Outer Exception", e);
				}
			}
			catch (Exception e)
			{
				testException = e;
			}

			SqlEventTracker.Instance.Clear();
			SqlEventTracker.Instance.AddSqlEvent(sqlBulkCopy, testException, TimeSpan.FromSeconds(56));
			var eventText = SqlEventTracker.Instance.SqlEventList.Single();
			CombineAssertions("Contains all parts of the stack trace", () =>
			{
				AssertContains("Header", QueryStackTraceRecorderCore.StackTraceHeader, eventText);
				AssertContains("Inner stack trace", testException.InnerException.StackTrace, eventText);
				AssertContains("Inner stack trace footer", "--- End of inner exception stack trace ---", eventText);
				AssertGreaterThan("Main stack trace for calling this UT is not exception lines", eventText.SplitByLine().Count(), new System.Diagnostics.StackTrace().ToString().SplitByLine().Count());
				AssertContains("Footer", QueryStackTraceRecorderCore.StackTraceFooter, eventText);
			});
		}

		public void TestRethrow()
		{
			var sqlBulkCopy = Db.Connection.GetSqlBulkCopy(SqlBulkCopyOptions.TableLock);
			Exception testException = null;

			try
			{
				DummyMethodThatReThrowsException();
			}
			catch (Exception e)
			{
				testException = e;
			}

			SqlEventTracker.Instance.Clear();
			SqlEventTracker.Instance.AddSqlEvent(sqlBulkCopy, testException, TimeSpan.FromSeconds(56));
			var eventText = SqlEventTracker.Instance.SqlEventList.Single();
			CombineAssertions("Contains the root exception and subsequent rethrows", () =>
			{
				AssertContains("Root exception", "DummyMethodThatThrowsException", eventText);
				AssertContains("Rethrow", "DummyMethodThatReThrowsException", eventText);
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
		public void TestEntireStackTraceComesOutOnOverloadedMethod()
		{
			Exception testException;

			try
			{
				try
				{
					throw new Exception("Inner Exception");
				}
				catch (Exception e)
				{
					throw new Exception("Outer Exception", e);
				}
			}
			catch (Exception e)
			{
				testException = e;
			}

			SqlEventTracker.Instance.Clear();
			SqlEventTracker.Instance.AddSqlEvent(new SqlCommand(), testException, TimeSpan.FromSeconds(56));
			var eventText = SqlEventTracker.Instance.SqlEventList.Single();
			CombineAssertions("Contains all parts of the stack trace", () =>
			{
				AssertContains("Header", QueryStackTraceRecorderCore.StackTraceHeader, eventText);
				AssertContains("Inner stack trace", testException.InnerException.StackTrace, eventText);
				AssertContains("Inner stack trace footer", "--- End of inner exception stack trace ---", eventText);
				AssertGreaterThan("Main stack trace for calling this UT is not exception lines", eventText.SplitByLine().Count(), new System.Diagnostics.StackTrace().ToString().SplitByLine().Count());
				AssertContains("Footer", QueryStackTraceRecorderCore.StackTraceFooter, eventText);
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
		public void TestReThrowWithOverloadedMethod()
		{
			Exception testException = null;

			try
			{
				DummyMethodThatReThrowsException();
			}
			catch (Exception e)
			{
				testException = e;
			}

			SqlEventTracker.Instance.Clear();
			SqlEventTracker.Instance.AddSqlEvent(new SqlCommand(), testException, TimeSpan.FromSeconds(56));
			var eventText = SqlEventTracker.Instance.SqlEventList.Single();
			CombineAssertions("Contains the root exception and subsequent rethrows", () =>
			{
				AssertContains("Root exception", "DummyMethodThatThrowsException", eventText);
				AssertContains("Rethrow", "DummyMethodThatReThrowsException", eventText);
			});
		}

		void DummyMethodThatReThrowsException()
		{
			try
			{
				DummyMethodThatThrowsException();
			}
			catch (Exception)
			{
				throw;
			}
		}
		void DummyMethodThatThrowsException()
		{
			throw new Exception("Hi");
		}

		public void TestGetInstance()
		{
			AssertNotNull("Valid Instance should be returned", SqlEventTracker.Instance);
			AssertEquals("Is enabled by default", true, SqlEventTracker.Instance.IsEnabled);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
		public void TestClear()
		{
			SqlEventTracker.Instance.Clear();

			SqlEventTracker.Instance.AddSqlEvent(new SqlCommand(), null, TimeSpan.FromMilliseconds(123)); // Internally the function takes an SQLCommand. It must take an SQL Command here in the test as well.

			AssertEquals("PreCondition: Event Added", true, SqlEventTracker.Instance.HasQueries);

			SqlEventTracker.Instance.Clear();
			AssertEquals("Events Cleared", false, SqlEventTracker.Instance.HasQueries);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
		public void TestSQLEventTrapping()
		{
			SqlCommand sQL = new SqlCommand("Whatever happens this is not its fault."); // Internally the function takes an SQLCommand. It must take an SQL Command here in the test as well.
			SqlEventTracker.Instance.Clear();
			AssertEquals("There should be no items in the list:" + SqlEventTracker.Instance.SqlEventDescription, false, SqlEventTracker.Instance.HasQueries);
			SqlEventTracker.Instance.AddSqlEvent(sQL, null, TimeSpan.FromMilliseconds(123));
			AssertEquals("There should be 1 item in the list:" + SqlEventTracker.Instance.SqlEventDescription, true, SqlEventTracker.Instance.HasQueries);
			Assert("A string should be returned", SqlEventTracker.Instance.SqlEventDescription.Length > 30);
			SqlEventTracker.Instance.Clear();
			AssertEquals("There should be no items in the list:" + SqlEventTracker.Instance.SqlEventDescription, false, SqlEventTracker.Instance.HasQueries);
		}

		public void TestSqlEventWithErrorsTrapping()
		{
			var failingCommandText = "INSERT AnyTable (Col1, Col2) VALUES (0)";
			var testException = ClearSqlEventListAndRunCommand(failingCommandText);

			if (testException == null)
			{
				Fail("[" + failingCommandText + "] command should have failed");
			}
			else
			{
				// If there was a DB disconnection, the reconnection code will issue some extra commands 
				// (like re-creating the temp #User table). Start over in this case.
				testException = ClearSqlEventListAndRunCommand(failingCommandText);
				var sqlEventString = SqlEventTracker.Instance.SqlEventDescription;
				var sqlFailedEventString = SqlEventTracker.Instance.SqlEventDescription;

				AssertEquals("There should be 1 item in the list:" + sqlEventString, true, SqlEventTracker.Instance.HasQueries);
				Assert("COMMAND FAILED should be part of SqlEventDescription:" + sqlEventString,
					SqlEventTracker.Instance.SqlEventDescription.IndexOf("COMMAND FAILED") > -1);
				Assert("Error Message [" + testException.Message + "] should be part of SqlEventDescription:" + sqlEventString,
					SqlEventTracker.Instance.SqlEventDescription.IndexOf(testException.Message) > -1);
				Assert("Command Text [" + failingCommandText + "] should be part of SqlEventDescription:" + sqlEventString,
					SqlEventTracker.Instance.SqlEventDescription.IndexOf(testException.Message) > -1);
				Assert("STACK TRACE should be part of SqlEventDescription:" + sqlEventString,
					SqlEventTracker.Instance.SqlEventDescription.IndexOf("STACK TRACE") > -1);
				Assert("END OF STACK TRACE should be part of SqlEventDescription:" + sqlEventString,
					SqlEventTracker.Instance.SqlEventDescription.IndexOf("END OF STACK TRACE") > -1);

				Assert("COMMAND FAILED should be part of SqlEventDescription:" + sqlFailedEventString,
					SqlEventTracker.Instance.SqlFailedEventDescription.IndexOf("COMMAND FAILED") > -1);
				Assert("Error Message [" + testException.Message + "] should be part of SqlEventDescription:" + sqlFailedEventString,
					SqlEventTracker.Instance.SqlFailedEventDescription.IndexOf(testException.Message) > -1);
				Assert("Command Text [" + failingCommandText + "] should be part of SqlEventDescription:" + sqlFailedEventString,
					SqlEventTracker.Instance.SqlFailedEventDescription.IndexOf(testException.Message) > -1);
				Assert("STACK TRACE should be part of SqlEventDescription:" + sqlEventString,
					SqlEventTracker.Instance.SqlEventDescription.IndexOf("STACK TRACE") > -1);
				Assert("END OF STACK TRACE should be part of SqlEventDescription:" + sqlEventString,
					SqlEventTracker.Instance.SqlEventDescription.IndexOf("END OF STACK TRACE") > -1);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
		[ExpectNoExceptions]
		public void TestSQLEventDescription()
		{
			SqlCommand sQL = new SqlCommand("Whatever happens this is not its fault."); // Internally the function takes an SQLCommand. It must take an SQL Command here in the test as well.
			SqlEventTracker.Instance.Clear();
			SqlEventTracker.Instance.AddSqlEvent(sQL, null, TimeSpan.FromMilliseconds(123));
			string sQLEvents = SqlEventTracker.Instance.SqlEventDescription;
			// This simply validates well formed XML is produced by method
			XmlDocument testDoc = new XmlDocument();
			testDoc.LoadXml(sQLEvents);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
		[ExpectNoExceptions]
		public void TestAddSqlEventDoesntThrowExceptionOnNullParamValue()
		{
			SqlCommand sQL = new SqlCommand("Whatever happens this is not its fault."); // Internally the function takes an SQLCommand. It must take an SQL Command here in the test as well.
			sQL.Parameters.AddWithValue("LALALA", null);
			SqlEventTracker.Instance.AddSqlEvent(sQL, null, TimeSpan.FromMilliseconds(123));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
		public void TestNullDateParameterIsShownAppropriately()
		{
			SqlCommand sQL = new SqlCommand("Whatever happens this is not its fault."); // Internally the function takes an SQLCommand. It must take an SQL Command here in the test as well.
			sQL.Parameters.AddWithValue("LALALA", DBNull.Value);
			SqlEventTracker.Instance.AddSqlEvent(sQL, null, TimeSpan.FromMilliseconds(123));
			Assert("Parameter value", SqlEventTracker.Instance.LastSqlEvent.Contains("LALALA = \"NULL\""));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
		public void TestAddSqlCommandEvent()
		{
			var commandArgsList = new List<SqlCommandExecutedEventArgs>();
			SqlEventTracker.Instance.SqlCommandExecutedEvent += (commandArgsList.Add);

			var sql = new SqlCommand("Whatever happens this is not its fault."); // Internally the function takes an SQLCommand. It must take an SQL Command here in the test as well.
			sql.Parameters.AddWithValue("LALALA", DBNull.Value);
			SqlEventTracker.Instance.AddSqlEvent(sql, null, TimeSpan.FromMilliseconds(123));

			AssertEquals(1, commandArgsList.Count);
			Assert(commandArgsList[0].Text.Contains(sql.CommandText));
			AssertEquals(null, commandArgsList[0].Exception);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
		public void TestAddParametersString()
		{
			var commandArgsList = new List<SqlCommandExecutedEventArgs>();
			SqlEventTracker.Instance.SqlCommandExecutedEvent += (commandArgsList.Add);

			var originalCmd = new SqlCommand("testproc") { CommandType = CommandType.StoredProcedure }; // Internally the function takes an SQLCommand. It must take an SQL Command here in the test as well.
			var parameter = originalCmd.Parameters.Add("@Code", SqlDbType.VarChar);
			parameter.Value = "test parameter";
			SqlEventTracker.Instance.AddSqlEvent(originalCmd, null, TimeSpan.FromMilliseconds(123));

			AssertEquals(1, commandArgsList.Count);
			AssertEquals(true, commandArgsList[0].Text.Contains("DECLARE @Code AS VarChar (14) = 'test parameter'\r\nexec testproc @Code"));

			commandArgsList.Clear();
			originalCmd = new SqlCommand
			{
				CommandText = "select A from StemNote where xxx = @Value",
				CommandType = CommandType.Text
			};
			parameter = originalCmd.Parameters.Add("@Value", SqlDbType.VarChar);
			parameter.Value = "test parameter";
			SqlEventTracker.Instance.AddSqlEvent(originalCmd, null, TimeSpan.FromMilliseconds(123));

			Assert(commandArgsList[0].Text.Contains("DECLARE @Value AS VarChar (14) = 'test parameter'\r\nselect A from StemNote where xxx = @Value"));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
		public void TestVeryLongParameterIsTruncated()
		{
			var commandArgsList = new List<SqlCommandExecutedEventArgs>();
			SqlEventTracker.Instance.SqlCommandExecutedEvent += (commandArgsList.Add);

			var originalCmd = new SqlCommand("testproc") { CommandType = CommandType.StoredProcedure }; // Internally the function takes an SQLCommand. It must take an SQL Command here in the test as well.
			var parameter = originalCmd.Parameters.Add("@Code", SqlDbType.VarChar);
			parameter.Value = new string('A', 20000);
			SqlEventTracker.Instance.AddSqlEvent(originalCmd, null, TimeSpan.FromMilliseconds(123));

			AssertEquals(true, SqlEventTracker.Instance.LastSqlEvent.Contains("A... (17952 bytes skipped)"));
		}

		[UseSnapshotProtection]
		public void TestProblemPartOfFailedStatementIsNotTruncated()
		{
			var exception = ClearSqlEventListAndRunCommand(longStatementWithConstraintViolation);
			if (exception == null)
			{
				Fail("Should Throw SqlException");
			}
			AssertEquals("Expected Exception Message", "{eed879c8-5383-465e-b59e-c3b00cc10f98,False,2627} Violation of PRIMARY KEY constraint 'PK_UX__Z0_PK'. Cannot insert duplicate key in object 'dbo.DummyBizo'. The duplicate key value is (eed879c8-5383-465e-b59e-c3b00cc10f90).", exception.Message);
			AssertEquals("Truncated sql contains part that caused exception", true, SqlEventTracker.Instance.LastSqlEvent.Contains(expectedResultAfterTruncation));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
		public void TestFailedSqlWithoutPkInExceptionIsTruncatedAsNormal()
		{
			var command = new SqlCommand
			{
				CommandText = longStatementWithConstraintViolation,
				CommandType = CommandType.Text
			};
			SqlEventTracker.Instance.AddSqlEvent(command, new Exception("Sql Exception that does not contain PK so truncation should begin at start of statement"), TimeSpan.FromMilliseconds(123));
			AssertEquals(true, SqlEventTracker.Instance.LastSqlEvent.Contains(longStatementWithConstraintViolation.Substring(0, 100)));
		}

		public void TestInvalidHighSurrogateString()
		{
			var sqlText = "SELECT \uDDDE";
			try
			{
				Db.Connection.ExecuteScalar(sqlText);
			}
			catch (SqlException)
			{
			}

			AssertNoExceptionThrown(() =>
			{
				var description = SqlEventTracker.Instance.SqlFailedEventDescription;
				AssertContains("?", description);
			});
		}

		#region Multithreading

		[ExpectNoExceptions]
		public void TestMultithreading()
		{
			Thread sqlAdder = new Thread(new ThreadStart(AddSqlThings));
			Thread sqlGetter = new Thread(new ThreadStart(GetSqlThings));

			try
			{
				sqlGetter.Start();
				sqlAdder.Start();

				sqlGetter.Join();
				sqlAdder.Join();

				Assert("Everything is fine", true);
			}
			finally
			{
				sqlGetter.Join();
				sqlAdder.Join();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
		void AddSqlThings()
		{
			for (int i = 0; i < 1000; i++)
			{
				SqlEventTracker.Instance.AddSqlEvent(new SqlCommand(), new Exception(string.Format(CultureInfo.InvariantCulture, "test message{0}", i)), TimeSpan.FromMilliseconds(123)); // This is used for testing only. We need to provide a not null value for this parameter
			}
		}

		void GetSqlThings()
		{
			string description;
			for (int i = 0; i < 1000; i++)
			{
				description = SqlEventTracker.Instance.SqlEventDescription;
			}
		}

		#endregion

		Exception ClearSqlEventListAndRunCommand(string sqlText)
		{
			try
			{
				SqlEventTracker.Instance.Clear();
				Db.Connection.ExecuteNonQuery(sqlText);
			}
			catch (Exception e)
			{
				return e;
			}

			return null;
		}
		const string longStatementWithConstraintViolation = @"DECLARE @PK varchar(38), @row_count int;
BEGIN TRY

SET @PK = 'eed879c8-5383-465e-b59e-c3b00cc10f90';
DECLARE @1_117 geography = geography::STGeomFromText('POINT EMPTY', 4326);
EXEC sys.sp_executesql N'INSERT dbo.DummyBizo (Z0_PK, Z0_AnotherDate, Z0_AnotherDecimal, Z0_AnotherNumber, Z0_BitFalse, Z0_BitFiltered, Z0_BitTrue, Z0_Bool, Z0_Byte, Z0_Code, Z0_Date, Z0_DateOnly, Z0_DateTimeOffset, Z0_Decimal, Z0_Description, Z0_FK_Code, Z0_Geography, Z0_Guid, Z0_IsSystem, Z0_IsValid, Z0_Money, Z0_Number, Z0_NVarChar, Z0_NVarCharMax, Z0_Short, Z0_SmallDateTime, Z0_VarBinaryMax, Z0_VarCharMax, Z0_Xml) VALUES
	(@11, @12, @13, @14, @15, @16, @17, @18, @19, @110, @111, @112, @113, @114, @115, @116, @117, @118, @119, @120, @121, @122, @123, @124, @125, @126, @127, @128, @129)
'
, N'@11 uniqueidentifier, @12 datetime, @13 decimal(18,3), @14 int, @15 bit, @16 bit, @17 bit, @18 char(1), @19 tinyint, @110 varchar(5), @111 datetime, @112 date, @113 datetimeoffset(7), @114 decimal(18,0), @115 varchar(100), @116 varchar(5), @117 geography, @118 uniqueidentifier, @119 bit, @120 char(1), @121 money, @122 int, @123 nvarchar(20), @124 nvarchar(max), @125 smallint, @126 smalldatetime, @127 varbinary(max), @128 varchar(max), @129 xml'
, @11 = 'eed879c8-5383-465e-b59e-c3b00cc10f90', @12 = NULL, @13 = 0, @14 = 0, @15 = 0, @16 = 0, @17 = 1, @18 = 'N', @19 = 0, @110 = 'NCODE1', @111 = NULL, @112 = NULL, @113 = NULL, @114 = 0, @115 = 'Default', @116 = '', @117 = @1_117, @118 = NULL, @119 = 1, @120 = 'N', @121 = 0, @122 = 0, @123 = N'', @124 = N'', @125 = 0, @126 = NULL, @127 = NULL, @128 = '', @129 = N'';

SET @PK = 'eed879c8-5383-465e-b59e-c3b00cc10f91';
DECLARE @2_117 geography = geography::STGeomFromText('POINT EMPTY', 4326);
EXEC sys.sp_executesql N'INSERT dbo.DummyBizo (Z0_PK, Z0_AnotherDate, Z0_AnotherDecimal, Z0_AnotherNumber, Z0_BitFalse, Z0_BitFiltered, Z0_BitTrue, Z0_Bool, Z0_Byte, Z0_Code, Z0_Date, Z0_DateOnly, Z0_DateTimeOffset, Z0_Decimal, Z0_Description, Z0_FK_Code, Z0_Geography, Z0_Guid, Z0_IsSystem, Z0_IsValid, Z0_Money, Z0_Number, Z0_NVarChar, Z0_NVarCharMax, Z0_Short, Z0_SmallDateTime, Z0_VarBinaryMax, Z0_VarCharMax, Z0_Xml) VALUES
	(@11, @12, @13, @14, @15, @16, @17, @18, @19, @110, @111, @112, @113, @114, @115, @116, @117, @118, @119, @120, @121, @122, @123, @124, @125, @126, @127, @128, @129)
'
, N'@11 uniqueidentifier, @12 datetime, @13 decimal(18,3), @14 int, @15 bit, @16 bit, @17 bit, @18 char(1), @19 tinyint, @110 varchar(5), @111 datetime, @112 date, @113 datetimeoffset(7), @114 decimal(18,0), @115 varchar(100), @116 varchar(5), @117 geography, @118 uniqueidentifier, @119 bit, @120 char(1), @121 money, @122 int, @123 nvarchar(20), @124 nvarchar(max), @125 smallint, @126 smalldatetime, @127 varbinary(max), @128 varchar(max), @129 xml'
, @11 = 'eed879c8-5383-465e-b59e-c3b00cc10f91', @12 = NULL, @13 = 0, @14 = 0, @15 = 0, @16 = 0, @17 = 1, @18 = 'N', @19 = 0, @110 = 'NCODE1', @111 = NULL, @112 = NULL, @113 = NULL, @114 = 0, @115 = 'Default', @116 = '', @117 = @1_117, @118 = NULL, @119 = 1, @120 = 'N', @121 = 0, @122 = 0, @123 = N'', @124 = N'', @125 = 0, @126 = NULL, @127 = NULL, @128 = '', @129 = N'';

SET @PK = 'eed879c8-5383-465e-b59e-c3b00cc10f92';
DECLARE @3_117 geography = geography::STGeomFromText('POINT EMPTY', 4326);
EXEC sys.sp_executesql N'INSERT dbo.DummyBizo (Z0_PK, Z0_AnotherDate, Z0_AnotherDecimal, Z0_AnotherNumber, Z0_BitFalse, Z0_BitFiltered, Z0_BitTrue, Z0_Bool, Z0_Byte, Z0_Code, Z0_Date, Z0_DateOnly, Z0_DateTimeOffset, Z0_Decimal, Z0_Description, Z0_FK_Code, Z0_Geography, Z0_Guid, Z0_IsSystem, Z0_IsValid, Z0_Money, Z0_Number, Z0_NVarChar, Z0_NVarCharMax, Z0_Short, Z0_SmallDateTime, Z0_VarBinaryMax, Z0_VarCharMax, Z0_Xml) VALUES
	(@11, @12, @13, @14, @15, @16, @17, @18, @19, @110, @111, @112, @113, @114, @115, @116, @117, @118, @119, @120, @121, @122, @123, @124, @125, @126, @127, @128, @129)
'
, N'@11 uniqueidentifier, @12 datetime, @13 decimal(18,3), @14 int, @15 bit, @16 bit, @17 bit, @18 char(1), @19 tinyint, @110 varchar(5), @111 datetime, @112 date, @113 datetimeoffset(7), @114 decimal(18,0), @115 varchar(100), @116 varchar(5), @117 geography, @118 uniqueidentifier, @119 bit, @120 char(1), @121 money, @122 int, @123 nvarchar(20), @124 nvarchar(max), @125 smallint, @126 smalldatetime, @127 varbinary(max), @128 varchar(max), @129 xml'
, @11 = 'eed879c8-5383-465e-b59e-c3b00cc10f92', @12 = NULL, @13 = 0, @14 = 0, @15 = 0, @16 = 0, @17 = 1, @18 = 'N', @19 = 0, @110 = 'NCODE1', @111 = NULL, @112 = NULL, @113 = NULL, @114 = 0, @115 = 'Default', @116 = '', @117 = @1_117, @118 = NULL, @119 = 1, @120 = 'N', @121 = 0, @122 = 0, @123 = N'', @124 = N'', @125 = 0, @126 = NULL, @127 = NULL, @128 = '', @129 = N'';

SET @PK = 'eed879c8-5383-465e-b59e-c3b00cc10f93';
DECLARE @4_117 geography = geography::STGeomFromText('POINT EMPTY', 4326);
EXEC sys.sp_executesql N'INSERT dbo.DummyBizo (Z0_PK, Z0_AnotherDate, Z0_AnotherDecimal, Z0_AnotherNumber, Z0_BitFalse, Z0_BitFiltered, Z0_BitTrue, Z0_Bool, Z0_Byte, Z0_Code, Z0_Date, Z0_DateOnly, Z0_DateTimeOffset, Z0_Decimal, Z0_Description, Z0_FK_Code, Z0_Geography, Z0_Guid, Z0_IsSystem, Z0_IsValid, Z0_Money, Z0_Number, Z0_NVarChar, Z0_NVarCharMax, Z0_Short, Z0_SmallDateTime, Z0_VarBinaryMax, Z0_VarCharMax, Z0_Xml) VALUES
	(@11, @12, @13, @14, @15, @16, @17, @18, @19, @110, @111, @112, @113, @114, @115, @116, @117, @118, @119, @120, @121, @122, @123, @124, @125, @126, @127, @128, @129)
'
, N'@11 uniqueidentifier, @12 datetime, @13 decimal(18,3), @14 int, @15 bit, @16 bit, @17 bit, @18 char(1), @19 tinyint, @110 varchar(5), @111 datetime, @112 date, @113 datetimeoffset(7), @114 decimal(18,0), @115 varchar(100), @116 varchar(5), @117 geography, @118 uniqueidentifier, @119 bit, @120 char(1), @121 money, @122 int, @123 nvarchar(20), @124 nvarchar(max), @125 smallint, @126 smalldatetime, @127 varbinary(max), @128 varchar(max), @129 xml'
, @11 = 'eed879c8-5383-465e-b59e-c3b00cc10f93', @12 = NULL, @13 = 0, @14 = 0, @15 = 0, @16 = 0, @17 = 1, @18 = 'N', @19 = 0, @110 = 'NCODE1', @111 = NULL, @112 = NULL, @113 = NULL, @114 = 0, @115 = 'Default', @116 = '', @117 = @1_117, @118 = NULL, @119 = 1, @120 = 'N', @121 = 0, @122 = 0, @123 = N'', @124 = N'', @125 = 0, @126 = NULL, @127 = NULL, @128 = '', @129 = N'';

SET @PK = 'eed879c8-5383-465e-b59e-c3b00cc10f94';
DECLARE @5_117 geography = geography::STGeomFromText('POINT EMPTY', 4326);
EXEC sys.sp_executesql N'INSERT dbo.DummyBizo (Z0_PK, Z0_AnotherDate, Z0_AnotherDecimal, Z0_AnotherNumber, Z0_BitFalse, Z0_BitFiltered, Z0_BitTrue, Z0_Bool, Z0_Byte, Z0_Code, Z0_Date, Z0_DateOnly, Z0_DateTimeOffset, Z0_Decimal, Z0_Description, Z0_FK_Code, Z0_Geography, Z0_Guid, Z0_IsSystem, Z0_IsValid, Z0_Money, Z0_Number, Z0_NVarChar, Z0_NVarCharMax, Z0_Short, Z0_SmallDateTime, Z0_VarBinaryMax, Z0_VarCharMax, Z0_Xml) VALUES
	(@11, @12, @13, @14, @15, @16, @17, @18, @19, @110, @111, @112, @113, @114, @115, @116, @117, @118, @119, @120, @121, @122, @123, @124, @125, @126, @127, @128, @129)
'
, N'@11 uniqueidentifier, @12 datetime, @13 decimal(18,3), @14 int, @15 bit, @16 bit, @17 bit, @18 char(1), @19 tinyint, @110 varchar(5), @111 datetime, @112 date, @113 datetimeoffset(7), @114 decimal(18,0), @115 varchar(100), @116 varchar(5), @117 geography, @118 uniqueidentifier, @119 bit, @120 char(1), @121 money, @122 int, @123 nvarchar(20), @124 nvarchar(max), @125 smallint, @126 smalldatetime, @127 varbinary(max), @128 varchar(max), @129 xml'
, @11 = 'eed879c8-5383-465e-b59e-c3b00cc10f94', @12 = NULL, @13 = 0, @14 = 0, @15 = 0, @16 = 0, @17 = 1, @18 = 'N', @19 = 0, @110 = 'NCODE1', @111 = NULL, @112 = NULL, @113 = NULL, @114 = 0, @115 = 'Default', @116 = '', @117 = @1_117, @118 = NULL, @119 = 1, @120 = 'N', @121 = 0, @122 = 0, @123 = N'', @124 = N'', @125 = 0, @126 = NULL, @127 = NULL, @128 = '', @129 = N'';

SET @PK = 'eed879c8-5383-465e-b59e-c3b00cc10f95';
DECLARE @6_117 geography = geography::STGeomFromText('POINT EMPTY', 4326);
EXEC sys.sp_executesql N'INSERT dbo.DummyBizo (Z0_PK, Z0_AnotherDate, Z0_AnotherDecimal, Z0_AnotherNumber, Z0_BitFalse, Z0_BitFiltered, Z0_BitTrue, Z0_Bool, Z0_Byte, Z0_Code, Z0_Date, Z0_DateOnly, Z0_DateTimeOffset, Z0_Decimal, Z0_Description, Z0_FK_Code, Z0_Geography, Z0_Guid, Z0_IsSystem, Z0_IsValid, Z0_Money, Z0_Number, Z0_NVarChar, Z0_NVarCharMax, Z0_Short, Z0_SmallDateTime, Z0_VarBinaryMax, Z0_VarCharMax, Z0_Xml) VALUES
	(@11, @12, @13, @14, @15, @16, @17, @18, @19, @110, @111, @112, @113, @114, @115, @116, @117, @118, @119, @120, @121, @122, @123, @124, @125, @126, @127, @128, @129)
'
, N'@11 uniqueidentifier, @12 datetime, @13 decimal(18,3), @14 int, @15 bit, @16 bit, @17 bit, @18 char(1), @19 tinyint, @110 varchar(5), @111 datetime, @112 date, @113 datetimeoffset(7), @114 decimal(18,0), @115 varchar(100), @116 varchar(5), @117 geography, @118 uniqueidentifier, @119 bit, @120 char(1), @121 money, @122 int, @123 nvarchar(20), @124 nvarchar(max), @125 smallint, @126 smalldatetime, @127 varbinary(max), @128 varchar(max), @129 xml'
, @11 = 'eed879c8-5383-465e-b59e-c3b00cc10f95', @12 = NULL, @13 = 0, @14 = 0, @15 = 0, @16 = 0, @17 = 1, @18 = 'N', @19 = 0, @110 = 'NCODE1', @111 = NULL, @112 = NULL, @113 = NULL, @114 = 0, @115 = 'Default', @116 = '', @117 = @1_117, @118 = NULL, @119 = 1, @120 = 'N', @121 = 0, @122 = 0, @123 = N'', @124 = N'', @125 = 0, @126 = NULL, @127 = NULL, @128 = '', @129 = N'';

SET @PK = 'eed879c8-5383-465e-b59e-c3b00cc10f96';
DECLARE @7_117 geography = geography::STGeomFromText('POINT EMPTY', 4326);
EXEC sys.sp_executesql N'INSERT dbo.DummyBizo (Z0_PK, Z0_AnotherDate, Z0_AnotherDecimal, Z0_AnotherNumber, Z0_BitFalse, Z0_BitFiltered, Z0_BitTrue, Z0_Bool, Z0_Byte, Z0_Code, Z0_Date, Z0_DateOnly, Z0_DateTimeOffset, Z0_Decimal, Z0_Description, Z0_FK_Code, Z0_Geography, Z0_Guid, Z0_IsSystem, Z0_IsValid, Z0_Money, Z0_Number, Z0_NVarChar, Z0_NVarCharMax, Z0_Short, Z0_SmallDateTime, Z0_VarBinaryMax, Z0_VarCharMax, Z0_Xml) VALUES
	(@11, @12, @13, @14, @15, @16, @17, @18, @19, @110, @111, @112, @113, @114, @115, @116, @117, @118, @119, @120, @121, @122, @123, @124, @125, @126, @127, @128, @129)
'
, N'@11 uniqueidentifier, @12 datetime, @13 decimal(18,3), @14 int, @15 bit, @16 bit, @17 bit, @18 char(1), @19 tinyint, @110 varchar(5), @111 datetime, @112 date, @113 datetimeoffset(7), @114 decimal(18,0), @115 varchar(100), @116 varchar(5), @117 geography, @118 uniqueidentifier, @119 bit, @120 char(1), @121 money, @122 int, @123 nvarchar(20), @124 nvarchar(max), @125 smallint, @126 smalldatetime, @127 varbinary(max), @128 varchar(max), @129 xml'
, @11 = 'eed879c8-5383-465e-b59e-c3b00cc10f96', @12 = NULL, @13 = 0, @14 = 0, @15 = 0, @16 = 0, @17 = 1, @18 = 'N', @19 = 0, @110 = 'NCODE1', @111 = NULL, @112 = NULL, @113 = NULL, @114 = 0, @115 = 'Default', @116 = '', @117 = @1_117, @118 = NULL, @119 = 1, @120 = 'N', @121 = 0, @122 = 0, @123 = N'', @124 = N'', @125 = 0, @126 = NULL, @127 = NULL, @128 = '', @129 = N'';

SET @PK = 'eed879c8-5383-465e-b59e-c3b00cc10f97';
DECLARE @8_117 geography = geography::STGeomFromText('POINT EMPTY', 4326);
EXEC sys.sp_executesql N'INSERT dbo.DummyBizo (Z0_PK, Z0_AnotherDate, Z0_AnotherDecimal, Z0_AnotherNumber, Z0_BitFalse, Z0_BitFiltered, Z0_BitTrue, Z0_Bool, Z0_Byte, Z0_Code, Z0_Date, Z0_DateOnly, Z0_DateTimeOffset, Z0_Decimal, Z0_Description, Z0_FK_Code, Z0_Geography, Z0_Guid, Z0_IsSystem, Z0_IsValid, Z0_Money, Z0_Number, Z0_NVarChar, Z0_NVarCharMax, Z0_Short, Z0_SmallDateTime, Z0_VarBinaryMax, Z0_VarCharMax, Z0_Xml) VALUES
	(@11, @12, @13, @14, @15, @16, @17, @18, @19, @110, @111, @112, @113, @114, @115, @116, @117, @118, @119, @120, @121, @122, @123, @124, @125, @126, @127, @128, @129)
'
, N'@11 uniqueidentifier, @12 datetime, @13 decimal(18,3), @14 int, @15 bit, @16 bit, @17 bit, @18 char(1), @19 tinyint, @110 varchar(5), @111 datetime, @112 date, @113 datetimeoffset(7), @114 decimal(18,0), @115 varchar(100), @116 varchar(5), @117 geography, @118 uniqueidentifier, @119 bit, @120 char(1), @121 money, @122 int, @123 nvarchar(20), @124 nvarchar(max), @125 smallint, @126 smalldatetime, @127 varbinary(max), @128 varchar(max), @129 xml'
, @11 = 'eed879c8-5383-465e-b59e-c3b00cc10f97', @12 = NULL, @13 = 0, @14 = 0, @15 = 0, @16 = 0, @17 = 1, @18 = 'N', @19 = 0, @110 = 'NCODE1', @111 = NULL, @112 = NULL, @113 = NULL, @114 = 0, @115 = 'Default', @116 = '', @117 = @1_117, @118 = NULL, @119 = 1, @120 = 'N', @121 = 0, @122 = 0, @123 = N'', @124 = N'', @125 = 0, @126 = NULL, @127 = NULL, @128 = '', @129 = N'';

SET @PK = 'eed879c8-5383-465e-b59e-c3b00cc10f98';
DECLARE @9_117 geography = geography::STGeomFromText('POINT EMPTY', 4326);
EXEC sys.sp_executesql N'INSERT dbo.DummyBizo (Z0_PK, Z0_AnotherDate, Z0_AnotherDecimal, Z0_AnotherNumber, Z0_BitFalse, Z0_BitFiltered, Z0_BitTrue, Z0_Bool, Z0_Byte, Z0_Code, Z0_Date, Z0_DateOnly, Z0_DateTimeOffset, Z0_Decimal, Z0_Description, Z0_FK_Code, Z0_Geography, Z0_Guid, Z0_IsSystem, Z0_IsValid, Z0_Money, Z0_Number, Z0_NVarChar, Z0_NVarCharMax, Z0_Short, Z0_SmallDateTime, Z0_VarBinaryMax, Z0_VarCharMax, Z0_Xml) VALUES
	(@11, @12, @13, @14, @15, @16, @17, @18, @19, @110, @111, @112, @113, @114, @115, @116, @117, @118, @119, @120, @121, @122, @123, @124, @125, @126, @127, @128, @129)
'
, N'@11 uniqueidentifier, @12 datetime, @13 decimal(18,3), @14 int, @15 bit, @16 bit, @17 bit, @18 char(1), @19 tinyint, @110 varchar(5), @111 datetime, @112 date, @113 datetimeoffset(7), @114 decimal(18,0), @115 varchar(100), @116 varchar(5), @117 geography, @118 uniqueidentifier, @119 bit, @120 char(1), @121 money, @122 int, @123 nvarchar(20), @124 nvarchar(max), @125 smallint, @126 smalldatetime, @127 varbinary(max), @128 varchar(max), @129 xml'
, @11 = 'eed879c8-5383-465e-b59e-c3b00cc10f90', @12 = NULL, @13 = 0, @14 = 0, @15 = 0, @16 = 0, @17 = 1, @18 = 'N', @19 = 0, @110 = 'NCODE1', @111 = NULL, @112 = NULL, @113 = NULL, @114 = 0, @115 = 'Default', @116 = '', @117 = @1_117, @118 = NULL, @119 = 1, @120 = 'N', @121 = 0, @122 = 0, @123 = N'', @124 = N'', @125 = 0, @126 = NULL, @127 = NULL, @128 = '', @129 = N'';

END TRY
BEGIN CATCH
	DECLARE @ErrMsg nvarchar(4000) = ERROR_MESSAGE(), @Severity int = ERROR_SEVERITY(), @ErrNum int = ERROR_NUMBER();

	IF (@Severity > 18) THROW;
	ELSE
	BEGIN
		DECLARE @Msg nvarchar(4000) = '{' + @PK + ',' + case when @ErrMsg = '~ConcurrencyError~' then 'True' else 'False' end + ',' + cast(@ErrNum as varchar(10)) + '} ' + @ErrMsg;
		RAISERROR('%s', @Severity, 1, @Msg);
		IF (XACT_STATE()) = -1 ROLLBACK;
	END
END CATCH;";
		const string expectedResultAfterTruncation = @"SET @PK = 'eed879c8-5383-465e-b59e-c3b00cc10f98';
DECLARE @9_117 geography = geography::STGeomFromText('POINT EMPTY', 4326);
EXEC sys.sp_executesql N'INSERT dbo.DummyBizo (Z0_PK, Z0_AnotherDate, Z0_AnotherDecimal, Z0_AnotherNumber, Z0_BitFalse, Z0_BitFiltered, Z0_BitTrue, Z0_Bool, Z0_Byte, Z0_Code, Z0_Date, Z0_DateOnly, Z0_DateTimeOffset, Z0_Decimal, Z0_Description, Z0_FK_Code, Z0_Geography, Z0_Guid, Z0_IsSystem, Z0_IsValid, Z0_Money, Z0_Number, Z0_NVarChar, Z0_NVarCharMax, Z0_Short, Z0_SmallDateTime, Z0_VarBinaryMax, Z0_VarCharMax, Z0_Xml) VALUES
	(@11, @12, @13, @14, @15, @16, @17, @18, @19, @110, @111, @112, @113, @114, @115, @116, @117, @118, @119, @120, @121, @122, @123, @124, @125, @126, @127, @128, @129)
'
, N'@11 uniqueidentifier, @12 datetime, @13 decimal(18,3), @14 int, @15 bit, @16 bit, @17 bit, @18 char(1), @19 tinyint, @110 varchar(5), @111 datetime, @112 date, @113 datetimeoffset(7), @114 decimal(18,0), @115 varchar(100), @116 varchar(5), @117 geography, @118 uniqueidentifier, @119 bit, @120 char(1), @121 money, @122 int, @123 nvarchar(20), @124 nvarchar(max), @125 smallint, @126 smalldatetime, @127 varbinary(max), @128 varchar(max), @129 xml'
, @11 = 'eed879c8-5383-465e-b59e-c3b00cc10f90', @12 = NULL, @13 = 0, @14 = 0, @15 = 0, @16 = 0, @17 = 1, @18 = 'N', @19 = 0, @110 = 'NCODE1', @111 = NULL, @112 = NULL, @113 = NULL, @114 = 0, @115 = 'Default', @116 = '', @117 = @1_117, @118 = NULL, @119 = 1, @120 = 'N', @121 = 0, @122 = 0, @123 = N'', @124 = N'', @125 = 0, @126 = NULL, @127 = NULL, @128 = '', @129 = N'';

END TRY
BEGIN CATCH
	DECLARE @ErrMsg nvarchar(4000) = ERROR_MESSAGE(), @Severity int = ERROR_SEVERITY(), @ErrNum int = ERROR_NUMBER();

	IF (@Severity > 18) THROW;
	ELSE
	BEGIN
		DECLARE @Msg nvarchar(4000) = '{' + @PK + ',' + case when @ErrMsg = '~ConcurrencyError~' then 'True' else 'False' end + ',' + cast(@ErrNum as varchar(10)) + '} ' + @ErrMsg;
		RAISERROR('%s', @Severity, 1, @Msg);
		IF (XACT_STATE()) = -1 ROLLBACK;
	END
END CATCH;";

		public void TestIsEnabledOnByDefault()
		{
			AssertEquals("SqlEventTracker.IsEnabled", true, new SqlEventTracker().IsEnabled);
		}
	}
}
