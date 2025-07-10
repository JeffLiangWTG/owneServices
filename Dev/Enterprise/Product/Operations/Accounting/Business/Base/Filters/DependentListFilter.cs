using System;
using System.Collections;
using System.Xml;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using DependentListValidation = Enterprise.ZArchitecture.Business.Validation;

namespace Enterprise.Accounting.Business.Base.Filters
{
	[SuppressWeaklyTypedCollectionMessage]
	public delegate IList GetDependentList(ZString value);

	public delegate ZQuery GetDependentListQuery(ZString value1, ZString value2);

	public class DependentListFilter : ModuleFilter
	{
		#region Construction

		public DependentListFilter(ZString description, SchemaStringColumn filterColumn1, SchemaStringColumn filterColumn2, IList list1, IList list2)
			: base(description)
		{
			EnsureListIsNotNull(list1, list2);
			EnsureFilterColumnIsNotNull(filterColumn1, filterColumn2);

			this.FilterColumn1 = filterColumn1;
			this.FilterColumn2 = filterColumn2;
			this.fList1 = list1;
			this.fList2 = list2;
		}

		public DependentListFilter(ZString description, SchemaStringColumn filterColumn1, SchemaStringColumn filterColumn2, GetList list1Delegate, IList list2)
			: base(description)
		{
			EnsureListDelegateIsNotNull(list1Delegate);
			EnsureListIsNotNull(list2);
			EnsureFilterColumnIsNotNull(filterColumn1, filterColumn2);

			this.FilterColumn1 = filterColumn1;
			this.FilterColumn2 = filterColumn2;
			this.fList1Delegate = list1Delegate;
			this.fList2 = list2;
		}

		public DependentListFilter(ZString description, SchemaStringColumn filterColumn1, SchemaStringColumn filterColumn2, IList list1, GetDependentList list2Delegate)
			: base(description)
		{
			EnsureListIsNotNull(list1);
			EnsureListDelegateIsNotNull(list2Delegate);
			EnsureFilterColumnIsNotNull(filterColumn1, filterColumn2);

			this.FilterColumn1 = filterColumn1;
			this.FilterColumn2 = filterColumn2;
			this.fList1 = list1;
			this.fList2Delegate = list2Delegate;
		}

		public DependentListFilter(ZString description, SchemaStringColumn filterColumn1, SchemaStringColumn filterColumn2, GetList list1Delegate, GetDependentList list2Delegate)
			: base(description)
		{
			EnsureListDelegateIsNotNull(list1Delegate);
			EnsureListDelegateIsNotNull(list2Delegate);
			EnsureFilterColumnIsNotNull(filterColumn1, filterColumn2);

			this.FilterColumn1 = filterColumn1;
			this.FilterColumn2 = filterColumn2;
			this.fList1Delegate = list1Delegate;
			this.fList2Delegate = list2Delegate;
		}

		public DependentListFilter(ZString description, GetDependentListQuery queryDelegate, IList list1, IList list2)
			: base(description, queryDelegate)
		{
			EnsureListIsNotNull(list1, list2);

			this.fList1 = list1;
			this.fList2 = list2;
		}

		public DependentListFilter(ZString description, GetDependentListQuery queryDelegate, GetList list1Delegate, IList list2)
			: base(description, queryDelegate)
		{
			EnsureListDelegateIsNotNull(list1Delegate);
			EnsureListIsNotNull(list2);

			this.fList1Delegate = list1Delegate;
			this.fList2 = list2;
		}

		public DependentListFilter(ZString description, GetDependentListQuery queryDelegate, IList list1, GetDependentList list2Delegate)
			: base(description, queryDelegate)
		{
			EnsureListIsNotNull(list1);
			EnsureListDelegateIsNotNull(list2Delegate);

			this.fList1 = list1;
			this.fList2Delegate = list2Delegate;
		}

		public DependentListFilter(ZString description, GetDependentListQuery queryDelegate, GetList list1Delegate, GetDependentList list2Delegate)
			: base(description, queryDelegate)
		{
			EnsureListDelegateIsNotNull(list1Delegate);
			EnsureListDelegateIsNotNull(list2Delegate);

			this.fList1Delegate = list1Delegate;
			this.fList2Delegate = list2Delegate;
		}

		public DependentListFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		#endregion

		#region Ensure Not Null

		protected void EnsureListDelegateIsNotNull(params GetDependentList[] listDelegates)
		{
			foreach (GetDependentList listDelegate in listDelegates)
			{
				if (listDelegate == null)
				{
					throw new NullReferenceException(GetType().Name + " " + Description + " listDelegate cannot be null.");
				}
			}
		}

		#endregion

		#region Lists

		readonly IList fList1;
		readonly IList fList2;
		readonly GetList fList1Delegate;
		readonly GetDependentList fList2Delegate;
		public readonly SchemaStringColumn FilterColumn1;
		public readonly SchemaStringColumn FilterColumn2;

		[SuppressWeaklyTypedCollectionMessage]
		public IList List1
		{
			get
			{
				IList result = null;

				if (fList1 != null)
				{
					result = fList1;
				}
				else if (fList1Delegate != null)
				{
					result = fList1Delegate.Invoke();
				}

				return result;
			}
		}

		[SuppressWeaklyTypedCollectionMessage]
		public IList List2
		{
			get
			{
				IList result = null;

				if (fList2 != null)
				{
					result = fList2;
				}
				else if (fList2Delegate != null)
				{
					result = fList2Delegate.Invoke(Property1);
				}

				return result;
			}
		}

