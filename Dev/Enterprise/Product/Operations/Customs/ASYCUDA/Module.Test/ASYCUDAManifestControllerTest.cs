using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ApplicationCodeTypeList = Enterprise.Customs.ManifestBase.ApplicationCodeTypeList;

namespace Enterprise.Customs.ASYCUDA.Module.Testing
{
	[TestedType(typeof(ASYCUDAManifestController))]
	sealed class ASYCUDAManifestControllerTest : ZControllerBasherTest
	{
		public void TestGetNewBusinessEntityInLocalFactory()
		{
			AsycudaManifestHeaderLookupsTest.EnsureOrCreateRefZZRecordsToControlVisibilityBasedOnModeAndCountry(Factory, new ModeAndCountry("AIR", "BD"));
			var controller = new ASYCUDAManifestController();
			controller.Provider = ApplicationBusinessProvider.GetApplicationBusinessProviders(Factory, "BD").First();
			controller.CountryCode = "BD";
			controller.CreateVOC = true;
			using (var testForm = controller.ShowNewForm() as ZForm)
			{
				var header = testForm.BusinessEntity as AsycudaManifestHeader;
				AssertNotNull(header);
				AssertEquals(ApplicationCodeTypeList.Codes.ShippingLine, header.AMA_ApplicationCode);
			}
		}

		public void TestGetNewBusinessEntityInLocalFactory_CountryCodeNotDefaultedToEU()
		{
			AsycudaManifestHeaderLookupsTest.EnsureOrCreateRefZZRecordsToControlVisibilityBasedOnModeAndCountry(Factory, new ModeAndCountry("AIR", "EU"));
			var controller = new ASYCUDAManifestController();
			controller.Provider = ApplicationBusinessProvider.GetApplicationBusinessProviders(Factory, "EU").First();
			controller.CountryCode = "EU";
			controller.CreateVOC = true;
			using (var testForm = controller.ShowNewForm() as ZForm)
			{
				var header = testForm.BusinessEntity as AsycudaManifestHeader;
				AssertNotNull(header);
				AssertEquals(ZString.Empty, header.AMA_RN_NKCountry);
			}
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(AsycudaManifestHeader), new ASYCUDAManifestController().TypeOfTopLevelBusinessObject);
		}

		public void TestContainerModeDefaultForSG()
		{
			using (ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var controller = new ASYCUDAManifestController();
				var controllerInternal = (new ASYCUDAManifestController()) as ZControllerInternals;
				controller.Provider = ApplicationBusinessProvider.GetApplicationBusinessProviders(Factory, Core.Constants.CountryCodes.Singapore).First();
				controller.CountryCode = "SG";
				using (var testForm = controller.ShowNewForm() as ZForm)
				{
					var header = testForm.BusinessEntity as AsycudaManifestHeader;
					AssertNotNull(header);
					AssertEquals("AMA_ContainerMode should default for SG manifests", "OTH", header.AMA_ContainerMode);
				}
			}
		}

		public void TestShowEditFormFromViewManifestToManifest()
		{
			Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header2.AMA_JobReference = "C123457";
			Factory.Save();
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.ASYCUDA.Manifest))
			{
				var viewCollection = GetSortedModuleCollection(module);
				AssertEquals(2, viewCollection.Count);
				var viewToEdit = (AsycudaManifestHeader)module.GridCollection.FindByPK(viewCollection[0].PK);
				using (var form1 = ((IFilterGridModuleInternalsForTesting)module).ShowEditForm(viewToEdit))
				{
					AssertCorrectFormAndBussinessObjectLoadedAndMoveNext(form1, viewCollection);
					using (var form2 = OpenedFormCache.GetInstance().GetForm(header2.PK.ToGuid(), ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest.ToString()))
					{
						var form2Entity = ((IZForm)form2).BusinessEntityForPersistingForm as AsycudaManifestHeader;
						AssertEquals(viewCollection[1].PK, form2Entity.PK);
					}
				}
			}
		}

		public void TestShowEditFormFromViewManifestToConsol()
		{
			var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header1.AMA_JobReference = "123457";
			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header2.SetParent(consol1);
			Factory.Save();
			AssertEquals("AMA_JobReference can be manually set when Consol is null", "123457", header1.AMA_JobReference);
			AssertEquals("AMA_JobReference is auto generated based on consol1.JK_UniqueConsignRef when Consol is not null", "26FYS0PM3GCY3VHFAX26_1", header2.AMA_JobReference);
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.ASYCUDA.Manifest))
			{
				var viewCollection = GetSortedModuleCollection(module);
				AssertEquals(2, viewCollection.Count);
				var viewToEdit = (AsycudaManifestHeader)module.GridCollection.FindByPK(viewCollection[0].PK);
				AssertEquals("Make sure the first element of viewCollection is header1", header1.PK, viewCollection[0].PK);
				using (var form1 = ((IFilterGridModuleInternalsForTesting)module).ShowEditForm(viewToEdit))
				{
					AssertCorrectFormAndBussinessObjectLoadedAndMoveNext(form1, viewCollection);
					using (var form2 = OpenedFormCache.GetInstance().GetForm(consol1.PK.ToGuid(), ControllerIDs.JobConsol.ToString()))
					{
						var form2Entity = ((IZForm)form2).BusinessEntityForPersistingForm as ForwardingConsol;
						var query = new ZQuery(AsycudaManifestHeaderSchema.AMA_ParentId, form2Entity.PK);
						query.AddToFilter(AsycudaManifestHeaderSchema.AMA_ParentTableCode, JobConsolSchema.Constants.Prefix);
						var consolManifestHeader = Factory.LoadTop1<AsycudaManifestHeader>(query);
						AssertEquals(viewCollection[1].PK, consolManifestHeader.PK);
					}
				}
			}
		}

		public override void TestNewForm()
		{
			Assert("This New Form option is not used for ASYCUDA", true);
		}

		public override Type ControllerToBashType => typeof(ASYCUDAManifestController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			header.FillWithValidTestData();
			Factory.Save();
			return header;
		}

		ZPKCollection GetSortedModuleCollection(ZFilterGridModule module)
		{
			((IFilterGridModuleInternalsForTesting)module).PerformSearch();
			((IFilterGridModuleInternalsForTesting)module).GridCollection.ApplySort(new SortInfo(AsycudaManifestHeader.Schema.AMA_JobReference, ListSortDirection.Ascending));
			Application.DoEvents();
			return ZModuleResults.Instance.GetPKCollectionForModule(ModuleIDs.Customs.ASYCUDA.Manifest);
		}

		void AssertCorrectFormAndBussinessObjectLoadedAndMoveNext(IZForm form1, ZPKCollection moduleCollection)
		{
			var formEntity = form1.BusinessEntityForPersistingForm as AsycudaManifestHeader;
			AssertEquals(moduleCollection[0].PK, formEntity.PK);
			Application.DoEvents();
			var formPreviousNext = (IPreviousNextControlProvider)form1;
			formPreviousNext.PreviousNextControlForTesting.FireNextButtonForTesting();
			Application.DoEvents();
		}
	}
}
