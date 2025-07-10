using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.PAVE.MENT.Shared;

namespace Enterprise.PAVE.MENT.Business
{
	public class SQLColumnSpecification : ColumnSpecification
	{
		public SQLColumnSpecification(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Virtual method is HasChanges_set. That'd be bad to override usually.")]
		public SQLColumnSpecification(BusinessObjectFactory factory, ZString code, ZString column, ZString columnType, ZBool requireCoalesce)
			: base(factory)
		{
			Code = code;
			Column = column;
			ColumnType = columnType;
			RequireCoalesce = requireCoalesce;
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Virtual method is HasChanges_set. That'd be bad to override usually.")]
		public SQLColumnSpecification(ZString code, ZString column, ZString columnType, ZBool requireCoalesce)
		{
			Code = code;
			Column = column;
			ColumnType = columnType;
			RequireCoalesce = requireCoalesce;
		}

		[XmlColumnProperty]
		[ResourceStringData("SQLColumnSpecification.Code", Caption = "Code", FullDescription = "The code of the column to be included.")]
		[List("Lookups.ColumnList")]
		public ZString Code
		{
			get { return GetXmlColumnPropertyValue<ZString>(CodeInfo); }
			set { SetXmlColumnPropertyValue(CodeInfo, value); }
		}

		[ResourceStringData("SQLColumnSpecification.Description", Caption = "Description", FullDescription = "The name of the column to be included.")]
		public ZString Description
		{
			get { return Lookups.ColumnList.GetDescriptionFromCode(Code); }
		}

		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(nameof(Code)); }
		}

		[XmlColumnProperty]
		public ZString ColumnType
		{
			get { return GetXmlColumnPropertyValue<ZString>(ColumnTypeInfo); }
			set { SetXmlColumnPropertyValue(ColumnTypeInfo, value); }
		}

		public ZPropertyInfo ColumnTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ColumnType)); }
		}

		[XmlColumnProperty]
		public ZBool RequireCoalesce
		{
			get { return GetXmlColumnPropertyValue<ZBool>(RequireCoalesceInfo); }
			set { SetXmlColumnPropertyValue(RequireCoalesceInfo, value); }
		}

		public ZPropertyInfo RequireCoalesceInfo
		{
			get { return GetZPropertyInfo(nameof(RequireCoalesce)); }
		}

		[XmlColumnProperty]
		public ColumnFunction ColumnFunction
		{
			get
			{
				if (columnFunction == null)
				{
					columnFunction = new ColumnFunction(ColumnType);
				}

				return columnFunction;
			}
		}

		ColumnFunction columnFunction;

		public SQLColumnSpecificationLookups Lookups
		{
			get
			{
				if (lookups == null || !IsLookupsCachedInBase)
				{
					lookups = GetNewLookups();
				}

				return lookups;
			}
		}

		protected SQLColumnSpecificationLookups GetNewLookups()
		{
			return new SQLColumnSpecificationLookups(this);
		}

		SQLColumnSpecificationLookups lookups;

		public class SQLColumnSpecificationComparer : IEqualityComparer<SQLColumnSpecification>
		{
			public bool Equals(SQLColumnSpecification x, SQLColumnSpecification y)
			{
				return x.Column.Equals(y.Column) && x.Selected.Equals(y.Selected);
			}

			public int GetHashCode(SQLColumnSpecification obj)
			{
				return obj.GetHashCode();
			}
		}

		public IZType TranslateObjectToCompatibleType(object o, int position, int totalItemCount)
		{
			try
			{
				IZType result;

				switch (ColumnType)
				{
					case MENTConstants.DecimalColumn:
						if (o is decimal)
						{
							result = (ZDecimal)(decimal)o;
						}
						else if (o is double)
						{
							var decimalValue = Convert.ToDouble(o, CultureInfo.InvariantCulture);
							result = (ZDecimal)(decimal)decimalValue;
						}
						else
						{
							result = (ZInt)(int)o;
						}
						break;
					case MENTConstants.DateColumn:
						result = (ZDateTime)(DateTime)o;
						break;
					case MENTConstants.StringColumn:
						result = (ZString)o.ToString();
						break;
					case MENTConstants.NullColumn:
						result = ZString.Empty;
						break;
					default:
						ErrorReporter.ReportOnce("Unrecognised Column Type");
						result = ZString.Empty;
						break;
				}
				return result;
			}
			catch (InvalidCastException ex)
			{
				ErrorReporter.ReportOnce(
					"InvalidColumnValueCast",
					string.Format(
						CultureInfo.InvariantCulture,
						@"Error translating SQL-object (type: {0}, value: {1}) 
from column (type={2}, description={3}, column={4}) 
with array-position={5} and array-length={6}: 
{7}",
						/*0*/ o.GetType().Name,
						/*1*/ o.ToString(),
						/*2*/ ColumnType,
						/*3*/ Description,
						/*4*/ Column,
						/*5*/ position,
						/*6*/ totalItemCount,
						/*7*/ ex.Message),
					ex);
				return ZString.Empty;
			}
		}
	}
}
