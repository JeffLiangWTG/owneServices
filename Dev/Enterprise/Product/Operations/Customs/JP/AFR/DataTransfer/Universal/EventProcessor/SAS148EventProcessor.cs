using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class SAS148EventProcessor : AFREventProcessor
	{
		public SAS148EventProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory) : base(eventDataObject, logger, factory)
		{
			sas148EventInfo = new SAS148EventInfo(eventDataObject);
		}

		protected override void ProcessCore(JPAFRHeader header)
		{
			if (header != null
				&& !header.JPH_IsShippingLineEntry
				&& !sas148EventInfo.MasterBillNumber.IsEmpty
				&& header.JPH_MasterBillNumber == sas148EventInfo.MasterBillNumber)
			{
				if ((sas148EventInfo.MasterBillIdentifier.Contains("M", StringComparison.OrdinalIgnoreCase)
					|| sas148EventInfo.MasterBillIdentifier.IsEmpty && !sas148EventInfo.AreAllVesselDetailEmpty)
					&& sas148EventInfo.AreAllHouseBillDiscrepancyCodeEmpty)
				{
					if (header.JPH_VesselDetailsChanged)
					{
						VesselInfoUpdater.UpdateVesselInformationDetailsForSAS148(logger, header,
							sas148EventInfo.NewCarrierCode,
							sas148EventInfo.NewVesselCallSign,
							sas148EventInfo.NewVoyageNumber,
							sas148EventInfo.NewLoadingPortCode,
							sas148EventInfo.NewLoadingPortSuffix,
							sas148EventInfo.NewVesselName,
							sas148EventInfo.ETA,
							sas148EventInfo.ETD);
					}
				}

				var isDeletionNotEmpty = !sas148EventInfo.DateTimeOfDeletion.IsEmpty;
				foreach (var billToUpdate in sas148EventInfo.BillsToUpdate)
				{
					var billNo = billToUpdate.BillNo;
					var bill = header.Bills.FirstOrDefault(x => x.JPB_BillNumber == billNo);
					if (bill == null)
					{
						logger.Log(Integration.LogType.Warning, string.Format(CultureInfo.InvariantCulture, "Bill '{0}' could not be found on AFR Job '{1}'.", billNo, header.JPH_JobReference));
					}
					else
					{
						var discrepancyCode = billToUpdate.DiscrepancyCode;
						var newStatus = GetNewReleaseStatus(bill.JPB_ReleaseStatus, sas148EventInfo.MasterBillIdentifier, sas148EventInfo.AreAllVesselDetailEmpty, discrepancyCode, isDeletionNotEmpty);
						VesselInfoUpdater.UpdateBillReleaseStatus(logger, bill, newStatus);
					}
				}
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		static ZString GetNewReleaseStatus(ZString oldReleaseStatus, ZString masterBillIdentifer, ZBool areAllNewVesselDetailsEmpty, ZString discrepancyCode, bool isDeletionNotEmpty)
		{
			var result = oldReleaseStatus;

			if (masterBillIdentifer.Contains("M", StringComparison.OrdinalIgnoreCase))
			{
				if (discrepancyCode.IsEmpty)
				{
					result = GetNewStatusCode(oldReleaseStatus, AFRBillCustomsStatusList.Codes.Registered);
				}
				else if (discrepancyCode == Constants.SDiscrepancyCode)
				{
					result = GetNewStatusCode(oldReleaseStatus, AFRBillCustomsStatusList.Codes.NL3);
				}
			}
			else if (masterBillIdentifer.IsEmpty)
			{
				if (!areAllNewVesselDetailsEmpty)
				{
					if (discrepancyCode.IsEmpty)
					{
						result = GetNewStatusCode(oldReleaseStatus, AFRBillCustomsStatusList.Codes.NL1);
					}
					else if (discrepancyCode == Constants.SDiscrepancyCode)
					{
						result = GetNewStatusCode(oldReleaseStatus, AFRBillCustomsStatusList.Codes.NL4);
					}
				}
				else if (isDeletionNotEmpty)
				{
					result = GetNewStatusCode(oldReleaseStatus, AFRBillCustomsStatusList.Codes.NL2);
				}
			}

			return result;
		}

		static ZString GetNewStatusCode(ZString oldValue, ZString newValue)
		{
			var result = oldValue;
			if (oldValue == AFRBillCustomsStatusList.Codes.Registered
				|| oldValue == AFRBillCustomsStatusList.Codes.NL1
				|| oldValue == AFRBillCustomsStatusList.Codes.NL2
				|| oldValue == AFRBillCustomsStatusList.Codes.NL3
				|| oldValue == AFRBillCustomsStatusList.Codes.NL4
				|| (oldValue == AFRBillCustomsStatusList.Codes.NL5 && newValue != AFRBillCustomsStatusList.Codes.NL2)
				)
			{
				result = newValue;
			}
			return result;
		}

		protected override string GetMessageTypeDesc(JPAFRHeader header)
		{
			return MessagingTypeList.Descriptions.NotificationnofMasterBillRegistrationStatus;
		}

		protected override bool IsSuccessResponse => true;

		readonly SAS148EventInfo sas148EventInfo;

		static class Constants
		{
			public const string SDiscrepancyCode = "S";
		}
	}
}
