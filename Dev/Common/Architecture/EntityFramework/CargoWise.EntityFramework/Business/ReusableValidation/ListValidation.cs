using System;
using System.Collections;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	public interface IListValidationInternals
	{
		void MessageErrorIfInvalidCode(ZPropertyInfo info, IEnumerable explicitList, IMultilingualString message, Action<string> addNotification);
	}

	public interface IExternalListValidation
	{
		bool IsValidTemplateRecordPK(ZGuid pk);
	}

	public class ListValidation : IListValidationInternals
	{
		#region Constants

		public static IMultilingualString InvalidCodeMessage
		{
			get { return ResString.GetMultilingualString("93b9b150-89fd-4a9b-9092-91a404427f4c", "You have not entered a valid code."); }
		}

		public static IMultilingualString InvalidCodeMessageError
		{
			get { return ResString.GetMultilingualString("1364e2fd-5b36-4371-b5c6-ad91c2129d61", "The code you have selected is not in the list."); }
		}

		public static IMultilingualString InactiveCodeMessage
		{
			get { return ResString.GetMultilingualString("8f10817f-8247-4f31-9cd3-861f69787df7", "This code is inactive."); }
		}

		#endregion

		#region For Generic Use

		public static IMultilingualString GetNotificationMessage(ZPropertyInfo propertyInfo)
		{
			return new PropertyInfoNotificationMessage(propertyInfo);
		}

		public static IMultilingualString GetNotificationMessage(IMultilingualString propertyName)
		{
			return new PropertyNotificationMessageMultilingual(propertyName);
		}

		public static IMultilingualString GetNotificationMessage(string propertyName)
		{
			return new PropertyNotificationMessage(propertyName);
		}

		struct PropertyNotificationMessage : IMultilingualString
		{
			public PropertyNotificationMessage(string propertyName)
			{
				this.propertyName = propertyName;
			}

			readonly string propertyName;

			string IMultilingualString.ToString()
			{
				return (!string.IsNullOrEmpty(propertyName))
								? Res.GetString("Common|Validation|InvalidPropertySelection", "Enter a valid {0}.", propertyName)
						: Res.GetString("Common|Validation|InvalidCodeSelection", "Enter a valid code.");
			}
		}

		struct PropertyInfoNotificationMessage : IMultilingualString
		{
			public PropertyInfoNotificationMessage(ZPropertyInfo propertyInfo)
			{
				this.propertyInfo = propertyInfo;
			}

			readonly ZPropertyInfo propertyInfo;

			string IMultilingualString.ToString()
			{
				return GetNotificationMessage(propertyInfo.HasHumanReadableName ? propertyInfo.HumanReadableName.ToString() : Res.GetString("0137baec-ae22-441c-943c-6c940a5eec63", "selection")).ToString();
			}
		}

		struct PropertyNotificationMessageMultilingual : IMultilingualString
		{
			public PropertyNotificationMessageMultilingual(IMultilingualString propertyName)
			{
				this.propertyName = propertyName;
			}

			readonly IMultilingualString propertyName;

			string IMultilingualString.ToString()
			{
				return GetNotificationMessage(propertyName.ToString()).ToString();
			}
		}

		public static void IfInvalidCode(INotificationType notificationType, ZPropertyInfo info, string message)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(message, "message");

			if (!info.Value.IsEmpty)
			{
				var validator = new BusinessObjectCollectionValidator();
				if (validator.IsValidCode(info, FindList(info)) == ValidationResult.NotFound)
				{
					info.AddNotification(notificationType, message);
				}
			}
		}

		public static void IfInvalidCode(INotificationType notificationType, ZPropertyInfo info, IBusinessObjectCollection list, string message)
		{
			var validator = new BusinessObjectCollectionValidator();
			if (!info.Value.IsEmpty && validator.IsValidCode(info, list) == ValidationResult.NotFound)
			{
				info.AddNotification(notificationType, message);
			}
		}

		/// <summary>
		/// Sets a notification if the code does not exist in the list.
		/// </summary>
		/// <param name="notificationType">The Notification Type to add.</param>
		/// <param name="info">The property to validate.</param>
		/// <param name="list">The list to check.</param>
		/// <param name="message">The message to add when code is invalid.</param>
		public static void IfInvalidCode(INotificationType notificationType, ZPropertyInfo info, ICodeDescriptionPairList list, string message)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(list, "list");

			var validator = new CodeDescriptionPairListValidator();
			if (!info.Value.IsEmpty && validator.IsValidCode(info, list) == ValidationResult.NotFound)
			{
				info.AddNotification(notificationType, message);
			}
		}

#if DEBUG // for unit tests only
		public static string InvalidCodeError
		{
			get { return Res.GetString("bd2a6ddf-d41f-4150-be3f-2660e72c5676", "Enter a valid") + " "; }
		}
