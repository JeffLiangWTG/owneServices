using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.DocWrappers.Testing
{
	[TestsSubclassesOf(typeof(GenericWrapperLoader), new Type[0], new[] { typeof(EmptyGenericWrapperLoader) })]
	public abstract class GenericWrapperLoaderBaseTest<TLoader, TWrapper> : TestCaseWithFactory
	{
		public abstract Core.Constants.DataContext[] SupportedDataContexts { get; }
		public abstract BusinessObject GetBusinessObjectToWrap();
		public abstract BusinessObject GetParentBusinessObjectToWrap();
		public abstract BusinessObject GetChildBusinessObjectToWrap();

		public void TestGetWrappersCausesNoException()
		{
			foreach (Core.Constants.DataContext dataContext in SupportedDataContexts)
			{
				GenericWrapperLoader loader = GenericWrapperLoader.GetFromDataContext(dataContext);

				AssertEquals(String.Format("Ensure loader is properly mapped to {0} data context in EnterpriseApplicationConfiguration.xml", dataContext), typeof(TLoader), loader.GetType());
				AssertEquals(typeof(TWrapper), loader.GetWrapperType());
				AssertNoExceptionThrown(() => loader.GetWrappers(GetBusinessObjectToWrap(), Factory));
				AssertNoExceptionThrown(() => loader.GetWrappers(GetParentBusinessObjectToWrap(), GetChildBusinessObjectToWrap(), Factory));
			}
		}
	}
}
