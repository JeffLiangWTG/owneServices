using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.Customs.ES.ExitControl.Business;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.ExitControl.GUI.Testing
{
	[TestedType(typeof(HeaderDetailsLayout))]
	public sealed class HeaderDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.ExitControl.GUI.HeaderDetailsLayoutBuilder<CusExitHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (EU.ExitControl.GUI.HeaderDetailsControlBag.Instance.BranchGuidFindBox, ControlWidthClass.Long);
				yield return (EU.ExitControl.GUI.HeaderDetailsControlBag.Instance.ExporterOrgAddressControl, ControlWidthClass.Long);
				yield return (EU.ExitControl.GUI.HeaderDetailsControlBag.Instance.CarrierAddressWithContactControl, ControlWidthClass.Auto);
				yield return (HeaderDetailsControlBag.Instance.BrokerCodeFindBox, ControlWidthClass.Auto);
				yield return (HeaderDetailsControlBag.Instance.CertificateDropEdit, ControlWidthClass.Auto);
				yield return (HeaderDetailsControlBag.Instance.TrainingCheckBox, ControlWidthClass.Long);
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
				var header = Factory.New<CusExitHeader>();
				AssertEquals("TrainingCheckBox not visible when not internal environment", false, LayoutForTesting.IsVisible(HeaderDetailsControlBag.Instance.TrainingCheckBox, header));
			}

			productRegistrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(true);
			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			{
				var header = Factory.New<CusExitHeader>();
				AssertEquals("TrainingCheckBox visible when internal environment", true, LayoutForTesting.IsVisible(HeaderDetailsControlBag.Instance.TrainingCheckBox, header));
			}
		}
	}
}
