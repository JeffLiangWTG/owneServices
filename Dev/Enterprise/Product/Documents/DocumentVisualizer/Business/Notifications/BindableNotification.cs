using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class BindableNotification : NonPersistentBusinessObject, IObsoleteValidation
	{
		public BindableNotification(INotification notification)
		{
			Argument.NotNull(notification, nameof(notification));

			this.notification = notification;
		}

		readonly INotification notification;

		#region Schema

		public static class Schema
		{
			public const string Source = "Source";
			public const string Type = "Type";
			public const string Message = "Message";
		}

		#endregion

		#region Source

		public ZString Source
		{
			get
			{
				return  notification.Source != null
					? notification.Source.Description
					: Res.GetString("89847b2f-15fb-4089-afe6-5abfc02b5d52", "Unknown");
			}
		}

		public ZPropertyInfo SourceInfo
		{
			get { return GetZPropertyInfo(Schema.Source); }
		}

		#endregion

		#region Type

		public ZString Type
		{
			get { return notification.Type.ToString(); }
		}

		public ZPropertyInfo TypeInfo
		{
			get { return GetZPropertyInfo(Schema.Type); }
		}

		#endregion

		#region Message

		public ZString Message
		{
			get { return notification.Message; }
		}

		public ZPropertyInfo MessageInfo
		{
			get { return GetZPropertyInfo(Schema.Message); }
		}

		#endregion
	}
}
