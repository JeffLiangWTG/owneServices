using System;
using Enterprise.Client.EDI.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IssueManager.Business.Test
{
	class ExceptionKeyBuilderTest : TransactionedTestCase
	{
		public void TestBuildKey()
		{
			var regexes = EDIDataRegistry.Instance.ExceptionKeyMatchingRegexes.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			var collection = new ExceptionKeyRegexCollection();
			collection.Add(new ExceptionKeyRegex() { Regex = @"ABCD", Description = "" });
			collection.Add(new ExceptionKeyRegex() { Regex = @"KEYBLAH", Description = "" });
			collection.Add(new ExceptionKeyRegex() { Regex = @"LD", Description = "" });
			EDIDataRegistry.Instance.ExceptionKeyMatchingRegexes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			EDIDataRegistry.Instance.ExceptionKeyMatchingRegexes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			ExceptionKeyBuilder builder = new ExceptionKeyBuilder();
			ExceptionKeyFields fields = new ExceptionKeyFields();

			fields.Key = "KEYBLAHSheep";
			fields.Message = "message (HResult: 0x34324) bla bla";
			AssertEquals("KEYBLAH (HResult: 0x34324)", builder.BuildKey(fields));

			fields.Key = "  KeyBLAhSheep   ";
			fields.Message = "message HResult: 0x34324 bla bla";
			AssertEquals("Part key should use case sensitive matching", "KeyBLAhSheep (HResult: 0x34324)", builder.BuildKey(fields));

			fields.Key = "  LDEFFHDD  ";
			fields.Message = "";
			AssertEquals("Keys less than 4 chars should be ignored", "", builder.BuildKey(fields));
		}

		public void TestBuildKeyWhenEmptyKeyAndCallStack()
		{
			ExceptionKeyBuilder builder = new ExceptionKeyBuilder();

			ExceptionKeyFields fields = new ExceptionKeyFields();
			fields.Key = fields.CallStack = string.Empty;
			fields.Source = "SomeCoreAssembly";
			AssertEquals("SomeCoreAssembly -", builder.BuildKey(fields));

			fields.AddMessage("Some exception that you should take note of");
			AssertEquals("SomeCoreAssembly - Some exception that you should take note of", builder.BuildKey(fields));

			ExceptionKeyFields fieldsWithInnerException = new ExceptionKeyFields();
			fieldsWithInnerException.Key = fieldsWithInnerException.CallStack = string.Empty;
			fieldsWithInnerException.Source = "SomeCargoWiseAssembly";
			fieldsWithInnerException.AddMessage("Very General Message");
			fieldsWithInnerException.AddMessage("A specific message");
			AssertEquals("SomeCargoWiseAssembly - Very General Message - A specific message", builder.BuildKey(fieldsWithInnerException));

			fieldsWithInnerException.AddMessage("A presumingly very specific message");
			AssertEquals("SomeCargoWiseAssembly - Very General Message - A specific message - A presumingly very specific message", builder.BuildKey(fieldsWithInnerException));
		}

		public void TestBuildKey_KeyMatchingRegexes()
		{
			var regexes = EDIDataRegistry.Instance.ExceptionKeyMatchingRegexes.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			var collection = new ExceptionKeyRegexCollection();
			collection.Add(new ExceptionKeyRegex() { Regex = @"^ABCD [0-9]{2}-Nov-12", Description = "" });
			collection.Add(new ExceptionKeyRegex() { Regex = @"ABCD", Description = "" });
			collection.Add(new ExceptionKeyRegex() { Regex = @"TEST\([A-Z]+", Description = "" });
			EDIDataRegistry.Instance.ExceptionKeyMatchingRegexes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			ExceptionKeyBuilder builder = new ExceptionKeyBuilder();
			ExceptionKeyFields fields = new ExceptionKeyFields();

			fields.Key = "ABCD 15-Nov-12 blah";
			AssertEquals("Should match and replace", @"^ABCD [0-9]{2}-Nov-12", builder.BuildKey(fields));

			fields.Key = "TEST(S";
			AssertEquals("Should match and replace", @"TEST\([A-Z]+", builder.BuildKey(fields));

			fields.Key = "TEST(1";
			AssertEquals("Should not match", "TEST(1", builder.BuildKey(fields));
		}

		public void TestBuildKey_ConcurrencyMassage()
		{
			ExceptionKeyBuilder builder = new ExceptionKeyBuilder();
			ExceptionKeyFields fields = new ExceptionKeyFields();

			fields.Key = "  **CONCURRENCY Error Saving Record **\r\n\r\nTablename: JobCharge  PK: a9075d04-9cc2-47b7-9545-119e74c1bd52  RowState: Modified  Business object around row = Enterprise.Accounting.Busines   ";
			fields.Message = "message HResult: 0x34324 bla bla";
			fields.CallStack = "temp stack trace ";
			AssertEquals("**CONCURRENCY Error Saving Record **  Tablename: JobCharge  \r\ntemp stack trace (HResult: 0x34324)", builder.BuildKey(fields));

			fields.Key = "  ** CONCURRENCY Error Saving Record **\r\n\r\nTablename: JobCharge  PK: a9075d04-9cc2-47b7-9545-119e74c1bd52  RowState: Modified  Business object around row = Enterprise.Accounting.Busines   ";
			AssertEquals("**CONCURRENCY Error Saving Record **  Tablename: JobCharge  \r\ntemp stack trace (HResult: 0x34324)", builder.BuildKey(fields));

			fields.Key = "dsjkhfdsjkfhk  ** CONCURRENCY Error Saving Record **\r\n\r\nTablename: JobCharge  PK: a9075d04-9cc2-47b7-9545-119e74c1bd52";
			AssertEquals("**CONCURRENCY Error Saving Record **  Tablename: JobCharge  \r\ntemp stack trace (HResult: 0x34324)", builder.BuildKey(fields));

			fields.Key = "ConcurrencyError";
			AddFourMessages(fields, "MyConcurrencyError", "InnerConcurrencyError", "  **CONCURRENCY Error Saving Record **\r\n\r\nTablename: JobCharge  PK: a9075d04-9cc2-47b7-9545-119e74c1bd52  RowState: Modified  Business object around row = Enterprise.Accounting.Busines   ", "AnotherInnerConcurrencyError");
			AssertEquals("**CONCURRENCY Error Saving Record **  Tablename: JobCharge  \r\ntemp stack trace", builder.BuildKey(fields));

			AddFourMessages(fields, "MyConcurrencyError", "InnerConcurrencyError", "  ***CONCURRENCY Error Saving Record **\r\n\r\nTablename: JobCharge  PK: a9075d04-9cc2-47b7-9545-119e74c1bd52  RowState: Modified  Business object around row = Enterprise.Accounting.Busines   ", "AnotherInnerConcurrencyError");
			AssertEquals("**CONCURRENCY Error Saving Record **  Tablename: JobCharge  \r\ntemp stack trace", builder.BuildKey(fields));

			AddFourMessages(fields, "MyConcurrencyError", "InnerConcurrencyError", "  *** CONCURRENCY Error Saving Record **\r\n\r\nTablename: JobCharge  PK: a9075d04-9cc2-47b7-9545-119e74c1bd52  RowState: Modified  Business object around row = Enterprise.Accounting.Busines   ", "AnotherInnerConcurrencyError");
			AssertEquals("**CONCURRENCY Error Saving Record **  Tablename: JobCharge  \r\ntemp stack trace", builder.BuildKey(fields));
		}

		void AddFourMessages(ExceptionKeyFields fields, string m1, string m2, string m3, string m4)
		{
			fields.Message = m1;
			fields.AddMessage(m2);
			fields.AddMessage(m3);
			fields.AddMessage(m4);
		}

		public void TestRemoveDefaultKeyRegex()
		{
			var fields = new ExceptionKeyFields { Key = reportGenerationError };
			var key = new ExceptionKeyBuilder().BuildKey(fields);

			const string expectedKeyWithRegexRemoved = @"Error Generating Report [Shipment Containers By Client and Carrier]
MenuItem PK: [23a83793-8b43-44a1-8134-edc1b46f7af5] BusinessContext: [RepFreightReport] Name/Path: [Shipment Containers By Client and Carrier] Filter: [] IsSystemDefined: [Y] IsClientSpecific: [N]
Template PK: [600d529e-bfb6-443c-a901-7cab1c5e8c7a] Name: [Shipment Containers by Client and Carrier] DataContext: [None] ExcelFilePath: [Enterprise\Product\Documents\ExcelTemplates\Reports\Shipment Containers by Client and Carrier.xls] IsSystemDefined: [Y] IsClientSpecific: [N]
--------------- Errors Found ---------------
Severity: [Fatal] Message: [Error loading table [ReportData]. Error: [Conversion failed when converting from a character string to uniqueidentifier.] occurred running SQL: [SELECT JC_ContainerNum, JC_ContainerMode, RC_Code, RC_TEU, HOUSEBILL, MASTERBILL, CUSENTRYINFO, JOBREFERENCE, VESSEL, VOYAGE, LOADPORTFIRST, ETDFIRST, CARRIERFULLNAME, CARRIERCODE, CLIENTFULLNAME, CLIENTCODE FROM Report_ShipmentContainersByClientAndCarrier () ORDER BY JC_ContainerNum,HouseBill option (recompile)]] Sheetname: [(unknown)] Cell Content: []";

			AssertEquals(expectedKeyWithRegexRemoved, key);
		}

		public void TestRemoveCustomKeyRegex()
		{
			var regexes = EDIDataRegistry.Instance.ExceptionKeyRegexes.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			regexes.Add(new ExceptionKeyRegex() { Regex = @"\[", Description = "" });
			regexes.Add(new ExceptionKeyRegex() { Regex = @"\]", Description = "" });
			EDIDataRegistry.Instance.ExceptionKeyRegexes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regexes);

			var fields = new ExceptionKeyFields { Key = reportGenerationError };
			var key = new ExceptionKeyBuilder().BuildKey(fields);

			const string expectedKeyWithRegexRemoved = @"Error Generating Report Shipment Containers By Client and Carrier
MenuItem PK: 23a83793-8b43-44a1-8134-edc1b46f7af5 BusinessContext: RepFreightReport Name/Path: Shipment Containers By Client and Carrier Filter:  IsSystemDefined: Y IsClientSpecific: N
Template PK: 600d529e-bfb6-443c-a901-7cab1c5e8c7a Name: Shipment Containers by Client and Carrier DataContext: None ExcelFilePath: Enterprise\Product\Documents\ExcelTemplates\Reports\Shipment Containers by Client and Carrier.xls IsSystemDefined: Y IsClientSpecific: N
--------------- Errors Found ---------------
Severity: Fatal Message: Error loading table ReportData. Error: Conversion failed when converting from a character string to uniqueidentifier. occurred running SQL: SELECT JC_ContainerNum, JC_ContainerMode, RC_Code, RC_TEU, HOUSEBILL, MASTERBILL, CUSENTRYINFO, JOBREFERENCE, VESSEL, VOYAGE, LOADPORTFIRST, ETDFIRST, CARRIERFULLNAME, CARRIERCODE, CLIENTFULLNAME, CLIENTCODE FROM Report_ShipmentContainersByClientAndCarrier () ORDER BY JC_ContainerNum,HouseBill option (recompile) Sheetname: (unknown) Cell Content:";

			AssertEquals(expectedKeyWithRegexRemoved, key);
		}

		const string reportGenerationError = @"Error Generating Report [Shipment Containers By Client and Carrier]
MenuItem PK: [23a83793-8b43-44a1-8134-edc1b46f7af5] BusinessContext: [RepFreightReport] Name/Path: [Shipment Containers By Client and Carrier] Filter: [] IsSystemDefined: [Y] IsClientSpecific: [N]
Template PK: [600d529e-bfb6-443c-a901-7cab1c5e8c7a] Name: [Shipment Containers by Client and Carrier] DataContext: [None] ExcelFilePath: [Enterprise\Product\Documents\ExcelTemplates\Reports\Shipment Containers by Client and Carrier.xls] IsSystemDefined: [Y] IsClientSpecific: [N]
--------------- Errors Found ---------------
Severity: [Fatal] Message: [Error loading table [ReportData]. Error: [Conversion failed when converting from a character string to uniqueidentifier.] occurred running SQL: [SELECT JC_ContainerNum, JC_ContainerMode, RC_Code, RC_TEU, HOUSEBILL, MASTERBILL, CUSENTRYINFO, JOBREFERENCE, VESSEL, VOYAGE, LOADPORTFIRST, ETDFIRST, CARRIERFULLNAME, CARRIERCODE, CLIENTFULLNAME, CLIENTCODE FROM Report_ShipmentContainersByClientAndCarrier (@p1248, @p1249, @p1250, @p1251, @p1252, @p1253, @p1254, @p1255) ORDER BY JC_ContainerNum,HouseBill option (recompile)]] Sheetname: [(unknown)] Cell Content: []";

		public void TestBuildKey_ForSqlException_ShouldIncludeMessage()
		{
			var fields = new ExceptionKeyFields
			{
				Key = "",
				Message = "SHUTDOWN is in progress.",
				CallStack = "IAMA callstack",
				Type = nameof(SqlException),
			};
			var key = new ExceptionKeyBuilder().BuildKey(fields);
			AssertEquals("SHUTDOWN is in progress.\r\nIAMA callstack", key);
		}

		public void TestBuildKey_Trimmed()
		{
			var regexes = EDIDataRegistry.Instance.ExceptionKeyRegexes.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			regexes.Add(new ExceptionKeyRegex() { Regex = @"(?<!^)\b[a-fA-F0-9]{8}(?:-[a-fA-F0-9]{4}){3}-[a-fA-F0-9]{12}\b", Description = "Remove GUIDs not at the start" });
			regexes.Add(new ExceptionKeyRegex() { Regex = @"ScheduleTaskToBusinessObjectMapping", Description = "" });

			EDIDataRegistry.Instance.ExceptionKeyRegexes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regexes);

			var fields = new ExceptionKeyFields { Key = "Volume too big on invoice line when saving, set to zero e29a0fea-171e-43f4-8f18-c067a751a52c" };
			var key = new ExceptionKeyBuilder().BuildKey(fields);
			AssertEquals("Volume too big on invoice line when saving, set to zero", key);

			fields = new ExceptionKeyFields { Key = "ScheduleTaskToBusinessObjectMapping", Message = "The paging file is too small for this operation to complete. (Exception from HRESULT: 0x800705AF)" };
			key = new ExceptionKeyBuilder().BuildKey(fields);
			AssertEquals("(HResult: 0x800705AF)", key);
		}
	}
}
