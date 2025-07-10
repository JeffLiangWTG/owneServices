using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsContainer : CusInBondContainer
		, IContainer
		, Integration.Customs.EU.NCTS.INctsContainer
		, ICusCodeDataTypeSupporter
		, ICusSealTypeSupporter
	{
		public new class Schema : AutoCusInBondContainer.Schema
		{
			public new const int BC_ContainerNumMaxLength = 17;
		}

		public NctsContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ChildEditable]
		public CusSealCollection Seals
		{
			get
			{
				if (seals == null)
				{
					seals = GetNewCusSealCollection();
					seals.Load();
					RegisterEditableChildObject(seals);
				}
				return seals;
			}
		}
		CusSealCollection seals;

		public IEnumerable<CusSeal> SealsForMessaging => Seals.Where(s => s.BK_UnloadingState != NctsUnloadedStateList.Codes.DAM);

		protected virtual CusSealCollection GetNewCusSealCollection() => new CusSealCollection(this);

		Type ICusSealTypeSupporter.CusSealType => CusSealTypeCore;

		protected virtual Type CusSealTypeCore => typeof(CusSeal);

		[ChildEditable]
		public NctsContainerItemCollection ItemNumbers
		{
			get
			{
				if (itemNumbers == null)
				{
					itemNumbers = GetNewNctsContainerItemCollection();
					itemNumbers.Load();
					RegisterEditableChildObject(itemNumbers);
				}
				return itemNumbers;
			}
		}
		NctsContainerItemCollection itemNumbers;

		protected virtual NctsContainerItemCollection GetNewNctsContainerItemCollection() => new NctsContainerItemCollection(this);

		[BusinessObjectTestExclude]
		public new ICusInBondCargoDescCollection<CusInBondCargoDesc> Commodities => base.Commodities;

		public new static readonly NctsContainerTypeDecider TypeDecider = new NctsContainerTypeDecider();

		[ResourceStringData("B298D1D6-72A8-4A4A-9894-EF0E948DA234", Caption = "Seal Quantity", MediumCaption = "Seal Qty.", ShortCaption = "Seal Qty.")]
		public ZInt TotalSealCount => Seals.Count + (BC_Seal1.IsEmpty ? 0 : 1) + (BC_Seal2.IsEmpty ? 0 : 1);

		public ZPropertyInfo TotalSealCountInfo => GetZPropertyInfo(nameof(TotalSealCount));

		#region IContainer Implementation

		[ResourceStringData("NctsHeaderContainer.ContainerNumber", Caption = "Container Number", ShortCaption = "Container")]
		[ReadOnlyMember(nameof(IsArrivalNotificationDisabled))]
		public virtual ZString ContainerNumber
		{
			get => base.BC_ContainerNum;
			set => base.BC_ContainerNum = value;
		}

		public ZPropertyInfo ContainerNumberInfo => GetWrappedZPropertyInfo(nameof(ContainerNumber), x => base.BC_ContainerNumInfo);

		ZBool IContainer.IsForInvoiceLine => true; // Show on T(S)AD doc

		[List(nameof(Lookups) + "." + nameof(NctsContainerLookups.TypeOfServiceList))]
		public override ZString BC_TypeOfService
		{
			get => base.BC_TypeOfService;
			set => base.BC_TypeOfService = value;
		}

		[ResourceStringData("5FE45DBD-D011-4B5A-BDDD-6B2F89140F58", Caption = "Container/Equipment Mode", MediumCaption = "Container Mode", ShortCaption = "Mode")]
		[List(nameof(Lookups) + "." + nameof(NctsContainerLookups.CargoIdTypeList))]
		[ReadOnlyMember(nameof(IsArrivalNotificationDisabled))]
		public override ZString BC_Mode
		{
			get => base.BC_Mode;
			set => base.BC_Mode = value;
		}

		[ResourceStringData("26BDB47E-873B-4AA9-AAC0-D148E1670620", Caption = "Container/Equipment Number", MediumCaption = "Container/Equipment No.", ShortCaption = "Container/Equipment")]
		[MaxLength(Schema.BC_ContainerNumMaxLength)]
		[ReadOnlyMember(nameof(IsArrivalNotificationDisabled))]
		public override ZString BC_ContainerNum
		{
			get => base.BC_ContainerNum;
			set => base.BC_ContainerNum = value;
		}

		[ResourceStringData("5485A964-D1D4-43BF-BC84-D8EFF06E0CB5", Caption = "Seal 1 Number", ShortCaption = "Seal 1")]
		[ReadOnlyMember(nameof(IsArrivalNotificationDisabled))]
		public override ZString BC_Seal1
		{
			get => base.BC_Seal1;
			set => base.BC_Seal1 = value;
		}

		[ResourceStringData("89D0B099-D4F2-464E-9340-4913F97514B7", Caption = "Seal 2 Number", ShortCaption = "Seal 2")]
		[ReadOnlyMember(nameof(IsArrivalNotificationDisabled))]
		public override ZString BC_Seal2
		{
			get => base.BC_Seal2;
			set => base.BC_Seal2 = value;
		}

		#region Base Implementation

		protected override ICusInBondCargoDescCollection<CusInBondCargoDesc> GetNewCommoditiesCollection() => new NctsCommonCargoDescCollection<NctsCommonCargoDesc>(this); // This is required to satisfy the base tests

		protected override Customs.Business.CusInBondMoveDetail MoveDetailCore => null;

		public BusinessObject Parent
		{
			get
			{
				BusinessObject result = null;
				if (BC_ParentTableCode == CusInBondCargoDescSchema.Constants.Prefix)
				{
					result = Factory.Load<NctsCommonCargoDesc>(BC_ParentID);
				}
				else if (BC_ParentTableCode == CusInBondEventSchema.Constants.Prefix)
				{
					result = Factory.Load<CusInBondEvent>(BC_ParentID);
				}
				return result;
			}
		}

		#endregion

		public new NctsHeader Header => (Parent as NctsCommonCargoDesc)?.Header ?? (Parent as EnRouteIncident)?.Header;

		protected override CusInBondContainerLookups GetNewLookups() => new NctsContainerLookups(this);

		public new NctsContainerLookups Lookups => (NctsContainerLookups)base.Lookups;

		#endregion

		protected override bool SupportsCloneCore() => true;

		#region ICusCodeDataTypeSupporter Members

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes() => GetCusCodeDataTypesCore();

		protected virtual IDictionary<ZString, Type> GetCusCodeDataTypesCore()
		{
			return new Dictionary<ZString, Type>
			{
				{ CusCodeDataTypeList.Codes.ItemNumber, typeof(NctsContainerItem) }
			};
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}

		#endregion

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			base.Delete();
		}

		protected sealed override CusInBondContainerValidation GetNewValidation() => IsPhase5
			? GetNewPhase5Validation()
			: GetNewPhase4Validation();

		protected virtual CusInBondContainerValidation GetNewPhase5Validation() => new NctsContainerPhase5Validation(this);

		protected virtual CusInBondContainerValidation GetNewPhase4Validation() => new NctsContainerPhase4Validation(this);

		public bool IsPhase5 => Header?.IsPhase5 ?? false;

		public new NctsContainerValidation Validation => (NctsContainerValidation)base.Validation;

		public bool IsArrivalNotificationDisabled => Parent is EnRouteIncident incident && incident.Header.IsArrivalNotificationDisabled;
	}
}
