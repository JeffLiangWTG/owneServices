using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using CargoWise.Data;
using Enterprise.Client.EDI;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class HealthCheckHandler : IHttpHandler
	{
		public bool IsReusable => false;

		public void ProcessRequest(HttpContext context)
		{
			ProcessRequest(new HttpContextWrapper(context));
		}

		public void ProcessRequest(HttpContextBase context)
		{
			var request = context.Request;
			var response = context.Response;

			if (!string.Equals(request.HttpMethod, HttpMethod.Head.Method, StringComparison.OrdinalIgnoreCase)
				&& !string.Equals(request.HttpMethod, HttpMethod.Get.Method, StringComparison.OrdinalIgnoreCase))
			{
				response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
				return;
			}

			bool isDbOnline = false;
			var message = EDIConstants.DatabaseConnectionStatus.Ok;
			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					Db.Connection.EnsureIsOpen();
					isDbOnline = true;
				}
				catch (DatabaseUpgradeInProgressException)
				{
					isDbOnline = false;
					message = EDIConstants.DatabaseConnectionStatus.Error;
				}
			}

			bool isDbVersionValid = false;
			if (isDbOnline)
			{
				isDbVersionValid =
						SchemaVersion.Application.CompareTo(Env.Registry.DatabaseMajorSchemaVersion, Env.Registry.DatabaseMinorSchemaVersion) == 0
						&& DataVersion.Application.CompareTo(Env.Registry.DatabaseSystemDataVersionMajor, Env.Registry.DatabaseSystemDataVersionMinor) == 0
						&& ClientDllChecker.CheckRegistry().IsOKToRun;
				if (!isDbVersionValid)
				{
					message = EDIConstants.DatabaseConnectionStatus.VersionMismatch;
				}
			}

			response.StatusCode = isDbOnline && isDbVersionValid ? (int)HttpStatusCode.OK : (int)HttpStatusCode.ServiceUnavailable;
			response.CacheControl = "no-cache";
			var output = response.OutputStream;
			using (var writer = new StreamWriter(output, Encoding.UTF8))
			{
				writer.Write(message);
			}
		}
	}
}
