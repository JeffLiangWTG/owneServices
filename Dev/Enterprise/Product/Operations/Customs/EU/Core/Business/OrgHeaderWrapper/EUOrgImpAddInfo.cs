using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business
{
	public class EUOrgImpAddInfo : AutoEUOrgImpAddInfo
	{
		public EUOrgImpAddInfo(BusinessObjectFactory factory) : base(factory)
		{
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				Deserialise();
			}
		}

		public EUOrgImpAddInfo(ZPropertyInfoString parentPropertyInfo)
			: base(parentPropertyInfo.BizObj.Factory)
		{
			ParentPropertyInfo = parentPropertyInfo;
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				Deserialise();
			}
		}

		[List(nameof(Lookups) + "." + nameof(EUOrgImpAddInfoLookups.DefermentMethodList))]
		[ResourceStringData("EUOrgImpAddInfo.ZO_OtherDeferType", Caption = "[48] Payment Party", FullDescription = "Deferment account used for all charges except VAT.")]
		public override ZString ZO_OtherDeferType
		{
			get { return base.ZO_OtherDeferType; }
			set { base.ZO_OtherDeferType = value; }
		}

		[ResourceStringData("EUOrgImpAddInfo.ZO_Box14UseIndirectRepresentation", Caption = "Indirect Rep.?", MediumCaption = "Use indirect representation?", FullDescription = "Use indirect representation for box 14?  If not, the default of direct (or self when relevant) will be used.")]
		public override ZBool ZO_Box14UseIndirectRepresentation
		{
			get { return base.ZO_Box14UseIndirectRepresentation; }
			set { base.ZO_Box14UseIndirectRepresentation = value; }
		}

		[ResourceStringData("EUOrgImpAddInfo.ZO_UseFr3FiscalRepresentation", Caption = "Use postponed VAT accounting?")]
		public override ZBool ZO_UseFr3FiscalRepresentation
		{
			get { return base.ZO_UseFr3FiscalRepresentation; }
			set { base.ZO_UseFr3FiscalRepresentation = value; }
		}

		[ResourceStringData("EUOrgImpAddInfo.ZO_Box14UseIndirectRepresentationForExporter", Caption = "Use indirect representation?")]
		public override ZBool ZO_Box14UseIndirectRepresentationForExporter
		{
			get => base.ZO_Box14UseIndirectRepresentationForExporter;
			set => base.ZO_Box14UseIndirectRepresentationForExporter = value;
		}

		public static EUOrgImpAddInfo Get(OrgHeader organisation, ZString countryCode)
		{
			EUOrgImpAddInfo result = null;
			if (organisation != null)
			{
				var customsJurisdiction = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode);
				if (ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(customsJurisdiction))
				{
					result = (EUOrgImpAddInfo)organisation.GetCountryData(countryCode).RegionSpecificImpAddInfo;
				}
				return result;
			}

			return result;
		}
	}
}
