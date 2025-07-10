using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class BlanketVesselChange : AutoBlanketVesselChange
	{
		public BlanketVesselChange(JPAFRHeader header) : base(new BusinessObjectFactory())
		{
			this.header = Argument.NotNull(header, nameof(header));
			newVesselVoyage = GetNewVesselVoyage(this.header);
			CopyValuesFromNewVesselVoyage(newVesselVoyage);
		}

		VesselVoyage GetNewVesselVoyage(JPAFRHeader header)
		{
			var newVesselVoyage = header.NewVesselVoyage;
			if (newVesselVoyage == null)
			{
				newVesselVoyage = Factory.New<VesselVoyage>();
				newVesselVoyage.B7_ParentID = header.PK;
				newVesselVoyage.B7_ParentTableCode = header.TablePrefix;
				newVesselVoyage.B7_Type = CusAddInfoTypeAttribute.Codes.JPAFRNewVesselVoyage;
				newVesselVoyage.JP_CarrierCode = header.JPH_CarrierCode;
				newVesselVoyage.JP_VesselName = header.JPH_VesselName;
				newVesselVoyage.JP_VesselCallSign = header.JPH_RadioCallSign;
				newVesselVoyage.JP_VesselCountry = header.JPH_RN_NKCountryOfReg;
				newVesselVoyage.JP_VoyageNumber = header.JPH_Voyage;
				newVesselVoyage.JP_OperationalCarrierVoyageNo = header.JPH_OperationalCarrierVoyageNo;
				newVesselVoyage.JP_PortOfLoadingCode = header.JPH_RL_NKLoading;
				newVesselVoyage.JP_PortOfLoadingSuffix = header.JPH_LoadingPortSuffix;
				newVesselVoyage.JP_IsDepartureFromRelaxedArea = header.JPH_RelaxedAppId;
				newVesselVoyage.JP_EstimatedDateTimeOfDeparture = header.JPH_ETD;
				newVesselVoyage.JP_BlanketChange = ZBool.True;
			}
			return newVesselVoyage;
		}

		void CopyValuesFromNewVesselVoyage(VesselVoyage newVesselVoyage)
		{
			using (GetValidationSuspender())
			{
				base.JPM_CarrierCodeNew = newVesselVoyage.JP_CarrierCode;
				base.JPM_VesselNameNew = newVesselVoyage.JP_VesselName;
				base.JPM_RadioCallSignNew = newVesselVoyage.JP_VesselCallSign;
				base.JPM_RN_NKCountryOfRegNew = newVesselVoyage.JP_VesselCountry;
				base.JPM_VoyageNumberNew = newVesselVoyage.JP_VoyageNumber;
				base.JPM_OperatorVoyageNew = newVesselVoyage.JP_OperationalCarrierVoyageNo;
				base.JPM_PortOfLoadingCodeNew = newVesselVoyage.JP_PortOfLoadingCode;
				base.JPM_PortOfLoadingSuffixNew = newVesselVoyage.JP_PortOfLoadingSuffix;
				base.JPM_IsDepartureFromRelaxedAreaNew = newVesselVoyage.JP_IsDepartureFromRelaxedArea;
				base.JPM_ETDNew = newVesselVoyage.JP_EstimatedDateTimeOfDeparture;
				SetJPM_AreAllBillsCheckedInternally(ZBool.True);
				SetJPM_BlanketChangeInternally(newVesselVoyage.JP_BlanketChange);
			}
		}

		public override ZString JPM_CarrierCodeNew
		{
			get { return base.JPM_CarrierCodeNew; }
			set
			{
				base.JPM_CarrierCodeNew = value;
				newVesselVoyage.JP_CarrierCode = value;
			}
		}

		[List(nameof(Vessels))]
		public override ZString JPM_VesselNameNew
		{
			get { return base.JPM_VesselNameNew; }
			set
			{
				if (base.JPM_VesselNameNew != value)
				{
					if (!IsCopying)
					{
						VesselCombination.DefaultCallSignAndNationalityIfNeeded(value);
					}
					base.JPM_VesselNameNew = value;
					newVesselVoyage.JP_VesselName = value;
				}
			}
		}

		public RefVessel Vessel => (RefVessel)Factory.LoadFromNaturalKey(typeof(RefVessel), RefVesselSchema.RV_Code, JPM_VesselNameNew);

		public RefVesselCollection Vessels => VesselCombination.Vessels;

		public override ZString JPM_RadioCallSignNew
		{
			get => base.JPM_RadioCallSignNew;
			set
			{
				base.JPM_RadioCallSignNew = value;
				newVesselVoyage.JP_VesselCallSign = value;
			}
		}

		[List(nameof(CountryOfRegs))]
		public override ZString JPM_RN_NKCountryOfRegNew
		{
			get => base.JPM_RN_NKCountryOfRegNew;
			set
			{
				base.JPM_RN_NKCountryOfRegNew = value;
				newVesselVoyage.JP_VesselCountry = value;
			}
		}

		public RefCountryCollection CountryOfRegs => new RefCountryCollection(Factory);

		public JPAFRVesselCombination VesselCombination => vesselCombination ?? (vesselCombination = new JPAFRVesselCombination(Factory, JPM_VesselNameNewInfo, JPM_RadioCallSignNewInfo, JPM_RN_NKCountryOfRegNewInfo));
		JPAFRVesselCombination vesselCombination;

		public override ZString JPM_VoyageNumberNew
		{
			get { return base.JPM_VoyageNumberNew; }
			set
			{
				base.JPM_VoyageNumberNew = value;
				newVesselVoyage.JP_VoyageNumber = value;
			}
		}

		public override ZString JPM_OperatorVoyageNew
		{
			get { return base.JPM_OperatorVoyageNew; }
			set
			{
				base.JPM_OperatorVoyageNew = value;
				newVesselVoyage.JP_OperationalCarrierVoyageNo = value;
			}
		}

		[RelatedBusinessObject("Loading")]
		[List(nameof(Loadings))]
		public override ZString JPM_PortOfLoadingCodeNew
		{
			get { return base.JPM_PortOfLoadingCodeNew; }
			set
			{
				base.JPM_PortOfLoadingCodeNew = value;
				newVesselVoyage.JP_PortOfLoadingCode = value;
			}
		}

		public RefUNLOCO Loading => (RefUNLOCO)Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, JPM_PortOfLoadingCodeNew);

		public RefUNLOCOCollection Loadings => new RefUNLOCOCollection(Factory);
		public override ZString JPM_PortOfLoadingSuffixNew
		{
			get { return base.JPM_PortOfLoadingSuffixNew; }
			set
			{
				base.JPM_PortOfLoadingSuffixNew = value;
				newVesselVoyage.JP_PortOfLoadingSuffix = value;
			}
		}

		public override ZBool JPM_IsDepartureFromRelaxedAreaNew
		{
			get { return base.JPM_IsDepartureFromRelaxedAreaNew; }
			set
			{
				base.JPM_IsDepartureFromRelaxedAreaNew = value;
				newVesselVoyage.JP_IsDepartureFromRelaxedArea = value;
			}
		}

		public override ZDateTime JPM_ETDNew
		{
			get { return base.JPM_ETDNew; }
			set
			{
				base.JPM_ETDNew = value;
				newVesselVoyage.JP_EstimatedDateTimeOfDeparture = value;
			}
		}

		public override ZBool JPM_AreAllBillsChecked
		{
			get
			{
				return base.JPM_AreAllBillsChecked;
			}
			set
			{
				SetJPM_AreAllBillsCheckedInternally(value);
				BlanketVesselChangeBills.Cast<BlanketVesselChangeBill>().Where(x => x.JPM_Send != value).ForEach(x => x.JPM_Send = value);
			}
		}

		internal void SetJPM_AreAllBillsCheckedInternally(ZBool areAllBillsChecked)
		{
			base.JPM_AreAllBillsChecked = areAllBillsChecked;
		}

		public override ZBool JPM_BlanketChange
		{
			get { return base.JPM_BlanketChange; }
			set
			{
				SetJPM_BlanketChangeInternally(value);
				newVesselVoyage.JP_BlanketChange = value;
			}
		}

		void SetJPM_BlanketChangeInternally(ZBool blanketChange)
		{
			base.JPM_BlanketChange = blanketChange;
			if (blanketChange)
			{
				JPM_AreAllBillsChecked = false;
			}
		}

		public bool AreVesselDetailsDifferentFromHeader => header.JPH_CarrierCode != JPM_CarrierCodeNew
								|| header.JPH_VesselName != JPM_VesselNameNew
								|| header.JPH_RadioCallSign != JPM_RadioCallSignNew
								|| header.JPH_RN_NKCountryOfReg != JPM_RN_NKCountryOfRegNew
								|| header.JPH_Voyage != JPM_VoyageNumberNew
								|| header.JPH_OperationalCarrierVoyageNo != JPM_OperatorVoyageNew
								|| header.JPH_RL_NKLoading != JPM_PortOfLoadingCodeNew
								|| header.JPH_LoadingPortSuffix != JPM_PortOfLoadingSuffixNew
								|| header.JPH_RelaxedAppId != JPM_IsDepartureFromRelaxedAreaNew
								|| header.JPH_ETD != JPM_ETDNew;

		public BlanketVesselChangeBillCollection BlanketVesselChangeBills
		{
			get
			{
				if (blanketVesselChangeBills == null)
				{
					blanketVesselChangeBills = new BlanketVesselChangeBillCollection(Factory);
					foreach (var bill in header.Bills)
					{
						var blanketVesselChangeBill = Factory.Load<BlanketVesselChangeBill>(bill.PK) ?? new BlanketVesselChangeBill(bill, this);
						blanketVesselChangeBills.Add(blanketVesselChangeBill);
					}
					RegisterEditableChildObject(blanketVesselChangeBills);
				}
				return blanketVesselChangeBills;
			}
		}
		BlanketVesselChangeBillCollection blanketVesselChangeBills;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public BlanketVesselChangeBill[] BillsToSend => BlanketVesselChangeBills.Cast<BlanketVesselChangeBill>().Where(x => x.JPM_Send).ToArray();

		public ZBool IsShippingLineEntry => header.JPH_IsShippingLineEntry;

		readonly JPAFRHeader header;
		readonly VesselVoyage newVesselVoyage;
	}
}
