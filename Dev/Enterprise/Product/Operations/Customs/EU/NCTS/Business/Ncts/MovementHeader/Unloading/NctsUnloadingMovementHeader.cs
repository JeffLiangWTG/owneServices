using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	[DependentBusinessObject(typeof(NctsHeader), nameof(NctsHeader.UnloadingMovementHeader))]
	public class NctsUnloadingMovementHeader : NctsCommonMovementHeader
		, Integration.Customs.EU.NCTS.IUnloadingMovementHeader
	{
		public NctsUnloadingMovementHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZInt TotalNumberOfItems => GoodsItems.Cast<NctsArrivalAndUnloadingCargoDesc>().Count(gi => !gi.IsMissing);

		public override ZLong TotalNumberOfPackages => GoodsItems.Cast<NctsArrivalAndUnloadingCargoDesc>().Where(gi => !gi.IsMissing).SelectMany(gi => gi.Packages.OfType<NctsPackage>()).Sum(p => p.B5_UnitCount);

		protected bool UnloadingTotalGrossMassInKilograms_ReadOnly => Header.UnloadingRemark.G9_Conform == YesNoList.Codes.Yes;

		[ReadOnlyMember(nameof(UnloadingTotalGrossMassInKilograms_ReadOnly))]
		public new ZDecimal TotalGrossMassInKilograms
		{
			get => BM_GrossWeight;
			set
			{
				BM_GrossWeight = value;
				BM_GrossWeightUQ = Core.Constants.Weight.Kilograms;
				TotalGrossMassInKilogramsInfo.RefreshBinding();
			}
		}

		public void SumAndStoreLinesGrossMassIfNotConforming()
		{
			if (Header.UnloadingRemark.G9_Conform == YesNoList.Codes.No)
			{
				base.BM_GrossWeight = GoodsItems.Cast<NctsArrivalAndUnloadingCargoDesc>().Where(gi => !gi.IsMissing).Sum(gi => gi.GrossMassInKilograms);
			}
		}

		public void ResetUnloadedTotalGrossMassInKilograms(ZDecimal grossMassInKilograms)
		{
			TotalGrossMassInKilograms = grossMassInKilograms;
		}

		public void ResetUnloadedGoodsItem(INctsArrivalAndUnloadingCargoDescCollection<NctsArrivalAndUnloadingCargoDesc> newGoodsItems)
		{
			foreach (NctsArrivalAndUnloadingCargoDesc item in GoodsItems)
			{
				item.ResultsOfControlCollection.DeleteAll();
			}
			GoodsItems.DeleteAll();

			foreach (var gi in newGoodsItems)
			{
				var unloadedGoodsItem = (NctsCommonCargoDesc)gi.TemplateCopy(PK);
				GoodsItems.Add(unloadedGoodsItem);
			}
		}

		public new static readonly NctsUnloadingMovementHeaderTypeDecider TypeDecider = new NctsUnloadingMovementHeaderTypeDecider();

		public new INctsArrivalAndUnloadingCargoDescCollection<NctsArrivalAndUnloadingCargoDesc> GoodsItems => (INctsArrivalAndUnloadingCargoDescCollection<NctsArrivalAndUnloadingCargoDesc>)base.GoodsItems;

		public new NctsUnloadingMovementHeaderValidation Validation => (NctsUnloadingMovementHeaderValidation)base.Validation;

		public new NctsUnloadingMovementHeaderLookups Lookups => (NctsUnloadingMovementHeaderLookups)base.Lookups;

		protected override INctsCommonCargoDescCollection<NctsCommonCargoDesc> CreateGoodsItems() => new NctsArrivalAndUnloadingCargoDescCollection<NctsArrivalAndUnloadingCargoDesc>(this);

		protected override CusInBondMoveHeaderValidation GetNewValidation() => new NctsUnloadingMovementHeaderValidation(this);

		protected override CusInBondMoveHeaderLookups GetNewLookups() => new NctsUnloadingMovementHeaderLookups(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BM_SubApplicationCode = Common.EU.NctsMoveHeaderType.Codes.Unloading;
		}

		protected override Type CusInBondCargoDescTypeCore => typeof(NctsArrivalAndUnloadingCargoDesc);

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			base.Delete();
		}
	}
}
