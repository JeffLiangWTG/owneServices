using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina
{
	public interface IAuthRequestBuilder
	{
		XStreamingElement BuildXML(TransactionInfo transaction);
	}

	class AuthRequestBuilder : IAuthRequestBuilder
	{
		public AuthRequestBuilder()
		{
			Helper = ObjectFactory.Get<IEInvoicingDependencyFactory>().GetTransactionInfoHelper();
		}

		readonly ITransactionInfoHelper Helper;

		XStreamingElement IAuthRequestBuilder.BuildXML(TransactionInfo transaction)
		{
			var cuit = Helper.GetRegistrationCode(transaction.BranchAddress, CountryCodes.Argentina, ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT);

			#region SuppressResourceStringsCheckRegion

			return new XStreamingElement("authRequest",
							new XElement("token", ""),
							new XElement("sign", ""),
							new XElement("cuitRepresentada", ((ZString)cuit).RemoveNonNumericCharacters()));

			#endregion
		}
	}
}
