using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.Application;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	public interface ICFDiObtenerPDFBuilder
	{
		XStreamingElement BuildXml(List<AuthorizationDetails> authorizationDetails);
	}

	class CFDiObtenerPDFBuilder : ICFDiObtenerPDFBuilder
	{
		readonly ITransactionInfoHelper Helper;

		public CFDiObtenerPDFBuilder()
		{
			Helper = ObjectFactory.Get<IEInvoicingDependencyFactory>().GetTransactionInfoHelper();
		}

		XStreamingElement ICFDiObtenerPDFBuilder.BuildXml(List<AuthorizationDetails> authorizationDetails)
			=> new XStreamingElement("ObtenerPDF", new XElement("UUID", Helper.GetGovernmentNumber(authorizationDetails)));
	}
}
