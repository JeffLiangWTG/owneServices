namespace Enterprise.Customs.IT.Business.Declaration;

public interface IJobDeclarationFieldsCleaner
{
	void CleanUpMessageDependentFieldsIfNoLongerApplicable();

	void CleanUpTransportModeInlandDependentFieldsIfNoLongerApplicable();

	void CleanUpShipmentIncoTermFieldsIfNoLongerApplicable();
}
