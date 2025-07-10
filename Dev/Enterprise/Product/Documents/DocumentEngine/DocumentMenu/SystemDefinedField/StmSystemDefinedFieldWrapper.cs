using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.SDF
{
	/// <summary>
	/// Wrapper object to hold the Value for each field
	/// </summary>
	[System.Diagnostics.DebuggerDisplay("{S1_Name} = {S1_Value}")]
	public class StmSystemDefinedFieldWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public static class Schema
		{
			public const string S1_Value = "S1_Value";
			public const string S1_Name = "S1_Name";
			public const string S1_Category = "S1_Category";
			public const string S1_Order = "S1_Order";
			public const string S1_Type = "S1_Type";
			public const string S1_Hint = "S1_Hint";
		}

		public StmSystemDefinedFieldWrapper(StmSystemDefinedField field)
		{
			fField = field;
		}

		#region S1_Name

		public ZString S1_Name
		{
			get { return fField.S1_NameMultilingual; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Get the value from database")]
		public ZString S1_NameFromDatabase => fField.S1_Name;

		public ZPropertyInfo S1_NameInfo
		{
			get { return fField == null ? null : fField.S1_NameInfo; }
		}

		#endregion

		#region S1_Category

		public ZString S1_Category
		{
			get { return fField.S1_CategoryMultilingual; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Get the value from database")]
		public ZString S1_CategoryFromDatabase => fField.S1_Category;

		public ZPropertyInfo S1_CategoryInfo
		{
			get { return fField == null ? null : fField.S1_CategoryInfo; }
		}

		#endregion

		#region S1_Order

		public ZShort S1_Order
		{
			get { return fField.S1_Order; }
		}

		public ZPropertyInfo S1_OrderInfo
		{
			get { return fField == null ? null : fField.S1_OrderInfo; }
		}

		#endregion

		#region S1_Type

		public ZString S1_Type
		{
			get { return fField.S1_Type; }
		}

		public ZPropertyInfo S1_TypeInfo
		{
			get { return fField == null ? null : fField.S1_TypeInfo; }
		}

		public ZString MultiColumnStyleType
		{
			get { return fField.MultiColumnStyleType; }
		}

		#endregion

		#region S1_Value

		public ZString S1_Value
		{
			get
			{
				return fS1_Value;
			}
			set
			{
				if (fS1_Value != value)
				{
					fS1_Value = value;
					S1_ValueInfo.RefreshBinding();
					HasChanges = true;
				}
			}
		}

		ZString fS1_Value;

		public ZPropertyInfo S1_ValueInfo
		{
			get { return GetZPropertyInfo(Schema.S1_Value); }
		}

		#endregion

		#region S1_Hint

		public ZString S1_Hint
		{
			get { return fField.S1_Hint; }
		}

		public ZPropertyInfo S1_HintInfo
		{
			get { return fField == null ? null : fField.S1_HintInfo; }
		}

		#endregion

		public ZString S1_DefaultValue
		{
			get
			{
				return ""; //TODO: To return proper default value
			}
		}

		#region Implementation

		readonly StmSystemDefinedField fField;

		#endregion
	}
}
