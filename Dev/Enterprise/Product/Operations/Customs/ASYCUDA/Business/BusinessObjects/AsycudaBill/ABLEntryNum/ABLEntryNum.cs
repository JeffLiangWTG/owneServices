using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ASYCUDA.Business
{
	[DependentBusinessObject(typeof(AsycudaBill), "CustomsEntryNumbers")]
	public class ABLEntryNum : CusEntryNumber
	{
		public ABLEntryNum(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[List(nameof(Lookups) + "." + nameof(ABLEntryNumLookups.CustomsEntryNumberTypes))]
		public override ZString CE_EntryType
		{
			get => base.CE_EntryType;
			set => base.CE_EntryType = value;
		}

		public override ZGuid CE_ParentID
		{
			get => base.CE_ParentID;
			set
			{
				var hasChanged = value != base.CE_ParentID;
				base.CE_ParentID = value;
				if (hasChanged && !IsCopying)
				{
					var packs = Bill?.Packs;
					if (packs?.Count == 1)
					{
						PackPivots.AddPivotFor(packs[0]);
					}
				}
			}
		}

		[ChildEditable]
		public ABLEntryNumRelatedPacksGenPivotCollection PackPivots
		{
			get
			{
				if (packPivots == null)
				{
					packPivots = new ABLEntryNumRelatedPacksGenPivotCollection(this);
					packPivots.Load();
					RegisterEditableChildObject(packPivots);
				}
				return packPivots;
			}
		}
		ABLEntryNumRelatedPacksGenPivotCollection packPivots;

		public ZDecimal WeightInKilos
			=> PackPivots.OfType<ABLEntryNumRelatedPacksGenPivot>().Sum(x => Core.Constants.Weight.Convert(x.Relation2Object?.APA_Weight ?? ZDecimal.Zero, x.Relation2Object?.APA_WeightUQ ?? ZString.Empty, Core.Constants.Weight.Kilograms));

		public ZDecimal NumberOfPackages => PackPivots.OfType<ABLEntryNumRelatedPacksGenPivot>().Sum(x => x.Relation2Object?.APA_PackQty ?? ZInt.Zero);

		public ZString FirstPackUQ => PackPivots.OfType<ABLEntryNumRelatedPacksGenPivot>().FirstOrDefault()?.Relation2Object?.APA_PackUQ ?? ZString.Empty;

		public AsycudaBill Bill => Factory.Load<AsycudaBill>(CE_ParentID);

		public ABLEntryNumProvider Provider
		{
			get
			{
				if (provider == null || provider.CountryCode != CE_RN_NKCountryCode)
				{
					provider = ABLEntryNumProvider.GetByCountryCode(CE_RN_NKCountryCode);
				}
				return provider;
			}
		}
		ABLEntryNumProvider provider;

		#endregion

		#region Implementation

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			var bill = Bill;
			var packs = bill?.Packs;
			if (packs != null)
			{
				if (bill?.CustomsEntryNumbers.Count == 1)
				{
					foreach (AsycudaPack pack in packs)
					{
						PackPivots.AddPivotFor(pack);
					}
				}
				else if (packs.Count == 1)
				{
					PackPivots.AddPivotFor(packs[0]);
				}
			}
		}

		public new ABLEntryNumLookups Lookups => (ABLEntryNumLookups)base.Lookups;
		protected override CusEntryNumLookups GetNewLookups() => new ABLEntryNumLookups(this);
		public new ABLEntryNumValidation Validation => (ABLEntryNumValidation)base.Validation;
		protected override CusEntryNumValidation GetNewValidation() => Provider.GetNewValidation(this);

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new ABLEntryNumFetchStrategy(this);

		#endregion

		#region Business Object Overrides

		protected override ZString HumanReadableNameCore => Res.GetString("47E718CA-ACB1-4281-BD69-F0F7A7855C9A", "{0} ({1})", CE_EntryNum, CE_EntryType);

		public override void Delete()
		{
			PackPivots.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion
	}
}
