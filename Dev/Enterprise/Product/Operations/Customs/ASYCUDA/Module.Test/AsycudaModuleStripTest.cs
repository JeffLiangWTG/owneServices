using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Module.Testing
{
	[TestedType(typeof(ModuleTextFilter))]
	sealed class AsycudaModuleStripTest : ModuleTextFilterTest
	{
		public void TestGetCurrentFilterControls()
		{
			using (var filterStrip = new AsycudaModuleStripForTest())
			{
				filterStrip.SetDataBinding(new ZArchitecture.Business.Internal.FilterStrip(new ModuleFilterCollection()), "");
				using (var result = filterStrip.GetCurrentFilterControlsForTest(new CountryRelatedFilter(AsycudaFilterStrip.FilterConstants.MessageStatus, (val1, val2) => new ZQuery(), Factory, ZArchitecture.FieldType.TextDropEdit, 3, CountryRelatedFilterHelper.ListGetters.MessageStatusGetter)))
				{
					AssertEquals(typeof(CountryRelatedFilterControl), result.GetType());
				}

				using (var result = filterStrip.GetCurrentFilterControlsForTest(new DataGroupingRelatedFilter(AsycudaFilterStrip.FilterConstants.ManifestType, (val1, val2) => new ZQuery(), Factory, ZArchitecture.FieldType.TextDropEdit, 3, NoResourceStringData.GetData(AsycudaFilterStrip.FilterConstants.ManifestType) , ApplicationBusinessProvider.GetManifestTypeListByCountry)))
				{
					AssertEquals(typeof(DataGroupingRelatedFilterControl), result.GetType());
				}
			}
		}

		sealed class AsycudaModuleStripForTest : AsycudaModuleStrip
		{
			public Control GetCurrentFilterControlsForTest(ModuleFilter currentModuleFilter) => GetCurrentFilterControls(currentModuleFilter)[0];
		}
	}
}
