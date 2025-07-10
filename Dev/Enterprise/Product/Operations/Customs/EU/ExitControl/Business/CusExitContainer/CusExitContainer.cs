using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.ExitControlBase.Business;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	[CodeProperty(ExitControlBase.Business.CusExitContainer.Schema.CXN_ContainerNumber), DescriptionProperty(ExitControlBase.Business.CusExitContainer.Schema.CXN_ContainerNumber)]
	public class CusExitContainer : ExitControlBase.Business.CusExitContainer
		, EUExitControl.ICusExitContainer
		, ICusSealTypeSupporter
		, IUcc6ValueProvider
	{
		public CusExitContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			if (IsUCC6)
			{
				SetReadOnlyIfStatusIsMissing(StatusIsMissing);
			}
		}

		public new class Schema : AutoCusExitContainer.Schema
		{
			public new const int CXN_ContainerNumberMaxLength = 17;
		}

		public static readonly CusExitContainerTypeDecider TypeDecider = new CusExitContainerTypeDecider();

		public new CusExitHeader Header => Factory.Load<CusExitHeader>(CXN_CXH_Header);

		[ResourceStringData("{5A052372-9646-4A1D-A7D4-904E25E29010}", Caption = "Is Equipment")]
		public override ZBool CXN_IsEquipment { get => base.CXN_IsEquipment; set => base.CXN_IsEquipment = value; }

		[ResourceStringData("{CE1648E0-45AC-4EB7-9B18-35DC5409B63B}", Caption = "Sequence Number", MediumCaption = "Seq Number", ShortCaption = "Seq Num.")]
		public override ZShort CXN_Sequence { get => base.CXN_Sequence; set => base.CXN_Sequence = value; }

		[ResourceStringData("EA097F63-36D4-4284-8745-75593003BD3A", Caption = "Number")]
		[MaxLength(Schema.CXN_ContainerNumberMaxLength)]
		public override ZString CXN_ContainerNumber
		{
			get => base.CXN_ContainerNumber;
			set => base.CXN_ContainerNumber = value;
		}

		[ResourceStringData("9BBAF818-5833-44C7-AD2B-1E6236397E71", Caption = "Status", MediumCaption = "Status", ShortCaption = "Status", FullDescription = "Container/Equipment Status")]
		[List(nameof(Lookups) + "." + nameof(CusExitContainerLookups.StatusList))]
		public override ZString CXN_Status
		{
			get => base.CXN_Status; set
			{
				var oldValue = CXN_Status;
				base.CXN_Status = value;
				if (!IsCopying && oldValue != CXN_Status && IsUCC6)
				{
					ClearStatusAndSetSealsReadOnlyIfStatusIsMissing();
				}
			}
		}

		[ResourceStringData("476777BD-A078-4AD7-AC37-976260EF7CA3", Caption = "Seals Quantity", ShortCaption = "Seals Qty", MediumCaption = "Seals Quantity", FullDescription = "Container/Equipment Seals Quantity")]
		public override ZShort CXN_SealCount
		{
			get => base.CXN_SealCount;
			set => base.CXN_SealCount = value;
		}

		[ChildEditable(true)]
		public CusExitSealCollection AllSealNumbers
		{
			get
			{
				if (allSealNumbers == null)
				{
					allSealNumbers = CreateNewCusExitSealCollection();
					allSealNumbers.Load();
					RegisterEditableChildObject(allSealNumbers);
				}

				return allSealNumbers;
			}
		}
		CusExitSealCollection allSealNumbers;

		protected virtual CusExitSealCollection CreateNewCusExitSealCollection() => new CusExitSealCollection(this);

		Type ICusSealTypeSupporter.CusSealType => CusSealTypeCore;

		protected virtual Type CusSealTypeCore => typeof(CusExitSeal);

		public IDictionary<ZShort, ZShort> AdditionalSealNumbersSequenceNumberDictionary => Factory.GetValue(ref additionalSealNumbersSequenceNumberDictionaryCached, () => AllSealNumbers
			.Select(x => x.BK_SequenceNumber).Where(x => x > ZShort.Zero)
			.GroupBy(x => x).ToDictionary(x => x.Key, y => (ZShort)y.Count()));
		CachedProperty<IDictionary<ZShort, ZShort>> additionalSealNumbersSequenceNumberDictionaryCached;

		public IDictionary<ZString, ZShort> AdditionalSealNumbersNumberDictionary => Factory.GetValue(ref additionalSealNumbersNumberDictionaryCached, () => AllSealNumbers
			.Select(x => x.BK_SealNumber).Where(x => !x.IsEmpty)
			.GroupBy(x => x).ToDictionary(x => x.Key, y => (ZShort)y.Count()));
		CachedProperty<IDictionary<ZString, ZShort>> additionalSealNumbersNumberDictionaryCached;

		public override void Delete()
		{
			if (!IsDeleted)
			{
				CusExitConsignmentPivots.Cast<CusExitConsignmentPivot>().ToArray().ForEach(x => x.CNP_CXN_Container = ZGuid.Empty);
			}
			base.Delete();
		}

		public bool IsUCC6 => IsUCC6Core;
		protected virtual bool IsUCC6Core => Header?.IsUCC6 ?? false;

		public new CusExitContainerLookups Lookups => (CusExitContainerLookups)base.Lookups;

		public new CusExitContainerValidation Validation => (CusExitContainerValidation)base.Validation;

		protected override ExitControlBase.Business.CusExitContainerValidation GetNewValidation() => new CusExitContainerValidation(this);

		public new ICusExitConsignmentPivotCollection<CusExitConsignmentPivot> CusExitConsignmentPivots => (ICusExitConsignmentPivotCollection<CusExitConsignmentPivot>)base.CusExitConsignmentPivots;

		protected override ICusExitConsignmentPivotCollection<ExitControlBase.Business.CusExitConsignmentPivot> CreateNewCusExitConsignmentPivotCollection() => new CusExitConsignmentPivotCollection<CusExitConsignmentPivot>(this);

		protected override ExitControlBase.Business.CusExitContainerLookups GetNewLookups() => IsUCC6
			? new CusExitContainerUcc6Lookups(this)
			: new CusExitContainerLookups(this);

		void ClearStatusAndSetSealsReadOnlyIfStatusIsMissing()
		{
			var readOnly = StatusIsMissing;
			if (readOnly)
			{
				AllSealNumbers.Cast<CusExitSeal>().ForEach(x => x.BK_UnloadingState = ZString.Empty);
			}
			SetReadOnlyIfStatusIsMissing(readOnly);
		}

		void SetReadOnlyIfStatusIsMissing(bool readOnly)
		{
			AllSealNumbers.SetReadOnlyIncludingChildren(readOnly);
		}

		bool StatusIsMissing => CXN_Status == DiscrepanciesStatusCodeList.Codes.Missing;

		public ICusExitContainerValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedValue<ICusExitContainerValidationDecider> validationDeciderCached;

		ICusExitContainerValidationDecider GetValidationDecider() => Header?.Configuration.CusExitContainerConfiguration.GetValidationDecider(this);
	}
}
