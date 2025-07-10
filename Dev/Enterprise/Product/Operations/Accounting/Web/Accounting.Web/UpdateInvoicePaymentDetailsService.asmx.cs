#if NETFRAMEWORK
using System.Web.Services;
using System.Web.Services.Protocols;
#elif NET
using CoreWCF;
#endif
using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Accounting.Web.Business;
using Enterprise.Accounting.Web.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Web
{
#if NET
	[ServiceContract(Namespace = "http://cargowise.com/Accounting/"), XmlSerializerFormat]
	public interface IUpdateInvoicePaymentDetailsService
	{
		[OperationContract(Action = "http://cargowise.com/Accounting/UpdateInvoicePaymentDetails")]
		public UpdateInvoicePaymentDetailsResponse UpdateInvoicePaymentDetails(UpdateInvoicePaymentDetailsRequest request);
	}
#endif

	/// <summary>
	/// Summary description for UpdateInvoicePaymentDetailsService
	/// </summary>
#if NETFRAMEWORK
	[WebService(Namespace = "http://cargowise.com/Accounting/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	// [System.Web.Script.Services.ScriptService]
	public class UpdateInvoicePaymentDetailsService : WebService
	{
		public SecuritySOAPHeader SecurityHeader;
#elif NET
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, Namespace = "http://cargowise.com/Accounting/", AddressFilterMode = AddressFilterMode.Any, IncludeExceptionDetailInFaults = true)]
	public class UpdateInvoicePaymentDetailsService : BaseService, IUpdateInvoicePaymentDetailsService
	{
#endif
#if NETFRAMEWORK
		[WebMethod]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
#endif
		public UpdateInvoicePaymentDetailsResponse UpdateInvoicePaymentDetails(UpdateInvoicePaymentDetailsRequest request)
		{
			var result = new UpdateInvoicePaymentDetailsResponse();

			try
			{
				using (Db.DisposableActionForDbConnection())
				{
					result.OriginalRequest = request;

					Initialise();

					using (var connection = DbAccess.NewConnection())
					{
						var dataAccess = CreateDataAccess(connection);

						result.ErrorMessage = request.ValidateAll(dataAccess);
						if (!string.IsNullOrEmpty(result.ErrorMessage))
						{
							result.Succeeded = false;
							return result;
						}

						if (!ServiceEnabled.ContainsKey(request.CompanyCode))
						{
							ServiceEnabled[request.CompanyCode] = dataAccess.GetBoolRegistryValue("ENABLEINVOICEPAYMENTWEBSERVICE", request.CompanyCode);
						}

						if (!ServiceEnabled[request.CompanyCode])
						{
							result.Succeeded = false;
							result.ErrorMessage = string.Format((NoResString)"Currently this service is disabled for the '{0}' company. To enable this Invoice Payment Web Service go to the 'Accounting > Web > Enable Invoice Payment Web Service' registry item.", request.CompanyCode);
							return result;
						}

						var loginError = dataAccess.ValidateLoginDetails(SecurityHeader.UserName, SecurityHeader.Password);
						if (!string.IsNullOrEmpty(loginError))
						{
							result.Succeeded = false;
							result.ErrorMessage = loginError;
							System.Threading.Thread.Sleep(3000);
						}
						else
						{
							var updater = new InvoicePaymentDetailsUpdater(dataAccess);
							result = updater.PayTransaction(request);
						}
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

		static Dictionary<string, bool> ServiceEnabled
		{
			get { return serviceEnabled ?? (serviceEnabled = new Dictionary<string, bool>()); }
		}

		[ThreadStatic]
		static Dictionary<string, bool> serviceEnabled;

		protected virtual TransactionPaymentDataAccess CreateDataAccess(System.Data.Common.DbConnection connection)
		{
			return new TransactionPaymentDataAccess(connection, null);
		}

		void Initialise()
		{
			EnterpriseApplicationConfiguration.ConfigureObjectFactory();
		}
	}
}
