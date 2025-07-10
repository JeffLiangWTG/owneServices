using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Integration.Customs.EU;

namespace Enterprise.Customs.ES.Business.CusTempStorage
{
	[DependentBusinessObject(typeof(CusTempStorageDec), "CusTempStorageLines")]
	public class CusTempStorageLine : EU.Business.CusTempStorage.CusTempStorageLine,
		Integration.Customs.ES.ICusTempStorageLine,
		IHugeSequenceNumberLine
	{
		public new class Schema : EU.Business.CusTempStorage.CusTempStorageLine.Schema
		{
			public const string STL_PackageMarks = "STL_PackageMarks";
			public const string STL_DescriptionOfGoods = "STL_DescriptionOfGoods";
		}

		public CusTempStorageLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Validation

		protected override EU.Business.CusTempStorage.CusTempStorageLineValidation GetNewValidation() => new CusTempStorageLineValidation(this);

		public new CusTempStorageLineValidation Validation => (CusTempStorageLineValidation)base.Validation;

		#endregion

		#region Lookups

		protected override EU.Business.CusTempStorage.CusTempStorageLineLookups GetNewLookups() => new CusTempStorageLineLookups(this);

		public new CusTempStorageLineLookups Lookups => (CusTempStorageLineLookups)base.Lookups;

		#endregion

		#region Dec
		public new CusTempStorageDec Dec => (CusTempStorageDec)base.Dec;

		#endregion

		#region Overrides

		public override ZGuid TSL_STH
		{
			get => base.TSL_STH;
			set
			{
				var oldvalue = base.TSL_STH;
				base.TSL_STH = value;
				if (!IsCopying && oldvalue != value)
				{
					SetLineNumOnSettingTSL_STH();
				}
			}
		}

		[ReadOnly(true)]
		public override ZInt TSL_LineNo { get => base.TSL_LineNo; set => base.TSL_LineNo = value; }

		[List(nameof(Lookups) + "." + nameof(CusTempStorageLineLookups.OwnerReferenceTypeList))]
		public override ZString TSL_OwnerReferenceType { get => base.TSL_OwnerReferenceType; set => base.TSL_OwnerReferenceType = value; }

		[List(nameof(Lookups) + "." + nameof(CusTempStorageLineLookups.UnionStatusList))]
		public override ZString TSL_UnionStatus { get => base.TSL_UnionStatus; set => base.TSL_UnionStatus = value; }

		[List(nameof(Lookups) + "." + nameof(CusTempStorageLineLookups.PackageTypeList))]
		public override ZString TSL_PackageType { get => base.TSL_PackageType; set => base.TSL_PackageType = value; }

		[List(nameof(Lookups) + "." + nameof(CusTempStorageLineLookups.GoodsTypeList))]
		public override ZString TSL_GoodsType { get => base.TSL_GoodsType; set => base.TSL_GoodsType = value; }

		[List(nameof(Lookups) + "." + nameof(CusTempStorageLineLookups.WeightUQList))]
		public override ZString TSL_GrossWeightUQ { get => base.TSL_GrossWeightUQ; set => base.TSL_GrossWeightUQ = value; }
		#endregion

		#region Implementation
		void SetLineNumOnSettingTSL_STH()
		{
			var lines = Dec?.CusTempStorageLines.Cast<CusTempStorageLine>() ?? Enumerable.Empty<IHugeSequenceNumberLine>();
			TSL_LineNo = (lines.Any() ? lines.Max(x => x.SequenceNumber) : ZInt.Zero) + 1;
		}
		#endregion

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var fNoteTypes = base.NoteTypesCore;
				fNoteTypes.Add(PredefinedNoteTypes.Instance.FRPackageMarks);
				fNoteTypes.Add(PredefinedNoteTypes.Instance.FRDescriptionOfGoods);
				return fNoteTypes;
			}
		}

		#region STL_PackageMarks

		public ZString STL_PackageMarks
		{
			get => Notes.GetNoteText(PredefinedNoteTypes.Instance.FRPackageMarks.Description);
			set
			{
				Notes.SetNoteText(this, STL_PackageMarksInfo, PredefinedNoteTypes.Instance.FRPackageMarks.Description, value);
			}
		}

		public ZPropertyInfo STL_PackageMarksInfo => GetZPropertyInfo(nameof(STL_PackageMarks));

		#endregion

		#region STL_DescriptionOfGoods

		public ZString STL_DescriptionOfGoods
		{
			get => Notes.GetNoteText(PredefinedNoteTypes.Instance.FRDescriptionOfGoods.Description);
			set
			{
				Notes.SetNoteText(this, STL_DescriptionOfGoodsInfo, PredefinedNoteTypes.Instance.FRDescriptionOfGoods.Description, value);
			}
		}

		public ZPropertyInfo STL_DescriptionOfGoodsInfo => GetZPropertyInfo(nameof(STL_DescriptionOfGoods));

		#endregion

		public CusTempStorageLineItem CusTempStorageLineItem => esCusTempStorageLineItem ??= CusTempStorageLineItems.FirstOrDefault() ?? CusTempStorageLineItems.AddNew();
		CusTempStorageLineItem esCusTempStorageLineItem;

		#region CusTempStorageLineItems

		protected override ICusTempStorageLineItemCollection<EU.Business.CusTempStorage.CusTempStorageLineItem> NewCusTempStorageLineItemCollection()
		{
			return new EU.Business.CusTempStorage.CusTempStorageLineItemCollection<CusTempStorageLineItem>(this);
		}

		public new ICusTempStorageLineItemCollection<CusTempStorageLineItem> CusTempStorageLineItems => (ICusTempStorageLineItemCollection<CusTempStorageLineItem>)base.CusTempStorageLineItems;

		#endregion
	}
}
