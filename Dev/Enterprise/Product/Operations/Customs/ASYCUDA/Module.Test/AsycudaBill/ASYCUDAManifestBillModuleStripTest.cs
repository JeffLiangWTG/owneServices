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
	sealed class ASYCUDAManifestBillModuleStripTest : ModuleTextFilterTest
	{
		public void TestGetCurrentFilterControls()
		{
			using (var filterStrip = new ASYCUDAManifestBillModuleStripForTest())
			{
				filterStrip.SetDataBinding(new ZArchitecture.Business.Internal.FilterStrip(new ModuleFilterCollection()), "");
				using (var result = filterStrip.GetCurrentFilterControlsForTest(new CountryRelatedFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.ManifestMsgStatus, (val1, val2) => new ZQuery(), Factory, ZArchitecture.FieldType.TextDropEdit, 3, CountryRelatedFilterHelper.ListGetters.MessageStatusGetter)))
				{
					AssertEquals(typeof(CountryRelatedFilterControl), result.GetType());
				}

				using (var result = filterStrip.GetCurrentFilterControlsForTest(new CountryRelatedFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.BillMsgStatus, (val1, val2) => new ZQuery(), Factory, ZArchitecture.FieldType.TextDropEdit, 3, CountryRelatedFilterHelper.ListGetters.MessageStatusGetter)))
				{
					AssertEquals(typeof(CountryRelatedFilterControl), result.GetType());
				}

				using (var result = filterStrip.GetCurrentFilterControlsForTest(new DataGroupingRelatedFilter(ASYCUDAManifestBillFilterStrip.FilterConstants.ManifestType, (val1, val2) => new ZQuery(), Factory, ZArchitecture.FieldType.TextDropEdit, 3, NoResourceStringData.GetData(ASYCUDAManifestBillFilterStrip.FilterConstants.ManifestType), ApplicationBusinessProvider.GetManifestTypeListByCountry)))
				{
					AssertEquals(typeof(DataGroupingRelatedFilterControl), result.GetType());
				}
			}
		}

		sealed class ASYCUDAManifestBillModuleStripForTest : ASYCUDAManifestBillModuleStrip
		{
			public Control GetCurrentFilterControlsForTest(ModuleFilter currentModuleFilter) => GetCurrentFilterControls(currentModuleFilter)[0];
		}
	}
}
