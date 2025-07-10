using CargoWise.EntityFramework;
using Enterprise.Customs.ExitControlBase.Business;

namespace Enterprise.Customs.CH.DeclarationActivation.Business;

public class DeclarationActivationReportValidation(DeclarationActivationReport parent) : CusExitReportValidation(parent)
{
	new DeclarationActivationReport Parent => (DeclarationActivationReport)base.Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateNextProcedure();
		ValidateCommunicationLanguage();
	}

	protected override void CheckCER_Location()
	{
		base.CheckCER_Location();
		ListValidation.MessageErrorIfInvalidCode(Parent.CER_LocationInfo);
	}

	protected override void CheckCER_Type()
	{
		base.CheckCER_Type();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CER_TypeInfo);
	}

	protected override void CheckCER_TransportType()
	{
		base.CheckCER_TransportType();
		ListValidation.MessageErrorIfInvalidCode(Parent.CER_TransportTypeInfo);
	}

	protected override void CheckCER_TransportMode()
	{
		base.CheckCER_TransportMode();
		ListValidation.MessageErrorIfInvalidCode(Parent.CER_TransportModeInfo);
	}

	protected override void CheckCER_RN_NKTransportNationality()
	{
		base.CheckCER_RN_NKTransportNationality();
		ListValidation.MessageErrorIfInvalidCode(Parent.CER_RN_NKTransportNationalityInfo);
	}

	protected override void CheckCER_OfficeOfExport()
	{
		base.CheckCER_OfficeOfExport();
		ListValidation.MessageErrorIfInvalidCode(Parent.CER_OfficeOfExportInfo);
	}

	protected override void CheckCER_AdditionalDeclarationType()
	{
		base.CheckCER_AdditionalDeclarationType();
		ListValidation.MessageErrorIfInvalidCode(Parent.CER_AdditionalDeclarationTypeInfo);
	}

	public void ValidateNextProcedure()
	{
		ValidateCalculatedProperty(Parent.NextProcedureInfo);
	}

	protected void CheckNextProcedure()
	{
		ListValidation.MessageErrorIfInvalidCode(Parent.NextProcedureInfo);
	}

	public void ValidateCommunicationLanguage()
	{
		ValidateCalculatedProperty(Parent.CommunicationLanguageInfo);
	}

	protected void CheckCommunicationLanguage()
	{
		ListValidation.MessageErrorIfInvalidCode(Parent.CommunicationLanguageInfo);
	}
}
