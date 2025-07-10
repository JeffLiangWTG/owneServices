using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class UserAgreementEDocRequestHelper : DataRequestHelper
	{
		public override string BaseUrl
		{
			get { return "UserAgreementEDocRequestHandler.axd"; }
		}

		public override bool EnableCache
		{
			get { return false; }
		}

		public override bool UseSecureQueryString
		{
			get { return true; }
		}

		public string GetHandlerUrl(ZGuid pK, Guid uniqueKey, string token)
		{
			uniqueKeyForQueryString = uniqueKey;
			tokenForQueryString = token;

			return GetHandlerUrl(pK);
		}

		string tokenForQueryString;
		Guid uniqueKeyForQueryString;

		protected override IEnumerable<KeyValuePair<string, string>> InsertAdditionalParameters(IEnumerable<KeyValuePair<string, string>> queryStringValues)
		{
			var additionalParameters = new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(TokenKey, tokenForQueryString),
				new KeyValuePair<string, string>(UniqueKey, uniqueKeyForQueryString.ToString()),
			};

			return queryStringValues.Union(additionalParameters);
		}

		public const string TokenKey = "Token";
		public const string UniqueKey = "UniqueKey";
	}
}
