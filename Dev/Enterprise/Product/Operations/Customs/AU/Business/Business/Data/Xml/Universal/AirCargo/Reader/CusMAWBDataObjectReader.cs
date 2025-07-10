using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusMAWBDataObjectReader : DataTransfer.Universal.AirManifest.CusMAWBDataObjectReader<CusMAWB, CusHAWB, AirManifestDataObjectReaderHelper>
	{
		public CusMAWBDataObjectReader(Shipment mawbDataObject, Shipment hVLVShipperConsolidation, IXmlImportLogger logger, UniversalObjectFactory factory, bool singleHAWBCheck = false)
			: base(mawbDataObject, hVLVShipperConsolidation, logger, factory, Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages, singleHAWBCheck)
		{
		}

		protected override CusMAWB GetNewBusinessObject()
		{
			var mawb = base.GetNewBusinessObject();

			if (IsHVLVProcess)
			{
				mawb.NeedUpdateFlightDetailFromCARST = true;
			}

			return mawb;
		}

		protected override CusMAWB GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			CusMAWB result = null;
			var masterBill = dataObject.GetMasterBill();
			if (!masterBill.IsEmpty)
			{
				var billType = dataObject.WayBillType.GetCodeAsUpperCase();
				if (!billType.IsEmpty && billType == WayBillTypeList.Codes.Master)
				{
					var coLoadBill = dataObject.GetColoadBill(HVLVShipperConsolidation).GetValueOrDefault();

					var query = new ZQuery(CusMAWBSchema.CM_MAWB, masterBill);
					query.AddToFilter(CusMAWBSchema.CM_ApplicationCode, applicationCode);
					var mAWBRecyclePeriod = Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.Value;
					if (mAWBRecyclePeriod > 0)
					{
						query.AddToFilter(CusMAWBSchema.CM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Now.AddMonths(-mAWBRecyclePeriod));
					}
					query.AddToFilter(CusMAWBSchema.CM_IsCTOMAWB, ZBool.False);
					query.OrderBy = CusMAWBSchema.Constants.CM_SystemCreateTimeUtc + " desc";
					query.AddToFilter(CusMAWBSchema.CM_MasterHouseBill, coLoadBill);

					if (IsHVLVProcess && coLoadBill.IsEmpty && !string.IsNullOrEmpty(ShipmentJobNumber))
					{
						var bills = factory.Load<CusMAWB>(query);
						result = GetMostInterestingExistingBusinessObjectForHVLV(bills);
					}
					else
					{
						result = factory.LoadTop1<CusMAWB>(query);
					}
				}
			}
			return result;
		}

		#region HVLV

		bool IsHVLVProcess => HVLVShipperConsolidation != null;

		string ShipmentJobNumber => shipmentJobNumber ?? (shipmentJobNumber = HVLVShipperConsolidation.DataContext?.DataSourceCollection?.FirstOrDefault()?.Key);
		string shipmentJobNumber;

		CusMAWB GetMostInterestingExistingBusinessObjectForHVLV(CusMAWB[] bills)
		{
			return bills?.FirstOrDefault(b => b.Logs.GetAllLogs().Cast<StmALog>().Any(l =>
				l.Parameters.TryGetValue(EventReferenceParameters.Codes.Type, out var type) && type == Core.Constants.ShipmentTypes.HighVolumeLowValue
				&& (l.Parameters.TryGetValue(EventReferenceParameters.Codes.JobNumber, out var jobNumber) && jobNumber == ShipmentJobNumber
				|| l.Parameters.TryGetValue(EventReferenceParameters.Codes.ReferenceNumber, out var refNumber) && refNumber == ShipmentJobNumber)));
		}

		#endregion

		protected override ZString? GetResponsiblePartyID(ZGuid responsiblePartyPK, ZString? responsiblePartyID)
		{
			var result = responsiblePartyID;
			if (!responsiblePartyPK.IsEmpty && result.GetValueOrDefault().IsEmpty)
			{
				var responsiblePartyIDQuery = new ZQuery(OrgCusCodeSchema.OK_OH, responsiblePartyPK);
				responsiblePartyIDQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.Australia);
				responsiblePartyIDQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, new[] { OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, OrgCusCode.CodeTypes.CustomsClientID });
				var cusCodes = factory.BOFactory.Load<OrgCusCode>(responsiblePartyIDQuery);
				if (cusCodes.Any())
				{
					var cusCode = cusCodes.FirstOrDefault(x => x.OK_CodeType == OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber) ?? cusCodes.FirstOrDefault(x => x.OK_CodeType == OrgCusCode.CodeTypes.CustomsClientID);
					if (cusCode != null)
					{
						result = cusCode.OK_CustomsRegNo;
					}
				}
			}
			return result;
		}

		protected override IEnumerable<DataTransfer.Universal.AirManifest.CusHAWBDataObjectReader<CusMAWB, CusHAWB, AirManifestDataObjectReaderHelper>> GetNewCusHAWBDataObjectReaders(Shipment shipmentDataObject, Shipment mawbDataObject, CusMAWB mawb)
		{
			yield return new CusHAWBDataObjectReader(shipmentDataObject, mawbDataObject, logger, Helper, mawb, null, IsHVLV, singleHAWBCheck);
		}

		protected override AirManifestDataObjectReaderHelper CreateNewUniversalDataObjectReaderHelper()
		{
			return new AirManifestDataObjectReaderHelper(factory, Core.Constants.CountryCodes.Australia);
		}

		protected override bool CheckUpdateMAWBDataIsAllowed(CusMAWB mawb, IEnumerable<ValueSetter> valueSetters)
		{
			var result = true;
			if (mawb.IsInDatabase && IsMessagingActive(mawb))
			{
				result = false;
				logger.LogBoth(LogType.Warning, Res.GetString("4CCD56A2-EDFC-4324-ADD3-B86E9F98FA10", "{0} data will not be updated as there is at least one House Bill with an active messaging.", mawb.HumanReadableName));
			}
			return result;
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(CusMAWB mawb)
		{
			var builder = new ZStringBuilder(base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(mawb));
			if (builder.IsEmpty && mawb != null && !singleHAWBCheck && mawb.IsInDatabase && IsMessagingActive(mawb))
			{
				var setters = GetValueSetters(mawb).OfType<IColumnValueSetterInfo>().ToList();
				foreach (var setterInfo in setters)
				{
					if (FieldsThatShouldNotChangeAfterCargoReporting.Contains(setterInfo.Column.Name))
					{
						var oldValue = setterInfo.Row.GetValue(setterInfo.Column);
						var newValue = setterInfo.Value;
						var propertyInfo = mawb.ZPropertyInfoHash.GetPropertySafe(setterInfo.Column.Name);
						if (newValue != null && oldValue.CompareTo(newValue) != 0)
						{
							builder.AppendLine(AttemptToUpdateFieldWhileMessagingIsActive(propertyInfo != null ? (string)propertyInfo.HumanReadableName : setterInfo.Column.Name, oldValue, newValue));
						}
					}
				}
				var arrivalDateSetterInfo = setters.FirstOrDefault(x => x.Column.Name == CusMAWBSchema.CM_ArrivalDate.Name);
				if (arrivalDateSetterInfo != null)
				{
					var newValue = (ZDateTime?)arrivalDateSetterInfo.Value;
					if (newValue != null && mawb.CM_ArrivalDate.Date.CompareTo(newValue.Value.Date) != 0)
					{
						builder.AppendLine(AttemptToUpdateFieldWhileMessagingIsActive(mawb.CM_ArrivalDateInfo.HumanReadableName, mawb.CM_ArrivalDate, newValue));
					}
				}
			}
			return builder.ToString();
		}

		static string AttemptToUpdateFieldWhileMessagingIsActive(string fieldName, object oldValue, object newValue)
		{
			return Res.GetString("68688A4D-E952-4477-A1A7-4DA77829A28D", "There is an attempt to update {0} from '{1}' to '{2}' on this Master/Sub-Master while it has at least one House Bill with an active messaging.", fieldName, oldValue, newValue);
		}

		protected override TransportLeg GetArrivalFlight(CusMAWB mawb)
		{
			return dataObject.GetArrivalTransportLeg(TransportMode.Air);
		}

		protected override void FillCountrySpecificDetails(CusMAWB mawb, Dictionary<string, ValueSetter> valueSetters)
		{
			base.FillCountrySpecificDetails(mawb, valueSetters);
			var mawbRow = GetColumnIndexer(mawb);
			if (IsHVLVProcess)
			{
				SetValue(mawbRow, CusMAWBSchema.CM_DepartureDate, new HVLVShipmentDataObjectWrapper(HVLVShipperConsolidation, dataObject, factory).DepartureDate, valueSetters);
			}
			PopulateWorkflowCustomFields(mawb, dataObject);
		}

		static IEnumerable<string> FieldsThatShouldNotChangeAfterCargoReporting
		{
			get
			{
				yield return CusMAWBSchema.CM_MAWB.Name;
				yield return CusMAWBSchema.CM_MasterHouseBill.Name;
				yield return CusMAWBSchema.CM_FlightNo.Name;
				yield return CusMAWBSchema.CM_RL_NKLoadPort.Name;
				yield return CusMAWBSchema.CM_RL_NKDischargePort.Name;
				yield return CusMAWBSchema.CM_OH_ResponsibleParty.Name;
				yield return CusMAWBSchema.CM_ResponsiblePartyID.Name;
				yield return CusMAWBSchema.CM_RL_NKFirstArrivalPort.Name;
				yield return CusMAWBSchema.CM_OA_UnpackDepotAddress.Name;
			}
		}

		static bool IsMessagingActive(CusMAWB mawb)
		{
			return mawb.ChildBills.Cast<CusHAWB>().Any(CusHAWBDataObjectReader.IsMessagingActive);
		}
	}
}
