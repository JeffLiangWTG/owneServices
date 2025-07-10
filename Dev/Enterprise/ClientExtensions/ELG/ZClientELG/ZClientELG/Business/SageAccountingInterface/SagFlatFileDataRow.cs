using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.ELG
{
	public abstract class SagFlatFileDataRow : FlatFileDataRow
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public SagFlatFileDataRow(int fieldCount)
			: base(fieldCount)
		{
			fieldProperties = new List<FlatFileFieldProperty>();
			AddFieldProperties();
		}

		public IReadOnlyList<string> Fields
		{
			get { return DataRow; }
		}

		#region Overrides
		#region SetField overrides
		public void SetField(FlatFileFieldProperty fieldProperty, ZString value)
		{
			base.SetField(fieldProperty.Name, value.Left(fieldProperty.Length));
		}

		public void SetField(FlatFileFieldProperty fieldProperty, ZDecimal value)
		{
			base.SetField(fieldProperty.Name, value, 2);
		}

		public void SetField(FlatFileFieldProperty fieldProperty, ZDecimal value, int decimalPlaces)
		{
			base.SetField(fieldProperty.Name, value, decimalPlaces);
		}

		public void SetField(FlatFileFieldProperty fieldProperty, ZDateTime value)
		{
			base.SetField(fieldProperty.Name, value, ELGConstants.DataDateFormat);
		}

		public void SetField(FlatFileFieldProperty fieldProperty, ZInt value)
		{
			base.SetField(fieldProperty.Name, value);
		}

		public void SetField(FlatFileFieldProperty fieldProperty, int value)
		{
			base.SetField(fieldProperty.Name, value);
		}
		#endregion

		#region GetField overrides
		public ZString GetZDateTimeFieldAsString(FlatFileFieldProperty fieldProperty, ZString format)
		{
			return GetFieldAsZDateTime(fieldProperty, format).ToString(format);
		}

		public ZString GetField(FlatFileFieldProperty fieldProperty)
		{
			return base.GetField(fieldProperty.Name);
		}

		public ZDateTime GetFieldAsZDateTime(FlatFileFieldProperty fieldProperty, ZString format)
		{
			return base.GetFieldAsZDateTime(fieldProperty.Name, format);
		}

		public ZDecimal GetFieldAsZDecimal(FlatFileFieldProperty fieldProperty, int decimals)
		{
			return base.GetFieldAsZDecimal(fieldProperty.Name, decimals);
		}

		public ZDecimal GetFieldAsZDecimal(FlatFileFieldProperty fieldProperty)
		{
			return base.GetFieldAsZDecimal(fieldProperty.Name);
		}

		public ZInt GetFieldAsZInt(FlatFileFieldProperty fieldProperty)
		{
			return base.GetFieldAsZInt(fieldProperty.Name);
		}
		#endregion
		#endregion

		protected abstract void AddFieldProperties();

		public virtual int Length
		{
			get
			{
				int length = 0;
				foreach (FlatFileFieldProperty fieldProperty in FieldProperties)
				{
					length += fieldProperty.Length;
				}
				return length;
			}
		}

		public List<FlatFileFieldProperty> FieldProperties
		{
			get { return fieldProperties; }
			protected set { fieldProperties = value; }
		}
		List<FlatFileFieldProperty> fieldProperties;
	}
}
