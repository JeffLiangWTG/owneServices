using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.TIP.ServiceTask.Testing
{
	[TestedType(typeof(ProductExportServiceTask))]
	class ProductExportServiceTaskTest : ServiceTaskTestCase<ProductExportServiceTask>
	{
		public void TestRunTask()
		{
			var highwaterMark = TIPDataRegistry.Instance.HighWaterMark;
			RunTaskSchedule(ServiceTask);
			Assert(ServiceTask.GetBuffer().AsString.Contains("Export Directory not specified or invalid."));
			Assert(ServiceTask.GetBuffer().AsString.Contains("Notification Group not specified or invalid."));
			Assert(ServiceTask.GetBuffer().AsString.Contains("Organisations not specified."));
			AssertNotContains("Export Started", $"====== Product Export to CSV for High Water Mark: '{highwaterMark}' Started ======", ServiceTask.GetBuffer().AsString);
			AssertNotContains("Export Finish", "====== Product Export to CSV Finished ======", ServiceTask.GetBuffer().AsString);
			ServiceTask.GetBuffer().Clear();
			SetUpRegistry();
			highwaterMark = TIPDataRegistry.Instance.HighWaterMark;
			RunTaskSchedule(ServiceTask);
			Assert(!ServiceTask.GetBuffer().AsString.Contains("Export Directory not specified or invalid."));
			Assert(!ServiceTask.GetBuffer().AsString.Contains("Notification Group not specified or invalid."));
			Assert(!ServiceTask.GetBuffer().AsString.Contains("Organisations not specified."));
			AssertContains("Export Started", $"====== Product Export to CSV for High Water Mark: '{highwaterMark}' Started ======", ServiceTask.GetBuffer().AsString);
			AssertContains("Export Finish", "====== Product Export to CSV Finished ======", ServiceTask.GetBuffer().AsString);
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("1hour", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
		#region Implementation
		void SetUpRegistry()
		{
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			GlbStaff staff = group.Staff.AddNew();
			staff.GS_Code = "P.T";
			staff.GS_EmailAddress = "pashtet@lan.com.ua";
			Factory.Save();
			ZDateTime testDate = ZDateTime.Now;
			DataTransferRegistryBusinessObject newValue = new DataTransferRegistryBusinessObject(Factory);
			newValue.Directory = Env.TempPath;
			newValue.GroupPK = group.PK;
			newValue.LastRunDateTime = ZDateTime.UtcNow.AddDays(-1);
			newValue.NextRunDateTime = ZDateTime.UtcNow;
			TIPDataRegistry.Instance.ProductExportRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
			OrgPartRelationRegistryBusinessObjectCollection collection = new OrgPartRelationRegistryBusinessObjectCollection();
			OrgPartRelationRegistryBusinessObject obj = (OrgPartRelationRegistryBusinessObject)collection.AddNew();
			obj.OrgHeaderPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			TIPDataRegistry.Instance.OrganisationProductRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		ProductExportServiceTask ServiceTask;
		protected override void SetUpCore()
		{
			base.SetUpCore();
			ServiceTask = new ProductExportServiceTask();
			InitialiseTaskSchedule(ServiceTask);
		}
		#endregion
	}
}
