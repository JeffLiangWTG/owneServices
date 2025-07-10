using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.EU.Manifest.Business
{
	public class AsycudaBill : ASYCUDA.Business.AsycudaBill, Integration.Customs.ASYCUDA.EUManifest.IAsycudaBill
	{
		public AsycudaBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : ASYCUDA.Business.AsycudaBill.Schema
		{
			public const string SpecialMentions = "SpecialMentions";
			public const int SpecialMentionsMaxLength = 5;
		}

		protected override ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill()
		{
			AsycudaBillValidationForRegularBill result = null;
			if (Header?.IsICSManifest ?? false)
			{
				result = new ICSAsycudaBillValidationForRegularBill(this);
			}
			else
			{
				result = new AsycudaBillValidationForRegularBill(this);
			}
			return result;
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;
		public new ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill> Packs => (ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>)base.Packs;
		protected override Type GetPackTypeCore() => typeof(AsycudaPack);
		protected override ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => new ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>(this);
		public new AsycudaBillLookups Lookups => (AsycudaBillLookups)base.Lookups;
		protected override ManifestBase.AsycudaBillLookups GetNewLookups() => new AsycudaBillLookups(this);

		#region SpecialMentions

		[MaxLength(Schema.SpecialMentionsMaxLength)]
		[ResourceStringData("AsycudaBill.SpecialMentions", Caption = "Special Mentions")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.SpecialMentionsList))]
		public ZString SpecialMentions
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.SpecialMentions);
			set
			{
				var oldValue = SpecialMentions;
				CheckMaximumLength(SpecialMentionsInfo, value);
				this.SetSystemDefinedValue(Schema.SpecialMentions, value);
				if (!IsValidationSuspended)
				{
					(Validation as ICSAsycudaBillValidationForRegularBill)?.ValidateSpecialMentions();
				}
				SpecialMentionsInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo SpecialMentionsInfo
		{
			get { return GetZPropertyInfo(nameof(SpecialMentions)); }
		}

		#endregion
	}
}
