using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ControlDependencyObserverTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ControlDependencyObserver(controlHost: null));
		}

		[ExpectNoExceptions]
		public void TestControlBahaviourDependencyObservers()
		{
			var controlHostMock = new Mock<IControlHost>();
			var bo = Factory.New<DummyBusinessObject>();
			var controlStateCollection = new ControlBehaviourContainerCollection
			{
				{ new ControlBehaviourContainer<DummyBusinessObject>(new ControlVisibilityBehaviour<DummyBusinessObject>(d => d.Z0_Number == 22), new Func<DummyBusinessObject, ZPropertyInfo>[] { d => d.Z0_NumberInfo }) },
			};

			controlHostMock.Setup(c => c.GetCurrentDataItem()).Returns(bo);
			controlHostMock.Setup(c => c.GetControl(BagForTest.Instance.ControlForTest)).Returns(BagForTest.Instance.ControlInstance);

			var controlDependencyObserver = new ControlDependencyObserver(controlHostMock.Object);
			controlDependencyObserver.ConfigureControlBehaviourObservers(BagForTest.Instance.ControlForTest, controlStateCollection);

			CombineAssertions(() =>
			{
				bo.Z0_Number = 12;
				controlHostMock.Verify(c => c.UpdateLayout(), Times.Once);
				controlHostMock.Invocations.Clear();

				bo.Z0_Number = 134;
				controlHostMock.Verify(c => c.UpdateLayout(), Times.Once);
				controlHostMock.Invocations.Clear();
			});

			controlDependencyObserver.UnsubscribeObservers();

			CombineAssertions(() =>
			{
				bo.Z0_Number = 43;
				controlHostMock.Verify(c => c.UpdateLayout(), Times.Never);
			});
		}

		class ControlForTest : Control
		{
		}

		class BagForTest : IControlBag
		{
			[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "There is no way to change static backing field of this property.")]
			public static BagForTest Instance { get; } = new BagForTest();

			public Control TemplateControl => null;

			public BagForTest()
			{
				ControlForTest = new ControlReference(this, nameof(ControlForTest));
			}

			public void CreateControls(Control container, IDictionary<ControlReference, Control> controlsByReference)
			{
				ControlInstance = new ControlForTest { Name = nameof(ControlForTest) };

				controlsByReference.Add(new ControlReference(this, nameof(ControlForTest)), ControlInstance);
			}

			public bool ContainsControl(string name) => Controls.Any(c => c.Name == name);

			public IReadOnlyCollection<ControlReference> Controls => new List<ControlReference> { ControlForTest };

			public ControlReference ControlForTest { get; }

			public ControlForTest ControlInstance { get; private set; }
		}
	}
}
