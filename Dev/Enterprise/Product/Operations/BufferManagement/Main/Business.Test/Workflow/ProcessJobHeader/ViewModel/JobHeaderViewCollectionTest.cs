using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(JobHeaderViewCollection))]
	class JobHeaderViewCollectionTest : NonPersistentBusinessObjectCollectionTestCase<JobHeaderViewCollection>
	{
		#region Implementation

		protected override JobHeaderViewCollection GetCollectionToTest()
		{
			var viewModel = new MultiJobHeaderEditorViewModel(Enumerable.Empty<BusinessObject>(), Factory);

			return viewModel.JobHeaderViews;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var viewModel = new MultiJobHeaderEditorViewModel(new[] { dummy }, Factory);

			return viewModel.JobHeaderViews.Single();
		}

		#endregion
	}
}
