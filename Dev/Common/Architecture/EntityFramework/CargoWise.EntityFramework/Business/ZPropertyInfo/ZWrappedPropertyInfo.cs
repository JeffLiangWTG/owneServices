using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public delegate ZPropertyInfo ZPropertyInfoGetter(object sender);

	public sealed class ZWrappedPropertyInfo : ZPropertyInfo, IWrappedPropertyInfo
	{
		internal ZWrappedPropertyInfo(string outerPropertyName, BusinessObject outerBizO, ZPropertyInfoGetter innerInfoGetter)
			: base(outerBizO, outerPropertyName, null)
		{
			if (innerInfoGetter == null)
			{
				throw new ArgumentNullException(nameof(innerInfoGetter));
			}
			this.innerInfoGetter = innerInfoGetter;
		}

		public ZPropertyInfo InnerInfo
		{
			get { return innerInfo ?? (innerInfo = GetInnerInfo()); }
		}
		ZPropertyInfo innerInfo;

		public bool IsLoaded
		{
			get { return innerInfo != null || BizObj.IsWrappedPropertyLoaded(this); }
		}

		ZPropertyInfo GetInnerInfo()
		{
			ZPropertyInfo result = innerInfoGetter(this);
			if (result != null && result.BizObj != this.BizObj)
			{
				BizObj.MarkWrappedPropertyAsLoaded(this);
			}
			return result;
		}

		readonly ZPropertyInfoGetter innerInfoGetter;

		#region MaxLength

		public override bool SupportsMaxLength => InnerInfo?.SupportsMaxLength ?? false;

		#endregion

		#region HasUserDescription

		protected override bool HasUserDescriptionCore => InnerInfo?.HasUserDescription ?? false;

		#endregion

		#region Description

		protected override string DescriptionCore => InnerInfo?.Description;

		#endregion

		#region DefaultHumanReadableName

		protected override ZString DefaultHumanReadableName => InnerInfo?.HumanReadableName ?? ZString.Empty;

		#endregion

		#region IsNullable

		protected override bool IsNullableCore => InnerInfo?.IsNullable ?? false;

		#endregion

		#region IsPersistent

		/// <summary>
		/// Will this property be stored persistently in the database when Factory.Save() is called?
		/// </summary>
		protected override bool IsPersistentCore
		{
			get { return false; }
		}

		#endregion

		#region Value

		bool HasPropertyOnBizObj
		{
			get
			{
				if (!HasCheckedIfPropertyIsOnBizObj)
				{
					fHasPropertyOnBizObj = Name.IndexOf('+') < 0 && BizObj.GetProperties().Find(Name, false) != null;
					//fHasPropertyOnBizObj = BizObj.GetType().GetProperty(Name, BindingFlags.NonPublic | BindingFlags.Public) != null;
					HasCheckedIfPropertyIsOnBizObj = true;
				}
				return fHasPropertyOnBizObj;
			}
		}
		bool HasCheckedIfPropertyIsOnBizObj;
		bool fHasPropertyOnBizObj;

		protected override IZType ValueCore
		{
			get
			{
				if (HasPropertyOnBizObj)
				{
					return (IZType)BizObj[Name];
				}
				else
				{
					return InnerInfo != null ? InnerInfo.Value : null;
				}
			}
			set
			{
				if (HasPropertyOnBizObj)
				{
					BizObj[Name] = value;
				}
				else if (InnerInfo != null)
				{
					InnerInfo.Value = value;
				}
			}
		}

		#endregion

		#region ValueChanged

		public override event EventHandler ValueChanged
		{
			add { InnerInfo.ValueChanged += value; }
			remove { InnerInfo.ValueChanged -= value; }
		}

		#endregion

		#region OriginalValue

		protected override IZType OriginalValueCore => InnerInfo?.OriginalValue;

		protected override bool HasChangesCore => InnerInfo != null && InnerInfo.HasChanges;

		#endregion

		#region DefaultValue / DatabaseDefault

		protected override IZType DefaultValueCore => InnerInfo?.DefaultValue;

		protected override object DatabaseDefaultCore => InnerInfo?.DatabaseDefault;

		#endregion

		#region Notifications

		public override bool IsBusinessObjectValidationSuspended()
		{
			return InnerInfo != null && InnerInfo.IsBusinessObjectValidationSuspended();
		}

		protected override void ClearAllNotificationsCore()
		{
			if (InnerInfo != null)
			{
#if DEBUG
				if (InnerInfo is ZWrappedPropertyInfo)
				{
					InnerInfo.ClearAllNotifications();
				}
				else
				{
					bool clearingNotificationAllowed = BizObj.CanSetValidationOnInfo(this);
					if (clearingNotificationAllowed)
					{
						InnerInfo.BizObj.AddInfoForValidation(InnerInfo);
					}
					try
					{
						if (InnerInfo.BizObj.PropertyInfoCurrentlyClearing != null && InnerInfo.BizObj.ValidationTestingEnabled)
						{
							var infoRecusiveWarnning = $"[{BizObj.GetType().Name}]{Name}:Attempted to set ActiveClear whilst it was already set,InnerInfo:{InnerInfo.BizObj.GetType().Name}_{InnerInfo.Name}";
							ErrorReporter.ReportOnce("Attempted to set ActiveClear whilst it was already set", infoRecusiveWarnning);
						}
						InnerInfo.BizObj.PropertyInfoCurrentlyClearing = InnerInfo;
						try
						{
#endif
							InnerInfo.ClearAllNotifications();
#if DEBUG
						}
						finally
						{
							InnerInfo.BizObj.PropertyInfoCurrentlyClearing = null;
						}
					}
					finally
					{
						if (clearingNotificationAllowed)
						{
							InnerInfo.BizObj.RemoveInfoForValidation(InnerInfo);
						}
					}
				}
			}
#else
			}
#endif
		}

		internal override void ClearNotification(INotificationType type, string message)
		{
			InnerInfo?.ClearNotification(type, message);
		}

		protected override void AddNotificationCore(INotificationType type, string message, bool checkInfoCanBeSet)
		{
			InnerInfo?.AddNotification(type, message);
		}

		internal override NotificationCollection CreateNewZNotifications()
		{
			return InnerInfo?.CreateNewZNotifications();
		}

		protected override NotificationCollection NotificationsStorage
		{
			get { return InnerInfo != null ? InnerInfo.Notifications as NotificationCollection : null; }
		}

		protected override void AddAllNotificationsFromCore(ZPropertyInfo source)
		{
			if (source != InnerInfo) // To prevent copying Notifications from the InnerInfo to itself
			{
				InnerInfo?.AddAllNotificationsFrom(source);
			}
		}

		#endregion

		#region AdditionalValidation

		public override event RunValidationInvoker AdditionalValidation
		{
			add { InnerInfo.AdditionalValidation += value; }
			remove { InnerInfo.AdditionalValidation -= value; }
		}

		#endregion
	}
}
