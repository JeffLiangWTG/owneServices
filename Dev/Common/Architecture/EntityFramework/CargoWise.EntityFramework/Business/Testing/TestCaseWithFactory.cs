#if DEBUG

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static CargoWise.EntityFramework.BusinessObjectFactory;

namespace CargoWise.EntityFramework.Testing
{
	public abstract class TestCaseWithFactory : TransactionedTestCase
	{
		static TestCaseWithFactory()
		{
			IsNullObjectDelegate = (object potentiallyNullObject) =>
			{
				return potentiallyNullObject != null && potentiallyNullObject is ComponentModel.INullable && ((ComponentModel.INullable)potentiallyNullObject).IsNull;
			};
		}

		public override void RunBare()
		{
			RegisterDbConnectionToRollback(Factory.RowFactory.DbConnection);
			base.RunBare();
		}

		static readonly Stack<NotificationInfoCollection> stackedNotificationInfos = new Stack<NotificationInfoCollection>();
		protected override void RunTest()
		{
			try
			{
				stackedNotificationInfos.Push(new NotificationInfoCollection());
				try
				{
					base.RunTest();
					NotificationInfos.FailIfSomeoneCalledAssertNoWithoutCallingAssertHas(this);
				}
				finally
				{
					stackedNotificationInfos.Pop();
				}
			}
			catch (TargetInvocationException e)
			{
				if (e.InnerException is ZSaveConcurrencyException)
				{
					IConcurrencyExceptionHandler handler = (IConcurrencyExceptionHandler)Activator.CreateInstance(ObjectFactory.GetType<IConcurrencyExceptionHandler>(), e.InnerException.InnerException);
					throw new Exception(handler.Info, e.InnerException);
				}
				else
				{
					throw;
				}
			}
		}

		protected void RegisterDbConnectionToRollback(BusinessObjectFactory factory)
		{
			RegisterDbConnectionToRollback(factory.RowFactory.DbConnection);
		}

		#region Factory Isolater

		public static IDisposable GetFactoryIsolater(BusinessObjectFactory factory)
		{
			return new FactoryIsolater(factory);
		}

		class FactoryIsolater : IDisposable
		{
			public FactoryIsolater(BusinessObjectFactory factory)
			{
				this.factory = factory;

				factory.RefreshEnabled = false;
				((IBusinessObjectFactoryInternals)factory).DisableQueryCacheReset = true;
			}

			public void Dispose()
			{
				factory.RefreshEnabled = true;
				((IBusinessObjectFactoryInternals)factory).DisableQueryCacheReset = false;
			}

			readonly BusinessObjectFactory factory;
		}

		#endregion

		#region Asserting presence or absence of notifications
		#region Errors
		#region Assert Has Errors

		/// <summary>
		/// Before using this method, consider whether AssertHasError() would be more appropriate.
		/// </summary>
		public static void AssertHasErrors(ZPropertyInfo info)
		{
			AssertHasErrors("", info);
		}

		/// <summary>
		/// Before using this method, consider whether AssertHasError() would be more appropriate.
		/// </summary>
		public static void AssertHasErrors(string message, ZPropertyInfo info)
		{
			Assert("Expected errors for '" + info.Name + "' == '" + info.Value + "'. " + message, info.HasErrors());
		}

		#endregion

		#region Assert Has Error

		public static void AssertHasError(string message, ZPropertyInfo info, string notificationExpectedToBeFound)
		{
			Assert(GetAssertHasNotificationString(message, info, notificationExpectedToBeFound, "error"), info.Notifications != null && info.Notifications.GetErrors().Contains(notificationExpectedToBeFound));
		}

		public static void AssertHasError(ZPropertyInfo info, string notificationExpectedToBeFound)
		{
			AssertHasError("", info, notificationExpectedToBeFound);
		}

		public static void AssertHasErrorContaining(string message, ZPropertyInfo info, string partialNotificationExpectedToBeFound)
		{
			Assert(GetAssertHasNotificationString(message, info, partialNotificationExpectedToBeFound, "Error"), info.Notifications != null && info.Notifications.GetErrors().ContainsNotificationContaining(partialNotificationExpectedToBeFound));
		}

		public static void AssertHasErrorContaining(ZPropertyInfo info, string partialNotificationExpectedToBeFound)
		{
			Assert(GetAssertHasNotificationString("", info, partialNotificationExpectedToBeFound, "Error"), info.Notifications != null && info.Notifications.GetErrors().ContainsNotificationContaining(partialNotificationExpectedToBeFound));
		}

		public static void AssertHasErrorContaining(ZPropertyInfo info, IMultilingualString partialNotificationExpectedToBeFound)
		{
			AssertHasErrorContaining(info, partialNotificationExpectedToBeFound.ToString());
		}

		public static void AssertHasRowError(string message, BusinessObject bO, string notificationExpectedToBeFound)
		{
			Assert(GetAssertHasRowNotificationString(message, bO, notificationExpectedToBeFound, "error"), bO.RowNotifications != null && bO.RowNotifications.GetErrors().Contains(notificationExpectedToBeFound));
		}

		public static void AssertHasRowError(BusinessObject bO, string notificationExpectedToBeFound)
		{
			AssertHasRowError("", bO, notificationExpectedToBeFound);
		}

		public static void AssertHasRowErrorContaining(BusinessObject bO, string partialNotificationExpectedToBeFound)
		{
			Assert(GetAssertHasRowNotificationString("", bO, partialNotificationExpectedToBeFound, "Error"), bO.RowNotifications != null && bO.RowNotifications.GetErrors().ContainsNotificationContaining(partialNotificationExpectedToBeFound));
		}

		public static void AssertHasRowWarning(string message, BusinessObject bO, string notificationExpectedToBeFound)
		{
			Assert(GetAssertHasRowNotificationString(message, bO, notificationExpectedToBeFound, "Warning"), bO.RowNotifications != null && bO.RowNotifications.GetWarnings().Contains(notificationExpectedToBeFound));
		}

		public static void AssertHasRowWarning(BusinessObject bO, string notificationExpectedToBeFound)
		{
			AssertHasRowWarning("", bO, notificationExpectedToBeFound);
		}

		public static void AssertHasRowWarningContaining(BusinessObject bO, string partialNotificationExpectedToBeFound)
		{
			AssertHasRowWarningContaining("", bO, partialNotificationExpectedToBeFound);
		}

		public static void AssertHasRowWarningContaining(string message, BusinessObject bO, string partialNotificationExpectedToBeFound)
		{
			Assert(GetAssertHasRowNotificationString(message, bO, partialNotificationExpectedToBeFound, "Warning"), bO.RowNotifications != null && bO.RowNotifications.GetWarnings().ContainsNotificationContaining(partialNotificationExpectedToBeFound));
		}
		#endregion

		#region Assert No Errors
		public static void AssertNoErrors(ZPropertyInfo info)
		{
			AssertNoErrors("", info);
		}

		public static void AssertNoErrors(string message, ZPropertyInfo info)
		{
			Assert(GetAssertNoNotificationsString(message, info, "Error"), !info.HasErrors());
		}

		public static void AssertNoError(ZPropertyInfo info, string notificationNotExpectedToBeFound)
		{
			AssertNoError("", info, notificationNotExpectedToBeFound);
		}

		public static void AssertNoError(string message, ZPropertyInfo info, string notificationNotExpectedToBeFound)
		{
			Assert(GetAssertNoNotificationsString(message, info, notificationNotExpectedToBeFound, "Error"), !info.HasError(notificationNotExpectedToBeFound));
		}

		public static void AssertNoErrorContaining(string message, ZPropertyInfo info, string partialNotificationNotExpectedToBeFound)
		{
			Assert(GetAssertNoNotificationsString(message, info, partialNotificationNotExpectedToBeFound, "Error"), info.Notifications == null || !info.Notifications.GetErrors().ContainsNotificationContaining(partialNotificationNotExpectedToBeFound));
		}

		public static void AssertNoErrorContaining(ZPropertyInfo info, string partialNotificationNotExpectedToBeFound)
		{
			AssertNoErrorContaining(ZString.Empty, info, partialNotificationNotExpectedToBeFound);
		}

		public static void AssertNoErrorContaining(ZPropertyInfo info, IMultilingualString partialNotificationNotExpectedToBeFound)
		{
			AssertNoErrorContaining(ZString.Empty, info, partialNotificationNotExpectedToBeFound.ToString());
		}

		public static void AssertNoErrors(BusinessObject bO)
		{
			AssertNoErrors("", bO);
		}

		public static void AssertNoErrors(string message, BusinessObject bO)
		{
			Assert(GetAssertNoNotificationsString(message, bO, "Error"), !bO.HasErrors);
		}

		public static void AssertNoRowErrors(BusinessObject bO)
		{
			AssertNoRowErrors("", bO);
		}

		public static void AssertNoRowErrors(string message, BusinessObject bO)
		{
			Assert(GetAssertNoRowNotificationsString(message, bO, "Error"), bO.RowNotifications == null || !bO.RowNotifications.HasErrors());
		}

		public static void AssertNoRowError(BusinessObject bO, string notificationNotExpectedToBeFound)
		{
			Assert(GetAssertNoRowNotificationsString("", bO, notificationNotExpectedToBeFound, "Error"), bO.RowNotifications == null || !bO.RowNotifications.GetErrors().Contains(notificationNotExpectedToBeFound));
		}

		public static void AssertNoRowError(string message, BusinessObject bO, string notificationNotExpectedToBeFound)
		{
			Assert(GetAssertNoRowNotificationsString(message, bO, notificationNotExpectedToBeFound, "Error"), bO.RowNotifications == null || !bO.RowNotifications.GetErrors().Contains(notificationNotExpectedToBeFound));
		}

		public static void AssertNoRowWarnings(BusinessObject bO)
		{
			AssertNoRowWarnings(string.Empty, bO);
		}

		public static void AssertNoRowWarnings(string message, BusinessObject bO)
		{
			Assert(GetAssertNoRowNotificationsString(message, bO, "Warning"), bO.RowNotifications == null || !bO.RowNotifications.HasWarnings());
		}

		public static void AssertNoRowErrorContaining(BusinessObject bO, string partialNotificationNotExpectedToBeFound)
		{
			Assert(GetAssertNoRowNotificationsString("", bO, partialNotificationNotExpectedToBeFound, "Error"), bO.RowNotifications == null || !bO.RowNotifications.GetErrors().ContainsNotificationContaining(partialNotificationNotExpectedToBeFound));
		}

		public static void AssertNoWarningError(BusinessObject bO, string notificationNotExpectedToBeFound)
		{
			Assert(GetAssertNoRowNotificationsString("", bO, notificationNotExpectedToBeFound, "Warning"), bO.RowNotifications == null || !bO.RowNotifications.GetWarnings().Contains(notificationNotExpectedToBeFound));
		}

		public static void AssertNoRowWarningContaining(BusinessObject bO, string partialNotificationNotExpectedToBeFound)
		{
			AssertNoRowWarningContaining(string.Empty, bO, partialNotificationNotExpectedToBeFound);
		}

		public static void AssertNoRowWarningContaining(string message, BusinessObject bO, string partialNotificationNotExpectedToBeFound)
		{
			Assert(GetAssertNoRowNotificationsString(message, bO, partialNotificationNotExpectedToBeFound, "Warning"), bO.RowNotifications == null || !bO.RowNotifications.GetWarnings().ContainsNotificationContaining(partialNotificationNotExpectedToBeFound));
		}

		#endregion

		#endregion

		#region Message errors

		#region Assert Has Message Errors

		/// <summary>
		/// Before using this method, consider whether AssertHasMessageError() would be more appropriate.
		/// </summary>
		public static void AssertHasMessageErrors(ZPropertyInfo info)
		{
			AssertHasMessageErrors("", info);
		}

		/// <summary>
		/// Before using this method, consider whether AssertHasMessageError() would be more appropriate.
		/// </summary>
		public static void AssertHasMessageErrors(string message, ZPropertyInfo info)
		{
			Assert("Expected message errors for '" + info.Name + "' == '" + info.Value + "'. " + message, info.HasMessageErrors());
		}

		#endregion

		#region Assert Has Message Error

		public static void AssertHasMessageError(string message, ZPropertyInfo info, string notificationExpectedToBeFound)
		{
			Assert(GetAssertHasNotificationString(message, info, notificationExpectedToBeFound, "message error"), info.Notifications != null && info.Notifications.GetMessageErrors().Contains(notificationExpectedToBeFound));
		}

		public static void AssertHasMessageError(string message, ZPropertyInfo info, IMultilingualString notificationExpectedToBeFound)
		{
			AssertHasMessageError(message, info, notificationExpectedToBeFound.ToString());
		}

