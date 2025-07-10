//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiIdentityRedirectUrlValidation
//
//    This class should be used for overriding validation in AutoEdiIdentityRedirectUrlValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;

namespace Enterprise.Client.EDI.IdentityRedirectUrl.Business
{
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Schema;
	using Res = ZClientEDI.Business.Res;

	public class EdiIdentityRedirectUrlValidation : AutoEdiIdentityRedirectUrlValidation
	{
		public EdiIdentityRedirectUrlValidation(AutoEdiIdentityRedirectUrl parent) : base(parent)
		{
		}

		EdiIdentityRedirectUrl RedirectUrl => redirectUrl ??= (EdiIdentityRedirectUrl)Parent;
		EdiIdentityRedirectUrl redirectUrl;

		protected override void CheckIAR_RedirectUrl()
		{
			base.CheckIAR_RedirectUrl();
			var url = RedirectUrl.IAR_RedirectUrl;
			var isValid = Uri.TryCreate(url, UriKind.Absolute, out var uri);
			if (!isValid || !IsValidAuthority(uri))
			{
				RedirectUrl.IAR_RedirectUrlInfo.AddError(InvalidUrlError);
				return;
			}

			if (url.Contains("*"))
			{
				RedirectUrl.IAR_RedirectUrlInfo.AddError(WildcardCharacterError);
			}

			if (RedirectUrl.IAR_RedirectType == EdiIdentityRedirectType.Codes.SinglePage || RedirectUrl.IAR_RedirectType == EdiIdentityRedirectType.Codes.Web)
			{
				//Check Scheme
				if (!IsValidScheme(uri))
				{
					RedirectUrl.IAR_RedirectUrlInfo.AddError(WebOrSpaSchemeError);
				}

				if (uri.Scheme == Uri.UriSchemeHttp && !string.IsNullOrEmpty(uri.Host) && !uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
				{
					RedirectUrl.IAR_RedirectUrlInfo.AddError(WebOrSpaSchemeError);
				}
			}
			else
			{
				if (!IsValidScheme(uri))
				{
					RedirectUrl.IAR_RedirectUrlInfo.AddError(InstalledClientSchemeError);
				}
			}

			var filter = new ZQuery(EdiIdentityRedirectUrlSchema.IAR_IDA, RedirectUrl.IAR_IDA);
			filter.AddToFilter(EdiIdentityRedirectUrlSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			filter.AddToFilter(EdiIdentityRedirectUrlSchema.IAR_RedirectUrl, RedirectUrl.IAR_RedirectUrl);
			var isInDatabase = Parent.Factory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(EdiIdentityRedirectUrl)), filter);

			if (isInDatabase)
			{
				RedirectUrl.IAR_RedirectUrlInfo.AddError(Res.GetString("8CFA7728-E645-40FF-ACE2-5F11701A1A98", "This URL already exists."));
			}
		}

		bool IsValidScheme(Uri uri)
		{
			if (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
			{
				return true;
			}
			return false;
		}

		bool IsValidAuthority(Uri uri)
		{
			//The IP6 loopback address is not allowed
			if ((uri.IsLoopback && uri.HostNameType == UriHostNameType.IPv6) || uri.Authority.Length <= 1)
			{
				return false;
			}
			return true;
		}

		protected override void CheckIAR_RedirectType()
		{
			base.CheckIAR_RedirectType();
			MandatoryValidation.CheckEntered(RedirectUrl.IAR_RedirectTypeInfo);
			ListValidation.ErrorIfInvalidCode(RedirectUrl.IAR_RedirectTypeInfo);
		}

		static string InvalidUrlError => Res.GetString("58A18CCC-6E00-462A-8715-B99A285A16E1", "Please enter the valid URL.");

		static string InstalledClientSchemeError => Res.GetString("7AC846AF-6ABF-4640-89EC-1302523829B9", "ICL URL must start with HTTP、HTTPS.");

		static string WebOrSpaSchemeError => Res.GetString("60B29838-BF84-4ACB-A186-05A90A6446CF", "Web/Spa URL must start with https or http://localhost.");

		static string WildcardCharacterError => Res.GetString("97C4F734-1132-47A2-866E-69D9F07AE06F", "Does not contain wildcard character.");
	}
}
