using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Italy
{
	public class CedentePrestatore
	{
		public XStreamingElement BuildXML(TransactionInfo transaction, ZString? recipientOrgCategory = null, OrgHeader orgHeader = null)
		{
			XStreamingElement cedentePrestatore = null;

			if (FatturaElettronicaDataHelper.IsPayable(transaction))
			{
				var datiAnagrafici = FatturaElettronicaDataHelper.BuildXmlForDatiAnagraficiBasedOnBillingOrg(transaction, recipientOrgCategory.HasValue ? (ZString)recipientOrgCategory : ZString.Empty, orgHeader);
				FatturaElettronicaDataHelper.BuildXmlForRegimeFiscale(datiAnagrafici);

				cedentePrestatore = new XStreamingElement("CedentePrestatore",
					datiAnagrafici,
					FatturaElettronicaDataHelper.BuildXmlForSedeBasedOnBillingOrg(transaction));
			}
			else
			{
				var datiAnagrafici = FatturaElettronicaDataHelper.BuildXmlForDatiAnagraficiBasedOnLoginCompany(transaction);
				FatturaElettronicaDataHelper.BuildXmlForRegimeFiscale(datiAnagrafici);

				cedentePrestatore = new XStreamingElement("CedentePrestatore",
					datiAnagrafici,
					FatturaElettronicaDataHelper.BuildXmlForDatiSedeBasedOnLoginCompany(transaction));
			}

			return cedentePrestatore;
		}
	}
}