		#endregion

		#region Property1

		[BusinessObjectTestExclude] // don't need a maxlength
		[List("List1")]
		public virtual ZString Property1
		{
			get { return fProperty1; }
			set
			{
				if (fProperty1 != value)
				{
					fProperty1 = value.Trim(' ');
					Property2 = DefaultProperty2;

					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty1();
						Validation.ValidateProperty2();
					}

					Property1Info.RefreshBinding();
					Property2Info.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo Property1Info
		{
			get { return GetZPropertyInfo(nameof(Property1)); }
		}

		ZString fProperty1;

		public DependentListValidation Property1Validation
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fProperty1Validation; }
			[System.Diagnostics.DebuggerStepThrough]
			set { fProperty1Validation = value; }
		}

		DependentListValidation fProperty1Validation;

		#endregion

		#region Property2

		[BusinessObjectTestExclude] // don't need a maxlength
		[List("List2")]
		public virtual ZString Property2
		{
			get { return fProperty2; }
			set
			{
				if (fProperty2 != value)
				{
					fProperty2 = value.Trim(' ');
					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty2();
					}
					Property2Info.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		ZString fProperty2;

		public ZPropertyInfo Property2Info
		{
			get { return GetZPropertyInfo(nameof(Property2)); }
		}

		public DependentListValidation Property2Validation
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fProperty2Validation; }
			[System.Diagnostics.DebuggerStepThrough]
			set { fProperty2Validation = value; }
		}
		DependentListValidation fProperty2Validation;

		#endregion

		#region GetNewCommonModuleFilter, CopyPersistantValuesFromFilter

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new DependentListFilter(category, parentCollection);
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			DependentListFilter filter = (DependentListFilter)filterToCopyFrom;
			Property1 = filter.Property1;
			Property2 = filter.Property2;
		}

		#endregion

		#region Clear / IsEmpty / Defaults

		protected override void ClearCore()
		{
			Property1 = DefaultProperty1;
			Property2 = DefaultProperty2;
		}

		protected override bool IsEmptyCore => Property1.IsEmpty && Property2.IsEmpty;

		public override bool IsExpensiveQuery => false;

		protected override FilterCategory DefaultCategory => FilterCategories.TextSearch;

		ZString fDefaultProperty1 = String.Empty;
		public ZString DefaultProperty1
		{
			get { return fDefaultProperty1; }
			set
			{
				if (DefaultProperty1 != value)
				{
					InvalidateCachedQuery();
				}

				fDefaultProperty1 = value;
				Property1 = value;
			}
		}

		ZString fDefaultProperty2 = String.Empty;
		public ZString DefaultProperty2
		{
			get { return fDefaultProperty2; }
			set
			{
				if (DefaultProperty2 != value)
				{
					InvalidateCachedQuery();
				}

				fDefaultProperty2 = value;
				Property2 = value;
			}
		}

		#endregion

		#region Validation

		public new DependentListFilterValidation Validation
		{
			get { return (DependentListFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new DependentListFilterValidation(this);
		}

		#endregion

		#region Query

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { Property1, Property2 }; }
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			ZQuery result = new ZQuery();

			if (!Property1.IsEmpty)
			{
				result.AddToFilter(FilterColumn1, SQLComparisonOperator.Equal, Property1);
			}

			if (!Property2.IsEmpty)
			{
				result.AddToFilter(FilterColumn2, SQLComparisonOperator.Equal, Property2);
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
			Property2 = RandomString(MaxLength);
		}

#endif
		#endregion
	}

	#region class Validation

	public class DependentListFilterValidation : ModuleFilterValidation
	{
		public DependentListFilterValidation(DependentListFilter parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		#region ValidateProperty1

		public void ValidateProperty1()
		{
			ValidateCalculatedProperty(Parent.Property1Info);
		}

		protected virtual void CheckProperty1()
		{
			ICodeDescriptionPairList codeDescPairList = Parent.List1 as ICodeDescriptionPairList;
			if (codeDescPairList != null)
			{
				ListValidation.ErrorIfInvalidCode(Parent.Property1Info, codeDescPairList);
			}
			else
			{
				BusinessObjectCollection collection = Parent.List1 as BusinessObjectCollection;
				if (collection != null)
				{
					ListValidation.ErrorIfInvalidCode(Parent.Property1Info, collection);
				}
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

		protected virtual void CheckProperty2()
		{
			ICodeDescriptionPairList codeDescPairList = Parent.List2 as ICodeDescriptionPairList;
			if (codeDescPairList != null)
			{
				ListValidation.ErrorIfInvalidCode(Parent.Property2Info, codeDescPairList);
			}
			else
			{
				BusinessObjectCollection collection = Parent.List2 as BusinessObjectCollection;
				if (collection != null)
				{
					ListValidation.ErrorIfInvalidCode(Parent.Property2Info, collection);
				}
			}

			if (Parent.Property2Validation != null)
			{
				Parent.Property2Validation(Parent.Property2Info);
			}
		}

		#endregion

		public override void ValidateAll()
		{
			ValidateProperty1();
			ValidateProperty2();
		}

		public override Type AutoValidationType
		{
			get { return GetType(); }
		}

		protected readonly DependentListFilter Parent;
	}

	#endregion
}
