using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AdditionalInfo : CusSupportingInfo
	{
		public AdditionalInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const string AdditionalInfoDescription = "AdditionalInfoDescription";
		}

		protected override ZString HumanReadableNameCore => Res.GetString("0E1EDFC3-1A10-4FFE-BA0E-0526A757959A", "Additional Info");

		protected override CusSupportingInfoValidation GetNewValidation() => new AdditionalInfoValidation(this);
		public new AdditionalInfoValidation Validation => (AdditionalInfoValidation)base.Validation;

		public new AdditionalInfoLookups Lookups => (AdditionalInfoLookups)base.Lookups;
		protected override CusSupportingInfoLookups GetNewLookups() => new AdditionalInfoLookups(this);

		#region Overrides

		[List(nameof(Lookups) + "." + nameof(AdditionalInfoLookups.CodeList))]
		[MaxLength(5)]
		[ResourceStringData("EUICS2.AdditionalInfo.CSI_Code", Caption = "Code")]
		[ReadOnlyMember(nameof(CSI_Code_ReadOnly))]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				var oldValue = CSI_Code;
				base.CSI_Code = value;
				if (oldValue != CSI_Code && !IsCopying)
				{
					additionalInfoDescriptionCache = null;
					AdditionalInfoDescriptionInfo.RefreshBinding();
				}
			}
		}

		bool CSI_Code_ReadOnly => !CSI_Description.IsEmpty && CSI_Code.IsEmpty;

		[ResourceStringData("EUICS2.AdditionalInfo.AdditionalInfoDescription", Caption = "Description")]
		public ZString AdditionalInfoDescription => CachedValueHelper.GetValue(ref additionalInfoDescriptionCache, GetAdditionalInfoDescription);
		CachedValue<ZString> additionalInfoDescriptionCache;

		public ZPropertyInfo AdditionalInfoDescriptionInfo => GetZPropertyInfo(Schema.AdditionalInfoDescription);

		ZString GetAdditionalInfoDescription()
		{
			var codeList = Lookups.CodeList as CodeDescriptionPairList;
			return codeList.GetDescriptionFromCode(CSI_Code) ?? ZString.Empty;
		}

		[MaxLength(512)]
		[ResourceStringData("EUICS2.AdditionalInfo.CSI_Description", Caption = "Text")]
		[ReadOnlyMember(nameof(CSI_Description_ReadOnly))]
		public override ZString CSI_Description
		{
			get => base.CSI_Description;
			set => base.CSI_Description = value;
		}

		bool CSI_Description_ReadOnly => !CSI_Code.IsEmpty && CSI_Description.IsEmpty;

		#endregion
	}
}