		public static void AssertHasMessageError(ZPropertyInfo info, string notificationExpectedToBeFound)
		{
			AssertHasMessageError("", info, notificationExpectedToBeFound);
		}

		public static void AssertHasMessageError(ZPropertyInfo info, IMultilingualString notificationExpectedToBeFound)
		{
			AssertHasMessageError("", info, notificationExpectedToBeFound);
		}

		public static void AssertHasMessageErrorContaining(ZPropertyInfo info, string partialNotificationExpectedToBeFound)
		{
			AssertHasMessageErrorContaining(ZString.Empty, info, partialNotificationExpectedToBeFound);
		}

		public static void AssertHasMessageErrorContaining(string message, ZPropertyInfo info, string partialNotificationExpectedToBeFound)
		{
			Assert(GetAssertHasNotificationString(message, info, partialNotificationExpectedToBeFound, "Message Error"), info.Notifications != null && info.Notifications.GetMessageErrors().ContainsNotificationContaining(partialNotificationExpectedToBeFound));
		}

		public static void AssertHasMessageErrorContaining(ZPropertyInfo info, IMultilingualString partialNotificationExpectedToBeFound)
		{
			AssertHasMessageErrorContaining(info, partialNotificationExpectedToBeFound.ToString());
		}

		#endregion

		#region Assert No Message Errors

		public static void AssertNoMessageErrors(ZPropertyInfo info)
		{
			AssertNoMessageErrors("", info);
		}

		public static void AssertNoMessageErrors(string message, ZPropertyInfo info)
		{
			Assert(GetAssertNoNotificationsString(message, info, "Message Error"), !info.HasMessageErrors());
		}

		public static void AssertNoMessageError(ZPropertyInfo info, string notificationNotExpectedToBeFound)
		{
			AssertNoMessageError("", info, notificationNotExpectedToBeFound);
		}

		public static void AssertNoMessageError(ZPropertyInfo info, IMultilingualString notificationNotExpectedToBeFound)
		{
			AssertNoMessageError("", info, notificationNotExpectedToBeFound);
		}

		public static void AssertNoMessageError(string message, ZPropertyInfo info, string notificationNotExpectedToBeFound)
		{
			Assert(GetAssertNoNotificationsString(message, info, notificationNotExpectedToBeFound, "Message Error"), !info.HasMessageError(notificationNotExpectedToBeFound));
		}

		public static void AssertNoMessageError(string message, ZPropertyInfo info, IMultilingualString notificationNotExpectedToBeFound)
		{
			AssertNoMessageError(message, info, notificationNotExpectedToBeFound.ToString());
		}

		public static void AssertNoMessageErrorContaining(ZPropertyInfo info, string partialNotificationNotExpectedToBeFound)
		{
			AssertNoMessageErrorContaining(ZString.Empty, info, partialNotificationNotExpectedToBeFound);
		}

		public static void AssertNoMessageErrorContaining(string message, ZPropertyInfo info, string partialNotificationNotExpectedToBeFound)
		{
			Assert(GetAssertNoNotificationsString(message, info, partialNotificationNotExpectedToBeFound, "Message Error"), info.Notifications == null || !info.Notifications.GetMessageErrors().ContainsNotificationContaining(partialNotificationNotExpectedToBeFound));
		}

		public static void AssertNoMessageErrorContaining(ZPropertyInfo info, IMultilingualString partialNotificationNotExpectedToBeFound)
		{
			AssertNoMessageErrorContaining(info, partialNotificationNotExpectedToBeFound.ToString());
		}

		public static void AssertNoMessageErrors(BusinessObject bO)
		{
			AssertNoMessageErrors("", bO);
		}

		public static void AssertNoMessageErrors(string message, BusinessObject bO)
		{
			Assert(GetAssertNoNotificationsString(message, bO, "Message Error"), !bO.HasMessageErrors);
		}

		#endregion

		public static void AssertHasRowMessageError(string message, BusinessObject bO, string notificationExpectedToBeFound)
		{
			Assert(GetAssertHasRowNotificationString(message, bO, notificationExpectedToBeFound, "message error"), bO.RowNotifications != null && bO.RowNotifications.GetMessageErrors().Contains(notificationExpectedToBeFound));
		}

		public static void AssertHasRowMessageError(BusinessObject bO, string notificationExpectedToBeFound)
		{
			AssertHasRowMessageError("", bO, notificationExpectedToBeFound);
		}

		public static void AssertHasRowMessageErrorContaining(BusinessObject bO, string partialNotificationExpectedToBeFound)
		{
			Assert(GetAssertHasRowNotificationString("", bO, partialNotificationExpectedToBeFound, "MessageError"), bO.RowNotifications != null && bO.RowNotifications.GetMessageErrors().ContainsNotificationContaining(partialNotificationExpectedToBeFound));
		}

		public static void AssertNoRowMessageError(string message, BusinessObject bO, string notificationNotExpectedToBeFound)
		{
			Assert(GetAssertNoRowNotificationsString(message, bO, notificationNotExpectedToBeFound, "MessageError"), bO.RowNotifications == null || !bO.RowNotifications.GetMessageErrors().Contains(notificationNotExpectedToBeFound));
		}

		public static void AssertNoRowMessageError(BusinessObject bO, string notificationNotExpectedToBeFound)
		{
			Assert(GetAssertNoRowNotificationsString("", bO, notificationNotExpectedToBeFound, "MessageError"), bO.RowNotifications == null || !bO.RowNotifications.GetMessageErrors().Contains(notificationNotExpectedToBeFound));
		}

		public static void AssertNoRowMessageErrorContaining(BusinessObject bO, string partialNotificationNotExpectedToBeFound)
		{
			Assert(GetAssertNoRowNotificationsString("", bO, partialNotificationNotExpectedToBeFound, "MessageError"), bO.RowNotifications == null || !bO.RowNotifications.GetMessageErrors().ContainsNotificationContaining(partialNotificationNotExpectedToBeFound));
		}

		public static void AssertNoRowMessageErrors(BusinessObject bO)
		{
			AssertNoRowMessageErrors("", bO);
		}

		public static void AssertNoRowMessageErrors(string message, BusinessObject bO)
		{
			Assert(GetAssertNoRowNotificationsString(message, bO, "", "MessageError"), !bO.HasRowMessageErrors);
		}

		#endregion

		#region Warnings

		#region Assert Has Warnings

		/// <summary>
		/// Before using this method, consider whether AssertHasWarning() would be more appropriate.
		/// </summary>
		public static void AssertHasWarnings(ZPropertyInfo info)
		{
			AssertHasWarnings("", info);
		}

		/// <summary>
		/// Before using this method, consider whether AssertHasWarning() would be more appropriate.
		/// </summary>
		public static void AssertHasWarnings(string message, ZPropertyInfo info)
		{
			Assert("Expected warnings for '" + info.Name + "' == '" + info.Value + "'. " + message, info.HasWarnings());
		}

		#endregion

		#region Assert Has Warning

		public static void AssertHasWarning(string message, ZPropertyInfo info, string notificationExpectedToBeFound)
		{
			Assert(GetAssertHasNotificationString(message, info, notificationExpectedToBeFound, "Warning"), info.Notifications != null && info.Notifications.GetWarnings().Contains(notificationExpectedToBeFound));
		}

		public static void AssertHasWarning(string message, ZPropertyInfo info, IMultilingualString notificationExpectedToBeFound)
		{
			AssertHasWarning(message, info, notificationExpectedToBeFound.ToString());
		}

		public static void AssertHasWarning(ZPropertyInfo info, string notificationExpectedToBeFound)
		{
			AssertHasWarning("", info, notificationExpectedToBeFound);
		}

		public static void AssertHasWarning(ZPropertyInfo info, IMultilingualString notificationExpectedToBeFound)
		{
			AssertHasWarning("", info, notificationExpectedToBeFound);
		}

		public static void AssertHasWarningContaining(string message, ZPropertyInfo info, string partialNotificationExpectedToBeFound)
		{
			Assert(GetAssertHasNotificationString(message, info, partialNotificationExpectedToBeFound, "Warning"), info.Notifications != null && info.Notifications.GetWarnings().ContainsNotificationContaining(partialNotificationExpectedToBeFound));
		}

		public static void AssertHasWarningContaining(ZPropertyInfo info, string partialNotificationExpectedToBeFound)
		{
			Assert(GetAssertHasNotificationString("", info, partialNotificationExpectedToBeFound, "Warning"), info.Notifications != null && info.Notifications.GetWarnings().ContainsNotificationContaining(partialNotificationExpectedToBeFound));
		}

		public static void AssertHasWarningContaining(ZPropertyInfo info, IMultilingualString partialNotificationExpectedToBeFound)
		{
			AssertHasWarningContaining(info, partialNotificationExpectedToBeFound.ToString());
		}

		#endregion

		#region Assert No Warnings

		public static void AssertNoWarnings(ZPropertyInfo info)
		{
			AssertNoWarnings("", info);
		}

		public static void AssertNoWarnings(string message, ZPropertyInfo info)
		{
			Assert(GetAssertNoNotificationsString(message, info, "Warning"), !info.HasWarnings());
		}

		public static void AssertNoWarning(ZPropertyInfo info, string notificationNotExpectedToBeFound)
		{
			AssertNoWarning("", info, notificationNotExpectedToBeFound);
		}

		public static void AssertNoWarning(ZPropertyInfo info, IMultilingualString notificationNotExpectedToBeFound)
		{
			AssertNoWarning("", info, notificationNotExpectedToBeFound.ToString());
		}

		public static void AssertNoWarning(string message, ZPropertyInfo info, string notificationNotExpectedToBeFound)
		{
			Assert(GetAssertNoNotificationsString(message, info, notificationNotExpectedToBeFound, "Warning"), !info.HasWarning(notificationNotExpectedToBeFound));
		}

		public static void AssertNoWarning(string message, ZPropertyInfo info, IMultilingualString notificationNotExpectedToBeFound)
		{
			AssertNoWarning(message, info, notificationNotExpectedToBeFound.ToString());
		}

		public static void AssertNoWarningContaining(string message, ZPropertyInfo info, string partialNotificationNotExpectedToBeFound)
		{
			Assert(GetAssertNoNotificationsString(message, info, partialNotificationNotExpectedToBeFound, "Warning"), info.Notifications == null || !info.Notifications.GetWarnings().ContainsNotificationContaining(partialNotificationNotExpectedToBeFound));
		}

		public static void AssertNoWarningContaining(ZPropertyInfo info, string partialNotificationNotExpectedToBeFound)
		{
			Assert(GetAssertNoNotificationsString("", info, partialNotificationNotExpectedToBeFound, "Warning"), info.Notifications == null || !info.Notifications.GetWarnings().ContainsNotificationContaining(partialNotificationNotExpectedToBeFound));
		}

		public static void AssertNoWarningContaining(ZPropertyInfo info, IMultilingualString partialNotificationNotExpectedToBeFound)
		{
			AssertNoWarningContaining(info, partialNotificationNotExpectedToBeFound.ToString());
		}

		public static void AssertNoWarnings(BusinessObject bO)
		{
			AssertNoWarnings("", bO);
		}

		public static void AssertNoWarnings(string message, BusinessObject bO)
		{
			Assert(GetAssertNoNotificationsString(message, bO, "Warning"), !bO.HasWarnings);
		}

		#endregion

		#endregion

		#region Notifications

		#region Assert Has Notifications

		/// <summary>
		/// Before using this method, consider whether AssertHasNotification() would be more appropriate.
		/// </summary>
		public static void AssertHasNotifications(ZPropertyInfo info)
		{
			AssertHasNotifications("", info);
		}

		/// <summary>
		/// Before using this method, consider whether AssertHasNotification() would be more appropriate.
		/// </summary>
		public static void AssertHasNotifications(string message, ZPropertyInfo info)
		{
			Assert("Expected notifications for '" + info.Name + "' == '" + info.Value + "'. " + message, info.HasNotifications());
		}

		#endregion

		#region Assert No Notifications

		public static void AssertNoNotifications(ZPropertyInfo info)
		{
			AssertNoNotifications("", info);
		}

		public static void AssertNoNotifications(string message, ZPropertyInfo info)
		{
			Assert(GetAssertNoNotificationsString(message, info, "notification"), !info.HasNotifications());
		}

		public static void AssertNoNotifications(BusinessObject bO)
		{
			AssertNoNotifications("", bO);
		}

		public static void AssertNoNotifications(string message, BusinessObject bO)
		{
			Assert(GetAssertNoNotificationsString(message, bO, "notification"), !bO.HasNotifications());
		}

		#endregion

		#endregion

		public class NotificationInfoCollection
		{
			#region NotificationInfos
			Dictionary<string, NotificationInfo> NotificationInfos
			{
				get
				{
					if (fNotificationInfos == null)
					{
						fNotificationInfos = new Dictionary<string, NotificationInfo>();
					}
					return fNotificationInfos;
				}
			}
			Dictionary<string, NotificationInfo> fNotificationInfos;
			#endregion

