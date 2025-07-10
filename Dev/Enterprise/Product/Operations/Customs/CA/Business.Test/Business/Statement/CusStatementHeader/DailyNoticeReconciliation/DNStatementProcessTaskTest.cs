using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(DailyNoticeStatementProcessTask))]
	sealed class DNStatementProcessTaskTest : ProcessTaskTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return WorkflowItems.AddNew();
		}

		ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					var header = Factory.New<CusStatementHeader>();
					header.B2_IsMonthlyStatement = false;
					workflowItems = header.WorkflowItems;
				}

				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;
	}
}
