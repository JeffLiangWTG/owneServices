using System;
using System.Collections.Generic;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public delegate ZQuery GetFlagsQuery(ZBool value);

	#region ModuleFlagsFilterDefaults

	public class ModuleFlagsFilterDefaults
	{
		public ModuleFlagsFilterDefaults(ModuleFlagsFilter parent)
		{
			Parent = parent;
			Defaults = new Dictionary<string, ZBool>();
		}

		public ZBool this[string flagName]
		{
			get
			{
				var valueFromParent = Parent[flagName]; // ensures flag name is valid
				return Defaults.ContainsKey(flagName) ? Defaults[flagName] : ZBool.False;
			}
			set
			{
				Parent[flagName] = value; // in addition to setting the value, ensures flag name is valid
				Defaults[flagName] = value;
			}
		}

		readonly Dictionary<string, ZBool> Defaults;
		readonly ModuleFlagsFilter Parent;
	}

	#endregion

	public class ModuleFlagsFilter : ModuleFilter
	{
		#region Construction

		public ModuleFlagsFilter(ZString description, string flagName, SchemaBoolColumn flagFilterColumn, ModuleFilterSubGroup moduleFilterSubGroup)
			: this(description, new[] { flagName }, new[] { flagFilterColumn })
		{
			this.moduleFilterSubGroup = moduleFilterSubGroup;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ModuleFlagsFilter(ZString description, string[] flagNames, SchemaBoolColumn[] flagFilterColumns)
			: base(description)
		{
			EnsureConstructorParamsAreValid("flagNames", flagNames, "flagFilterColumns", flagFilterColumns);
			FlagNames = flagNames;
			FlagFilterColumns = flagFilterColumns;
			SetDefaultJoinCondition();
		}

		public ModuleFlagsFilter(ZString description, string[] flagNames, GetFlagsQuery[] queryDelegates)
			: this(description, flagNames, queryDelegates, JoinCondition.And)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ModuleFlagsFilter(ZString description, string[] flagNames, GetFlagsQuery[] queryDelegates, JoinCondition joinConditionForQueryDelegates)
			: base(description)
		{
			EnsureConstructorParamsAreValid("flagNames", flagNames, "queryDelegates", queryDelegates);
			FlagNames = flagNames;
			QueryDelegates = queryDelegates;
			this.joinConditionForQueryDelegates = joinConditionForQueryDelegates;
			SetDefaultJoinCondition();
		}

		protected virtual void EnsureConstructorParamsAreValid(string paramater1Name, string[] flagNames, string paramater2Name, object[] parameter2Elements)
		{
			EnsureFlagsAreValid(paramater1Name, flagNames);
			EnsureFlagsAreValid(paramater2Name, parameter2Elements);

			if (flagNames.Length != parameter2Elements.Length && ShouldCheckFlagAmountEqualsDelegateAmount)
			{
				throw new ArgumentException(string.Format(
					"{0} - {1} and {2} do not contain the same number of elements.", GetType().Name, paramater1Name, paramater2Name));
			}
		}

		protected virtual bool ShouldCheckMaximumFlags
		{
			get { return true; }
		}

		protected virtual bool ShouldCheckFlagAmountEqualsDelegateAmount
		{
			get { return true; }
		}

		void EnsureFlagsAreValid(string paramaterName, params object[] elements)
		{
			var msgPrefix = GetType().Name + " " + paramaterName + " ";

			if (elements == null)
			{
				throw new ArgumentNullException(msgPrefix + "cannot be null.");
			}
			if (elements.Length == 0)
			{
				throw new ArgumentException(msgPrefix + "contains 0 elements.");
			}
			if (elements.Length > MaximumFlagsCount && ShouldCheckMaximumFlags)
			{
				throw new ArgumentOutOfRangeException(msgPrefix + "contains " + elements.Length + " elements - the maximum supported is " + MaximumFlagsCount + ".");
			}
			foreach (var o in elements)
			{
				if (o == null)
				{
					throw new ArgumentNullException(msgPrefix + "contains a null element.");
				}

				var s = o as string;
				if (s != null && s.Trim().Length == 0)
				{
					throw new ArgumentNullException(msgPrefix + "contains an empty element.");
				}
			}
		}

#if DEBUG
		public
#endif
		readonly SchemaBoolColumn[] FlagFilterColumns;
		public readonly string[] FlagNames;
		readonly GetFlagsQuery[] QueryDelegates;
		readonly JoinCondition joinConditionForQueryDelegates;
		readonly ModuleFilterSubGroup moduleFilterSubGroup;

		public const int MaximumFlagsCount = 10;

		#endregion

		#region GetNewCommonModuleFilter, CopyPropertiesToFilter

		protected internal override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			throw new NotSupportedException("The Flags module filter does not support common filters.");
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			var filter = (ModuleFlagsFilter)filterToCopyFrom;
			Property0 = filter.Property0;
			Property1 = filter.Property1;
			Property2 = filter.Property2;
			Property3 = filter.Property3;
			Property4 = filter.Property4;
			Property5 = filter.Property5;
			Property6 = filter.Property6;
			Property7 = filter.Property7;
			Property8 = filter.Property8;
			Property9 = filter.Property9;
		}

		#endregion

		#region IsExpensiveQuery

		public override bool IsExpensiveQuery
		{
			get { return false; }
		}

		#endregion

		#region Default Category

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.StatusAndFlags; }
		}

		#endregion

		#region Clear / IsEmpty / Default

		protected override void ClearCore()
		{
			for (var i = 0; i < FlagNames.Length; i++)
			{
				SetPropertyValue(i, DefaultProperties[FlagNames[i]]);
			}
		}

		protected override bool IsEmptyCore => false;

		public ModuleFlagsFilterDefaults DefaultProperties
		{
			get { return defaultProperties ?? (defaultProperties = new ModuleFlagsFilterDefaults(this)); }
		}

		ModuleFlagsFilterDefaults defaultProperties;

		#endregion

		#region IsCommon

		protected override bool SupportsCommon
		{
			get { return false; }
		}

		#endregion

		#region Indexer - facilitates setting properties programatically

		public new ZBool this[string flagName]
		{
			get
			{
				for (var i = 0; i < FlagNames.Length; i++)
				{
					if (flagName == FlagNames[i])
					{
						return GetPropertyValue(i);
					}
				}
				throw new IndexOutOfRangeException("The flag '" + flagName + "' does not exist.");
			}
			set
			{
				var flagFound = false;

				for (var i = 0; i < FlagNames.Length; i++)
				{
					if (flagName == FlagNames[i])
					{
						SetPropertyValue(i, value);
						flagFound = true;
					}
				}
				if (!flagFound)
				{
					throw new IndexOutOfRangeException("The flag '" + flagName + "' does not exist.");
				}
			}
		}

		#endregion

		#region Properties Mutually Exclusive

		public bool ArePropertiesMutuallyExclusive
		{
			get;
			set;
		}

		#endregion

		#region PropertyValidation

		public Validation PropertyValidation
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return propertyValidation; }
			[System.Diagnostics.DebuggerStepThrough]
			set { propertyValidation = value; }
		}
		Validation propertyValidation;

		#endregion

		#region Properties 0 through 9

		#region Property0

		void SetTheOtherPropertiesToFalseIfMutuallyExclusive(int propertyIndex)
		{
			var value = GetPropertyValue(propertyIndex);

			if (value && ArePropertiesMutuallyExclusive)
			{
				for (var index = 0; index < 10; index++)
				{
					if (index != propertyIndex)
					{
						SetPropertyValue(index, false);
					}
				}
			}
		}

		public ZBool Property0
		{
			get { return fProperty0; }
			set
			{
				if (fProperty0 != value)
				{
					fProperty0 = value;

					SetTheOtherPropertiesToFalseIfMutuallyExclusive(0);

					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty0();
					}
					Property0Info.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo Property0Info
		{
			get { return GetZPropertyInfo(nameof(Property0)); }
		}

		ZBool fProperty0;

		#endregion

		#region Property1

		public ZBool Property1
		{
			get { return fProperty1; }
			set
			{
				if (fProperty1 != value)
				{
					fProperty1 = value;

					SetTheOtherPropertiesToFalseIfMutuallyExclusive(1);

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

		ZBool fProperty1;

		#endregion

		#region Property2

		public ZBool Property2
		{
			get { return fProperty2; }
			set
			{
				if (fProperty2 != value)
				{
					fProperty2 = value;

					SetTheOtherPropertiesToFalseIfMutuallyExclusive(2);

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

		ZBool fProperty2;

		#endregion

		#region Property3

		public ZBool Property3
		{
			get { return fProperty3; }
			set
			{
				if (fProperty3 != value)
				{
					fProperty3 = value;

					SetTheOtherPropertiesToFalseIfMutuallyExclusive(3);

					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty3();
					}
					Property3Info.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo Property3Info
		{
			get { return GetZPropertyInfo(nameof(Property3)); }
		}

		ZBool fProperty3;

		#endregion

		#region Property4

		public ZBool Property4
		{
			get { return fProperty4; }
			set
			{
				if (fProperty4 != value)
				{
					fProperty4 = value;

					SetTheOtherPropertiesToFalseIfMutuallyExclusive(4);

					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty4();
					}
					Property4Info.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo Property4Info
		{
			get { return GetZPropertyInfo(nameof(Property4)); }
		}

		ZBool fProperty4;

		#endregion

		#region Property5

		public ZBool Property5
		{
			get { return fProperty5; }
			set
			{
				if (fProperty5 != value)
				{
					fProperty5 = value;

					SetTheOtherPropertiesToFalseIfMutuallyExclusive(5);

					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty5();
					}
					Property5Info.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo Property5Info
		{
			get { return GetZPropertyInfo(nameof(Property5)); }
		}

		ZBool fProperty5;

		#endregion

		#region Property6

		public ZBool Property6
		{
			get { return fProperty6; }
			set
			{
				if (fProperty6 != value)
				{
					fProperty6 = value;

					SetTheOtherPropertiesToFalseIfMutuallyExclusive(6);

					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty6();
					}
					Property6Info.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo Property6Info
		{
			get { return GetZPropertyInfo(nameof(Property6)); }
		}

		ZBool fProperty6;

		#endregion

		#region Property7

		public ZBool Property7
		{
			get { return fProperty7; }
			set
			{
				if (fProperty7 != value)
				{
					fProperty7 = value;

					SetTheOtherPropertiesToFalseIfMutuallyExclusive(7);

					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty7();
					}
					Property7Info.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo Property7Info
		{
			get { return GetZPropertyInfo(nameof(Property7)); }
		}

		ZBool fProperty7;

		#endregion

		#region Property8

		public ZBool Property8
		{
			get { return fProperty8; }
			set
			{
				if (fProperty8 != value)
				{
					fProperty8 = value;

					SetTheOtherPropertiesToFalseIfMutuallyExclusive(8);

					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty8();
					}
					Property8Info.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo Property8Info
		{
			get { return GetZPropertyInfo(nameof(Property8)); }
		}

		ZBool fProperty8;

		#endregion

		#region Property9

		public ZBool Property9
		{
			get { return fProperty9; }
			set
			{
				if (fProperty9 != value)
				{
					fProperty9 = value;

					SetTheOtherPropertiesToFalseIfMutuallyExclusive(9);

					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty9();
					}
					Property9Info.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo Property9Info
		{
			get { return GetZPropertyInfo(nameof(Property9)); }
		}

		ZBool fProperty9;

		#endregion

#if DEBUG
		protected
#endif
 ZBool GetPropertyValue(int propertyNumber)
		{
			var info = GetType().GetProperty("Property" + propertyNumber);
			return (ZBool)info.GetValue(this, null);
		}

		void SetPropertyValue(int propertyNumber, ZBool value)
		{
			var info = GetType().GetProperty("Property" + propertyNumber);
			info.SetValue(this, value, null);
		}

		#endregion

		#region Validation

		public new ModuleFlagsFilterValidation Validation
		{
			get { return (ModuleFlagsFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ModuleFlagsFilterValidation(this);
		}

		#endregion

		#region Query

		protected override object[] QueryDelegateParameters
		{
			get { return null; } // not used, we handle the delegate invoking ourselves
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			var result = new ZQuery();
			var joinCondition = (ShowAddOrRadioBox && OrJoinCondition) ? JoinCondition.Or : JoinCondition.And;

			for (var i = 0; i < FlagFilterColumns.Length; i++)
			{
				var value = GetPropertyValue(i);
				if (value || moduleFilterSubGroup != null) // don't filter on false flags unless we have a sub group
				{
					result.AddToFilter(joinCondition, FlagFilterColumns[i], value);
				}
			}

			return result;
		}

		protected override bool HasQueryDelegate
		{
			get { return (QueryDelegates != null); }
		}

		protected override ZQuery RunQueryDelegate()
		{
			// users can filter on false flags here if they wish - this is intentional for the rare case that they need to do so
			var result = new ZQuery();

			for (var i = 0; i < QueryDelegates.Length; i++)
			{
				var queryDelegate = QueryDelegates[i];
				var value = GetPropertyValue(i);
				var query = queryDelegate.Invoke(value);

				result.AddToFilter(query, joinConditionForQueryDelegates);
			}

			return result;
		}

		#endregion

		#region JoinCondition

		public bool ShowAddOrRadioBox { get; set; }

		public ZBool AndJoinCondition
		{
			get { return andJoinCondition; }
			set
			{
				if (AndJoinCondition != value)
				{
					InvalidateCachedQuery();
				}

				andJoinCondition = value;
				AndJoinConditionInfo.RefreshBinding();
			}
		}
		ZBool andJoinCondition;

		public ZPropertyInfo AndJoinConditionInfo
		{
			get { return GetZPropertyInfo(nameof(AndJoinCondition)); }
		}

		public ZBool OrJoinCondition
		{
			get { return orJoinCondition; }
			set
			{
				if (OrJoinCondition != value)
				{
					InvalidateCachedQuery();
				}

				orJoinCondition = value;
				OrJoinConditionInfo.RefreshBinding();
			}
		}
		ZBool orJoinCondition;

		public ZPropertyInfo OrJoinConditionInfo
		{
			get { return GetZPropertyInfo(nameof(OrJoinCondition)); }
		}

		void SetDefaultJoinCondition()
		{
			AndJoinCondition = true;
			OrJoinCondition = false;
		}

		#endregion

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			Property0 = ZBool.False;
		}

#endif
		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			for (var i = 0; i < FlagNames.Length; i++)
			{
				writer.WriteElementString("Property" + i, GetPropertyValue(i).ToString());
			}
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			for (var i = 0; i < FlagNames.Length; i++)
			{
				if (reader.Name == "Property" + i)
				{
					var value = new ZBool(reader.ReadElementString("Property" + i));
					SetPropertyValue(i, value);
				}
			}
		}

		#endregion
	}

	public class ModuleUnionOrOrFilter : ModuleFlagsFilter
	{
		public ModuleUnionOrOrFilter(ZString description, string flagName, SchemaBoolColumn flagFilterColumn, ModuleFilterSubGroup moduleFilterSubGroup)
			: base(description, flagName, flagFilterColumn, moduleFilterSubGroup)
		{
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			return new ZQuery();
		}

		protected override void EnsureConstructorParamsAreValid(string paramater1Name, string[] flagNames, string paramater2Name, object[] parameter2Elements)
		{
		}

		#region Default Category

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.Other; }
		}

		#endregion
	}

	#region Module Recompile Filter
	public class ModuleRecompileFilter : ModuleFlagsFilter
	{
		public ModuleRecompileFilter(ZString description, string flagName, SchemaBoolColumn flagFilterColumn, ModuleFilterSubGroup moduleFilterSubGroup)
			: base(description, flagName, flagFilterColumn, moduleFilterSubGroup)
		{
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			return new ZQuery();
		}

		protected override void EnsureConstructorParamsAreValid(string paramater1Name, string[] flagNames, string paramater2Name, object[] parameter2Elements)
		{
		}

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.Execution; }
		}
	}
	#endregion

	#region Module Cardinality FIlter
	public class ModuleCardinalityFilter : ModuleFlagsFilter
	{
		public ModuleCardinalityFilter(ZString description, string flagName, SchemaBoolColumn flagFilterColumn, ModuleFilterSubGroup moduleFilterSubGroup)
			: base(description, flagName, flagFilterColumn, moduleFilterSubGroup)
		{
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			return new ZQuery();
		}

		protected override void EnsureConstructorParamsAreValid(string paramater1Name, string[] flagNames, string paramater2Name, object[] parameter2Elements)
		{
		}

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.Execution; }
		}
	}
	#endregion

	#region class Validation

	public class ModuleFlagsFilterValidation : ModuleFilterValidation
	{
		public ModuleFlagsFilterValidation(ModuleFlagsFilter parent)
			: base(parent)
		{
		}

		protected new ModuleFlagsFilter ParentFilter
		{
			get { return (ModuleFlagsFilter)base.ParentFilter; }
		}

		#region Validate Properties

		#region ValidateProperty0

		public void ValidateProperty0()
		{
			((IValidationInternals)this).Validate(ParentFilter.Property0Info, CheckProperty0);
		}

		protected virtual void CheckProperty0()
		{
			ParentFilter.PropertyValidation?.Invoke(ParentFilter.Property0Info);
		}

		#endregion

		#region ValidateProperty1

		public void ValidateProperty1()
		{
			((IValidationInternals)this).Validate(ParentFilter.Property1Info, CheckProperty1);
		}

		protected virtual void CheckProperty1()
		{
			ParentFilter.PropertyValidation?.Invoke(ParentFilter.Property1Info);
		}

		#endregion

		#region ValidateProperty2

		public void ValidateProperty2()
		{
			((IValidationInternals)this).Validate(ParentFilter.Property2Info, CheckProperty2);
		}

		protected virtual void CheckProperty2()
		{
			ParentFilter.PropertyValidation?.Invoke(ParentFilter.Property2Info);
		}

		#endregion

		#region ValidateProperty3

		public void ValidateProperty3()
		{
			((IValidationInternals)this).Validate(ParentFilter.Property3Info, CheckProperty3);
		}

		protected virtual void CheckProperty3()
		{
			ParentFilter.PropertyValidation?.Invoke(ParentFilter.Property3Info);
		}

		#endregion

		#region ValidateProperty4

		public void ValidateProperty4()
		{
			((IValidationInternals)this).Validate(ParentFilter.Property4Info, CheckProperty4);
		}

		protected virtual void CheckProperty4()
		{
			ParentFilter.PropertyValidation?.Invoke(ParentFilter.Property4Info);
		}

		#endregion

		#region ValidateProperty5

		public void ValidateProperty5()
		{
			((IValidationInternals)this).Validate(ParentFilter.Property5Info, CheckProperty5);
		}

		protected virtual void CheckProperty5()
		{
			ParentFilter.PropertyValidation?.Invoke(ParentFilter.Property5Info);
		}

		#endregion

		#region ValidateProperty6

		public void ValidateProperty6()
		{
			((IValidationInternals)this).Validate(ParentFilter.Property6Info, CheckProperty6);
		}

		protected virtual void CheckProperty6()
		{
			ParentFilter.PropertyValidation?.Invoke(ParentFilter.Property6Info);
		}

		#endregion

		#region ValidateProperty7

		public void ValidateProperty7()
		{
			((IValidationInternals)this).Validate(ParentFilter.Property7Info, CheckProperty7);
		}

		protected virtual void CheckProperty7()
		{
			ParentFilter.PropertyValidation?.Invoke(ParentFilter.Property7Info);
		}

		#endregion

		#region ValidateProperty8

		public void ValidateProperty8()
		{
			((IValidationInternals)this).Validate(ParentFilter.Property8Info, CheckProperty8);
		}

		protected virtual void CheckProperty8()
		{
			ParentFilter.PropertyValidation?.Invoke(ParentFilter.Property8Info);
		}

		#endregion

		#region ValidateProperty9

		public void ValidateProperty9()
		{
			((IValidationInternals)this).Validate(ParentFilter.Property9Info, CheckProperty9);
		}

		protected virtual void CheckProperty9()
		{
			ParentFilter.PropertyValidation?.Invoke(ParentFilter.Property9Info);
		}

		#endregion

		#endregion

		#region Validate All

		public override void ValidateAll()
		{
			ValidateProperty0();
			ValidateProperty1();
			ValidateProperty2();
			ValidateProperty3();
			ValidateProperty4();
			ValidateProperty5();
			ValidateProperty6();
			ValidateProperty7();
			ValidateProperty8();
			ValidateProperty9();
		}

		public override Type AutoValidationType
		{
			get { return this.GetType(); }
		}

		#endregion
	}

	#endregion
}
