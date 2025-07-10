using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MasterFiles
{
	public class FROrgImpAddInfo : AutoFROrgImpAddInfo, Integration.Customs.FR.IOrgImpAddInfo
	{
		public FROrgImpAddInfo(BusinessObjectFactory factory) : base(factory)
		{
			using (GetValidationSuspender())
			{
				using (SuspendSettingHasChanges())
				{
					Deserialise(false);
				}
			}
		}

		public FROrgImpAddInfo(ZPropertyInfoString parentPropertyInfo) : base(parentPropertyInfo.BizObj.Factory)
		{
			ParentPropertyInfo = parentPropertyInfo;
			using (GetValidationSuspender())
			{
				using (SuspendSettingHasChanges())
				{
					Deserialise(false);
				}
			}
		}

		[ResourceStringData("FROrgImpAddInfo|ZO_VATDeferType", Caption = "VAT Procedure")]
		[List(nameof(Lookups) + "." + nameof(FROrgImpAddInfoLookups.VATProcedureList))]
		public override ZString ZO_VATDeferType
		{
			get => base.ZO_VATDeferType;
			set => base.ZO_VATDeferType = value;
		}

		[ResourceStringData("FROrgImpAddInfo|ZO_DeltaG1SubProcedure", Caption = "Delta G1 Sub Procedure")]
		[List(nameof(Lookups) + "." + nameof(FROrgImpAddInfoLookups.DeltaG1SubProcedureList))]
		public override ZString ZO_DeltaG1SubProcedure
		{
			get => base.ZO_DeltaG1SubProcedure;
			set => base.ZO_DeltaG1SubProcedure = value;
		}

		[ResourceStringData("FROrgImpAddInfo|ZO_VATProcedureDateLimit", Caption = "VAT Procedure Date Limit", ShortCaption = "Date")]
		public override ZDateTime ZO_VATProcedureDateLimit
		{
			get => base.ZO_VATProcedureDateLimit;
			set => base.ZO_VATProcedureDateLimit = value;
		}

		public OrgHeader OrgHeader => Parent?.OrgHeader;

		public OrgCountryData Parent
		{
			get
			{
				return (OrgCountryData)ParentPropertyInfo.BizObj;
			}
		}

		public static FROrgImpAddInfo Get(OrgHeader organisation) => (FROrgImpAddInfo)organisation?.GetCountryData(Core.Constants.CountryCodes.France).ImpAddInfo;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			ZO_VATDeferType = VATProcedureList.Codes.S;
		}
	}
}
