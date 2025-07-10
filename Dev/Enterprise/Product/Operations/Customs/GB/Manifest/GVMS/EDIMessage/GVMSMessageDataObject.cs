using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Enterprise.Customs.GB.GVMS
{
	public class GVMSMessageDataObject : IGvmsMessageBuilder
	{
		public string messageId { get; set; }
		public string gmrId { get; set; }
		public string gmrState { get; set; }
		public int gmrStatusVersion { get; set; }
		public bool inspectionRequired { get; set; }
		public string createdDateTime { get; set; }
		public string updatedDateTime { get; set; }
		public List<RuleFailure> ruleFailures { get; set; }
		public Metadata metadata { get; set; }
		public string haulierEORI { get; set; }
		public virtual bool isUnaccompanied { get; set; }
		public bool isOwnVehicle { get; set; }
		public bool isTSAD { get; set; }
		public virtual string direction { get; set; }
		public virtual string haulierType { get; set; }
		public virtual string vehicleRegNum { get; set; }
		public virtual List<string> trailerRegistrationNums { get; set; }
		public virtual List<string> containerReferenceNums { get; set; }
		public virtual Plannedcrossing plannedCrossing { get; set; }
		public Checkedincrossing checkedInCrossing { get; set; }
		public Actualcrossing actualCrossing { get; set; }
		public List<ReportToLocation> reportToLocations { get; set; }
		public string routeId { get; set; }
		public string localDateTimeOfDeparture { get; set; }
		public string localDateTimeOfArrival { get; set; }
		public virtual List<Eidrdeclaration> eidrDeclarations { get; set; }
		public virtual List<UkimsEidrdeclaration> ukimsEidrDeclarations { get; set; }
		public virtual List<Tirdeclaration> tirDeclarations { get; set; }
		public virtual List<Atadeclaration> ataDeclarations { get; set; }
		public string traderEORI { get; set; }
		public virtual Emptyvehicle emptyVehicle { get; set; }
		public virtual List<Customsdeclaration> customsDeclarations { get; set; }
		public virtual MtpDeclaration mtpDeclaration { get; set; }
		public virtual List<Transitdeclaration> transitDeclarations { get; set; }
		public virtual List<IndirectExportdeclaration> indirectExportDeclarations { get; set; }
		public virtual Dbcdeclaration dbcDeclaration { get; set; }
		public List<Link> links { get; set; }
		public string customsDeclarationId { get; set; }
		public string tirCarnetId { get; set; }
		public string ataCarnetId { get; set; }
		public virtual string sAndSMasterRefNum { get; set; }
		public string transitDeclarationId { get; set; }
		public virtual ExemptionDeclaration exemptionDeclaration { get; set; }
		public virtual UkcDeclaration ukcDeclaration { get; set; }
	}

	public class RuleFailure
	{
		public string code { get; set; }
		public string technicalMessage { get; set; }
		public string field { get; set; }
		public string value { get; set; }
	}

	public class Metadata
	{
		public string state { get; set; }
		public int gmrStatusVersion { get; set; }
		public bool inspectionRequired { get; set; }
		public List<ReportToLocation> reportToLocations { get; set; }
		public string createdDateTime { get; set; }
		public string updatedDateTime { get; set; }
		public List<RuleFailure> ruleFailures { get; set; }
	}

	public class Plannedcrossing
	{
		public string routeId { get; set; }
		public string localDateTimeOfDeparture { get; set; }
	}

	public class Checkedincrossing
	{
		public string routeId { get; set; }
		public string localDateTimeOfArrival { get; set; }
	}

	public class Actualcrossing
	{
		public string routeId { get; set; }
		public string localDateTimeOfArrival { get; set; }
	}

	public class Eidrdeclaration
	{
		public string traderEORI { get; set; }
		public string localReferenceNumber { get; set; }
		public string procedureCode { get; set; }
		public string sAndSMasterRefNum { get; set; }
	}

	public class UkimsEidrdeclaration
	{
		public string traderEORI { get; set; }
		public bool nopWaiver { get; set; }
		public string localReferenceNumber { get; set; }
		public string sAndSMasterRefNum { get; set; }
	}

	public class Tirdeclaration
	{
		public string tirCarnetId { get; set; }
		public string sAndSMasterRefNum { get; set; }
	}

	public class Atadeclaration
	{
		public string ataCarnetId { get; set; }
		public string sAndSMasterRefNum { get; set; }
	}

	public class Emptyvehicle
	{
		public bool isOwnVehicle { get; set; }
		public string sAndSMasterRefNum { get; set; }
	}

	public class Customsdeclaration
	{
		public string customsDeclarationId { get; set; }
		public string sAndSMasterRefNum { get; set; }
		public string customsDeclarationPartId { get; set; }
	}

	public class Transitdeclaration
	{
		public string transitDeclarationId { get; set; }
		[JsonIgnore(Condition = JsonIgnoreCondition.Never)]
		public bool isTSAD { get; set; }
		public string sAndSMasterRefNum { get; set; }
	}

	public class IndirectExportdeclaration
	{
		public string eadMasterRefNum { get; set; }
	}

	public class Dbcdeclaration
	{
		[JsonIgnore(Condition = JsonIgnoreCondition.Never)]
		public bool isOwnVehicle { get; set; }
		public List<DbcGoodsItem> dbcGoods { get; set; }
	}

	public class DbcGoodsItem
	{
		public string sAndSMasterRefNum { get; set; }
	}

	public class Link
	{
		public string href { get; set; }
		public string method { get; set; }
		public string rel { get; set; }
	}

	public class ExemptionDeclaration
	{
		public List<ExemptedGoods> exemptedGoods { get; set; }
	}

	public class ExemptedGoods
	{
		public string sAndSMasterRefNum { get; set; }
	}

	public class ReportToLocation
	{
		public string inspectionTypeId { get; set; }
		public List<string> locationIds { get; set; }
	}

	public class MtpDeclaration
	{
		public List<MtpGoodsItem> mtpGoods { get; set; }
	}

	public class MtpGoodsItem
	{
		public string sAndSMasterRefNum { get; set; }
	}

	public class UkcDeclaration
	{
		public string fpoEORI { get; set; }
	}
}
