using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business
{
	public class GBOrgImpAddInfo : AutoGBOrgImpAddInfo, Integration.Customs.GB.IOrgImpAddInfo
	{
		public GBOrgImpAddInfo(BusinessObjectFactory factory) : base(factory)
		{
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				Deserialise();
			}
		}

		public GBOrgImpAddInfo(ZPropertyInfoString parentPropertyInfo)
			: base(parentPropertyInfo.BizObj.Factory)
		{
			ParentPropertyInfo = parentPropertyInfo;
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				Deserialise();
			}
		}

		#region Properties

		[List(nameof(Lookups) + "." + nameof(GBOrgImpAddInfoLookups.DefermentMethodList))]
		public override ZString ZO_VATDeferType
		{
			get { return base.ZO_VATDeferType; }
			set { base.ZO_VATDeferType = value; }
		}

		public override ZPropertyInfo ZO_VATDeferTypeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.ZO_VATDeferType); }
		}

		[List(nameof(Lookups) + "." + nameof(GBOrgImpAddInfoLookups.ClientsDucrSourceAttributeFieldList))]
		public override ZString ZO_Box44ClientsDucrSourceAttributeField
		{
			get { return base.ZO_Box44ClientsDucrSourceAttributeField; }
			set { base.ZO_Box44ClientsDucrSourceAttributeField = value; }
		}

		public override ZPropertyInfo ZO_Box44ClientsDucrSourceAttributeFieldInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.ZO_Box44ClientsDucrSourceAttributeField); }
		}

		[List(nameof(Lookups) + "." + nameof(GBOrgImpAddInfoLookups.ClientsDucrSourceAttributeFieldList))]
		public override ZString ZO_Box7DeclarantsReferenceSourceAttributeField
		{
			get { return base.ZO_Box7DeclarantsReferenceSourceAttributeField; }
			set { base.ZO_Box7DeclarantsReferenceSourceAttributeField = value; }
		}

		public override ZPropertyInfo ZO_Box7DeclarantsReferenceSourceAttributeFieldInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.ZO_Box7DeclarantsReferenceSourceAttributeField); }
		}

		#endregion

		public static GBOrgImpAddInfo Get(OrgHeader organisation)
		{
			var addInfo = (GBOrgImpAddInfo)organisation?.GetCountryData(Core.Constants.CountryCodes.UnitedKingdom).ImpAddInfo;
			if (organisation != null)
			{
				addInfo.Organisation = organisation;
			}

			return addInfo;
		}

		public OrgHeader Organisation { get; set; }
	}
}
