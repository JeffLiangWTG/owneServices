using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Shared.CW;

namespace Enterprise.ServiceManager.Business.Testing
{
	sealed class NudgingSchemaResolverTest : TestCaseWithFactory
	{
		public void TestGetColumnType()
		{
			// Arrange
			var tuples = new (string table, string column, Type expectedType)[]
			{
				(OrgHeaderSchema.Constants.TableName, OrgHeaderSchema.Constants.OH_Code, typeof(string)),
				(OrgHeaderSchema.Constants.TableName, OrgHeaderSchema.Constants.PK, typeof(Guid)),
				(GlbDeviceLocationSchema.Constants.TableName, GlbDeviceLocationSchema.Constants.V2_MeasurementTimeUtc, typeof(DateTime)),
				(GlbDeviceLocationSchema.Constants.TableName, GlbDeviceLocationSchema.Constants.PK, typeof(Guid)),
			};

			// Act
			foreach (var tuple in tuples)
			{
				var result = nudgingSchemaResolver.GetColumnType(tuple.table, tuple.column);

				// Assert
				CombineAssertions(() =>
				{
					AssertEquals($"{tuple.table}.{tuple.column} type", tuple.expectedType, result);
				});
			}
		}

		public void TestGetColumnTypeExceptions()
		{
			// Arrange
			var tuples = new (string table, string column, Type expectedType, string expectedMessage)[]
			{
				(null, OrgHeaderSchema.Constants.OH_Code, typeof(ArgumentNullException), "table"),
				("SomeStrangeTable", OrgHeaderSchema.Constants.OH_Code, typeof(ArgumentException), "SomeStrangeTable"),
				(OrgHeaderSchema.Constants.TableName, null, typeof(ArgumentNullException), "column"),
				(OrgHeaderSchema.Constants.TableName, "SomeStrangeColumn", typeof(ArgumentException), "SomeStrangeColumn")
			};

			// Act
			foreach (var tuple in tuples)
			{
				// Assert
				CombineAssertions(() =>
				{
					var result = AssertExceptionThrown<Exception>(() => nudgingSchemaResolver.GetColumnType(tuple.table, tuple.column));

					AssertEquals($"{tuple.table}.{tuple.column} type", tuple.expectedType, result.GetType());
					AssertContains($"{tuple.table}.{tuple.column} message", tuple.expectedMessage, result.Message);
				});
			}
		}

		[ExpectNoExceptions]
		public void TestIsLiteralOnly()
		{
			// Arrange
			var tuples = new (string table, string column, bool isLiteralOnly)[]
			{
				(AccEInvoicingTransactionPivotSchema.Constants.TableName, AccEInvoicingTransactionPivotSchema.Constants.AIP_Status, false),
				(AccEInvoicingTransactionPivotSchema.Constants.TableName, AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode, true),
				(AccEInvoicingTransactionPivotSchema.Constants.TableName, AccEInvoicingTransactionPivotSchema.Constants.PK, true),
				(CusEntryNumSchema.Constants.TableName, CusEntryNumSchema.Constants.CE_EntryIsSystemGenerated, true),
			};
			// Act
			// Assert
			CombineAssertions(() =>
			{
				foreach (var value in tuples)
				{
					AssertEquals(nudgingSchemaResolver.IsParameterizable(value.table, value.column), value.isLiteralOnly);
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nudgingSchemaResolver = new NudgingSchemaResolver(new EnterpriseSchemaResolver());
		}

		NudgingSchemaResolver nudgingSchemaResolver;
	}
}
