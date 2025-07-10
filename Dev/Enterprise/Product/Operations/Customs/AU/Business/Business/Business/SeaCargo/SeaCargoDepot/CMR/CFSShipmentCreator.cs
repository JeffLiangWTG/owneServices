using CargoWise.EntityFramework;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CFSShipmentCreator : CFSRecordCreator
	{
		public CFSShipmentCreator(DepotCusOutturn outturn, BusinessObjectFactory factory)
			: base(outturn, factory)
		{
			CreateConsol(outturn);
			CreateContainer(outturn);
			CreateShipment(outturn);
		}

		public CFSShipmentCreator(DepotCusOutturn outturn, CFSLoadListConsol consol, BusinessObjectFactory factory)
			: base(outturn, factory)
		{
			fConsol = consol;
			CreateContainer(outturn);
			CreateShipment(outturn);
		}

		public CFSLoadListConsol Consol
		{
			get { return fConsol; }
		}
		CFSLoadListConsol fConsol;

		public CFSContainer Container
		{
			get { return fContainer; }
		}
		CFSContainer fContainer;

		public CFSShipment Shipment
		{
			get { return fShipment; }
		}
		CFSShipment fShipment;

		#region Implementation

		void CreateConsol(DepotCusOutturn outturn)
		{
			fConsol = FindExistingConsol(outturn);
			if (Consol == null)
			{
				fConsol = new CFSLoadListConsolCreator(outturn, Factory).Consol;
				fConsol.JK_MasterBillNum = outturn.C5_MasterBill;
			}
		}

		void CreateContainer(DepotCusOutturn outturn)
		{
			if (outturn.C5_CargoType != CMRImportCargoTypes.Codes.BreakBulk &&
				outturn.C5_CargoType != CMRImportCargoTypes.Codes.Bulk)
			{
				fContainer = FindExistingContainer(outturn, Consol);
				if (Container == null)
				{
					fContainer = new CFSContainerCreator(outturn, Consol, Factory).Container;
					fContainer.JC_ContainerMode = ContainerModeFromOutturn(outturn);
				}
				if (Container != null && outturn.Underbond != null && outturn.Underbond.C4_ParentID.IsEmpty)
				{
					outturn.Underbond.C4_ParentID = Container.PK;
				}
			}
		}

		void CreateShipment(DepotCusOutturn outturn)
		{
			fShipment = FindExistingShipment(outturn);
			if (fShipment == null)
			{
				fShipment = Consol.Shipments.AddNew();
				fShipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				fShipment.JS_PackingMode = ContainerModeFromOutturn(outturn);
				fShipment.JS_HouseBill = outturn.C5_HouseBill.Left(CFSShipment.Schema.JS_HouseBillMaxLength);
				CFSPackLine packLine = null;
				if (fShipment.OuterPackLines.Count == 0 && Container != null)
				{
					foreach (CFSPackLine containerPackLine in Container.PackLines)
					{
						if (containerPackLine.JL_JS == fShipment.PK)
						{
							packLine = containerPackLine;
							break;
						}
					}
				}
				if (packLine == null)
				{
					packLine = fShipment.OuterPackLines.AddNew();
				}
				if (Container != null)
				{
					packLine.SetContainer(Container.PK);
				}

				if (outturn.C5_OuterPacks != 0)
				{
					packLine.JL_PackageCount = outturn.C5_OuterPacks;
					fShipment.JS_OuterPacks = outturn.C5_OuterPacks;
				}
				if (!outturn.C5_OuterPackUnits.IsEmpty)
				{
					packLine.JL_F3_NKPackType = SeaCargoUtilities.ConvertCMRPackageTypeToPkgUnit(outturn.C5_OuterPackUnits);
				}
				fShipment.JS_GoodsDescription = outturn.C5_GoodsDescription.Left(fShipment.JS_GoodsDescriptionInfo.MaxLength);
				fShipment.JS_MarksAndNumbers = outturn.C5_MarksAndNumbers;
				if (outturn.SEIMatched)
				{
					fShipment.JS_ActualVolume = outturn.SEIMessageLine.Volume.ResultDecimal;
					fShipment.JS_UnitOfVolume = SeaCargoUtilities.ConvertCMRVolumeUnitToVolumeUnit(outturn.SEIMessageLine.Volume.ResultString);
					fShipment.JS_ActualWeight = outturn.SEIMessageLine.GrossWeight.ResultDecimal;
					fShipment.JS_UnitOfWeight = outturn.SEIMessageLine.GrossWeight.ResultString;
				}
			}
			outturn.C5_ParentID = Shipment.PK;
			outturn.C5_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			if ((outturn.C5_CargoType == CMRImportCargoTypes.Codes.BreakBulk ||
				outturn.C5_CargoType == CMRImportCargoTypes.Codes.Bulk) && outturn.Underbond != null)
			{
				outturn.Underbond.C4_ParentID = Shipment.PK;
				outturn.Underbond.C4_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			}
		}

		CFSShipment FindExistingShipment(DepotCusOutturn outturn)
		{
			CFSShipment result = null;
			if (!outturn.C5_HouseBill.IsEmpty)
			{
				ZQuery houseBillFilter = new ZQuery(JobShipmentSchema.JS_HouseBill, outturn.C5_HouseBill);
				CFSShipment[] existingHouseBills = (CFSShipment[])Factory.Load(typeof(CFSShipment), houseBillFilter);
				foreach (CFSShipment existingShipment in existingHouseBills)
				{
					if (existingShipment.ArrivalConsol != null && ConsolVoyageMatchesOutturn((CFSLoadListConsol)existingShipment.ArrivalConsol, outturn.Header))
					{
						result = existingShipment;
						return result;
					}
				}
			}
			return result;
		}

		#endregion
	}
}
