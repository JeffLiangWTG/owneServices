using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class OperationalActionFieldDescriptorValidation : AutoOperationalActionFieldDescriptorValidation
	{
		public OperationalActionFieldDescriptorValidation(AutoOperationalActionFieldDescriptor parent)
			: base(parent) { }

		protected override void CheckFieldCaption()
		{
			CheckEnteredAndUnique(Parent.FieldCaptionInfo);
		}

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", Justification = "We are constructing a ZGeography to see if it throws an exception or not.")]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal Strings are safe to use in this context")]
		protected override void CheckDefaultValue()
		{
			base.CheckDefaultValue();
			if (Parent.DefaultValueFieldType.Equals("DateTime") || Parent.DefaultValueFieldType.Equals("DateTimeOffset"))
			{
				try
				{
					new ZDateTime(Parent.DefaultValue);
				}
				catch (ZTypeValueException)
				{
					Parent.DefaultValueInfo.AddError(Res.GetString("8CAF1A8F-D751-4A9B-BA87-50D0CE609A47", "Please pick a valid date."));
				}
			}
			else if (Parent.DefaultValueFieldType.Equals("Time"))
			{
				try
				{
					new ZTime(Parent.DefaultValue);
				}
				catch (ZTypeValueException)
				{
					Parent.DefaultValueInfo.AddError(Res.GetString("F7D1C7AB-27A1-4030-AE8F-81259D349571", "Please pick a valid time."));
				}
			}
			else if (Parent.DefaultValueFieldType.Equals("Geography"))
			{
				try
				{
					new ZGeography(Parent.DefaultValue);
				}
				catch (ZTypeValueException)
				{
					Parent.DefaultValueInfo.AddError(Res.GetString("a250e7b6-7d76-4f9c-8945-6344e82a098a", "Please pick a valid geography."));
				}
			}
		}

		protected override void CheckFieldName()
		{
			MandatoryValidation.CheckEntered(Parent.FieldNameInfo);

			var errorMessageFieldBulkUpdateNotAllowed = Res.GetString("149f75e3-1b51-43cd-8861-cd7f44bbbc03", "This field cannot be bulk updated.");

			if (Parent.ParentCollections.Count > 0)
			{
				var collection = Parent.ParentCollections.First();

				foreach (OperationalActionFieldDescriptor descriptor in collection)
				{
					if (descriptor.PK != Parent.PK && descriptor.FieldName == Parent.FieldName && (descriptor.Filter.IsEmpty || Parent.Filter.IsEmpty))
					{
						Parent.FieldNameInfo.AddError(Res.GetString("db252457-e75d-43eb-b816-27875bbe929a", "Specifying the same field more than once is not meaningful without also specifying filters."));
					}
				}
			}

			if (Parent.Context == null)
			{
				return;
			}

			var field = Parent.Context.FieldSupporters[Parent.FieldName];

			if (field == null && Parent.TemplateNamesForCustomFieldNotInCurrentContext.Count > 0)
			{
				var warningBuilder = new StringBuilder();
				warningBuilder.AppendLine(Res.GetString("06423EC7-23B0-4158-9108-25D6DC0C93D8", "This field is a custom field from the following templates not available in current context:"));

				Parent.TemplateNamesForCustomFieldNotInCurrentContext.ForEach(s => warningBuilder.AppendLine(s));
				Parent.FieldNameInfo.AddWarning(warningBuilder.ToString());
			}
			else if (Parent.HasChanges)
			{
				if (field == null)
				{
					Parent.FieldNameInfo.AddError(Res.GetString("287DEA10-7A89-4653-86B3-A205B47C2796", "Select a valid field."));
				}
				else if (field.ReadOnly)
				{
					Parent.FieldNameInfo.AddError(errorMessageFieldBulkUpdateNotAllowed);
				}
			}
		}

		protected override void CheckFilter()
		{
			base.CheckFilter();

			try
			{
				IFilterExpression expression = FilterParser.Parse(Parent.Filter);
				IList<string> constraints = expression.GetConstraints();
				IFilterExpression[] subexpressions = expression.GetSubExpressions().Length > 0 ? expression.GetSubExpressions() : new IFilterExpression[1] { expression };

				foreach (string constraint in constraints)
				{
					OperationalActionFieldSupporter supporter = Parent.Context.FieldSupporters[constraint];

					if (supporter == null)
					{
						ValidateType = false;
						Parent.FilterInfo.AddError(Res.GetString("b5f5acdb-8ee9-4974-ad40-a934d080fe02", "'{0}' is not a valid field.", constraint));
					}
				}
				if (ValidateType)
				{
					foreach (IFilterExpression subexpression in subexpressions)
					{
						if (subexpression.ToString().Contains("<") || subexpression.ToString().Contains(">"))
						{
							var subconstraint = subexpression.GetConstraints()[0];
							var type = Parent.Context.FieldSupporters.GetFieldType(subconstraint);
							if (Array.IndexOf(allowedTypes, type) == -1)
							{
								Parent.FilterInfo.AddError(Res.GetString("F31FDFD0-3664-4785-8E8E-FA0C460EE7E7", "'{0}' is of type '{1}', which is not valid for greater than/less than comparisons.", subconstraint, type.Name));
							}
						}
					}
				}
			}
			catch (ParseException ex)
			{
				Parent.FilterInfo.AddError(ex.Message);
			}
		}

		readonly Type[] allowedTypes = new Type[9] { typeof(int), typeof(decimal), typeof(DateTime), typeof(string), typeof(double), typeof(ZInt), typeof(ZDecimal), typeof(ZDateTime), typeof(ZString) };

		bool ValidateType = true;

		protected override void CheckOrder()
		{
			CheckEnteredAndUnique(Parent.OrderInfo);
		}

		protected override void CheckEmptyBehaviour()
		{
			MandatoryValidation.CheckEntered(Parent.EmptyBehaviourInfo);
			ListValidation.ErrorIfInvalidCode(Parent.EmptyBehaviourInfo, Parent.Lookups.EmptyBehaviour_List);
		}

		protected override void CheckDefaultingStrategy()
		{
			base.CheckDefaultingStrategy();

			if (!Parent.DefaultingStrategyInfo.ReadOnly || Parent.TemplateNamesForCustomFieldNotInCurrentContext.Count <= 0)
			{
				ListValidation.ErrorIfInvalidCode(Parent.DefaultingStrategyInfo, Parent.Lookups.DefaultStrategy_List);
			}
		}

		#region Implementation

		void CheckEnteredAndUnique(ZPropertyInfo propertyInfo)
		{
			MandatoryValidation.CheckEntered(propertyInfo);
			if (Parent.ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(propertyInfo);
			}
		}

		public new OperationalActionFieldDescriptor Parent
		{
			get { return (OperationalActionFieldDescriptor)base.Parent; }
		}

		#endregion
	}
}