			public void AssertHasNotificationCalled(string notificationMessage)
			{
				NotificationInfo info;
				if (!NotificationInfos.TryGetValue(notificationMessage, out info))
				{
					info = new NotificationInfo(notificationMessage);
					NotificationInfos[notificationMessage] = info;
				}
				info.AssertHasNotificationHasBeenCalled();
			}

			public void AssertNoNotificationCalled(string notificationMessage)
			{
				NotificationInfo info;
				if (!NotificationInfos.TryGetValue(notificationMessage, out info))
				{
					info = new NotificationInfo(notificationMessage);
					NotificationInfos[notificationMessage] = info;
				}
				info.AssertNoNotificationHasBeenCalled();
			}

			public void FailIfSomeoneCalledAssertNoWithoutCallingAssertHas(TestCaseWithFactory testCase)
			{
				if (fNotificationInfos != null)
				{
					ZStringBuilder wobblyNotificationMessages = new ZStringBuilder();
					foreach (KeyValuePair<string, NotificationInfo> infoKey in NotificationInfos)
					{
						NotificationInfo notification = infoKey.Value;
						if (notification.CalledAssertNoWithoutCallingAssertHas)
						{
							wobblyNotificationMessages.Append("'" + notification.NotificationMessage + "'\r\n");
						}
					}
					if (!wobblyNotificationMessages.IsEmpty)
					{
						Fail(testCase.GetType().ToString() + " " + testCase.RunMethod.ToString() + @"

The following Notification Messages were used in 'AssertNoNotifications' without a matching 
'AssertHasNotifications' in the test to make sure the message is still being used at all:

" + wobblyNotificationMessages.ToString() +
@"
Please make sure that any time you use 'AssertNoNotifications' to make sure that a Notification
Message is NOT used that you use 'AssertHasNotifications' to make sure that the message can still be
applied at all. 

Otherwise you might be testing for a message that no longer exists....

");
					}
				}
			}

			class NotificationInfo
			{
				public NotificationInfo(string notificationMessage)
				{
					this.notificationMessage = notificationMessage;
					this.assertHasBeenCalled = false;
					this.assertNoBeenCalled = false;
				}
				readonly string notificationMessage;
				bool assertHasBeenCalled;
				bool assertNoBeenCalled;

				public string NotificationMessage
				{
					get { return notificationMessage; }
				}

				public void AssertHasNotificationHasBeenCalled()
				{
					assertHasBeenCalled = true;
				}
				public void AssertNoNotificationHasBeenCalled()
				{
					assertNoBeenCalled = true;
				}
				public bool CalledAssertNoWithoutCallingAssertHas
				{
					get { return assertNoBeenCalled && !assertHasBeenCalled; }
				}
			}
		}

		static NotificationInfoCollection NotificationInfos
		{
			get { return stackedNotificationInfos.Count > 0 ? stackedNotificationInfos.Peek() : new NotificationInfoCollection(); }
		}

		static string GetAssertHasNotificationString(string message, ZPropertyInfo info, string notification, string notificationTypeName)
		{
			NotificationInfos.AssertHasNotificationCalled(notification);
			int notificationCount = info.Notifications != null ? info.Notifications.Count() : 0;
			string notificationText = info.Notifications != null ? GetUniqueMessageListWithTypes(info.Notifications) : string.Empty;
			return string.Format("The property '{0}' == '{1}' should have this {2}:\r\n'{3}'\r\n{4}\r\nNotification count on this property is {5}.\r\n{6}", info.Name, info.Value, notificationTypeName, notification, message, notificationCount, notificationText);
		}

		static string GetAssertNoNotificationsString(string message, ZPropertyInfo info, string notification, string notificationTypeName)
		{
			NotificationInfos.AssertNoNotificationCalled(notification);
			int notificationCount = info.Notifications != null ? info.Notifications.Count() : 0;
			string notificationText = info.Notifications != null ? GetUniqueMessageListWithTypes(info.Notifications) : string.Empty;
			return string.Format("The property '{0}' == '{1}' should not have this {2}:\r\n'{3}'\r\n{4}\r\nNotification count on this property is {5}.\r\n{6}", info.Name, info.Value, notificationTypeName, notification, message, notificationCount, notificationText);
		}

		static string GetAssertNoNotificationsString(string message, ZPropertyInfo info, string notificationTypeName)
		{
			int notificationCount = info.Notifications != null ? info.Notifications.Count() : 0;
			string notificationText = info.Notifications != null ? GetUniqueMessageListWithTypes(info.Notifications) : string.Empty;
			return string.Format("Expected no {0}s for '{1}'.\r\n{2}\r\nNotification count on this property is {3}.\r\n{4}", notificationTypeName, info.Name, message, notificationCount, notificationText);
		}

		static string GetAssertNoNotificationsString(string message, BusinessObject bO, string notificationTypeName)
		{
			return string.Format("Expected no {0}s for '{1}'.\r\n{2}\r\nNotification count on this property is {3}.\r\n{4}", notificationTypeName, bO.GetType().FullName, message, bO.NotificationsIncludingChildren.Count(), GetUniqueMessageListWithTypes(bO.NotificationsIncludingChildren));
		}

		static string GetAssertHasRowNotificationString(string message, BusinessObject bO, string notification, string notificationTypeName)
		{
			int notificationCount = bO.RowNotifications != null ? bO.RowNotifications.Count() : 0;
			string notificationText = bO.RowNotifications != null ? GetUniqueMessageListWithTypes(bO.RowNotifications) : "";
			return string.Format("The BO '{0}' should have this {1}:\r\n'{2}'\r\n{3}\r\nRowNotification count on this BO is {4}.\r\n{5}", bO.HumanReadableName, notificationTypeName, notification, message, notificationCount, notificationText);
		}

		static string GetAssertNoRowNotificationsString(string message, BusinessObject bO, string notification, string notificationTypeName)
		{
			int notificationCount = bO.RowNotifications != null ? bO.RowNotifications.Count() : 0;
			string notificationText = bO.RowNotifications != null ? GetUniqueMessageListWithTypes(bO.RowNotifications) : "";
			return string.Format("The BO Row '{0}' should not have this {1}:\r\n'{2}'\r\n{3}\r\nNotification count on this BO Row is {4}.\r\n{5}", bO.HumanReadableName, notificationTypeName, notification, message, notificationCount, notificationText);
		}

		static string GetAssertNoRowNotificationsString(string message, BusinessObject bO, string notificationTypeName)
		{
			int notificationCount = bO.RowNotifications != null ? bO.RowNotifications.Count() : 0;
			string notificationText = bO.RowNotifications != null ? GetUniqueMessageListWithTypes(bO.RowNotifications) : "";
			return string.Format("Expected no {0}s for '{1}'.\r\n{2}\r\nNotification count on this property is {3}.\r\n{4}", notificationTypeName, bO.HumanReadableName, message, notificationCount, notificationText);
		}

		static string GetUniqueMessageListWithTypes(IEnumerable<INotification> notifications)
		{
			var notificationList = Argument.NotNull(notifications, nameof(notifications));
			var result = new List<string>();
			foreach (var notification in notificationList)
			{
				if (notification != null && !result.Contains(notification.Message))
				{
					result.Add($"{notification.Type.NotificationTypeName(ResourceStrings.Grammar.PluralState.NonPlural)}: {notification.Message}");
				}
			}

			return string.Join("\n", result);
		}

		#endregion

		#region AssertMaxDbHits

		public void AssertMaxTableHits(int maxHits, string tableName)
		{
			AssertMaxTableHits(maxHits, tableName, Factory);
		}

		public void AssertMaxTableHits(int maxHits, string tableName, BusinessObjectFactory factory)
		{
			AssertMaxTableHits(string.Empty, maxHits, tableName, factory);
		}

		public void AssertMaxTableHits(string message, int maxHits, string tableName)
		{
			AssertMaxTableHits(message, maxHits, tableName, Factory);
		}

		public void AssertMaxTableHits(string message, int maxHits, string tableName, BusinessObjectFactory factory)
		{
			var totalHits = factory.GetTableHitCount(tableName);
			AssertEquals(string.Format("Expected at most {0} hits on {1} but was {2}. {3}", maxHits, tableName, totalHits, message), true, maxHits >= totalHits);
		}

		public void AssertMaxHitsForAnyTable(string message, int maxHits)
		{
			AssertMaxHitsForAnyTable(message, maxHits, Factory);
		}

