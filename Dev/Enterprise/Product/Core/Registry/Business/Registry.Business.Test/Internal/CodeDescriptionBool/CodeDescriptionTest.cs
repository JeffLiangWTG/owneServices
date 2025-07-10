using CargoWise.EntityFramework;
using CargoWise.Types;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	public class CodeDescriptionTest : TestCase
	{
		public class CanDeleteTest : TestCase
		{
			protected override void SetUp()
			{
				base.SetUp();
				var mock = new Mock<CodeDescription<ZByte>> { CallBase = true };
				registry = mock.Object;
			}

			public void TestReasonForCannotDelete()
			{
				// Arrange

				// Act
				var result = ((ICanDelete)registry).ReasonForNotAbleToDelete;

				// Assert
				AssertEquals("This is a system defined value and cannot be deleted.", result);
			}

			public void TestCanDelete()
			{
				// Arrange
				registry.SystemDefined = false;

				// Act
				var result = ((ICanDelete)registry).CanDelete;

				// Assert
				AssertEquals(true, result);
			}

			public void TestCanNotDeleteWhenSystem()
			{
				// Arrange
				registry.SystemDefined = true;

				// Act
				var result = ((ICanDelete)registry).CanDelete;

				// Assert
				AssertEquals(false, result);
			}

			CodeDescription<ZByte> registry;
		}

		public class CodeAndDescriptionReadOnlyTest : TestCase
		{
			protected override void SetUp()
			{
				base.SetUp();
				var mock = new Mock<CodeDescription<ZByte>> { CallBase = true };
				registry = mock.Object;
			}

			CodeDescription<ZByte> registry;

			public class NotSystemDefinedTest : CodeAndDescriptionReadOnlyTest
			{
				protected override void SetUp()
				{
					base.SetUp();
					registry.SystemDefined = false;
				}

				public void TestCodeInfoNotReadonly()
				{
					var result = registry.CodeInfo.ReadOnly;
					AssertEquals(false, result);
				}

				public void TestDescriptionInfoNotReadonly()
				{
					var result = registry.DescriptionInfo.ReadOnly;
					AssertEquals(false, result);
				}
			}

			public class SystemDefinedTest : CodeAndDescriptionReadOnlyTest
			{
				protected override void SetUp()
				{
					base.SetUp();
					registry.SystemDefined = true;
				}

				public void TestCodeInfoIsReadonly()
				{
					var result = registry.CodeInfo.ReadOnly;
					AssertEquals(true, result);
				}

				public void TestDescriptionInfoIsReadonly()
				{
					var result = registry.DescriptionInfo.ReadOnly;
					AssertEquals(true, result);
				}
			}
		}

		public class ValidationTest : TestCase
		{
			public void TestValidateValueClearsNotification()
			{
				// Arrange
				var businessObject = new CodeDescriptionForRegistryBusinessObjectTest
				{
					Code = "ABC",
					Value = 2,
				};
				businessObject.ValueInfo.AddError("Error!");

				// Act
				businessObject.ValidateValue();

				// Assert
				AssertEquals(false, businessObject.HasErrors);
			}

			public void TestValidateValueCoreIsCalledOnSetter()
			{
				// Arrange
				var businessObjectMock = new Mock<CodeDescriptionForRegistryBusinessObjectTest> { CallBase = true };

				// Act
				businessObjectMock.Object.Value = 4;

				// Assert
				AssertNoExceptionThrown(
					() => businessObjectMock
						.Protected()
						.Verify("ValidateValueCore", Times.Once()));
			}

			public void TestRunPreSaveValidationCallsValidateValue()
			{
				// Arrange
				var businessObjectMock = new Mock<CodeDescriptionForRegistryBusinessObjectTest> { CallBase = true };

				// Act
				businessObjectMock.Object.RunPreSaveValidation();

				// Assert
				AssertNoExceptionThrown(
					() => businessObjectMock
						.Protected()
						.Verify("ValidateValueCore", Times.Once()));
			}
		}
	}
}
