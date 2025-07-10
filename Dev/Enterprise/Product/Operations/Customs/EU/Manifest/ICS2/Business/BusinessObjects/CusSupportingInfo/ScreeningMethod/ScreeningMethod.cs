using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class ScreeningMethod : CusSupportingInfo
	{
		public ScreeningMethod(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const string ScreeningMethodDescription = "ScreeningMethodDescription";
		}

		protected override ZString HumanReadableNameCore => Res.GetString("B135FAB0-61B0-4AEF-AF51-729652EE2753", "Screening Method");

		protected override CusSupportingInfoValidation GetNewValidation() => new ScreeningMethodValidation(this);
		public new ScreeningMethodValidation Validation => (ScreeningMethodValidation)base.Validation;

		public new ScreeningMethodLookups Lookups => (ScreeningMethodLookups)base.Lookups;
		protected override CusSupportingInfoLookups GetNewLookups() => new ScreeningMethodLookups(this);

		[List(nameof(Lookups) + "." + nameof(ScreeningMethodLookups.CodeList))]
		[MaxLength(4)]
		[ResourceStringData("EUICS2.ScreeningMethod.CSI_Code", Caption = "Code")]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				var oldValue = CSI_Code;
				base.CSI_Code = value;
				if (oldValue != CSI_Code && !IsCopying)
				{
					screeningMethodDescriptionCache = null;
					ScreeningMethodDescriptionInfo.RefreshBinding();
				}
			}
		}

		[ResourceStringData("EUICS2.ScreeningMethod.ScreeningMethodDescription", Caption = "Description")]
		public ZString ScreeningMethodDescription => CachedValueHelper.GetValue(ref screeningMethodDescriptionCache, GetScreeningMethodDescription);
		CachedValue<ZString> screeningMethodDescriptionCache;

		public ZPropertyInfo ScreeningMethodDescriptionInfo => GetZPropertyInfo(Schema.ScreeningMethodDescription);

		ZString GetScreeningMethodDescription()
		{
			var codeList = Lookups.CodeList as ZZRefCusCodeListCombinedCollection;
			codeList.Load();

			var codeCombined = codeList?.Find(c => c.ZZD_Code.EqualsIgnoringCase(CSI_Code))?.FirstOrDefault();
			return codeCombined?.ZZD_Description ?? ZString.Empty;
		}
	}
}
