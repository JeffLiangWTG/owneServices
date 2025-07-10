using System;
using System.Globalization;
using System.Net;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Integration.Licensing;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Recruiter.Business.LearningCentreTestResultHelper;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.LearningCenter
{
	public static class LearningCentreJobSkillHelper
	{
		static ResourceString UriFormatErrorMessage { get; } = ResString.GetMultilingualString("F33B2436-C76D-42EF-85C2-E9A9FF09F9B1", "Can't create Web Certification link. Please verify value of the registry item '{0}'.", WebDataRegistry.Instance.WebCertificationUrl.Caption);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings")]
		public static string GetJobSkillUrl(string staffCode, string jobSkillCode = "")
		{
			var factory = new BusinessObjectFactory();
			EDIOrgContact contact = GetPrimaryContact(staffCode, factory);

			if (contact == null)
			{
				return string.Empty;
			}

			SecureQueryString queryString = new SecureQueryString();
			queryString.Add(OrgContactSchema.Constants.Prefix, contact.PK.ToString());

			try
			{
				var uriBuilder = new UriBuilder(WebDataRegistry.Instance.WebCertificationUrl.Value);
				uriBuilder.Path = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", uriBuilder.Path.TrimEnd('/'), "login.aspx");
				uriBuilder.Query = string.Format(CultureInfo.InvariantCulture, "{0}={1}", SecureQueryString.QueryStringKey, WebUtility.UrlEncode(queryString.ToString()));
				return uriBuilder.Uri.AbsoluteUri;
			}
			catch (UriFormatException e)
			{
				throw new UriFormatException(UriFormatErrorMessage, e);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings")]
		public static string GetJobSkillUrlNoLogin(string jobSkillCode)
		{
			SecureQueryString queryString = new SecureQueryString();

			try
			{
				var uriBuilder = new UriBuilder(WebDataRegistry.Instance.WebCertificationUrl.Value);
				uriBuilder.Path = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", uriBuilder.Path.TrimEnd('/'), "login.aspx");
				uriBuilder.Query = string.Format(CultureInfo.InvariantCulture, "{0}={1}", SecureQueryString.QueryStringKey, WebUtility.UrlEncode(queryString.ToString()));
				return uriBuilder.Uri.AbsoluteUri;
			}
			catch (UriFormatException e)
			{
				throw new UriFormatException(UriFormatErrorMessage, e);
			}
		}

		public static JobSkillProgressDetails GetJobSkillProgressDetails(string staffCode, string skillCode)
		{
			var factory = new BusinessObjectFactory();
			EDIOrgContact contact = GetPrimaryContact(staffCode, factory);

			if (contact == null)
			{
				return new JobSkillProgressDetails();
			}

			return LearningCentreTestResultHelper.GetJobSkillProgressDetails(contact.PK.ToGuid(), skillCode);
		}

		static EDIOrgContact GetPrimaryContact(string staffCode, BusinessObjectFactory factory)
		{
			var dbNum = ObjectFactory.Get<IProductRegistration>().Key.DatabaseNumber;

			var query = new ZDBOnlyQuery(typeof(EDIOrgContact));
			var userAccountQuery = new ZDBOnlySubQuery(typeof(EdiCustomerUserAccount), EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact);
			userAccountQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_UserID, staffCode);

			var licDatabaseQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), EdiCustomerUserAccountSchema.EUA_LD);
			licDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_DatabaseNumber, dbNum);

			userAccountQuery.AddSubQuery(licDatabaseQuery, JoinCondition.And);
			query.AddSubQuery(userAccountQuery, JoinCondition.And);

			return factory.LoadTop1<EDIOrgContact>(query);
		}
	}
}