#endif

		#endregion

		#region Warnings

		#region WarnIfInvalidPK

		/// <summary>
		/// Sets a warning if the PK does not exist in the list.
		/// Will try to find list by inspecting property metadata.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <exception cref="Exception">If the list can not be found</exception>
		public static void WarnIfInvalidPK(ZPropertyInfo info)
		{
			Argument.NotNull(info, "info");

			if (!info.Value.IsEmpty)
			{
				WarnIfInvalidPK(info, null, FindList(info), GetNotificationMessage(info));
			}
		}

		/// <summary>
		/// Sets a warning if the PK does not exist in the list.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="list">The list to check.</param>
		public static void WarnIfInvalidPK(ZPropertyInfo info, IBusinessObjectCollection list)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(list, "list");

			if (!info.Value.IsEmpty)
			{
				WarnIfInvalidPK(info, list, null, GetNotificationMessage(info));
			}
		}

		/// <summary>
		/// Sets a warning if the PK does not exist in the list.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="list">The list to check.</param>
		/// <param name="message">The message to show if the test fails.</param>
		public static void WarnIfInvalidPK(ZPropertyInfo info, IBusinessObjectCollection list, IMultilingualString message)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(list, "list");
			Argument.NotNull(message, "message");

			if (!info.Value.IsEmpty)
			{
				WarnIfInvalidPK(info, list, null, message);
			}
		}

		static void WarnIfInvalidPK(ZPropertyInfo info, IEnumerable explicitList, IEnumerable foundList, IMultilingualString message)
		{
			Validate(info, explicitList, foundList, NotificationType.Warning, message, GetValidator(explicitList ?? foundList).IsValidPK);
		}

		#endregion

		#region WarnIfInvalidCode

		/// <summary>
		/// Sets a warning if the code does not exist in the list.
		/// Will try to find list by inspecting property metadata.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <exception cref="Exception">If the list can not be found</exception>
		public static void WarnIfInvalidCode(ZPropertyInfo info)
		{
			Argument.NotNull(info, "info");

			if (!info.Value.IsEmpty)
			{
				WarnIfInvalidCode(info, null, FindList(info), InvalidCodeMessage);
			}
		}

		/// <summary>
		/// Sets a warning if the code does not exist in the list.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="list">The list to check.</param>
		public static void WarnIfInvalidCode(ZPropertyInfo info, IBusinessObjectCollection list)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(list, "list");

			if (!info.Value.IsEmpty)
			{
				WarnIfInvalidCode(info, list, null, InvalidCodeMessage);
			}
		}

		/// <summary>
		/// Sets a warning if the code does not exist in the list.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="list">The list to check.</param>
		/// <param name="message">The message to show if the test fails.</param>
		public static void WarnIfInvalidCode(ZPropertyInfo info, IBusinessObjectCollection list, IMultilingualString message)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(list, "list");
			Argument.NotNull(message, "message");

			if (!info.Value.IsEmpty)
			{
				WarnIfInvalidCode(info, list, null, message);
			}
		}

		/// <summary>
		/// Sets a warning if the code does not exist in the list.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="list">The list to check.</param>
		public static void WarnIfInvalidCode(ZPropertyInfo info, ICodeDescriptionPairList list)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(list, "list");

			if (!info.Value.IsEmpty)
			{
				WarnIfInvalidCode(info, list, null, InvalidCodeMessage);
			}
		}

		/// <summary>
		/// Sets a warning if the code does not exist in the list.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="list">The list to check.</param>
		/// <param name="message">The message to show if the test fails.</param>
		public static void WarnIfInvalidCode(ZPropertyInfo info, ICodeDescriptionPairList list, IMultilingualString message)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(list, "list");
			Argument.NotNull(message, "message");

			if (!info.Value.IsEmpty)
			{
				WarnIfInvalidCode(info, list, null, message);
			}
		}

		/// <summary>
		/// Sets a warning if the code does not exist in the list.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="list">The list to check.</param>
		/// <param name="messagePrefix">The message prefix to use with standard failure message.</param>
		public static void WarnIfInvalidCode(ZPropertyInfo info, ICodeDescriptionPairList list, string messagePrefix)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(list, "list");
			Argument.NotNullOrEmpty(messagePrefix, "messagePrefix");

			if (!info.Value.IsEmpty)
			{
				WarnIfInvalidCode(info, list, null, MultilingualString.Join(string.Empty, (NoResString)messagePrefix, (MultilingualString)InvalidCodeMessage));
			}
		}

		static void WarnIfInvalidCode(ZPropertyInfo info, IEnumerable explicitList, IEnumerable foundList, IMultilingualString message)
		{
			Validate(info, explicitList, foundList, NotificationType.Warning, message, GetValidator(explicitList ?? foundList).IsValidCode);
		}

		/// <summary>
		/// Sets an warning if the code does not exist in the list.
		/// Will try to find list by inspecting property metadata.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="message">The message to show if the test fails.</param>
		/// <exception cref="Exception">If the list can not be found</exception>
		public static void WarnIfInvalidCode(ZPropertyInfo info, IMultilingualString message)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(message, "message");

			if (!info.Value.IsEmpty)
			{
				WarnIfInvalidCode(info, null, FindList(info), message);
			}
		}
		#endregion

		#endregion

		#region Errors

		#region ErrorIfInvalidPK

		/// <summary>
		/// Sets an error if the PK does not exist in the list. 
		/// Will try to find list by inspecting property metadata.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <exception cref="Exception">If the list can not be found</exception>
		public static void ErrorIfInvalidPK(ZPropertyInfo info)
		{
			Argument.NotNull(info, "info");

			if (!info.Value.IsEmpty)
			{
				ErrorIfInvalidPK(info, null, FindList(info), GetNotificationMessage(info));
			}
		}

		/// <summary>
		/// Sets an error if the PK does not exist in the list.
		/// Will try to find list by inspecting property metadata.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="message">The message to show if the test fails.</param>
		public static void ErrorIfInvalidPK(ZPropertyInfo info, IMultilingualString message)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(message, "message");

			if (!info.Value.IsEmpty)
			{
				ErrorIfInvalidPK(info, null, FindList(info), message);
			}
		}

		/// <summary>
		/// Sets an error if the PK does not exist in the list.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="list">The list to check.</param>
		public static void ErrorIfInvalidPK(ZPropertyInfo info, IBusinessObjectCollection list)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(list, "list");

			if (!info.Value.IsEmpty)
			{
				ErrorIfInvalidPK(info, list, null, GetNotificationMessage(info));
			}
		}

		/// <summary>
		/// Sets an error if the PK does not exist in the list.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="list">The list to check.</param>
		public static void ErrorIfInvalidPK(ZPropertyInfo info, ICodeDescriptionPairList list)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(list, "list");

			if (!info.Value.IsEmpty)
			{
				ErrorIfInvalidPK(info, list, null, GetNotificationMessage(info));
			}
		}

		/// <summary>
		/// Sets an error if the PK does not exist in the list.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="list">The list to check.</param>
		/// <param name="message">The message to show if the test fails.</param>
		public static void ErrorIfInvalidPK(ZPropertyInfo info, IBusinessObjectCollection list, IMultilingualString message)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(list, "list");
			Argument.NotNull(message, "message");

			if (!info.Value.IsEmpty)
			{
				ErrorIfInvalidPK(info, list, null, message);
			}
		}

		/// <summary>
		/// Sets an error if the PK does not exist in the list.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="list">The list to check.</param>
		/// <param name="message">The message to show if the test fails.</param>
		public static void ErrorIfInvalidPK(ZPropertyInfo info, ICodeDescriptionPairList list, IMultilingualString message)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(list, "list");
			Argument.NotNull(message, "message");

			if (!info.Value.IsEmpty)
			{
				ErrorIfInvalidPK(info, list, null, message);
			}
		}

		static void ErrorIfInvalidPK(ZPropertyInfo info, IEnumerable explicitList, IEnumerable foundList, IMultilingualString message)
		{
			Validate(info, explicitList, foundList, NotificationType.Error, message, GetValidator(explicitList ?? foundList).IsValidPK);
		}

		#endregion

		#region ErrorIfInvalidCode

		/// <summary>
		/// Sets an error if the code does not exist in the list.
		/// Will try to find list by inspecting property metadata.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <exception cref="Exception">If the list can not be found</exception>
		public static void ErrorIfInvalidCode(ZPropertyInfo info)
		{
			Argument.NotNull(info, "info");

			if (!info.Value.IsEmpty)
			{
				ErrorIfInvalidCode(info, null, FindList(info), GetNotificationMessage(info));
			}
		}

		/// <summary>
		/// Sets an error if the code does not exist in the list.
		/// Will try to find list by inspecting property metadata.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="message">The message to show if the test fails.</param>
		/// <exception cref="Exception">If the list can not be found</exception>
		public static void ErrorIfInvalidCode(IMultilingualString message, ZPropertyInfo info)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(message, "message");

			if (!info.Value.IsEmpty)
			{
				ErrorIfInvalidCode(info, null, FindList(info), message);
			}
		}

		/// <summary>
		/// Sets an error if the code does not exist in the list.
		/// Will try to find list by inspecting property metadata.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="propertyName">The property name to use with standard failure message.</param>
		/// <exception cref="Exception">If the list can not be found</exception>
		public static void ErrorIfInvalidCode(ZPropertyInfo info, IMultilingualString propertyName)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(propertyName, "propertyName");

			if (!info.Value.IsEmpty)
			{
				ErrorIfInvalidCode(info, null, FindList(info), GetNotificationMessage(propertyName));
			}
		}

		/// <summary>
		/// Sets an error if the code does not exist in the list.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="list">The list to check.</param>
		public static void ErrorIfInvalidCode(ZPropertyInfo info, IBusinessObjectCollection list)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(list, "list");

			if (!info.Value.IsEmpty)
			{
				ErrorIfInvalidCode(info, list, null, GetNotificationMessage(info));
			}
		}

		/// <summary>
		/// Sets an error if the code does not exist in the list.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="list">The list to check.</param>
		public static void ErrorIfInvalidCode(ZPropertyInfo info, ICodeDescriptionPairList list)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(list, "list");

			if (!info.Value.IsEmpty)
			{
				ErrorIfInvalidCode(info, list, null, GetNotificationMessage(info));
			}
		}

		/// <summary>
		/// Sets an error if the code does not exist in the list.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="list">The list to check.</param>
		/// <param name="message">The message to show if the test fails.</param>
		public static void ErrorIfInvalidCode(ZPropertyInfo info, IBusinessObjectCollection list, IMultilingualString message)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(list, "list");
			Argument.NotNull(message, "message");

			if (!info.Value.IsEmpty)
			{
				ErrorIfInvalidCode(info, list, null, message);
			}
		}

		/// <summary>
		/// Sets an error if the code does not exist in the list.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="list">The list to check.</param>
		/// <param name="propertyName">The property name to use with standard failure message.</param>
		public static void ErrorIfInvalidCode(ZPropertyInfo info, ICodeDescriptionPairList list, IMultilingualString propertyName)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(list, "list");
			Argument.NotNull(propertyName, "propertyName");

			if (!info.Value.IsEmpty)
			{
				ErrorIfInvalidCode(info, list, null, GetNotificationMessage(propertyName));
			}
		}

		static void ErrorIfInvalidCode(ZPropertyInfo info, IEnumerable explicitList, IEnumerable foundList, IMultilingualString message)
		{
			Validate(info, explicitList, foundList, NotificationType.Error, message, GetValidator(explicitList ?? foundList).IsValidCode);
		}

		#endregion

		#region ErrorIfInvalidCodeButJustWarnIfExistingInactiveCode

		/// <summary>
		/// Sets an error if the code does not exist in the list, but just warn if it is an existing inactive code
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="allCodes">The list of all valid codes.</param>
		/// <param name="activeCodes">The subset of codes that are active.</param>
		public static void ErrorIfInvalidCodeButJustWarnIfExistingInactiveCode(ZPropertyInfo info, ICodeDescriptionPairList allCodes, ICodeDescriptionPairList activeCodes)
		{
			if (!info.BizObj.IsInDatabase || info.HasChanges || !allCodes.ContainsCode(info.Value))
			{
				ListValidation.ErrorIfInvalidCode(info, activeCodes);
			}
			else
			{
				ListValidation.WarnIfInvalidCode(info, activeCodes, ListValidation.InactiveCodeMessage);
			}
		}

		#endregion

		#endregion

		#region MessageErrors

		#region MessageErrorIfInvalidCode

		/// <summary>
		/// Sets a message error if the code does not exist in the list.
		/// Will try to find list by inspecting property metadata.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <exception cref="Exception">If the list can not be found</exception>
		public static void MessageErrorIfInvalidCode(ZPropertyInfo info)
		{
			Argument.NotNull(info, "info");

			if (!info.Value.IsEmpty)
			{
				MessageErrorIfInvalidCode(info, null, FindList(info), InvalidCodeMessageError);
			}
		}

		/// <summary>
		/// Sets a specific message error if the code does not exist in the list specified in the Property Metadata.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="message">The message to show if the test fails.</param>
		/// <exception cref="Exception">If the list can not be found</exception>
		public static void MessageErrorIfInvalidCode(ZPropertyInfo info, IMultilingualString message)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(message, "message");

			if (!info.Value.IsEmpty)
			{
				MessageErrorIfInvalidCode(info, null, FindList(info), message);
			}
		}

		/// <summary>
		/// Sets a message error if the code does not exist in the list.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="list">The list to check.</param>
		public static void MessageErrorIfInvalidCode(ZPropertyInfo info, ICodeDescriptionPairList list)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(list, "list");

			if (!info.Value.IsEmpty)
			{
				MessageErrorIfInvalidCode(info, list, null, InvalidCodeMessageError);
			}
		}

		/// <summary>
		/// Sets a message error if the code does not exist in the list.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="list">The list to check.</param>
		/// <param name="message">The message to show if the test fails.</param>
		public static void MessageErrorIfInvalidCode(ZPropertyInfo info, ICodeDescriptionPairList list, IMultilingualString message)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(list, "list");
			Argument.NotNull(message, "message");

			if (!info.Value.IsEmpty)
			{
				MessageErrorIfInvalidCode(info, list, null, message);
			}
		}

		/// <summary>
		/// Sets a message error if the code does not exist in the list.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="list">The list to check.</param>
		public static void MessageErrorIfInvalidCode(ZPropertyInfo info, IBusinessObjectCollection list)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(list, "list");

			if (!info.Value.IsEmpty)
			{
				MessageErrorIfInvalidCode(info, list, null, InvalidCodeMessageError);
			}
		}

		/// <summary>
		/// Sets a message error if the code does not exist in the list.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="list">The list to check.</param>
		/// <param name="message">The message to show if the test fails.</param>
		public static void MessageErrorIfInvalidCode(ZPropertyInfo info, IBusinessObjectCollection list, IMultilingualString message)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(list, "list");
			Argument.NotNull(message, "message");

			if (!info.Value.IsEmpty)
			{
				MessageErrorIfInvalidCode(info, list, null, message);
			}
		}

		static void MessageErrorIfInvalidCode(ZPropertyInfo info, IEnumerable explicitList, IEnumerable foundList, IMultilingualString message)
		{
			Validate(info, explicitList, foundList, NotificationType.MessageError, message, GetValidator(explicitList ?? foundList).IsValidCode);
		}

		#endregion

		#region MessageErrorIfInvalidCodeCaseSensitive

		public static void MessageErrorIfInvalidCodeCaseSensitive(ZPropertyInfo info)
		{
			Argument.NotNull(info, "info");

			MessageErrorIfInvalidCodeCaseSensitive(info, null, FindList(info));
		}

		public static void MessageErrorIfInvalidCodeCaseSensitive(ZPropertyInfo info, IEnumerable list)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(list, "list");

			MessageErrorIfInvalidCodeCaseSensitive(info, list, null);
		}

		static void MessageErrorIfInvalidCodeCaseSensitive(ZPropertyInfo info, IEnumerable explicitList, IEnumerable foundList)
		{
			if (!info.Value.IsEmpty)
			{
				var list = explicitList ?? foundList;
				var validator = list is ICodeDescriptionPairList ? (ListValidator)new CodeDescriptionPairListCaseSensitiveValidator() : new BusinessObjectCollectionCaseSensitiveValidator();
				Validate(info, explicitList, foundList, NotificationType.MessageError, InvalidCodeMessageError, validator.IsValidCode);
			}
		}

		#endregion

		#region MessageErrorIfInvalidCodeCaseSensitiveOrEmpty

		public static void MessageErrorIfInvalidCodeCaseSensitiveOrEmpty(ZPropertyInfo info)
		{
			Argument.NotNull(info, "info");
			MessageErrorIfInvalidCodeCaseSensitiveOrEmpty(info, null, FindList(info));
		}

		public static void MessageErrorIfInvalidCodeCaseSensitiveOrEmpty(ZPropertyInfo info, IEnumerable list)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(list, "list");
			MessageErrorIfInvalidCodeCaseSensitiveOrEmpty(info, list, null);
		}

		static void MessageErrorIfInvalidCodeCaseSensitiveOrEmpty(ZPropertyInfo info, IEnumerable explicitList, IEnumerable foundList)
		{
			if (info.Value.IsEmpty)
			{
				var description = MandatoryValidation.GetErrorFieldFromProperyInfo(info);
				info.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(description));
				return;
			}

			MessageErrorIfInvalidCodeCaseSensitive(info, explicitList, foundList);
		}

		#endregion

		#region MessageErrorIfInvalidCodeOrEmpty

		/// <summary>
		/// Sets a message error if the code does not exist in the list or value is empty.
		/// Will try to find list by inspecting property metadata.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <exception cref="Exception">If the list can not be found</exception>
		public static void MessageErrorIfInvalidCodeOrEmpty(ZPropertyInfo info)
		{
			Argument.NotNull(info, "info");

			MessageErrorIfInvalidCodeOrEmpty(info, null, FindList(info), "");
		}

		/// <summary>
		/// Sets a message error if the code does not exist in the list or value is empty.
		/// Will try to find list by inspecting property metadata.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="list">The list to check.</param>
		public static void MessageErrorIfInvalidCodeOrEmpty(ZPropertyInfo info, ICodeDescriptionPairList list)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(list, "list");

			MessageErrorIfInvalidCodeOrEmpty(info, list, null, "");
		}

		/// <summary>
		/// Sets a message error if the code does not exist in the list or value is empty.
		/// Will try to find list by inspecting property metadata.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="list">The list to check.</param>
		/// <param name="description">The description to use with failure message.</param>
		public static void MessageErrorIfInvalidCodeOrEmpty(ZPropertyInfo info, ICodeDescriptionPairList list, string description)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(list, "list");
			Argument.NotNullOrEmpty(description, "description");

			MessageErrorIfInvalidCodeOrEmpty(info, list, null, description);
		}

		/// <summary>
		/// Sets a message error if the code does not exist in the list or value is empty.
		/// Will try to find list by inspecting property metadata.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="list">The list to check.</param>
		public static void MessageErrorIfInvalidCodeOrEmpty(ZPropertyInfo info, IBusinessObjectCollection list)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(list, "list");

			MessageErrorIfInvalidCodeOrEmpty(info, list, null, "");
		}

		/// <summary>
		/// Sets a message error if the code does not exist in the list or value is empty.
		/// Will try to find list by inspecting property metadata.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="list">The list to check.</param>
		/// <param name="description">The description to use with failure message.</param>
		public static void MessageErrorIfInvalidCodeOrEmpty(ZPropertyInfo info, IBusinessObjectCollection list, ZString description)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(list, "list");
			Argument.NotNullOrEmpty(description, "description");

			MessageErrorIfInvalidCodeOrEmpty(info, list, null, description);
		}

		/// <summary>
		/// Sets a message error if the code does not exist in the list or value is empty.
		/// Will try to find list by inspecting property metadata.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="description">The description to use with failure message.</param>
		public static void MessageErrorIfInvalidCodeOrEmpty(ZPropertyInfo info, ZString description)
		{
			Argument.NotNull(info, "info");
			Argument.NotNullOrEmpty(description, "description");

			MessageErrorIfInvalidCodeOrEmpty(info, null, FindList(info), description);
		}

		static void MessageErrorIfInvalidCodeOrEmpty(ZPropertyInfo info, IEnumerable explicitList, IEnumerable foundList, string description)
		{
			if (info.Value.IsEmpty)
			{
				if (string.IsNullOrEmpty(description))
				{
					description = MandatoryValidation.GetErrorFieldFromProperyInfo(info);
				}
				info.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(description));
				return;
			}

			Validate(info, explicitList, foundList, NotificationType.MessageError, InvalidCodeMessageError, GetValidator(explicitList ?? foundList).IsValidCode);
		}

		#endregion

		#endregion

		#region ListValidators for IsValidCode / IsValidPK

		abstract class ListValidator
		{
			public abstract ValidationResult IsValidPK(ZPropertyInfo info, IEnumerable list);
			public abstract ValidationResult IsValidCode(ZPropertyInfo info, IEnumerable list);
		}

		enum ValidationResult { Valid, NotFound, Cancelled }

		class BusinessObjectCollectionValidator : ListValidator
		{
			public override ValidationResult IsValidCode(ZPropertyInfo info, IEnumerable list)
			{
				return IsValidCodeCore(info, list, false);
			}

			internal BusinessObject GetBusinessObjectByCode(ZPropertyInfo info, IEnumerable list)
			{
				var collection = (IBusinessObjectCollection)list;
				var elementType = collection.TypeOfElements;
				if (collection is ICompositeCollection)
				{
					elementType = ((ICompositeCollection)collection).TypeOfElementFromCode((ZString)info.Value);
				}

				var propertyName = CodePropertyAttribute.CodePropertyNameFromType(elementType);
				var tableName = BusinessObjectFactory.GetTableNameFromType(elementType);
				var schemaColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(propertyName, tableName);

				var query = new ZQuery(schemaColumn, info.Value);
				_ = query.AddToFilter(collection.CompleteFilter, JoinCondition.And);

				return collection.Factory.LoadTop1(elementType, query);
			}

			public override ValidationResult IsValidPK(ZPropertyInfo info, IEnumerable list)
			{
				if (list is IExternalListValidation externalValidation)
				{
					if (externalValidation.IsValidTemplateRecordPK((ZGuid)info.Value))
					{
						return ValidationResult.Valid;
					}
				}

				var collection = (IBusinessObjectCollection)list;

				var elementType = collection.TypeOfElements;
				if (collection is ICompositeCollection)
				{
					elementType = ((ICompositeCollection)collection).TypeOfElementFromPK((ZGuid)info.Value);
				}

				var obj = collection.Factory.Load(elementType, (ZGuid)info.Value);

				IActiveBusinessObjectCollection activeCollection = collection as IActiveBusinessObjectCollection;
				ValidationResult result = ValidationResult.NotFound;
				if (obj != null)
				{
					if (activeCollection != null)
					{
						result = activeCollection.Contains(obj) ? ValidationResult.Valid : ValidationResult.NotFound;
					}
					else
					{
						result = obj.MatchesFilter(collection.CompleteFilter) ? ValidationResult.Valid : ValidationResult.NotFound;
					}
				}

				if (result == ValidationResult.Valid)
				{
					ICancellable cancellableBizO = obj as ICancellable;
					if (cancellableBizO != null && cancellableBizO.IsCancelled)
					{
						result = ValidationResult.Cancelled;
					}
				}

				return result;
			}

			internal BusinessObject GetBusinessObjectByPK(ZPropertyInfo info, IEnumerable list)
			{
				var collection = (IBusinessObjectCollection)list;
				var elementType = collection.TypeOfElements;
				if (collection is ICompositeCollection)
				{
					elementType = ((ICompositeCollection)collection).TypeOfElementFromPK((ZGuid)info.Value);
				}

				return collection.Factory.Load(elementType, (ZGuid)info.Value);
			}

			protected ValidationResult IsValidCodeCore(ZPropertyInfo info, IEnumerable list, bool matchCase)
			{
				var collection = (IBusinessObjectCollection)list;

				var elementType = collection.TypeOfElements;
				if (collection is ICompositeCollection)
				{
					elementType = ((ICompositeCollection)collection).TypeOfElementFromCode((ZString)info.Value);
				}

				var propertyName = CodePropertyAttribute.CodePropertyNameFromType(elementType);
				var tableName = BusinessObjectFactory.GetTableNameFromType(elementType);
				var schemaResolver = ObjectFactory.Get<IApplicationSchemaResolver>();
				var schemaColumn = schemaResolver.GetSchemaColumn(propertyName, tableName);

				var query = new ZQuery();
				_ = query.AddToFilter(schemaColumn, info.Value);
				_ = query.AddToFilter(collection.CompleteFilter, JoinCondition.And);

				if (tableName == "StmNote")
				{
					//we need all queries to fill out ST_Table. and eventually StmNote will be partitioned on ST_Table, so there should be no such thing as a mixed ST_Table collection of notes.
					var sTTableColumn = schemaResolver.GetSchemaColumn("ST_Table", "StmNote");

					var stmNoteBizo = collection
						.Cast<BusinessObject>()
						.FirstOrDefault(note => note.TableName == "StmNote" && !string.IsNullOrWhiteSpace(note[sTTableColumn] as string));
					var tablename = stmNoteBizo?[sTTableColumn] as string ?? string.Empty;

					_ = query.AddToFilter(sTTableColumn, tablename);
				}

				var bizO = collection.Factory.LoadTop1(elementType, query);
				var result = bizO is null ? ValidationResult.NotFound : ValidationResult.Valid;
				if (result == ValidationResult.Valid)
				{
					if (bizO is ICancellable cancellableBizO && cancellableBizO.IsCancelled)
					{
						result = ValidationResult.Cancelled;
					}
					else if (matchCase)
					{
						if (bizO.TryGetPropertyValueByName(schemaColumn.Name, out var bizoPropertyValue) && !bizoPropertyValue.Equals(info.Value))
						{
							result = ValidationResult.NotFound;
						}
					}
				}

				return result;
			}
		}

		class BusinessObjectCollectionCaseSensitiveValidator : BusinessObjectCollectionValidator
		{
			public override ValidationResult IsValidCode(ZPropertyInfo info, IEnumerable list)
			{
				return IsValidCodeCore(info, list, true);
			}
		}

		class CodeDescriptionPairListValidator : ListValidator
		{
			public override ValidationResult IsValidCode(ZPropertyInfo info, IEnumerable list)
			{
				var lst = (ICodeDescriptionPairList)list;
				return lst.ContainsCode(info.Value) ? ValidationResult.Valid : ValidationResult.NotFound;
			}

			public override ValidationResult IsValidPK(ZPropertyInfo info, IEnumerable list)
			{
				foreach (ICodeDescription element in list)
				{
					if (info.Value.Equals(element.PK))
					{
						return ValidationResult.Valid;
					}
				}
				return ValidationResult.NotFound;
			}
		}

		class CodeDescriptionPairListCaseSensitiveValidator : CodeDescriptionPairListValidator
		{
			public override ValidationResult IsValidCode(ZPropertyInfo info, IEnumerable list)
			{
				var trimmedCode = info.Value.ToString().TrimEnd();
				foreach (ICodeDescription element in list)
				{
					if (string.Equals(element.Code.Trim(), trimmedCode))
					{
						return ValidationResult.Valid;
					}
				}
				return ValidationResult.NotFound;
			}
		}

		static ListValidator GetValidator(IEnumerable collection)
		{
			return collection is ICodeDescriptionPairList ? new CodeDescriptionPairListValidator() : new BusinessObjectCollectionValidator();
		}

		#endregion

		#region Implementation

		static IEnumerable FindList(ZPropertyInfo info)
		{
			return MetaData.GetListDataSource(info.BizObj, info.PropertyDescriptor)
				?? throw new Exception("Can't find property member specified in [List] attribute for property " + info.BizObj.GetType() + "." + info.Name);
		}

		static void Validate(ZPropertyInfo info, IEnumerable explicitList, IEnumerable foundList, INotificationType notificationType, IMultilingualString message, IsValidValidPredicate isValidPredicate)
		{
			Validate(info, explicitList, foundList, message, isValidPredicate, m => info.AddNotification(notificationType, m));
		}

		static void Validate(ZPropertyInfo info, IEnumerable explicitList, IEnumerable foundList, IMultilingualString message, IsValidValidPredicate isValidPredicate, Action<string> addNotification)
		{
			if (explicitList == foundList)
			{
				// TODO: Uncomment and fix everywhere
				// ErrorReporter.ReportOnce(info.GetHashCode().ToString(), "There is already list found via [List] attribute. Please do not pass list explicitly");
			}

			IEnumerable list = explicitList ?? foundList;

			if (!info.Value.IsEmpty && isValidPredicate(info, list) == ValidationResult.NotFound)
			{
				addNotification(message.ToString());
			}
		}

		delegate ValidationResult IsValidValidPredicate(ZPropertyInfo info, IEnumerable collection);

		#endregion

		public static void ErrorIfCancelledAndEditable(ZPropertyInfo info)
		{
			if (!info.Value.IsEmpty && info.Value.IsValid && !info.ReadOnly)
			{
				var list = MetaData.GetListDataSource(info.BizObj, info.PropertyDescriptor);
				if (list != null)
				{
					ErrorIfCancelledAndEditableCore(info, list);
				}
			}
		}

		public static void ErrorIfCancelledAndEditable(ZPropertyInfo info, IEnumerable list)
		{
			if (!info.Value.IsEmpty && info.Value.IsValid && !info.ReadOnly)
			{
				ErrorIfCancelledAndEditableCore(info, list);
			}
		}

		static void ErrorIfCancelledAndEditableCore(ZPropertyInfo info, IEnumerable list)
		{
			var isCancelled = false;
			var showWarningIfCancelled = false;

			ICompositeCollection compositeCollection;
			BusinessObject bizO = null;

			if (list is IBusinessObjectCollection)
			{
				if (info.Value is ZString)
				{
					bizO = new BusinessObjectCollectionValidator().GetBusinessObjectByCode(info, list);
				}
				else
				{
					bizO = new BusinessObjectCollectionValidator().GetBusinessObjectByPK(info, list);
				}
			}
			else if ((compositeCollection = list as ICompositeCollection) != null)
			{
				if (info.Value is ZString)
				{
					var stringValue = (ZString)info.Value;
					var elementType = compositeCollection.TypeOfElementFromCode(stringValue);
					if (elementType != null && typeof(BusinessObject).IsAssignableFrom(elementType) && typeof(ICancellable).IsAssignableFrom(elementType))
					{
						string codePropertyName;
						ITableSchema tableSchema;
						SchemaColumn schemaColumn;
						if ((codePropertyName = CodePropertyAttribute.CodePropertyNameFromType(elementType)) != null &&
							(tableSchema = BusinessObjectFactory.GetTableSchemaFromType(elementType, false)) != null &&
							(schemaColumn = tableSchema.GetSchemaColumn(codePropertyName)) != null)
						{
							bizO = info.BizObj.Factory.LoadFromNaturalKey(elementType, schemaColumn, stringValue);
						}
					}
				}
				else
				{
					var guidValue = (ZGuid)info.Value;
					var elementType = compositeCollection.TypeOfElementFromPK(guidValue);
					if (elementType != null && typeof(BusinessObject).IsAssignableFrom(elementType) && typeof(ICancellable).IsAssignableFrom(elementType))
					{
						bizO = info.BizObj.Factory.Load(elementType, guidValue);
					}
				}
			}

			isCancelled = bizO is ICancellable cancellableBizO && cancellableBizO.IsCancelled;
			showWarningIfCancelled = bizO?.ShowWarningIfCancelled ?? false;
			ErrorIfCancelled(info, isCancelled, showWarningIfCancelled);
		}

		public static void ErrorIfCancelled(ZPropertyInfo info, bool isCancelled, bool showWarningIfCancelled = false)
		{
			if (isCancelled)
			{
				var description = info.HasHumanReadableName ? info.HumanReadableName.ToString() : Res.GetString("0137baec-ae22-441c-943c-6c940a5eec63", "selection");
				if (!showWarningIfCancelled && (!info.BizObj.IsInDatabase || info.HasChanges))
				{
					info.AddError(Res.GetString("cbfcde34-a8d9-4754-85ef-88714ff9c91c", "This {0} is inactive - it may not be used.", description));
				}
				else
				{
					info.AddWarning(Res.GetString("398c24f0-c51f-4414-bf4e-f2075ed30252", "This {0} is inactive.", description));
				}
			}
		}

		#region Implementation of IListValidationInternals

		void IListValidationInternals.MessageErrorIfInvalidCode(ZPropertyInfo info, IEnumerable explicitList, IMultilingualString message, Action<string> addNotification)
		{
			var foundList = FindList(info);
			Validate(info, explicitList, foundList, message, GetValidator(explicitList ?? foundList).IsValidCode, addNotification);
		}

		#endregion

		/// <summary>
		/// Sets an error if the code does not exist in the list or the value is empty
		/// Will try to find list by inspecting property metadata.
		/// </summary>
		/// <param name="info">The property to validate.</param>
		public static void ErrorIfInvalidCodeOrEmpty(ZPropertyInfo info)
		{
			Argument.NotNull(info, "info");

			var foundList = FindList(info);
			ErrorIfInvalidCodeOrEmpty(info, foundList);
		}

		/// <summary>
		/// Sets an error if the code does not exist in the list or the value is empty
		/// </summary>
		/// <param name="info">The property to validate.</param>
		/// <param name="list">The list to check.</param>
		public static void ErrorIfInvalidCodeOrEmpty(ZPropertyInfo info, IEnumerable list)
		{
			Argument.NotNull(info, "info");
			Argument.NotNull(list, "list");

			MandatoryValidation.CheckEntered(info);
			ErrorIfInvalidCode(info, list, null, GetNotificationMessage(info));
		}
	}
}
