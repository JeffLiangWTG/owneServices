#if NETFRAMEWORK
using System.Web.Services;
using System.Web.Services.Protocols;
#elif NET
using CoreWCF;
#endif
using System;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Accounting.Export.Business;
using Enterprise.Accounting.Web.Business;
using Enterprise.Accounting.Web.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Web
{
#if NET
	[ServiceContract(Namespace = "http://cargowise.com/Accounting/"), XmlSerializerFormat]
	public interface IAccountingTransactionExportService
	{
		[OperationContract(Action = "http://cargowise.com/Accounting/CreateBatch")]
		public AccountingTransactionExportResponse CreateBatch(AccountingTransactionCreateBatchRequest request);

		[OperationContract(Action = "http://cargowise.com/Accounting/ExportBatch")]
		public AccountingTransactionExportResponse ExportBatch(AccountingTransactionExportRequest request);
	}

#endif

	/// <summary>
	/// Summary description for AccountingTransactionExportService
	/// </summary>
#if NETFRAMEWORK
	[WebService(Namespace = "http://cargowise.com/Accounting/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	// [System.Web.Script.Services.ScriptService]
	public class AccountingTransactionExportService : WebService
	{
		public SecuritySOAPHeader SecurityHeader;
#elif NET
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, Namespace = "http://cargowise.com/Accounting/", AddressFilterMode = AddressFilterMode.Any, IncludeExceptionDetailInFaults = true)]
	public class AccountingTransactionExportService : BaseService, IAccountingTransactionExportService
	{
#endif
#if NETFRAMEWORK
		[WebMethod]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
#endif
		public AccountingTransactionExportResponse CreateBatch(AccountingTransactionCreateBatchRequest request)
		{
			using (Db.DisposableActionForDbConnection())
			{
				AccountingTransactionExportResponse result = new AccountingTransactionExportResponse();
				try
				{
					if (request == null)
					{
						throw new ArgumentNullException(nameof(request));
					}

					if (SecurityHeader == null)
					{
						throw new Exception("You must provide a SOAP Header: SecurityHeader.  It should contain the UserName and Password.");
					}

					result.ErrorMessage = request.Validate();

					if (!string.IsNullOrEmpty(result.ErrorMessage))
					{
						result.Succeeded = false;
						return result;
					}

					using (var connection = DbAccess.NewConnection())
					{
						BatchExportDataAccess dataAccess = new BatchExportDataAccess(connection, null);

						string loginError = dataAccess.ValidateLoginDetails(SecurityHeader.UserName, SecurityHeader.Password);

						if (!string.IsNullOrEmpty(loginError))
						{
							result.Succeeded = false;
							result.ErrorMessage = loginError;
							System.Threading.Thread.Sleep(3000);
						}
						else
						{
							AccountingTransactionWebExporter exporter = new AccountingTransactionWebExporter(dataAccess);
							result = exporter.CreateBatch(request.CompanyCode);
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					result.Succeeded = false;
					result.ErrorMessage = ErrorHelper.ReportError(ex);
				}

				return result;
			}
		}

#if NETFRAMEWORK
		[WebMethod]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
#endif
		public AccountingTransactionExportResponse ExportBatch(AccountingTransactionExportRequest request)
		{
			using (Db.DisposableActionForDbConnection())
			{
				if (Globals.IsTest)
				{
					Db.Connection.ExecuteScalar((NoResString)"SELECT 'OK'");
				}

				AccountingTransactionExportResponse result = new AccountingTransactionExportResponse();
				try
				{
					result.Succeeded = false;
					if (request == null)
					{
						throw new ArgumentNullException(nameof(request));
					}

					if (SecurityHeader == null)
					{
						throw new Exception("You must provide a SOAP Header: SecurityHeader.  It should contain the UserName and Password.");
					}

					Initialise();

					result.ErrorMessage = request.Validate();

					if (!string.IsNullOrEmpty(result.ErrorMessage))
					{
						return result;
					}

					using (var connection = DbAccess.NewConnection())
					{
						BatchExportDataAccess dataAccess = new BatchExportDataAccess(connection, null);

						string loginError = dataAccess.ValidateLoginDetails(SecurityHeader.UserName, SecurityHeader.Password);

						if (!string.IsNullOrEmpty(loginError))
						{
							result.ErrorMessage = loginError;
							System.Threading.Thread.Sleep(3000);
						}
						else
						{
							AccountingTransactionWebExporter exporter = new AccountingTransactionWebExporter(dataAccess);
							result = exporter.ExportBatch(request.CompanyCode, request.BatchNumber, request.Namespace);
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					result.Succeeded = false;
					result.ErrorMessage = ErrorHelper.ReportError(ex);
				}

				return result;
			}
		}

		void Initialise()
		{
			EnterpriseApplicationConfiguration.ConfigureObjectFactory();
		}
	}
}
