using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Italy
{
	public class CessionarioCommittente
	{
		public XStreamingElement BuildXML(TransactionInfo transaction, ZString recipientOrgCategory, OrgHeader orgHeader)
		{
			XStreamingElement cessionarioCommittente = null;

			if (FatturaElettronicaDataHelper.IsPayable(transaction))
			{
				cessionarioCommittente = new XStreamingElement("CessionarioCommittente",
					FatturaElettronicaDataHelper.BuildXmlForDatiAnagraficiBasedOnLoginCompany(transaction),
					FatturaElettronicaDataHelper.BuildXmlForDatiSedeBasedOnLoginCompany(transaction));
			}
			else
			{
				cessionarioCommittente = new XStreamingElement("CessionarioCommittente",
					FatturaElettronicaDataHelper.BuildXmlForDatiAnagraficiBasedOnBillingOrg(transaction, recipientOrgCategory, orgHeader),
					FatturaElettronicaDataHelper.BuildXmlForSedeBasedOnBillingOrg(transaction));
			}
			return cessionarioCommittente;
		}
	}
}
