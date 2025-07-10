
#pragma warning disable 1591

namespace Enterprise.Customs.GB.CNS.WebServices.CnsChiefEDI
{
	using System.Diagnostics.CodeAnalysis;

	[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:ParameterNamesMustBeginWithLowerCaseLetter", Justification = "Suppression for SOAP web methods")]
	[System.CodeDom.Compiler.GeneratedCode("System.Web.Services", "2.0.50727.3053")]
	[System.Diagnostics.DebuggerStepThrough()]
	[System.ComponentModel.DesignerCategory("code")]
	[System.Web.Services.WebServiceBinding(Name = "ChiefEDIPort", Namespace = "http://www.destin8.co.uk/Chief")]
	public partial class ChiefEDIPortQSService : System.Web.Services.Protocols.SoapHttpClientProtocol
	{
		[System.Web.Services.Protocols.SoapRpcMethod("", RequestNamespace = "http://www.destin8.co.uk/Chief", ResponseNamespace = "http://www.destin8.co.uk/Chief")]
		[return: System.Xml.Serialization.SoapElement("Response")]
		public string processEDIMessage(string EDI, string CompanyCode, bool Operational)
		{
			object[] results = this.Invoke("processEDIMessage", new object[] { EDI, CompanyCode, Operational });
			return ((string)(results[0]));
		}
	}
}

#pragma warning restore 1591
