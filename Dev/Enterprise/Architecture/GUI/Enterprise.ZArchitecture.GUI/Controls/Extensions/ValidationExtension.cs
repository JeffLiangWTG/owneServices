using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI.Controls.Extensions
{
	#region Interface

	public interface IValidationExtension : IControlExtension
	{
		bool IsValidating { get; }
		void Validate();
	}

	#endregion

	public class ValidationExtension : ControlExtension, IValidationExtension
	{
		#region Fields

		bool isValidating;

		#endregion

		#region Mounting

		public override void Initialize(IExtendedControl owner)
		{
			base.Initialize(owner);
			Owner.Host.Validated += OnValidated;
		}

		public override void Dispose()
		{
			if (Owner != null)
			{
				Owner.Host.Validated -= OnValidated;
			}
			base.Dispose();
		}

		#region Control Event Handlers

		void OnValidated(object sender, EventArgs args)
		{
			Validate();
		}

		#endregion

		#endregion

		#region Implementation

		public bool IsValidating
		{
			get { return isValidating; }
		}

		public void Validate()
		{
			if (Owner == null)
			{
				return;
			}

			var notificationBindingMembers = Owner.Host as INotificationDataMembers;
			var dataBoundControl = DataBoundControl.Get(Owner.Host);

			try
			{
				isValidating = true;

				if (notificationBindingMembers != null)
				{
					foreach (var dataMember in notificationBindingMembers.NotificationDataMembers)
					{
						if (!string.IsNullOrEmpty(dataMember))
						{
							var obj = dataBoundControl == null ? null : GetObjectFromControl(Owner.Host, dataBoundControl.DataSource, new KBindingMemberInfo(dataMember).BindingPath) as BusinessObject;
							if (obj != null && !obj.IsDeleted)
							{
								ValidateProperty(obj, new KBindingMemberInfo(dataMember).BindingField);
							}
						}
					}
				}
				else if (dataBoundControl != null && !string.IsNullOrEmpty(dataBoundControl.DataMember))
				{
					var obj = dataBoundControl == null ? null : GetObjectFromControl(Owner.Host, dataBoundControl.DataSource, new KBindingMemberInfo(dataBoundControl.DataMember).BindingPath) as BusinessObject;
					if (obj != null && !obj.IsDeleted)
					{
						using (PerformanceStatisticsCollector.StartMonitoring("ValidateProperty", obj.GetType().FullName + ":" + dataBoundControl.DataMember))
						{
							ValidateProperty(obj, new KBindingMemberInfo(dataBoundControl.DataMember).BindingField);
						}
					}
				}
			}
			finally
			{
				isValidating = false;
				ForceNotificationRedraw();
			}
		}

		protected virtual object GetObjectFromControl(Control control, object dataSource, string bindingPath)
		{
			object result = null;
			if (dataSource != null && control.BindingContext != null)
			{
				var manager = control.BindingContext[dataSource, bindingPath];
				if (manager != null && manager.Position > -1 && manager.Position < manager.Count)
				{
					result = manager.GetCurrent();
				}
			}
			return result;
		}

		protected virtual void ValidateProperty(BusinessObject obj, string property)
		{
			if (!obj.IsDeleted)
			{
				((IBusinessObjectInternals)obj).Validate(property);
			}
		}

		protected virtual void ForceNotificationRedraw()
		{
			if (Owner.Extensions != null && Owner.Extensions.Supports<INotificationExtension>())
			{
				Owner.Extensions.Get<INotificationExtension>().Redraw();
			}
		}

		#endregion
	}
}
