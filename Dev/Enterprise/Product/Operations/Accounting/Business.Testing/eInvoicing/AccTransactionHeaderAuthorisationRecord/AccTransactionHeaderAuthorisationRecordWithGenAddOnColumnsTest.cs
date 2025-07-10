using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Accounting.Business.EInvoicing.Testing
{
	public abstract class AccTransactionHeaderAuthorisationRecordWithGenAddOnColumnsTest : AccTransactionHeaderAuthorisationRecordTest
	{
		public abstract void TestGenAddOnColumnNameAndTypes();

		protected abstract GenAddOnColumnCollection GenAddOnColumns { get; }

		protected void AssertGenAddOnColumn(ZString propertyName, object propertyValue)
		{
			var genAddOnColumn = GenAddOnColumns.Find(c => c.XA_Name == propertyName && c.XA_Type == GetGenAddOnColumnDataType(propertyValue));
			AssertNotNull(genAddOnColumn);
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.NewWithValidTestData(GetExpectedBusinessObjectType());

		string GetGenAddOnColumnDataType(object propertyValue)
		{
			ZString? propType = null;
			if (propertyValue is bool || propertyValue is ZBool)
			{
				propType = AddOnColumnDataType.Codes.Boolean;
			}
			else if (propertyValue is byte[])
			{
				propType = AddOnColumnDataType.Codes.Byte;
			}
			else if (propertyValue is DateTime || propertyValue is ZDateTime || propertyValue is ZDate || propertyValue is ZDateTimeOffset)
			{
				propType = AddOnColumnDataType.Codes.Datetime;
			}
			else if (propertyValue is decimal || propertyValue is ZDecimal)
			{
				propType = AddOnColumnDataType.Codes.Decimal;
			}
			else if (propertyValue is Guid || propertyValue is ZGuid)
			{
				propType = AddOnColumnDataType.Codes.Guid;
			}
			else if (propertyValue is int || propertyValue is ZInt)
			{
				propType = AddOnColumnDataType.Codes.Integer;
			}
			else if (propertyValue is short || propertyValue is ZShort)
			{
				propType = AddOnColumnDataType.Codes.Short;
			}
			else if (propertyValue is string || propertyValue is ZString)
			{
				propType = AddOnColumnDataType.Codes.String;
			}
			return propType;
		}
	}
}
