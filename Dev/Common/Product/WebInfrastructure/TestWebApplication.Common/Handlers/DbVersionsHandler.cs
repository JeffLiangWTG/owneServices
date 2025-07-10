using System.IO;
using System.Net;
using System.Web;
using CargoWise.Data;

namespace Enterprise.Web.TestWebApplication.Common.Handlers
{
	class DbVersionsHandler : IHttpHandler
	{
		public bool IsReusable => true;

		public void ProcessRequest(HttpContext context)
		{
			switch (Path.GetFileName(context.Request.Path))
			{
				case "GetSchemaVersions":
					ReturnDbSchemaVersions(context);
					break;

				case "GetCurrentVersion":
					ReturnCurrentVersion(context);
					break;

				default:
					ReturnValidTerms(context);
					break;
			}
		}

		static void ReturnDbSchemaVersions(HttpContext context)
		{
			using var disposable = Db.DisableSchemaVersionCheck();

			ResponseWriter.WriteResponse(
				context,
				$"DbSchemaVersion={DbRegistry.DatabaseMajorSchemaVersion.LoadValue(Db.Connection)}.{DbRegistry.DatabaseMinorSchemaVersion.LoadValue(Db.Connection)}");
		}

		static void ReturnCurrentVersion(HttpContext context)
		{
			using var disposable = Db.DisableSchemaVersionCheck();
			var packageVersion = Db.Connection.ExecuteScalar<string>(@"
SELECT
	convert(varchar(2), SZ_MajorVersion) + '.' + convert(varchar(2), SZ_MinorVersion) + '.' + convert(varchar(2), SZ_Release) + '.' + convert(varchar(2), SZ_Patch)
FROM
	[StmUpgrade]
WHERE
	SZ_Status = 'CUR'
");

			ResponseWriter.WriteResponse(
				context,
				$"CurrentVersion={packageVersion}");
		}

		static void ReturnValidTerms(HttpContext context)
		{
			const string message = @"Path invalid! valid path options are:
GetSchemaVersions
GetCurrentVersion
";
			context.Response.ClearHeaders();
			ResponseWriter.WriteResponse(
				context,
				message,
				HttpStatusCode.MethodNotAllowed);
		}
	}
}
