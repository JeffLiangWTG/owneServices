using System;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Shared;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class IETS413Wrapper : IIETS413
	{
		IETS413Wrapper(TemporaryStorageHeader header)
		{
			this.header = Argument.NotNull(header, nameof(header));
		}

		readonly TemporaryStorageHeader header;

		public IConsignmentHeaderMasterLevel ConsignmentHeaderMasterLevel => consignmentHeaderMasterLevel ?? (consignmentHeaderMasterLevel = ConsignmentHeaderMasterLevelWrapper.New(header));
		IConsignmentHeaderMasterLevel consignmentHeaderMasterLevel;

		public ICustomsOffice CustomsOfficeOfPresentation => customsOfficeOfPresentation ?? (header.PresentationCustomsOffice.IsEmpty ? null : customsOfficeOfPresentation = CustomsOfficeWrapper.New(header.PresentationCustomsOffice));
		ICustomsOffice customsOfficeOfPresentation;

		public DateTime? DateAndTimeOfPresentationOfTheGoods => dateAndTimeOfPresentationOfTheGoods ?? (header.AMA_DateAtCustomsOffice.IsValid ? dateAndTimeOfPresentationOfTheGoods = ZDateTime.Truncate(header.AMA_DateAtCustomsOffice.ToUniversalBranchTime(), TimeSpan.TicksPerSecond).ToNullableDateTime() : null);
		DateTime? dateAndTimeOfPresentationOfTheGoods;

		public DateTime? DeclarationDate => declarationDate ?? (header.DeclarationDate.IsValid ? declarationDate = ZDateTime.Truncate(header.DeclarationDate.ToUniversalBranchTime(), TimeSpan.TicksPerSecond).ToNullableDateTime() : null);
		DateTime? declarationDate;

		public IDeclarant Declarant => declarant ?? (header.Declarant?.Header != null ? declarant = DeclarantWrapper.New(header) : null);
		IDeclarant declarant;

		public byte EnsReUseIndicator => header.ENSReuse;

		public string Crn => crn ?? (crn = header.CRN);
		string crn;

		public string Mrn => mrn ?? (mrn = header.MRN);
		string mrn;

		public string MessageType => messageType ?? (messageType = header.AMA_MessageType);
		string messageType;

		public IMessageHeader MessageHeader => messageHeader ?? (messageHeader = new MessageHeaderWrapper());
		IMessageHeader messageHeader;

		public IPersonPresentingTheGoods PersonPresentingTheGoods => personPresentingTheGoods ?? (personPresentingTheGoods = PersonPresentingTheGoodsWrapper.New(header));
		IPersonPresentingTheGoods personPresentingTheGoods;

		public IRepresentative Representative => representative ?? (representative = RepresentativeWrapper.New(header));
		IRepresentative representative;

		public ICustomsOffice SupervisingCustomsOffice => supervisingCustomsOffice ?? (supervisingCustomsOffice = CustomsOfficeWrapper.New(header.AMA_CustomsOffice));
		ICustomsOffice supervisingCustomsOffice;

		public static IETS413Wrapper New(TemporaryStorageHeader header) => header == null ? null : new IETS413Wrapper(header);
	}
}
