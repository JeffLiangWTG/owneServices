using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZGridCustomColumnsInitializer
	{
		public ZGridCustomColumnsInitializer(ZGrid grid, IBusinessObjectCollection collection, ResourceStringData groupName, bool isVisible = false, bool isReadonly = true, bool isCustomColumn = true)
		{
			Argument.NotNull(grid, "grid");
			Argument.NotNull(collection, "collection");

			Grid = grid;
			Collection = collection;
			this.groupName = groupName;
			this.isVisible = isVisible;
			this.IsReadonly = isReadonly;
			this.IsCustomColumn = isCustomColumn;
		}

		#region AddCustomColumns

		public void AddCustomColumns(IEnumerable<ICustomProperty> customProperties)
		{
			if (customProperties.Any())
			{
				//Assuming Grid.Columns/ColumnStyles maintains sorted - Visible in front
				var columnStylesVisibleCount = (from ZGridColumnInfo info in Grid.ColumnStyles
												where info.IsVisible
												select info).Count();
				var columnsVisibleCount = (from ZGridColumn column in Grid.Columns
										   where column.IsVisible
										   select column).Count();

				using (Grid.SuspendRefreshTableStylesAndRefreshAtDisposal())
				{
					foreach (var field in customProperties)
					{
						if (Grid.GetColumnStyle(GetClearName(field.Identifier)) == null)
						{
							var columnInfo = GetColumnInfo(field);

							Grid.ColumnStyles.Insert(columnStylesVisibleCount++, columnInfo);
							if (Grid.Columns.Count > 0)
							{
								Grid.Columns.Insert(columnInfo, columnsVisibleCount++);
								Grid.SetAvailability(!columnInfo.IsUnavailable, field.Identifier);
							}
						}
					}
				}
			}
		}

		ZTextBoxColumnStyleInfo GetColumnInfo(ICustomProperty property)
		{
			var columnInfo = GetColumnInfo(property.Info);

			columnInfo.ColumnName = GetClearName(property.Identifier);
			columnInfo.IsCustomColumn = true;
			columnInfo.Caption = GetCaption(property.Identifier, property.Info);
			columnInfo.GroupName = groupName;
			columnInfo.IsVisible = property.Info.Visible && isVisible;
			columnInfo.IsReadOnly = IsReadonly;
			columnInfo.ColumnComparer = property.HasMatchingLegacyIdentifier;

			var maxLengthMetaData = property.Info.GetMetaData(MetaDataTypes.MaxLength);
			if (maxLengthMetaData != null)
			{
				columnInfo.MaxLengthOverride = (int)maxLengthMetaData.Value;
			}

			((IOverridablePropertyDescriptor)columnInfo).PropertyDescriptor = GetPropertyDescriptor(property);

#if DEBUG
			TypeDescriptor.AddAttributes(columnInfo, new SuppressFormsLocalizedTestAttribute());
#endif

			return columnInfo;
		}

		protected virtual ZTextBoxColumnStyleInfo GetColumnInfo(DynamicBusinessObjectProperty property)
		{
			if (property.Type == typeof(ZInt) || property.Type == typeof(ZDecimal) || property.Type == typeof(ZShort) || property.Type == typeof(ZByte))
			{
				return new ZCalcEditColumnStyleInfo { Decimals = property.Type == typeof(ZDecimal) ? 2 : 0 };
			}
			if (property.Type == typeof(ZDateTime))
			{
				return new ZDateEditColumnStyleInfo { DateTimeFormat = GetDateFormat(property) };
			}
			if (property.Type == typeof(ZDateTimeOffset))
			{
				return new ZDateTimeOffsetEditColumnStyleInfo { DateTimeFormat = GetDateFormat(property) };
			}
			if (property.Type == typeof(ZBool))
			{
				return new ZCheckBoxColumnStyleInfo();
			}
			if (property.Type == typeof(ZString) && property.GetMetaData(MetaDataTypes.ListDataSource) != null)
			{
				return new ZMultiControlColumnStyleInfo();
			}

			return new ZTextBoxColumnStyleInfo();
		}

		static string GetCaption(string propertyName, DynamicBusinessObjectProperty property)
		{
			var metadata = property.GetMetaData(MetaDataTypes.Description);
			if (metadata != null)
			{
				var description = (IDescription)metadata.Value;
				return description.GetDescription(0);
			}

			return propertyName;
		}

		static string GetClearName(string propertyName)
		{
			if (propertyName.IndexOfAny(new[] { '.', '+' }) >= 0)
			{
				var sb = new StringBuilder(propertyName);
				sb.Replace('.', '_');
				sb.Replace('+', '_');
				return sb.ToString();
			}

			return propertyName;
		}

		static ZDateTimePickerFormat GetDateFormat(DynamicBusinessObjectProperty property)
		{
			var metaData = property.GetMetaData(MetaDataTypes.DateTimeFormat);

			if (metaData != null)
			{
				switch ((KDateTimeFormat)metaData.Value)
				{
					case KDateTimeFormat.Long:
						return ZDateTimePickerFormat.Long;
					case KDateTimeFormat.Short:
						return ZDateTimePickerFormat.Short;
					case KDateTimeFormat.Time:
						return ZDateTimePickerFormat.Time;
				}
			}

			return ZDateTimePickerFormat.Short;
		}

		protected virtual PropertyDescriptor GetPropertyDescriptor(ICustomProperty property)
		{
			return new ZCustomPropertyDescriptor(property.Identifier, property.Info.Type);
		}

		#endregion

		#region Fields and Properties

		protected ZGrid Grid { get; private set; }
		protected IBusinessObjectCollection Collection { get; private set; }
		readonly ResourceStringData groupName;
		readonly bool isVisible;
		protected bool IsReadonly { get; private set; }
		protected bool IsCustomColumn { get; private set; }

		#endregion
	}

	public class ZActiveGridCustomColumnsInitializer : ZGridCustomColumnsInitializer, IDisposable
	{
		public ZActiveGridCustomColumnsInitializer(ZGrid grid, IBusinessObjectCollection collection, ResourceStringData groupName, bool isVisible = false, bool isReadonly = true)
			: base(grid, collection, groupName, isVisible, isReadonly)
		{ }

		protected override ZTextBoxColumnStyleInfo GetColumnInfo(DynamicBusinessObjectProperty property)
		{
			var columnInfo = base.GetColumnInfo(property);
			columnInfo.IsSubmissive = true;
			return columnInfo;
		}

		#region HookCustomisedFieldsSetChanged

		public void HookCollection()
		{
			Collection.ListChanged += CollectionListChanged;
			CollectionListChanged(Collection, new ListChangedEventArgs(ListChangedType.Reset, -1));
		}

		public void UnhookCollection()
		{
			Collection.ListChanged -= CollectionListChanged;
			foreach (BusinessObject bizo in Collection)
			{
				UnhookBusinessObject(bizo);
			}
		}

		void CollectionListChanged(object sender, ListChangedEventArgs e)
		{
			if (e.ListChangedType == ListChangedType.ItemAdded)
			{
				if (e.NewIndex >= 0 && e.NewIndex < Collection.Count)
				{
					HookBusinessObjects(new[] { (BusinessObject)Collection[e.NewIndex] });
				}
			}
			else if (e.ListChangedType == ListChangedType.Reset)
			{
				UnHookElementsNotInCollection();
				HookElementsNowInCollection();
			}
			else if (e.ListChangedType == ListChangedType.ItemDeleted)
			{
				UnHookElementsNotInCollection();
			}
		}

		void UnHookElementsNotInCollection()
		{
			var oldHookedElements = new List<BusinessObject>(HookedElements);
			foreach (BusinessObject businessObject in Collection)
			{
				oldHookedElements.Remove(businessObject);
			}

			foreach (var businessObject in oldHookedElements)
			{
				UnhookBusinessObject(businessObject);
			}
		}

		void HookElementsNowInCollection()
		{
			HookBusinessObjects(Collection.Cast<BusinessObject>().Except(HookedElements).ToArray());
		}

		void HookBusinessObjects(IEnumerable<BusinessObject> businessObjects)
		{
			if (businessObjects.Any())
			{
				using (Grid.SuspendRefreshTableStylesAndRefreshAtDisposal())
				{
					foreach (var businessObject in businessObjects)
					{
						var propertyContainer = businessObject as ICustomPropertyContainer;
						if (propertyContainer != null)
						{
							AddCustomColumns(propertyContainer.CustomProperties);
						}

						var activeContainer = businessObject as IActiveCustomPropertyContainer;
						if (activeContainer != null)
						{
							UnSubscribePropertySetChanged(businessObject, handler => activeContainer.PropertySetChanged -= handler);
							SubscribePropertySetChanged(businessObject, handler => activeContainer.PropertySetChanged += handler);
						}
					}

					HookBusinessObjectCore(businessObjects);
				}
			}
		}

		protected virtual void HookBusinessObjectCore(IEnumerable<BusinessObject> businessObject) { }

		void UnhookBusinessObject(BusinessObject businessObject)
		{
			var activeContainer = businessObject as IActiveCustomPropertyContainer;
			if (activeContainer != null)
			{
				UnSubscribePropertySetChanged(businessObject, handler => activeContainer.PropertySetChanged -= handler);
			}

			UnhookBusinessObjectCore(businessObject);
		}

		protected virtual void UnhookBusinessObjectCore(BusinessObject businessObject) { }

		protected void SubscribePropertySetChanged(BusinessObject businessObject, Action<EventHandler> subscribeAction)
		{
			if (businessObject != null && subscribeAction != null && !eventSubscriptionList.ContainsKey(businessObject.PK))
			{
				EventHandler handler = (s, e) =>
				{
					UnhookBusinessObject(businessObject);
					HookBusinessObjects(new[] { businessObject });
				};

				var subscriptionInfo = new EventSubscriptionInfo
				{
					BusinessObject = businessObject,
					Handler = handler
				};

				subscribeAction(handler);
				eventSubscriptionList.Add(businessObject.PK, subscriptionInfo);
			}
		}

		protected void UnSubscribePropertySetChanged(BusinessObject businessObject, Action<EventHandler> unsubscribeAction)
		{
			if (businessObject != null && unsubscribeAction != null && eventSubscriptionList.ContainsKey(businessObject.PK))
			{
				var subscriptionInfo = eventSubscriptionList[businessObject.PK];

				unsubscribeAction(subscriptionInfo.Handler);
				eventSubscriptionList.Remove(businessObject.PK);
			}
		}

		class EventSubscriptionInfo
		{
			public BusinessObject BusinessObject { get; set; }
			public EventHandler Handler { get; set; }
		}

		readonly Dictionary<ZGuid, EventSubscriptionInfo> eventSubscriptionList = new Dictionary<ZGuid, EventSubscriptionInfo>();

		IEnumerable<BusinessObject> HookedElements
		{
			get { return eventSubscriptionList.Values.Select(subscriptionInfo => subscriptionInfo.BusinessObject); }
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			UnhookCollection();
		}

		#endregion
	}
}
