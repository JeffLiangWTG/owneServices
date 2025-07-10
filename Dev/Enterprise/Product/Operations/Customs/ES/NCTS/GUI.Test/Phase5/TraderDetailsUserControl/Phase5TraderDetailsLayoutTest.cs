using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.ES.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5TraderDetailsLayout))]
	sealed class Phase5TraderDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new TraderDetailsLayoutBuilder<NctsHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				var euBag = EU.NCTS.GUI.TraderDetailsControlBag.Instance;
				var esBag = TraderDetailsControlBag.Instance;
				yield return (euBag.PrincipalDocAddressControl, ControlWidthClass.LongControl);
				yield return (euBag.ConsignorDocAddressControl, ControlWidthClass.LongControl);
				yield return (euBag.ConsigneeDocAddressControl, ControlWidthClass.LongControl);
				yield return (euBag.RepresentativeDocAddressControl, ControlWidthClass.LongControl);
				yield return (esBag.BrokerCodeFindBox, ControlWidthClass.Long);
				yield return (esBag.CertificateDropEdit, ControlWidthClass.Long);
				yield return (esBag.TrainingCheckBox, ControlWidthClass.Long);
			}
		}

		public void TestAddControlBehaviour()
		{
			AssertEquals("ZDocAddressControl", true, LayoutForTesting.HasBehaviourByBehaviourType(EU.NCTS.GUI.TraderDetailsControlBag.Instance.RepresentativeDocAddressControl, typeof(DocAddressControlDisplayModeCompactWithOverrideBehaviour)));
		}

		public void TestTrainingCheckBoxVisibility()
		{
			var productRegistrationMock = new Mock<IProductRegistration>();
			var keyMock = new Mock<IProductRegistrationKey>();
			productRegistrationMock.Setup(m => m.Key).Returns(keyMock.Object);

			productRegistrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(false);
			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			{
				var header = Factory.New<NctsHeader>();
				AssertEquals("TrainingCheckBox not visible when not internal environment", false, LayoutForTesting.IsVisible(TraderDetailsControlBag.Instance.TrainingCheckBox, header));
			}

			productRegistrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(true);
			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			{
				CombineAssertions(() =>
				{
					var header = Factory.New<NctsHeader>();
					header.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
					AssertEquals("TrainingCheckBox visible when internal environment and BM_Phase is not TNN", true, LayoutForTesting.IsVisible(TraderDetailsControlBag.Instance.TrainingCheckBox, header));

					header.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
					AssertEquals("TrainingCheckBox not visible when internal environment but BM_Phase is TNN", false, LayoutForTesting.IsVisible(TraderDetailsControlBag.Instance.TrainingCheckBox, header));
				});
			}
		}

		public void TestBrokerCodeFindBoxVisibility()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<NctsHeader>();
				header.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
				AssertEquals("BrokerCodeFindBox visible when internal environment and BM_Phase is not TNN", true, LayoutForTesting.IsVisible(TraderDetailsControlBag.Instance.BrokerCodeFindBox, header));

				header.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
				AssertEquals("BrokerCodeFindBox not visible when internal environment but BM_Phase is TNN", false, LayoutForTesting.IsVisible(TraderDetailsControlBag.Instance.BrokerCodeFindBox, header));
			});
		}

		public void TestCertificateDropEditVisibility()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<NctsHeader>();
				header.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
				AssertEquals("TrainingCheckBox visible when internal environment and BM_Phase is not TNN", true, LayoutForTesting.IsVisible(TraderDetailsControlBag.Instance.TrainingCheckBox, header));

				header.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
				AssertEquals("TrainingCheckBox not visible when internal environment but BM_Phase is TNN", false, LayoutForTesting.IsVisible(TraderDetailsControlBag.Instance.TrainingCheckBox, header));
			});
		}
	}
}
