using System;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business.Internal
{
	public class FilterStrip : NonPersistentBusinessObject, IXmlSerializable, ILayoutDetailTreeNode
	{
		#region Schema

		public abstract class Schema
		{
			public const string FilterDescription = "FilterDescription";
			public const string FilterDescriptionList = "FilterDescriptionList";
		}

		#endregion

		#region Construction

		[EditorBrowsable(EditorBrowsableState.Never)] // XmlSerializer will blow up without this
		FilterStrip()
		{
		}

		public FilterStrip(ModuleFilterCollection moduleFilters)
		{
			fModuleFilters = moduleFilters;
		}

		#endregion

		#region ResetFilter

		public void ResetFilter()
		{
			FilterDescription = ZString.Empty;
		}

		#endregion

		#region FilterDescription

		public static string SelectFilterDescriptionText
		{
			get { return Res.GetString("FilterStrip|EmptyFilterText", "<select something to filter by>"); }
		}

		public bool IsFilterDescriptionEmpty
		{
			get { return fFilterDescription.IsEmpty; }
		}

		[BusinessObjectTestExclude] // when empty, getter returns "<select something to filter by>"
		public ZString FilterDescription
		{
			get
			{
				if (IsFilterDescriptionEmpty)
				{
					return SelectFilterDescriptionText;
				}

				return fFilterDescription;
			}
			set
			{
				if (fFilterDescription != value)
				{
					if (shouldSetDescriptionWithoutTriggeringChangesToModuleFilter)
					{
						fFilterDescription = value;
					}
					else
					{
						if (ModuleFilters != null)
						{
							var currentModuleFilter = CurrentModuleFilter;
							if (currentModuleFilter != null)
							{
								currentModuleFilter.ModuleFilterChanged -= OnNewActiveFilterModuleChanged;
							}

							if (!currentModuleFilterDescription.IsEmpty)
							{
								ModuleFilters.DisableModuleFilter(currentModuleFilterDescription);
							}
						}

						fFilterDescription = value;
						currentModuleFilterDescription = ZString.Empty;

						if (!IsFilterDescriptionEmpty)
						{
							if (ModuleFilters != null)
							{
								var newActiveFilter = ModuleFilters.GetVisibleModuleFilterAndDuplicateAndDeactivateIfActive(value, IncludeDuplicatesWhenGettingModuleFilter);

								if (newActiveFilter != null)
								{
									newActiveFilter.ModuleFilterChanged += OnNewActiveFilterModuleChanged;
									currentModuleFilterDescription = newActiveFilter.Description;
									CopyPropertiesToModuleFilter();
									CopyGroupProperties();
									newActiveFilter.IsActive = true;
								} // else user typed in crap
							}
						}
					}

					FilterDescriptionInfo.RefreshBinding();

					if (ModuleFilters != null)
					{
						Validation.ValidateFilterDescription();
					}
				}
			}
		}

		public IDisposable SuspendTriggeringChangeOfModuleFilterOnSettingDescription()
		{
			shouldSetDescriptionWithoutTriggeringChangesToModuleFilter = true;
			return new DisposableAction(() => shouldSetDescriptionWithoutTriggeringChangesToModuleFilter = false);
		}

		bool shouldSetDescriptionWithoutTriggeringChangesToModuleFilter;

		public bool IncludeDuplicatesWhenGettingModuleFilter { get; set; }

		public event EventHandler ModuleFilterChanged;

		void CopyPropertiesToModuleFilter()
		{
			var currentModuleFilter = CurrentModuleFilter;
			currentModuleFilter.GroupName = GroupName;

			if (!currentModuleFilter.IsOrCategoryReadOnly)
			{
				currentModuleFilter.OrCategory = OrCategory;
			}
			else
			{
				OrCategory = currentModuleFilter.OrCategory;
			}

			if (!currentModuleFilter.IsGroupOrCategoryReadOnly)
			{
				currentModuleFilter.GroupOrCategory = GroupOrCategory;
			}
			else
			{
				GroupOrCategory = currentModuleFilter.GroupOrCategory;
			}
		}

		void CopyGroupProperties()
		{
			var parent = ParentCollections.FirstOrDefault() as FilterStripCollection;
			if (parent == null)
			{
				return;
			}

			var sameGroupFilters = parent.Cast<FilterStrip>().Where(f => !object.ReferenceEquals(f, this) && string.Equals(f.GroupName, GroupName, StringComparison.Ordinal));

			var firstInCategory = sameGroupFilters.FirstOrDefault();
			var currentModuleFilter = CurrentModuleFilter;
			if (currentModuleFilter != null)
			{
				var copiedFilter = sameGroupFilters.FirstOrDefault(f => string.Equals(f.CurrentModuleFilter?.OriginalCode, currentModuleFilter.OriginalCode, StringComparison.OrdinalIgnoreCase));

				if (copiedFilter != null && !currentModuleFilter.IsOrCategoryReadOnly)
				{
					OrCategory = copiedFilter.OrCategory;
				}

				if (firstInCategory != null && !currentModuleFilter.IsGroupOrCategoryReadOnly)
				{
					GroupOrCategory = firstInCategory.GroupOrCategory;
				}
			}
			else
			{
				if (firstInCategory != null)
				{
					GroupOrCategory = firstInCategory.GroupOrCategory;
				}
			}
		}

		void OnNewActiveFilterModuleChanged(object sender, EventArgs e)
		{
			if (ModuleFilterChanged != null)
			{
				ModuleFilterChanged(this, EventArgs.Empty);
			}
		}

		public ZString CurrentModuleFilterDescription
		{
			get { return currentModuleFilterDescription; }
		}

		public ZPropertyInfoString FilterDescriptionInfo
		{
			get { return (ZPropertyInfoString)GetZPropertyInfo(Schema.FilterDescription); }
		}

		public CodeDescriptionPairList FilterDescriptionList
		{
			get { return ModuleFilters.Filter_List; }
		}

		protected bool FilterDescription_ReadOnly
		{
			get { return (CurrentModuleFilter != null && CurrentModuleFilter.Visibility == FilterVisibility.AlwaysVisible); }
		}

		public ZString FilterDescriptionLocalized
		{
			get
			{
				var currentModuleFilter = CurrentModuleFilter;
				if (currentModuleFilter != null && currentModuleFilter.HasMultilingualDescription)
				{
					return currentModuleFilter.LocalizedDescription;
				}
				else
				{
					return FilterDescription;
				}
			}
			set
			{
				var match = FilterDescriptionLocalizedList.ToArray().FirstOrDefault(pair =>
						(pair.Code.Equals(value, StringComparison.OrdinalIgnoreCase) || ((CodeDescriptionPair)pair).MultilingualCode.ToString().Equals(value, StringComparison.OrdinalIgnoreCase))
								&& !(pair is CategoryCodeDescriptionPair));
				FilterDescription = match != null ? (ZString)match.Description : value;
			}
		}

		public ZPropertyInfo FilterDescriptionLocalizedInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(FilterDescriptionLocalized), o => FilterDescriptionInfo); }
		}

		protected bool FilterDescriptionLocalized_ReadOnly
		{
			get { return FilterDescription_ReadOnly; }
		}

		public CodeDescriptionPairList FilterDescriptionLocalizedList
		{
			get
			{
				var codeDescriptionPairList = new CodeDescriptionPairList();

				if (ModuleFilters != null)
				{
					foreach (ICodeDescription codeDescription in FilterDescriptionList)
					{
						if (codeDescription is CategoryCodeDescriptionPair)
						{
							codeDescriptionPairList.Add(codeDescription);
						}
						else
						{
							var filter = ModuleFilters[codeDescription.Description];
							var localizedDescription = filter != null ? filter.LocalizedDescription : (codeDescription is IMultilingualDescription ? ((IMultilingualDescription)codeDescription).MultilingualDescription : (NoResString)codeDescription.Description);
							codeDescriptionPairList.AddPair(localizedDescription, codeDescription.Description);
						}
					}
				}

				return codeDescriptionPairList;
			}
		}

		ZString fFilterDescription;
		ZString currentModuleFilterDescription;

		#endregion

		#region OrCategory

		public event EventHandler OrCategoryChanged;

		public FilterOrCategory OrCategory
		{
			get { return fOrCategory; }
			set
			{
				if (fOrCategory != value)
				{
					if (CurrentModuleFilter != null)
					{
						CurrentModuleFilter.OrCategory = value;
					}

					fOrCategory = value;
					OnOrCategoryChanged();
				}
			}
		}

		void OnOrCategoryChanged()
		{
			if (OrCategoryChanged != null)
			{
				OrCategoryChanged(this, EventArgs.Empty);
			}
		}

		FilterOrCategory fOrCategory = FilterOrCategory.None;

		#region Group Name

		public ZString GroupName
		{
			get { return groupName; }
			set
			{
				if (groupName != value)
				{
					if (CurrentModuleFilter != null)
					{
						CurrentModuleFilter.GroupName = value;
					}
					groupName = value;
					CopyGroupProperties();
				}
				else if (groupName == value && CurrentModuleFilter != null)
				{
					CurrentModuleFilter.GroupName = value;
				}
			}
		}

		ZString groupName;

		#endregion

		#region GroupOrCategory

		public FilterOrCategory GroupOrCategory
		{
			get { return groupOrCategory; }
			set
			{
				if (groupOrCategory != value)
				{
					if (CurrentModuleFilter != null)
					{
						CurrentModuleFilter.GroupOrCategory = value;
					}

					groupOrCategory = value;
				}
			}
		}

		FilterOrCategory groupOrCategory = FilterOrCategory.None;

		#endregion

		#region Additional Colour Name

		public ZString AdditionalColourName
		{
			get { return additionalColourName; }
			set { additionalColourName = value; }
		}

		ZString additionalColourName;

		#endregion

		#region Additional Group Colour Name

		public ZString AdditionalGroupColourName
		{
			get { return additionalGroupColourName; }
			set { additionalGroupColourName = value; }
		}

		ZString additionalGroupColourName;

		#endregion

		#region Filter Lock Status

		public bool FilterPropertyLockStatus { get; set; }

		#endregion

		#endregion

		#region CurrentModuleFilter, ModuleFilters

		public ModuleFilter CurrentModuleFilter
		{
			get { return ModuleFilters != null && !CurrentModuleFilterDescription.IsEmpty ? ModuleFilters[CurrentModuleFilterDescription] : null; }
		}

		public ModuleFilterCollection ModuleFilters
		{
			get { return fModuleFilters; }
		}

		readonly ModuleFilterCollection fModuleFilters;

		#endregion

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			FilterDescription = ZString.Empty;
		}

		#endregion

		#region Is <type> Filter

		public bool IsTextFilter
		{
			get { return CurrentModuleFilter is ModuleTextFilter; }
		}

		public bool IsTextRangeFilter
		{
			get { return CurrentModuleFilter is ModuleTextRangeFilter; }
		}

		public bool IsTextFilterWithList
		{
			get
			{
				var textFilter = CurrentModuleFilter as ModuleTextFilter;
				return (textFilter != null && textFilter.List != null);
			}
		}

		public bool IsNkFilter
		{
			get { return CurrentModuleFilter is ModuleNkFilter; }
		}

		public bool IsTextAndNkFilter
		{
			get { return CurrentModuleFilter is ModuleTextAndNkFilter; }
		}

		public bool IsPeriodFilter
		{
			get { return CurrentModuleFilter is ModulePeriodFilter; }
		}

		public bool IsNumberFilter
		{
			get { return CurrentModuleFilter is ModuleNumberFilter; }
		}

		public bool IsDateFilter
		{
			get { return CurrentModuleFilter is ModuleDateFilter; }
		}

		public bool IsSingleDateFilter
		{
			get { return CurrentModuleFilter is ModuleSingleDateFilter; }
		}

		public bool IsGuidFilter
		{
			get { return CurrentModuleFilter is ModuleGuidFilter; }
		}

		public bool IsGuidsFilter
		{
			get { return CurrentModuleFilter is ModuleGuidsFilter; }
		}

		public bool IsLocationFilter
		{
			get { return CurrentModuleFilter is ModuleLocationFilter; }
		}

		public bool IsFlagsFilter
		{
			get { return CurrentModuleFilter is ModuleFlagsFilter; }
		}

		public bool IsNumberRangeFilter
		{
			get { return CurrentModuleFilter is ModuleNumberRangeFilter; }
		}

		public bool IsSqlFilter
		{
			get { return CurrentModuleFilter is ModuleSQLFilter; }
		}

		public bool IsTimeFilter
		{
			get { return CurrentModuleFilter is ModuleTimeFilter; }
		}

		public bool IsUserDefinedFilter
		{
			get { return CurrentModuleFilter is ModuleUserDefinedFilter; }
		}

		#endregion

		#region Validation

		public FilterStripValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual FilterStripValidation GetNewValidation()
		{
			return new FilterStripValidation(this);
		}

		#endregion

		#region IXmlSerializable Members

		XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			if (reader.Name != "ModuleFilter")
			{
				reader.ReadToFollowing("ModuleFilter");
			}

			if (reader.Name == "ModuleFilter")
			{
				if (CurrentModuleFilter != null)
				{
					reader.ReadStartElement();
					((IXmlSerializable)CurrentModuleFilter).ReadXml(reader);
					reader.ReadEndElement();
				}
				else
				{
					// module filter doesn't exist anymore (eg. Freight team have changed their filters)
					reader.Skip();
				}
			}
		}

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			if (CurrentModuleFilter != null)
			{
				writer.WriteStartElement("ModuleFilter");
				((IXmlSerializable)CurrentModuleFilter).WriteXml(writer);
				writer.WriteEndElement();
			}
		}

		#endregion

		#region ILayoutDetailsTreeViewable Members

		string ILayoutDetailTreeNode.UniqueID
		{
			get { return FilterDescriptionLocalized; }
		}

		string ILayoutDetailTreeNode.ParentUniqueID
		{
			get { return CurrentModuleFilter != null ? CurrentModuleFilter.Category.Description : ZString.Empty; }
		}

		#endregion
	}
}
