#if NETFRAMEWORK
using System.Web.Services.Protocols;
#elif NET
using System.Runtime.Serialization;
#endif

namespace Enterprise.Accounting.Web
{
	/// <summary>
	/// Header for SOAP Messages that includes authentication information
	/// </summary>
#if NETFRAMEWORK
	public class SecuritySOAPHeader : SoapHeader
#elif NET
	[DataContract(Namespace = "http://cargowise.com/Accounting/")]
	public class SecuritySOAPHeader
#endif
	{
		#region Constructors

		public SecuritySOAPHeader()
		{
			this.userName = "";
			this.password = "";
		}

		#endregion

		#region Properties

#if NET
		[DataMember(Order = 0)]
#endif
		public string UserName
		{
			get { return this.userName; }
			set { this.userName = value; }
		}

#if NET
		[DataMember(Order = 1)]
#endif
		public string Password
		{
			get { return this.password; }
			set { this.password = value; }
		}

		#endregion

		#region Implementation

		string userName;
		string password;

		#endregion
	}
}
