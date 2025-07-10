using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UserPortal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public static class SalesDiscoveryConductorUrlHelper
	{
		public const string HashCodeQueryStringKey = "HashCode";
		public const string InquiryIDQueryStringKey = "InquiryID";
		public const string OpportunityIDQueryStringKey = "OpportunityID";
		public const string OrgCodeQueryStringKey = "OrgCode";
		public const string OrgNameQueryStringKey = "OrgName";

		public static Uri GetSalesDiscoveryConductorUrl(SalesEnquiry inquiry)
		{
			if (inquiry == null)
			{
				throw new ArgumentNullException(nameof(inquiry), "Cannot create a URL for a null inquiry");
			}

			var organization = inquiry.Factory.Load<OrgHeader>(inquiry.OrgPk);
			var orgName = organization != null ? organization.OH_FullName : inquiry.CompanyName;

			return GetSalesDiscoveryConductorUrl(inquiry.O1_LeadUniqueReference, ZString.Empty, inquiry.OrgCode, orgName);
		}

		public static Uri GetSalesDiscoveryConductorUrl(OrgOpportunity opportunity)
		{
			if (opportunity == null)
			{
				throw new ArgumentNullException(nameof(opportunity), "Cannot create a URL for a null opportunity");
			}

			var inquiry = opportunity.Factory.Load<SalesEnquiry>(opportunity.P8_O1_Enquiry);
			var organization = opportunity.Factory.Load<OrgHeader>(opportunity.P8_OH);

			return GetSalesDiscoveryConductorUrl(inquiry?.O1_LeadUniqueReference, opportunity.P8_OpportunityID, organization?.OH_Code, organization?.OH_FullName);
		}

		static Uri GetSalesDiscoveryConductorUrl(string inquiryID, string opportunityID, string orgCode, string orgName)
		{
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			if (!string.IsNullOrEmpty(inquiryID))
			{
				list.Add(new KeyValuePair<string, string>(InquiryIDQueryStringKey, inquiryID));
			}

			if (!string.IsNullOrEmpty(opportunityID))
			{
				list.Add(new KeyValuePair<string, string>(OpportunityIDQueryStringKey, opportunityID));
			}

			if (!string.IsNullOrEmpty(orgCode))
			{
				list.Add(new KeyValuePair<string, string>(OrgCodeQueryStringKey, orgCode));
			}

			if (!string.IsNullOrEmpty(orgName))
			{
				list.Add(new KeyValuePair<string, string>(OrgNameQueryStringKey, orgName));
			}

			return GetURL(list.ToArray());
		}

		static string GetBase64HashCode(string text)
		{
			using (var sha256 = SHA256.Create())
			{
				var inputBytes = Encoding.UTF8.GetBytes(text);
				var base64String = Convert.ToBase64String(sha256.ComputeHash(inputBytes));
				return WebUtility.UrlEncode(base64String);
			}
		}

		static Uri GetURL(params KeyValuePair<string, string>[] additionalQueryStrings)
		{
			string queryString = string.Empty;
			foreach (KeyValuePair<string, string> pair in additionalQueryStrings)
			{
				queryString = queryString + pair.Key + "=" + WebUtility.UrlEncode(pair.Value) + "&";
			}

			if (!string.IsNullOrEmpty(queryString))
			{
				var decodedQueryString = WebUtility.UrlDecode(queryString);
				var hashCode = GetBase64HashCode(decodedQueryString);
				queryString = queryString + HashCodeQueryStringKey + "=" + hashCode;
			}

			try
			{
				UriBuilder uriBuilder = new UriBuilder(EDIDataRegistry.Instance.SalesConductorDomainUrl.Value);
				uriBuilder.Query = queryString;
				return uriBuilder.Uri;
			}
			catch (UriFormatException e)
			{
				throw new UriFormatException(ResString.GetMultilingualString("824f30e9-1c6a-42ac-9c93-81e513b82d45", "Can't create Sales Discovery Conductor link. Please verify value of the registry item '{0}'.", EDIDataRegistry.Instance.SalesConductorDomainUrl.Name), e);
			}
		}

		[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		public static ZMenuItem SalesMenuItem(CargoWise.EntityFramework.IBusiness businessEntity)
		{
			if (businessEntity == null)
			{
				throw new ArgumentNullException(nameof(businessEntity), "Cannot create a URL for a null businessEntity");
			}

			if (!(businessEntity is SalesEnquiry || businessEntity is OrgOpportunity))
			{
				throw new ArgumentException("Can only create URLs for SalesEnquiries and OrgOpportunities", nameof(businessEntity));
			}

			var result = new ZMenuItem(SalesConductorName);
			var inquiry = businessEntity as SalesEnquiry;
			var opportunity = businessEntity as OrgOpportunity;

			result.MenuItems.Add(new ZMenuItem(SalesConductorContentPlaylistBuilder, delegate
			{
				LaunchUserPortal(EDIDataRegistry.Instance.SalesConductorContentPlaylistBuilderUrl.Value);
			}));

			if (inquiry != null)
			{
				result.MenuItems.Add(new ZMenuItem(SalesDiscoveryName, delegate
				{
					var url = GetSalesDiscoveryConductorUrl(inquiry);
					LaunchUserPortal(url.AbsoluteUri);
				}));
			}
			else
			{
				result.MenuItems.Add(new ZMenuItem(SalesDiscoveryName, delegate
				{
					var url = GetSalesDiscoveryConductorUrl(opportunity);
					LaunchUserPortal(url.AbsoluteUri);
				}));
			}

			return result;
		}

		static void LaunchUserPortal(string url)
		{
			LastUrlLaunched = url;
			new UserPortalLauncher().LaunchUserPortal("", url);
		}

		public static MultilingualString SalesConductorName => ResString.GetMultilingualString("7bbab4dd-bd27-48fa-b09b-3ed47dcf22eb", "Sales Conductor");
		public static MultilingualString SalesDiscoveryName => ResString.GetMultilingualString("0fa548cf-97b1-4dbf-aa64-c9477a3a9987", "&Sales Discovery");
		public static MultilingualString SalesConductorContentPlaylistBuilder => ResString.GetMultilingualString("91bad932-47bc-4acd-bc7d-6e12c7484582", "&Sales Content Play List Builder");

		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		internal static string LastUrlLaunched { get; private set; }

		internal static void ClearLastUrlLaunched()
		{
			LastUrlLaunched = string.Empty;
		}
	}
}

