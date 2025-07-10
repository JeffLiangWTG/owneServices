using System;
using System.Text;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(SelectNDRPathRegistryDataType))]
	public class SelectNDRPathRegistryDataTypeTest : RegistryDataTypeTestCase<SelectNDRPathRegistryDataType>
	{
		public void TestValidate()
		{
			AssertExceptionThrown("Should throw RegistryValidationException when AllowEmailsToBeSentFromUsersAddress is false", typeof(RegistryValidationException), "The default value cannot be overridden as the Allow Emails To Be Sent From User's Address Registry setting is set to No.", () =>
			{
				using (RawDataRegistry.Instance.AllowEmailsToBeSentFromUsersAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var mockRepo = new MockRepository(MockBehavior.Strict);
					var mockPairListProvider = mockRepo.Create<ICodeDescriptionPairListProvider>(MockBehavior.Strict);
					var list = new CodeDescriptionPairList();
					list.AddPair("AB", "SomeValue AB");
					mockPairListProvider.Setup(m => m.CodeDescriptionPairList).Returns(list);
					var dataType = new SelectNDRPathRegistryDataType(mockPairListProvider.Object);
					dataType.Validate(null, "AB", Guid.Empty, Guid.Empty, Guid.Empty);
				}
			});

			AssertNoExceptionThrown("Should not throw RegistryValidationException when AllowEmailsToBeSentFromUsersAddress is true", () =>
			{
				using (RawDataRegistry.Instance.AllowEmailsToBeSentFromUsersAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var mockRepo = new MockRepository(MockBehavior.Strict);
					var mockPairListProvider = mockRepo.Create<ICodeDescriptionPairListProvider>(MockBehavior.Strict);
					var list = new CodeDescriptionPairList();
					list.AddPair("AB", "SomeValue AB");
					mockPairListProvider.Setup(m => m.CodeDescriptionPairList).Returns(list);
					var dataType = new SelectNDRPathRegistryDataType(mockPairListProvider.Object);
					dataType.Validate(null, "AB", Guid.Empty, Guid.Empty, Guid.Empty);
				}
			});
		}

		protected override object GetNullRepresentation()
		{
			return StringRegistryDataTypeTest.GetNullStringRepresentation();
		}

		protected override SelectNDRPathRegistryDataType GetNewDataType()
		{
			return new SelectNDRPathRegistryDataType(OLookUpEditType.PaymentType);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB("", Encoding.Unicode.GetBytes("")),
				new ValidSampleAndBinaryValueInDB("CCX", Encoding.Unicode.GetBytes("CCX")),
				new ValidSampleAndBinaryValueInDB("PPD", Encoding.Unicode.GetBytes("PPD"))
			};
		}
	}
}
