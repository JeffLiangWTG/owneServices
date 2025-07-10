using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Enterprise.UniversalDataBuss.Integration;
using static System.FormattableString;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[UniversalDataContext(DataContextType.Outturn)]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class CusOutturn : Customs.Business.CusOutturn
		, Customs.Business.IStatusNeedsRecalculationProvider
		, ICMRMessageRespondee
		, Integration.Customs.AU.ICusOutturn
		, Customs.Business.ISynchroniserReadOnlyMembersProvider
		, ITransitWarehouseSyncEventParent
	{
		public new abstract class Schema : Customs.Business.CusOutturn.Schema
		{
		}

		public CusOutturn(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			StatusCalculator = new CusOutturnCustomsStatusCalculator(this);
		}

		protected internal TypeLoaderCollection ParentLoadersInternal => base.ParentLoaders;

		protected override TypeLoaderCollection GetParentLoaders()
		{
			TypeLoaderCollection result = base.GetParentLoaders();
			result.Add(new TypeLoader(typeof(CusHAWBBase)));
			result.Add(new TypeLoader(typeof(CusMAWBBase)));
			result.Add(new TypeLoader(typeof(CusPartShip)));
			result.Add(new TypeLoader(typeof(CusSCAContainer)));
			result.Add(new TypeLoader(typeof(CusSCAPivot)));
			result.Add(new TypeLoader(typeof(CusSCADepotContainer)));
			result.Add(new TypeLoader(typeof(CusSCADepotHouse)));
			result.Add(new WrapperTypeLoader<CFSLoadListConsol, CFSLoadListConsolWrapper>());
			result.Add(new WrapperTypeLoader<CFSContainer, CFSContainerWrapper>());
			//Result.Add(new WrapperTypeLoader(typeof(TallyContainer), typeof(CFSContainerWrapper)));
			result.Add(new WrapperTypeLoader<CFSShipment, CFSShipmentWrapper>());
			return result;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;
			C5_ApplicationCode = Customs.Business.CusOutturnApplicationCodeList.Codes.CMR;
		}

		#region Business Object Overrides

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return C5_HouseBill.IsEmpty ? Res.GetString("86810006-AB3C-4F0E-9D4E-F3A211F1D70B", "Outturn Bill") : Res.GetString("D52757B6-5492-4182-9CC2-9EE6CA037C91", "Outturn Bill {0}", new ZString(Invariant($"{C5_HouseBill}")));
			}
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#region Lookups

		public new CusOutturnLookups Lookups
		{
			get { return (CusOutturnLookups)base.Lookups; }
		}

		protected override Customs.Business.CusOutturnLookups GetNewLookups()
		{
			return new CusOutturnLookups(this);
		}

		#endregion

		#region Validation

		protected override Customs.Business.CusOutturnValidation GetNewValidation()
		{
			return new CusOutturnValidation(this);
		}

		#endregion

		protected override Customs.Business.CusStatus CustomsStatusCore
		{
			get
			{
				if (fCMRCargoStatus == null)
				{
					fCMRCargoStatus = new CargoCusStatus(C5_CustomsStatusInfo, StatusCalculator);
				}
				return fCMRCargoStatus;
			}
		}
		CargoCusStatus fCMRCargoStatus;

		public readonly CusOutturnCustomsStatusCalculator StatusCalculator;

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			CusOutturn result = (CusOutturn)base.CloneInternal(args);
			result.C5_C4_Underbond = ZGuid.Empty;
			return result;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Properties

		#region CusOutturnKey
		public CUSCARLineKey CusOutturnKey
		{
			get
			{
				CUSCARLineKey result = new CUSCARLineKey();
				result.container = C5_ContainerNumber;
				result.masterBill = C5_MasterBill;
				result.houseBill = C5_HouseBill;
				return result;
			}
		}
		#endregion

		#region Customs Status
		[ReadOnly(true)]
		public override ZString C5_CustomsStatus
		{
			get { return base.C5_CustomsStatus; }
		}
		#endregion

		#region C5_PackagesOutturned

		public override ZInt C5_PackagesOutturned
		{
			get { return base.C5_PackagesOutturned; }
			set
			{
				base.C5_PackagesOutturned = value;
				UpdateOutturnResultType();
			}
		}

		public override ZInt C5_OuterPacks
		{
			get { return base.C5_OuterPacks; }
			set
			{
				base.C5_OuterPacks = value;
				UpdateOutturnResultType();
			}
		}

		void UpdateOutturnResultType()
		{
			if (C5_OuterPacks > 0)
			{
				if (C5_PackagesOutturned < C5_OuterPacks)
				{
					C5_OutturnResultType = CMROutturnResultType.Codes.ShortLanded;
				}
				else if (C5_PackagesOutturned > C5_OuterPacks)
				{
					C5_OutturnResultType = CMROutturnResultType.Codes.SurplusPackages;
				}
				else
				{
					C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;
				}
			}
		}

		#endregion

		#region C5_CargoType

		public override ZString C5_CargoType
		{
			get { return base.C5_CargoType; }
			set
			{
				if (value != base.C5_CargoType && (value == CMRImportCargoTypes.Codes.Bulk || base.C5_CargoType == CMRImportCargoTypes.Codes.Bulk))
				{
					Lookups.PurgePackageTypes();
				}

				base.C5_CargoType = value;
			}
		}

		#endregion

		#endregion

		#region ReadOnly
		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}
		#endregion

		#region IStatusNeedsRecalculationProvider Members

		bool Customs.Business.IStatusNeedsRecalculationProvider.StatusNeedsRecalculation
		{
			get { return !IsDeleted && MessagesHasChanges; }
		}

		#endregion

		#region ICMRMessageRespondee Members

		ZString ICMRMessageRespondee.Details
		{
			get { return ZString.Empty; }
		}

		ZString ICMRMessageRespondee.ShortDescription
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region ITransitWarehouseSyncEventParent

		ZString ITransitWarehouseSyncEventParent.CustomsStatus => C5_CustomsStatus;

		#endregion
	}
}
