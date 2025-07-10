using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public sealed class TemporaryStorageHeaderValidation : EU.Business.CusTempStorage.TemporaryStorageHeaderValidation
{
	public TemporaryStorageHeaderValidation(TemporaryStorageHeader parent) : base(parent)
	{
	}

	protected override void CheckAMA_CustomsProfile()
	{
		base.CheckAMA_CustomsProfile();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.AMA_CustomsProfileInfo);
	}

	protected override void CheckAMA_OA_Representative()
	{
		base.CheckAMA_OA_Representative();
		var representativeInfo = Parent.AMA_OA_RepresentativeInfo;
		MandatoryValidation.MessageErrorIfNotEntered(representativeInfo);
	}

	protected override void CheckTransportType()
	{
		base.CheckTransportType();
		MandatoryValidation.CheckEntered(Parent.TransportTypeInfo);
	}

	protected override void CheckPresentationCustomsOffice_MandatoryValidation()
	{
		MandatoryValidation.MessageErrorIfNotEntered(Parent.PresentationCustomsOfficeInfo);
	}

	protected override void CheckAMA_AgentType()
	{
		base.CheckAMA_AgentType();
		ListValidation.ErrorIfInvalidCodeOrEmpty(Parent.AMA_AgentTypeInfo);
	}

	protected override void CheckDeclarantAndRepresentativeAreDifferent() { }

	protected override void CheckRepresentativeAndDeclarantAreDifferent() { }
}
