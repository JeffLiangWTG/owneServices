using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.SeaManifest;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class CMRCusSCAOceanBillDataObjectReader : CusSCAOceanBillDataObjectReader<CusSCAOceanBill, CusSCAHouse, CusSCAContainer, CusSCAPivot>
	{
		public CMRCusSCAOceanBillDataObjectReader(Shipment dataObject, Shipment hVLVConsolidatorShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, hVLVConsolidatorShipment, logger, factory)
		{
			this.hvlvConsolidatorShipmentWrapper = hVLVConsolidatorShipment != null ? new HVLVShipmentDataObjectWrapper(hVLVConsolidatorShipment, dataObject, factory) : null;
		}
		readonly HVLVShipmentDataObjectWrapper hvlvConsolidatorShipmentWrapper;

		#region Overrides

		protected override TransportLeg GetArrivalVoyage()
		{
			return dataObject.GetArrivalTransportLeg(TransportMode.Sea);
		}

		protected override void PopulateCountrySpecificDetails(IColumnIndexer oceanBill, Dictionary<string, ValueSetter> valueSetters)
		{
			base.PopulateCountrySpecificDetails(oceanBill, valueSetters);
			var targetBO = oceanBill as CusSCAOceanBill;
			if (targetBO != null)
			{
				PopulateWorkflowCustomFields(targetBO, dataObject);

				if (IsInHVLVProcess)
				{
					SetValue(targetBO, CusSCAOceanBillSchema.CB_DateOfDeparture, new HVLVShipmentDataObjectWrapper(HVLVShipperConsolidation, dataObject, factory).DepartureDate, valueSetters);
				}
			}
		}

		protected override void ProcessPopulatedHouseBills(IColumnIndexer oceanBill, List<CusSCAHouse> houseBills)
		{
			base.ProcessPopulatedHouseBills(oceanBill, houseBills);
			(oceanBill as CusSCAOceanBill).HouseBills.Reload(false);
		}

		protected override CusSCAOceanBill GetNewBusinessObject()
		{
			var oceanBill = base.GetNewBusinessObject();
			return oceanBill;
		}

		protected override CusSCAOceanBill GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var result = base.GetExistingBusinessObjectUsingModuleSpecificBusinessRules();
			if (IsInHVLVProcess && result == null)
			{
				result = GetExistingBusinessObjectUsingGenPivotCollectionForHVLV();
			}

			return result;
		}

		CusSCAOceanBill GetExistingBusinessObjectUsingGenPivotCollectionForHVLV()
		{
			var result = default(CusSCAOceanBill);
			var shipment = factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, ShipmentJobNumber)).FirstOrDefault();
			if (shipment != null)
			{
				var header = shipment.HVLVConsignmentHeader;
				if (header != null)
				{
					var zQuery = new ZQuery(GenPivotSchema.XX_RelationType, GenPivotTypes.HighVolumeLowValue);
					zQuery.AddToFilter(GenPivotSchema.XX_Relation1ID, header.PK);
					zQuery.AddToFilter(GenPivotSchema.XX_Relation1TableCode, HVLVConsignmentHeaderSchema.Constants.Prefix);
					zQuery.AddToFilter(GenPivotSchema.XX_Relation2TableCode, CusSCAOceanBillSchema.Constants.Prefix);
					result = factory.Load<GenPivot>(zQuery).Select(genPivot => genPivot.Relation2Object as CusSCAOceanBill).FirstOrDefault(oceanBill => !oceanBill.IsCancelled);
				}
			}

			return result;
		}

		protected override ZString GetApplicationCode()
		{
			return Enterprise.Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
		}

		protected override CusSCAHouseDataObjectReader<CusSCAHouse, CusSCAPivot> GetNewCusSCAHouseDataObjectReader(IColumnIndexer oceanBill, Shipment subShipmentDataObject)
		{
			return new CMRCusSCAHouseDataObjectReader(oceanBill, hvlvConsolidatorShipmentWrapper, subShipmentDataObject, logger, factory);
		}

		protected override CusSCAContainerDataObjectReader<CusSCAContainer> GetNewCusSCAContainerDataObjectReader(IColumnIndexer oceanBill, Container containerDataObject)
		{
			return new CusSCAContainerDataObjectReader(oceanBill, hvlvConsolidatorShipmentWrapper, containerDataObject, logger, factory);
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(CusSCAOceanBill targetBO)
		{
			var builder = new ZStringBuilder(base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO));
			if (targetBO != null && targetBO.IsInDatabase && IsMessagingActive(targetBO))
			{
				var oceanBill = GetColumnIndexer(targetBO);
				var containerReaderList = new List<CusSCAContainerDataObjectReader>();
				if (dataObject.ContainerCollection != null)
				{
					foreach (var containerData in dataObject.ContainerCollection)
					{
						var containerReader = GetNewCusSCAContainerDataObjectReader(oceanBill, containerData) as CusSCAContainerDataObjectReader;
						containerReaderList.Add(containerReader);
					}
				}
				cusSCAContainerReaderCollection = containerReaderList;

				var error = CheckTheFieldsIsChangedAfterCargoReporting(targetBO, oceanBill);
				if (!error.IsEmpty)
				{
					builder.AppendLine(error);
				}
				foreach (var containerReader in containerReaderList)
				{
					var containerError = containerReader.CheckTheFieldsIsChangedAfterCargoReporting();
					if (!containerError.IsEmpty)
					{
						builder.AppendLine(containerError);
					}
				}
			}
			return builder.ToString();
		}

		ZString CheckTheFieldsIsChangedAfterCargoReporting(CusSCAOceanBill targetBO, IColumnIndexer oceanBill)
		{
			var builder = new ZStringBuilder();

			foreach (var setterInfo in GetValueSetters(oceanBill).OfType<IColumnValueSetterInfo>())
			{
				if (fieldsThatShouldNotChangeAfterCargoReporting.Contains(setterInfo.Column.Name))
				{
					var oldValue = setterInfo.Row.GetValue(setterInfo.Column) != null ? setterInfo.Row.GetValue(setterInfo.Column).ToString() : string.Empty;
					var newValue = setterInfo?.Value?.ToString();
					var propertyInfo = targetBO.ZPropertyInfoHash.GetPropertySafe(setterInfo.Column.Name);
					if (newValue != null && string.Compare(oldValue, newValue, System.StringComparison.OrdinalIgnoreCase) != 0)
					{
						builder.AppendLine(Res.GetString("68688A4D-E952-4477-A1A7-4DA77829A28D", "There is an attempt to update {0} from '{1}' to '{2}' on this Master/Sub-Master while it has at least one House Bill with an active messaging.",
							propertyInfo != null ? (string)propertyInfo.HumanReadableName : setterInfo.Column.Name, oldValue, newValue));
					}
				}
			}
			return builder.ToString();
		}

		protected override bool CheckUpdateCusSCAOceanBillDataIsAllowed(CusSCAOceanBill oceanBill)
		{
			var result = true;
			if (oceanBill != null && oceanBill.IsInDatabase && IsMessagingActive(oceanBill))
			{
				result = false;
				logger.LogBoth(LogType.Warning, Res.GetString("4CCD56A2-EDFC-4324-ADD3-B86E9F98FA10", "{0} data will not be updated as there is at least one House Bill with an active messaging.", oceanBill.HumanReadableName));
			}
			return result;
		}

		readonly string[] fieldsThatShouldNotChangeAfterCargoReporting = new string[]
		{
			CusSCAOceanBillSchema.CB_OceanBill.Name,
			CusSCAOceanBillSchema.CB_VesselName.Name,
			CusSCAOceanBillSchema.CB_Voyage.Name,
			CusSCAOceanBillSchema.CB_LloydsIMO.Name,
			CusSCAOceanBillSchema.CB_RL_NKPortOfLoading.Name,
			CusSCAOceanBillSchema.CB_RL_NKPortOfDischarge.Name,
			CusSCAOceanBillSchema.CB_RL_NKPortOfFirstArrival.Name,
			CusSCAOceanBillSchema.CB_ResponsiblePartyID.Name,
			CusSCAOceanBillSchema.CB_PrincipalID.Name,
			CusSCAOceanBillSchema.CB_MasterHouseBill.Name,
		};
		#endregion // Overrides

		#region Implementation

		static bool IsMessagingActive(CusSCAOceanBill oceanBill)
		{
			return oceanBill.HouseBills.Cast<CusSCAHouse>().Any(CMRCusSCAHouseDataObjectReader.IsMessagingActive);
		}

		#endregion // Implementation
	}
}
