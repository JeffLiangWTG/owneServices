using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class RegistryDataTypeTest : TestCase
	{
		public void TestDefaultValues()
		{
			AssertEquals("AllowNull", false, DataType.AllowNull);
			AssertEquals("HasDefaultEditorInfo", true, DataType.HasDefaultEditorInfo);
			AssertEquals("IsFallBackMergeValuesImplemented", false, DataType.IsFallBackMergeValuesImplemented);
			AssertEquals("IsFallBackMergeActualValuesImplemented", false, DataType.IsFallBackMergeActualValuesImplemented);
			AssertEquals("IsValidatedOnSetEvenIfEqualDefaultValue", false, DataType.IsValidatedOnSetEvenIfEqualDefaultValue);
		}

		public void TestSuspendValidation()
		{
			using (DataType.SuspendValidation())
			{
				DataType.Validate(null, null, Guid.Empty, Guid.Empty, Guid.Empty);
				AssertEquals("ValidationCount", 0, DataType.ValidationCount);
			}

			DataType.Validate(null, null, Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("ValidationCount", 1, DataType.ValidationCount);
		}

		public void TestICanCallToStringLikeABoss()
		{
			AssertNoExceptionThrown(() => DataType.ToString());
		}

		public void TestValidatingEventHandler()
		{
			var expectedErrorMessage = "Validation error here!";
			DataType.Validating += (s, e) => { throw new RegistryValidationException(expectedErrorMessage); };
			try
			{
				DataType.Validate(null, null, Guid.Empty, Guid.Empty, Guid.Empty);
			}
			catch (RegistryValidationException e)
			{
				AssertEquals("Validation error should be thrown by attached validating event", expectedErrorMessage, e.Message);
			}
		}

		public void TestCastProposedToT_NoNullException()
		{
			var item = new BooleanRegistryItem("Test", (NoResString)"TestCategory/TestSubCat", null, null, RegistryStorageFlags.Company, RegistryOptions.IsHidden, false);
			AssertNoExceptionThrown(() => item.DataType.Validate(item, null, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		MockRegistryDataType DataType
		{
			get { return dataType ?? (dataType = new MockRegistryDataType()); }
		}
		MockRegistryDataType dataType;

		#region class MockRegistryDataType

		class MockRegistryDataType : RegistryDataType<string>
		{
			public MockRegistryDataType()
				: base(null, null)
			{
			}

			public override bool IsDefaultValueImmutable => false;

			public int ValidationCount
			{
				get { return validationCount; }
			}

			protected override byte[] SerialiseCore(string value)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			protected override string DeserialiseCore(byte[] value)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				validationCount++;
			}

			int validationCount;
		}

		#endregion
	}
}
