using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Messaging
{
	public interface ICADHeader
	{
		ZString TypeCode_Box1 { get; }
		ZString WsSType_Box2 { get; }
		ZDateTime AccountingDate_Box3 { get; }
		ZString AccountSecurityCode_Box4 { get; }
		ZString CADTransactionNo_Box4 { get; }
		ZString OfficeNo_Box5 { get; }
		ZString ModeOfTransport_Box6 { get; }
		ZDateTime ReleaseDate_Box7 { get; }
		ZDecimal GrossWeightKg_Box8 { get; }
		ZString CarrierCodeAtImportation_Box9 { get; }
		ZString Pre_CARM_Box10 { get; }
		ZString Z1_RPP { get; }
		ZString ImporterBN_Box11 { get; }
		ZString ImporterDetails_Box12 { get; }
		ZString BrokerOrAgentBN_Box13 { get; }
		ZString BrokerOrAgentDetails_Box14 { get; }
		ZString CargoControlNo_Box15 { get; }
		ZString RecordOfIntentNo_Box16 { get; }
		ZString PreviousTransactionNo_Box17 { get; }
		ZDateTime AcceptedDate_Box18 { get; }
		ZString OriginalTransactionNo_Box19 { get; }
		ZString PrevTransNoWarehouse_Box20 { get; }
		ZString PortOfUnlading_Box21 { get; }
		ZString Notes_Box35 { get; }
		ZDecimal TotalValueForDuty_Box113 { get; }
		ZDecimal TotalPSTAndHST_Box114 { get; }
		ZDecimal TotalPSTCannabisAmount_Box115 { get; }
		ZDecimal TotalProvAlcoholTaxAmount_Box116 { get; }
		ZDecimal TotalProvTobaccoAmount_Box117 { get; }
		ZDecimal TotalDeclarationRelieved_Box118 { get; }
		ZDecimal TotalAmount_Box119 { get; }
		ZDecimal TotalCustomsDuties_Box120 { get; }
		ZDecimal TotalExciseDuties_Box121 { get; }
		ZDecimal TotalExciseTaxes_Box122 { get; }
		ZDecimal TotalGST_Box123 { get; }
		ZDecimal TotalAnti_Dumping_Box124 { get; }
		ZDecimal TotalCountervailing_Box125 { get; }
		ZDecimal TotalSurtaxes_Box126 { get; }
		ZDecimal TotalSafeguards_Box127 { get; }
		ZDecimal TotalInterest_Box128 { get; }
		ZDecimal TotalDutiesAndTaxesWithInterest_Box129 { get; }
		ZDecimal TotalDutiesAndTaxes_Box130 { get; }
		IEnumerable<ICADSubHeader> CADSubHeaders { get; }
	}
}
