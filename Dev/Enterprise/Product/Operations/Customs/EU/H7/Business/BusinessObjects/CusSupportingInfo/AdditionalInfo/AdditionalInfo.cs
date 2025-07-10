using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.H7.Business
{
	public class AdditionalInfo : BaseAdditionalInfo
	{
		public AdditionalInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const string AdditionalInfoDescription = nameof(AdditionalInfo.AdditionalInfoDescription);
		}

		public new AdditionalInfoValidation Validation => (AdditionalInfoValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation() => new AdditionalInfoValidation(this);

		public ZString DataGrouping
		{
			get
			{
				if (!dataGroupingCached.HasValue)
				{
					dataGroupingCached = Parent is IDataGroupingProvider provider ? provider.DataGrouping : ZString.Empty;
				}
				return dataGroupingCached.Value;
			}
		}
		ZString? dataGroupingCached;

		[MaxLength(5)]
		[ResourceStringData("EUH7.AdditionalInfo.CSI_Code", Caption = "Code")]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set => base.CSI_Code = value;
		}

		[MaxLength(512)]
		[ResourceStringData("EUH7.AdditionalInfo.CSI_Description", Caption = "Description")]
		public override ZString CSI_Description
		{
			get => base.CSI_Description;
			set => base.CSI_Description = value;
		}

		[ResourceStringData("EUH7.AdditionalInfo.AdditionalInfoDescription", Caption = "Description")]
		public ZString AdditionalInfoDescription => Factory.GetValue(ref additionalInfoDescriptionCache, GetAdditionalInfoDescription);
		CachedProperty<ZString> additionalInfoDescriptionCache;

		public ZPropertyInfo AdditionalInfoDescriptionInfo => GetZPropertyInfo(Schema.AdditionalInfoDescription);

		protected virtual ZString GetAdditionalInfoDescription()
		{
			var codeList = Lookups.CodeList as CodeDescriptionPairList;
			return codeList?.GetDescriptionFromCode(CSI_Code);
		}
	}
}
