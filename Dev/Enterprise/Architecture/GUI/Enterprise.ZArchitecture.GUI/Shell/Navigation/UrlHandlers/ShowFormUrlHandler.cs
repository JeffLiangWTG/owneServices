using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.URLHandler;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture
{
	[ImmutableObject(true)]
	public abstract class ShowFormUrlHandler : UrlHandler
	{
		protected ShowFormUrlHandler()
		{
		}

		public string Create(ControllerID controllerID, IBusiness bizo, ZGuid pk, IEnumerable<string> args = null)
		{
			Argument.NotNull(bizo, nameof(bizo));
			return Create(controllerID, pk, MustBeLoggedIntoCorrectCompany(controllerID, bizo), string.Empty, args);
		}

		public string Create(ControllerID controllerID, ZGuid pk, IEnumerable<string> args = null)
		{
			return Create(controllerID, pk, MustBeLoggedIntoCorrectCompany(controllerID, pk), string.Empty, args);
		}

		public string CreateWithoutApplicationContext(ControllerID controllerID, ZGuid pk)
		{
			return Create(controllerID, pk, false);
		}

		public string CreateWithSpecifiedLicenceCode(ControllerID controllerID, ZGuid pk, string licenceCode)
		{
			return Create(controllerID, pk, MustBeLoggedIntoCorrectCompany(controllerID, pk), licenceCode);
		}

		[SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Well-known language IDs can be safely lowercased.")]
		public string CreateWebTrampolineUri(ControllerID controllerID, ZGuid pk)
		{
			var formattable = (FormattableString)$"link/{ExpectedCommandText}/{controllerID.Name}/{pk}?"; // Building a URI, string is not localizable.

			var uri = FormatUriSafeInvariantString(formattable);

			if (MustBeLoggedIntoCorrectCompany(controllerID, pk))
			{
				uri += "LicenceCode=" + StaticCurrentFetcher.Instance.CurrentCompany.LicenceKeyIdentifier + "&";
			}

			if (!string.Equals(Res.CurrentLanguage, Res.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
			{
				uri += "lang=" + Res.CurrentLanguage.ToLowerInvariant() + "&";
			}

			uri = uri.TrimEnd(new[] { '?', '&' });

			var baseUrl = string.IsNullOrWhiteSpace(DataRegistry.Instance.WebVersionLaunchUrl) ? WebDataRegistry.Instance.RootServicesUri.Value : DataRegistry.Instance.WebVersionLaunchUrl;
			var fullUri = JoinUriSegments(baseUrl, uri);
			return fullUri;

			string FormatUriSafeInvariantString(FormattableString fs)
			{
				var arguments = fs.GetArguments();
				for (var i = 0; i < arguments.Length; i++)
				{
					var argumentString = arguments[i]?.ToString();

					if (argumentString != null)
					{
						argumentString = Uri.EscapeDataString(argumentString);
					}

					arguments[i] = argumentString;
				}

				return string.Format(CultureInfo.InvariantCulture, fs.Format, arguments);
			}

			string JoinUriSegments(string first, string second)
			{
				if (first[first.Length - 1] == '/' || second[0] == '/')
				{
					return first + second;
				}

				return first + "/" + second;
			}
		}

		public string CreateFromTrampolineData(string controllerName, Guid pk, string licenceCode)
		{
			#region SuppressResourceStringsCheckRegion

			var queryString = new QueryString();
			queryString.Add("Command", ExpectedCommandText);

			if (!string.IsNullOrEmpty(licenceCode))
			{
				queryString.Add("LicenceCode", licenceCode);
			}

			queryString.Add("ControllerID", controllerName);
			queryString.Add("BusinessEntityPK", pk.ToString());

			var launchUrl = RawDataRegistry.Instance.WebVersionLaunchUrl.Value;
			if (!string.IsNullOrEmpty(launchUrl))
			{
				queryString.Add("WebVersionLaunchUrl", launchUrl);
			}

			if (InstanceDetails.Current != null)
			{
				if(InstanceDetails.Current.Domain != null)
				{
					queryString.Add("Domain", InstanceDetails.Current.Domain);
					queryString.Add("Instance", InstanceDetails.Current.Instance);
				}

				if (InstanceDetails.ShouldAddDatabaseInfoToUrls)
				{
					queryString.Add("ServerName", InstanceDetails.Current.ServerName);
					queryString.Add("DatabaseName", InstanceDetails.Current.DatabaseName);
				}
			}

			queryString.Add("Hash", CreateQueryStringSecurityHash(queryString));

			return GetUrlFromQueryString(queryString);

			#endregion
		}

		string Create(ControllerID controllerID, ZGuid pk, bool beCompanySpecific, string licenceCode = "", IEnumerable<string> args = null)
		{
			#region SuppressResourceStringsCheckRegion

			var queryString = new QueryString();
			queryString.Add("Command", ExpectedCommandText);
			if (beCompanySpecific)
			{
				if (string.IsNullOrEmpty(licenceCode))
				{
					queryString.Add("LicenceCode", GetCurrentCompanyLicenceKeyIdentifier(StaticCurrentFetcher.Instance.CurrentCompany));
				}
				else
				{
					queryString.Add("LicenceCode", licenceCode);
				}
			}
			queryString.Add("ControllerID", controllerID.Name);
			queryString.Add("BusinessEntityPK", pk.ToString());
			queryString.Add("VersionNumber", new EnterpriseInformationRetriever().VersionNumber);
			if (InstanceDetails.Current != null)
			{
				if(InstanceDetails.Current.Domain != null)
				{
					queryString.Add("Domain", InstanceDetails.Current.Domain);
					queryString.Add("Instance", InstanceDetails.Current.Instance);
				}

				if (InstanceDetails.ShouldAddDatabaseInfoToUrls)
				{
					queryString.Add("ServerName", InstanceDetails.Current.ServerName);
					queryString.Add("DatabaseName", InstanceDetails.Current.DatabaseName);
				}
			}

			if (args != null)
			{
				var escapedArgs = GetEscapedArgs(args);
				queryString.Add(ArgsQueryStringIdentifier, string.Join(ArgsDelimiter.ToString(), escapedArgs));
			}

			queryString.Add("Hash", CreateQueryStringSecurityHash(queryString));

			return GetUrlFromQueryString(queryString);

			#endregion
		}

		#region UrlHandler Overrides

		protected override void CheckLoginState(QueryString queryString)
		{
			var licenceCode = queryString["LicenceCode"];
			if (licenceCode != null)
			{
				EnsureLoggedIntoCorrectCompany(licenceCode);
			}

			base.CheckLoginState(queryString);
		}

		protected void EnsureLoggedIntoCorrectCompany(ZString urlCreatedFromLicenceCode)
		{
			var currentCompanyLicenceKeyIdentifier = GetCurrentCompanyLicenceKeyIdentifier(CurrentCompany);
			if (urlCreatedFromLicenceCode != currentCompanyLicenceKeyIdentifier)
			{
				var urlCreatedFromCompany = NewFactory().LoadFromNaturalKey(ObjectFactory.GetType(typeof(IGlbCompany)), GlbCompanySchema.GC_Code, urlCreatedFromLicenceCode.SubstringSafe(3, 3));
				var correctCompany = urlCreatedFromCompany == null ? Res.GetString("f5f0457d-1064-471a-aad8-9dd116c52747", "correct") + " " : "'" + urlCreatedFromCompany[GlbCompanySchema.GC_Name] + "'";
				throw new EnterpriseUrlHandlerException(
					Res.GetString("5773d8c0-bf10-4b0b-9021-d3125a635992", @"{0} is not running in the correct company or from the correct licensed server installation directory.
Log into the {1} company and try again.", Constants.ProductName, correctCompany));
			}
		}

		protected override bool HandleCore(QueryString queryString)
		{
			var (controllerID, pk) = GetControllerIDAndBusinessEntityPk(queryString);
			var args = GetArgs(queryString);
			var isOpenFromWindowPersister = GetWindowPersisterStatus(queryString);
			if (controllerID == null)
			{
				return false;
			}
			else
			{
				ShowForm(controllerID, pk, args, isOpenFromWindowPersister);
				return true;
			}
		}

		static (ControllerID, Guid) GetControllerIDAndBusinessEntityPk(QueryString queryString)
		{
			var controllerIDAsString = queryString["ControllerID"];
			var businessEntityPk = GetBusinessEntityPk(queryString);

			if (ClientHookLoader.Instance?.Client == Clients.EDI)
			{
				var ediHelper = ObjectFactory.Get<IEDIUrlHandlerHelper>();

				if (ediHelper.RequiresExtensionController(controllerIDAsString))
				{
					var (extensionControllerIDAsString, extensionbusinessEntityPk) = ediHelper.GetExtensionControllerIDAndBusinessEntityPk(controllerIDAsString, businessEntityPk);
					return (GetControllerID(extensionControllerIDAsString, queryString.Get("VersionNumber")), extensionbusinessEntityPk);
				}
			}

			return (GetControllerID(controllerIDAsString, queryString.Get("VersionNumber")), businessEntityPk);
		}

		internal static ControllerID GetControllerID(QueryString queryString) => GetControllerID(queryString["ControllerID"], queryString.Get("VersionNumber"));

		static ControllerID GetControllerID(string controllerIDAsString, string versionNumber = "")
		{
			ControllerID controllerID;

			if (controllerIDAsString == null)
			{
				controllerID = null;
			}
			else
			{
				controllerID = ZControllerFactory.Instance.GetRegisteredIdentifierByName(controllerIDAsString);
				if (controllerID == null)
				{
					throw new EnterpriseUrlHandlerException($"Record type is unknown to the current version of {Constants.ProductName}. The Controller Name is {controllerIDAsString}. " + (versionNumber.IsNullOrEmpty() ? "" : $"The Recommended Application version is {versionNumber}"));
				}
			}

			return controllerID;
		}

		static bool MustBeLoggedIntoCorrectCompany(ControllerID controllerID, ZGuid identifier)
		{
			var controller = ZControllerFactory.Create(controllerID);
			return controller == null || controller.MakeUrlOnlyOpenableForCurrentCompany(identifier);
		}

		static bool MustBeLoggedIntoCorrectCompany(ControllerID controllerID, IBusiness bizo)
		{
			var controller = ZControllerFactory.Create(controllerID);
			return controller == null || controller.MakeUrlOnlyOpenableForCurrentCompany(bizo);
		}

		protected internal override string[] GetQueryNamesSecuredBySecurityHash(QueryString queryString)
		{
			return new string[] { "LicenceCode", "ControllerID", "BusinessEntityPK" };
		}

		#endregion

		#region ShowForm

		protected void ShowForm(ControllerID controllerID, ZGuid pk, IEnumerable<string> args, bool isOpenFromWindowPersister)
		{
			try
			{
				ShowFormOnUIThread(controllerID, pk, args, isOpenFromWindowPersister);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (!(ex is EnterpriseUrlHandlerException))
				{
					ErrorReporter.ReportOnce(ex.Message, ex);
				}
#if WINZOR
				else  //Avoid exception form being displayed twice.
				{
					throw;
				}
#else
				throw;
#endif
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void ShowFormOnUIThread(ControllerID controllerID, ZGuid pk, IEnumerable<string> args, bool isOpenFromWindowPersister)
		{
			var form = PerformFormAction(controllerID, pk, args);
#if !WINZOR
			if (form != null)
			{
				Application.DoEvents();
				ForceFormToActivate(form, isOpenFromWindowPersister);
			}
#endif
		}

		protected abstract Form PerformFormAction(ControllerID controllerID, ZGuid pk, IEnumerable<string> args);

#endregion

		protected virtual bool IsReportError
		{
			get { return false; }
		}
	}
}
