using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.EU.TemporaryStorage.GUI;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(G5V1TemporaryStorageLayout))]
	sealed class G5V1TemporaryStorageLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (EUControlBagInstance.CountryCodeFindBox, ControlWidthClass.Medium);
				yield return (EUControlBagInstance.MessageTypeDropEdit, ControlWidthClass.Long);
				yield return (ESControlBagInstance.IsSimplifiedCheckBox, ControlWidthClass.Long);
				yield return (EUControlBagInstance.TransportModeDropEdit, ControlWidthClass.Long);
				yield return (EUControlBagInstance.TransportTypeDropEdit, ControlWidthClass.Long);
				yield return (EUControlBagInstance.ArrivalTransportMeansCodeTextBox, ControlWidthClass.Medium);
				yield return (EUControlBagInstance.DeclarantAddressControl, ControlWidthClass.Long);
				yield return (EUControlBagInstance.RepresentativeAddressControl, ControlWidthClass.Long);
				yield return (EUControlBagInstance.SupervisingCustomsOfficeCodeFindBox, ControlWidthClass.Long);
				yield return (EUControlBagInstance.LocationOfGoodsUserControl, ControlWidthClass.Long);
				yield return (ESControlBagInstance.DestinationCustomsOfficeCodeFindBox, ControlWidthClass.Long);
				yield return (ESControlBagInstance.DestinationLocationOfGoodsUserControl, ControlWidthClass.Long);
				yield return (ESControlBagInstance.ManualLocationOfGoodsCodeFindBox, ControlWidthClass.Long);
				yield return (ESControlBagInstance.TransportDocumentTypeDropEdit, ControlWidthClass.Long);
				yield return (ESControlBagInstance.TransportDocumentTextBox, ControlWidthClass.Long);
				yield return (ESControlBagInstance.UnionGoodsCheckBox, ControlWidthClass.Long);
				yield return (EUControlBagInstance.AuthorizationTypeDropEdit, ControlWidthClass.Long);
				yield return (EUControlBagInstance.AuthorizationOwnerGuidFindBox, ControlWidthClass.Long);
				yield return (EUControlBagInstance.AuthorizationNumberCodeFindBox, ControlWidthClass.Long);
				yield return (EUControlBagInstance.HasHouseConsignmentCheckBox, ControlWidthClass.Long);
				yield return (ESControlBagInstance.MovementOfContainersOnlyCheckBox, ControlWidthClass.Long);
				yield return (EUControlBagInstance.CusAgentCodeFindBox, ControlWidthClass.Long);
				yield return (ESControlBagInstance.CertificateDropEdit, ControlWidthClass.Long);
				yield return (ESControlBagInstance.TrainingCheckBox, ControlWidthClass.Long);
			}
		}

		public void TestVisibilityOfLocationOfGoodsControls()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var layout = ((IPanelLayoutProvider)new G5V1TemporaryStorageLayout()).Layout;

			CombineAssertions(() =>
			{
				header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
				AssertEquals("DestinationLocationOfGoodsUserControl should be visible", true, LayoutForTesting.IsVisible(G5V1TemporaryStorageUserControlBag.Instance.DestinationLocationOfGoodsUserControl, header));
				AssertEquals("ManualLocationOfGoodsCodeFindBox should be hidden", false, LayoutForTesting.IsVisible(G5V1TemporaryStorageUserControlBag.Instance.ManualLocationOfGoodsCodeFindBox, header));

				header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
				AssertEquals("DestinationLocationOfGoodsUserControl should be visible", true, LayoutForTesting.IsVisible(G5V1TemporaryStorageUserControlBag.Instance.DestinationLocationOfGoodsUserControl, header));
				AssertEquals("ManualLocationOfGoodsCodeFindBox should be hidden", false, LayoutForTesting.IsVisible(G5V1TemporaryStorageUserControlBag.Instance.ManualLocationOfGoodsCodeFindBox, header));

				header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
				AssertEquals("DestinationLocationOfGoodsUserControl should be hidden", false, LayoutForTesting.IsVisible(G5V1TemporaryStorageUserControlBag.Instance.DestinationLocationOfGoodsUserControl, header));
				AssertEquals("ManualLocationOfGoodsCodeFindBox should be visible", true, LayoutForTesting.IsVisible(G5V1TemporaryStorageUserControlBag.Instance.ManualLocationOfGoodsCodeFindBox, header));

				header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
				AssertEquals("DestinationLocationOfGoodsUserControl should be hidden", false, LayoutForTesting.IsVisible(G5V1TemporaryStorageUserControlBag.Instance.DestinationLocationOfGoodsUserControl, header));
				AssertEquals("ManualLocationOfGoodsCodeFindBox should be visible", true, LayoutForTesting.IsVisible(G5V1TemporaryStorageUserControlBag.Instance.ManualLocationOfGoodsCodeFindBox, header));
			});
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (ESControlBagInstance.DeclarationDetailsGroupBox, ControlWidthClass.Auto);
				yield return (ESControlBagInstance.GuaranteeGroupBox, ControlWidthClass.Auto);
				yield return (ESControlBagInstance.DocumentsTabControl, ControlWidthClass.Auto);
			}
		}

		public void TestTrainingCheckBoxVisibility()
		{
			var productRegistrationMock = new Mock<IProductRegistration>();
			productRegistrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(false);
			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			{
				var header = Factory.New<TemporaryStorageHeader>();
				header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
				AssertEquals("TrainingCheckBox not visible when not internal environment", false, LayoutForTesting.IsVisible(G5V1TemporaryStorageUserControlBag.Instance.TrainingCheckBox, header));
			}

			productRegistrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(false);
			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			{
				var header = Factory.New<TemporaryStorageHeader>();
				header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
				AssertEquals("TrainingCheckBox not visible when not internal environment and LAM", false, LayoutForTesting.IsVisible(G5V1TemporaryStorageUserControlBag.Instance.TrainingCheckBox, header));
			}

			productRegistrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(true);
			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			{
				var header = Factory.New<TemporaryStorageHeader>();
				header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
				AssertEquals("TrainingCheckBox visible when internal environment", true, LayoutForTesting.IsVisible(G5V1TemporaryStorageUserControlBag.Instance.TrainingCheckBox, header));
			}

			productRegistrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(true);
			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			{
				var header = Factory.New<TemporaryStorageHeader>();
				header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
				AssertEquals("TrainingCheckBox not visible when internal environment and LAM", false, LayoutForTesting.IsVisible(G5V1TemporaryStorageUserControlBag.Instance.TrainingCheckBox, header));
			}
		}

		public void TestIsMessageTypeManualFiedsVisibilty()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			CombineAssertions(() =>
			{
				header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
				AssertEquals("DeclarantAddressControl when AMA_MessageType = G5X", true, LayoutForTesting.IsVisible(EUControlBagInstance.DeclarantAddressControl, header));
				AssertEquals("RepresentativeAddressControl when AMA_MessageType = G5X", true, LayoutForTesting.IsVisible(EUControlBagInstance.RepresentativeAddressControl, header));
				AssertEquals("SupervisingCustomsOfficeCodeFindBox when AMA_MessageType = G5X", true, LayoutForTesting.IsVisible(EUControlBagInstance.SupervisingCustomsOfficeCodeFindBox, header));
				AssertEquals("LocationOfGoodsUserControl when AMA_MessageType = G5X", true, LayoutForTesting.IsVisible(EUControlBagInstance.LocationOfGoodsUserControl, header));
				AssertEquals("DestinationCustomsOfficeCodeFindBox when AMA_MessageType = G5X", true, LayoutForTesting.IsVisible(ESControlBagInstance.DestinationCustomsOfficeCodeFindBox, header));
				AssertEquals("AuthorizationTypeDropEdit when AMA_MessageType = G5X", true, LayoutForTesting.IsVisible(EUControlBagInstance.AuthorizationTypeDropEdit, header));
				AssertEquals("AuthorizationOwnerGuidFindBox when AMA_MessageType = G5X", true, LayoutForTesting.IsVisible(EUControlBagInstance.AuthorizationOwnerGuidFindBox, header));
				AssertEquals("AuthorizationNumberCodeFindBox when AMA_MessageType = G5X", true, LayoutForTesting.IsVisible(EUControlBagInstance.AuthorizationNumberCodeFindBox, header));
				AssertEquals("CusAgentCodeFindBox when AMA_MessageType = G5X", true, LayoutForTesting.IsVisible(EUControlBagInstance.CusAgentCodeFindBox, header));
				AssertEquals("CertificateDropEdit when AMA_MessageType = G5X", true, LayoutForTesting.IsVisible(ESControlBagInstance.CertificateDropEdit, header));
				AssertEquals("TrainingCheckBox when AMA_MessageType = G5X", true, LayoutForTesting.IsVisible(ESControlBagInstance.TrainingCheckBox, header));
				AssertEquals("DocumentsTabControl when AMA_MessageType = G5X", true, LayoutForTesting.IsVisible(ESControlBagInstance.DocumentsTabControl, header));
				AssertEquals("GuaranteeGroupBox when AMA_MessageType = G5X", true, LayoutForTesting.IsVisible(ESControlBagInstance.GuaranteeGroupBox, header));
				AssertEquals("UnionGoodsCheckBox when AMA_MessageType = G5X", false, LayoutForTesting.IsVisible(ESControlBagInstance.UnionGoodsCheckBox, header));
				AssertEquals("HasHouseConsignmentCheckBox when AMA_MessageType = G5X", false, LayoutForTesting.IsVisible(EUControlBagInstance.HasHouseConsignmentCheckBox, header));
				AssertEquals("IsSimplifiedCheckBox when AMA_MessageType = G5X", false, LayoutForTesting.IsVisible(ESControlBagInstance.IsSimplifiedCheckBox, header));
				AssertEquals("MovementOfContainersOnlyCheckBox when AMA_MessageType = G5X", false, LayoutForTesting.IsVisible(ESControlBagInstance.MovementOfContainersOnlyCheckBox, header));

				header.AMA_MessageType = "G5R";
				AssertEquals("HasHouseConsignmentCheckBox when AMA_MessageType = G5R", true, LayoutForTesting.IsVisible(EUControlBagInstance.HasHouseConsignmentCheckBox, header));
				AssertEquals("IsSimplifiedCheckBox when AMA_MessageType = G5R", true, LayoutForTesting.IsVisible(ESControlBagInstance.IsSimplifiedCheckBox, header));
				AssertEquals("MovementOfContainersOnlyCheckBox when AMA_MessageType = G5R", true, LayoutForTesting.IsVisible(ESControlBagInstance.MovementOfContainersOnlyCheckBox, header));

				header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
				AssertEquals("DeclarantAddressControl when AMA_MessageType = LAM", false, LayoutForTesting.IsVisible(EUControlBagInstance.DeclarantAddressControl, header));
				AssertEquals("RepresentativeAddressControl when AMA_MessageType = LAM", false, LayoutForTesting.IsVisible(EUControlBagInstance.RepresentativeAddressControl, header));
				AssertEquals("SupervisingCustomsOfficeCodeFindBox when AMA_MessageType = LAM", false, LayoutForTesting.IsVisible(EUControlBagInstance.SupervisingCustomsOfficeCodeFindBox, header));
				AssertEquals("LocationOfGoodsUserControl when AMA_MessageType = LAM", false, LayoutForTesting.IsVisible(EUControlBagInstance.LocationOfGoodsUserControl, header));
				AssertEquals("DestinationCustomsOfficeCodeFindBox when AMA_MessageType = LAM", false, LayoutForTesting.IsVisible(ESControlBagInstance.DestinationCustomsOfficeCodeFindBox, header));
				AssertEquals("AuthorizationTypeDropEdit when AMA_MessageType = LAM", false, LayoutForTesting.IsVisible(EUControlBagInstance.AuthorizationTypeDropEdit, header));
				AssertEquals("AuthorizationOwnerGuidFindBox when AMA_MessageType = LAM", false, LayoutForTesting.IsVisible(EUControlBagInstance.AuthorizationOwnerGuidFindBox, header));
				AssertEquals("AuthorizationNumberCodeFindBox when AMA_MessageType = LAM", false, LayoutForTesting.IsVisible(EUControlBagInstance.AuthorizationNumberCodeFindBox, header));
				AssertEquals("CusAgentCodeFindBox when AMA_MessageType = LAM", false, LayoutForTesting.IsVisible(EUControlBagInstance.CusAgentCodeFindBox, header));
				AssertEquals("CertificateDropEdit when AMA_MessageType = LAM", false, LayoutForTesting.IsVisible(ESControlBagInstance.CertificateDropEdit, header));
				AssertEquals("TrainingCheckBox when AMA_MessageType = LAM", false, LayoutForTesting.IsVisible(ESControlBagInstance.TrainingCheckBox, header));
				AssertEquals("DocumentsTabControl when AMA_MessageType = LAM", false, LayoutForTesting.IsVisible(ESControlBagInstance.DocumentsTabControl, header));
				AssertEquals("GuaranteeGroupBox when AMA_MessageType = LAM", false, LayoutForTesting.IsVisible(ESControlBagInstance.GuaranteeGroupBox, header));
				AssertEquals("UnionGoodsCheckBox when AMA_MessageType = LAM", false, LayoutForTesting.IsVisible(ESControlBagInstance.UnionGoodsCheckBox, header));
				AssertEquals("HasHouseConsignmentCheckBox when AMA_MessageType = LAM", false, LayoutForTesting.IsVisible(EUControlBagInstance.HasHouseConsignmentCheckBox, header));
				AssertEquals("IsSimplifiedCheckBox when AMA_MessageType = LAM", false, LayoutForTesting.IsVisible(ESControlBagInstance.IsSimplifiedCheckBox, header));
				AssertEquals("MovementOfContainersOnlyCheckBox when AMA_MessageType = LAM", false, LayoutForTesting.IsVisible(ESControlBagInstance.MovementOfContainersOnlyCheckBox, header));

				header.AMA_MessageType = "G5E";
				AssertEquals("HasHouseConsignmentCheckBox when AMA_MessageType = G5E", true, LayoutForTesting.IsVisible(EUControlBagInstance.HasHouseConsignmentCheckBox, header));
				AssertEquals("IsSimplifiedCheckBox when AMA_MessageType = G5E", true, LayoutForTesting.IsVisible(ESControlBagInstance.IsSimplifiedCheckBox, header));
				AssertEquals("MovementOfContainersOnlyCheckBox when AMA_MessageType = G5E", true, LayoutForTesting.IsVisible(ESControlBagInstance.MovementOfContainersOnlyCheckBox, header));

				header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
				AssertEquals("GuaranteeGroupBox when AMA_MessageType = TSM", true, LayoutForTesting.IsVisible(ESControlBagInstance.GuaranteeGroupBox, header));
				AssertEquals("UnionGoodsCheckBox when AMA_MessageType = TSM", true, LayoutForTesting.IsVisible(ESControlBagInstance.UnionGoodsCheckBox, header));
				AssertEquals("CusAgentCodeFindBox when AMA_MessageType = TSM", true, LayoutForTesting.IsVisible(EUControlBagInstance.CusAgentCodeFindBox, header));
				AssertEquals("CertificateDropEdit when AMA_MessageType = TSM", true, LayoutForTesting.IsVisible(ESControlBagInstance.CertificateDropEdit, header));
				AssertEquals("TrainingCheckBox when AMA_MessageType = TSM", true, LayoutForTesting.IsVisible(ESControlBagInstance.TrainingCheckBox, header));
				AssertEquals("HasHouseConsignmentCheckBox when AMA_MessageType = TSM", false, LayoutForTesting.IsVisible(EUControlBagInstance.HasHouseConsignmentCheckBox, header));
				AssertEquals("IsSimplifiedCheckBox when AMA_MessageType = TSM", false, LayoutForTesting.IsVisible(ESControlBagInstance.IsSimplifiedCheckBox, header));
				AssertEquals("MovementOfContainersOnlyCheckBox when AMA_MessageType = TSM", false, LayoutForTesting.IsVisible(ESControlBagInstance.MovementOfContainersOnlyCheckBox, header));
			});
		}

		TemporaryStorageUserControlBag EUControlBagInstance => TemporaryStorageUserControlBag.Instance;
		G5V1TemporaryStorageUserControlBag ESControlBagInstance => G5V1TemporaryStorageUserControlBag.Instance;

		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new TemporaryStorageLayoutBuilder<EU.Business.CusTempStorage.TemporaryStorageHeader>();
	}
}
