using System;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business.Customs.Manifest;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderWorkflowProviderTestCase : WorkflowProviderTest<AsycudaManifestHeader, ProcessTaskCollection<AsycudaManifestHeaderProcessTask, AsycudaManifestHeader>>
	{
		public void TestGetTemplateFilterCriteria_ForLoadPort()
		{
			Header.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(Header.AMA_RL_NKPortOfLoadingInfo, ProcessTaskTemplate.P0_LoadPortCountryInfo, "AUSYD", "MYPKG", ZString.Empty);
		}

		public void TestGetTemplateFilterCriteria_ForDischargePort()
		{
			Header.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(Header.AMA_RL_NKPortOfDischargeInfo, ProcessTaskTemplate.P0_DischargePortCountryInfo, "AUSYD", "MYPKG", ZString.Empty);
		}

		public void TestGetTemplateFilterCriteria_ForManifestType()
		{
			Header.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(Header.AMA_ManifestTypeInfo, ProcessTaskTemplate.P0_SubType3Info, "123", "ABC", ZString.Empty);
		}

		public void TestGetTemplateFilterCriteria_ForCountry()
		{
			Header.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(Header.AMA_RN_NKCountryInfo, ProcessTaskTemplate.P0_SubType4Info, "AU", "JP", ZString.Empty);
		}

		public void TestWorkflowItemsCreatedWhenStandAloneOrDecoupled()
		{
			IWorkflowProvider workflowProvider = Header;
			ProcessTaskTemplate.P0_ProcessType = workflowProvider.WorkflowType;
			ProcessTaskTemplate.P0_SubType4 = Core.Constants.CountryCodes.Australia;
			Header.Factory.Save();
			AssertEquals(0, workflowProvider.WorkflowItems.Count);
			AssertEquals("Is StandAlone", true, Header.IsStandAlone);

			using (ManifestCustomsDataRegistry.Instance.EnableManifestConsolDecoupling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Australia;
				Header.Factory.Save();
				AssertEquals("Creates workflow when StandAlone", 1, workflowProvider.WorkflowItems.Count);
			}

			using (ManifestCustomsDataRegistry.Instance.EnableManifestConsolDecoupling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
				workflowProvider.WorkflowItems.RemoveAndDeleteAll();
				Header.Factory.Save();

				Header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Australia;
				Header.Factory.Save();
				AssertEquals("Creates workflow when decoupled and standalone", 1, workflowProvider.WorkflowItems.Count);
			}

			var consol = Factory.New<ForwardingConsol>();
			Header.SetParent(consol);
			AssertEquals("Not StandAlone", false, Header.IsStandAlone);

			using (ManifestCustomsDataRegistry.Instance.EnableManifestConsolDecoupling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
				workflowProvider.WorkflowItems.RemoveAndDeleteAll();
				Header.Factory.Save();

				Header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Australia;
				Header.Factory.Save();
				AssertEquals("Not creates workflow when not StandAlone", 0, workflowProvider.WorkflowItems.Count);
			}

			using (ManifestCustomsDataRegistry.Instance.EnableManifestConsolDecoupling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
				workflowProvider.WorkflowItems.RemoveAndDeleteAll();
				Header.Factory.Save();

				Header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Australia;
				Header.Factory.Save();
				AssertEquals("Creates workflow when decoupled and not standalone", 1, workflowProvider.WorkflowItems.Count);
			}
		}

		protected override ZString ExpectedWorkflowType => AsycudaManifestWorkflowDescriptor.Constants.Code;

		AsycudaManifestHeader Header => BusinessObject;
	}
}
