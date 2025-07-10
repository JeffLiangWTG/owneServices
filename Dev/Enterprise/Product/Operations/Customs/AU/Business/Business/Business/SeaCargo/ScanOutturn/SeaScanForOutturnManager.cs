using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SeaScanForOutturnManager : ScanForOutturnManager
	{
		public SeaScanForOutturnManager(ScanCusSCAOceanBill scanObj)
			: base(scanObj)
		{ }

		protected new ScanCusSCAOceanBill ScanObj
		{
			get { return (ScanCusSCAOceanBill)base.ScanObj; }
		}

		public new SeaScanWizardDataSource ScanWizardDataSource
		{
			get { return (SeaScanWizardDataSource)base.ScanWizardDataSource; }
		}

		protected override ScanWizardDataSource GetScanWizardDataSource(ScanMasterBill scanObj)
		{
			return new SeaScanWizardDataSource((ScanCusSCAOceanBill)scanObj);
		}

		public override string ValidateStandAloneUnderbond()
		{
			return string.Empty;
		}

		protected override OutturnLine GetNewOutturnLine()
		{
			return new SeaOutturnLine();
		}

		public override OutturnLineCollection GetNewOutturnLineCollection()
		{
			return new SeaOutturnLineCollection(Factory);
		}

		SeaUnderbondSelectorLine SeaSelectedUnderbond
		{
			get { return (SeaUnderbondSelectorLine)base.SelectedUnderbond; }
		}

		public override string ValidateSelectedUnderbond()
		{
			var result = base.ValidateSelectedUnderbond();
			if (string.IsNullOrEmpty(result))
			{
				if (SeaSelectedUnderbond.OutturnStatus == OutturnStatus.AwaitingResponseFromCustoms)
				{
					result = WatingResponseFromCusotmsError;
				}
			}
			return result;
		}

		internal static string WatingResponseFromCusotmsError
		{
			get { return Res.GetString("16910799-CF4C-451E-A78E-AE24B508638E", "This outturn is currently waiting for a Customs response. You cannot continue until a reply is received."); }
		}

		public override string SaveOutturnResult(Customs.Business.ISendMessagesToCustomsExtraMembersAndDetermineRequiredMessagesAndSendThem sender)
		{
			if (IsOutturnCollectionEmpty)
			{
				return OutturnCollectionIsEmpty;
			}

			var outturnHeader = ScanObj.OutturnHeader;
			if (outturnHeader == null)
			{
				return NoOutturnHeader;
			}

			var factory = new BusinessObjectFactory();
			outturnHeader = factory.Load<CusOutturnHeader>(outturnHeader.PK);

			var containerNo = SelectedUnderbond.Underbond.ContainerNumber;

			IScanMasterBillProvider singleSurplusMaster = null;
			if (ScanObj.IsStandAlone)
			{
				singleSurplusMaster = ScanObj.MasterBill;
			}
			else if (ScanWizardDataSource.ShipmentSelectorLineCollection != null)
			{
				var selectedShipments = ScanWizardDataSource.ShipmentSelectorLineCollection.GetHLSShipmentIncludeInScan();
				if (!selectedShipments.Any())
				{
					return NoStandAloneSeaCargoForSurplusConsignment;
				}
				else if (selectedShipments.Count() == 1)
				{
					singleSurplusMaster = ScanObj.MasterBill.GetStandAloneOceanBill(selectedShipments.ElementAt(0).Shipment);
				}
			}

			foreach (SeaOutturnLine line in OutturnCollection)
			{
				if (line.HouseBill != null)//non-surplus
				{
					var error = AddOutturn(line, outturnHeader, containerNo);
					if (!error.IsNullOrEmpty())
					{
						return error;
					}
				}
			}

			foreach (SurplusOutturnLine line in ScanWizardDataSource.SurplusOutturnCollection)
			{
				var manifestInfo = GetManfifestInformation(ScanObj.MasterBill.CB_LloydsIMO, ScanObj.MasterBill.CB_Voyage, containerNo, line.ConsignmentRef);
				if (manifestInfo != null)
				{
					line.OutturnLine.ManifestInfo = manifestInfo;
					line.OutturnLine.HouseBill = manifestInfo.HouseBill;
					line.OutturnLine.Underbond = ScanObj.SelectedUnderbond;
				}

				var surplusMaster = singleSurplusMaster ?? ScanObj.MasterBill.GetStandAloneOceanBill(line.SelectedShipmentLine.Shipment);
				if (surplusMaster == null)
				{
					return NoStandAloneSeaCargoForSurplusConsignment;
				}

				line.OutturnLine.Underbond = ScanObj.SelectedUnderbond;
				surplusMaster.CreateSurplusConsignment(factory, line.OutturnLine);
				var error = AddOutturn(line.OutturnLine, outturnHeader, containerNo, CMROutturnResultType.Codes.SurplusConsignment);
				if (!error.IsNullOrEmpty())
				{
					return error;
				}
			}

			var manager = ((Customs.Business.IMessageManageableBizObj)outturnHeader).GetMessageManagerForAmendmentDetection();
			try
			{
				if (sender.DetermineRequiredMessagesAndSendThem(manager) == ContinueWithSave.Yes)
				{
					factory.Save();
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ZExceptionReporting.HandleSaveException(e);
			}

			return string.Empty;
		}

		string AddOutturn(OutturnLine line, CusOutturnHeader outturnHeader, ZString containerNo, string outturnResultType = null)
		{
			if (line.ManifestInfo != null && line.ManifestInfo.Quantity == 0 && line.Count == 0)
			{
				return string.Empty;
			}

			if (outturnResultType == null)
			{
				outturnResultType = GetOutturnResultType(line.ManifestInfo.Quantity, line.Count);
			}

			var outturnLine = outturnHeader.Outturns.Cast<CusOutturn>().FirstOrDefault(x => x.C5_HouseBill == line.HouseBill.HouseBill && x.C5_ContainerNumber == containerNo);
			if (outturnLine == null)
			{
				outturnLine = outturnHeader.Outturns.AddNew();
				outturnLine.C5_CargoType = CMRImportCargoTypes.Codes.LessThanContainerLoad;
				outturnLine.C5_HouseBill = line.HouseBill.HouseBill;
				outturnLine.C5_MasterBill = ((CusSCAHouse)line.HouseBill).OceanBill.CB_OceanBill;
				outturnLine.C5_ContainerNumber = containerNo;
				outturnLine.C5_OuterPacks = line.ManifestInfo.Quantity;
				outturnLine.C5_OuterPackUnits = line.ManifestInfo.UQ;
				outturnLine.C5_ParentID = line.ManifestInfo.PK;
				outturnLine.C5_ParentTableCode = line.ManifestInfo.TablePrefix;
			}

			if (outturnLine.C5_CargoUnpackDate.IsEmpty)
			{
				outturnLine.C5_CargoUnpackDate = ZDateTime.Now;
			}
			if (outturnLine.C5_PackagesUnits.IsEmpty)
			{
				outturnLine.C5_PackagesUnits = outturnLine.C5_OuterPackUnits;
			}
			switch (outturnResultType)
			{
				case CMROutturnResultType.Codes.NilDiscrepancy:
					outturnLine.C5_PackagesOutturned = line.Count;
					break;
				case CMROutturnResultType.Codes.SurplusConsignment:
				case CMROutturnResultType.Codes.SurplusPackages:
					outturnLine.C5_PackagesOutturned = line.Count;
					outturnLine.C5_GoodsDescription = line.ManifestInfo.GoodsDescription;
					outturnLine.C5_MarksAndNumbers = line.ManifestInfo.MarksAndNumbers;
					break;
				case CMROutturnResultType.Codes.ShortLanded:
					outturnLine.C5_PackagesOutturned = line.Count;
					break;
			}
			outturnLine.C5_OutturnResultType = outturnResultType;
			return string.Empty;
		}

		CusSCAPivot GetManfifestInformation(ZString lloyd, ZString voyageNo, ZString container, ZString house)
		{
			var queryText = String.Format(@" {0} IN (
SELECT {0} FROM {1}
		LEFT JOIN {2} ON {3} = {4}
		LEFT JOIN {5} ON {6} = {7}
		LEFT JOIN {8} ON {9} = {10}
WHERE {11} = @house AND {12} = @container AND {13} = @lloyd AND {14} = @voyage)"
, CusSCAPivotSchema.Constants.PK //0
, CusSCAPivotSchema.Constants.TableName //1
, CusSCAHouseSchema.Constants.TableName //2
, CusSCAHouseSchema.Constants.PK //3
, CusSCAPivotSchema.Constants.CV_CA //4
, CusSCAContainerSchema.Constants.TableName //5
, CusSCAContainerSchema.Constants.PK //6
, CusSCAPivotSchema.Constants.CV_CN //7
, CusSCAOceanBillSchema.Constants.TableName //8
, CusSCAContainerSchema.Constants.CN_CB //9
, CusSCAOceanBillSchema.Constants.PK //10
, CusSCAHouseSchema.Constants.CA_HouseBill //11
, CusSCAContainerSchema.Constants.CN_ContainerNumber //12
, CusSCAOceanBillSchema.Constants.CB_LloydsIMO //13
, CusSCAOceanBillSchema.Constants.CB_Voyage //14
);
			var query = new ZDBOnlyQuery(typeof(CusSCAPivot));
			query.AddFilterAndZSQLParameterCollection(queryText, new ZSqlParameterCollection() {
				ZSqlParameter.New("@house", house, CusSCAHouseSchema.CA_HouseBill),
				ZSqlParameter.New("@container", container, CusSCAContainerSchema.CN_ContainerNumber),
				ZSqlParameter.New("@lloyd", lloyd, CusSCAOceanBillSchema.CB_LloydsIMO),
				ZSqlParameter.New("@voyage", voyageNo, CusSCAOceanBillSchema.CB_Voyage)
			});
			return Factory.LoadTop1<CusSCAPivot>(query);
		}

		internal static string OutturnCollectionIsEmpty
		{
			get { return Res.GetString("4432477B-D216-4817-A207-8DF8F44F627A", "Outturn collection is empty."); }
		}

		internal static string NoOutturnHeader
		{
			get { return Res.GetString("3FC0B425-DA88-42F7-97F4-19CDBBC4711C", "There is no sea outturn header."); }
		}

		internal static string NoStandAloneSeaCargoForSurplusConsignment
		{
			get { return Res.GetString("37E53E99-77E9-4E93-B6BB-E096C440A322", "There is no standalone Sea Cargo job for surplus consignments."); }
		}
	}
}
