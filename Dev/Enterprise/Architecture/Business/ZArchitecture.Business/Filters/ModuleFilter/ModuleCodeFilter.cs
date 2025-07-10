using System;
using System.Collections;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public delegate ZQuery GetCodeQuery(ZString value1, ZString value2);

	public abstract class ModuleCodeFilter : ModuleFilterWithLists
	{
		#region Construction

		protected ModuleCodeFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public ModuleCodeFilter(ZString description, SchemaStringColumn filterColumn1, IBusinessObjectCollection list1, SchemaStringColumn filterColumn2, IBusinessObjectCollection list2)
			: base(description, filterColumn1, list1, filterColumn2, list2)
		{
		}

		public ModuleCodeFilter(ZString description, GetCodeQuery queryDelegate, IBusinessObjectCollection list1, IBusinessObjectCollection list2)
			: base(description, queryDelegate, list1, list2)
		{
		}

		public ModuleCodeFilter(ZString description, GetCodeQuery queryDelegate, IList list1, IList list2)
			: base(description, queryDelegate, list1, list2)
		{
		}

		public ModuleCodeFilter(ZString description, GetCodeQuery queryDelegate, GetList list1Delegate, GetList list2Delegate)
			: base(description, queryDelegate, list1Delegate, list2Delegate)
		{
		}

		#endregion

		#region CopyPersistantValuesFromFilter

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			var filter = (ModuleCodeFilter)filterToCopyFrom;
			Property1 = filter.Property1;
			Property2 = filter.Property2;
		}

		#endregion

		#region IsExpensiveQuery

		public override bool IsExpensiveQuery
		{
			get { return false; }
		}

		#endregion

		#region Clear / IsEmpty / Default

		protected override void ClearCore()
		{
			Property1 = DefaultProperty1;
			Property2 = DefaultProperty2;
		}

		protected override bool IsEmptyCore => Property1.IsEmpty && Property2.IsEmpty;

		public ZString DefaultProperty1
		{
			get { return fDefaultProperty1; }
			set
			{
				fDefaultProperty1 = value;
				Property1 = value;
			}
		}

		public ZString DefaultProperty2
		{
			get { return fDefaultProperty2; }
			set
			{
				fDefaultProperty2 = value;
				Property2 = value;
			}
		}

		ZString fDefaultProperty1;
		ZString fDefaultProperty2;

		#endregion

		#region Property1

		[BusinessObjectTestExclude] // don't need a maxlength
		public virtual ZString Property1
		{
			get { return fProperty1; }
			set
			{
				if (fProperty1 != value)
				{
					fProperty1 = value;
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

		ZString fProperty1;

		#endregion

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

		#region Property2

		[BusinessObjectTestExclude] // don't need a maxlength
		public virtual ZString Property2
		{
			get { return fProperty2; }
			set
			{
				if (fProperty2 != value)
				{
					fProperty2 = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty2();
					}
					Property2Info.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo Property2Info
		{
			get { return GetZPropertyInfo(nameof(Property2)); }
		}

		ZString fProperty2;

		#endregion

		#region Property2Validation

		public Validation Property2Validation
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return property2Validation; }
			[System.Diagnostics.DebuggerStepThrough]
			set { property2Validation = value; }
		}
		Validation property2Validation;

		#endregion

		#region Validation

		public new ModuleCodeFilterValidation Validation
		{
			get { return (ModuleCodeFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ModuleCodeFilterValidation(this);
		}

		#endregion

		#region Query

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { Property1, Property2 }; }
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			var result = new ZQuery();
			if (!Property1.IsEmpty)
			{
				result.AddToFilter(FilterColumn1, Property1);
			}

			if (!Property2.IsEmpty)
			{
				result.AddToFilter(FilterColumn2, Property2);
			}

			return result;
		}

		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			writer.WriteElementString("Property1", Property1);
			writer.WriteElementString("Property2", Property2);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			if (reader.Name == "Property1")
			{
				Property1 = reader.ReadElementString("Property1");
			}

			if (reader.Name == "Property2")
			{
				Property2 = reader.ReadElementString("Property2");
			}
		}

		#endregion

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			Property1 = RandomString(MaxLength);
		}

#endif
		#endregion
	}

	#region class Validation

	public class ModuleCodeFilterValidation : ModuleFilterValidation
	{
		public ModuleCodeFilterValidation(ModuleCodeFilter parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		#region ValidateProperty1

		public void ValidateProperty1()
		{
			ValidateCalculatedProperty(Parent.Property1Info);
		}

		protected void CheckProperty1()
		{
			var list = Parent.List1;
			if (list != null)
			{
				ErrorIfInvalidCode(Parent.Property1Info, list);
			}

			if (Parent.Property1Validation != null)
			{
				Parent.Property1Validation(Parent.Property1Info);
			}
		}

		#endregion

		#region ValidateProperty2

		public void ValidateProperty2()
		{
			ValidateCalculatedProperty(Parent.Property2Info);
		}

		protected void CheckProperty2()
		{
			var list = Parent.List2;
			if (list != null)
			{
				ErrorIfInvalidCode(Parent.Property2Info, list);
			}
			if (Parent.Property2Validation != null)
			{
				Parent.Property2Validation(Parent.Property2Info);
			}
		}

		#endregion

		#region Validate All

		public override void ValidateAll()
		{
			ValidateProperty1();
			ValidateProperty2();
		}

		public override Type AutoValidationType
		{
			get { return this.GetType(); }
		}

		protected readonly ModuleCodeFilter Parent;

		#endregion
	}

	#endregion
}
