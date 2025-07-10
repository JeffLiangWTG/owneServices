using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TNT.AirCargo
{
	public abstract class ImportAirCargo : NonPersistentBusinessObject, IObsoleteValidation, IAirCargo
	{
		public ImportAirCargo(BusinessObjectFactory factory, FlightRecord flightDetail, ZString branchCode)
			: base(factory)
		{
			if (flightDetail == null)
			{
				throw new ArgumentNullException(nameof(flightDetail));
			}
			this.FlightDetail = flightDetail;
			this.BranchCode = branchCode;
			InitialiseBoundProperties();
		}

		#region Branch

		public GlbBranch Branch
		{
			get { return fBranch; }
		}

		GlbBranch fBranch;

		#endregion

		#region Bound Properties

		#region MasterBill

		[MaxLength(CusMAWB.Schema.CM_MAWBMaxLength)]
		public ZString MasterBill
		{
			get { return fMasterBill; }
			set
			{
				ZString newValue = value.Replace(" ", "").Replace("-", "");
				if (fMasterBill != newValue)
				{
					SetNonPersistentPropertyValue(MasterBillInfo, ref fMasterBill, newValue);
					if (!IsValidationSuspended)
					{
						BillValidator.Validate(MasterBillInfo);
					}
				}
			}
		}

		ZString fMasterBill;

		public ZPropertyInfo MasterBillInfo
		{
			get { return GetZPropertyInfo(nameof(MasterBill)); }
		}

		#endregion

		#region FlightNo

		[MaxLength(CusMAWB.Schema.CM_FlightNoMaxLength)]
		public ZString FlightNo
		{
			get { return fFlightNo; }
			set { SetNonPersistentPropertyValue(FlightNoInfo, ref fFlightNo, value); }
		}

		ZString fFlightNo;

		public ZPropertyInfo FlightNoInfo
		{
			get { return GetZPropertyInfo(nameof(FlightNo)); }
		}

		#endregion

		#region ArrivalDate

		public ZDateTime ArrivalDate
		{
			get { return fArrivalDate; }
			set { SetNonPersistentPropertyValue(ArrivalDateInfo, ref fArrivalDate, value); }
		}

		ZDateTime fArrivalDate;

		public ZPropertyInfo ArrivalDateInfo
		{
			get { return GetZPropertyInfo(nameof(ArrivalDate)); }
		}

		#endregion

		#region DepartureDate

		public ZDateTime DepartureDate
		{
			get { return fDepartureDate; }
			set { SetNonPersistentPropertyValue(DepartureDateInfo, ref fDepartureDate, value); }
		}

		ZDateTime fDepartureDate;

		public ZPropertyInfo DepartureDateInfo
		{
			get { return GetZPropertyInfo(nameof(DepartureDate)); }
		}

		#endregion

		#region PortOfLoading

		[MaxLength(CusMAWB.Schema.CM_RL_NKLoadPortMaxLength)]
		public ZString PortOfLoading
		{
			get { return fPortOfLoading; }
			set { SetNonPersistentPropertyValue(PortOfLoadingInfo, ref fPortOfLoading, value); }
		}

		ZString fPortOfLoading;

		public ZPropertyInfo PortOfLoadingInfo
		{
			get { return GetZPropertyInfo(nameof(PortOfLoading)); }
		}

		#endregion

		#region PortOfDischarge

		[MaxLength(CusMAWB.Schema.CM_RL_NKLoadPortMaxLength)]
		public ZString PortOfDischarge
		{
			get { return fPortOfDischarge; }
			set { SetNonPersistentPropertyValue(PortOfDischargeInfo, ref fPortOfDischarge, value); }
		}

		ZString fPortOfDischarge;

		public ZPropertyInfo PortOfDischargeInfo
		{
			get { return GetZPropertyInfo(nameof(PortOfDischarge)); }
		}

		#endregion

		#endregion

		#region Search Properties

		#region SearchMasterBill

		[MaxLength(CusMAWB.Schema.CM_MAWBMaxLength)]
		public ZString SearchMasterBill
		{
			get { return fSearchMasterBill; }
			set
			{
				ZString newValue = value.Replace(" ", "").Replace("-", "");
				if (fSearchMasterBill != newValue)
				{
					SetNonPersistentPropertyValue(SearchMasterBillInfo, ref fSearchMasterBill, newValue);
					if (!IsValidationSuspended)
					{
						BillValidator.Validate(SearchMasterBillInfo);
					}
				}
			}
		}

		ZString fSearchMasterBill;

		public ZPropertyInfo SearchMasterBillInfo
		{
			get { return GetZPropertyInfo(nameof(SearchMasterBill)); }
		}

		#endregion

		#region SearchFlightNo

		[MaxLength(CusMAWB.Schema.CM_FlightNoMaxLength)]
		public ZString SearchFlightNo
		{
			get { return fSearchFlightNo; }
			set { SetNonPersistentPropertyValue(SearchFlightNoInfo, ref fSearchFlightNo, value); }
		}

		ZString fSearchFlightNo;

		public ZPropertyInfo SearchFlightNoInfo
		{
			get { return GetZPropertyInfo(nameof(SearchFlightNo)); }
		}

		#endregion

		#region SearchArrivalDate

		public ZDateTime SearchArrivalDate
		{
			get { return fSearchArrivalDate; }
			set { SetNonPersistentPropertyValue(SearchArrivalDateInfo, ref fSearchArrivalDate, value); }
		}

		ZDateTime fSearchArrivalDate;

		public ZPropertyInfo SearchArrivalDateInfo
		{
			get { return GetZPropertyInfo(nameof(SearchArrivalDate)); }
		}

		#endregion

		#region SearchPortOfLoading

		[MaxLength(CusMAWB.Schema.CM_RL_NKLoadPortMaxLength)]
		public ZString SearchPortOfLoading
		{
			get { return fSearchPortOfLoading; }
			set { SetNonPersistentPropertyValue(SearchPortOfLoadingInfo, ref fSearchPortOfLoading, value); }
		}

		ZString fSearchPortOfLoading;

		public ZPropertyInfo SearchPortOfLoadingInfo
		{
			get { return GetZPropertyInfo(nameof(SearchPortOfLoading)); }
		}

		#endregion

		#region SearchPortOfDischarge

		[MaxLength(CusMAWB.Schema.CM_RL_NKDischargePortMaxLength)]
		public ZString SearchPortOfDischarge
		{
			get { return fSearchPortOfDischarge; }
			set { SetNonPersistentPropertyValue(SearchPortOfDischargeInfo, ref fSearchPortOfDischarge, value); }
		}

		ZString fSearchPortOfDischarge;

		public ZPropertyInfo SearchPortOfDischargeInfo
		{
			get { return GetZPropertyInfo(nameof(SearchPortOfDischarge)); }
		}

		#endregion

		#endregion

		#region SearchFilter

		public ZQuery SearchFilter
		{
			get
			{
				ZQuery result = new ZQuery();

				if (!SearchMasterBill.IsEmpty)
				{
					result.AddToFilter(CusMAWBSchema.CM_MAWB, SearchMasterBill);
				}

				if (SearchArrivalDate.IsValidSqlDateTime)
				{
					result.AddToFilter(CusMAWBSchema.CM_ArrivalDate, SearchArrivalDate);
				}

				if (!SearchFlightNo.IsEmpty)
				{
					result.AddToFilter(CusMAWBSchema.CM_FlightNo, SearchFlightNo);
				}

				if (!SearchPortOfLoading.IsEmpty)
				{
					result.AddToFilter(CusMAWBSchema.CM_RL_NKLoadPort, SearchPortOfLoading);
				}

				if (!SearchPortOfDischarge.IsEmpty)
				{
					result.AddToFilter(CusMAWBSchema.CM_RL_NKDischargePort, SearchPortOfDischarge);
				}

				return result;
			}
		}

		#endregion

		#region IAirCargo Members

		public void PopulateNewData(CusMAWB masterBill)
		{
			Buffer.Clear();
			PopulateCusMAWB(masterBill);
			PopulateDataCore(masterBill);
		}

		public void PopulateData(CusMAWB masterBill)
		{
			Buffer.Clear();
			PopulateDataCore(masterBill);
		}

		public event TNTProgressEventHandler Progress;

		public bool IsValid
		{
			get
			{
				bool result = true;
				if (Buffer.HasErrors)
				{
					DialogResult answer = Globals.Message.Show("The following errors were encoutered" + System.Environment.NewLine + Buffer.AsString, "Error Encoutered", MessageBoxButtons.YesNo, MessageBoxIcon.Error, DialogResult.No);
					result = (answer == DialogResult.Yes);
				}

				return result;
			}
		}

		public void Save()
		{
			SaveCore();
		}

		#endregion

		#region Implementation

		void InitialiseBoundProperties()
		{
			SetPropertyInfoValue(MasterBillInfo, FlightDetail.MasterBill, ForeignKeyType.None);
			SetPropertyInfoValue(FlightNoInfo, FlightDetail.FlightNumber, ForeignKeyType.None);
			ArrivalDate = FlightDetail.FlightDate;
			DepartureDate = FlightDetail.FlightDate;
			SetPropertyInfoValue(PortOfLoadingInfo, FlightDetail.PortOfLoading, ForeignKeyType.PortNK);
			SetPropertyInfoValue(PortOfDischargeInfo, FlightDetail.PortOfDischarge, ForeignKeyType.PortNK);

			SearchMasterBill = MasterBill;
			SearchFlightNo = FlightNo;
			SearchArrivalDate = ArrivalDate;
			SearchPortOfLoading = PortOfLoading;
			SearchPortOfDischarge = PortOfDischarge;

			fBranch = (GlbBranch)Factory.LoadFromNaturalKey(typeof(GlbBranch), GlbBranchSchema.GB_Code, BranchCode);
		}

		protected virtual void SaveCore()
		{
		}

		protected virtual void PopulateCusMAWB(CusMAWB newCusMawb)
		{
			SetPropertyInfoValue(newCusMawb.CM_MAWBInfo, MasterBill, ForeignKeyType.None);
			SetPropertyInfoValue(newCusMawb.CM_FlightNoInfo, FlightNo, ForeignKeyType.None);
			newCusMawb.CM_ArrivalDate = ArrivalDate;
			newCusMawb.CM_DepartureDate = DepartureDate;
			SetPropertyInfoValue(newCusMawb.CM_RL_NKLoadPortInfo, PortOfLoading, ForeignKeyType.None);
			SetPropertyInfoValue(newCusMawb.CM_RL_NKDischargePortInfo, PortOfDischarge, ForeignKeyType.None);
			newCusMawb.CM_GB = (Branch == null) ? ZGuid.Empty : Branch.PK;
		}

		protected abstract void PopulateDataCore(CusMAWB existingCusMawb);

		protected void ShowProgress(int current, int total, string message)
		{
			if (Progress != null)
			{
				Progress(this, new TNTProgressEventArgs(current, total, message));
			}
		}

		#region SetPropertyInfoValue

		protected void SetPropertyInfoValue(ZPropertyInfo property, ZString valueAsString, ForeignKeyType fKType)
		{
			Mapper.SetPropertyInfoValue(property, GetValidString(valueAsString).Trim().Left(property.MaxLength), fKType, Buffer);
		}

		ZString GetValidString(ZString value)
		{
			ZString result = ZString.Empty;

			if (!value.IsEmpty)
			{
				result = value.KeepChars(ValidCharacters, " ");
			}

			return result;
		}

		public const string ValidCharacters = @"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 .,-()/=!""%*;<>&";

		#endregion

		#region BillValidator

		protected AirWayBillValidator BillValidator
		{
			get
			{
				if (fBillValidator == null)
				{
					fBillValidator = new AirWayBillValidator();
				}

				return fBillValidator;
			}
		}

		AirWayBillValidator fBillValidator;

		#endregion

		#region Mapper

		protected TNTStringToBusinessObjectFieldConverter Mapper
		{
			get
			{
				if (fMapper == null)
				{
					fMapper = TNTStringToBusinessObjectFieldConverter.Instance;
				}
				return fMapper;
			}
		}

		TNTStringToBusinessObjectFieldConverter fMapper;

		#endregion

		#region Buffer

		protected internal NotificationBuffer Buffer
		{
			get
			{
				if (fBuffer == null)
				{
					fBuffer = new NotificationBuffer();
				}
				return fBuffer;
			}
		}

		NotificationBuffer fBuffer;

		#endregion

		protected readonly ZString BranchCode;
		protected internal readonly FlightRecord FlightDetail;

		#endregion
	}
}
