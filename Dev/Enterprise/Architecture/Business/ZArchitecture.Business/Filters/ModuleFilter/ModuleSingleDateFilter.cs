using System;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	public delegate ZQuery GetSingleDateQuery(ZDateTime value);

	public class ModuleSingleDateFilter : ModuleFilter
	{
		#region Construction

		protected ModuleSingleDateFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public ModuleSingleDateFilter(ZString description, SchemaDateTimeColumn filterColumn)
			: base(description, filterColumn)
		{
		}

		public ModuleSingleDateFilter(ZString description, GetSingleDateQuery queryDelegate)
			: base(description, queryDelegate)
		{
		}

		#endregion

		#region Property1

		public ZDateTime Property1
		{
			get { return property1; }
			set
			{
				if (property1 != value)
				{
					property1 = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty1();
					}
					Property1Info.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo Property1Info
		{
			get { return GetZPropertyInfo(nameof(Property1)); }
		}

		ZDateTime property1;

		#region Property1Validation

		public Validation Property1Validation
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return property1Validation; }
			[System.Diagnostics.DebuggerStepThrough]
			set { property1Validation = value; }
		}
		Validation property1Validation;

		#endregion

		#endregion

		#region DateTimeFormat

		public ZDateTimePickerFormat DateTimeFormat
		{
			get => dateTimeFormat;
			set => dateTimeFormat = value;
		}
		ZDateTimePickerFormat dateTimeFormat = ZDateTimePickerFormat.Short;

		#endregion

		protected override void ClearCore()
		{
			Property1 = ZDateTime.Empty;
		}

		protected override bool IsEmptyCore => Property1.IsEmpty;

		protected internal override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new ModuleSingleDateFilter(category, parentCollection);
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			var filter = (ModuleSingleDateFilter)filterToCopyFrom;
			Property1 = filter.Property1;
		}

		public override bool IsExpensiveQuery
		{
			get { return false; }
		}

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.Dates; }
		}

		public new ModuleFilterSingleDateValidation Validation
		{
			get { return (ModuleFilterSingleDateValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ModuleFilterSingleDateValidation(this);
		}

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { Property1 }; }
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			var result = new ZQuery();
			if (Property1.IsValid)
			{
				result.AddToFilter(FilterColumn, Property1);
			}
			return result;
		}

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			string sqlDate1 = Property1.IsValid ? Property1.SqlFormat : ZString.Empty;
			writer.WriteElementString("Property1", sqlDate1);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			ZString dateString1 = (reader.Name == "Property1") ? reader.ReadElementString("Property1") : "";
			if (!dateString1.IsEmpty)
			{
				try
				{
					Property1 = ZDateTime.FromSqlFormat(dateString1);
				}
				catch (FormatException) { }
			}
		}

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			Property1 = ZDateTime.BrettsBirthday;
		}

#endif
		#endregion

		#region class Validation

		public class ModuleFilterSingleDateValidation : ModuleFilterValidation
		{
			public ModuleFilterSingleDateValidation(ModuleSingleDateFilter parent)
				: base(parent)
			{
				Parent = parent;
			}

			#region ValidateProperty1

			public void ValidateProperty1()
			{
				ValidateCalculatedProperty(Parent.Property1Info);
			}

			protected void CheckProperty1()
			{
				TypeValidation.CheckValidZDateTimeWithoutRange(Parent.Property1Info);
				TypeValidation.CheckValidZDateTimeRange(Parent.Property1Info);

				if (Parent.Property1Validation != null)
				{
					Parent.Property1Validation(Parent.Property1Info);
				}
			}

			#endregion

			public override void ValidateAll()
			{
				ValidateProperty1();
			}

			public override Type AutoValidationType
			{
				get { return this.GetType(); }
			}

			protected readonly ModuleSingleDateFilter Parent;
		}

		#endregion
	}
}
