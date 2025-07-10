using System.Collections.Generic;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.DocumentWrappers.ProcessManagement.Testing
{
	public abstract class DocBaseWorkItemTest<T, TWrapper> : DocumentWrapperTestCase
			where T : WorkItemCommon
			where TWrapper : DocBaseWorkItem
	{
		public void TestJobHeader()
		{
			AssertNull("JobHeader", WorkItemWrapper.JobHeader);
		}

		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { WorkItemWrapper };
		}

		protected T WorkItemCommon;
		protected TWrapper WorkItemWrapper;

		protected override void SetUp()
		{
			WorkItemCommon = GetNewWorkItemCommon();
			WorkItemWrapper = CreateWorkItemWrapper(WorkItemCommon);
			AssertNotNull("Wrapper not null", WorkItemWrapper);
			base.SetUp();
		}

		protected virtual T GetNewWorkItemCommon()
		{
			return Factory.NewWithValidTestData<T>();
		}

		protected virtual TWrapper CreateWorkItemWrapper(T workItem)
		{
			TWrapper result = (TWrapper)DocBaseWorkItem.New(workItem, Factory);
			((IBODocDataProvider)result).SetDocWrapperContext(new Dictionary<string, object>()); // Will call through to OnDocWrappersContextSet().
			result.SetReportNameForTesting("Report Name");
			return result;
		}

		#endregion
	}
}
