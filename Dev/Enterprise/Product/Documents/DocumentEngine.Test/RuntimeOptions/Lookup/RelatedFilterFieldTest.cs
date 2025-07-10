using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class RelatedFilterFieldTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestZValueAndValueRemainInSync()
		{
			var reportCommand = PrepareReportCommandForTest();
			var client = GlbCompany.CurrentCompany.OrgProxy;
			ColumnConfigurationManager configuration;

			using (var documentPack = new DocumentPack(reportCommand))
			using (var report = documentPack.GetFirstReport())
			{
				configuration = new CombinedConfigurationManager(report.ColumnHeadingManager, "My Test Configuration");

				report.PrepareForRender();
				AssertEquals("Pre-condition: report.FilterCollection.Count", 14, report.FilterCollection.Count);

				var clientField = report.FilterCollection["Client"] as LookupField;
				AssertEquals(ZGuid.Empty, clientField.ZValue);
				AssertEquals(Guid.Empty, clientField.Value);

				clientField.ZValue = client.PK;
				AssertEquals(client.PK, clientField.ZValue);
				AssertEquals(client.PK, clientField.Value);

				clientField.ZValue = ZGuid.Empty;
				AssertEquals(ZGuid.Empty, clientField.ZValue);
				AssertEquals(Guid.Empty, clientField.Value);

				clientField.ZValue = ZGuid.Invalid;
				AssertEquals(ZGuid.Invalid, clientField.ZValue);
				AssertEquals(Guid.Empty, clientField.Value);

				clientField.Value = Guid.Empty;
				AssertEquals(ZGuid.Empty, clientField.ZValue);
				AssertEquals(Guid.Empty, clientField.Value);

				clientField.Value = client.PK.ToGuid();
				AssertEquals(client.PK, clientField.ZValue);
				AssertEquals(client.PK, clientField.Value);

				clientField.Value = Guid.Empty;
				AssertEquals(ZGuid.Empty, clientField.ZValue);
				AssertEquals(Guid.Empty, clientField.Value);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadingAConfigurationSetsUpFilterRelations()
		{
			var reportCommand = PrepareReportCommandForTest();
			var client = GlbCompany.CurrentCompany.OrgProxy;
			ColumnConfigurationManager configuration;

			using (var documentPack = new DocumentPack(reportCommand))
			using (var report = documentPack.GetFirstReport())
			{
				configuration = new CombinedConfigurationManager(report.ColumnHeadingManager, "My Test Configuration");

				report.PrepareForRender();
				AssertEquals("Pre-condition: report.FilterCollection.Count", 14, report.FilterCollection.Count);

				var clientField = report.FilterCollection["Client"] as LookupField;
				clientField.ZValue = client.PK;
				AssertEquals("Pre-condition: Client field should be set.", client.PK, clientField.Value);

				var productField = report.FilterCollection["Product"] as LookupField;
				AssertContains("Pre-condition: Client field filter should contain client pk somewhere because it is related.", client.PK.ToString(), productField.BindToList.CompleteFilter.LiteralTextSqlFormatted);

				var filters = new CollectionOfIFilter();
				filters.Add(clientField);

				configuration.Save(filters, string.Empty, string.Empty, string.Empty, string.Empty);
			}

			using (var documentPack = new DocumentPack(reportCommand))
			using (var report = documentPack.GetFirstReport())
			{
				report.PrepareForRender();
				AssertEquals("Pre-condition: report.FilterCollection.Count", 14, report.FilterCollection.Count);

				configuration.Load(report);

				var clientField = report.FilterCollection["Client"] as LookupField;
				AssertEquals("Client field should be deserialized from configuration.", client.PK, clientField.Value);

				var productField = report.FilterCollection["Product"] as LookupField;
				AssertContains("Client field filter should contain client pk somewhere because it is related.", client.PK.ToString(), productField.BindToList.CompleteFilter.LiteralTextSqlFormatted);
				Assert("Product field should not be read only", !productField.ReadOnly);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDependentFilterReadonlyWasSetCorrectly()
		{
			var reportCommand = PrepareReportCommandForTest();
			var client = GlbCompany.CurrentCompany.OrgProxy;

			using (var documentPack = new DocumentPack(reportCommand))
			using (var report = documentPack.GetFirstReport())
			{
				report.PrepareForRender();

				var clientField = report.FilterCollection["Client"] as LookupField;
				var productField = report.FilterCollection["Product"] as LookupField;

				Assert("Product field should be readonly by default", productField.ReadOnly);

				clientField.ZValue = client.PK;
				AssertEquals("Client field should be set.", client.PK, clientField.Value);
				clientField.ValidateZValue();
				Assert("Product field should be not readonly cause its 'parent' field is valid", !productField.ReadOnly);

				clientField.ZValue = ZGuid.Invalid;
				AssertEquals("Client field should be set.", ZGuid.Empty, clientField.Value);
				clientField.ValidateZValue();
				Assert("Product field should be readonly cause its 'parent' field is invalid", productField.ReadOnly);

				clientField.ZValue = client.PK;
				AssertEquals("Client field should be set.", client.PK, clientField.Value);
				clientField.ValidateZValue();
				Assert("Product field should be not readonly cause its 'parent' field is valid again", !productField.ReadOnly);
			}
		}

		ReportCommand PrepareReportCommandForTest()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("Whs Stock Movements.xls", TestFilesSubFolder.ReportTestFiles);
			var template = Factory.New<StmTemplateBase>();
			template.SO_Name = "Whs Stock Movement";
			template.SO_Template = excelTemplate.GetAsByteArray();

			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			return reportCommand;
		}
	}
}
