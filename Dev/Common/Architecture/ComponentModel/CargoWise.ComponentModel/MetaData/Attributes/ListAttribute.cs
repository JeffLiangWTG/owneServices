using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.ComponentModel.NotificationExtensions;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Apply this attribute to a property whose value should be looked up in a value/display
	/// pair list.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class ListAttribute : MetaDataBaseAttribute, INotificationProvidingAttribute
	{
		/// <summary>
		/// Specify the list data source.
		/// </summary>
		/// <param name="listDataSourceMember">The entity member that returns the list.</param>
		public ListAttribute(string listDataSourceMember)
		{
			Argument.NotNull(listDataSourceMember, nameof(listDataSourceMember));
			this.listDataSourceMember = listDataSourceMember;
		}

		/// <summary>
		/// Specify the list data source and display/value members for the list elements.
		/// </summary>
		/// <param name="listDataSourceMember">The entity member that returns the list.</param>
		/// <param name="displayMember">The list element member that returns the display string.</param>
		/// <param name="valueMember">The list element member that returns the value.</param>
		public ListAttribute(string listDataSourceMember, string valueMember, string displayMember)
			: this(listDataSourceMember)
		{
			Argument.NotNull(listDataSourceMember, nameof(listDataSourceMember));
			this.valueMember = valueMember;
			this.displayMember = displayMember;
		}

		/// <summary>
		/// Get the member property of the entity that returns the data source for the list.
		/// </summary>
		public string ListDataSourceMember
		{
			get { return listDataSourceMember; }
		}
		readonly string listDataSourceMember;

		/// <summary>
		/// Get the member on each element of the list that returns the value.
		/// </summary>
		public string ValueMember
		{
			get { return valueMember; }
		}
		readonly string valueMember;

		/// <summary>
		/// Get the member on each element of the list that returns the display string.
		/// </summary>
		public string DisplayMember
		{
			get { return displayMember; }
		}
		readonly string displayMember;

		/// <summary>
		/// Get or set whether to validate that only a value in the list may be assigned to the property.
		/// </summary>
		public bool AllowOnlyTheseValues
		{
			get { return allowOnlyTheseValues; }
			set { allowOnlyTheseValues = value; }
		}
		bool allowOnlyTheseValues;

		public override bool ProvidesMetaDataValue(string metaDataTypeId)
		{
			return
				(ValueMember != null && metaDataTypeId == MetaDataTypes.ListValueMember) ||
				(DisplayMember != null && metaDataTypeId == MetaDataTypes.ListDisplayMember);
		}

		public override bool ProvidesMetaDataMember(string metaDataTypeId)
		{
			return metaDataTypeId == MetaDataTypes.ListDataSource;
		}

		public override object GetMetaDataValue(string metaDataTypeId)
		{
			if (metaDataTypeId == MetaDataTypes.ListValueMember)
			{
				return ValueMember;
			}
			else if (metaDataTypeId == MetaDataTypes.ListDisplayMember)
			{
				return DisplayMember;
			}
			else
			{
				return null;
			}
		}

		public override string GetMetaDataMember(string metaDataTypeId)
		{
			if (metaDataTypeId == MetaDataTypes.ListDataSource)
			{
				return ListDataSourceMember;
			}
			else
			{
				return null;
			}
		}

		public override IDictionary<string, string> ReferencedMembers
		{
			get
			{
				var result = new Dictionary<string, string>();
				result[ListDataSourceMember] = MetaDataTypes.ListDataSource;
				return result;
			}
		}

		protected override Attribute GetAttributeOnOuterProperty(WrappingPropertyDescriptor wrappingProperty)
		{
			return new ListAttribute(
				wrappingProperty.Outer.Name + "+" + ListDataSourceMember,
				(ValueMember == null) ? null : wrappingProperty.Outer.Name + "+" + ValueMember,
				(DisplayMember == null) ? null : wrappingProperty.Outer.Name + "+" + DisplayMember);
		}

		#region INotificationProvidingAttribute Members

		public IEnumerable<INotification> Validate(object component, PropertyDescriptor propertyBeingValidated)
		{
			var notifications = new NotificationCollection();
			if (AllowOnlyTheseValues)
			{
				var list = (IList)MetaData.GetMetaData(component, propertyBeingValidated, MetaDataTypes.ListDataSource);
				if (list != null)
				{
					var valueProperty = GetValueProperty(propertyBeingValidated);
					notifications.ErrorIfNotInList(component, propertyBeingValidated, valueProperty, list);
				}
			}
			return notifications;
		}

		bool INotificationProvidingAttribute.ProvidesNotifications(PropertyDescriptor propertyBeingValidated)
		{
			return ProvidesNotifications();
		}

		public bool ProvidesNotifications()
		{
			return AllowOnlyTheseValues;
		}

		PropertyDescriptor GetValueProperty(PropertyDescriptor propertyBeingValidated)
		{
			Argument.NotNull(propertyBeingValidated, nameof(propertyBeingValidated));
			if (lastValueProperty == null || propertyBeingValidated != lastPropertyBeingValidatedForValueProperty)
			{
				var locator = MetaDataValueMemberLocator.GetInstance(propertyBeingValidated, MetaDataTypes.ListDataSource);
				if (locator.ListElementType != null)
				{
					lastValueProperty = (ValueMember == null) ? null : TypeDescriptor.GetProperties(locator.ListElementType)[ValueMember];
					lastPropertyBeingValidatedForValueProperty = propertyBeingValidated;
				}
			}
			return lastValueProperty;
		}
		PropertyDescriptor lastValueProperty;
		PropertyDescriptor lastPropertyBeingValidatedForValueProperty;

		#endregion
	}
}
