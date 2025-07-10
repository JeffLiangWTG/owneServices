using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.SDF
{
	public abstract class StmSystemDefinedFieldBase : AutoStmSystemDefinedField
	{
		public StmSystemDefinedFieldBase(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			S1_DisplayEditRule = StmSystemDefinedFieldLookups.DisplayEditRuleCodes.InGrid;
			S1_Validation = StmSystemDefinedFieldLookups.ValidationCodes.NoValidation;
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			UpdateRangeValueDecimalPlaces();
		}

		public BusinessObjectCollection ParentCollection
		{
			get
			{
				BusinessObjectCollection result = null;

				if (((IBusinessObjectInternals)this).ParentCollections.Length == 1 && ((IBusinessObjectInternals)this).ParentCollections[0].TypeOfElements == GetType())
				{
					result = ((IBusinessObjectInternals)this).ParentCollections[0];
				}

				return result;
			}
		}

		internal bool IsGrid
		{
			get { return (S1_Type == StmSystemDefinedFieldLookups.TypeCodes.Grid); }
		}

		internal bool IsTextOrIntOrDecimalType
		{
			get
			{
				return S1_Type == StmSystemDefinedFieldLookups.TypeCodes.Text ||
					S1_Type == StmSystemDefinedFieldLookups.TypeCodes.Integer ||
					S1_Type == StmSystemDefinedFieldLookups.TypeCodes.Decimal;
			}
		}

		#region ZProperty Overrides

		#region S1_Precision

		public override ZDecimal S1_Precision
		{
			get { return base.S1_Precision; }
			set
			{
				bool isValueChanged = (base.S1_Precision != value);
				base.S1_Precision = value;

				if (isValueChanged)
				{
					UpdateRangeValueDecimalPlaces();
				}
			}
		}

		#endregion

		#region S1_Type

		public override ZString S1_Type
		{
			get { return base.S1_Type; }
			set
			{
				bool isValueChanged = (base.S1_Type != value);
				base.S1_Type = value;

				if (isValueChanged)
				{
					S1_Precision = 0m;

					if (!IsTextOrIntOrDecimalType)
					{
						S1_LowerValue = 0;
						S1_UpperValue = 0;
					}
				}
			}
		}

		#endregion

		#region S1_Validation

		public override ZString S1_Validation
		{
			get { return base.S1_Validation; }
			set
			{
				bool isValueChanged = (base.S1_Validation != value);
				base.S1_Validation = value;

				if (isValueChanged && value != StmSystemDefinedFieldLookups.ValidationCodes.RangeValueRequired)
				{
					S1_LowerValue = 0;
					S1_UpperValue = 0;
				}
			}
		}

		#endregion

		#endregion

		#region ZPropertyInfo Overrides

		protected bool S1_LowerValue_ReadOnly
		{
			get { return (S1_Validation != StmSystemDefinedFieldLookups.ValidationCodes.RangeValueRequired || !IsTextOrIntOrDecimalType); }
		}

		protected bool S1_Precision_ReadOnly
		{
			get { return (S1_Type != StmSystemDefinedFieldLookups.TypeCodes.Decimal); }
		}

		protected bool S1_UpperValue_ReadOnly
		{
			get { return (S1_Validation != StmSystemDefinedFieldLookups.ValidationCodes.RangeValueRequired || !IsTextOrIntOrDecimalType); }
		}

		#endregion

		#region Range Value Decimal Places

		public ZInt RangeValueDecimalPlaces
		{
			get { return fRangeValueDecimalPlaces; }
		}

		public ZPropertyInfo RangeValueDecimalPlacesInfo
		{
			get { return GetZPropertyInfo(nameof(RangeValueDecimalPlaces)); }
		}

		void UpdateRangeValueDecimalPlaces()
		{
			if (S1_Type == StmSystemDefinedFieldLookups.TypeCodes.Decimal)
			{
				string precision = S1_Precision.ToString("n1");
				int fractionalPart = int.Parse(new string(precision[precision.Length - 1], 1));
				fRangeValueDecimalPlaces = (fractionalPart > 3) ? 0 : fractionalPart;
			}
			else
			{
				fRangeValueDecimalPlaces = 0;
			}

			RangeValueDecimalPlacesInfo.RefreshBinding();
		}

		ZInt fRangeValueDecimalPlaces;

		#endregion
	}
}
