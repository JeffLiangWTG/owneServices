using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.VisualBoards.Business.Test
{
	[TestsSubclassesOf(typeof(IBoardSectionDescriptor))]
	public abstract class BoardSectionDescriptorTestCase<T> : TestCaseWithFactory
		where T : IBoardSectionDescriptor
	{
		public void TestGetSectionConfigurationBizo()
		{
			TestGetSectionConfigurationBizoCore(GetDescriptor());
		}

		public void TestGetSectionConfigurationBizoDoesNotThrowExceptionWithNullSection()
		{
			var descriptor = GetDescriptor();
			AssertExceptionThrown<ArgumentNullException>(() => descriptor.GetSectionConfigurationBizo(null));
		}

		public void TestGetSectionConfigurationControl()
		{
			TestGetSectionConfigurationControlCore(GetDescriptor());
		}

		public void TestGetAdditionalTabs()
		{
			TestGetAdditionalTabsCore(GetDescriptor());
		}

		public void TestGetGetSectionControl()
		{
			TestGetSectionControlCore(GetDescriptor());
		}

		public void TestGetViewModel()
		{
			TestGetViewModelCore(GetDescriptor());
		}

		#region Implementation

		IBoardSectionDescriptor GetDescriptor()
		{
			return SectionDescriptorProvider.Get(Type);
		}

		protected abstract string Type { get; }

		protected abstract void TestGetSectionConfigurationBizoCore(IBoardSectionDescriptor descriptor);
		protected abstract void TestGetSectionConfigurationControlCore(IBoardSectionDescriptor descriptor);
		protected abstract void TestGetAdditionalTabsCore(IBoardSectionDescriptor descriptor);
		protected abstract void TestGetSectionControlCore(IBoardSectionDescriptor descriptor);
		protected abstract void TestGetViewModelCore(IBoardSectionDescriptor descriptor);

		#endregion
	}
}
