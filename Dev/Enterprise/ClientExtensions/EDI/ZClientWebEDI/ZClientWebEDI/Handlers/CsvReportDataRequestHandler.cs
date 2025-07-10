using System;
using System.Collections.Specialized;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.SessionState;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using ICSharpCode.SharpZipLib.Zip;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public abstract class CsvReportDataRequestHandler<T> : IHttpHandler, IReadOnlySessionState where T : DataRequestHelper, new()
	{
		#region IHttpHandler Members

		public void ProcessRequest(HttpContext context)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var lockTaken = false;
				object user = null;

				try
				{
					if (context.Session != null)
					{
						user = context.Session["SiteUser"];
						if (user != null)
						{
							Monitor.Enter(user, ref lockTaken);
						}
					}

					if (!lockTaken)
					{
						if (context.Session == null)
						{
							ErrorReporter.ReportOnce("StreamDataRequestHandler", "The session is null.");
						}
						else if (user == null)
						{
							ErrorReporter.ReportOnce("StreamDataRequestHandler", "The user is null.");
						}
					}

					if (QueryString != null && QueryString.Count > 0)
					{
						ProcessRequestCore();
					}
					else
					{
						context.Response.Write("No data found.");
					}
				}
				catch (InvalidQueryStringException)
				{
					HttpContext.Current.Response.StatusCode = 400;
					HttpContext.Current.Response.End();
				}
				catch (ThreadAbortException)
				{ }
				finally
				{
					try
					{
						queryString = null;
						secureQueryString = null;
					}
					finally
					{
						if (lockTaken && user != null)
						{
							Monitor.Exit(user);
						}
					}
				}
			}
		}

		void ProcessRequestCore()
		{
			string fileName = SecureQueryString[StlUsageReportRequestHelper.Constants.FileName];
			string fileType = SecureQueryString[StlUsageReportRequestHelper.Constants.FileType];

			var fullFileName = fileName + "." + fileType;
			var compressedFileName = fileName + ".zip";

			var response = HttpContext.Current.Response;
			response.ContentType = DataContentTypes.Zip;
			var contentDispositionType = nameof(ContentDispositionType.Attachment).ToUpperInvariant();
			response.AddHeader("Content-Disposition", FormattableString.Invariant($"{contentDispositionType}; filename=\"{compressedFileName}\""));

			var reportingBizO = GetReportingBizO();
			var bufferBuilder = new StringBuilder();
			int bufferSize = 15000;

			if (reportingBizO != null)
			{
				using (var compressStream = new ZipOutputStream(response.OutputStream))
				{
					var entry = new ZipEntry(fullFileName);
					compressStream.PutNextEntry(entry);

					reportingBizO.GetCsvUsageReport(
						(csvLine) =>
						{
							bufferBuilder.AppendLine(csvLine);

							if (bufferBuilder.Length >= bufferSize)
							{
								if (response.IsClientConnected)
								{
									var data = Encoding.ASCII.GetBytes(bufferBuilder.ToString());
									compressStream.Write(data, 0, data.Length);
									if (!ResponseFlushSafe(response))
									{
										return;
									}
									bufferBuilder = new StringBuilder();
								}
								else
								{
									response.End();
									return;
								}
							}
						});

					if (response.IsClientConnected)
					{
						if (bufferBuilder.Length > 0)
						{
							var remainingData = Encoding.ASCII.GetBytes(bufferBuilder.ToString());
							compressStream.Write(remainingData, 0, remainingData.Length);
						}
						compressStream.Close();

						if (!ResponseFlushSafe(response))
						{
							return;
						}
					}
				}
			}

			response.End();
		}

		static bool ResponseFlushSafe(HttpResponse response)
		{
			if (response.IsClientConnected)
			{
				try
				{
					response.Flush();
					return true;
				}
				catch (HttpException ex)
				{
					if (ex.ErrorCode == unchecked((int)0x800704CD))
					{
						response.End();
						return false;
					}
					throw;
				}
			}
			else
			{
				response.End();
				return false;
			}
		}

		protected abstract ICsvReportingBusinessObject GetReportingBizO();

		public bool IsReusable
		{
			get { return false; }
		}

		#endregion

		#region Factory

		public BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		#endregion

		#region DataRequestHelper

		public T RequestHelper
		{
			get { return requestHelper ?? (requestHelper = new T()); }
		}
		T requestHelper;

		#endregion

		#region QueryString

		protected SecureQueryString SecureQueryString
		{
			get { return secureQueryString ?? (secureQueryString = new SecureQueryString(QueryString[SecureQueryString.QueryStringKey])); }
		}
		SecureQueryString secureQueryString;

		protected NameValueCollection QueryString
		{
			get
			{
				if (queryString == null && HttpContext.Current.Request.QueryString != null)
				{
					queryString = RequestHelper.UseSecureQueryString ? new SecureQueryString(HttpContext.Current.Request[SecureQueryString.QueryStringKey]) : HttpContext.Current.Request.QueryString;
				}
				return queryString;
			}
		}
		NameValueCollection queryString;

		#endregion
	}
}
