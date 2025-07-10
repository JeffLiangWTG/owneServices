using CargoWise.Types;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS
{
	public interface INctsDocumentWrapper<TLineWrapper> where TLineWrapper : DocBaseWrapper
	{
		ZString BOX1REGIME { get; }
		ZString BOX18DEPARTURETRANSPORTFLAG { get; }
		ZString BOX18DEPARTURETRANSPORTID { get; }
		ZString BOX2CONSIGNOR { get; }
		ZString BOX2CONSIGNOREORI { get; }
		ZString BOX35GROSSMASS { get; }
		ZString BOX38NETTMASS { get; }
		ZString BOX50PRINCIPAL { get; }
		ZString BOX50PRINCIPALEORI { get; }
		ZString BOX50SIGNATURE { get; }
		ZString BOX51TRANSITOFFICE1 { get; }
		ZString BOX51TRANSITOFFICE2 { get; }
		ZString BOX51TRANSITOFFICE3 { get; }
		ZString BOX51TRANSITOFFICE4 { get; }
		ZString BOX51TRANSITOFFICE5 { get; }
		ZString BOX51TRANSITOFFICE6 { get; }
		ZString BOX52GUARANTEE { get; }
		ZString BOX52GUARANTEECODE { get; }
		ZString BOX52GUARANTEEVALIDITY { get; }
		ZString BOX53OFFICEOFDESTINATION { get; }
		ZString BOX15COUNTRYOFORIGIN { get; }
		ZString BOX17COUNTRYOFDESTINATION { get; }
		ZString BOXCDATE { get; }
		ZBool SHOWSTAMPONBOXC { get; }
		ZString BOXCDEPARTUREOFFICECOUNTRYCODE { get; }
		ZString BOXCAUTHORIZEDCONSIGNORNAME { get; }
		ZString BOXCAUTHORISATIONNUMBER { get; }
		ZString BOXCUNIQUEREFERENCENUMBER { get; }
		ZString BOX5ITEMS { get; }
		ZString BOX6PACKAGES { get; }
		ZString BOX8CONSIGNEE { get; }
		ZString BOX8CONSIGNEEEORI { get; }
		ZString BOXCOFFICEOFDEPARTURE { get; }
		ZString BOXCOFFICEOFDEPARTURECODE { get; }
		ZString BOXDRESULT { get; }
		ZString BOXDSEALSAFFIXEDNUMBER { get; }
		ZString BOXDSEALSIDENTITY { get; }
		ZString BOXDTIMELIMITDATE { get; }
		ZString BOXDSIGNATURE { get; }
		ZString EDIENTERPRISEVERSION { get; }
		DocBaseWrapperCollection<TLineWrapper> Lines { get; }
		ZString MOVEMENTREFERENCENUMBER { get; }
		ZString EMAILSUBJECT { get; }
		ZString RETURNOFFICEADDRESS { get; }
		ZString FALLBACKINFORMATION { get; }
		ZString NOTRELEASEDWATERMARK { get; }
		ZString BOXDCLEARANCE { get; }
		ZString LOCALREFERENCENUMBER { get; }
	}
}
