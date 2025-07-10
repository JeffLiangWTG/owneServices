using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using Enterprise.AuditDataServices.Glow.Subscribers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.Glow.Test
{
	[TestedType(typeof(ProcessTemplateValidationActionGlowSubscriber))]
	class ProcessTemplateValidationActionGlowSubscriberTest : ProcessTaskTemplateGlowSubscriberBaseTest
	{
		public void TestProperties()
		{
			var subscriber = NewDataChangeSubscriber();
			AssertEquals(ProcessTemplateValidationActionSchema.Instance, subscriber.Table);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				ProcessTemplateValidationActionSchema.Constants.P0A_ActionSource,
				ProcessTemplateValidationActionSchema.Constants.P0A_P0_WorkflowTemplate,
				ProcessTemplateValidationActionSchema.Constants.P0A_P0V_ValidationRule,
			}, subscriber.SpecificColumns.Select(c => c.Name));
			Assert(subscriber.NotifyInsert);
			Assert(subscriber.NotifyUpdate);
			Assert(subscriber.NotifyDelete);
			AssertEquals("PTA", subscriber.Code);
			AssertEquals("ProcessTemplateValidationAction Change Subscriber", subscriber.Description);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new ProcessTemplateValidationActionGlowSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestProcessChanges()
		{
			var factory = new BusinessObjectFactory();
			var template = factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "TST";
			factory.Save();

			clientMock.Setup(c => c.PostAsync(ServiceRelativePath, It.IsAny<HttpContent>())).Callback((string uri, HttpContent content) =>
			{
				var contentAsString = content.ReadAsStringAsync().GetAwaiter().GetResult();
				AssertEquals(@$"[""{template.PK}""]", contentAsString);
			}).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted))).Verifiable();

			var subscriber = NewDataChangeSubscriber();
			var changeTable = MakeDataTable(template.PK.ToGuid());
			subscriber.ProcessChanges(null, changeTable);
			clientMock.Verify();
		}

		public void TestProcessChanges_InsertData()
		{
			var factory = new BusinessObjectFactory();
			var template = factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "TST";
			factory.Save();

			clientMock.Setup(c => c.PostAsync(ServiceRelativePath, It.IsAny<HttpContent>())).Callback((string uri, HttpContent content) =>
			{
				var contentAsString = content.ReadAsStringAsync().GetAwaiter().GetResult();
				AssertEquals(@$"[""{template.PK}""]", contentAsString);
			}).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted))).Verifiable();

			var subscriber = NewDataChangeSubscriber();
			var changeTable = MakeDataTable(template.PK.ToGuid(), setModified: false);
			changeTable.Rows[0].SetAdded();

			subscriber.ProcessChanges(null, changeTable);
			clientMock.Verify();
		}

		public void TestProcessChanges_DeleteData()
		{
			var factory = new BusinessObjectFactory();
			var template = factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "TST";
			factory.Save();

			clientMock.Setup(c => c.PostAsync(ServiceRelativePath, It.IsAny<HttpContent>())).Callback((string uri, HttpContent content) =>
			{
				var contentAsString = content.ReadAsStringAsync().GetAwaiter().GetResult();
				AssertEquals(@$"[""{template.PK}""]", contentAsString);
			}).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted))).Verifiable();

			var subscriber = NewDataChangeSubscriber();
			var changeTable = MakeDataTable(template.PK.ToGuid(), setModified: false);
			changeTable.Rows[0].Delete();

			subscriber.ProcessChanges(null, changeTable);
			clientMock.Verify();
		}

		public void TestProcessChanges_ParentChanged()
		{
			var factory = new BusinessObjectFactory();
			var template1 = factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "TS1";
			var template2 = factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "TS2";
			factory.Save();

			clientMock.Setup(c => c.PostAsync(ServiceRelativePath, It.IsAny<HttpContent>())).Callback((string uri, HttpContent content) =>
			{
				var contentAsString = content.ReadAsStringAsync().GetAwaiter().GetResult();
				var values = JArray.Parse(contentAsString).Select(x => x.Value<string>()).ToList();
				AssertEquals(2, values.Count);
				AssertCollectionContains(template1.PK.ToString(), values);
				AssertCollectionContains(template2.PK.ToString(), values);
			}).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted))).Verifiable();

			var subscriber = NewDataChangeSubscriber();
			var changeTable = MakeDataTable(template1.PK.ToGuid());

			changeTable.Rows[0][ProcessTemplateValidationActionSchema.P0A_P0_WorkflowTemplate.Name] = template2.PK.ToGuid();
			subscriber.ProcessChanges(null, changeTable);
			clientMock.Verify();
		}

		protected override string ServiceRelativePath => "api/workflowTemplate/validation/reportChanged";

		DataTable MakeDataTable(Guid template, bool setModified = true)
		{
			var dataTable = new DataTable();
			dataTable.Columns.Add(ProcessTemplateValidationActionSchema.P0A_P0_WorkflowTemplate.Name, ProcessTemplateValidationActionSchema.P0A_P0_WorkflowTemplate.DotNetType);
			var row = dataTable.NewRow();
			row[ProcessTemplateValidationActionSchema.P0A_P0_WorkflowTemplate.Name] = template;
			dataTable.Rows.Add(row);
			row.AcceptChanges();

			if (setModified)
			{
				row.SetModified();
			}

			return dataTable;
		}

		protected override DataTable NewDataTable()
		{
			var factory = new BusinessObjectFactory();
			var template = factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "TST";
			factory.Save();

			return MakeDataTable(template.PK.ToGuid());
		}

		protected override void TearDown()
		{
			ProcessTemplateValidationActionGlowSubscriber.ResetStaticMemebers();
			base.TearDown();
		}

		protected override DataTable GetTestDataTable() => null;
	}
}
