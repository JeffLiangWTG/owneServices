using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class BMControlCustomisationLineValidation : ControlCustomisationValidationBase
	{
		public BMControlCustomisationLineValidation(BMControlCustomisationLine parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly BMControlCustomisationLine parent;

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidatePropertySource();
			ValidatePropertyName();
			ValidateBackgroundColor();
		}

		protected override void CheckControlType()
		{
			base.CheckControlType();

			if (!parent.ControlType.IsEmpty)
			{
				var property = parent.Property;
				if (property != null || parent.IsCustomField)
				{
					Type type = parent.IsCustomField ? parent.CustomFieldPropertyType : property.PropertyType;
					string userFriendlyFieldType = GetUserFriendlyFieldType(type);

					switch (parent.ControlType)
					{
						case PropertyTypeList.Codes.Boolean:
							CheckControlIsType(type, typeof(ZBool), ResString.GetMultilingualString("4a4401fe-3474-43b2-b8ef-045ff0eeecde", "Only true/false Properties are valid for this selected Type, however {0} is a {1} Property.", parent.PropertyNameDescription, userFriendlyFieldType));
							break;

						case PropertyTypeList.Codes.Date:
						case PropertyTypeList.Codes.DateTime:
							CheckControlIsType(type, new[] { typeof(ZDateTime), typeof(ZDateTimeOffset) }, ResString.GetMultilingualString("69120bbe-8cac-4d1b-beac-5a2ab82d6600", "Only Date or Date Time Properties are valid for this selected Type, however {0} is a {1} Property.", parent.PropertyNameDescription, userFriendlyFieldType));
							break;

						case PropertyTypeList.Codes.Duration:
							CheckControlIsType(type, typeof(ZDateTime), ResString.GetMultilingualString("10948951-d8cd-4196-a6dc-602abb0df43e", "Only duration Properties are valid for this selected Type, however {0} is a {1} Property.", parent.PropertyNameDescription, userFriendlyFieldType));
							break;

						case PropertyTypeList.Codes.Number:
							CheckControlIsType(type, typeof(INumericZType), ResString.GetMultilingualString("e8b3bf69-e7ec-455d-b6a1-054199597a5d", "Only number Properties are valid for this selected Type, however {0} is a {1} Property.", parent.PropertyNameDescription, userFriendlyFieldType));
							break;

						case PropertyTypeList.Codes.Text:
							if (parent.PropertySource == PropertySourceList.Codes.Job)
							{
								CheckControlIsType(type, typeof(ZString), ResString.GetMultilingualString("f81ac429-5633-4989-9121-6ed5a9df6b49", "Only text Properties are valid for this selected Type, however {0} is a {1} Property.", parent.PropertyNameDescription, userFriendlyFieldType));
							}
							break;
					}
				}
			}
		}

		void CheckControlIsType(Type actualType, Type expectedType, string error)
		{
			CheckControlIsType(actualType, new[] { expectedType }, error);
		}

		void CheckControlIsType(Type actualType, Type[] expectedTypes, string error)
		{
			if (actualType == null || !expectedTypes.Any(expectedType => expectedType.IsAssignableFrom(actualType)))
			{
				parent.ControlTypeInfo.AddError(error);
			}
		}

		static string GetUserFriendlyFieldType(Type type)
		{
			if (type != null)
			{
				if (type.IsAssignableFrom(typeof(ZBool)))
				{
					return ResString.GetMultilingualString("36b5c89a-cf78-45cd-9713-7c91906351e6", "true/false");
				}

				if (type.IsAssignableFrom(typeof(ZDateTime)))
				{
					return ResString.GetMultilingualString("b8e3c03c-e857-40d1-8042-fe221780e2dc", "date time");
				}

				if (type.IsAssignableFrom(typeof(INumericZType)) || type.GetInterface("CargoWise.Types.INumericZType") != null)
				{
					return ResString.GetMultilingualString("c81e9add-7221-4d7c-8fd5-1d3bbea7b4a3", "number");
				}

				if (type.IsAssignableFrom(typeof(ZString)))
				{
					return ResString.GetMultilingualString("ae085f30-d53a-4580-b1f2-a7f64ff56c75", "text");
				}
			}

			return ResString.GetMultilingualString("7ffb6f9a-6f82-4766-ae8b-bc8d50b42f21", "non-permissible");
		}

		public void ValidatePropertySource()
		{
			ValidateCalculatedProperty(parent.PropertySourceInfo);
		}

		protected void CheckPropertySource()
		{
			ListValidation.ErrorIfInvalidCode(parent.PropertySourceInfo);
			MandatoryValidation.CheckEntered(parent.PropertySourceInfo);

			if (parent.PropertySource == PropertySourceList.Codes.ProcessTask)
			{
				var controlType = parent.Parent.FM_ControlType;
				if (controlType == CustomisedControlTypeList.Codes.WorkflowDetailedCard || controlType == CustomisedControlTypeList.Codes.WorkflowSummaryCard)
				{
					parent.PropertySourceInfo.AddError(Res.GetString("97f059b9-e334-4695-9ff8-2b0d9c156f00", "Workflow cards may not display properties from tasks."));
				}
			}
		}

		public void ValidatePropertyName()
		{
			ValidateCalculatedProperty(parent.PropertyNameInfo);
		}

		protected void CheckPropertyName()
		{
			if (parent.PropertySource == PropertySourceList.Codes.Job)
			{
				if (!parent.Parent.FM_JobType.IsEmpty)
				{
					if (!parent.IsFirstOrFirstOrDefaultMacro)
					{
						var property = parent.Property;
						if (property == null && parent.CustomFieldPropertyType == null)
						{
							parent.PropertyNameInfo.AddError(ResString.GetMultilingualString("af9e81d5-63bd-4233-8818-efd3af11b229", "Property not found."));
						}
						else if (parent.CustomFieldPropertyType == typeof(ComboBoxCustomFieldType))
						{
							parent.PropertyNameInfo.AddError(ResString.GetMultilingualString("ccc51f9b-fca4-459e-8ca2-309c99a8d4a0", "Combo box custom fields are not supported."));
						}
					}
					else if (((string)parent.Parent.FM_ControlType).In(CustomisedControlTypeList.Codes.DetailedCard, CustomisedControlTypeList.Codes.WorkflowDetailedCard))
					{
						if (parent.IsFirstOrFirstOrDefaultMacro)
						{
							parent.PropertyNameInfo.AddError(Res.GetString("a00df817-ead1-4e31-ad40-1181d2b9cc6c", "Unable to render First or First Or Default macros on detailed cards."));
						}
					}
				}
				else
				{
					parent.PropertyNameInfo.AddError(Res.GetString("1E51EEBB-F117-4333-B519-7F45E636F943", "When property source is job, the job type should not empty."));
				}
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(parent.PropertyNameInfo);
			}
			MandatoryValidation.CheckEntered(parent.PropertyNameInfo);
		}

		protected override void CheckBackgroundColor()
		{
			base.CheckBackgroundColor();

			var isSupportTransparentBG = true;

			if (parent.Property != null && parent.BackgroundColor == Color.Transparent.Name)
			{
				if (parent.ControlType == PropertyTypeList.Codes.Text && !parent.IsReadOnly)
				{
					isSupportTransparentBG = false;
				}
				else if (UnsupportedTransparentBGControl.Contains(parent.ControlType.ToString()))
				{
					isSupportTransparentBG = false;
				}
			}

			if (!isSupportTransparentBG)
			{
				parent.BackgroundColorInfo.AddError(Res.GetString("800768d1-bf5f-465e-9c1e-b1b6ecba7bd5", "Transparent color is not supported on '{0}' type.", parent.ControlType));
			}
		}

		readonly IEnumerable<string> UnsupportedTransparentBGControl = new[] { PropertyTypeList.Codes.Number, PropertyTypeList.Codes.Duration };

		protected override void CheckIsReadOnly()
		{
			base.CheckIsReadOnly();

			if ((string)parent.Parent.FM_ControlType == CustomisedControlTypeList.Codes.TaskCard && !parent.IsReadOnly)
			{
				parent.IsReadOnlyInfo.AddError(Res.GetString("db134117-194f-43b6-bbb8-df358df1e7ba", "Summary task card properties must be read-only."));
			}
		}

		protected override void CheckOrientation()
		{
			base.CheckOrientation();

			if (parent.Orientation == nameof(BMBoardSectionOrientation.Vertical))
			{
				if (parent.ControlType == PropertyTypeList.Codes.Text)
				{
					if (!parent.IsReadOnly)
					{
						parent.OrientationInfo.AddError(Res.GetString("dcc664b2-1355-4c1c-acc9-6eb816f19c2b", "Vertical orientation is only supported for read-only properties."));
					}
				}
				else
				{
					parent.OrientationInfo.AddError(Res.GetString("ceb0f9dc-8ed5-4540-847f-04cafb48e1ae", "Vertical orientation is not supported for this property type."));
				}
			}
		}
	}
}
