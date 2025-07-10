using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZClientWebCargoWiseEDI.Services
{
	[DataContract(Namespace = "http://schemas.cargowise.com/")]
	public class ErrorData
	{
		public ErrorData(string reason, string detailedInformation)
		{
			Reason = reason;
			DetailedInformation = detailedInformation;
		}

		[DataMember]
		public string Reason { get; private set; }

		[DataMember]
		public string DetailedInformation { get; private set; }
	}

	[DataContract(Namespace = "http://schemas.cargowise.com/")]
	public class LoggedinUser
	{
		public LoggedinUser(Guid contactPk, bool isSuperUser)
		{
			ContactPk = contactPk;
			IsSuperUser = isSuperUser;
		}

		[DataMember]
		public Guid ContactPk { get; private set; }

		[DataMember]
		public bool IsSuperUser { get; private set; }
	}

	[ServiceContract(Namespace = "http://schemas.cargowise.com/")]
	public interface ILoginService
	{
		[OperationContract]
		LoggedinUser Login(string companyCode, string username, string password);

		[OperationContract]
		bool HasSecurityRight(Guid contactPk, bool isSuperUser, string securityRightName);
	}

	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	[ServiceBehavior(AddressFilterMode = AddressFilterMode.Any, ConcurrencyMode = ConcurrencyMode.Single, Namespace = "http://schemas.cargowise.com/")]
	public class LoginService : MyAccountWebServiceBase, ILoginService
	{
		#region Login

		public LoggedinUser Login(string companyCode, string username, string password)
		{
			LoggedinUser result = null;

			try
			{
				ValidateRequestIpAddress();

				var org = new BusinessObjectFactory().LoadFromNaturalKey<EDIOrgHeader>(OrgHeaderSchema.OH_Code, companyCode);
				if (org != null && !org.HasCurrentSupportContractOrNoActiveLicence)
				{
					string message = "We have encountered a problem with your account. Please <a href='mailto:Invoicing%26Licensing@cargowise.com'>email</a> Invoicing & Licencing for further information.";
					HandleError(message);
				}

				MyAccountWebUser webuser = new MyAccountWebUser();
				webuser.Login(companyCode, username, password);
				if (webuser.IsLoggedIn)
				{
					bool isSuperUser = username == User.SupportUserName;
					Guid contactPk = webuser.LoggedInUser.PK.ToGuid();
					result = new LoggedinUser(contactPk, isSuperUser);
				}
				else
				{
					result = new LoggedinUser(Guid.Empty, false);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException() && !(ex is FaultException<ErrorData>))
			{
				HandleError("Server Error", ex.Message + System.Environment.NewLine + ex.StackTrace);
				ErrorReporter.ReportOnce(ex.Message, ex);
			}

			return result;
		}

		#endregion

		#region Has Security Right

		public bool HasSecurityRight(Guid contactPk, bool isSuperUser, string securityRightName)
		{
			bool result = false;

			try
			{
				ValidateRequestIpAddress();

				WebSecurityRight securityRight = EDIWebSecurityRightsList.New().FirstOrDefault(right => right.Code == securityRightName);
				OrgContact contact = new BusinessObjectFactory().Load<OrgContact>(contactPk);

				if (securityRight == null)
				{
					HandleError("Security right does not exist");
				}
				else if (!isSuperUser && contact == null)
				{
					HandleError("Contact does not exist");
				}
				else if (isSuperUser)
				{
					result = true;
				}
				else
				{
					MyAccountWebUser webuser = new MyAccountWebUser();
					webuser.Login(contact.Header.OH_Code, contact.OC_Email, User.WebTransientPassword);
					result = webuser.AreSecurityRightsGranted(securityRight);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException() && !(ex is FaultException<ErrorData>))
			{
				HandleError("Server Error", ex.Message + System.Environment.NewLine + ex.StackTrace);
				ErrorReporter.ReportOnce(ex.Message, ex);
			}

			return result;
		}

		#endregion

	}
}
