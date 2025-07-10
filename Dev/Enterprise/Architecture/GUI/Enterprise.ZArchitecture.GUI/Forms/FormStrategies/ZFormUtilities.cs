using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.URLHandler;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public static class ZFormUtilities
	{
		#region Ensure Selected Control Value Committed

		/// <summary>
		/// This ensures that any uncommitted values in the selected control are pushed into the business layer before
		/// any business logic runs in response to menu item clicks. Clearly this is not done for unbound controls.
		/// </summary>
		public static void EnsureSelectedControlValueCommitted(Form form)
		{
			var zform = form as ZForm;
			if (zform == null || !zform.IsProcessingCutCopyOrPaste)
			{
				var activeBoundControl = GetActiveBoundControl(form.GetFrontMostActiveControl());
				if (activeBoundControl != null)
				{
					var activeZGridControl = activeBoundControl as ZGrid;
					try
					{
						if (activeZGridControl != null) //Grids are special and are not bound like other controls.
						{
							activeZGridControl.CancelCurrentEditIfNotEdited();
							activeZGridControl.EndEdit(null, activeZGridControl.CurrentCell.RowNumber, false);
							activeZGridControl.ListManager.EndCurrentEdit();
						}
						else
						{
							PerformControlValidationMethod.Invoke(activeBoundControl, new object[] { true });
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						if (ex is System.Data.DeletedRowInaccessibleException || (ex is TargetInvocationException && ex.InnerException is System.Data.DeletedRowInaccessibleException))
						{
							//Control is on a detached business object that just got deleted. Since the user deleted it, they don't want to commit any values to it anyway (nor would it have any meaning).
						}
						else
						{
							throw;
						}
					}
				}
			}
		}

		/// <summary>
		/// MethodInfo for System.Windows.Forms.Control.PerformControlValidation(bool force)
		/// </summary>
		static MethodInfo PerformControlValidationMethod
		{
			get
			{
				if (performControlValidationMethod == null)
				{
					performControlValidationMethod = typeof(Control).GetMethod("PerformControlValidation", BindingFlags.NonPublic | BindingFlags.Instance);
				}
				return performControlValidationMethod;
			}
		}
		[ThreadStatic]
		static MethodInfo performControlValidationMethod;

		/// <summary>
		/// Gets the Bound Parent Control of the specified control
		/// </summary>
		/// <param name="parent"></param>
		/// <returns></returns>
		static Control GetActiveBoundControl(Control parent)
		{
			Control result = null;

			while (result == null && parent != null && parent.Parent != null && parent != parent.Parent)
			{
				if (IsControlBound(parent))
				{
					result = parent;
				}
				else
				{
					result = GetActiveBoundControl(parent.Parent);
					parent = parent.Parent;
				}
			}

			return result;
		}

		/// <summary>
		/// Checks if the control specified is bound. (NOTE: Special handling for ZGrid)
		/// </summary>
		/// <param name="control"></param>
		/// <returns></returns>
		static bool IsControlBound(Control control)
		{
			bool result;
			if (control is ZGrid)
			{
				result = ((ZGrid)control).ListManager != null;
			}
			else
			{
				result = control.DataBindings.Count > 0;
			}
			return result;
		}

		#endregion

		#region Handle Standard Key Codes

		public static void ShowDebugInfo(Control control)
		{
			using (var debugForm = new DebugControlInfoForm())
			{
				debugForm.ShowControlInfo(control);
			}
		}

		#endregion

		#region Favorites/Recent

		/// <summary>
		/// Returns a LinkWrapper for business object on a form; or null if LinkWrapper cannot be constructed (for example when business object is new and was not saved yet).
		/// </summary>
		/// <remarks>User business identifier from passed zForm.</remarks>
		public static LinkWrapper GetLinkWrapperForFavoriteOrRecent(ZForm zForm)
		{
			return GetLinkWrapperForFavoriteOrRecent(zForm, GetBusiness(zForm), false);
		}

		/// <summary>
		/// Returns a LinkWrapper for business object on a form; or null if LinkWrapper cannot be constructed (for example when business object is new and was not saved yet).
		/// </summary>
		/// <remarks>Uses passed business object's PK as identifier.</remarks>
		public static LinkWrapper GetLinkWrapperForFavoriteOrRecent(ZForm zForm, IBusiness business)
		{
			return GetLinkWrapperForFavoriteOrRecent(zForm, business, true);
		}

		/// <summary>
		/// Returns a LinkWrapper for business object on a form; or null if LinkWrapper cannot be constructed (for example when business object is new and was not saved yet).
		/// </summary>
		public static LinkWrapper GetLinkWrapperForFavoriteOrRecent(ZForm zForm, IBusiness business, bool useBusinessPk)
		{
			if (zForm != null && zForm.ControllerID != null)
			{
				var controller = ZControllerFactory.Create(zForm.ControllerID);
				var bizO = business as BusinessObject;
				if (controller != null && controller.ModuleID != null && bizO != null && bizO.IsInDatabase)
				{
					IBusiness loadedBusiness = null;
					var identifier = useBusinessPk ? bizO.PK.ToGuid() : GetIdentifier(zForm, business, controller, out loadedBusiness);
					if (identifier != Guid.Empty)
					{
						if (loadedBusiness != null)
						{
							bizO = loadedBusiness as BusinessObject;
						}
						return new LinkWrapper(controller.ModuleID.Name, identifier, BusinessEntityShortcutUrl(zForm, business, identifier), ((IRecentItemCaptionFormatter)zForm).FormatRecentItemCaption(bizO.HumanReadableShortcutName));
					}
				}
			}
			return null;
		}

		public static IBusiness GetBusiness(ZForm zForm)
		{
			return zForm != null ? (zForm.BusinessEntity ?? zForm.BusinessEntityForPersistingForm) : null;
		}

		public static Guid GetIdentifier(ZForm zForm, IBusiness bizO, ZController controller)
		{
			return GetIdentifier(zForm, bizO, controller, out _);
		}

		static Guid GetIdentifier(ZForm zForm, IBusiness bizO, ZController controller, out IBusiness otherBizo)
		{
			otherBizo = null;
			if (zForm == null)
			{
				return Guid.Empty;
			}

			var identifier = zForm.IdentifierForPersistingForm;
			var loadedBusiness = controller.LoadBusinessEntity(bizO.Factory, identifier); //may differ from original business object/identifier
			if (loadedBusiness != null && loadedBusiness.Identifier.IsValid)
			{
				if (loadedBusiness is ITemplateRecordProvider templateRecordProvider && templateRecordProvider.IsTemplateRecord &&
					templateRecordProvider.TemplateRecord is IBusiness templateRecordBizo && templateRecordBizo.Identifier.IsValid)
				{
					otherBizo = templateRecordBizo;
					return templateRecordBizo.Identifier.ToGuid();
				}
				otherBizo = loadedBusiness;
				return loadedBusiness.Identifier.ToGuid();
			}
			return identifier;
		}

		[SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		public static string BusinessEntityShortcutUrl(ZForm zForm, bool useWebHyperlinks)
		{
			if (zForm is null || zForm.ControllerID is null)
			{
				return string.Empty;
			}

			var controller = ZControllerFactory.Create(zForm.ControllerID);
			if (controller?.ModuleID is null || !controller.SupportsHyperlinking)
			{
				return string.Empty;
			}

			var business = GetBusiness(zForm);
			var identifier = GetIdentifier(zForm, business, controller);

			if (useWebHyperlinks)
			{
				return ShowEditFormUrlHandler.Instance.CreateWebTrampolineUri(zForm.ControllerID, identifier);
			}
			else
			{
				return ShowEditFormUrlHandler.Instance.Create(zForm.ControllerID, business, identifier);
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		public static string BusinessEntityShortcutUrl(ZForm zForm, IBusiness business, Guid identifier)
		{
			return zForm != null ? ShowEditFormUrlHandler.Instance.Create(zForm.ControllerID, business, identifier) : string.Empty;
		}

		public static Tuple<ControllerID, Guid> GetControllerIDsFromURLSafe(string url)
		{
			Tuple<ControllerID, Guid> result = null;
			url = WebUtility.HtmlDecode(url);

			if (url.StartsWith(UrlHandler.EdiUrlPrefix))
			{
				try
				{
					var queryString = EnterpriseUrlHandlerService.GetEnterpriseUrlQueryString(url);
					var controllerID = ShowFormUrlHandler.GetControllerID(queryString);
					var businessObjectPk = UrlHandler.GetBusinessEntityPk(queryString);

					if (controllerID != null)
					{
						return Tuple.Create(controllerID, businessObjectPk);
					}
				}
				catch (QueryStringException)
				{
					// The query string was invalid.
				}
				catch (EnterpriseUrlHandlerException)
				{
					// The url didn't have valid parts.
				}
			}
			else if (WebHyperlinksExpression.Match(url) is var match && match.Success)
			{
				var controllerName = match.Groups[2].Value;
				var bizOKey = match.Groups[3].Value;

				if (Guid.TryParse(bizOKey, out var bizOPK))
				{
					var controllerID = ZControllerFactory.Instance.GetRegisteredIdentifierByName(controllerName);
					if (controllerID != null)
					{
						return Tuple.Create(controllerID, bizOPK);
					}
				}
			}

			return result;
		}

		static Regex WebHyperlinksExpression { get; } = new Regex(
			@"https:\/\/(.*)\/link\/ShowEditForm\/([A-Z\d]+)\/([A-Z\d\-]+)",
			RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		#endregion

		#region Reload Form

		public static void ReloadCurrentForm(ZForm zForm)
		{
			if (zForm.ControllerID == null)
			{
				Globals.Message.Show(
					ResString.GetMultilingualString("4EEE594E-C00D-4605-AF42-55EE0FA9F2A0", "This form type cannot be reloaded."),
					ResString.GetMultilingualString("814FF414-1CEA-4AB6-9EA5-8266A6A9A0B0", "Error"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
			else
			{
				if (zForm.BusinessEntity is BusinessObject bizo && bizo.IsInDatabase)
				{
					var context = new DialogDefaultContext(
	new ZGuid("6F0C9011-13F8-44C4-9E7B-CCA3317A916C"),
	ResString.GetMultilingualString("09AD763D-C4D0-46C0-9CE3-1DD2C35BA303", "Confirm Form Reload?"),
	ZMessageBoxButtons.YesNo,
	ZMessageBoxIcon.Warning,
	null,
	showCheckboxOnly: true,
	resultsNotToSave: new[] { ZDialogResult.No });

					var message = ResString.GetMultilingualString("09EAE2C0-081D-4B6A-A070-37457D378AF7", @"Reloading will close and reopen this form.
You will have the option to save or discard any changes.

Do you wish to continue?");

					if (Globals.Message.ShowOrDefault(context, message) == ZDialogResult.Yes)
					{
						zForm.ReloadForm();
					}
				}
				else
				{
					Globals.Message.Show(
						ResString.GetMultilingualString("C3FF792D-EA1B-46EB-BFEC-282720A41A79", "This form cannot be reloaded as it has never been saved."),
						ResString.GetMultilingualString("35B4EA34-68CE-4299-ABD9-7D5AE089616C", "Improper Reload"),
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);
				}
			}
		}

		#endregion

		#region Web Hyperlinks

		public static bool WebHyperlinksEnabled => DataRegistry.Instance.WebHyperlinksEnabled && !string.IsNullOrEmpty(WebDataRegistry.Instance.RootServicesUri.Value);

		#endregion

		#region Service Hyperlinks

		public static string WebVersionLaunchUrl => DataRegistry.Instance.WebVersionLaunchUrl;

		public static string EnterpriseServicesUrl => WebDataRegistry.Instance.RootServicesUri.Value;

		#endregion

	}
}
