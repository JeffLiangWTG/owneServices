#if NETFRAMEWORK
using System.Web.Services;
using System.Web.Services.Protocols;
#elif NET
using CoreWCF;
#endif
using System;
using CargoWise.Common;
using Enterprise.Accounting.Export.Business;
using Enterprise.Accounting.Web.Business;
using Enterprise.Accounting.Web.Core;

namespace Enterprise.Accounting.Web
{
#if NET
	[ServiceContract(Namespace = "http://cargowise.com/Accounting/"), XmlSerializerFormat]
	public interface ICreditLimitService
	{
		[OperationContract(Action = "http://cargowise.com/Accounting/GetCreditLimitAndBalanceDetails")]
		public CreditLimitAndBalanceResponse GetCreditLimitAndBalanceDetails(CreditLimitAndBalanceRequest request);
		[OperationContract(Action = "http://cargowise.com/Accounting/CheckTransactionPaymentStatus")]
		public TransactionPaymentStatusResponse CheckTransactionPaymentStatus(TransactionPaymentStatusRequest request);
	}
#endif

	/// <summary>
	/// Summary description for Service1
	/// </summary>
#if NETFRAMEWORK
	[WebService(Namespace = "http://cargowise.com/Accounting/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	// [System.Web.Script.Services.ScriptService]
	public class CreditLimitService : WebService
	{
		public SecuritySOAPHeader SecurityHeader;
#elif NET
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, Namespace = "http://cargowise.com/Accounting/", AddressFilterMode = AddressFilterMode.Any, IncludeExceptionDetailInFaults = true)]
	public class CreditLimitService : BaseService, ICreditLimitService
	{
#endif
#if NETFRAMEWORK
		[WebMethod]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
#endif
		public CreditLimitAndBalanceResponse GetCreditLimitAndBalanceDetails(CreditLimitAndBalanceRequest request)
		{
			CreditLimitAndBalanceResponse result = new CreditLimitAndBalanceResponse();

			try
			{
				result.ErrorMessage = request.Validate();
				if (!string.IsNullOrEmpty(result.ErrorMessage))
				{
					result.Succeeded = false;
					return result;
				}

				using (var conn = DbAccess.NewConnection())
				{
					var access = CreateBaseDataAccess(conn);

					string loginError = access.ValidateLoginDetails(SecurityHeader.UserName, SecurityHeader.Password);
					if (!string.IsNullOrEmpty(loginError))
					{
						result.Succeeded = false;
						result.ErrorMessage = loginError;
						System.Threading.Thread.Sleep(3000);
					}
					else
					{
						BaseDataAccess dataAccess = new BaseDataAccess(conn, null);
						result = new CreditLimitAndBalanceChecker(dataAccess).GetCreditLimitAndBalanceDetails(request);
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

#if NETFRAMEWORK
		[WebMethod]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
#endif
		public TransactionPaymentStatusResponse CheckTransactionPaymentStatus(TransactionPaymentStatusRequest request)
		{
			TransactionPaymentStatusResponse result = new TransactionPaymentStatusResponse();

			try
			{
				using (var conn = DbAccess.NewConnection())
				{
					var access = CreateTransactionPaymentDataAccess(conn);

					string loginError = access.ValidateLoginDetails(SecurityHeader.UserName, SecurityHeader.Password);
					if (!string.IsNullOrEmpty(loginError))
					{
						result.Succeeded = false;
						result.ErrorMessage = loginError;
					}
					else
					{
						BaseDataAccess dataAccess = new BaseDataAccess(conn, null);
						result = new CreditLimitAndBalanceChecker(dataAccess).GetTransactionPaymentStatus(request);
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

		protected virtual TransactionPaymentDataAccess CreateTransactionPaymentDataAccess(System.Data.Common.DbConnection conn)
		{
			return new TransactionPaymentDataAccess(conn);
		}

		protected virtual BaseDataAccess CreateBaseDataAccess(System.Data.Common.DbConnection conn)
		{
			return new BaseDataAccess(conn);
		}
	}
}
