using System;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.PAVE.MENT.Business
{
	public class ColumnFunction : NonPersistentBusinessObject<ColumnFunctionValidation>, IColumnFunction
	{
		public ColumnFunction(string columnType)
		{
			this.columnType = columnType;
		}

		readonly string columnType;

		public string ColumnType
		{
			get { return columnType; }
		}

		public ZString GetFunctionAsStringWithFormat()
		{
			var format = ZString.Empty;

			switch (FunctionType)
			{
				case ColumnFunctionTypes.Codes.None:
					format = "{0}"; // Sql format to be string manipulated
					break;
				case ColumnFunctionTypes.Codes.Round:
					format = Parameter1 > 0
						? string.Format(CultureInfo.InvariantCulture, "FLOOR({0} / {1}) * {1}", "{0}", Parameter1) // Sql format to be string manipulated
						: "FLOOR({0})"; // Sql format to be string manipulated
					break;
				default:
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Function type: {0} does not exist", FunctionType));
			}

			return format;
		}

		[XmlColumnProperty]
		[ResourceStringData("ColumnFunction.FunctionType", Caption = "Function Type", FullDescription = "The type of function to apply to the column")]
		[List("ColumnFunctionTypes")]
		public ZString FunctionType
		{
			get { return GetXmlColumnPropertyValue<ZString>(FunctionTypeInfo); }
			set
			{
				SetXmlColumnPropertyValue(FunctionTypeInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateFunctionType();
					Validation.ValidateParameter1();
				}
			}
		}

		public ZPropertyInfo FunctionTypeInfo
		{
			get { return GetZPropertyInfo(nameof(FunctionType)); }
		}

		[XmlColumnProperty]
		[ReadOnlyMember(nameof(Parameter1Readonly))]
		[ResourceStringData("ColumnFunction.Parameter1", Caption = "Parameter 1", FullDescription = "The first parameter to the function selected")]
		public ZDecimal Parameter1
		{
			get { return GetXmlColumnPropertyValue<ZDecimal>(Parameter1Info); }
			set
			{
				SetXmlColumnPropertyValue(Parameter1Info, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateParameter1();
				}
			}
		}

		public ZPropertyInfo Parameter1Info
		{
			get { return GetZPropertyInfo(nameof(Parameter1)); }
		}

		protected bool Parameter1Readonly
		{
			get { return FunctionType == ColumnFunctionTypes.Codes.None; }
		}

		public ColumnFunctionTypes ColumnFunctionTypes
		{
			get { return new ColumnFunctionTypes(); }
		}

		#region BusinessObject Overrides

		public override ColumnFunctionValidation GetNewValidation()
		{
			return new ColumnFunctionValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			FunctionType = ColumnFunctionTypes.Codes.None;
		}

		#endregion
	}
}
