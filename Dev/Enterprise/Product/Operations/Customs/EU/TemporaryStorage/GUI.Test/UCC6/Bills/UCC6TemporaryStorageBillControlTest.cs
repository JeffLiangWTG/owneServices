using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	sealed class UCC6TemporaryStorageBillControlTest : TestCaseWithFactory
	{
		public void TestTabPages()
		{
			using (var control = new UCC6TemporaryStorageBillControl())
			{
				var tabControl = control.FindSingle<ZTemplateTabControl>("LineDetailTabControl");
				AssertNotNull(tabControl);

				AssertEquals("Tab Pages count", 8, tabControl.TabCount);

				AssertContainsExactElementsInExactOrder("Tab Pages names", new[]
				{
					"BillDetailsTabPage",
					"PacksTabPage",
					"PackedItemsTabPage",
					"BillPartiesTabPage",
					"SupportingDocumentsTabPage",
					"PreviousDocumentsTabPage",
					"AdditionalInformationTabPage",
					"SupplyChainActorTabPage"
				}, tabControl.TabPages.ToList<ZTabPage>().Select(x => x.Name));
			}
		}

		[RequiresSTA]
		public void TestAdditionalInformationTabPage_WhenTabDisabled()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var temporaryStorageMock = new Mock<ITemporaryStorageLayoutProvider>();
			temporaryStorageMock.Setup(x => x.GetTemporaryStorageBillDetailTabLayout().IsAdditionalInformationTabVisible).Returns(false);
			var temporaryStorageConfiguration = TemporaryStorageConfigurationSetup(temporaryStorageMock);

			using (ObjectFactory.Substitute("UCC6TemporaryStorageLayoutProviders", temporaryStorageConfiguration))
			using (var control = new UCC6TemporaryStorageBillControl())
			{
				var tabControl = control.FindSingle<ZTemplateTabControl>("LineDetailTabControl");
				var additionalInfoTabPage = tabControl.GetTabPage("AdditionalInformationTabPage");

				control.SetDataBinding(header, string.Empty);
				AssertNotNull("AdditionalInformationTabVisible", additionalInfoTabPage);
				AssertEquals("AdditionalInformationTabPage TabVisible is false", false, additionalInfoTabPage.TabVisible);
			}
		}

		public void TestAdditionalInformationTabPage_WhenTabEnabled()
		{
			var temporaryStorageMock = new Mock<ITemporaryStorageLayoutProvider>();
			var temporaryStorageConfiguration = TemporaryStorageConfigurationSetup(temporaryStorageMock);
			temporaryStorageMock.Setup(x => x.GetTemporaryStorageBillDetailTabLayout().IsAdditionalInformationTabVisible).Returns(true);

			using (ObjectFactory.Substitute("UCC6TemporaryStorageLayoutProviders", temporaryStorageConfiguration))
			using (var control = new UCC6TemporaryStorageBillControl())
			{
				var tabControl = control.FindSingle<ZTemplateTabControl>("LineDetailTabControl");
				var additionalInfoTabPage = tabControl.GetTabPage("AdditionalInformationTabPage");
				var header = Factory.New<TemporaryStorageHeader>();
				control.SetDataBinding(header, string.Empty);
				AssertNotNull("When AdditionalInformationTabVisible is true", additionalInfoTabPage);
				AssertEquals("AdditionalInformationTabPage enabled when message type is not 'TF'", true, additionalInfoTabPage.TabVisible);

				header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
				AssertEquals("AdditionalInformationTabPage disabled when message type is 'TF'", false, additionalInfoTabPage.TabVisible);
			}
		}

		public void TestPackagesTabPage()
		{
			using (var control = new UCC6TemporaryStorageBillControl())
			{
				var tabControl = control.FindSingle<ZTemplateTabControl>("LineDetailTabControl");
				var additionalInfoTabPage = tabControl.GetTabPage("PacksTabPage");
				var ucc6TemporaryStoragePackagesControl = additionalInfoTabPage.FindSingle<UCC6TemporaryStoragePackagesControl>("UCC6TemporaryStoragePackagesControl");
				AssertNotNull(ucc6TemporaryStoragePackagesControl);
			}
		}

		public void TestSupplyChainActorTabPage()
		{
			using (var control = new UCC6TemporaryStorageBillControl())
			{
				CombineAssertions(() =>
				{
					var tabControl = control.FindSingle<ZTemplateTabControl>("LineDetailTabControl");
					var supplyChainActorTabPage = tabControl.GetTabPage("SupplyChainActorTabPage");
					var supplyChainActorTabUserControl = supplyChainActorTabPage.FindSingle<SupplyChainActorTabUserControl>("SupplyChainActorTabUserControl");
					AssertNotNull(supplyChainActorTabUserControl);

					var header = Factory.New<TemporaryStorageHeader>();
					header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
					control.SetDataBinding(header, string.Empty);
					tabControl.Refresh();

					AssertEquals("SupplyChainActorTabPage disabled when message type is 'TF'", false, supplyChainActorTabPage.TabVisible);
				});
			}
		}

		public void TestPreviousDocumentsTabPage()
		{
			using (var control = new UCC6TemporaryStorageBillControl())
			{
				CombineAssertions(() =>
				{
					var tabControl = control.FindSingle<ZTemplateTabControl>("LineDetailTabControl");
					var previousDocumentsTabPage = tabControl.GetTabPage("PreviousDocumentsTabPage");
					var previousDocumentsLayoutPanel = previousDocumentsTabPage.FindSingle<DynamicLayoutPanel>("PreviousDocumentsLayoutPanel");
					AssertNotNull(previousDocumentsLayoutPanel);

					var header = Factory.New<TemporaryStorageHeader>();
					header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
					control.SetDataBinding(header, string.Empty);
					tabControl.Refresh();

					AssertEquals("SupplyChainActoPreviousDocumentsTabPagerTabPage disabled when message type is 'TF'", false, previousDocumentsTabPage.TabVisible);
				});
			}
		}

		public void TestSupportingDocumentsTabPage_WhenTabDisabled()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var temporaryStorageMock = new Mock<ITemporaryStorageLayoutProvider>();
			temporaryStorageMock.Setup(x => x.GetTemporaryStorageBillDetailTabLayout().IsSupportingDocumentsTabVisible).Returns(false);
			var temporaryStorageConfiguration = TemporaryStorageConfigurationSetup(temporaryStorageMock);

			using (ObjectFactory.Substitute("UCC6TemporaryStorageLayoutProviders", temporaryStorageConfiguration))
			using (var control = new UCC6TemporaryStorageBillControl())
			{
				var tabControl = control.FindSingle<ZTemplateTabControl>("LineDetailTabControl");
				var supportingDocumentsTabPage = tabControl.GetTabPage("SupportingDocumentsTabPage");
				AssertNotNull("When SupportingDocumentsTabVisible is false", supportingDocumentsTabPage);

				control.SetDataBinding(header, string.Empty);
				AssertNotNull("SupportingDocumentsTabVisible", supportingDocumentsTabPage);
				AssertEquals("SupportingDocumentsTabPage TabVisible is false", false, supportingDocumentsTabPage.TabVisible);
			}
		}

		public void TestSupportingDocumentsTabPage_WhenTabEnabled()
		{
			var temporaryStorageMock = new Mock<ITemporaryStorageLayoutProvider>();
			var temporaryStorageConfiguration = TemporaryStorageConfigurationSetup(temporaryStorageMock);
			temporaryStorageMock.Setup(x => x.GetTemporaryStorageBillDetailTabLayout().IsSupportingDocumentsTabVisible).Returns(true);

			using (ObjectFactory.Substitute("UCC6TemporaryStorageLayoutProviders", temporaryStorageConfiguration))
			using (var control = new UCC6TemporaryStorageBillControl())
			{
				var tabControl = control.FindSingle<ZTemplateTabControl>("LineDetailTabControl");
				var supportingDocumentsTabPage = tabControl.GetTabPage("SupportingDocumentsTabPage");
				var header = Factory.New<TemporaryStorageHeader>();
				control.SetDataBinding(header, string.Empty);
				AssertNotNull("When SupportingDocumentsTabVisible is true", supportingDocumentsTabPage);
				AssertEquals("SupportingDocumentsTabPage enabled when message type is not 'TF'", true, supportingDocumentsTabPage.TabVisible);

				header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
				AssertEquals("SupportingDocumentsTabPage disabled when message type is 'TF'", false, supportingDocumentsTabPage.TabVisible);
			}
		}

		public void TestBillPartiesTabPage()
		{
			using (var control = new UCC6TemporaryStorageBillControl())
			{
				var tabControl = control.FindSingle<ZTemplateTabControl>("LineDetailTabControl");
				var billPartiesTabPage = tabControl.GetTabPage("BillPartiesTabPage");

				var header = Factory.New<TemporaryStorageHeader>();
				header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
				control.SetDataBinding(header, string.Empty);
				tabControl.Refresh();

				AssertEquals("BillPartiesTabPage disabled when message type is 'TF'", false, billPartiesTabPage.TabVisible);
			}
		}

		KeyObjectHandleDictionaryObject TemporaryStorageConfigurationSetup(Mock<ITemporaryStorageLayoutProvider> tempMock)
		{
			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(m => m.GetObject()).Returns(tempMock.Object);

			return new KeyObjectHandleDictionaryObject
			{
				{ GlbCompany.CurrentCompany.GC_RN_NKCountryCode, objectHandleMock.Object }
			};
		}
	}
}
