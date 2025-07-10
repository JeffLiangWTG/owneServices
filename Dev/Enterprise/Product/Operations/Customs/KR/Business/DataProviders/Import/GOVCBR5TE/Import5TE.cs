using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import5TE : IImport5TEHeader
	{
		public string ImportDeclarationNumber { get; set; }
		public int SequenceNo { get; set; }
		public string ApplicationReason { get; set; }
		public string BrokerID { get; set; }
		public Organisation Payer { get; set; }

		ZString IImport5TEHeader.ImportDeclarationNumber => ImportDeclarationNumber;
		ZInt IImport5TEHeader.SequenceNo => SequenceNo;
		ZString IImport5TEHeader.ApplicationReason => ApplicationReason;
		ZString IImport5TEHeader.BrokerID => BrokerID;
		IOrganization IImport5TEHeader.Payer => Payer;
	}
}
