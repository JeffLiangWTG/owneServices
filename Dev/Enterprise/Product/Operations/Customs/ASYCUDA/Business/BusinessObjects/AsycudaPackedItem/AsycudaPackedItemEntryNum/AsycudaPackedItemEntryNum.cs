using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ASYCUDA.Business
{
	[DependentBusinessObject(typeof(AsycudaPackedItem), "CustomsEntryNumbers")]
	public class AsycudaPackedItemEntryNum : CusEntryNumber
	{
		public AsycudaPackedItemEntryNum(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaPackedItem Parent { get => (AsycudaPackedItem)base.Parent; set => base.Parent = value; }

		#region Override Properties

		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemEntryNumLookups.CustomsEntryNumberTypes))]
		[ResourceStringData("AsycudaPackedItemEntryNum.CE_EntryType", Caption = "Type", MediumCaption = "Type", ShortCaption = "Type", FullDescription = "Indicates the type of reference number.")]
		public override ZString CE_EntryType
		{
			get { return base.CE_EntryType; }
			set { base.CE_EntryType = value; }
		}

		[ResourceStringData("AsycudaPackedItemEntryNum.CE_EntryNum", Caption = "Reference Number", MediumCaption = "Reference No.", ShortCaption = "Ref. No.", FullDescription = "Reference number associated with the selected reference number type.")]
		public override ZString CE_EntryNum
		{
			get { return base.CE_EntryNum; }
			set { base.CE_EntryNum = value; }
		}

		public override bool ReadOnly => CE_EntryIsSystemGenerated;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CE_EntryIsSystemGenerated = false;
		}

		#region ICanDelete

		public override bool CanDelete => !CE_EntryIsSystemGenerated;

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("497894f9-fbfc-4c2f-9ff7-ae41eb58c70c", "Selected item is System Defined, and cannot be deleted by users."); }
		}

		#endregion

		#endregion

		#region Implementation

		protected override CusEntryNumLookups GetNewLookups()
		{
			return new AsycudaPackedItemEntryNumLookups(this);
		}

		public new AsycudaPackedItemEntryNumLookups Lookups => (AsycudaPackedItemEntryNumLookups)base.Lookups;

		protected override CusEntryNumValidation GetNewValidation()
		{
			return new AsycudaPackedItemEntryNumValidation(this);
		}

		public new AsycudaPackedItemEntryNumValidation Validation => (AsycudaPackedItemEntryNumValidation)base.Validation;

		#endregion

		#region Business Object Overrides

		protected override ZString HumanReadableNameCore => Res.GetString("F51C319F-0732-4524-A85A-2DD61459F26F", "{0} ({1})", CE_EntryNum, CE_EntryType);

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new AsycudaPackedItemEntryNumFetchStrategy(this);
	}
}
