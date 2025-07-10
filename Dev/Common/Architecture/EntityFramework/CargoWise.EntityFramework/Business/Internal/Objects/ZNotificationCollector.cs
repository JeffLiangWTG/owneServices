using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	[DebuggerTypeProxy(typeof(IEnumerableNotification_DebuggerTypeProxy))]
	public class ZNotificationCollector : IEnumerable<INotification>
	{
		public enum PropertyDescriptionType
		{
			ColumnName,
			HumanReadableName,
			None
		}

		public ZNotificationCollector(IBusiness business, ZBool includeChildren, ZBool includeNotificationTypeInMessage)
			: this(business, includeChildren, includeNotificationTypeInMessage, PropertyDescriptionType.ColumnName)
		{
		}

		public ZNotificationCollector(IBusiness business, ZBool includeChildren, ZBool includeNotificationTypeInMessage, PropertyDescriptionType descriptionToInclude)
		{
			Business = business;
			IncludeChildren = includeChildren;
			IncludeNotificationTypeInMessage = includeNotificationTypeInMessage;
			DescriptionToInclude = descriptionToInclude;
		}

		readonly IBusiness Business;
		readonly ZBool IncludeChildren;
		readonly ZBool IncludeNotificationTypeInMessage;
		readonly PropertyDescriptionType DescriptionToInclude;

		protected virtual bool ShouldIncludeNotificationsFromObject(BusinessObject businessObject)
		{
			return true;
		}

		protected virtual bool ShouldIncludeNotificationsFromInfo(ZPropertyInfo info)
		{
			return true;
		}

		protected virtual INotification GetNotification(BusinessObject bizObj, INotification notification, string replacedMessage)
		{
			if (ShouldCapitalize(replacedMessage))
			{
				replacedMessage = UppercaseFirst(replacedMessage); //capitalises first letter
			}
			return notification.ReplaceMessage(replacedMessage);
		}

		#region GetFormattedNotifications

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Refactoring is needed in future")]
		IEnumerable<INotification> GetFormattedNotifications(IBusiness business, Dictionary<IBusiness, object> checkedBizos)
		{
			if (checkedBizos == null)
			{
				checkedBizos = new Dictionary<IBusiness, object>();
			}

			AddToParentChildChain(business);

			BusinessObject businessObject = business as BusinessObject;

			if (businessObject != null && !businessObject.IsDeleted && ShouldIncludeNotificationsFromObject(businessObject))
			{
				if (checkedBizos.ContainsKey(businessObject))
				{
					ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Cycle references found on the businessObject(Type: {0}), please check and fix.", business.GetType().Name));
					yield break;
				}
				checkedBizos.Add(businessObject, null);

				foreach (INotification next in GetFormattedRowNotifications(businessObject))
				{
					yield return next;
				}

				IList childrenToAddErrorsFrom = IncludeChildren ? business.Children : Array.Empty<object>();
				foreach (IBusiness child in childrenToAddErrorsFrom)
				{
					BusinessObject bizo = child as BusinessObject;

					if (bizo == null || !bizo.IsTopLevel)
					{
						foreach (INotification next in GetFormattedNotifications(child, checkedBizos))
						{
							yield return next;
						}
					}
				}
				var propertyInfoHash = businessObject.ZPropertyInfoHash;
				if (propertyInfoHash != null)
				{
					foreach (ZPropertyInfo info in propertyInfoHash.GetPropertyInfos(PropertyInfoTypes.NonWrapping))
					{
						if (ShouldIncludeNotificationsFromInfo(info))
						{
							foreach (INotification next in GetFormattedPropertyNotifications(info))
							{
								yield return next;
							}
						}
					}
					foreach (ZWrappedPropertyInfo wrappedInfo in propertyInfoHash.GetPropertyInfos(PropertyInfoTypes.Wrapping))
					{
						if (wrappedInfo != null &&
							ShouldIncludeNotificationsFromInfo(wrappedInfo) &&
							wrappedInfo.IsLoaded &&
							wrappedInfo.InnerInfo != null &&
							businessObject != wrappedInfo.InnerInfo.BizObj &&
							wrappedInfo.Name.IndexOf('+') < 0 // ZWrappedPropertyInfo with '+' in name is autocreated during binding to subproperties
							)
						{
							foreach (INotification next in GetFormattedPropertyNotifications(wrappedInfo))
							{
								yield return next;
							}
						}
					}
				}

				if (checkedBizos.ContainsKey(businessObject))
				{
					checkedBizos.Remove(businessObject);
				}
			}
			else if (businessObject == null)
			{
				IBusinessObjectCollection collection = business as IBusinessObjectCollection;
				if (collection != null)
				{
					bool keepProcessing = true;
					int maximumRetries = 3;
					while (keepProcessing && maximumRetries > 0)
					{
						maximumRetries--;
						int originalCount = collection.Count;
						List<INotification> tempResult = new List<INotification>();
						foreach (BusinessObject child in collection.ToArray())
						{
							tempResult.AddRange(GetFormattedNotifications(child, checkedBizos));
						}
						if (originalCount == collection.Count)
						{
							foreach (INotification next in tempResult)
							{
								yield return next;
							}
							keepProcessing = false;
						}
					}
				}
			}

			RemoveFromParentChildChain(business);
		}

		#region ParentChildChain

		protected virtual IList<IBusiness> ParentChildChain { get { return null; } }

		void AddToParentChildChain(IBusiness business)
		{
			if (ParentChildChain == null)
			{
				return;
			}

			ParentChildChain.Add(business);
		}
		void RemoveFromParentChildChain(IBusiness business)
		{
			if (ParentChildChain == null)
			{
				return;
			}

			ParentChildChain.Remove(business);
		}

		#endregion

		IEnumerable<INotification> GetFormattedPropertyNotifications(ZPropertyInfo property)
		{
			if (property == null)
			{
				throw new ArgumentNullException(nameof(property));
			}
			if (property.Notifications != null)
			{
				foreach (INotification notification in property.Notifications)
				{
					yield return GetNotification(property.BizObj, notification, FormatNotificationType(notification.Type) + GetFormattedPropertyName(property) + notification.Message);
				}
			}
		}

		IEnumerable<INotification> GetFormattedRowNotifications(BusinessObject businessObject)
		{
			foreach (INotification notification in businessObject.RowNotifications)
			{
				yield return GetNotification(businessObject, notification, FormatNotificationType(notification.Type) + businessObject.HumanReadableName + ": " + notification.Message);
			}
		}

		string FormatNotificationType(INotificationType notificationType)
		{
			return IncludeNotificationTypeInMessage ? ZNotificationsExtensions.NotificationTypeName(notificationType, ResourceStrings.Grammar.PluralState.NonPlural) + " - " : "";
		}

		#endregion

		#region GetFormattedPropertyName

		string GetFormattedPropertyName(ZPropertyInfo property)
		{
			string result = GetPropertyName(property);
			if (!string.IsNullOrEmpty(result))
			{
				result += ": ";
			}
			return result;
		}

		string GetPropertyName(ZPropertyInfo property)
		{
			string result = "";

			if (DescriptionToInclude == PropertyDescriptionType.ColumnName)
			{
				result = property.Name;
			}
			else if (DescriptionToInclude == PropertyDescriptionType.HumanReadableName)
			{
				_ = property.HasHumanReadableName;
				result = property.HumanReadableName;
			}

			return result;
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<INotification> GetEnumerator()
		{
			return GetFormattedNotifications(Business, null).GetEnumerator();
		}

		#endregion

		#region Anonymous String Functions

		static bool ShouldCapitalize(string s)
		{
			return s.Length >= 2 && (!(char.IsLower(s[0]) && char.IsUpper(s[1])));
		}

		static string UppercaseFirst(string s)
		{
			return char.ToUpper(s[0]) + (s.Length > 1 ? s.Substring(1) : string.Empty);
		}

		#endregion
	}

	#region Debugger Type Proxy

	class IEnumerableNotification_DebuggerTypeProxy
	{
		public IEnumerableNotification_DebuggerTypeProxy(ZNotificationCollector notifications)
		{
			this.notifications = notifications;
		}

		public INotification[] Notifications
		{
			get { return new List<INotification>(notifications).ToArray(); }
		}

		readonly ZNotificationCollector notifications;
	}

	#endregion
}
