using System;
using System.Data;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native.Common.Converters;
using Enterprise.DataTransfer.Native.DB;
using Enterprise.DataTransfer.Native.DB.Keys;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Common.EntityConverter
{
	public class ERConverterTest : TransactionedTestCase
	{
		public void TestMergeEntityToRow()
		{
			entity["PK"] = Guid.NewGuid();
			entity["Code"] = "ABC";
			entity["Guid"] = null;

			row = converter.Convert(entity);

			AssertNotEquals("Should not merge primary key to row", entity["PK"], row[AutoDummyBizo.Schema.PK]);
			AssertEquals("Should merge other property to row", entity["Code"], row[AutoDummyBizo.Schema.Z0_Code]);
			AssertEquals("Should be null if property value is null", DBNull.Value, row[AutoDummyBizo.Schema.Z0_Guid]);

			row = converter.Convert(entity, new EntityContext(sessionServices, new FactoryProvider()) { AlwaysUseInternalPK = true });

			AssertEquals("Should merge primary key to row", entity["PK"], row[AutoDummyBizo.Schema.PK]);
			AssertEquals("Should merge other property to row", entity["Code"], row[AutoDummyBizo.Schema.Z0_Code]);
			AssertEquals("Should be null if property value is null", DBNull.Value, row[AutoDummyBizo.Schema.Z0_Guid]);
		}

		public void TestConvertVarBinary()
		{
			const string Base64String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

			entity.InternalPK = Guid.NewGuid();
			entity[AutoDummyBizo.Schema.Z0_VarBinaryMax.Substring(3)] = Base64String;

			row = converter.Convert(entity);

			AssertEquals(Convert.FromBase64String(Base64String), (byte[])row[AutoDummyBizo.Schema.Z0_VarBinaryMax]);
		}

		public void TestConvertDateTimeOffset()
		{
			var dateTimeOffset = DateTimeOffset.Now;

			entity.InternalPK = Guid.NewGuid();
			entity[AutoDummyBizo.Schema.Z0_DateTimeOffset.Substring(3)] = dateTimeOffset; // Substr. because definition properties don't have table code.

			row = converter.Convert(entity);

			AssertEquals(dateTimeOffset, row[AutoDummyBizo.Schema.Z0_DateTimeOffset]);
		}

		public void TestUpdateRowFromEntityForExcludeImportProperty()
		{
			var orgDefinition = TestUtil.FindEntityDefinition("Organization", "OrgHeader");
			var orgHeader = new Entity(orgDefinition, sessionServices);
			orgHeader["ScreeningStatus"] = "MAT";
			var orgSourceRow = orgHeader.Definition.Table.ToDataTable().NewRow();
			converter.UpdateRowFromEntity(orgHeader, null, orgSourceRow);
			AssertEquals(DBNull.Value, orgSourceRow["OH_ScreeningStatus"]);
		}

		public void TestConvertPassword()
		{
			var plainTextPassword = "PSWD";

			var cusBondDetailDefinition = TestUtil.FindEntityDefinition("Organization", "OrgHeader.CusBondDetail");
			var cusBondDetailEntity = new Entity(cusBondDetailDefinition, sessionServices);
			cusBondDetailEntity.InternalPK = Guid.NewGuid();
			cusBondDetailEntity["Password"] = plainTextPassword;
			var cusBondDetailSourceRow = cusBondDetailEntity.Definition.Table.ToDataTable().NewRow();
			converter.UpdateRowFromEntity(cusBondDetailEntity, null, cusBondDetailSourceRow);
			AssertEquals(plainTextPassword, cusBondDetailSourceRow["PW_Password"]);
		}

		[ExpectExceptionMessage(typeof(NativeXMLUserVisibleException), "ERROR: DummyBizo.Code - Maximum length allowed in this column is 5 characters, 6 were provided - [ABCDEF].")]
		public void TestValueExceedMaxLength()
		{
			string value = "ABCDEF";
			entity["Code"] = value;

			Assert("PRE: The length of Code exceeds the max length of the column", value.Length > AutoDummyBizo.Schema.Z0_CodeMaxLength);

			row = converter.Convert(entity);
		}

#if NETFRAMEWORK
		[ExpectExceptionMessage(typeof(NativeXMLUserVisibleException), "[DummyBizo.Bool] : String was not recognized as a valid Boolean.Couldn't store <N> in Bool Column.  Expected type is Boolean. Data type expected in this column is either 'true' or 'false'.")]
#else
		[ExpectExceptionMessage(typeof(NativeXMLUserVisibleException), "[DummyBizo.Bool] : String 'N' was not recognized as a valid Boolean.Couldn't store <N> in Bool Column.  Expected type is Boolean. Data type expected in this column is either 'true' or 'false'.")]
#endif
		public void TestValueBool()
		{
			string value = "N";
			entity["Bool"] = value;
			row = converter.Convert(entity);
		}

#if NETFRAMEWORK
		[ExpectExceptionMessage(typeof(NativeXMLUserVisibleException), "[DummyBizo.Byte] : Input string was not in a correct format.Couldn't store <FOO> in Byte Column.  Expected type is Byte.")]
#else
		[ExpectExceptionMessage(typeof(NativeXMLUserVisibleException), "[DummyBizo.Byte] : The input string 'FOO' was not in a correct format.Couldn't store <FOO> in Byte Column.  Expected type is Byte.")]
#endif
		public void TestValueInvalidByte()
		{
			string value = "FOO";
			entity["Byte"] = value;

			row = converter.Convert(entity);
		}

#if NETFRAMEWORK
		[ExpectExceptionMessage(typeof(NativeXMLUserVisibleException), "[DummyBizo.Byte] : Input string was not in a correct format.Couldn't store <> in Byte Column.  Expected type is Byte.")]
#else
		[ExpectExceptionMessage(typeof(NativeXMLUserVisibleException), "[DummyBizo.Byte] : The input string '' was not in a correct format.Couldn't store <> in Byte Column.  Expected type is Byte.")]
#endif
		public void TestValueByteEmpty()
		{
			string value = "";
			entity["Byte"] = value;

			row = converter.Convert(entity);
		}

#if NETFRAMEWORK
		[ExpectExceptionMessage(typeof(NativeXMLUserVisibleException), "[DummyBizo.Number] : Input string was not in a correct format.Couldn't store <BAR> in Number Column.  Expected type is Int32.")]
#else
		[ExpectExceptionMessage(typeof(NativeXMLUserVisibleException), "[DummyBizo.Number] : The input string 'BAR' was not in a correct format.Couldn't store <BAR> in Number Column.  Expected type is Int32.")]
#endif
		public void TestValueIncorrectNumber()
		{
			string value = "BAR";
			entity["Number"] = value;

			row = converter.Convert(entity);
		}

		[ExpectExceptionMessage(typeof(NativeXMLUserVisibleException), "[DummyBizo.Number] : Value was either too large or too small for an Int32.Couldn't store <678374897987888347888888889999999999> in Number Column.  Expected type is Int32.")]
		public void TestValueExceedsLimitNumber()
		{
			string value = "678374897987888347888888889999999999";
			entity["Number"] = value;

			row = converter.Convert(entity);
		}

		[ExpectExceptionMessage(typeof(NativeXMLUserVisibleException), "[DummyBizo.Decimal] : Value was too large for a decimal type. Couldn't store <1234567890123456789> in decimal column. Limit is 18 digits before the decimal point but 19 were provided.")]
		public void TestValueExceedsLimitDecimalNoFraction()
		{
			string value = "1234567890123456789";
			entity["Decimal"] = value;

			var columnInfo = TestUtil.FindEntityDefinition("Dummy", "DummyBizo").Table.Columns["Z0_Decimal"];
			Assert("PRE: The length of Decimal exceeds the Precision of the column", value.Length > columnInfo.Precision - columnInfo.Scale);

			row = converter.Convert(entity);
		}

		[ExpectExceptionMessage(typeof(NativeXMLUserVisibleException), "[DummyBizo.Decimal] : Value was too large for a decimal type. Couldn't store <1234567890123456789.123456> in decimal column. Limit is 18 digits before the decimal point but 19 were provided.")]
		public void TestValueExceedsLimitDecimal()
		{
			string value = "1234567890123456789.123456";
			entity["Decimal"] = value;

			var columnInfo = TestUtil.FindEntityDefinition("Dummy", "DummyBizo").Table.Columns["Z0_Decimal"];
			Assert("PRE: The length of Decimal exceeds the Precision of the column", value.Split('.')[0].Length > columnInfo.Precision - columnInfo.Scale);

			row = converter.Convert(entity);
		}

		[ExpectExceptionMessage(typeof(NativeXMLUserVisibleException), "[DummyBizo.AnotherDecimal] : Value was too large for a decimal type. Couldn't store <1234567890123456> in decimal column. Limit is 15 digits before the decimal point but 16 were provided.")]
		public void TestValueExceedsLimitAnotherDecimalNoFraction()
		{
			string value = "1234567890123456";
			entity["AnotherDecimal"] = value;

			var columnInfo = TestUtil.FindEntityDefinition("Dummy", "DummyBizo").Table.Columns["Z0_AnotherDecimal"];
			Assert("PRE: The length of Decimal exceeds the Precision of the column", value.Length > columnInfo.Precision - columnInfo.Scale);

			row = converter.Convert(entity);
		}

		[ExpectExceptionMessage(typeof(NativeXMLUserVisibleException), "[DummyBizo.AnotherDecimal] : Value was too large for a decimal type. Couldn't store <1234567890123456.123456> in decimal column. Limit is 15 digits before the decimal point but 16 were provided.")]
		public void TestValueExceedsLimitAnotherDecimal()
		{
			string value = "1234567890123456.123456";
			entity["AnotherDecimal"] = value;

			var columnInfo = TestUtil.FindEntityDefinition("Dummy", "DummyBizo").Table.Columns["Z0_AnotherDecimal"];
			Assert("PRE: The length of Decimal exceeds the Precision of the column", value.Split('.')[0].Length > columnInfo.Precision - columnInfo.Scale);

			row = converter.Convert(entity);
		}

#if NETFRAMEWORK
		[ExpectExceptionMessage(typeof(NativeXMLUserVisibleException), "[DummyBizo.Decimal] : Input string was not in a correct format.Couldn't store <BOO> in Decimal Column.  Expected type is Decimal.")]
#else
		[ExpectExceptionMessage(typeof(NativeXMLUserVisibleException), "[DummyBizo.Decimal] : The input string 'BOO' was not in a correct format.Couldn't store <BOO> in Decimal Column.  Expected type is Decimal.")]
#endif
		public void TestValueIncorrectDecimal()
		{
			string value = "BOO";
			entity["Decimal"] = value;

			row = converter.Convert(entity);
		}

		[ExpectExceptionMessage(typeof(NativeXMLUserVisibleException), "[DummyBizo.Money] : Value was either too large or too small for Money. Couldn't store <999999999999999.000> in Money Column.")]
		public void TestValueExceedsLimitMoney()
		{
			string value = "999999999999999.000";
			entity["Money"] = value;

			row = converter.Convert(entity);
		}

		[ExpectExceptionMessage(typeof(NativeXMLUserVisibleException), "[DummyBizo.Money] : Value was too large for a money type. Couldn't store <7625699999999999.000> in money column. Limit is 15 digits before the decimal point but 16 were provided.")]
		public void TestValueExceedsPrecisionLimitMoney()
		{
			string value = "7625699999999999.000";
			entity["Money"] = value;

			row = converter.Convert(entity);
		}

		public void TestSetForeignKey()
		{
			var methodInfo = typeof(ERConverter).GetMethod("SetForeignKey", BindingFlags.Static | BindingFlags.NonPublic);
			var dummyRow = ((INeedRow)new BusinessObjectFactory { RefreshEnabled = false }.New<DummyBusinessObject>()).Row;
			var key = new Key(new ColumnDef(dummyRow.TableDef(), DummyBizoSchema.Constants.Z0_Code, "string"));
			const string value = "Some long description text";

			var expectedMessage = $"Could not set <{dummyRow.Table.TableName}>.<{DummyBizoSchema.Constants.Z0_Code}> to [{value}] as the value exceeds the maximum length of {DummyBizoSchema.Z0_Code.MaxLength} characters. Verify that you did not use a full name if code was expected.";

			try
			{
				methodInfo.Invoke(null, new object[] { dummyRow, key, value });
				Fail("Should throw exception about max length violation");
			}
			catch (NativeXMLUserVisibleException nex)
			{
				AssertEquals(expectedMessage, nex.Message);
			}
			catch (TargetInvocationException tex) when (tex.InnerException is NativeXMLUserVisibleException nex)
			{
				AssertEquals(expectedMessage, nex.Message);
			}
		}

		#region Implementation

		DataRow SetupDataRow(Table tableDef)
		{
			var table = tableDef.ToDataTable();

			return table.NewRow();
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestUtil.AlterDummyTable();
			sessionServices = new AncillaryImportServices();
			var definition = TestUtil.FindEntityDefinition("Dummy", "DummyBizo");
			var tableDef = definition.Table;
			row = SetupDataRow(tableDef);
			converter = new ERConverter(TestUtil.Connection, sessionServices);
			entity = new Entity(definition, sessionServices);
		}

		DataRow row;
		AncillaryImportServices sessionServices;
		IEntity entity;
		ERConverter converter;

		#endregion
	}
}
