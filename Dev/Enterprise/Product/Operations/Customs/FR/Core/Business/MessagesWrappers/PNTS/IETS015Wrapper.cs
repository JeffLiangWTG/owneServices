using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces;
using Enterprise.Customs.EU.Business.CusTempStorage;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	[CodeAlive("Will be used in the future")]
	public class IETS015Wrapper : IIETS015
	{
		IETS015Wrapper(TemporaryStorageHeader header)
		{
			this.header = Argument.NotNull(header, nameof(header));
		}

		readonly TemporaryStorageHeader header;

		public IConsignmentHeaderMasterLevel ConsignmentHeaderMasterLevel => consignmentHeaderMasterLevel ?? (consignmentHeaderMasterLevel = ConsignmentHeaderMasterLevelWrapper.New(header));
		IConsignmentHeaderMasterLevel consignmentHeaderMasterLevel;

		public IDeclarant Declarant => declarant ?? (declarant = DeclarantWrapper.New(header));
		IDeclarant declarant;

		public byte EnsReUseIndicator => header.ENSReuse;

		public string Lrn => lrn ?? (lrn = header.LRN);
		string lrn;

		public IMessageHeader MessageHeader => messageHeader ?? (messageHeader = new MessageHeaderWrapper());
		IMessageHeader messageHeader;

		public IRepresentative Representative => representative ?? (representative = RepresentativeWrapper.New(header));
		IRepresentative representative;

		public ICustomsOffice SupervisingCustomsOffice => supervisingCustomsOffice ?? (supervisingCustomsOffice = CustomsOfficeWrapper.New(header.AMA_CustomsOffice));
		ICustomsOffice supervisingCustomsOffice;

		public static IETS015Wrapper New(TemporaryStorageHeader header) => header == null ? null : new IETS015Wrapper(header);
	}
}
