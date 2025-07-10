using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(JobHeaderView))]
	class JobHeaderViewTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var viewModel = new MultiJobHeaderEditorViewModel(new[] { dummy }, Factory);

			return viewModel.JobHeaderViews.Single();
		}

		#endregion
	}
}
