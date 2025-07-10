using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Controls.Extensions.Testing
{
	sealed class ValidationExtensionTest : BaseExtensionTest<ValidationExtension>
	{
		GenericExtendedDataBoundControl control;
		GenericExtendedDataBoundControl_WithNotificationDataMembers control2;
		MockFakeBusinessObject dataSource;

		TestValidationExtension extension;
		TestValidationExtension extension2;

		protected override void SetUp()
		{
			base.SetUp();

			dataSource = new MockFakeBusinessObject();
			control = new GenericExtendedDataBoundControl(dataSource, "property1");
			control2 = new GenericExtendedDataBoundControl_WithNotificationDataMembers(new string[] { "property1", "property2" }, dataSource, "property1");

			extension = new TestValidationExtension();
			extension.Initialize(control);
			extension2 = new TestValidationExtension();
			extension2.Initialize(control2);
		}

		[ExpectNoExceptions]
		public void TestDoNothingIfHostIsNotDataBound()
		{
			var ext = new ValidationExtension();
			ext.Initialize(new GenericExtendedControl());
			ext.Validate();
		}

		public void TestDoNothingIfTargetBusinessObjectIsNull()
		{
			extension2.GetObjectFromConsolResult = null;

			extension2.Validate();

			Assert(!extension2.ValidatePropertyWasCalled);
		}

		public void TestDoNothingIfTargetBusinessObjectIsDeleted()
		{
			dataSource.Deleted = true;
			extension2.GetObjectFromConsolResult = dataSource;

			extension2.Validate();

			Assert(!extension2.ValidatePropertyWasCalled);
		}

		public void TestValidatesEachPropertyMemberAndForcesNotificationExtensionToRedrawAfterwards()
		{
			extension2.GetObjectFromConsolResult = dataSource;

			extension2.Validate();

			AssertEquals(true, extension2.ValidatePropertyWasCalled);
			AssertEquals(2, extension2.PropertiesToValidate.Count);
			AssertEquals("property1", extension2.PropertiesToValidate[0]);
			AssertEquals("property2", extension2.PropertiesToValidate[1]);

			AssertEquals(true, extension2.ForceNotificationRedrawWasCalled);
		}

		public void TestForcesNotificationExtensionToRedrawEvenIfErrorOccuredDuringValidation()
		{
			extension.ThrowOnValidateProperty = true;

			extension.GetObjectFromConsolResult = dataSource;

			try
			{
				extension.Validate();
			}
			catch { }

			AssertEquals(true, extension.ValidatePropertyWasCalled);
			AssertEquals(1, extension.PropertiesToValidate.Count);
			AssertEquals("property1", extension.PropertiesToValidate[0]);

			AssertEquals(true, extension.ForceNotificationRedrawWasCalled);
		}

		public void TestForcesNotificationExtensionToRedrawEvenIfErrorOccuredDuringValidation_ForINotificationDataMembers()
		{
			extension2.ThrowOnValidateProperty = true;

			extension2.GetObjectFromConsolResult = dataSource;

			try
			{
				extension2.Validate();
			}
			catch { }

			AssertEquals(true, extension2.ValidatePropertyWasCalled);
			AssertEquals(1, extension2.PropertiesToValidate.Count);
			AssertEquals("property1", extension2.PropertiesToValidate[0]);

			AssertEquals(true, extension2.ForceNotificationRedrawWasCalled);
		}

		[ExpectNoExceptions]
		public void TestForceNotificationRedrawUsesValidationExtensionIfSupported()
		{
			var extensions = new Mock<IControlExtensionCollection>(MockBehavior.Strict);
			var notificationExtension = new Mock<INotificationExtension>(MockBehavior.Strict);

			control2.Extensions = extensions.Object;
			extension2.ShouldCallOriginalMethod = true;

			extensions.SetupSequence(m => m.Supports<INotificationExtension>()).Returns(true).Returns(false);
			extensions.Setup(m => m.Get<INotificationExtension>()).Returns(notificationExtension.Object);
			notificationExtension.Setup(m => m.Redraw());

			extension2.CallForceNotificationRedraw();

			extension2.CallForceNotificationRedraw();
		}

		#region Support

		class TestValidationExtension : ValidationExtension
		{
			public bool ShouldCallOriginalMethod;
			public bool ForceNotificationRedrawWasCalled;

			public bool ValidatePropertyWasCalled;
			public readonly List<string> PropertiesToValidate = new List<string>();
			public bool ThrowOnValidateProperty;

			protected override void ForceNotificationRedraw()
			{
				if (ShouldCallOriginalMethod)
				{
					base.ForceNotificationRedraw();
				}

				ForceNotificationRedrawWasCalled = true;
			}

			public void CallForceNotificationRedraw()
			{
				ForceNotificationRedraw();
			}

			protected override void ValidateProperty(BusinessObject obj, string property)
			{
				ValidatePropertyWasCalled = true;

				PropertiesToValidate.Add(property);

				if (ThrowOnValidateProperty)
				{
					throw new Exception();
				}
			}

			public object GetObjectFromConsolResult
			{
				get { return getObjectFromConsolResult; }
				set
				{
					getObjectFromConsolResult = value;
					getObjectFromConsolResultSet = true;
				}
			}
			object getObjectFromConsolResult;
			bool getObjectFromConsolResultSet;

			protected override object GetObjectFromControl(Control control, object dataSource, string bindingPath)
			{
				return getObjectFromConsolResultSet ? GetObjectFromConsolResult : base.GetObjectFromControl(control, dataSource, bindingPath);
			}
		}

		class MockFakeBusinessObject : FakeBusinessObject
		{
			public bool Deleted;

			public override bool IsDeleted
			{
				get { return Deleted; }
			}
		}

		#endregion
	}
}
