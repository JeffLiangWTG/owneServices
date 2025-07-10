using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	[TestedType(typeof(ArrivalNotificationDetailsLayout))]
	sealed class Phase5ArrivalNotificationDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ArrivalNotificationDetailsLayoutBuilder<NctsHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (EU.NCTS.GUI.ArrivalNotificationDetailsControlBag.Instance.OverrideFreightDetailsCheckBox, ControlWidthClass.Auto);
				yield return (EU.NCTS.GUI.ArrivalNotificationDetailsControlBag.Instance.LocalReferenceNumberTextBox, ControlWidthClass.Long);
				yield return (EU.NCTS.GUI.ArrivalNotificationDetailsControlBag.Instance.MrnTextBox, ControlWidthClass.Long);
				yield return (EU.NCTS.GUI.ArrivalNotificationDetailsControlBag.Instance.DestinationCustomsOfficeCodeCodeFindBox, ControlWidthClass.Long);
				yield return (EU.NCTS.GUI.ArrivalNotificationDetailsControlBag.Instance.AuthorizationCodeDropEdit, ControlWidthClass.Long);
				yield return (EU.NCTS.GUI.ArrivalNotificationDetailsControlBag.Instance.NumberCodeFindBox, ControlWidthClass.Long);
				yield return (ArrivalNotificationDetailsControlBag.Instance.ArrivalGoodsLocationZCodeFindBox, ControlWidthClass.Long);
				yield return (EU.NCTS.GUI.ArrivalNotificationDetailsControlBag.Instance.IncidentFlagDropEdit, ControlWidthClass.Long);
				yield return (ArrivalNotificationDetailsControlBag.Instance.AdditionalArrivalNotificationDetailsUserControl, ControlWidthClass.LongNoCaption);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (EU.NCTS.GUI.ArrivalNotificationDetailsControlBag.Instance.DestinationTraderDocAddressControl, ControlWidthClass.Auto);
				yield return (ArrivalNotificationDetailsControlBag.Instance.RepresentativeTraderZDocAddressControl, ControlWidthClass.Auto);
				yield return (ArrivalNotificationDetailsControlBag.Instance.BrokerCodeFindBox, ControlWidthClass.Auto);
				yield return (ArrivalNotificationDetailsControlBag.Instance.CertificateDropEdit, ControlWidthClass.Auto);
				yield return (ArrivalNotificationDetailsControlBag.Instance.TrainingCheckBox, ControlWidthClass.Long);
			}
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
				AssertEquals("TrainingCheckBox not visible when not internal environment", false, LayoutForTesting.IsVisible(ArrivalNotificationDetailsControlBag.Instance.TrainingCheckBox, header));
			}

			productRegistrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(true);
			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			{
				var header = Factory.New<NctsHeader>();
				AssertEquals("TrainingCheckBox visible when internal environment", true, LayoutForTesting.IsVisible(ArrivalNotificationDetailsControlBag.Instance.TrainingCheckBox, header));
			}
		}
	}
}
