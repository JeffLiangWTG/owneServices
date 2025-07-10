using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[CodeProperty(Schema.CX_HouseBill), DescriptionProperty(Schema.CX_HouseBill)]
	public class CusSCADepotHouse : AutoCusSCADepotHouse, ICusUnderbondDependentCollectionParent
	{
		#region Constructors

		public CusSCADepotHouse(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region Static Loaders

		public static CusSCADepotHouse Load(BusinessObjectFactory factory, ZString houseBillNumber, ZString oceanBillNumber, ZString status, ZString lloydsNumber, ZString voyageNumber)
		{
			CusSCADepotHouse result = null;

			var expectedHouseBillNumber = houseBillNumber.IsEmpty ? oceanBillNumber : houseBillNumber;
			var houseFilter = new ZQuery(CusSCADepotHouseSchema.CX_HouseBill, expectedHouseBillNumber);
			houseFilter.AddToFilter(CusSCADepotHouseSchema.CX_Status, status);
			var existingHouses = (CusSCADepotHouse[])factory.Load(typeof(CusSCADepotHouse), houseFilter);

			foreach (var possibleHouse in existingHouses)
			{
				var container = possibleHouse.Container;
				if (container != null && container.CJ_LloydsNumber.ToString() == lloydsNumber && container.CJ_Voyage == voyageNumber)
				{
					result = possibleHouse;
					break;
				}
			}

			return result;
		}

		#endregion

		#region Related Business Objects

		public CusSCADepotContainer Container
		{
			get { return Factory.Load<CusSCADepotContainer>(CX_CJ); }
		}

		#endregion

		#region Business Object Overrides

		#region Clone

		public new CusSCADepotHouse Clone()
		{
			return (CusSCADepotHouse)base.Clone();
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#endregion

		#region CopyPersistentValuesFrom

		public void CopyPersistentValuesFrom(CusSCADepotHouse sourceObject)
		{
			base.CopyPersistentValuesFrom(sourceObject);
		}

		#endregion

		#region FromShipment

		public void FromShipment(CommonShipment shipment)
		{
			var damaged = 0;
			var pillaged = 0;
			var outturn = 0;
			var totalPackageCount = 0;
			foreach (var packLine in shipment.OuterPackLines.Cast<PackLine>())
			{
				damaged += packLine.JL_Damaged;
				pillaged += packLine.JL_Pillaged;
				outturn += packLine.JL_Outturn;
				totalPackageCount += packLine.JL_PackageCount;
			}
			if (shipment.HandledOnBehalfOfForwarder != null && shipment.HandledOnBehalfOfForwarder.CustomsCodes != null)
			{
				CX_ClientID = shipment.HandledOnBehalfOfForwarder.LocalManifestID;
			}
			CX_Damaged = damaged;
			CX_HouseBill = shipment.JS_HouseBill;
			CX_PackageCount = totalPackageCount;
			CX_Pillaged = pillaged;
			CX_Short = outturn >= totalPackageCount ? 0 : totalPackageCount - outturn;
			CX_Surplus = outturn <= totalPackageCount ? 0 : outturn - totalPackageCount;
		}

		#endregion

		#region MessageDataEqual

		public bool MessageDataEqual(CusSCADepotHouse compareTo)
		{
			var result = compareTo.CX_ClientID == CX_ClientID
				&& compareTo.CX_Damaged == CX_Damaged
				&& compareTo.CX_HouseBill == CX_HouseBill
				&& compareTo.CX_PackageCount == CX_PackageCount
				&& compareTo.CX_Pillaged == CX_Pillaged
				&& compareTo.CX_Short == CX_Short
				&& compareTo.CX_Surplus == CX_Surplus;
			return result;
		}

		#endregion

		#region Properties

		public bool IsNILOutturn
		{
			get
			{
				return CX_Damaged == 0
					&& CX_Pillaged == 0
					&& CX_Short == 0
					&& CX_Surplus == 0;
			}
		}

		#endregion

		#region ICusUnderbondDependentCollectionParent Members

		public ZString Details
		{
			get
			{
				return CX_HouseBill;
			}
		}

		public IOutturnableLine[] OutturnableLines
		{
			get
			{
				return Underbonds.Cast<CusUnderbond>().SelectMany(x => x.Outturns.Cast<CusOutturn>()).Select(x => x.Parent).Where(x => x != null).Distinct().ToArray();
			}
		}

		Customs.Business.CusUnderbondCollection ICusUnderbondDependentCollectionParent.Underbonds => Underbonds;
		[ChildEditable(true)]
		public CusUnderbondCollection Underbonds
		{
			get
			{
				if (fUnderbonds == null)
				{
					fUnderbonds = new CusUnderbondCollection(this);
					fUnderbonds.Load();
					RegisterEditableChildObject(fUnderbonds);
				}
				return fUnderbonds;
			}
		}
		CusUnderbondCollection fUnderbonds;

		ZString IOutturnableLine.CargoStatus
		{
			get { return CX_Status; }
		}

		ZInt IOutturnableLine.PackagesManifested
		{
			get { return CX_PackageCount; }
		}

		bool ICusUnderbondDependentCollectionParent.CanSendWithoutDelay
		{
			get { return true; }
		}

		public ZString UnderbondHumanReadableName
		{
			get
			{
				return "SHIPMENT " + CX_HouseBill;
			}
		}

		bool ICusUnderbondDependentCollectionParent.UsesTranshipmentPortOnUnderbond
		{
			get { return false; }
		}

		ZString ICusUnderbondDependentCollectionParent.DefaultTranshipmentPort
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}
