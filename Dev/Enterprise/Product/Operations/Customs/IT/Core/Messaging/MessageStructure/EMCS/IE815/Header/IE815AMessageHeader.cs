using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;
using Enterprise.Customs.IT.Messaging.MessageStructure;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public class IE815AMessageHeader : IMessageHeader
{
	public IE815AMessageHeader(IIE815EMCSMessage iE815EMCSMessage)
	{
		this.iE815EMCSMessage = Argument.NotNull(iE815EMCSMessage, "iE815EMCSMessage");
		this.iE815EMCSMessageHeader = Argument.NotNull(iE815EMCSMessage.Header, "iE815EMCSMessageHeader");
	}
	readonly IIE815EMCSMessage iE815EMCSMessage;
	readonly IIE815EMCSMessageHeader iE815EMCSMessageHeader;

	[MessageLayout(Order = 0)]
	public IE815FixedPart Attributes => new IE815FixedPart(iE815EMCSMessageHeader.Attributes);

	[MessageLayout(Order = 1)]
	public IE815ADetailPart DetailPart => new IE815ADetailPart(iE815EMCSMessageHeader.DetailPart, iE815EMCSMessage.Continuations.Count());

	[MessageLayout(Order = 2)]
	public IE815HeaderEad HeaderEad => new IE815HeaderEad(iE815EMCSMessageHeader.HeaderEad);

	[MessageLayout(Order = 3)]
	public IE815ConsignorTrader ConsignorTrader => new IE815ConsignorTrader(iE815EMCSMessageHeader.ConsignorTrader);

	[MessageLayout(Order = 4)]
	public IE815PlaceOfDispatchTrader PlaceOfDispatchTrader => new IE815PlaceOfDispatchTrader(iE815EMCSMessageHeader.PlaceOfDispatchTrader);

	[MessageLayout(Order = 5)]
	public IE815DispatchImportOffice DispatchImportOffice => new IE815DispatchImportOffice(iE815EMCSMessageHeader.DispatchImportOffice);

	[MessageLayout(Order = 6)]
	public IE815CompetentAuthorityDispatchOffice CompetentAuthorityDispatchOffice => new IE815CompetentAuthorityDispatchOffice(iE815EMCSMessageHeader.CompetentAuthorityDispatchOffice);

	[MessageLayout(Order = 7)]
	public IE815ConsigneeTrader ConsigneeTrader => new IE815ConsigneeTrader(iE815EMCSMessageHeader.ConsigneeTrader);

	[MessageLayout(Order = 8)]
	public IE815ComplementConsigneeTrader ComplementConsigneeTrader => new IE815ComplementConsigneeTrader(iE815EMCSMessageHeader.ComplementConsigneeTrader);

	[MessageLayout(Order = 9)]
	public IE815DeliveryPlaceTrader DeliveryPlaceTrader => new IE815DeliveryPlaceTrader(iE815EMCSMessageHeader.DeliveryPlaceTrader);

	[MessageLayout(Order = 10)]
	public IE815DeliveryPlaceCustomsOffice DeliveryPlaceCustomsOffice => new IE815DeliveryPlaceCustomsOffice(iE815EMCSMessageHeader.DeliveryPlaceCustomsOffice);

	[MessageLayout(Order = 11)]
	public IE815TransportMode TransportMode => new IE815TransportMode(iE815EMCSMessageHeader.TransportMode);

	[MessageLayout(Order = 12)]
	public IE815TransportArrangerTrader TransportArrangerTrader => new IE815TransportArrangerTrader(iE815EMCSMessageHeader.TransportArrangerTrader);

	[MessageLayout(Order = 13)]
	public IE815FirstTransporter FirstTransporterTrader => new IE815FirstTransporter(iE815EMCSMessageHeader.FirstTransporterTrader);

	[MessageLayout(Order = 14)]
	public IE815EadDraft EadDraft => new IE815EadDraft(iE815EMCSMessageHeader.EadDraft);

	[MessageLayout(Order = 15)]
	public IE815MovementGuarantee MovementGuarantee => new IE815MovementGuarantee(iE815EMCSMessageHeader.MovementGuarantee);

	[MessageLayout(Order = 16)]
	public IE815ImportSadContainer ImportSads => new IE815ImportSadContainer(iE815EMCSMessageHeader.ImportSads);

	[MessageLayout(Order = 17)]
	public IE815TransportDetailContainer TransportDetailContainer => new IE815TransportDetailContainer(iE815EMCSMessageHeader.TransportDetailContainer);

	[MessageLayout(Order = 18)]
	public IE815Certificates Certificates => new IE815Certificates(iE815EMCSMessageHeader.Certificates);

	[MessageLayout(Order = 19)]
	public IE815BodyEad BodyEad => new IE815BodyEad(iE815EMCSMessageHeader.BodyEad);
}
