using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Common;

public sealed class TransshipmentNoticeSubmissionInformationItemDocumentWrapper(HouseBillFieldsResponse item) : DocumentEngineCore.DocWrappers.DocumentWrapper
{
	readonly HouseBillFieldsResponse item = Argument.NotNull(item, nameof(item));

	#region Item Fields

	public ZString I_9_1 => item.ResponseRecords?.ElementAtOrDefault(0)?.HouseBillNumber ?? ZString.Empty;

	public ZString I_10_1 => item.ResponseRecords?.ElementAtOrDefault(0)?.TemporaryLandingRegistrationNumber ?? ZString.Empty;

	public ZString I_9_2 => item.ResponseRecords?.ElementAtOrDefault(1)?.HouseBillNumber ?? ZString.Empty;

	public ZString I_10_2 => item.ResponseRecords?.ElementAtOrDefault(1)?.TemporaryLandingRegistrationNumber ?? ZString.Empty;

	public ZString I_9_3 => item.ResponseRecords?.ElementAtOrDefault(2)?.HouseBillNumber ?? ZString.Empty;

	public ZString I_10_3 => item.ResponseRecords?.ElementAtOrDefault(2)?.TemporaryLandingRegistrationNumber ?? ZString.Empty;

	public ZString I_9_4 => item.ResponseRecords?.ElementAtOrDefault(3)?.HouseBillNumber ?? ZString.Empty;

	public ZString I_10_4 => item.ResponseRecords?.ElementAtOrDefault(3)?.TemporaryLandingRegistrationNumber ?? ZString.Empty;

	public ZString I_9_5 => item.ResponseRecords?.ElementAtOrDefault(4)?.HouseBillNumber ?? ZString.Empty;

	public ZString I_10_5 => item.ResponseRecords?.ElementAtOrDefault(4)?.TemporaryLandingRegistrationNumber ?? ZString.Empty;

	public ZString I_9_6 => item.ResponseRecords?.ElementAtOrDefault(5)?.HouseBillNumber ?? ZString.Empty;

	public ZString I_10_6 => item.ResponseRecords?.ElementAtOrDefault(5)?.TemporaryLandingRegistrationNumber ?? ZString.Empty;

	public ZString I_9_7 => item.ResponseRecords?.ElementAtOrDefault(6)?.HouseBillNumber ?? ZString.Empty;

	public ZString I_10_7 => item.ResponseRecords?.ElementAtOrDefault(6)?.TemporaryLandingRegistrationNumber ?? ZString.Empty;

	public ZString I_9_8 => item.ResponseRecords?.ElementAtOrDefault(7)?.HouseBillNumber ?? ZString.Empty;

	public ZString I_10_8 => item.ResponseRecords?.ElementAtOrDefault(7)?.TemporaryLandingRegistrationNumber ?? ZString.Empty;

	public ZString I_9_9 => item.ResponseRecords?.ElementAtOrDefault(8)?.HouseBillNumber ?? ZString.Empty;

	public ZString I_10_9 => item.ResponseRecords?.ElementAtOrDefault(8)?.TemporaryLandingRegistrationNumber ?? ZString.Empty;

	public ZString I_9_10 => item.ResponseRecords?.ElementAtOrDefault(9)?.HouseBillNumber ?? ZString.Empty;

	public ZString I_10_10 => item.ResponseRecords?.ElementAtOrDefault(9)?.TemporaryLandingRegistrationNumber ?? ZString.Empty;

	#endregion
}
