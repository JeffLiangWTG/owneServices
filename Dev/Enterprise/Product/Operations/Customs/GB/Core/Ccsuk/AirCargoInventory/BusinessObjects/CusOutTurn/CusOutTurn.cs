using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class CusOutTurn : Customs.Business.CusOutturn, ICanDelete, Integration.Customs.GB.CCSUK.ICusOutturn
	{
		public CusOutTurn(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		AllCusUnderbondsCollection removalsList;
		public AllCusUnderbondsCollection RemovalsList
		{
			get
			{
				if (removalsList == null && Hawb != null)
				{
					removalsList = Hawb.AllCusUnderbonds;
					removalsList.Load();
				}
				return removalsList;
			}
		}

		[ReadOnlyMember(nameof(IsBeingReleasedNowOrIsReleasedAlreadyOrIsDelivered))]
		public override ZInt C5_PackagesOutturned
		{
			get { return base.C5_PackagesOutturned; }
			set { base.C5_PackagesOutturned = value; }
		}

		[List(nameof(RemovalsList))]
		[ReadOnlyMember(nameof(C5_C4_UnderbondReadOnly))]
		public override ZGuid C5_C4_Underbond
		{
			get { return base.C5_C4_Underbond; }
			set
			{
				base.C5_C4_Underbond = value;
				if (Underbond != null)
				{
					SplitReferenceToWhichThisPertains = Underbond.SplitReferenceToWhichThisRemovalPertains;
				}
			}
		}
		bool C5_C4_UnderbondReadOnly
		{
			get { return ParentDoesNotHaveUnderbonds || IsBeingReleasedNowOrIsReleasedAlreadyOrIsDelivered; }
		}

		CusUnderbond underbond;
		public new CusUnderbond Underbond
		{
			get { return underbond ?? (underbond = Factory.Load<CusUnderbond>(C5_C4_Underbond)); }
		}

		[ReadOnlyMember(nameof(SplitReferenceToWhichThisPertains_ReadOnly))]
		[List(nameof(Awb) + "." + nameof(ICcsukCusAwb.Splits))]
		public ZString SplitReferenceToWhichThisPertains
		{
			get { return C5_MessageStatus; }
			set
			{
				C5_MessageStatus = value;
				if (!SplitReferenceToWhichThisPertains.IsEmpty)
				{
					var split = Awb.Splits[SplitReferenceToWhichThisPertains];
					if (split != null && C5_MarksAndNumbers.IsEmpty)
					{
						C5_MarksAndNumbers = split.HandlingInformation;  // When adding a new receipt row from a previously-created split, bring in the marks supplied at time of creation
					}
				}
			}
		}

		public ZPropertyInfo SplitReferenceToWhichThisPertainsInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(SplitReferenceToWhichThisPertains), x => C5_MessageStatusInfo); }
		}

		public ZBool SplitReferenceToWhichThisPertains_ReadOnly
		{
			get { return ParentDoesNotHaveAnySplits || UnderbondRelatesToASingleSplit || IsBeingReleasedNowOrIsReleasedAlreadyOrIsDelivered; }
		}

		[List(nameof(PackageTypesList))]
		public override ZString C5_PackagesUnits
		{
			get { return base.C5_PackagesUnits; }
			set { base.C5_PackagesUnits = value; }
		}

		public CodeDescriptionPairList PackageTypesList
		{
			get
			{
				return Enterprise.Customs.Universal.RefCusCodeListTypes.GetCachedListValidBeforeDate(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
								Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, Enterprise.Customs.EU.Business.UniversalReferenceConstants.UNPackTypeStartDate);
			}
		}

		ZBool ParentDoesNotHaveAnySplits
		{
			get { return Awb != null && !Awb.HasSplits; }
		}

		public ZBool UnderbondRelatesToASingleSplit
		{
			get { return Underbond != null && !Underbond.SplitReferenceToWhichThisRemovalPertains.IsEmpty; }
		}

		public ZBool ParentDoesNotHaveUnderbonds
		{
			get { return Awb == null || (Awb.TSRs.Count + Awb.IARs.Count + Awb.ISRs.Count + Awb.FBKs.Count) == 0; }
		}

		CusHAWB hawb;
		CusHAWB Hawb
		{
			get { return hawb ?? (hawb = Factory.Load<CusHAWB>(C5_ParentID)); }
		}

		internal ICcsukCusAwb AwbOrSplit
		{
			get
			{
				ICcsukCusAwb result = Awb;
				if (Awb != null && !SplitReferenceToWhichThisPertains.IsEmpty)
				{
					var split = Awb.Splits[SplitReferenceToWhichThisPertains];
					if (split != null)
					{
						result = split;
					}
				}
				return result;
			}
		}

		public //for list binding, pfff
		ICcsukCusAwb Awb
		{
			get
			{
				ICcsukCusAwb result = null;
				if (Hawb != null)
				{
					if (Hawb.CS_IsMasterHouse)
					{
						result = Hawb.MAWB; // for basics
					}
					else
					{
						result = hawb;
					}
				}
				return result;
			}
		}

		public override bool ReadOnly
		{
			get { return base.ReadOnly || (IsDelivered && IsInDatabase); }
			set { base.ReadOnly = value; }
		}

		public ZBool IsDelivered
		{
			get { return C5_ReceiptOnlyIndicator; }
			set
			{
				var hasChanged = C5_ReceiptOnlyIndicator != value;
				C5_ReceiptOnlyIndicator = value;
				// TODO - if this field is not readonly and has no validation errors, add a log
				if (value && hasChanged && Hawb != null && IsReleasedAlready)
				{
					Validation.ValidateC5_ReceiptOnlyIndicator();
					if (!IsDeliveredInfo.Notifications.GetErrors().Any())
					{
						var bizOForLogging = Hawb.CS_IsMasterHouse ? Hawb.MAWB : (BusinessObject)Hawb;
						if (bizOForLogging != null)
						{
							var logReference = string.Format(CultureInfo.InvariantCulture, "{0}{1} delivered from [{2}], [{3}]", C5_PackagesOutturned, C5_PackagesUnits, ShedStorageLocationForRraDocument, C5_MarksAndNumbers);
							bizOForLogging.GetLogs().AddNew(Events.Delivered, logReference, ZDateTimeOffset.Now);
							this.GetLogs().AddNew(Events.Delivered, logReference, ZDateTimeOffset.Now);
						}
					}
				}
			}
		}
		public ZPropertyInfo IsDeliveredInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(IsDelivered), x => C5_ReceiptOnlyIndicatorInfo); }
		}

		/// <summary>
		/// Indicates that the row has already been included on an RRA document
		/// </summary>
		[ReadOnly(true)]
		public ZBool IsReleasedAlready
		{
			get { return C5_PillageIndicator; }
			set { C5_PillageIndicator = value; }
		}
		public ZPropertyInfo IsReleasedAlreadyInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(IsReleasedAlready), x => C5_PillageIndicatorInfo); }
		}

		/// <summary>
		/// User wants to relase these goods in the next RR print.  Is wiped when the RRA is rendered.
		/// </summary>
		[ReadOnlyMember(nameof(IsIsBeingReleasedNowReadOnly))]
		public ZBool IsBeingReleasedNow
		{
			get { return C5_SealIntactIndicator; }
			set { C5_SealIntactIndicator = value; }
		}
		public ZPropertyInfo IsBeingReleasedNowInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(IsBeingReleasedNow), x => C5_SealIntactIndicatorInfo); }
		}
		bool IsIsBeingReleasedNowReadOnly
		{
			get { return IsDelivered || IsReleasedAlready; }
		}

		protected override Customs.Business.CusOutturnValidation GetNewValidation()
		{
			return new CusOutTurnValidation(this);
		}

		public new CusOutTurnValidation Validation
		{
			get { return (CusOutTurnValidation)base.Validation; }
		}

		bool WarehouseLocationIDReadOnly
		{
			get { return IsBeingReleasedNowOrIsReleasedAlreadyOrIsDelivered; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI003")]
		[List(nameof(WarehouseLocations))]
		[RelatedBusinessObject("ShedStorageLocation")]
		[ReadOnlyMember(nameof(WarehouseLocationIDReadOnly))]
		public ZGuid WarehouseLocationID
		{
			get
			{
				if (pivotForWarehouse == null || pivotForWarehouse.IsDeleted)
				{
					FindOrMakeNewWarehousePivot();
				}
				return pivotForWarehouse.XX_Relation2ID;
			}
			set
			{
				if (value.IsEmpty && pivotForWarehouse != null)
				{
					pivotForWarehouse.Delete();
					pivotForWarehouse = null;
				}
				else
				{
					if (pivotForWarehouse == null || pivotForWarehouse.IsDeleted)
					{
						FindOrMakeNewWarehousePivot();
					}
					pivotForWarehouse.XX_Relation2ID = value;
					Validation.ValidateWarehouseLocationID();
				}
				WarehouseLocationIDInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo WarehouseLocationIDInfo
		{
			get { return GetZPropertyInfo(nameof(WarehouseLocationID)); }
		}

		GenPivot pivotForWarehouse;

		internal void FindOrMakeNewWarehousePivot()
		{
			var pivotQuery = new ZQuery(GenPivotSchema.XX_Relation1ID, PK);
			pivotForWarehouse = Factory.LoadTop1<GenPivot>(pivotQuery);
			if (pivotForWarehouse == null)
			{
				pivotForWarehouse = Factory.New<GenPivot>();
				pivotForWarehouse.XX_Relation1ID = PK;
			}
		}

		public WhsLocation ShedStorageLocation
		{
			get { return Factory.Load<WhsLocation>(WarehouseLocationID); }
		}

		public WhsLocationCollection WarehouseLocations
		{
			get { return GetWarehouseLocations(Awb); }
		}

		internal static WhsLocationCollection GetWarehouseLocations(ICcsukCusAwb awb)
		{
			var ccsukShedCode = awb.CargoTerminalOperator;
			return awb.Factory.GetCachedValue("GB.CCSUK.CusOutTurn.WarehouseLocations." + ccsukShedCode, delegate
				{
					var warehouse = awb.Factory.LoadTop1<WhsWarehouse>(new ZQuery(WhsWarehouseSchema.WW_WarehouseCode, ccsukShedCode));
					return warehouse != null ? new WhsLocationCollection(warehouse) : new WhsLocationCollection(awb.Factory);
				});
		}

		bool IsBeingReleasedNowOrIsReleasedAlreadyOrIsDelivered
		{
			get { return IsBeingReleasedNow || IsReleasedAlready || IsDelivered; }
		}

		public ZString ShedStorageLocationForRraDocument
		{
			get { return ShedStorageLocation != null ? ShedStorageLocation.WLV_LocationString : ZString.Empty; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			if (C5_CargoReceiptDate.IsEmpty)
			{
				C5_CargoReceiptDate = ZDateTime.Now;
			}
			if (C5_PackagesUnits.IsEmpty)
			{
				C5_PackagesUnits = "PK";
			}
		}

		#region ICanDelete
		public override bool CanDelete
		{
			get { return !IsBeingReleasedNow && !IsReleasedAlready && !IsDelivered; }
		}

		bool ICanDelete.CanDelete
		{
			get { return this.CanDelete; }
		}

		void ICanDelete.OnCannotDelete()
		{
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return CanDelete ? (NoResString)string.Empty : ResString.GetMultilingualString("27A9B865-C135-4005-90FD-E57523E81FC8", "This row is already (being) released."); }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return this.ReasonForNotAbleToDelete; }
		}
		#endregion

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var newRow = (CusOutTurn)base.CloneInternal(args);
			newRow.C5_PackagesOutturned = 0;
			newRow.IsDelivered = false;
			newRow.IsReleasedAlready = false;
			newRow.WarehouseLocationID = WarehouseLocationID;
			return newRow;
		}

		public ZString Divide(ZString newPiecesAsString)
		{
			var result = "";
			ZInt pieceCount = 0;
			ZInt.TryParse(newPiecesAsString, out pieceCount);
			if (ReadOnly || IsReleasedAlready)
			{
				result = "This row cannot be divided, it is released or delivered";
			}
			else if (pieceCount == 0)
			{
				result = "Select a value greater than zero";
			}
			else if (pieceCount >= C5_PackagesOutturned)
			{
				result = "Select a value less than the current pieces count (" + C5_PackagesOutturned + ")";
			}
			else
			{
				var newOutturn = (CusOutTurn)Clone();
				newOutturn.C5_PackagesOutturned = pieceCount;
				C5_PackagesOutturned -= pieceCount;
				if (Hawb.CS_IsMasterHouse)
				{
					Hawb.MAWB.OutTurns.Add(newOutturn);
					Hawb.MAWB.OutTurns.RefreshBindingIncludingChildren();
				}
				Hawb.OutTurns.Add(newOutturn);
				Hawb.OutTurns.RefreshBindingIncludingChildren();
			}
			return result;
		}
	}
}
