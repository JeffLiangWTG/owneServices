using CargoWise.ComponentModel;
using CargoWise.Types;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.CH.Business;

public class MessageSendingDeclaration : AutoMessageSendingDeclaration, IMessageSendingDeclaration
{
	public MessageSendingDeclaration(ExportDeclarationMessageSendingObjectParent sendingObjectParent) : base(sendingObjectParent.Factory)
	{
		SendingObjectParent = sendingObjectParent;
		SetDefaultsFromDeclaration();
	}

	public ExportDeclarationMessageSendingObjectParent SendingObjectParent;

	public JobDeclaration ParentDeclaration => SendingObjectParent.ParentDeclaration;

	JobDeclaration IMessageSendingDeclaration.WrappedDeclaration => ParentDeclaration;

	void SetDefaultsFromDeclaration()
	{
		JE_DeclarationLanguage = ParentDeclaration.JE_DeclarationLanguage.Left(Schema.JE_DeclarationLanguageMaxLength);
		JE_LocationOfGoods = ParentDeclaration.JE_LocationOfGoods;
		JE_TransportMode = ParentDeclaration.JE_TransportMode;
		JE_MasterBill = ParentDeclaration.JE_MasterBill;
		JE_VoyageFlightNo = ParentDeclaration.JE_VoyageFlightNo;
		JE_RN_NKTransportNationality = ParentDeclaration.JE_RN_NKTransportNationality;
		JE_TransportMeans = ParentDeclaration.JE_TransportMeans;
		JE_VesselName = ParentDeclaration.JE_VesselName;
	}

	[List($"{nameof(ParentDeclaration)}.{nameof(JobDeclaration.Lookups)}.{nameof(JobDeclarationLookups.DeclarationLanguageList)}")]
	public override ZString JE_DeclarationLanguage { get => base.JE_DeclarationLanguage; set => base.JE_DeclarationLanguage = value; }

	[List($"{nameof(ParentDeclaration)}.{nameof(JobDeclaration.Lookups)}.{nameof(JobDeclarationLookups.AuthorizationsList)}")]
	public override ZString JE_LocationOfGoods { get => base.JE_LocationOfGoods; set => base.JE_LocationOfGoods = value; }

	[List($"{nameof(ParentDeclaration)}.{nameof(JobDeclaration.Lookups)}.{nameof(JobDeclarationLookups.TransportTypeList)}")]
	public override ZString JE_TransportMode { get => base.JE_TransportMode; set => base.JE_TransportMode = value; }

	[List($"{nameof(ParentDeclaration)}.{nameof(JobDeclaration.Lookups)}.{nameof(JobDeclarationLookups.TransportNationalities)}")]
	public override ZString JE_RN_NKTransportNationality { get => base.JE_RN_NKTransportNationality; set => base.JE_RN_NKTransportNationality = value; }

	[List($"{nameof(ParentDeclaration)}.{nameof(JobDeclaration.Lookups)}.{nameof(JobDeclarationLookups.TransportMeansList)}")]
	public override ZString JE_TransportMeans { get => base.JE_TransportMeans; set => base.JE_TransportMeans = value; }

	public bool IsAir => JE_TransportMode == TransportModes.Air;

	public bool IsOwnPropulsion => JE_TransportMode == TransportModes.OwnPropulsion;

	ZString IMessageSendingDeclaration.JE_SpecificCircumstanceIndicator => ParentDeclaration.JE_SpecificCircumstanceIndicator;
	ZString IMessageSendingDeclaration.JE_VehicleType => ParentDeclaration.JE_VehicleType;
}
