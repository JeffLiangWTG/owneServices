using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TNT.AirCargo
{
	public class IQDownAirCargo : ImportAirCargo
	{
		public IQDownAirCargo(BusinessObjectFactory factory, FlightRecord flightDetail, ZString branchCode)
			: base(factory, flightDetail, branchCode)
		{
		}

		#region Bound Properties

		#region NoOfHouseBills

		public ZInt NoOfHouseBills
		{
			get { return ConsignmentDetails.Count; }
		}

		public ZPropertyInfo NoOfHouseBillsInfo
		{
			get { return GetZPropertyInfo(nameof(NoOfHouseBills)); }
		}

		#endregion

		#region LinkedMasterBillRef

		public ZString LinkedMasterBillRef
		{
			get
			{
				CusMAWB airCargo = LinkedMasterBill;
				return airCargo == null ? ZString.Empty : airCargo.UnderbondHumanReadableName;
			}
		}

		public ZPropertyInfo LinkedMasterBillRefInfo
		{
			get { return GetZPropertyInfo(nameof(LinkedMasterBillRef)); }
		}

		public CusMAWB LinkedMasterBill
		{
			get { return (CusMAWB)Factory.Load(typeof(CusMAWB), LinkedMasterBillPK); }
		}

		#endregion

		#endregion

		#region MatchingAirCargos

		public ReadOnlyCusMAWBCollection MatchingAirCargos
		{
			get
			{
				if (fMatchingAirCargos == null)
				{
					fMatchingAirCargos = new ReadOnlyCusMAWBCollection(Factory);
				}

				return fMatchingAirCargos;
			}
		}

		ReadOnlyCusMAWBCollection fMatchingAirCargos;

		#endregion

		#region LoadSearchData

		public void LoadSearchData()
		{
			MatchingAirCargos.Load(SearchFilter);
		}

		#endregion

		public void AddConsignmentDetail(ConsignmentRecord consignmentRec)
		{
			ConsignmentDetails.Add(consignmentRec);
		}

		public void AddConsignmentNotes(ConsignmentNoteRecord consignmentNote)
		{
			ConsignmentNotes.Add(consignmentNote);
		}

		public bool HasSameFlightDetail(FlightRecord flightRec)
		{
			return FlightDetail.MasterBill.Trim() == flightRec.MasterBill.Trim() &&
				FlightDetail.FlightNumber.Trim() == flightRec.FlightNumber.Trim() &&
				FlightDetail.FlightDate == flightRec.FlightDate &&
				FlightDetail.PortOfLoading.Trim() == flightRec.PortOfLoading.Trim() &&
				FlightDetail.PortOfDischarge.Trim() == flightRec.PortOfDischarge.Trim();
		}

		public void CopySearchData()
		{
			MasterBill = SearchMasterBill;
			FlightNo = SearchFlightNo;
			ArrivalDate = SearchArrivalDate;
			PortOfLoading = SearchPortOfLoading;
			PortOfDischarge = SearchPortOfDischarge;
		}

		#region Update Methods

		protected override void PopulateDataCore(CusMAWB existingCusMawb)
		{
			CurrentLinkedRef = existingCusMawb.PK;
			int i = 0;
			foreach (ConsignmentRecord consignment in ConsignmentDetails)
			{
				ShowProgress(++i, ConsignmentDetails.Count, "Loading Housebill '" + consignment.HouseBill + "'");

				CusHAWB[] matchedCusHAWBs = (CusHAWB[])existingCusMawb.ChildBills.Find(new ZQuery(CusHAWBSchema.CS_HAWB, consignment.HouseBill));

				if (matchedCusHAWBs.Length > 1)
				{
					DialogResult answer = Globals.Message.Show(string.Format("{0} has duplicated Housebills.{1}They can not be updated. Skip this Housebill and import the rest.", existingCusMawb.UnderbondHumanReadableName, System.Environment.NewLine),
						string.Format("Duplicated Housebill '{0}'", consignment.HouseBill), MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
					if (answer == DialogResult.OK)
					{
						continue;
					}
					else
					{
						Buffer.Notify(new ErrorNotification(ErrorType.MoreThan1NKMatch, "Housebill: " + consignment.HouseBill));
						break;
					}
				}
				else
				{
					CusHAWB matchedCusHAWB = null;
					if (matchedCusHAWBs.Length == 1)
					{
						matchedCusHAWB = matchedCusHAWBs[0];
					}
					else
					{
						matchedCusHAWB = existingCusMawb.ChildBills.AddNew();
					}

					ConsignmentUpdator updator = new ConsignmentUpdator(consignment, ConsignmentNotes, Buffer);
					updator.Update(matchedCusHAWB);
					updator.UpdateQuantumOriginalValue(matchedCusHAWB, FlightDetail.RecordKey, BranchCode);
				}
			}
		}

		#endregion

		#region Implementation

		protected override void SaveCore()
		{
			LinkedMasterBillPK = CurrentLinkedRef;
		}

		#region ConsignmentDetails

		internal ConsignmentRecordCollection ConsignmentDetails
		{
			get
			{
				if (fConsignmentDetails == null)
				{
					fConsignmentDetails = new ConsignmentRecordCollection();
				}
				return fConsignmentDetails;
			}
		}

		ConsignmentRecordCollection fConsignmentDetails;

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

		ZGuid CurrentLinkedRef;
		ZGuid LinkedMasterBillPK;

		#endregion
	}
}
