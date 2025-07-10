using System.Linq;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Customs.JP.MessageDefinitions.Inbound;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Common;

public sealed class CancellationOfTransshipmentReportDocumentWrapper(IJPInboundMessageParseResult parseResult, BusinessObjectFactory factory) : InboundMessageDocumentWrapper<ICancellationOfTransshipReport>(parseResult, factory)
{
	#region Header Fields

	public ZString H_2 => messageProvider?.SubmitterCode ?? ZString.Empty;

	public ZString H_3 => messageProvider?.VesselCode ?? ZString.Empty;

	public ZString H_4 => messageProvider?.VesselName ?? ZString.Empty;

	public ZString H_5 => messageProvider?.ManifestSubmissionPortCode ?? ZString.Empty;

	public ZString H_6 => messageProvider?.ManifestSubmissionPortSuffix ?? ZString.Empty;

	public ZString H_7 => messageProvider?.ImplementerOfSubmissionProcedure ?? ZString.Empty;

	public ZString H_8 => messageProvider?.MasterBillNumber ?? ZString.Empty;

	#endregion

	#region Item Fields

	public ZString I_9_1 => GetHouseBillNumber(0);

	public ZString I_9_2 => GetHouseBillNumber(1);

	public ZString I_9_3 => GetHouseBillNumber(2);

	public ZString I_9_4 => GetHouseBillNumber(3);

	public ZString I_9_5 => GetHouseBillNumber(4);

	public ZString I_9_6 => GetHouseBillNumber(5);

	public ZString I_9_7 => GetHouseBillNumber(6);

	public ZString I_9_8 => GetHouseBillNumber(7);

	public ZString I_9_9 => GetHouseBillNumber(8);

	public ZString I_9_10 => GetHouseBillNumber(9);

	public ZString I_9_11 => GetHouseBillNumber(10);

	public ZString I_9_12 => GetHouseBillNumber(11);

	public ZString I_9_13 => GetHouseBillNumber(12);

	public ZString I_9_14 => GetHouseBillNumber(13);

	public ZString I_9_15 => GetHouseBillNumber(14);

	public ZString I_9_16 => GetHouseBillNumber(15);

	public ZString I_9_17 => GetHouseBillNumber(16);

	public ZString I_9_18 => GetHouseBillNumber(17);

	public ZString I_9_19 => GetHouseBillNumber(18);

	public ZString I_9_20 => GetHouseBillNumber(19);

	#endregion

	ZString GetHouseBillNumber(int index) => messageProvider.HouseBillNumbers.ElementAtOrDefault(index) ?? ZString.Empty;
}
