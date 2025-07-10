using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Enterprise.AuditDataServices.Glow.Subscribers;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.Glow.Test
{
	[TestedType(typeof(ProcessTaskTemplateGlowSubscriber))]

	class ProcessTaskTemplateGlowSubscriberTest : ProcessTaskTemplateGlowSubscriberBaseTest
	{
		public void TestProperties()
		{
			var subscriber = new ProcessTaskTemplateGlowSubscriber();
			AssertEquals(ProcessTaskTemplateSchema.Instance, subscriber.Table);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				ProcessTaskTemplateSchema.Constants.P0_Name,
				ProcessTaskTemplateSchema.Constants.P0_ProcessType,
				ProcessTaskTemplateSchema.Constants.P0_IsActive,
				ProcessTaskTemplateSchema.Constants.P0_IsPartialTemplate,
				ProcessTaskTemplateSchema.Constants.P0_IsSystem,
				ProcessTaskTemplateSchema.Constants.P0_IsUniversal,
				ProcessTaskTemplateSchema.Constants.P0_GC,
				ProcessTaskTemplateSchema.Constants.P0_OH_Client,
				ProcessTaskTemplateSchema.Constants.P0_SubType1,
				ProcessTaskTemplateSchema.Constants.P0_LoadPortCountry,
				ProcessTaskTemplateSchema.Constants.P0_DischargePortCountry,
				ProcessTaskTemplateSchema.Constants.P0_EffectiveStartDateUtc,
				ProcessTaskTemplateSchema.Constants.P0_EffectiveEndDateUtc,
			}, subscriber.SpecificColumns.Select(c => c.Name));
			Assert(subscriber.NotifyInsert);
			Assert(subscriber.NotifyUpdate);
			Assert(subscriber.NotifyDelete);
			AssertEquals("PTG", subscriber.Code);
			AssertEquals("ProcessTaskTemplate Change Subscriber", subscriber.Description);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new ProcessTaskTemplateGlowSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestProcessChanges()
		{
			var processType = "TST";
			clientMock.Setup(c => c.PostAsync(ServiceRelativePath, It.IsAny<HttpContent>())).Callback((string uri, HttpContent content) =>
			{
				var contentAsString = content.ReadAsStringAsync().GetAwaiter().GetResult();
				AssertEquals(@$"[""{processType}""]", contentAsString);
			}).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted))).Verifiable();

			var subscriber = NewDataChangeSubscriber();
			var changeTable = MakeDataTable(processType);

			subscriber.ProcessChanges(null, changeTable);
			clientMock.Verify();
		}

		public void TestProcessChanges_InsertData()
		{
			var processType = "TST";
			clientMock.Setup(c => c.PostAsync(ServiceRelativePath, It.IsAny<HttpContent>())).Callback((string uri, HttpContent content) =>
			{
				var contentAsString = content.ReadAsStringAsync().GetAwaiter().GetResult();
				AssertEquals(@$"[""{processType}""]", contentAsString);
			}).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted))).Verifiable();

			var subscriber = NewDataChangeSubscriber();
			var changeTable = MakeDataTable(processType, setModified: false);
			changeTable.Rows[0].SetAdded();

			subscriber.ProcessChanges(null, changeTable);
			clientMock.Verify();
		}

		public void TestProcessChanges_DeleteData()
		{
			var processType = "TST";
			clientMock.Setup(c => c.PostAsync(ServiceRelativePath, It.IsAny<HttpContent>())).Callback((string uri, HttpContent content) =>
			{
				var contentAsString = content.ReadAsStringAsync().GetAwaiter().GetResult();
				AssertEquals(@$"[""{processType}""]", contentAsString);
			}).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted))).Verifiable();

			var subscriber = NewDataChangeSubscriber();
			var changeTable = MakeDataTable(processType, setModified: false);
			changeTable.Rows[0].Delete();

			subscriber.ProcessChanges(null, changeTable);
			clientMock.Verify();
		}

		public void TestProcessChanges_ProcessTypeChanged()
		{
			var processType1 = "TS1";
			var processType2 = "TS2";
			clientMock.Setup(c => c.PostAsync(ServiceRelativePath, It.IsAny<HttpContent>())).Callback((string uri, HttpContent content) =>
			{
				var contentAsString = content.ReadAsStringAsync().GetAwaiter().GetResult();
				AssertEquals(@$"[""{processType2}"",""{processType1}""]", contentAsString);
			}).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted))).Verifiable();

			var subscriber = NewDataChangeSubscriber();
			var changeTable = MakeDataTable(processType1);

			changeTable.Rows[0][ProcessTaskTemplateSchema.P0_ProcessType.Name] = processType2;
			subscriber.ProcessChanges(null, changeTable);
			clientMock.Verify();
		}

		protected override string ServiceRelativePath => "api/workflowTemplate/reportChanged";

		DataTable MakeDataTable(string processType, bool setModified = true)
		{
			var dataTable = new DataTable();
			dataTable.Columns.Add(ProcessTaskTemplateSchema.P0_ProcessType.Name, ProcessTaskTemplateSchema.P0_ProcessType.DotNetType);
			var row = dataTable.NewRow();
			row[ProcessTaskTemplateSchema.P0_ProcessType.Name] = processType;
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
			return MakeDataTable("TST");
		}

		protected override void TearDown()
		{
			ProcessTaskTemplateGlowSubscriber.ResetStaticMemebers();
			base.TearDown();
		}

		protected override DataTable GetTestDataTable() => null;
	}
}
