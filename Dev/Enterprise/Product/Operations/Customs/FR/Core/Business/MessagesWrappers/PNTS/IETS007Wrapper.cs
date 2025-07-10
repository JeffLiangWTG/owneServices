using System;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Shared;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	[CodeAlive("Will be used in the future")]
	public class IETS007Wrapper : IIETS007
	{
		public IETS007Wrapper(TemporaryStorageHeader header)
		{
			this.header = Argument.NotNull(header, nameof(header));
		}

		readonly TemporaryStorageHeader header;

		public IMessageHeader MessageHeader => messageHeader ?? (messageHeader = new MessageHeaderWrapperIETS007());
		IMessageHeader messageHeader;

		public string Lrn => lrn ?? (lrn = header.LRN);
		string lrn;

		public DateTime? DateAndTimeOfPresentationOfTheGoods => dateAndTimeOfPresentationOfTheGoods ?? (header.AMA_DateAtCustomsOffice.IsValid ? dateAndTimeOfPresentationOfTheGoods = ZDateTime.Truncate(header.AMA_DateAtCustomsOffice.ToUniversalBranchTime(), TimeSpan.TicksPerSecond).ToNullableDateTime() : null);
		DateTime? dateAndTimeOfPresentationOfTheGoods;

		public DateTime? DeclarationDate => declarationDate ?? (header.DeclarationDate.IsValid ? declarationDate = ZDateTime.Truncate(header.DeclarationDate.ToUniversalBranchTime(), TimeSpan.TicksPerSecond).ToNullableDateTime() : null);
		DateTime? declarationDate;

		public string Crn => crn ?? (crn = header.CRN);
		string crn;

		public ICustomsOffice CustomsOfficeOfPresentation => customsOfficeOfPresentation ?? (header.PresentationCustomsOffice.IsEmpty ? null : customsOfficeOfPresentation = CustomsOfficeWrapper.New(header.PresentationCustomsOffice));
		ICustomsOffice customsOfficeOfPresentation;

		public IPersonPresentingTheGoods PersonPresentingTheGoods => personPresentingTheGoods ?? (personPresentingTheGoods = PersonPresentingTheGoodsWrapper.New(header));
		IPersonPresentingTheGoods personPresentingTheGoods;

		public IDeclarant Declarant => declarant ?? (header.Declarant?.Header != null ? declarant = DeclarantWrapper.New(header) : null);
		IDeclarant declarant;

		public IRepresentative Representative => representative ?? (representative = RepresentativeWrapper.New(header));
		IRepresentative representative;

		public IConsignmentHeaderMasterLevel ConsignmentHeaderMasterLevel => consignmentHeaderMasterLevel ?? (consignmentHeaderMasterLevel = ConsignmentHeaderMasterLevelWrapper.New(header));
		IConsignmentHeaderMasterLevel consignmentHeaderMasterLevel;

		public static IETS007Wrapper New(TemporaryStorageHeader header) => header == null ? null : new IETS007Wrapper(header);
	}
}
