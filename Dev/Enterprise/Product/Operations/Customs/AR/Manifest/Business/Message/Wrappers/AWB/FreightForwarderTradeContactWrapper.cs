using CargoWise.Common;
using CargoWise.Customs.AR.MessageContracts;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AR.Manifest.Business
{
	internal class FreightForwarderTradeContactWrapper : ITradeContact
	{
		internal FreightForwarderTradeContactWrapper(GlbCompany company)
		{
			this.company = Argument.NotNull(company, "company cannot be null");
		}
		readonly GlbCompany company;

		string ITradeContact.PersonName => ZString.Empty;

		string ITradeContact.DirectTelephoneCommunicationCompleteNumber => company.GC_Phone;

		string ITradeContact.FaxCommunicationCompleteNumber => company.GC_Fax;

		string ITradeContact.EmailCommunicationID => company.GC_Email;
	}
}
