using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(GenericFreightJobServicesWrapperLoader))]
	sealed class GenericFreightJobServicesWrapperLoaderTest : GenericWrapperLoaderBaseTest<GenericFreightJobServicesWrapperLoader, ServiceWrapper>
	{
		public override BusinessObject GetBusinessObjectToWrap()
		{
			return Factory.New<JobService>();
		}

		public override BusinessObject GetParentBusinessObjectToWrap()
		{
			return null;
		}

		public override BusinessObject GetChildBusinessObjectToWrap()
		{
			return Factory.New<JobService>();
		}

		public override Core.Constants.DataContext[] SupportedDataContexts
		{
			get { return new Core.Constants.DataContext[] { Core.Constants.DataContext.GenericFreightJobServices }; }
		}
	}
}
