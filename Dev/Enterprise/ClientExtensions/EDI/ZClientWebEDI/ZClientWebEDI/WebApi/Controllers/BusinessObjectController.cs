using System;
using System.Net;
using System.Net.Http;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class BusinessObjectController : ControllerWithEnvironment
	{
		protected HttpResponseMessage UpdateBusinessObject<T>(JObject jsonObject, string routeName, Action<T> action)
			where T : class, IBusiness
		{
			if (Request == null)
			{
				return new HttpResponseMessage(HttpStatusCode.Forbidden);
			}

			if (!TryGetPk(jsonObject, out var pk, out var error))
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, $"{error} - {routeName}");
			}

			T bizO = (T)(object)Factory.Load(typeof(T), pk);
			if (bizO == null)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotFound,
					Invariant($"{typeof(T).Name} '{pk}' could not be found - {routeName}"));
			}

			action(bizO);

			return Request.CreateResponse(HttpStatusCode.OK);
		}

		bool TryGetPk(JObject jsonObject, out ZGuid pk, out string error)
		{
			pk = default;

			var jsonToken = jsonObject["PK"];
			if (jsonToken == null)
			{
				error = "Request must contain an object identifier";
				return false;
			}

			if (ZGuid.TryParse(jsonToken.ToString(), out pk))
			{
				error = null;
				return true;
			}

			error = Invariant($"'{jsonToken}' is not a valid identifier");
			return false;
		}

		protected BusinessObjectFactory Factory
		{
			get
			{
				using (Db.DisposableActionForDbConnection())
				{
					return factory ?? (factory = new BusinessObjectFactory());
				}
			}
		}
		BusinessObjectFactory factory;
	}
}
