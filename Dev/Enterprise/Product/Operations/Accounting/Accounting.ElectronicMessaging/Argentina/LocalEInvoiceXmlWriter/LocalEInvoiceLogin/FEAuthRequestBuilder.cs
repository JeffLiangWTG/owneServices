
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina
{
	public interface IFEAuthRequestBuilder
	{
		XStreamingElement BuildFEAuthRequestInfo(TransactionInfo transaction, XNamespace xNameSpace, ZString registrationNumber);
	}

	class FEAuthRequestBuilder : IFEAuthRequestBuilder
	{
		#region SuppressResourceStringsCheckRegion

		XStreamingElement IFEAuthRequestBuilder.BuildFEAuthRequestInfo(TransactionInfo transaction, XNamespace xNameSpace, ZString registrationNumber)
		{
			Argument.NotNull(transaction, nameof(transaction));

			return new XStreamingElement(xNameSpace + "Auth",
							new XElement(xNameSpace + "Token", ""),
							new XElement(xNameSpace + "Sign", ""),
							new XElement(xNameSpace + "Cuit", registrationNumber.RemoveNonNumericCharacters()));
		}

		#endregion
	}
}
