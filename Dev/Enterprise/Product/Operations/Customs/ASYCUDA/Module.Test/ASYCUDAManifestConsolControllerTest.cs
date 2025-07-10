using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Module.Testing
{
	[TestedType(typeof(ASYCUDAManifestConsolController))]
	sealed class ASYCUDAManifestConsolControllerTest : ZControllerBasherTest
	{
		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(ForwardingConsol), new ASYCUDAManifestConsolController().TypeOfTopLevelBusinessObject);
		}

		[TestDate(2017, 9, 15)]
		public void TestShowAndEditViewFormWithCountryTabSelected()
		{
			var cusCodeList1 = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList1.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry;
			cusCodeList1.ZZD_Code = Core.Constants.CountryCodes.SouthAfrica;
			cusCodeList1.ZZD_Description = "South Africa";
			cusCodeList1.ZZD_CountryOrGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping;
			cusCodeList1.ZZD_StartDate = new ZDateTime(2017, 7, 1);
			cusCodeList1.ZZD_EndDate = new ZDateTime(2017, 12, 1);
			cusCodeList1.Attributes.AddNew("NVC", "17.3.29.001");
			var cusCodeList2 = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList2.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry;
			cusCodeList2.ZZD_Code = Core.Constants.CountryCodes.Fiji;
			cusCodeList2.ZZD_Description = "Fiji";
			cusCodeList2.ZZD_CountryOrGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping;
			cusCodeList2.ZZD_StartDate = new ZDateTime(2017, 7, 1);
			cusCodeList2.ZZD_EndDate = new ZDateTime(2017, 12, 1);
			cusCodeList2.Attributes.AddNew("NVC", "17.3.29.002");
			Factory.Save();
			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_RL_NKLoadPort = "ZAAAA";
			consol1.JK_RL_NKDischargePort = "FJAAA";
			consol1.JK_TransportMode = "AIR";
			var header1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			header1.SetParent(consol1);
			var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header2.SetParent(consol1);
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			header2.AMA_JobReference = "C123457";
			Factory.Save();
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.ASYCUDA.Manifest))
			{
				var viewCollection = GetSortedModuleCollection(module);
				AsycudaManifestHeader view1 = null;
				AsycudaManifestHeader view2 = null;
				foreach (var item in module.GridCollection)
				{
					var view = item as AsycudaManifestHeader;
					if (view.PK == header1.PK)
					{
						view1 = view;
					}
					else if (view.PK == header2.PK)
					{
						view2 = view;
					}
				}

				AssertNotNull(view1);
				AssertNotNull(view2);
				using (var form = ((IFilterGridModuleInternalsForTesting)module).ShowViewForm(view1))
				{
					AssertSelectedTabPage(form, header1.AMA_RN_NKCountry, header1.AMA_ManifestType);
				}

				using (var form = ((IFilterGridModuleInternalsForTesting)module).ShowEditForm(view1))
				{
					AssertSelectedTabPage(form, header1.AMA_RN_NKCountry, header1.AMA_ManifestType);
				}

				using (var form = ((IFilterGridModuleInternalsForTesting)module).ShowViewForm(view2))
				{
					AssertSelectedTabPage(form, header2.AMA_RN_NKCountry, header2.AMA_ManifestType);
				}

				using (var form = ((IFilterGridModuleInternalsForTesting)module).ShowEditForm(view2))
				{
					AssertSelectedTabPage(form, header2.AMA_RN_NKCountry, header2.AMA_ManifestType);
				}
			}
		}

		public void TestShowEditFormFromConsolToConsol()
		{
			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header1.SetParent(consol1);
			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header2.SetParent(consol2);
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
					using (var form2 = OpenedFormCache.GetInstance().GetForm(consol2.PK.ToGuid(), ControllerIDs.JobConsol.ToString()))
					{
						var form2Entity = ((IZForm)form2).BusinessEntityForPersistingForm as ForwardingConsol;
						var query2 = new ZQuery(AsycudaManifestHeaderSchema.AMA_ParentId, form2Entity.PK);
						query2.AddToFilter(AsycudaManifestHeaderSchema.AMA_ParentTableCode, JobConsolSchema.Constants.Prefix);
						var consol2ManifestHeader = Factory.LoadTop1<AsycudaManifestHeader>(query2);
						AssertEquals(viewCollection[1].PK, consol2ManifestHeader.PK);
					}
				}
			}
		}

		public void TestShowEditFormFromConsolToManifest()
		{
			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header1.SetParent(consol1);
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

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		public override Type ControllerToBashType => typeof(ASYCUDAManifestConsolController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.ASYCUDA.ASYCUDAManifestConsol;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			header.SetParent(consol);
			Factory.Save();
			return consol;
		}

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			header.FillWithValidTestData();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			header.SetParent(consol);
			header.AMA_JobReference = "C123457";
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.BreakBulk;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "123";
			consol.Transports[0].JW_TransportMode = Core.Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";
			return consol;
		}

		void AssertSelectedTabPage(IZForm form, string countryCode, string manifestType)
		{
			var consolForm = form as ConsolForm;
			AssertNotNull(consolForm);
			var control = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest).UserControl;
			var tabControl = (ZTemplateTabControl)control.Controls.Find("MainTabControl", false).First();
			AssertEquals(3, tabControl.TabCount);
			AssertEquals(countryCode + manifestType + "TabPage", tabControl.SelectedTab.Name);
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
			var form1Entity = form1.BusinessEntityForPersistingForm as ForwardingConsol;
			var query = new ZQuery(AsycudaManifestHeaderSchema.AMA_ParentId, form1Entity.PK);
			query.AddToFilter(AsycudaManifestHeaderSchema.AMA_ParentTableCode, JobConsolSchema.Constants.Prefix);
			var consol1ManifestHeader = Factory.LoadTop1<AsycudaManifestHeader>(query);
			AssertEquals(moduleCollection[0].PK, consol1ManifestHeader.PK);
			Application.DoEvents();
			var formPreviousNext = (IPreviousNextControlProvider)form1;
			formPreviousNext.PreviousNextControlForTesting.FireNextButtonForTesting();
			Application.DoEvents();
		}
	}
}
