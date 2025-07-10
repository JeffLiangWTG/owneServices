using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Customs.US.Testing
{
	[TestedType(typeof(ExportEntryFilerID))]
	sealed class ExportEntryFilerIDTest : RegistryBusinessObjectTemplateTestCase<ExportEntryFilerID>
	{
		public void TestValidation()
		{
			var result = new ExportEntryFilerID();
			result.EntryFilerIDType = ZString.Empty;
			AssertHasErrorContaining(result.EntryFilerIDTypeInfo, MandatoryValidation.MustBeEntered);
			result.EntryFilerIDType = "!";
			AssertNoErrorContaining(result.EntryFilerIDTypeInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(result.EntryFilerIDTypeInfo, ListValidation.InvalidCodeError);

			result.EntryFilerIDType = AESEntryFilerIDTypeList.Codes.DataUniversalNumberingSystem;
			AssertNoErrorContaining(result.EntryFilerIDTypeInfo, ListValidation.InvalidCodeError);

			result.EntryFilerID = "12-12345678";
			AssertHasError(result.EntryFilerIDInfo, ExportEntryFilerID.DUNSRightFormat);

			result.EntryFilerID = ZString.Empty;
			AssertHasErrorContaining(result.EntryFilerIDInfo, MandatoryValidation.MustBeEntered);

			result.EntryFilerID = "121234567";
			AssertNoErrorContaining(result.EntryFilerIDInfo, MandatoryValidation.MustBeEntered);
			AssertNoError(result.EntryFilerIDInfo, ExportEntryFilerID.DUNSRightFormat);

			result.EntryFilerIDType = AESEntryFilerIDTypeList.Codes.EmployerIdentificationNumber;
			result.EntryFilerID = "999999999";
			AssertNoError(result.EntryFilerIDInfo, ExportEntryFilerID.EINNumberRightFormat);

			result.EntryFilerID = "9999999991";
			AssertHasError(result.EntryFilerIDInfo, ExportEntryFilerID.EINNumberRightFormat);

			result.EntryFilerID = "99-9999999";
			AssertNoError(result.EntryFilerIDInfo, ExportEntryFilerID.EINNumberRightFormat);

			result.EntryFilerIDType = AESEntryFilerIDTypeList.Codes.SocialSecurityNumber;
			result.EntryFilerID = "999999999";
			AssertNoError(result.EntryFilerIDInfo, ExportEntryFilerID.SSNRightFormat);

			result.EntryFilerID = "9999999991";
			AssertHasError(result.EntryFilerIDInfo, ExportEntryFilerID.SSNRightFormat);

			result.EntryFilerID = "999-99-9999";
			AssertNoError(result.EntryFilerIDInfo, ExportEntryFilerID.SSNRightFormat);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override ExportEntryFilerID GetBusinessObjectToClone()
		{
			var result = new ExportEntryFilerID();
			result.EntryFilerID = "12-1234567";
			result.EntryFilerIDType = AESEntryFilerIDTypeList.Codes.EmployerIdentificationNumber;
			return result;
		}

		protected override ExportEntryFilerID GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
