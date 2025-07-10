using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

public interface IMessageSendingDeclaration
{
	JobDeclaration WrappedDeclaration { get; }

	ZString JE_DeclarationLanguage { get; set; }
	ZString JE_LocationOfGoods { get; set; }
	ZString JE_TransportMode { get; set; }
	ZString JE_SpecificCircumstanceIndicator { get; }
	ZString JE_VehicleType { get; }
}
