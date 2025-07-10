using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class CC432BWrapper : ICC432B
	{
		CC432BWrapper(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			this.declaration = Argument.NotNull(entryHeader.Declaration, nameof(declaration));
			var mergedLines = entryHeader.MergedLines;
			Argument.GreaterThan(mergedLines.Count, 0, (NoResString)"CustEntryHeader merged lines should exist");
		}

		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;

		public static CC432BWrapper New(CusEntryHeader entryHeader) => entryHeader == null ? null : new CC432BWrapper(entryHeader);

		public IPco CustomsOfficeOfPresentation => customsOfficeOfPresentation ?? (customsOfficeOfPresentation = CustomsOfficeOfPresentationWrapper.New(declaration));
		IPco customsOfficeOfPresentation;

		public IDeclarant Declarant => declarant ?? (declarant = DeclarantWrapper.New(declaration));
		IDeclarant declarant;

		public IGoodsShipment GoodsShipment => goodsShipment ?? (goodsShipment = GoodsShipmentWrapper.New(entryHeader));
		IGoodsShipment goodsShipment;

		public ICC432BCciOperation ImportOperation => importOperation ?? (importOperation = CC432BCciOperationWrapper.New(entryHeader));
		ICC432BCciOperation importOperation;

		public IRepresentative Representative => representative ?? (representative = RepresentativeWrapper.New(declaration));
		IRepresentative representative;
	}
}
