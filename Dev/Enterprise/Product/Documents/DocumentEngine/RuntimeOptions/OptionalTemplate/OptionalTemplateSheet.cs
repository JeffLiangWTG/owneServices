using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class OptionalTemplateSheet : FilterField, IBindableBooleanItem, IJsonSerializable
	{
		public OptionalTemplateSheet(BusinessObjectFactory factory, string name, int order = 0)
			: base(factory)
		{
			Name = name;
			DisplayName = name;
			Order = order;
		}
		public readonly string Name;

		public OptionalTemplateSheet(BusinessObjectFactory factory)
			: base(factory)
		{
			Name = (NoResString)"None";
			DisplayName = (NoResString)"None";
		}

		public int Order { get; set; }

		#region Constructor For IJsonSerializable

		internal OptionalTemplateSheet(OptionalTemplateSheetJsonData data)
			: base(data)
		{
			Selected = data.Selected;
			Name = data.Name;
		}

		#endregion

		public ZBool Selected
		{
			get { return selected; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(SelectedInfo, ref selected, value);
				if (!IsValidationSuspended)
				{
					Validate();
				}
			}
		}
		ZBool selected;

		public override bool IsEmpty
		{
			get { return !Selected; }
		}

		public ZPropertyInfo SelectedInfo
		{
			get { return GetZPropertyInfo(nameof(Selected)); }
		}

		public void Validate()
		{
			SelectedInfo.ClearAllNotifications();

			if (!IsValid)
			{
				SelectedInfo.AddError(ValidationError);
			}
		}

		protected override bool FieldSpecificsIsCompatibleWith(FilterField otherFilterField)
		{
			return true;
		}

		public override void FillFilterData(ReportFilterData reportFilterData)
		{
			//Do nothing
		}

		public override void SetFilterValue(ReportFilterData reportFilterData)
		{
			//Do nothing
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validate();
		}

		protected override string NonEmptyWhereClause()
		{
			return String.Format("{0} = {1}", FieldName, Selected ? "Y" : "N");
		}

		public override FilterFieldSuggestedUserControlType SuggestedUserControlType
		{
			get
			{
				return FilterFieldSuggestedUserControlType.ZCheckBox;
			}
		}

		public override object ValueAsObject
		{
			get
			{
				return Selected;
			}
		}

		#region IJsonSerializable Members

		public object GetJsonData()
		{
			var result = new OptionalTemplateSheetJsonData
			{
				Selected = Selected,
				Name = Name
			};
			SetJsonData(result);
			return result;
		}

		#endregion

		#region IFilter Members

		public override void SafeCopyValuesFrom(IFilter source)
		{
			throw new NotImplementedException();
		}

		public override void ClearValues()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IBindableBooleanItem Members

		public ZBool BoolValue
		{
			get
			{
				return Selected;
			}
			set
			{
				Selected = value;
				BoolValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo BoolValueInfo
		{
			get { return GetZPropertyInfo(nameof(BoolValue)); }
		}

		public ZString Text
		{
			get { return DisplayName; }
		}

		#endregion
	}
}
