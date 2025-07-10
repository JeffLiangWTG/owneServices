using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Common;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.Environment.DialogDefault
{
	public class DialogDefaultContext
	{
		public DialogDefaultContext(ZGuid dialogIdentifier, string caption, ZMessageBoxButtons? buttons, ZMessageBoxIcon icon, bool showCheckboxOnly = false, ZDialogResult defaultResult = ZDialogResult.None, bool isGlobalOnly = false, ResourceStringData checkBoxCaption = null)
			: this(dialogIdentifier, caption, buttons, icon, ZBlob.Empty, null, showCheckboxOnly, null, defaultResult, isGlobalOnly, checkBoxCaption)
		{ }

		public DialogDefaultContext(ZGuid dialogIdentifier, string caption, ZMessageBoxButtons? buttons, ZMessageBoxIcon icon, ZGuid contextItem1, ZGuid contextItem2 = default(ZGuid), ResourceStringData nullContextDescription = null, bool showCheckboxOnly = false)
			: this(dialogIdentifier, caption, buttons, icon, ToZBlob(contextItem1, contextItem2), nullContextDescription, showCheckboxOnly)
		{
		}

		///  <summary>
		/// 		The context from which a defaultable dialog can be created
		///  </summary>
		/// <param name="dialogIdentifier">
		///		The identifier for this family of dialogs. If two contexts have the same identifier then they are considered the same dialog
		/// </param>
		/// <param name="caption">
		/// 		The title for the dialog box
		///  </param>
		///  <param name="buttons">
		/// 		The buttons to display on the box. Null for no buttons
		/// 		Note that DialogResult.Cancel is always a possible return value
		///  </param>
		///  <param name="context">
		/// 		An additional context for this default. For example, if this dialog refers to an action to do to a particular type of task, that type of task would be the context.
		/// 		An empty context indicates that this applies to all defaults that match the dialogIdentifier (In the example above, that would be applies to all contexts)
		///  </param>
		///  <param name="showCheckboxOnly">
		/// 		If true, rather than showing a full defaults modifying control, there will be a simple 'remember me' checkbox that will save defaults for user only
		///  </param>
		///  <param name="nullContextDescription">
		/// 		Describes what a null context would mean in respect to this dialog. In the 'tasks' example above you could use "Applies to all tasks"
		///  </param>
		///  <param name="resultsNotToSave">
		/// 		If the DialogResult returned from the form is one of the ones in the list, then it will not be saved. This is useful to avoid saving things like DialogResult.Cancel 
		/// 		in situations that may confuse the user
		///  </param>
		///  <param name="forceOverriddenDefaults">
		/// 		If true, the form will not allow to close and thus accept Cancel as the result when read only and dialog default is not set to Cancel.
		/// 		Should be set to true for operations that are crucial to perform.
		///  </param>
		public DialogDefaultContext(ZGuid dialogIdentifier, string caption, ZMessageBoxButtons? buttons, ZMessageBoxIcon icon, ZBlob context, ResourceStringData nullContextDescription = null, bool showCheckboxOnly = false, IEnumerable<ZDialogResult> resultsNotToSave = null, ZDialogResult defaultResult = ZDialogResult.None, bool isGlobalOnly = false, ResourceStringData checkBoxCaption = null, bool forceOverriddenDefaults = false)
		{
			Argument.NotNull(caption, nameof(caption));

			if (!dialogIdentifier.IsValid)
			{
				throw new ArgumentException("Invalid dialog identifier", nameof(dialogIdentifier));
			}

			if ((showCheckboxOnly || context.IsEmpty) && nullContextDescription != null)
			{
				throw new InvalidOperationException("The user cannot select a context, the nullContextDescription is not required.");
			}

			DialogIdentifier = dialogIdentifier;
			Caption = caption;
			Buttons = buttons;
			Icon = icon;
			ShowCheckboxOnly = showCheckboxOnly;
			NullContextDescription = nullContextDescription;
			Context = context;
			DialogResultsToNotSave = resultsNotToSave ?? Enumerable.Empty<ZDialogResult>();
			CheckBoxCaption = checkBoxCaption;
			GlobalOnly = isGlobalOnly;
			ForceOverriddenDefaults = forceOverriddenDefaults;

			if (defaultResult == ZDialogResult.None || IsValidResult(defaultResult))
			{
				DefaultResult = defaultResult;
			}
			else
			{
				DefaultResult = ZDialogResult.None;
				ErrorReporter.ReportOnce(string.Format(CultureInfo.CurrentCulture, "MessageBoxButtons.{0} does not contain an option for {1}", Enum.GetName(typeof(ZMessageBoxButtons), buttons), defaultResult));
			}
		}

		public ZGuid DialogIdentifier { get; private set; }
		public string Caption { get; private set; }
		public ZMessageBoxButtons? Buttons { get; private set; }
		public ZMessageBoxIcon Icon { get; private set; }
		public ZDialogResult DefaultResult { get; private set; }
		public bool ShowCheckboxOnly { get; private set; }
		public ResourceStringData NullContextDescription { get; private set; }
		public bool NullContextAllowed { get { return NullContextDescription != null; } }
		public ZBlob Context { get; private set; }
		public IEnumerable<ZDialogResult> DialogResultsToNotSave { get; private set; }
		public bool GlobalOnly { get; set; }
		public ResourceStringData CheckBoxCaption { get; }
		public bool ForceOverriddenDefaults { get; }

		internal const int ContextMaxLength = 32;
		const int BytesInGuid = 16;

		public static ZBlob ToZBlob(params ZGuid[] guids)
		{
			if (guids.Length > ContextMaxLength / BytesInGuid)
			{
				throw new ArgumentException("Cannot display that amount of ZGuids as a ZBlob within the space constraints");
			}

			return new ZBlob(guids
				.Where(g => !g.IsEmpty)
				.SelectMany(guid => guid.ToGuid().ToByteArray())
				.ToArray()
			);
		}

		public static ZBlob ToZBlob(string s, Encoding encoding = null)
		{
			if (string.IsNullOrEmpty(s))
			{
				throw new ArgumentException("S must not be null or empty. Consider using ZBlob.Empty instead");
			}

			var bytes = (encoding ?? Encoding.Default).GetBytes(s);
			if (bytes.Length < ContextMaxLength)
			{
				return new ZBlob(bytes);
			}
			else
			{
				using (var hasher = SHA256.Create())
				{
					return new ZBlob(hasher.ComputeHash(bytes));
				}
			}
		}

		public static bool IsValidResult(ZMessageBoxButtons? buttons, ZDialogResult dialogResult)
		{
			if (dialogResult == ZDialogResult.Cancel || buttons == null) { return true; }

			switch (buttons)
			{
				case ZMessageBoxButtons.OK:
				case ZMessageBoxButtons.OKCancel:
					return dialogResult == ZDialogResult.OK;
				case ZMessageBoxButtons.YesNo:
				case ZMessageBoxButtons.YesNoCancel:
					return dialogResult == ZDialogResult.Yes || dialogResult == ZDialogResult.No;
				case ZMessageBoxButtons.RetryCancel:
					return dialogResult == ZDialogResult.Retry;
				case ZMessageBoxButtons.AbortRetryIgnore:
					return dialogResult == ZDialogResult.Abort || dialogResult == ZDialogResult.Retry || dialogResult == ZDialogResult.Ignore;
				default:
					throw new DeveloperNotificationException(string.Format("MessageBoxButtons.{0} has not been catered to in the defaulting mechanism", Enum.GetName(typeof(ZMessageBoxButtons), buttons)));
			}
		}

		public bool IsValidResult(ZDialogResult dr)
		{
			return IsValidResult(Buttons, dr);
		}

		public override bool Equals(object obj)
		{
			var dObj = obj as DialogDefaultContext;

			return dObj != null &&
				   Buttons == dObj.Buttons &&
				   Icon == dObj.Icon &&
				   DialogIdentifier.Equals(dObj.DialogIdentifier) &&
				   ShowCheckboxOnly == dObj.ShowCheckboxOnly &&
				   Caption.Equals(dObj.Caption) &&
				   GlobalOnly == dObj.GlobalOnly &&
				   CheckBoxCaption == dObj.CheckBoxCaption;
		}

		public override int GetHashCode()
		{
			var prime = 7919;
			var hashCode = (int)Buttons + 8 * Convert.ToInt32(ShowCheckboxOnly);

			//Overflow doesn't matter in hashing
			unchecked
			{
				hashCode = (hashCode * prime) + (int)Icon;
				hashCode = (hashCode * prime) + DialogIdentifier.GetHashCode();
				hashCode = (hashCode * prime) + Caption.GetHashCode();
				hashCode = (hashCode * prime) + GlobalOnly.GetHashCode();
				hashCode = (hashCode * prime) + (CheckBoxCaption != null ? CheckBoxCaption.GetHashCode() : 0);
			}

			return hashCode;
		}
	}
}