		public void AssertMaxHitsForAnyTable(string message, int maxHits, BusinessObjectFactory factory)
		{
			var tablesExceedingMaxHits = factory.RowFactory.TableSelects.Where(t => t.Value > maxHits).ToArray();
			var tableHitMessages = string.Join(System.Environment.NewLine, tablesExceedingMaxHits.Select(t => string.Format("    {0} has {1} hits.", t.TableName, t.Value)));

			AssertEquals(string.Format(
@"Expected at most {0} hits on all tables but was:
{1}

{2}", maxHits, tableHitMessages, message), true, 0 == tablesExceedingMaxHits.Length);
		}

		#endregion

		#region AssertDbHitsForAllFactories

		/// <summary>
		///		Asserts the db hits aggregated across all BusinessObjectFactories, including those that would normally pass out of scope during execution of the code inside the using block.
		/// </summary>
		/// <example>
		///		using (AssertDbHitsForAllFactories(hits))
		///		{
		///			DoCostlyOperation();
		///		}
		/// </example>
		/// <param name="expectedHitCounts">
		///		The dictionary of expected hits per table name.
		/// </param>
		/// <param name="ignoreUnspecified">
		///		A value indicating whether to ignore hits from the tables not listed in <paramref name="expectedHitCounts"/>.
		/// </param>
		/// <param name="thresholdForUnspecified">
		///		A value indicating a critical number of hits for unspecified tables, i.e. tables with the number of hits equal or greater to 
		///		this number will be included into the result even though they are not listed in <paramref name="expectedHitCounts"/> 
		///		and <paramref name="ignoreUnspecified"/> is <c>true</c>.
		/// </param>
		/// <param name="includeFactoryPredicate">
		///		Filters the factories which should have their hits tracked. Leave this as null to include all factories.
		/// </param>
		/// <returns>
		///		An <see cref="IDisposable"/> which should be wrapped in a using block to stop caching transient factories.
		/// </returns>
		public static IDisposable AssertDbHitsForAllFactories(IDictionary<string, int> expectedHitCounts, bool ignoreUnspecified = false, bool useOnlyNewFactories = false, int thresholdForUnspecified = 5, Predicate<BusinessObjectFactory> includeFactoryPredicate = null, bool ignoreHitsFromTablesCachedInUberFactory = false, string[] tablesToCollectQueriesFor = null, Func<string, bool> shouldCollectQueriesForExistingFactory = null)
		{
			return AssertDbHitsForAllFactories(null, expectedHitCounts, ignoreUnspecified, useOnlyNewFactories, thresholdForUnspecified, includeFactoryPredicate, ignoreHitsFromTablesCachedInUberFactory, tablesToCollectQueriesFor: tablesToCollectQueriesFor, shouldCollectQueriesForExistingFactory: shouldCollectQueriesForExistingFactory);
		}

		/// <summary>
		///		Asserts the db hits aggregated across all BusinessObjectFactories, including those that would normally pass out of scope during execution of the code inside the using block.
		/// </summary>
		/// <example>
		///		using (AssertDbHitsForAllFactories(hits))
		///		{
		///			DoCostlyOperation();
		///		}
		/// </example>
		/// <param name="message">
		///		The message to be displayed within the assertion failure.
		/// </param>
		/// <param name="expectedHitCounts">
		///		The dictionary of expected hits per table name.
		/// </param>
		/// <param name="ignoreUnspecified">
		///		A value indicating whether to ignore hits from the tables not listed in <paramref name="expectedHitCounts"/>.
		/// </param>
		/// <param name="thresholdForUnspecified">
		///		A value indicating a critical number of hits for unspecified tables, i.e. tables with the number of hits equal or greater to 
		///		this number will be included into the result even though they are not listed in <paramref name="expectedHitCounts"/> 
		///		and <paramref name="ignoreUnspecified"/> is <c>true</c>.
		/// </param>
		/// <param name="includeFactoryPredicate">
		///		Filters the factories which should have their hits tracked. Leave this as null to include all factories.
		/// </param>
		/// <returns>
		///		An <see cref="IDisposable"/> which should be wrapped in a using block to stop caching transient factories.
		/// </returns>
		public static IDisposable AssertDbHitsForAllFactories(string message, IDictionary<string, int> expectedHitCounts, bool ignoreUnspecified = false, bool useOnlyNewFactories = false, int thresholdForUnspecified = 5, Predicate<BusinessObjectFactory> includeFactoryPredicate = null, bool ignoreHitsFromTablesCachedInUberFactory = false, int hitTolerance = 0, string stackTraceToIgnore = null, string[] tablesToCollectQueriesFor = null, int acceptableVariance = 0, IList<string> tablesToIgnore = null, Func<string, bool> shouldCollectQueriesForExistingFactory = null, bool logNonPersistentTableHit = false)
		{
			return AssertDbHitsForAllFactoriesCore((expected, actual) => actual < 0 ? actual == expected : actual < hitTolerance || (actual >= expected - acceptableVariance && actual <= expected + acceptableVariance), message, expectedHitCounts, ignoreUnspecified, useOnlyNewFactories, thresholdForUnspecified, includeFactoryPredicate, ignoreHitsFromTablesCachedInUberFactory, stackTraceToIgnore, tablesToCollectQueriesFor, tablesToIgnore, shouldCollectQueriesForExistingFactory, logNonPersistentTableHit);
		}

		/// <summary>
		///		Asserts the maximum db hits aggregated across all BusinessObjectFactories, including those that would normally pass out of scope during execution of the code inside the using block.
		/// </summary>
		/// <example>
		///		using (AssertMaxDbHitsForAllFactories(hits))
		///		{
		///			DoCostlyOperation();
		///		}
		/// </example>
		/// <param name="expectedMaxHitCounts">
		///		The dictionary of expected max hits per table name.
		/// </param>
		/// <param name="ignoreUnspecified">
		///		A value indicating whether to ignore hits from the tables not listed in <paramref name="expectedMaxHitCounts"/>.
		/// </param>
		/// <param name="thresholdForUnspecified">
		///		A value indicating a critical number of hits for unspecified tables, i.e. tables with the number of hits equal or greater to 
		///		this number will be included into the result even though they are not listed in <paramref name="expectedMaxHitCounts"/> 
		///		and <paramref name="ignoreUnspecified"/> is <c>true</c>.
		/// </param>
		/// <param name="includeFactoryPredicate">
		///		Filters the factories which should have their hits tracked. Leave this as null to include all factories.
		/// </param>
		/// <returns>
		///		An <see cref="IDisposable"/> which should be wrapped in a using block to stop caching transient factories.
		/// </returns>
		public static IDisposable AssertMaxDbHitsForAllFactories(IDictionary<string, int> expectedMaxHitCounts, bool ignoreUnspecified = false, bool useOnlyNewFactories = false, int thresholdForUnspecified = 5, Predicate<BusinessObjectFactory> includeFactoryPredicate = null, bool ignoreHitsFromTablesCachedInUberFactory = false)
		{
			return AssertMaxDbHitsForAllFactories(null, expectedMaxHitCounts, ignoreUnspecified, useOnlyNewFactories, thresholdForUnspecified, includeFactoryPredicate, ignoreHitsFromTablesCachedInUberFactory);
		}

		/// <summary>
		///		Asserts the maximum db hits aggregated across all BusinessObjectFactories, including those that would normally pass out of scope during execution of the code inside the using block.
		/// </summary>
		/// <example>
		///		using (AssertMaxDbHitsForAllFactories(hits))
		///		{
		///			DoCostlyOperation();
		///		}
		/// </example>
		/// <param name="message">
		///		The message to be displayed within the assertion failure.
		/// </param>
		/// <param name="expectedMaxHitCounts">
		///		The dictionary of expected max hits per table name.
		/// </param>
		/// <param name="ignoreUnspecified">
		///		A value indicating whether to ignore hits from the tables not listed in <paramref name="expectedMaxHitCounts"/>.
		/// </param>
		/// <param name="thresholdForUnspecified">
		///		A value indicating a critical number of hits for unspecified tables, i.e. tables with the number of hits equal or greater to 
		///		this number will be included into the result even though they are not listed in <paramref name="expectedMaxHitCounts"/> 
		///		and <paramref name="ignoreUnspecified"/> is <c>true</c>.
		/// </param>
		/// <param name="includeFactoryPredicate">
		///		Filters the factories which should have their hits tracked. Leave this as null to include all factories.
		/// </param>
		/// <returns>
		///		An <see cref="IDisposable"/> which should be wrapped in a using block to stop caching transient factories.
		/// </returns>
		public static IDisposable AssertMaxDbHitsForAllFactories(string message, IDictionary<string, int> expectedMaxHitCounts, bool ignoreUnspecified = false, bool useOnlyNewFactories = false, int thresholdForUnspecified = 5, Predicate<BusinessObjectFactory> includeFactoryPredicate = null, bool ignoreHitsFromTablesCachedInUberFactory = false, string stackTraceToIgnore = null, string[] tablesToCollectQueriesFor = null, Func<string, bool> shouldCollectQueriesForExistingFactory = null)
		{
			return AssertDbHitsForAllFactoriesCore((expectedMax, actual) => actual <= expectedMax, message, expectedMaxHitCounts, ignoreUnspecified, useOnlyNewFactories, thresholdForUnspecified, includeFactoryPredicate, ignoreHitsFromTablesCachedInUberFactory, stackTraceToIgnore, tablesToCollectQueriesFor, null, shouldCollectQueriesForExistingFactory);
		}

		static string GetNiceTableStyleCss(string textAlignment = "Center") => string.Format(@"
<style>
th.local, table tr td.local {{
	border: 1px solid black;
	border-collapse: collapse;
	padding: 5px;
	text-align: {0};
}}
table.local {{
	border: 1px solid black;
	border-collapse: collapse; 
}}
th.local {{
	background:#005596;
	color:#FFF;
	white-space:nowrap;
}}
</style>
", textAlignment);

		public static string GetQueryHitDetails(IEnumerable<(string TableName, string FactoryName, IEnumerable<TableHitQuery> Queries)> tableWithQueries, string testFilesPath, string ignoreStackTraceBeforeThis)
		{
			string fileName = Path.Combine(testFilesPath, string.Format(CultureInfo.InvariantCulture, "QueryHits_{0}.csv", ZGuid.NewZGuid().ToString()));
			using (var stream = File.CreateText(fileName))
			{
				foreach (var tableWithQuery in tableWithQueries)
				{
					foreach (var query in tableWithQuery.Queries)
					{
						var lineData = query.StackTrace.SplitByLine().Select(x => x.Trim()).Reverse().ToList();
						var indexToRemoveFrom = lineData.IndexOf(ignoreStackTraceBeforeThis);
						if (indexToRemoveFrom != -1)
						{
							lineData.RemoveRange(0, indexToRemoveFrom + 1);
						}
						lineData.Insert(0, query.Query);
						lineData.Insert(0, tableWithQuery.TableName);
						lineData.Insert(0, tableWithQuery.FactoryName);
						stream.Write(CreateCsvLine(lineData.ToArray()));
					}
				}
				stream.Flush();
			}

			return fileName;
		}

		static string CreateCsvLine(string[] fieldValues)
		{
			var result = new StringBuilder();
			for (int i = 0; i < fieldValues.Length; i++)
			{
				string fieldValue = fieldValues[i] ?? string.Empty;
				if (i != 0)
				{
					result.Append(",");
				}
				result.Append("\"" + fieldValue.Replace("\"", "\"\"") + "\"");
			}
			result.Append(Environment.NewLine);
			return result.ToString();
		}

		delegate bool AssertionCheck<T>(T expected, T actual);
		static IDisposable AssertDbHitsForAllFactoriesCore(AssertionCheck<int> assertionCheck, string message, IDictionary<string, int> expectedHitCounts, bool ignoreUnspecified, bool useOnlyNewFactories, int thresholdForUnspecified, Predicate<BusinessObjectFactory> includeFactoryPredicate, bool ignoreHitsFromTablesCachedInUberFactory, string stackTraceToIgnore, string[] tablesToCollectQueriesFor, IList<string> tablesToIgnore, Func<string, bool> shouldCollectQueriesForExistingFactory, bool logNonPersistentTableHit = false)
		{
			// Note: logNonPersistentTableHit will be remove in another workitem as we want to log non persistent hint

			var disposable = PersistentFactoryCacheManager.Instance.TrackAllCreatedFactories_ForTest(tablesToCollectQueriesFor, shouldCollectQueriesForExistingFactory, logNonPersistentTableHit: (logNonPersistentTableHit || !NUnit.Framework.TestingState.IsRunningOnDAT));
			var factories = GetMatchingFactories(includeFactoryPredicate).ToList();
			EnsureFactoriesAreNamed(factories);
			var startingHits = GetTableHits(factories);

			return new DisposableAction(() =>
			{
				try
				{
					AssertDbHitsForAllFactoriesOnDispose(message, expectedHitCounts, assertionCheck, ignoreUnspecified, useOnlyNewFactories, thresholdForUnspecified, includeFactoryPredicate, ignoreHitsFromTablesCachedInUberFactory, stackTraceToIgnore, tablesToIgnore, startingHits);
				}
				finally
				{
					disposable.Dispose();
				}
			});
		}

		static void AssertDbHitsForAllFactoriesOnDispose(string message, IDictionary<string, int> expectedHitCounts, AssertionCheck<int> assertionCheck, bool ignoreUnspecified, bool useOnlyNewFactories, int thresholdForUnspecified, Predicate<BusinessObjectFactory> includeFactoryPredicate, bool ignoreHitsFromTablesCachedInUberFactory, string stackTraceToIgnore, IList<string> tablesToIgnore, HitCountsByTable startingHits)
		{
			wasAssertDbHitsCalled = true;

			var caption = !string.IsNullOrWhiteSpace(message) ? "DB Hits - " + message : "DB Hits";
			var result = new StringBuilder(GetNiceTableStyleCss()).Append($@"
<table class=""local"">
	<caption><b>{caption}</b></caption>
	<tr>
		<th class=""local"">Table</th>
		<th class=""local"">Expected</th>
		<th class=""local"">Actual</th>
		<th class=""local"">Factory</th>
	</tr>
");
			tablesToIgnore = tablesToIgnore ?? new List<string>();
			tablesToIgnore = tablesToIgnore.Concat(TablesToAlwaysIgnore()).ToList();

			foreach (var pair in expectedHitCounts)
			{
				Assert($"Table ({pair.Key}) is in the Ignore list, but is also expected to have hits tracked against it. Either remove it from the Ignore list, or remove it from the Expected hits list.", !tablesToIgnore.Contains(pair.Key));
			}

			var factories = (useOnlyNewFactories ? PersistentFactoryCacheManager.Instance.TrackedFactories_ForTest : GetMatchingFactories(null)).Where(f => includeFactoryPredicate == null || includeFactoryPredicate(f)).ToArray();
			var success = true;
			EnsureFactoriesAreNamed(factories);
			var hits = GetTableHits(factories);
			RemoveStartingHits(hits, startingHits);

			foreach (var hit in hits.OrderBy(_ => _.Key))
			{
				var tableName = hit.Key;
				var hitCountsByFactories = hit.Value;
				var actualHitCount = hitCountsByFactories.Sum(_ => _.Value.Hit);

				if (!expectedHitCounts.TryGetValue(tableName, out int expectedHitCount) && ignoreUnspecified && actualHitCount <= thresholdForUnspecified)
				{
					continue;
				}

				if (!assertionCheck.Invoke(expectedHitCount, actualHitCount) && !(ignoreHitsFromTablesCachedInUberFactory && IsCachedInUberFactory(tableName)) && !tablesToIgnore.Contains(tableName))
				{
					var factoryHits = hitCountsByFactories.OrderByDescending(_ => _.Value.Hit).ToArray();
					var factoryHitsLength = factoryHits.Length;
					for (var i = 0; i < factoryHitsLength; i++)
					{
						var factoryHit = factoryHits[i];
						result.AppendFormat("<tr>");

						if (i == 0)
						{
							var span = factoryHitsLength > 1 ? factoryHitsLength + 1 : 0;

							result.AppendFormat("<td rowspan={0} class=\"local\">{1}</td>", span, tableName);
							result.AppendFormat("<td rowspan={0} class=\"local\">{1}</td>", span, expectedHitCount);
						}

						result.AppendFormat("<td class=\"local\">{0}</td>", factoryHit.Value.Hit);  // Value - hits count in the factory
						result.AppendFormat("<td class=\"local\">{0}</td>", factoryHit.Key);    // Key - factory name
						result.AppendFormat("</tr>");
					}

					if (factoryHitsLength > 1)
					{
						result.AppendFormat("<tr>");
						result.AppendFormat("<td class=\"local\"><b>{0}</b></td>", actualHitCount);
						result.AppendFormat("<td class=\"local\"></td>");
						result.AppendFormat("</tr>");
					}

					success = false;
				}
			}

			var missingHits = expectedHitCounts.Where(hit => !hits.ContainsKey(hit.Key) && hit.Value > 0);
			foreach (var hit in missingHits.Where(h => !assertionCheck.Invoke(h.Value, 0)))
			{
				result.AppendFormat("<tr>");
				result.AppendFormat("<td class=\"local\">{0}</td>", hit.Key);
				result.AppendFormat("<td class=\"local\">{0}</td>", hit.Value);
				result.AppendFormat("<td colspan=2 class=\"local\">0</td>");
				result.AppendFormat("</tr>");

				success = false;
			}

			result.AppendFormat("</table>");
			if (!success)
			{
				string actualQueryInfo;
				if (!TestingState.IsRunningOnDAT)
				{
					actualQueryInfo = "<br>To enable a csv file containing the actual queries for a specific table populate tablesToCollectQueriesFor with the tables you want to monitor";
					if (hits.Any(x => x.Value.Any(v => v.Value.Queries.Count > 0)))
					{
						var queryHitDetails = GetQueryHitDetails(hits.SelectMany(x => x.Value.Where(v => v.Value.Queries.Any()).SelectMany(v => v.Value.Queries.Select(q => (x.Key, q.Key, q.Value)))), GetTestFilesPath(), stackTraceToIgnore);
						actualQueryInfo = $@"<br>Queries data can be found here:<br><a href=""{queryHitDetails}"">{queryHitDetails}</a>";
					}
				}
				else
				{
					actualQueryInfo = "<br>Running this test locally can produce a csv file containing the actual queries found.";
				}
				result.Append(actualQueryInfo);
			}
			HtmlAssert(result.ToString(), success);
		}

		// Otherwise every hit test ever will have these 
		static HashSet<string> TablesToAlwaysIgnore()
		{
			return new HashSet<string>()
			{
				ProcessFieldChangeRuleSchema.Constants.TableName,
				ProcessFieldChangeRuleFieldSchema.Constants.TableName
			};
		}

		static bool wasAssertDbHitsCalled;

		public static void ResetWasAssertDbHitsCalled()
		{
			wasAssertDbHitsCalled = false;
		}

		public static void SetWasAssertDbHitsCalled()
		{
			wasAssertDbHitsCalled = true;
		}

		public static bool GetWasAssertDbHitsCalled()
		{
			return wasAssertDbHitsCalled;
		}

		static void RemoveStartingHits(HitCountsByTable actualHitsByTable, HitCountsByTable startingHits)
		{
			var tableNames = actualHitsByTable.Keys.ToArray();

			foreach (var tableName in tableNames)
			{
				var actualHitsForTable = actualHitsByTable[tableName];

				if (startingHits.TryGetValue(tableName, out var startingHitsByFactory))
				{
					var factoryIds = actualHitsForTable.Keys.ToArray();

					foreach (var factoryId in factoryIds)
					{
						if (startingHitsByFactory.TryGetValue(factoryId, out var startingHitsForFactory))
						{
							actualHitsForTable[factoryId].Hit -= startingHitsForFactory.Hit;
						}
					}
				}
			}
		}

		static IEnumerable<BusinessObjectFactory> GetMatchingFactories(Predicate<BusinessObjectFactory> includeFactoryPredicate)
		{
			return PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories().Where(f => includeFactoryPredicate == null || includeFactoryPredicate(f));
		}

		static HitCountsByTable GetTableHits(IEnumerable<BusinessObjectFactory> factories)
		{
			var tableHits = new HitCountsByTable();

			foreach (var factory in factories)
			{
				TableHitCount[] selects;

				using (factory.ThreadSentry.IsOwner ? null : ((ThreadSentry)factory.ThreadSentry).ForcefullyBorrowThreadOwnership_ForTest())
				{
					selects = factory.TableSelects;
				}

				foreach (var tableHitCount in selects)
				{
					if (!tableHits.TryGetValue(tableHitCount.TableName, out HitCountsByFactory hitsForFactory))
					{
						hitsForFactory = new HitCountsByFactory();
						tableHits[tableHitCount.TableName] = hitsForFactory;
					}
					var hitData = new HitData() { Hit = tableHitCount.Value };
#if DEBUG
					if (tableHitCount.Queries?.Any() ?? false)
					{
						hitData.Queries.Add(factory.NameForDebugging, tableHitCount.Queries);
					}
#endif
					hitsForFactory[factory.NameForDebugging] = hitData;
				}
			}

			return tableHits;
		}

		static void EnsureFactoriesAreNamed(IEnumerable<BusinessObjectFactory> factories)
		{
			foreach (var factory in factories)
			{
				var factoryInstanceSuffix = string.Concat(" ", factory._Instance.ToString());
				var factoryNameForDebugging = factory.NameForDebugging;

				if (string.IsNullOrEmpty(factory.NameForDebugging))
				{
					factory.NameForDebugging = string.Concat("Unnamed", factoryInstanceSuffix);
				}
				else if (!factoryNameForDebugging.EndsWith(factoryInstanceSuffix))
				{
					factory.NameForDebugging = string.Concat(factoryNameForDebugging, factoryInstanceSuffix);
				}
			}
		}

		class HitCountsByTable : Dictionary<string, HitCountsByFactory>
		{
		}

		class HitCountsByFactory : Dictionary<string, HitData>
		{
		}

		class HitData
		{
			public int Hit;
#if DEBUG
			public Dictionary<string, IEnumerable<TableHitQuery>> Queries => queries ?? (queries = new Dictionary<string, IEnumerable<TableHitQuery>>());
			Dictionary<string, IEnumerable<TableHitQuery>> queries;
#endif
		}

		#endregion

		#region AssertDbHits

		public static void AssertDbHits(Dictionary<string, int> expectedHitCounts, BusinessObjectFactory factory, bool ignoredNotSpecifiedUnlessGreaterThan5Hits = false, bool ignoreHitsFromTablesCachedInUberFactory = false)
		{
			AssertDbHits("", expectedHitCounts, factory, ignoredNotSpecifiedUnlessGreaterThan5Hits, ignoreHitsFromTablesCachedInUberFactory);
		}

		public static void AssertDbHits(string message, Dictionary<string, int> expectedHitCounts, BusinessObjectFactory factory, bool ignoredNotSpecifiedUnlessGreaterThan5Hits = false, bool ignoreHitsFromTablesCachedInUberFactory = false)
		{
			AssertDbHits(message, (IEnumerable<KeyValuePair<string, int>>)expectedHitCounts, factory, ignoredNotSpecifiedUnlessGreaterThan5Hits, ignoreHitsFromTablesCachedInUberFactory);
		}

		public static void AssertDbHits(string message, IEnumerable<KeyValuePair<string, int>> expectedHitCounts, BusinessObjectFactory factory, bool ignoredNotSpecifiedUnlessGreaterThan5Hits = false, bool ignoreHitsFromTablesCachedInUberFactory = false)
		{
			AssertDbHitsCore(message, expectedHitCounts, (expected, actual) => actual == expected, factory, ignoredNotSpecifiedUnlessGreaterThan5Hits, ignoreHitsFromTablesCachedInUberFactory);
		}

		public static void AssertMaxDbHits(Dictionary<string, int> expectedMaxHitCounts, BusinessObjectFactory factory, bool ignoredNotSpecifiedUnlessGreaterThan5Hits = false, bool ignoreHitsFromTablesCachedInUberFactory = false)
		{
			AssertMaxDbHits("", expectedMaxHitCounts, factory, ignoredNotSpecifiedUnlessGreaterThan5Hits, ignoreHitsFromTablesCachedInUberFactory);
		}

		public static void AssertDbHits(IEnumerable<KeyValuePair<string, int>> expectedHitCounts, BusinessObjectFactory factory, bool ignoredNotSpecifiedUnlessGreaterThan5Hits = false, bool ignoreHitsFromTablesCachedInUberFactory = false)
		{
			AssertDbHits("", expectedHitCounts, factory, ignoredNotSpecifiedUnlessGreaterThan5Hits, ignoreHitsFromTablesCachedInUberFactory);
		}

		public static void AssertMaxDbHits(string message, Dictionary<string, int> expectedMaxHitCounts, BusinessObjectFactory factory, bool ignoredNotSpecifiedUnlessGreaterThan5Hits = false, bool ignoreHitsFromTablesCachedInUberFactory = false)
		{
			AssertMaxDbHits(message, (IEnumerable<KeyValuePair<string, int>>)expectedMaxHitCounts, factory, ignoredNotSpecifiedUnlessGreaterThan5Hits, ignoreHitsFromTablesCachedInUberFactory);
		}

		public static void AssertMaxDbHits(IEnumerable<KeyValuePair<string, int>> expectedMaxHitCounts, BusinessObjectFactory factory, bool ignoredNotSpecifiedUnlessGreaterThan5Hits = false, bool ignoreHitsFromTablesCachedInUberFactory = false)
		{
			AssertMaxDbHits("", expectedMaxHitCounts, factory, ignoredNotSpecifiedUnlessGreaterThan5Hits, ignoreHitsFromTablesCachedInUberFactory);
		}

		public static void AssertMaxDbHits(string message, IEnumerable<KeyValuePair<string, int>> expectedMaxHitCounts, BusinessObjectFactory factory, bool ignoredNotSpecifiedUnlessGreaterThan5Hits = false, bool ignoreHitsFromTablesCachedInUberFactory = false)
		{
			AssertDbHitsCore(message, expectedMaxHitCounts, (expectedMax, actual) => actual <= expectedMax, factory, ignoredNotSpecifiedUnlessGreaterThan5Hits, ignoreHitsFromTablesCachedInUberFactory);
		}

		static void AssertDbHitsCore(string message, IEnumerable<KeyValuePair<string, int>> expectedHitCounts, AssertionCheck<int> assertionCheck, BusinessObjectFactory factory, bool ignoredNotSpecifiedUnlessGreaterThan5Hits = false, bool ignoreHitsFromTablesCachedInUberFactory = false)
		{
			wasAssertDbHitsCalled = true;

			var builder = new StringBuilder();
			var hitsCopy = expectedHitCounts.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
			var shouldNotifyAboutQueryCaptureFunctionality = false;

			foreach (var tableHitCount in factory.TableSelects.OrderBy(x => x.TableName))
			{
				int expectedHitCount;

				if (!hitsCopy.TryGetValue(tableHitCount.TableName, out expectedHitCount) && ignoredNotSpecifiedUnlessGreaterThan5Hits && tableHitCount.Value < 6)
				{
					continue;
				}

				if (!assertionCheck.Invoke(expectedHitCount, tableHitCount.Value) && !(ignoreHitsFromTablesCachedInUberFactory && IsCachedInUberFactory(tableHitCount.TableName)) && !TablesToAlwaysIgnore().Contains(tableHitCount.TableName))
				{
					AppendHitCountMismatchError(builder, tableHitCount.TableName, tableHitCount.Value, expectedHitCount, ref shouldNotifyAboutQueryCaptureFunctionality, tableHitCount.Queries);
				}

				hitsCopy.Remove(tableHitCount.TableName);
			}

			var missingHits = expectedHitCounts.Where(hit => !factory.TableSelects.Any(x => x.TableName == hit.Key) && hit.Value > 0);
			foreach (var kvp in missingHits.Where(h => !assertionCheck.Invoke(h.Value, 0)))
			{
				AppendHitCountMismatchError(builder, kvp.Key, 0, kvp.Value, ref shouldNotifyAboutQueryCaptureFunctionality);
			}

			if (shouldNotifyAboutQueryCaptureFunctionality)
			{
				builder.Append(
$@"<br />Use {nameof(BusinessObjectFactory)}.{nameof(BusinessObjectFactory.EnableTableHitQueryCollection)}, or {nameof(AssertDbHitsWithUsefulQueryInformation)} to include a full listing of the actual queries and stacktraces for specific factories and the tables passed in with the expectedHitCounts parameter.
To use and get information from AssertDbHitsWithUsefulQueryInformation you must wrap the method body with a using statement and the disposable.

For example, the following code:
<code>
var factory = new BusinessObjectFactory();
// Code that does something with it
AssertDbHits(factory, hits)
</code>

Would need to be made into:
<code>
var factory = new BusinessObjectFactory();
using (AssertDbHitsWithUsefulQueryInformation(hits, factory))
{{
&nbsp;&nbsp;// Code that does something with it
}}
</code>

If there is still no extra information, make sure that the table you want to see is a part of the DbHit assertions list.
".Replace(Environment.NewLine, "<br />"));
			}

			AssertionCount++;

			if (builder.Length > 0)
			{
				if (!string.IsNullOrEmpty(message))
				{
					builder.Insert(0, message + "<br />");
				}

				HtmlFail(builder.ToString());
			}
		}

		static bool IsCachedInUberFactory(string tableName)
		{
			return RowFactory.IsCachedTable(tableName);
		}

		static void AppendHitCountMismatchError(StringBuilder builder, string tableName, int actualHitCount, int expectedHitCount, ref bool shouldNotifyAboutQueryCaptureFunctionality, IEnumerable<TableHitQuery> queries = null)
		{
			builder.Append("<br /><strong>");
			builder.Append(tableName);
			builder.Append("</strong>");
			builder.Append(" database hit count (round trips) was expected to be ");
			builder.Append(HtmlFormatGoodValue(expectedHitCount));
			builder.Append(" but was ");
			builder.Append(HtmlFormatBadValue(actualHitCount));

			if (queries != null && queries.Any())
			{
				builder.Append(GetNiceTableStyleCss("Left"));
				builder.Append("<br />Hits on this table originated from the below queries:");
				builder.Append(@"<br /><table class=""local""><tr><th class=""local"">Query Text</th><th class=""local"">Stacktrace</th><tr>");

				void AddCellWithTextSplitByLine(string cellText, Predicate<string> continueWhenTrue = null, Predicate<string> breakWhenTrue = null)
				{
					builder.Append(@"<td class=""local"">");

					var shouldAppendText = continueWhenTrue == null;

					foreach (var line in cellText.SplitByLine())
					{
						if (!shouldAppendText && !(shouldAppendText = continueWhenTrue(line)))
						{
							continue;
						}

						if (breakWhenTrue != null && breakWhenTrue(line))
						{
							break;
						}

						builder.Append(line);
						builder.Append("<br />");
					}

					builder.Append("</td>");
				}

				var startOfUsefulStacktraceText = nameof(TableHitCounter);
				var endOfUsefulStacktraceText = $"{nameof(TestCase)}.{nameof(TestCase.RunBare)}()";

				foreach (var query in queries)
				{
					builder.Append("<tr>");

					AddCellWithTextSplitByLine(query.Query);
					AddCellWithTextSplitByLine(query.StackTrace, line => line.Contains(startOfUsefulStacktraceText), line => line.Contains(endOfUsefulStacktraceText));

					builder.Append("</tr>");
				}

				builder.Append("</table>");
			}
			else if (actualHitCount > 0)
			{
				shouldNotifyAboutQueryCaptureFunctionality = true;
			}
		}

		#endregion

		#region AssertDbHitsWithUsefulQueryInformation

		/// <summary>
		/// Calls AssertDbHits upon disposal of the return value. In between calling this function and this disposal, enables collection of queries and stacktraces on the tables included in <paramref name="expectedHitCounts"/>.
		/// </summary>
		/// <param name="expectedHitCounts">The tables expected to be hit between calling this function and disposal of its return value.</param>
		/// <param name="factory">The factory whose db hits will be tracked.</param>
		/// <param name="ignoredNotSpecifiedUnlessGreaterThan5Hits">Instructs the test to ignore hits on tables not included in <paramref name="expectedHitCounts"/> unless the table is hit more than 5 times.</param>
		/// <param name="ignoreHitsFromTablesCachedInUberFactory">Instructs the test to ignore hits on tables that are cached in the uber factory.</param>
		/// <returns></returns>
		public static IDisposable AssertDbHitsWithUsefulQueryInformation(IDictionary<string, int> expectedHitCounts, BusinessObjectFactory factory, bool ignoredNotSpecifiedUnlessGreaterThan5Hits = false, bool ignoreHitsFromTablesCachedInUberFactory = false)
		{
			var queryTrackerDisposable = factory.EnableTableHitQueryCollection(expectedHitCounts.Keys.ToArray());

			return new DisposableAction(() =>
			{
				try
				{
					AssertDbHits(expectedHitCounts, factory, ignoredNotSpecifiedUnlessGreaterThan5Hits, ignoreHitsFromTablesCachedInUberFactory);
				}
				finally
				{
					queryTrackerDisposable.Dispose();
				}
			});
		}

		#endregion

		#region More AssertMaxDbHits

		public static void AssertMaxDbHits(int max, BusinessObjectFactory factory)
		{
			AssertMaxDbHits("", max, factory.TableSelects);
		}

		public static void AssertMaxDbHits(string message, int max, BusinessObjectFactory factory)
		{
			AssertMaxDbHits(message, max, factory.TableSelects);
		}

		public static void AssertMaxDbHits(int max, TableHitCount[] hits)
		{
			AssertMaxDbHits("", max, hits);
		}

		public static void AssertMaxDbHits(string message, int max, TableHitCount[] hits)
		{
			int totalHits = 0;

			StringBuilder builder = new StringBuilder(message);
			builder.AppendLine();
			builder.AppendLine();

			Comparison<TableHitCount> comparison = (TableHitCount x, TableHitCount y) =>
			{
				int result = y.Value.CompareTo(x.Value);
				if (result == 0)
				{
					result = x.TableName.CompareTo(y.TableName);
				}

				return result;
			};

			Array.Sort(hits, comparison);

			foreach (TableHitCount hit in hits.Where(x => !TablesToAlwaysIgnore().Contains(x.TableName)))
			{
				totalHits += hit.Value;
				builder.AppendFormat("{0}: {1}", hit.TableName, hit.Value);
				builder.AppendLine();
			}

			builder.AppendLine();
			builder.AppendFormat("Hits: {0}/{1}", totalHits, max);
			builder.AppendLine();

			Assert(builder.ToString(), totalHits <= max);
		}

		#endregion

		#region AssertTableHitCount

		protected void AssertTableHitCount(string message, int expectedHitCount, string tableName)
		{
			AssertTableHitCount(message, expectedHitCount, tableName, Factory);
		}

		protected void AssertTableHitCount(int expectedHitCount, string tableName)
		{
			AssertTableHitCount(null, expectedHitCount, tableName, Factory);
		}

		protected void AssertTableHitCount(int expectedHitCount, string tableName, BusinessObjectFactory factory)
		{
			AssertTableHitCount(null, expectedHitCount, tableName, factory);
		}

		protected void AssertTableHitCount(string message, int expectedHitCount, string tableName, BusinessObjectFactory factory)
		{
			var hitCount = factory.GetTableHitCount(tableName);
			AssertEquals(message ?? "Expected hit count for table " + tableName, expectedHitCount, hitCount);
		}

		#endregion

		#region AssertHasDefault/AssertNoDefault(s)

		public static void AssertHasDefault(IFilterBusinessObjectDefaultsProvider provider, string filterName, string propertyName, IZType expectedValue)
		{
			AssertHasDefault("", provider, filterName, propertyName, expectedValue);
		}
		public static void AssertHasDefault(string message, IFilterBusinessObjectDefaultsProvider provider, string filterName, string propertyName, IZType expectedValue)
		{
			ZString key = Key(filterName, propertyName);

			if (provider.FilterBusinessObjectDefaults.ContainsDefaultFor(key))
			{
				AssertEquals(message, expectedValue, provider.FilterBusinessObjectDefaults[key].Value);
			}
			else
			{
				StringBuilder builder = new StringBuilder(message);
				builder.AppendLine(message);
				builder.AppendFormat("No default defined for '{0}'\r\n\r\n", key);
				builder.AppendLine("Did however manage to find:");

				foreach (FilterBusinessObjectDefault def in provider.FilterBusinessObjectDefaults)
				{
					builder.AppendLine(DefaultAsString(def));
				}

				Fail(builder.ToString());
			}
		}

		public static void AssertNoDefaults(IFilterBusinessObjectDefaultsProvider provider)
		{
			AssertNoDefaults("should not have added any filters", provider);
		}

		public static void AssertNoDefaults(string message, IFilterBusinessObjectDefaultsProvider defaultsProvider)
		{
			StringBuilder builder = new StringBuilder();

			foreach (FilterBusinessObjectDefault def in defaultsProvider.FilterBusinessObjectDefaults)
			{
				builder.AppendLine(DefaultAsString(def));
			}

			AssertEquals(message, "", builder.ToString());
		}
		public static void AssertNoDefaults(IFilterBusinessObjectDefaultsProvider provider, string filterName)
		{
			AssertNoDefaults(string.Format("Not expecting to find any defaults for the '{0}' filter", filterName), provider, filterName);
		}
		public static void AssertNoDefaults(ZString message, IFilterBusinessObjectDefaultsProvider provider, string filterName)
		{
			StringBuilder builder = new StringBuilder();

			foreach (FilterBusinessObjectDefault def in provider.FilterBusinessObjectDefaults)
			{
				if (def.FilterName == filterName)
				{
					builder.AppendLine(DefaultAsString(def));
				}
			}

			AssertEquals(message, "", builder.ToString());
		}

		public static void AssertNoDefault(IFilterBusinessObjectDefaultsProvider provider, string filterName, string propertyName)
		{
			AssertNoDefault(string.Format("Not expecting to find a default for '{0}'", Key(filterName, propertyName)), provider, filterName, propertyName);
		}

		public static void AssertNoDefault(string message, IFilterBusinessObjectDefaultsProvider provider, string filterName, string propertyName)
		{
			AssertEquals(message, "", DefaultAsString(provider, filterName, propertyName));
		}

		static string DefaultAsString(IFilterBusinessObjectDefaultsProvider provider, string filterName, string propertyName)
		{
			string key = Key(filterName, propertyName);

			if (provider.FilterBusinessObjectDefaults.ContainsDefaultFor(key))
			{
				return DefaultAsString(provider.FilterBusinessObjectDefaults[key]);
			}
			else
			{
				return string.Empty;
			}
		}
		static string DefaultAsString(FilterBusinessObjectDefault def)
		{
			return string.Format("'{0}' -> \"{1}\"", def.Key, def.Value);
		}
		static string Key(string filterName, string propertyName)
		{
			return filterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + propertyName;
		}

		#endregion

		#region Date Assertions

		public void AssertZDatesWithin5Minutes(ZString message, ZDateTimeOffset date1, ZDateTimeOffset date2) => AssertZDatesWithin5Minutes(message, date1.ToUtcZDateTime(), date2.ToUtcZDateTime());

		public void AssertZDatesWithin5Minutes(ZString message, ZDateTime date1, ZDateTime date2)
		{
			if (!date1.IsEmpty && !date2.IsEmpty)
			{
				Assert(message + " should be equal (accurate to within 5 minutes)", date1.AddMinutes(-5) < date2);
				Assert(message + " should be equal (accurate to within 5 minutes)", date1.AddMinutes(5) > date2);
			}
			else
			{
				AssertEquals(ZDateTime.Empty, date1);
				AssertEquals(ZDateTime.Empty, date2);
			}
		}

		#endregion

		#region AssertContainsExactElementsInAnyOrder for String/ZString

		/// <summary>
		/// Asserts that two collections contain the same elements ignoring order. ZStrings will be converted to strings for comparison.
		/// </summary>
		/// <param name="message">Custom fail assertion message.</param>
		/// <param name="expected">Expected collection of objects.</param>
		/// <param name="actual">Actual collection of objects.</param>
		public static void AssertContainsExactElementsInAnyOrder(string message, IEnumerable<string> expected, IEnumerable<ZString> actual)
		{
			AssertContainsExactElementsInAnyOrder(message, expected, actual.Select(x => x.ToString()));
		}

		/// <summary>
		/// Asserts that two collections contain the same elements ignoring order. ZStrings will be converted to strings for comparison.
		/// </summary>
		/// <param name="expected">Expected collection of objects.</param>
		/// <param name="actual">Actual collection of objects.</param>
		public static void AssertContainsExactElementsInAnyOrder(IEnumerable<string> expected, IEnumerable<ZString> actual)
		{
			AssertContainsExactElementsInAnyOrder(null, expected, actual);
		}

		/// <summary>
		/// Asserts that two collections contain the same elements ignoring order. ZStrings will be converted to strings for comparison.
		/// </summary>
		/// <param name="message">Custom fail assertion message.</param>
		/// <param name="expected">Expected collection of objects.</param>
		/// <param name="actual">Actual collection of objects.</param>
		public static void AssertContainsExactElementsInAnyOrder(string message, IEnumerable<ZString> expected, IEnumerable<string> actual)
		{
			AssertContainsExactElementsInAnyOrder(message, expected.Select(x => x.ToString()), actual);
		}

		/// <summary>
		/// Asserts that two collections contain the same elements ignoring order. ZStrings will be converted to strings for comparison.
		/// </summary>
		/// <param name="expected">Expected collection of objects.</param>
		/// <param name="actual">Actual collection of objects.</param>
		public static void AssertContainsExactElementsInAnyOrder(IEnumerable<ZString> expected, IEnumerable<string> actual)
		{
			AssertContainsExactElementsInAnyOrder(null, expected, actual);
		}

		#endregion

		#region AssertContainsExactElementsInExactOrder for String/ZString

		/// <summary>
		/// Asserts that two collections contain the same elements in the same order. ZStrings will be converted to strings for comparison.
		/// </summary>
		/// <param name="message">Custom fail assertion message.</param>
		/// <param name="expected">Expected collection of objects.</param>
		/// <param name="actual">Actual collection of objects.</param>
		public static void AssertContainsExactElementsInExactOrder(string message, IEnumerable<string> expected, IEnumerable<ZString> actual)
		{
			AssertContainsExactElementsInExactOrder(message, expected, actual.Select(x => x.ToString()));
		}

		/// <summary>
		/// Asserts that two collections contain the same elements in the same order. ZStrings will be converted to strings for comparison.
		/// </summary>
		/// <param name="expected">Expected collection of objects.</param>
		/// <param name="actual">Actual collection of objects.</param>
		public static void AssertContainsExactElementsInExactOrder(IEnumerable<string> expected, IEnumerable<ZString> actual)
		{
			AssertContainsExactElementsInExactOrder(null, expected, actual);
		}

		/// <summary>
		/// Asserts that two collections contain the same elements in the same order. ZStrings will be converted to strings for comparison.
		/// </summary>
		/// <param name="message">Custom fail assertion message.</param>
		/// <param name="expected">Expected collection of objects.</param>
		/// <param name="actual">Actual collection of objects.</param>
		public static void AssertContainsExactElementsInExactOrder(string message, IEnumerable<ZString> expected, IEnumerable<string> actual)
		{
			AssertContainsExactElementsInExactOrder(message, expected.Select(x => x.ToString()), actual);
		}

		/// <summary>
		/// Asserts that two collections contain the same elements in the same order. ZStrings will be converted to strings for comparison.
		/// </summary>
		/// <param name="expected">Expected collection of objects.</param>
		/// <param name="actual">Actual collection of objects.</param>
		public static void AssertContainsExactElementsInExactOrder(IEnumerable<ZString> expected, IEnumerable<string> actual)
		{
			AssertContainsExactElementsInExactOrder(null, expected, actual);
		}

		#endregion

		#region AssertCollectionContains

		public static void AssertCollectionContains(ZQuery filter, BusinessObjectCollection collection)
		{
			AssertCollectionContains(string.Format("{0} could not find object using filter ({1})", collection.HumanReadableName, filter.LiteralTextADO), filter, collection);
		}

		public static void AssertCollectionContains(string message, ZQuery filter, BusinessObjectCollection collection)
		{
			AssertEquals(message, true, collection.Find(filter).Length > 0);
		}

		#endregion

		#region AssertBusinessObjectTypesNotCreatedOrLoaded

		public void AssertBusinessObjectTypesNotCreatedOrLoaded(BusinessObjectFactory factory, params Type[] businessObjectTypesToDisallowLoad)
		{
			Hashtable typesNotAllowed = new Hashtable();
			foreach (BusinessObject bizO in ((IBusinessObjectFactoryInternals)factory).AllBusinessObjects)
			{
				foreach (Type businessObjectTypeToDisallowLoad in businessObjectTypesToDisallowLoad)
				{
					if (businessObjectTypeToDisallowLoad.IsInstanceOfType(bizO))
					{
						typesNotAllowed[businessObjectTypeToDisallowLoad] = null;
					}
				}
			}
			if (typesNotAllowed.Count > 0)
			{
				string message = "The follow business object types were created or loaded unnecessarily which may be a problem for performance:\r\n\r\n";
				foreach (Type type in typesNotAllowed.Keys)
				{
					message += type.FullName + "\r\n";
				}
				Fail(message);
			}
			Assert(true);
		}

		#endregion

		#region AssertPersistentPropertiesHitCount

		public void AssertPersistentPropertiesHitCount(string message, int expectedHitCount, Action codeToTest)
		{
			AssertPersistentPropertiesHitCount(message, null, expectedHitCount, codeToTest);
		}

		public void AssertPersistentPropertiesHitCount(string message, Type tableType, int expectedHitCount, Action codeToTest, BusinessObjectFactory factoryToTest = null)
		{
			var typeHitCount = GetPropertyHitTableResults(tableType, codeToTest, factoryToTest);
			var messageBuilder = new ZStringBuilder(message);
			messageBuilder.Append("***************************************************************************************************");
			messageBuilder.Append("The main goal of this sort of tests is to catch code that gets exponentially slower with more data.");
			messageBuilder.Append("Therefore, if the test fails, and hit count increases significanty, then:");
			messageBuilder.Append("1. Record actual hit count.");
			messageBuilder.Append("2. Modify test to create 2x number of records.");
			messageBuilder.Append("3. Compare the new actual hit count with old. It should increase less than or exactly 2x.");
			messageBuilder.Append("***************************************************************************************************");
			messageBuilder.Append("");

			var actualHitCount = 0;
			foreach (var pair in typeHitCount.OrderByDescending(p => p.Value.Sum(c => c.Value)))
			{
				messageBuilder.Append($"Properties hit count for the {pair.Key.Name} ({pair.Value.Sum(p => p.Value):0,0}):");

				foreach (var columnPair in pair.Value.OrderByDescending(col => col.Value))
				{
					actualHitCount += columnPair.Value;
					messageBuilder.Append($"   -   {columnPair.Key.ColumnName} ({columnPair.Value:0,0})");
				}
				messageBuilder.Append("");
			}

			AssertEquals(messageBuilder.ToStringWithNewLineBetweenAppends(), expectedHitCount, actualHitCount);
		}

		Dictionary<Type, Dictionary<DataColumn, int>> GetPropertyHitTableResults(Type tableType, Action codeToTest, BusinessObjectFactory factoryToTest = null)
		{
			var factoryToUse = factoryToTest ?? Factory;

			var typeHitCount = new Dictionary<Type, Dictionary<DataColumn, int>>();
			AccessingPersistentValueForTestingDelegate hitCounter = (bizO, column) =>
			{
				var bizOType = bizO.GetType();
				if (tableType == null || bizOType == tableType)
				{
					Dictionary<DataColumn, int> columnsHitCount;
					if (!typeHitCount.TryGetValue(bizOType, out columnsHitCount))
					{
						columnsHitCount = new Dictionary<DataColumn, int>();
						typeHitCount.Add(bizOType, columnsHitCount);
					}

					if (!columnsHitCount.ContainsKey(column))
					{
						columnsHitCount.Add(column, 0);
					}
					columnsHitCount[column]++;
				}
			};
			factoryToUse.AccessingPersistentValueForTesting += hitCounter;

			try
			{
				codeToTest();
			}
			finally
			{
				factoryToUse.AccessingPersistentValueForTesting -= hitCounter;
			}

			return typeHitCount;
		}

		public int GetPersistentPropertiesHitCount(Action codeToTest, BusinessObjectFactory factoryToTest = null) => GetPersistentPropertiesHitCount(null, codeToTest, factoryToTest);

		public int GetPersistentPropertiesHitCount(Type tableType, Action codeToTest, BusinessObjectFactory factoryToTest = null)
		{
			var typeHitCount = GetPropertyHitTableResults(tableType, codeToTest, factoryToTest);
			return typeHitCount.Sum(p => p.Value.Sum(c => c.Value));
		}

		#endregion

		#region AssertQueryResults

		protected void AssertQueryResults<T>(ZQuery query, params (T BO, bool MatchesQuery)[] expectedResults) where T : class, IIdentified
		{
			foreach (var expectedResult in expectedResults)
			{
				if (!(expectedResult.BO is BusinessObject))
				{
					throw new InvalidOperationException(FormattableString.Invariant(
						$"{nameof(AssertQueryResults)} is intended to be used with {nameof(BusinessObject)} in the {nameof(expectedResult)}."
					));
				}
			}

			SchemaColumn pkColumn = GetPKColumnFromType(typeof(T));
			var fullQuery = new ZQuery(pkColumn, SQLComparisonOperator.Equal, expectedResults.Select(r => r.BO.Identifier).ToArray());
			fullQuery.AddToFilter(query);
			var actualResults = Factory.Load<T>(fullQuery);

			string[] expectedResultText = expectedResults
				.Where(r => r.MatchesQuery)
				.Select(r => $"{(r.BO as BusinessObject)?.HumanReadableShortcutName} (PK={r.BO.Identifier})")
				.ToArray();

			string[] actualResultText = actualResults
				.Select(r => $"{(r as BusinessObject)?.HumanReadableShortcutName} (PK={r.Identifier})")
				.ToArray();

			var queryText = $"Query:\r\n{query.LiteralTextSqlFormatted.Replace("\t", "    ")}";
			AssertContainsExactElementsInAnyOrder(queryText, expectedResultText, actualResultText);
		}

		#endregion

		#region AssertCodeDescriptionPairList

		public void AssertCodeDescriptionPairList(ICodeDescriptionPairList codeList, params (string code, string description)[] codes)
			=> AssertCodeDescriptionPairList(string.Empty, codeList, codes);

		public void AssertCodeDescriptionPairList(string message, ICodeDescriptionPairList codeList, params (string code, string description)[] expectedCodes)
		{
			Argument.NotNull(codeList, nameof(codeList));
			Argument.NotNull(expectedCodes, nameof(expectedCodes));

			if (!string.IsNullOrEmpty(message))
			{
				message += "<br>";
			}

			HtmlAssertEquals($"{message}Expected codes should be unique", expectedCodes.Length, expectedCodes.Distinct().Count());

			var actualCodes = codeList.Cast<ICodeDescription>().Select(x => x.Code);
			AssertContainsExactElementsInAnyOrder($"{message}Codes", expectedCodes.Select(x => x.code), actualCodes);
			foreach (var (code, description) in expectedCodes)
			{
				HtmlAssertEquals($"{message}Code '{code}'", description, codeList.GetDescriptionFromCode(code));
			}
		}

		#endregion

		#region AssertCached

		public void AssertCached<T>(Func<T> selector) where T : class => AssertCached("Cached", selector);

		public void AssertCached<T>(string message, Func<T> selector) where T : class => AssertSame(message, selector(), selector());

		#endregion

		#region AssertArgumentExceptionThrown

		protected ArgumentException AssertArgumentExceptionThrown(string paramName, AnonymousMethod codeToRun, string exceptionMessage = null) =>
			AssertArgumentExceptionThrown<ArgumentException>(string.Empty, paramName, codeToRun, exceptionMessage);

		protected ArgumentException AssertArgumentExceptionThrown(string assertMessage, string paramName, AnonymousMethod codeToRun, string exceptionMessage = null) =>
			AssertArgumentExceptionThrown<ArgumentException>(assertMessage, paramName, codeToRun, exceptionMessage);

		protected TArgumentException AssertArgumentExceptionThrown<TArgumentException>(string paramName, AnonymousMethod codeToRun, string exceptionMessage = null)
			where TArgumentException : ArgumentException =>
			AssertArgumentExceptionThrown<TArgumentException>(string.Empty, paramName, codeToRun, exceptionMessage);

		protected TArgumentException AssertArgumentExceptionThrown<TArgumentException>(string assertMessage, string paramName, AnonymousMethod codeToRun, string exceptionMessage = null)
			where TArgumentException : ArgumentException
		{
			var exception = AssertExceptionThrown<TArgumentException>(assertMessage, codeToRun);
			var assertPrefix = string.IsNullOrEmpty(assertMessage) ? string.Empty : $"{assertMessage}: ";
			AssertEquals($"{assertPrefix}ParamName", paramName, exception.ParamName);
			if (!string.IsNullOrWhiteSpace(exceptionMessage))
			{
				AssertContains($"{assertPrefix}Message", exceptionMessage, exception.Message);
			}
			return exception;
		}

		#endregion

		#region AssertEntity

		/// <summary>
		/// Used to perform asserts on an entity.
		/// </summary>
		/// <remarks>
		/// <example>
		/// Example Test:
		/// <code>
		///   AssertEntity&lt;CusEntryLine&gt;()
		///       .HasProperty(x => x.CL_Description)
		///       .WithAttribute&lt;ReadOnlyAttribute&gt;(x => x.IsReadOnly, because: "42")
		/// </code>
		/// Assert Message:
		/// <code>
		///   The�following�list�of�failures�occurred�(2):-
		///   CusEntryLine::CL_Description should have ReadOnlyAttribute
		///   CusEntryLine::CL_Description should have ReadOnlyAttribute where x => x.IsReadOnly
		///     Because:�42
		/// </code>
		/// </example>
		/// </remarks>
		/// <typeparam name="T">The type which is being asserted</typeparam>
		public static TypeAsserter<T> AssertEntity<T>() where T : class => new ();

		public static PropertyAsserter AssertEntityProperty(ZPropertyInfo propertyInfo) => PropertyAsserter.FromPropertyInfo(propertyInfo);

		#region AssertHasDecimalPlacesAttribute

		public static void AssertHasDecimalPlacesAttribute(ZPropertyInfo propertyInfo, int expectedDecimalPlaces)
		{
			AssertHasDecimalPlacesAttribute(string.Empty, propertyInfo, expectedDecimalPlaces);
		}

		public static void AssertHasDecimalPlacesAttribute(string message, ZPropertyInfo propertyInfo, int expectedDecimalPlaces)
		{
			Argument.NotNull(propertyInfo, nameof(propertyInfo));
			var getActualDecimalPlaces = (SingleMetaDataAttribute x) => GetActualMetaDataValue(x, propertyInfo) as int?;
			AssertEntityProperty(propertyInfo)
				.WithAttribute<DecimalPlacesAttribute>(x => getActualDecimalPlaces(x).Equals(expectedDecimalPlaces), because: message);
		}

		static object GetActualMetaDataValue(SingleMetaDataAttribute attribute, ZPropertyInfo propertyInfo)
		{
			if (attribute is null)
			{
				return null;
			}
			var metaValueId = attribute.MetaDataTypeId;
			if (attribute.ProvidesMetaDataMember(metaValueId))
			{
				var metaValueMember = attribute.GetMetaDataMember(metaValueId);
				var bizOToCheck = propertyInfo.BizObj;
				var typeToCheck = bizOToCheck.GetType();
				return typeToCheck
					.GetProperty(metaValueMember, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
					?.GetValue(bizOToCheck);
			}
			if (attribute.ProvidesMetaDataValue(metaValueId))
			{
				return attribute.GetMetaDataValue(metaValueId);
			}
			return null;
		}

		#endregion

		public readonly struct TypeAsserter<T>
		{
			public PropertyAsserter HasProperty<TProp>(Expression<Func<T, TProp>> propertySelector) => PropertyAsserter.FromSelector(propertySelector);
		}

		public readonly struct PropertyAsserter
		{
			PropertyAsserter(string entityName, MemberInfo member)
			{
				this.entityName = entityName;
				this.member = member;
				listAsserter = new (() => WithAttributeCore<ListAttribute>(entityName, member));
				maxLengthAsserter = new (() => WithAttributeCore<MaxLengthAttribute>(entityName, member));
				resourceStringAsserter = new (() => WithAttributeCore<ResourceStringDataAttribute>(entityName, member));
			}

			readonly string entityName;
			readonly MemberInfo member;
			readonly Lazy<AttributeAsserter<ListAttribute>> listAsserter;
			readonly Lazy<AttributeAsserter<MaxLengthAttribute>> maxLengthAsserter;
			readonly Lazy<AttributeAsserter<ResourceStringDataAttribute>> resourceStringAsserter;

			public static PropertyAsserter FromPropertyInfo(ZPropertyInfo propertyInfo)
			{
				var entity = propertyInfo.BizObj;
				var entityType = entity.GetType();
				var entityName = entityType.Name;
				var propertyName = propertyInfo.Name;
				var property = propertyInfo.PropertyDescriptor.ComponentType.GetProperty(propertyName);
				return new PropertyAsserter(entityName, property);
			}

			public static PropertyAsserter FromSelector<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> propertySelector)
			{
				var entityType = typeof(TEntity);
				var entityName = entityType.Name;
				if (propertySelector is LambdaExpression { Body: MemberExpression memberExpression })
				{
					var declaringMember = memberExpression.Member;
					var member = entityType.GetMember(declaringMember.Name).FirstOrDefault() ?? declaringMember;
					return new PropertyAsserter(entityName, member);
				}
				HtmlFail($"Could not find property {typeof(TProperty)} within {entityType} with provided propertySelector<br />" +
					$"  Expected:  x => x.MyProperty<br />" +
					$"  But found: {propertySelector}");
				return new PropertyAsserter(entityName, null);
			}

			AttributeAsserter<TAttribute> WithAttributeCore<TAttribute>(bool inspectInherit = false, string because = null)
				where TAttribute : Attribute => WithAttributeCore<TAttribute>(entityName, member, inspectInherit, because ?? string.Empty);

			static AttributeAsserter<TAttribute> WithAttributeCore<TAttribute>(string entityName, MemberInfo member, bool inspectInherit = false, string because = null)
				where TAttribute : Attribute => new (entityName, member, inspectInherit, because ?? string.Empty);

			public PropertyAsserter WithAttribute<TAttribute>(bool inspectInherit = false, string because = null)
				where TAttribute : Attribute
			{
				_ = WithAttributeCore<TAttribute>(inspectInherit, because);
				return this;
			}

			public PropertyAsserter WithAttribute<TAttribute>(Expression<Func<TAttribute, bool>> predicate, bool inspectInherit = false, string because = null)
				where TAttribute : Attribute
			{
				_ = WithAttributeCore<TAttribute>(inspectInherit, because).Where(predicate, because);
				return this;
			}

			public PropertyAsserter WithCaption(string expectedCaption, string because = null)
			{
				_ = resourceStringAsserter.Value.Where(x => x.Caption == expectedCaption, because);
				return this;
			}

			public PropertyAsserter WithShortCaption(string expectedShortCaption, string because = null)
			{
				_ = resourceStringAsserter.Value.Where(x => x.ShortCaption == expectedShortCaption, because);
				return this;
			}

			public PropertyAsserter WithMediumCaption(string expectedMediumCaption, string because = null)
			{
				_ = resourceStringAsserter.Value.Where(x => x.MediumCaption == expectedMediumCaption, because);
				return this;
			}

			public PropertyAsserter WithFullDescription(string expectedFullDescription, string because = null)
			{
				_ = resourceStringAsserter.Value.Where(x => x.FullDescription == expectedFullDescription, because);
				return this;
			}

			public PropertyAsserter WithList(string expectedListDataSourceMember, string because = null)
			{
				_ = listAsserter.Value.Where(x => x.ListDataSourceMember == expectedListDataSourceMember, because);
				return this;
			}

			public PropertyAsserter WithMaxLength(int expectedMaxLength, string because = null)
			{
				_ = maxLengthAsserter.Value.Where(x => x.MaxLength == expectedMaxLength, because);
				return this;
			}

			public PropertyAsserter WithMaxLength(string expectedMaxLengthMember, string because = null)
			{
				_ = maxLengthAsserter.Value.Where(x => x.MaxLengthMember == expectedMaxLengthMember, because);
				return this;
			}
		}

		internal readonly struct AttributeAsserter<TAttribute>
			where TAttribute : Attribute
		{
			public AttributeAsserter(string entityName, MemberInfo member, bool inspectInherit, string because)
			{
				this.entityName = entityName;
				memberName = member.Name;
				attributes = member.GetCustomAttributes<TAttribute>(inspectInherit).ToList();
				if (!string.IsNullOrWhiteSpace(because))
				{
					because += "<br>";
				}
				HtmlAssertNotEquals($"{because}{entityName}::{memberName} should have {typeof(TAttribute).Name}", 0, attributes.Count);
			}

			readonly IReadOnlyCollection<TAttribute> attributes;
			readonly string entityName;
			readonly string memberName;

			public AttributeAsserter<TAttribute> Where(Expression<Func<TAttribute, bool>> predicate, string because = null)
			{
				if (!attributes.Any(predicate.Compile()))
				{
					var assertMessage = new ZStringBuilder()
						.Append($"{entityName}::{memberName} should have {typeof(TAttribute).Name}<br />")
						.Append($"  Where:   {HumanReadableExpression(predicate, out var actualValue)}")
						.AppendIfNotEmpty("<br />  But was: ", actualValue)
						.AppendIfNotEmpty("<br />  Because: ", because)
						.ToString();
					HtmlFail(assertMessage);
				}
				return this;
			}

			string HumanReadableExpression(Expression<Func<TAttribute, bool>> expression, out string actualValue)
			{
				if (expression is
					{
						NodeType: ExpressionType.Lambda, Body: BinaryExpression
						{
							Left: MemberExpression { Member: { MemberType: MemberTypes.Property, Name: var propertyName } },
							NodeType: ExpressionType.Equal,
							Right: MemberExpression
							{
								Member: { MemberType: MemberTypes.Field, Name: var boxedName },
								Expression: ConstantExpression { Value: var boxedObject }
							}
						}
					})
				{
					var boxedValue = boxedObject.GetType().GetField(boxedName).GetValue(boxedObject);
					if (boxedValue is string)
					{
						boxedValue = $"'{boxedValue}'";
					}
					actualValue = GetAttributeValueOrEmpty(propertyName);
					return $"{propertyName} is {boxedValue}";
				}
				actualValue = string.Empty;
				return expression.ToString();
			}

			string GetAttributeValueOrEmpty(string propertyName)
			{
				var attributeValue = attributes.Select(x => typeof(TAttribute).GetProperty(propertyName)?.GetValue(x))
					.WhereNotNull()
					.FirstOrDefault();
				return attributeValue switch
				{
					null => "NULL",
					string str => $"'{str}'",
					_ => attributeValue.ToString(),
				};
			}
		}

		#endregion

		#region Diff

		/// <summary>
		/// Compares the differences between all the properties on two BusinessObjects of the same type.
		/// </summary>
		/// <param name="a">First BusinessObject to compare.</param>
		/// <param name="b">Second BusinessObject to compare.</param>
		/// <returns>A list of the properties that are different, and their values on both BusinessObjects.</returns>
		public static string Diff(BusinessObject a, BusinessObject b)
		{
			AssertEquals("Precondition for Diff: BusinessObjects must be of the same type.", a.GetType().FullName, b.GetType().FullName);
			AssertEquals("Precondition for Diff: BusinessObjects must have the same number of properties.", a.ZPropertyInfoHash.Count, b.ZPropertyInfoHash.Count);

			// Extracting the properties into generic lists, we sort them by name.
			List<ZPropertyInfo> aInfoList = new List<ZPropertyInfo>();
			foreach (ZPropertyInfo info in a.ZPropertyInfoHash)
			{
				aInfoList.Add(info);
			}
			List<ZPropertyInfo> bInfoList = new List<ZPropertyInfo>();
			foreach (ZPropertyInfo info in b.ZPropertyInfoHash)
			{
				bInfoList.Add(info);
			}

			Comparison<ZPropertyInfo> infoComparison = delegate(ZPropertyInfo x, ZPropertyInfo y)
			{ return x.Name.CompareTo(y.Name); };
			aInfoList.Sort(infoComparison);
			bInfoList.Sort(infoComparison);

			// Iterating through all the properties on both BusinessObjects simultaneously, and finding the differences.
			IEnumerator<ZPropertyInfo> aEnumerator = aInfoList.GetEnumerator();
			IEnumerator<ZPropertyInfo> bEnumerator = bInfoList.GetEnumerator();

			StringBuilder result = new StringBuilder();

			while (aEnumerator.MoveNext() && bEnumerator.MoveNext())
			{
				ZPropertyInfo aCurrent = aEnumerator.Current;
				ZPropertyInfo bCurrent = bEnumerator.Current;

				if (aCurrent.Name != bCurrent.Name)
				{
					Fail("Diff: Properties are out of order or different.");
				}

				if (!aCurrent.Value.Equals(bCurrent.Value))
				{
					result.AppendFormat("[{0}] a: [{1}] b: [{2}]\r\n", aCurrent.Name, aCurrent.Value.ToString(), bCurrent.Value.ToString());
				}
			}

			return result.ToString();
		}

		#endregion

		#region Implementation

		BusinessObjectFactory fFactory;
		protected internal int MaxAllowedFactoryCreations;
		protected BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = NewFactory();
				}
				return fFactory;
			}
		}

		protected virtual BusinessObjectFactory NewFactory()
		{
			return new BusinessObjectFactory();
		}

		protected void ReleaseFactory()
		{
			fFactory = null;
		}

		protected override void TearDown()
		{
			base.TearDown();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = null;
			DummyDependantBusinessObject.TypeDecider.TypeForLoadOverride = null;
		}

		#endregion
	}

	#region Test Helper Classes

	public abstract class TestCaseWithDummy : TestCaseWithFactory
	{
		protected DummyBusinessObject Dummy;

		protected override void SetUp()
		{
			base.SetUp();
			Dummy = (DummyBusinessObject)Factory.New(TypeOfDummy);
			AssertNotNull("Dummy should be created!", Dummy);
		}

		protected void DeleteAllDummies()
		{
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			collection.Load();
			collection.RemoveAndDeleteAll();
		}

		protected virtual Type TypeOfDummy { get { return typeof(DummyBusinessObject); } }
	}

	internal abstract class TestCaseWithDummyForValidationTesting : TestCaseWithDummy
	{
		protected override void RunTest()
		{
			using (Dummy.SuspendValidationTesting())
			{
				base.RunTest();
			}
		}
	}

	#endregion
}

#endif
