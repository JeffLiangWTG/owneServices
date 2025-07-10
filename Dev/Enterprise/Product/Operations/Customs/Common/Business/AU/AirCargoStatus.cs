using System.ComponentModel;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU
{
	[ImmutableObject(true)]
	public class AirCargoStatus : CodeDescriptionPair
	{
		AirCargoStatus(object code, MultilingualString description) : base(code, description)
		{
		}

		public static readonly AirCargoStatus NotSent = new AirCargoStatus("NOT", ResString.GetMultilingualString("2680EC4F-951F-43A1-8BE6-C6859AEF245D", "Not Sent"));
		public static readonly AirCargoStatus WaitingForResponse = new AirCargoStatus("WAIT", ResString.GetMultilingualString("B38302D1-7611-4C50-9E13-C1EEAE478E56", "Waiting for response"));
		public static readonly AirCargoStatus MayBeDelivered = new AirCargoStatus("C000", ResString.GetMultilingualString("97573665-38D3-4690-B969-231AE490054A", "May be delivered"));
		public static readonly AirCargoStatus ScreenFreeMayBeDelivered = new AirCargoStatus("C150", ResString.GetMultilingualString("1F9FDC77-8C81-4C52-BDD1-121547BF69E5", "Screen free may be delivered"));
		public static readonly AirCargoStatus ScreenFreeSubjectToQuarantine = new AirCargoStatus("C155", ResString.GetMultilingualString("9BD201EF-0325-419E-BE7E-B0961943AC24", "Screen free subject to Quarantine"));
		public static readonly AirCargoStatus ApprovedA100 = new AirCargoStatus("A100", ResString.GetMultilingualString("B49AE15A-5D66-4EC7-B32A-AE570AEAE7C4", "Approved to move underbond to approved s77G depot. Local movement."));
		public static readonly AirCargoStatus ApprovedA105 = new AirCargoStatus("A105", ResString.GetMultilingualString("9D654C52-B3EE-4C48-B939-C36CDB8A3DE8", "Approved to move underbond to approved s77G depot, subj to Quarantine. Local movement"));
		public static readonly AirCargoStatus ApprovedA120 = new AirCargoStatus("A120", ResString.GetMultilingualString("1DCCA3C9-CB7F-4036-ADB2-81C7E9723274", "Approved to move underbond to a specified approved s77G depot. Interstate movement"));
		public static readonly AirCargoStatus ApprovedA125 = new AirCargoStatus("A125", ResString.GetMultilingualString("ADEF0AB5-1D16-4639-8C5C-B6025C32D3B9", "Approved to move underbond to a specified approved s77G depot, subj to Quarantine. Interstate movement."));
		public static readonly AirCargoStatus ApprovedA140 = new AirCargoStatus("A140", ResString.GetMultilingualString("F230A193-6AB9-4AAC-AB57-89684930A24A", "Time-up cargo approved for bonding, revert to manual documents after bonding"));
		public static readonly AirCargoStatus IncompleteHoldCargoHoldDocs = new AirCargoStatus("H600", ResString.GetMultilingualString("CE3E53F3-020B-41F0-BA49-EFDF12164F16", "Customs action incomplete, hold cargo, hold docs."));
		public static readonly AirCargoStatus IncompleteHoldCargoHoldDocsSubQuarantine = new AirCargoStatus("H605", ResString.GetMultilingualString("923F8C3B-D963-40D6-8E57-6E354D8E3723", "Customs action incomplete, hold cargo, hold docs, subj to Quarantine."));
		public static readonly AirCargoStatus AwaitingHoldCargoHoldDocs = new AirCargoStatus("H610", ResString.GetMultilingualString("A69C8455-AEBB-4405-9201-D94BE160474A", "Awaiting movement request, hold cargo, hold docs."));
		public static readonly AirCargoStatus AwaitingSubQuarantine = new AirCargoStatus("H615", ResString.GetMultilingualString("5240A1D0-899E-4255-8B78-4143DA4B9CF0", "Awaiting movement request, subj to Quarantine."));
		public static readonly AirCargoStatus RevertToManualProcedures = new AirCargoStatus("M800", ResString.GetMultilingualString("B714CF07-2758-4B03-87AF-574A64E754C4", "Revert to manual procedures."));
		public static readonly AirCargoStatus ManualNoHouseBills = new AirCargoStatus("M810", ResString.GetMultilingualString("5F372DCD-6B44-4ED9-ABE4-54A156EF08EA", "Manual - no house bills present."));
		public static readonly AirCargoStatus ManualDomesticCargo = new AirCargoStatus("M820", ResString.GetMultilingualString("2F73B8A5-1D53-4DA4-B72A-987C4E0FE238", "Manual - domestic cargo."));
		public static readonly AirCargoStatus RevertToManualDocs = new AirCargoStatus("M830", ResString.GetMultilingualString("931F98CA-BBF7-4A43-B3D9-1DC1F1BC8357", "Revert to manual docs."));
		public static readonly AirCargoStatus PaperClearAWBPerforatedNotRequired = new AirCargoStatus("P200", ResString.GetMultilingualString("EAC56A06-D9D0-47E4-89D6-3FE286322123", "Paper Clear. The paper AWB is not required to be perforated."));
		public static readonly AirCargoStatus PaperClearSubQuarantine = new AirCargoStatus("P205", ResString.GetMultilingualString("01326045-8B13-4577-B694-9F6E9BBE6062", "Paper Clear, Subj to Quarantine. Paper AWB does not require perforation"));
		public static readonly AirCargoStatus TranshipmentCargo = new AirCargoStatus("P300", ResString.GetMultilingualString("FE79BA9B-5B99-4C2E-A9A7-48B071188D65", "Transhipment cargo."));
		public static readonly AirCargoStatus TranshipmentCargoSubQuarantine = new AirCargoStatus("P305", ResString.GetMultilingualString("08487362-67AC-4FCF-B55D-48BF8FDB65E0", "Transhipment cargo, subj to Quarantine."));
		public static readonly AirCargoStatus CustomsQuarPermissionToTranship = new AirCargoStatus("P310", ResString.GetMultilingualString("EE0E64D5-2272-4400-8415-0A79CB993BF6", "Customs and Quarantine permission to tranship."));
		public static readonly AirCargoStatus MoveGoodsToQuarantine = new AirCargoStatus("Q160", ResString.GetMultilingualString("7ED802A4-2D32-4A06-AC8A-5C62F37D14E4", "Move goods to Quarantine."));
		public static readonly AirCargoStatus QuarantineHoldCargoHoldDocs = new AirCargoStatus("Q600", ResString.GetMultilingualString("DF5226DA-037F-4597-91B3-3D3725E015DD", "Quarantine hold cargo and documents."));
		public static readonly AirCargoStatus MovementDeleted = new AirCargoStatus("R700", ResString.GetMultilingualString("DB091B4A-EB63-4C79-B24A-16F98CA37489", "Movement deleted"));
		public static readonly AirCargoStatus EstablishmentInMovement = new AirCargoStatus("R710", ResString.GetMultilingualString("CE462487-A7CD-4DD9-9F4E-3917EB59BB8D", "Establishment in movement request is not automated."));
		public static readonly AirCargoStatus NotPermittedToMoveToThisPort = new AirCargoStatus("R720", ResString.GetMultilingualString("E475B7A8-3084-4D14-9706-DEB51E2F405C", "Not permitted to move to this port."));
		public static readonly AirCargoStatus RequestToMoveToThisEstablishmentRejected = new AirCargoStatus("R730", ResString.GetMultilingualString("67D0E080-13C7-42E6-8DE7-9B8C868BB248", "Request to move to this establishment rejected."));
		public static readonly AirCargoStatus RequestPendingMoreThan28DaysAndRejected = new AirCargoStatus("R740", ResString.GetMultilingualString("8300E950-2258-45B9-9064-F8D4F854F184", "Request pending more than 28 days, request rejected."));
		public static readonly AirCargoStatus UBondMovementNotRequired = new AirCargoStatus("R750", ResString.GetMultilingualString("934CD0BD-398C-4AFF-A039-812E848CF35E", "U/bond movement not required, cargo already may be delivered."));
		public static readonly AirCargoStatus IncompleteEntryRequired = new AirCargoStatus("W310", ResString.GetMultilingualString("8A5923AB-948B-4D09-8D53-06F22289F0A9", "Customs action incomplete, Entry required."));
		public static readonly AirCargoStatus Incomplete = new AirCargoStatus("W320", ResString.GetMultilingualString("5C75F438-3E81-4743-B11D-2EE2A8777521", "Customs action incomplete."));
		public static readonly AirCargoStatus ExaminationRequiredEntryRequired = new AirCargoStatus("W330", ResString.GetMultilingualString("DC2B25D9-8622-4701-8EB1-EE4FD7C951D6", "Customs examination required, Entry required."));
		public static readonly AirCargoStatus ExaminationRequired = new AirCargoStatus("W335", ResString.GetMultilingualString("09E46AAF-3E52-4713-ADDA-C3071442810A", "Customs examination required."));
		public static readonly AirCargoStatus CustomsClearSubQuarantine = new AirCargoStatus("W400", ResString.GetMultilingualString("B44C6467-2A87-41C3-A648-216427539494", "Customs clear, subj to Quarantine. Delivery on written Quarantine authority."));
		public static readonly AirCargoStatus EntryRequiredSubQuarantine = new AirCargoStatus("W410", ResString.GetMultilingualString("B3196488-3FC2-4716-955C-8777AA716C45", "Customs Entry required, subj to Quarantine."));
		public static readonly AirCargoStatus ActionIncompleteSubQuarantine = new AirCargoStatus("W420", ResString.GetMultilingualString("889D3F2A-2CC2-41F2-8358-54F06D3CF874", "Customs action incomplete, subject to Quarantine."));
		public static readonly AirCargoStatus EntryAndExamRequiredSubQuarantine = new AirCargoStatus("W430", ResString.GetMultilingualString("869346BA-0BBB-4D8A-8BE7-80F777DDC3B9", "Customs Entry and examination required, subj to Quarantine."));
		public static readonly AirCargoStatus ExaminationRequiredSubQuarantine = new AirCargoStatus("W435", ResString.GetMultilingualString("952E3E5D-082C-4165-A27C-B76481FA7C3A", "Customs examination required, subj to Quarantine."));
		public static readonly AirCargoStatus DocumentsToScreeners = new AirCargoStatus("W450", ResString.GetMultilingualString("338A4F76-D2EE-445F-9D16-542E05F88E88", "Documents to screeners."));
		public static readonly AirCargoStatus DeficientConsigneeName = new AirCargoStatus("W910", ResString.GetMultilingualString("9EFB9A17-70F5-40E5-A686-C8F20588EBAC", "Deficient information supplied in Consignee Name field."));
		public static readonly AirCargoStatus DeficientConsigneeAddress = new AirCargoStatus("W920", ResString.GetMultilingualString("4DE46313-04A0-471C-BAAE-CB553E1C7162", "Deficient information supplied in Consignee Address field."));
		public static readonly AirCargoStatus DeficientConsignorName = new AirCargoStatus("W930", ResString.GetMultilingualString("23B560C7-515A-4D1D-AADD-F6462CF07E26", "Deficient information supplied in Consignor Name field"));
		public static readonly AirCargoStatus DeficientConsignorAddress = new AirCargoStatus("W940", ResString.GetMultilingualString("D676033A-1D69-410A-B226-3EB8B14C47B8", "Deficient information supplied in Consignor Address field."));
		public static readonly AirCargoStatus DeficientGoodsDescription = new AirCargoStatus("W950", ResString.GetMultilingualString("3E6523BA-A98D-4388-A89B-7E8DF46FB3E2", "Deficient information supplied in Goods Description field."));
		public static readonly AirCargoStatus IncorrectDestinationPort = new AirCargoStatus("W960", ResString.GetMultilingualString("9FB8B39E-6451-4732-922E-201B832C69FF", "Incorrect information supplied in Destination Port field."));
		public static readonly AirCargoStatus DeficientManyFields = new AirCargoStatus("W970", ResString.GetMultilingualString("23A9B7FA-D3AC-4412-9A59-1F331E711580", "Deficient information supplied in many fields, refer to free text segment for details"));
		public static readonly AirCargoStatus SeizedByCustoms = new AirCargoStatus("Z500", ResString.GetMultilingualString("7C9052B5-E218-466D-AB1E-8296790E271B", "Shipment seized by Customs."));
		public static readonly AirCargoStatus SeizedByQuarantine = new AirCargoStatus("Z510", ResString.GetMultilingualString("0289244B-8058-402F-980E-57E17D5F0614", "Shipment seized by Quarantine."));
		public static readonly AirCargoStatus ZeroLanded = new AirCargoStatus("Z998", ResString.GetMultilingualString("533544DD-EE5B-4DB3-B875-7392FFC2D95D", "Zero-Landed"));
	}
}
