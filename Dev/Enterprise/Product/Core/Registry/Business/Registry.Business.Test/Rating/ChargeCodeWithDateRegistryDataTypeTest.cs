using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ChargeCodeWithDateRegistryDataType))]
	sealed class ChargeCodeWithDateRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ChargeCodeWithDateRegistryDataType>
	{
		[ExpectNoExceptions]
		public void TestValidateTickWithChargeCode()
		{
			var dataType = new ChargeCodeWithDateRegistryDataType();
			dataType.Validate(RatingDataRegistry.Instance.CustomsQuarantineChargeCode, new ChargeCodeWithDate() { ChargeCode = Guid.NewGuid() }, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		[ExpectException(typeof(RegistryValidationException))]
		public void TestValidateTickWithNoChargeCode()
		{
			var dataType = new ChargeCodeWithDateRegistryDataType();
			dataType.Validate(RatingDataRegistry.Instance.CustomsQuarantineChargeCode, new ChargeCodeWithDate(), Guid.Empty, Guid.Empty, Guid.Empty);
		}

		protected override ChargeCodeWithDateRegistryDataType GetNewDataType()
		{
			return new ChargeCodeWithDateRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var chargeCodeWithDate = new ChargeCodeWithDate();
			chargeCodeWithDate.ChargeCode = ZGuid.BrettsGuid;
			chargeCodeWithDate.ActiveTimeUtc = ZDateTime.Today;
			return new[] { new ValidSampleAndBinaryValueInDB(chargeCodeWithDate, GetNewDataType().Serialise(chargeCodeWithDate)) };
		}

		protected override bool HasEditor
		{
			get { return false; } // Using an EditorInfo instead of standard RegistryEditor attribute.
		}

		protected override string ExpectedEditorName
		{
			get { return null; }
		}
	}
}
