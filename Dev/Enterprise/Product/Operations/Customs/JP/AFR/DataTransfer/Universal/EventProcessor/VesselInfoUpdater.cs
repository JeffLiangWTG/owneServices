using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public static class VesselInfoUpdater
	{
		public static void UpdateVesselInformationDetailsForSAS148(IXmlImportLogger logger, JPAFRHeader header, ZString newCarrierCode,
			ZString newVesselCallSign, ZString newVoyageNumber, ZString newLoadingPortCode, ZString newLoadingPortSuffix, ZString newVesselName,
			ZDateTime newETA, ZDateTime newETD)
		{
			var jobReference = header.JPH_JobReference;

			UpdateVesselInformation(logger, jobReference, header.JPH_CarrierCodeInfo, newCarrierCode);

			if (!newVesselName.IsEmpty || !newVesselCallSign.IsEmpty)
			{
				if (!newVesselName.IsEmpty)
				{
					var vessel = GetVesselByName(header.Factory, newVesselName);
					if (vessel == null)
					{
						logger.Log(Integration.LogType.Error,
							string.Format(CultureInfo.InvariantCulture, "Vessel name '{0}' is not on file.",
								newVesselName));
					}
					UpdateVesselNameWithCallSignAndCountryofReg(logger, header, newVesselName, vessel?.RV_RadioCallSign, vessel?.RV_RN_NKCountryOfReg);
				}
				else
				{
					if (newVesselCallSign != header.JPH_RadioCallSign)
					{
						var vessel = GetVesselByCallSign(header.Factory, newVesselCallSign);
						if (vessel == null)
						{
							logger.Log(Integration.LogType.Error,
								string.Format(CultureInfo.InvariantCulture, "Vessel can not be found via Vessel Call Sign '{0}'.",
									newVesselCallSign));
						}
						UpdateVesselNameWithCallSignAndCountryofReg(logger,
							header,
							vessel?.RV_Code ?? new ZString("Vessel not on file " + newVesselCallSign),
							newVesselCallSign,
							vessel?.RV_RN_NKCountryOfReg);
					}
				}
			}
			else
			{
				UpdateVesselNameWithCallSignAndCountryofReg(logger, header, ZString.Empty, ZString.Empty, ZString.Empty);
			}

			UpdateVesselInformation(logger, jobReference, header.JPH_VoyageInfo, newVoyageNumber);
			UpdateVesselInformation(logger, jobReference, header.JPH_RL_NKLoadingInfo, newLoadingPortCode);
			UpdateVesselInformation(logger, jobReference, header.JPH_LoadingPortSuffixInfo, newLoadingPortSuffix);
			if (!newETA.IsEmpty)
			{
				UpdateVesselInformation(logger, jobReference, header.JPH_ETAInfo, newETA);
			}

			if (!newETD.IsEmpty)
			{
				UpdateVesselInformation(logger, jobReference, header.JPH_ETDInfo, newETD);
			}
		}

		public static void UpdateVesselInformationDetails(IXmlImportLogger logger, JPAFRHeader header, ZString newCarrierCode, ZString newVesselCallSign, ZString newVoyageNumber, ZString newLoadingPortCode, ZString newLoadingPortSuffix)
		{
			var jobReference = header.JPH_JobReference;

			UpdateVesselInformation(logger, jobReference, header.JPH_CarrierCodeInfo, newCarrierCode);

			if (!newVesselCallSign.IsEmpty)
			{
				var vessel = GetVesselByCallSign(header.Factory, newVesselCallSign);
				if (vessel == null)
				{
					logger.Log(Integration.LogType.Warning,
						string.Format(CultureInfo.InvariantCulture, "Vessel can not be found via Vessel Call Sign '{0}'.",
							newVesselCallSign));
				}
				UpdateVesselNameWithCallSignAndCountryofReg(logger,
					header,
					vessel?.RV_Code ?? ZString.Empty,
					newVesselCallSign,
					vessel?.RV_RN_NKCountryOfReg);
			}
			else
			{
				UpdateVesselNameWithCallSignAndCountryofReg(logger, header, ZString.Empty, ZString.Empty, ZString.Empty);
			}

			UpdateVesselInformation(logger, jobReference, header.JPH_VoyageInfo, newVoyageNumber);
			UpdateVesselInformation(logger, jobReference, header.JPH_RL_NKLoadingInfo, newLoadingPortCode);
			UpdateVesselInformation(logger, jobReference, header.JPH_LoadingPortSuffixInfo, newLoadingPortSuffix);
		}

		public static void UpdateBillReleaseStatus(IXmlImportLogger logger, JPAFRBills bill, ZString newStatus)
		{
			UpdatePropertyCore(bill.JPB_ReleaseStatusInfo, newStatus,
				(oValue, nValue) =>
				{
					logger.Log(Integration.LogType.Information, string.Format(CultureInfo.InvariantCulture, "The Bill '{0}' Customs Status of AFR Job '{1}' changes from '{2}' to '{3}'.", bill.JPB_BillNumber, bill.Header.JPH_JobReference, oValue, nValue));
				}
			);
		}

		static RefVessel GetVesselByCallSign(BusinessObjectFactory factory, ZString vesselCallSign)
		{
			var query = new ZQuery(RefVesselSchema.RV_RadioCallSign, vesselCallSign);
			return factory.LoadTop1<RefVessel>(query);
		}

		static RefVessel GetVesselByName(BusinessObjectFactory factory, ZString vesselName)
		{
			var query = new ZQuery(RefVesselSchema.RV_Code, vesselName);
			return factory.LoadTop1<RefVessel>(query);
		}

		static void UpdateVesselNameWithCallSignAndCountryofReg(IXmlImportLogger logger, JPAFRHeader header, ZString? newVesselName, ZString? newCallSign, ZString? newCountryOfReg)
		{
			var jobReference = header.JPH_JobReference;
			if (newVesselName.HasValue)
			{
				UpdateVesselInformation(logger, jobReference, header.JPH_VesselNameInfo, newVesselName.Value);
			}
			if (newCallSign.HasValue)
			{
				UpdateVesselInformation(logger, jobReference, header.JPH_RadioCallSignInfo, newCallSign.Value);
			}
			if (newCountryOfReg.HasValue)
			{
				UpdateVesselInformation(logger, jobReference, header.JPH_RN_NKCountryOfRegInfo, newCountryOfReg.Value);
			}
		}

		static void UpdateVesselInformation(IXmlImportLogger logger, ZString jobReference, ZPropertyInfo propertyInfo, ZDateTime newValue)
		{
			UpdatePropertyCore(propertyInfo, newValue,
				(oValue, nValue) =>
				{
					logger.Log(Integration.LogType.Information, ZString.Format("The {0} of AFR Job '{1}' changes from '{2}' to '{3}'.", propertyInfo.Description, jobReference, oValue, nValue));
				});
		}

		static void UpdatePropertyCore(ZPropertyInfo propertyInfo, ZDateTime newValue, LogHandler<ZDateTime> logHandler)
		{
			var oldValue = new ZDateTime(propertyInfo.Value);
			if (oldValue != newValue)
			{
				propertyInfo.Value = newValue;
				logHandler?.Invoke(oldValue, newValue);
			}
		}

		static void UpdateVesselInformation(IXmlImportLogger logger, ZString jobReference, ZPropertyInfo propertyInfo, ZString newValue)
		{
			UpdatePropertyCore(propertyInfo, newValue,
				(oValue, nValue) =>
				{
					logger.Log(Integration.LogType.Information, ZString.Format("The {0} of AFR Job '{1}' changes from '{2}' to '{3}'.", propertyInfo.Description, jobReference, oValue, nValue));
				});
		}

		static void UpdatePropertyCore(ZPropertyInfo propertyInfo, ZString newValue, LogHandler<ZString> logHandler)
		{
			newValue = newValue.Trim();
			var oldValue = new ZString(propertyInfo.Value);
			if (oldValue != newValue)
			{
				propertyInfo.Value = newValue;
				logHandler?.Invoke(oldValue, newValue);
			}
		}

		delegate void LogHandler<in T>(T oldValue, T newValue);
	}
}
