using System.Collections.Generic;

namespace Enterprise.Customs.GB.GVMS
{
	public interface IGvmsMessageBuilder
	{
		string messageId { get; }
		string gmrId { get; }
		string gmrState { get; }
		int gmrStatusVersion { get; }
		bool inspectionRequired { get; }
		string createdDateTime { get; }
		string updatedDateTime { get; }
		List<RuleFailure> ruleFailures { get; }
		Metadata metadata { get; }
		string haulierEORI { get; }
		bool isUnaccompanied { get; }
		bool isOwnVehicle { get; }
		bool isTSAD { get; }
		string direction { get; }
		string haulierType { get; }
		string vehicleRegNum { get; }
		List<string> trailerRegistrationNums { get; }
		List<string> containerReferenceNums { get; }
		Plannedcrossing plannedCrossing { get; }
		Checkedincrossing checkedInCrossing { get; }
		Actualcrossing actualCrossing { get; }
		List<ReportToLocation> reportToLocations { get; }
		string routeId { get; }
		string localDateTimeOfDeparture { get; }
		string localDateTimeOfArrival { get; }
		List<Eidrdeclaration> eidrDeclarations { get; }
		List<UkimsEidrdeclaration> ukimsEidrDeclarations { get; }
		List<Tirdeclaration> tirDeclarations { get; }
		List<Atadeclaration> ataDeclarations { get; }
		string traderEORI { get; }
		Emptyvehicle emptyVehicle { get; }
		List<Customsdeclaration> customsDeclarations { get; }
		MtpDeclaration mtpDeclaration { get; }
		List<Transitdeclaration> transitDeclarations { get; }
		List<Link> links { get; }
		string customsDeclarationId { get; }
		string tirCarnetId { get; }
		string ataCarnetId { get; }
		string sAndSMasterRefNum { get; }
		string transitDeclarationId { get; }
		ExemptionDeclaration exemptionDeclaration { get; }
		UkcDeclaration ukcDeclaration { get; }
	}
}
