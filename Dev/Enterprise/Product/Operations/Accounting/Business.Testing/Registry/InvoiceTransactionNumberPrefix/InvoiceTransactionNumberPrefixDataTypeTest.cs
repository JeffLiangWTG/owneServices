using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(InvoiceTransactionNumberPrefixDataType))]
	class InvoiceTransactionNumberPrefixDataTypeTest : RegistryDataTypeTestCase<InvoiceTransactionNumberPrefixDataType>
	{
		[ExpectNoExceptions]
		public void TestUniquenessValidation()
		{
			Guid anotherCompany = new BusinessObjectFactory().LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, Environment.Env.CurrentCompany.PK) { OrderBy = GlbCompanySchema.Constants.GC_Code }).PK.ToGuid();
			InvoiceTransactionNumberPrefixDataType dataType = GetNewDataType();
			InvoiceTransactionNumberPrefixRegistryItem registryItem = new InvoiceTransactionNumberPrefixRegistryItem(String.Empty, null, null, null, RegistryStorageFlags.Company);

			bool rightExceptionCaught = false;
			try
			{
				registryItem.SetValue(anotherCompany, Guid.Empty, Guid.Empty, "ABC");
				dataType.Validate(registryItem, "ABC", Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			}
			catch (Exception ex)
			{
				rightExceptionCaught = ex is RegistryValidationException && ex.Message == "Value must be unique for all companies in the database. The other company already using this value is DEM.";
			}
			finally
			{
				Assert("RegistryValidationException with the right Message should have been caught.", rightExceptionCaught);
			}

			dataType.Validate(registryItem, "XYZ", Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
		}

		[ExpectNoExceptions]
		public void TestLength()
		{
			InvoiceTransactionNumberPrefixRegistryItem registryItem = new InvoiceTransactionNumberPrefixRegistryItem(String.Empty, null, null, null, RegistryStorageFlags.Company);

			bool rightExceptionCaught = false;
			try
			{
				registryItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "ABCDE");
			}
			catch (Exception ex)
			{
				rightExceptionCaught = ex is RegistryValidationException && ex.Message == "Length must be between 1 and 4.";
			}
			finally
			{
				Assert("RegistryValidationException with the right Message should have been caught.", rightExceptionCaught);
			}

			registryItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "ABC");
		}

		#region Implementation

		protected override InvoiceTransactionNumberPrefixDataType GetNewDataType()
		{
			return new InvoiceTransactionNumberPrefixDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB("ABC", new byte[] { 65, 0, 66, 0, 67, 0 }) };
		}

		protected override object GetNullRepresentation()
		{
			return StringRegistryDataTypeTest.GetNullStringRepresentation();
		}

		#endregion
	}
}
