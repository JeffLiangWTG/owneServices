using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.SDF
{
	public abstract class StmSystemDefinedFieldBaseValidation : StmSystemDefinedFieldValidation
	{
		public StmSystemDefinedFieldBaseValidation(StmSystemDefinedFieldBase parent) : base(parent)
		{
		}

		protected new StmSystemDefinedFieldBase Parent
		{
			get { return (StmSystemDefinedFieldBase)base.Parent; }
		}

		protected override void CheckS1_Category()
		{
			base.CheckS1_Category();
			MandatoryValidation.CheckEntered(Parent.S1_CategoryInfo);
			TranslatableDataFieldAttribute.Validate(Parent.S1_CategoryInfo);
		}

		protected override void CheckS1_Default()
		{
			base.CheckS1_Default();

			ListValidation.ErrorIfInvalidCode(Parent.S1_DefaultInfo, Parent.Lookups.Defaults);

			if (!Parent.S1_Default.IsEmpty && !Parent.S1_DefaultInfo.HasErrors())
			{
				string description = Parent.Lookups.Defaults.GetDescriptionFromCode(Parent.S1_Default);

				if ((Parent.S1_Type == StmSystemDefinedFieldLookups.TypeCodes.DateOnly ||
					Parent.S1_Type == StmSystemDefinedFieldLookups.TypeCodes.DateTime) &&
					description != nameof(ZDateTime))
				{
					Parent.S1_DefaultInfo.AddError(Res.GetString("3dadb135-be21-4360-945c-9b6357cfb12c", "Please select a Date/Time field."));
				}
				else if (Parent.S1_Type == StmSystemDefinedFieldLookups.TypeCodes.Decimal &&
					description != nameof(ZDecimal))
				{
					Parent.S1_DefaultInfo.AddError(Res.GetString("a6fbcf1c-13f8-4a32-80ac-09583865a875", "Please select a Decimal field."));
				}
				else if (Parent.S1_Type == StmSystemDefinedFieldLookups.TypeCodes.Integer &&
					description != nameof(ZByte) && description != nameof(ZInt) && description != nameof(ZShort))
				{
					Parent.S1_DefaultInfo.AddError(Res.GetString("4c95b2d8-41e2-450b-b4da-5793418b5cd3", "Please select a Byte, Integer or Short field."));
				}
				else if (Parent.S1_Type == StmSystemDefinedFieldLookups.TypeCodes.Boolean &&
					description != nameof(ZBool))
				{
					Parent.S1_DefaultInfo.AddError(Res.GetString("7dfe6fdb-850f-4574-a005-a0a0822e3b05", "Please select a Boolean field."));
				}
			}
		}

		protected override void CheckS1_DisplayEditRule()
		{
			base.CheckS1_DisplayEditRule();
			MandatoryValidation.CheckEntered(Parent.S1_DisplayEditRuleInfo);
			ListValidation.ErrorIfInvalidCode(Parent.S1_DisplayEditRuleInfo, Parent.Lookups.DisplayEditRules);
		}

		protected override void CheckS1_Hint()
		{
			base.CheckS1_Hint();
			MandatoryValidation.CheckEntered(Parent.S1_HintInfo);
		}

		protected override void CheckS1_Name()
		{
			base.CheckS1_Name();
			MandatoryValidation.CheckEntered(Parent.S1_NameInfo);

			if (!Parent.S1_NameInfo.HasErrors() && Parent.ParentCollection != null)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.S1_NameInfo);
			}

			TranslatableDataFieldAttribute.Validate(Parent.S1_NameInfo);
		}

		protected override void CheckS1_Precision()
		{
			base.CheckS1_Precision();

			if (!Parent.S1_PrecisionInfo.HasErrors())
			{
				if (Parent.S1_Type == StmSystemDefinedFieldLookups.TypeCodes.Decimal)
				{
					CompareValidation.CheckWithinRange(Parent.S1_PrecisionInfo, 1.0m, 9.3m);

					if (!Parent.S1_PrecisionInfo.HasErrors())
					{
						string precision = Parent.S1_Precision.ToString("n1");
						int fractionalPart = int.Parse(new string(precision[precision.Length - 1], 1));

						if (fractionalPart > 3)
						{
							Parent.S1_PrecisionInfo.AddError(Res.GetString("c672d1c7-7d20-43fc-b7e1-0149c2a3992d", "Please enter a fractional part within the range .0 to .3."));
						}
						else if (fractionalPart > Math.Floor((double)Parent.S1_Precision))
						{
							Parent.S1_PrecisionInfo.AddError(Res.GetString("a4c21d8c-1c90-4a2e-b479-ac19f2bd2ea3", "Please enter a Precision that is larger than its fractional part."));
						}
					}
				}
				else
				{
					MandatoryValidation.CheckNotEntered(Parent.S1_PrecisionInfo);
				}
			}
		}

		protected override void CheckS1_Type()
		{
			base.CheckS1_Type();

			MandatoryValidation.CheckEntered(Parent.S1_TypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.S1_TypeInfo, Parent.Lookups.Types);

			ValidateS1_Validation();
			ValidateS1_LowerValue();
			ValidateS1_UpperValue();
		}

		protected override void CheckS1_Validation()
		{
			base.CheckS1_Validation();

			MandatoryValidation.CheckEntered(Parent.S1_ValidationInfo);
			ListValidation.ErrorIfInvalidCode(Parent.S1_ValidationInfo, Parent.Lookups.Validations);

			if (!Parent.S1_ValidationInfo.HasErrors() && Parent.S1_Validation == StmSystemDefinedFieldLookups.ValidationCodes.RangeValueRequired && !Parent.IsTextOrIntOrDecimalType)
			{
				Parent.S1_ValidationInfo.AddError(Res.GetString("1230b36e-2a57-4f85-849f-7e6b66fbd6ab", "'Range Value Required' is only valid if Type is 'Text', 'Integer' or 'Decimal'."));
			}
		}

		protected void CheckOrder(ZPropertyInfo propertyInfo)
		{
			CompareValidation.CheckNumberGreaterThanZero(propertyInfo);

			if (!propertyInfo.HasErrors() && Parent.ParentCollection != null)
			{
				bool isInSequence = false;
				var propertyValue = (ZShort)propertyInfo.Value;
				var propertyValueMinusOne = propertyValue - 1;

				foreach (StmSystemDefinedFieldBase field in Parent.ParentCollection)
				{
					if (field != Parent)
					{
						if (field[propertyInfo.Name].Equals(propertyValue))
						{
							propertyInfo.AddError(Res.GetString("bbaf433c-46d5-403a-b57f-1df55ebe72db", "Please enter a unique Order."));
							break;
						}
						else if (field[propertyInfo.Name].Equals(propertyValueMinusOne))
						{
							isInSequence = true;
						}
					}
				}

				if (!propertyInfo.HasErrors() && propertyValue != 1 && !isInSequence)
				{
					propertyInfo.AddError(Res.GetString("3e530b18-f6e1-4eff-9382-b06bc25b4896", "The Order is out of sequence as there is no Order number {0}. Please adjust the Order numbers so that they are in sequence.", propertyValueMinusOne));
				}
			}
		}

		#region S1_Lower/UpperValue

		protected override void CheckS1_LowerValue()
		{
			base.CheckS1_LowerValue();

			if (!Parent.S1_LowerValueInfo.HasErrors())
			{
				CheckRangeValue(Parent.S1_LowerValueInfo);

				if (!Parent.S1_LowerValueInfo.HasErrors() && ShouldValidateRangeValue && Parent.S1_LowerValue >= Parent.S1_UpperValue)
				{
					Parent.S1_LowerValueInfo.AddError(Res.GetString("6e042799-b24d-42bb-80be-ef22fb210af3", "Please enter a value less than the Upper Value."));
				}
			}

			if (Parent.S1_UpperValueInfo.HasErrors())
			{
				ValidateS1_UpperValue();
			}
		}

		protected override void CheckS1_UpperValue()
		{
			base.CheckS1_UpperValue();

			if (!Parent.S1_UpperValueInfo.HasErrors())
			{
				CheckRangeValue(Parent.S1_UpperValueInfo);

				if (!Parent.S1_UpperValueInfo.HasErrors() && ShouldValidateRangeValue && Parent.S1_UpperValue <= Parent.S1_LowerValue)
				{
					Parent.S1_UpperValueInfo.AddError(Res.GetString("a9b66726-f3b2-4b1b-9662-509563d56b71", "Please enter a value greater than the Lower Value."));
				}
			}

			if (Parent.S1_LowerValueInfo.HasErrors())
			{
				ValidateS1_LowerValue();
			}
		}

		void CheckRangeValue(ZPropertyInfo propertyInfo)
		{
			if (ShouldValidateRangeValue)
			{
				if (Parent.S1_Type == StmSystemDefinedFieldLookups.TypeCodes.Decimal)
				{
					ValidateS1_Precision();

					if (Parent.S1_PrecisionInfo.HasErrors())
					{
						propertyInfo.AddError(Res.GetString("cdde3f20-1c67-460a-869e-2c37bfd3c0e2", "Please fix the errors on Precision first before changing this value."));
					}
					else
					{
						CompareValidation.CheckNumberNotNegative(propertyInfo);

						if (!Parent.S1_PrecisionInfo.HasErrors())
						{
							string[] precisionParts = Parent.S1_Precision.ToString("n1").Split('.');
							TypeValidation.CheckValidDecimal(propertyInfo, int.Parse(precisionParts[0]), int.Parse(precisionParts[1]));
						}
					}
				}
			}
			else
			{
				MandatoryValidation.CheckNotEntered(propertyInfo);
			}
		}

		bool ShouldValidateRangeValue
		{
			get { return (Parent.S1_Validation == StmSystemDefinedFieldLookups.ValidationCodes.RangeValueRequired && Parent.IsTextOrIntOrDecimalType); }
		}

		#endregion
	}
}
