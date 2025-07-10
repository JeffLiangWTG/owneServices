using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TNT.AirCargo
{
	public class XXXAirCargo : ImportAirCargo
	{
		public XXXAirCargo(BusinessObjectFactory factory, FlightRecord flightDetail, ConsignmentRecord consignment, ZString branchCode)
			: base(factory, flightDetail, branchCode)
		{
			if (consignment == null)
			{
				throw new ArgumentNullException(nameof(consignment));
			}
			this.Consignment = consignment;
			InitialiseBoundProperties();
		}

		#region Bound Properties

		#region MatchedMasterBill

		public ZString MatchedMasterBill
		{
			get
			{
				CusHAWB airCargo = LinkedHouseBill;
				return airCargo == null ? ZString.Empty : airCargo.CS_MasterBillNum;
			}
		}

		public ZPropertyInfo MatchedMasterBillInfo
		{
			get { return GetZPropertyInfo(nameof(MatchedMasterBill)); }
		}

		#endregion

		#region HouseBill

		public ZString HouseBill
		{
			get { return Consignment.HouseBill; }
		}

		public ZPropertyInfo HouseBillInfo
		{
			get { return GetZPropertyInfo(nameof(HouseBill)); }
		}

		#endregion

		#region Origin

		public ZString Origin
		{
			get { return Consignment.Origin; }
		}

		public ZPropertyInfo OriginInfo
		{
			get { return GetZPropertyInfo(nameof(Origin)); }
		}

		#endregion

		#region Destination

		public ZString Destination
		{
			get { return Consignment.Destination; }
		}

		public ZPropertyInfo DestinationInfo
		{
			get { return GetZPropertyInfo(nameof(Destination)); }
		}

		#endregion

		#region Consignee

		public ZString Consignee
		{
			get { return Consignment.ConsigneeName; }
		}

		public ZPropertyInfo ConsigneeInfo
		{
			get { return GetZPropertyInfo(nameof(Consignee)); }
		}

		#endregion

		#region Consignor

		public ZString Consignor
		{
			get { return Consignment.ConsignorName; }
		}

		public ZPropertyInfo ConsignorInfo
		{
			get { return GetZPropertyInfo(nameof(Consignor)); }
		}

		#endregion

		#region PackageCount

		public ZShort PackageCount
		{
			get { return Consignment.PackageCount; }
		}

		public ZPropertyInfo PackageCountInfo
		{
			get { return GetZPropertyInfo(nameof(PackageCount)); }
		}

		#endregion

		#region LinkedHouseBillRef

		public ZString LinkedHouseBillRef
		{
			get
			{
				CusHAWB airCargo = LinkedHouseBill;
				return airCargo == null ? ZString.Empty : airCargo.CS_MessageReference;
			}
		}

		public ZPropertyInfo LinkedHouseBillRefInfo
		{
			get { return GetZPropertyInfo(nameof(LinkedHouseBillRef)); }
		}

		#endregion

		#region LinkedHouseBill

		public CusHAWB LinkedHouseBill
		{
			get { return (CusHAWB)Factory.Load(typeof(CusHAWB), LinkedHouseBillPK); }
		}

		#endregion

		#endregion

		public void ClearSearchData()
		{
			SearchMasterBill = "";
			SearchFlightNo = "";
			SearchArrivalDate = ZDateTime.Empty;
			SearchPortOfLoading = "";
			SearchPortOfDischarge = "";
		}

		public void AddConsignmentNotes(ConsignmentNoteRecord consignmentNote)
		{
			ConsignmentNotes.Add(consignmentNote);
		}

		#region Implementation

		void InitialiseBoundProperties()
		{
			if (SearchPortOfLoading.IsEmpty)
			{
				SetPropertyInfoValue(SearchPortOfLoadingInfo, Consignment.Origin, ForeignKeyType.PortNK);
			}

			if (SearchPortOfDischarge.IsEmpty)
			{
				SetPropertyInfoValue(SearchPortOfDischargeInfo, Consignment.Destination, ForeignKeyType.PortNK);
			}

			if (SearchArrivalDate.IsEmpty)
			{
				SearchArrivalDate = ZDateTime.Now;
			}
		}

		#region Update & Save Methods

		protected override void PopulateDataCore(CusMAWB existingCusMawb)
		{
			CusHAWB[] matchedCusHAWBs = (CusHAWB[])existingCusMawb.ChildBills.Find(new ZQuery(CusHAWBSchema.CS_HAWB, Consignment.HouseBill));

			if (matchedCusHAWBs.Length > 0)
			{
				Globals.Message.ShowWarning(string.Format("Housebill '{0}' already exist.{1}It will be ignored", Consignment.HouseBill, System.Environment.NewLine), "Housebill Already Exist");
			}
			else
			{
				CurrentHAWB = existingCusMawb.ChildBills.AddNew();
				ConsignmentUpdator updator = new ConsignmentUpdator(Consignment, ConsignmentNotes, Buffer);
				updator.Update(CurrentHAWB);
				updator.UpdateQuantumOriginalValue(CurrentHAWB, FlightDetail.RecordKey, BranchCode);
				MarkSurplusConsignment(CurrentHAWB);
			}
		}

		void MarkSurplusConsignment(CusHAWB hAWB)
		{
			CusUnderbond underbond = CreateNewCusUnderbond(hAWB);
			CusOutturn outTurn = underbond.Outturns.AddNew();
			outTurn.C5_PackagesOutturned = hAWB.CS_PiecesLanded;
			outTurn.Parent = hAWB;
			outTurn.C5_OutturnResultType = CMROutturnResultType.Codes.SurplusConsignment;
		}

		CusUnderbond CreateNewCusUnderbond(CusHAWB hAWB)
		{
			CusUnderbond result = hAWB.AllUnderbonds.AddNew();
			result.C4_ParentID = hAWB.PK;
			result.C4_ModeOfMovement = CMRUnderbondModeOfMovement.Codes.Air;
			return result;
		}

		protected override void SaveCore()
		{
			if (CurrentHAWB != null)
			{
				LinkedHouseBillPK = CurrentHAWB.PK;
				CurrentHAWB = null;
			}
		}

		#endregion

		#region ConsignmentNotes

		internal ConsignmentNoteRecordCollection ConsignmentNotes
		{
			get
			{
				if (fConsignmentNotes == null)
				{
					fConsignmentNotes = new ConsignmentNoteRecordCollection();
				}
				return fConsignmentNotes;
			}
		}

		ConsignmentNoteRecordCollection fConsignmentNotes;

		#endregion

		internal readonly ConsignmentRecord Consignment;
		CusHAWB CurrentHAWB;
		ZGuid LinkedHouseBillPK;

		#endregion
	}
}
