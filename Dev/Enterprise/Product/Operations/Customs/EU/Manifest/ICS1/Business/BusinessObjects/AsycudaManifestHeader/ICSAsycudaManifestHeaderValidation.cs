using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.Business
{
	public class ICSAsycudaManifestHeaderValidation : AsycudaManifestHeaderValidation
	{
		public ICSAsycudaManifestHeaderValidation(AsycudaManifestHeader parent)
			: base(parent)
		{
		}

		protected override void CheckAMA_VesselName()
		{
			base.CheckAMA_VesselName();
			if ((Parent.IsSea || Parent.IsInlandTransport) && Parent.Consol == null)
			{
				if (Parent.AMA_VesselName.IsEmpty)
				{
					Parent.AMA_VesselNameInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Parent.AMA_VesselNameInfo.HumanReadableName));
				}
				else
				{
					var vessel = Parent.Vessel;
					if (vessel != null && vessel.RV_LloydsNumber.IsEmpty)
					{
						Parent.AMA_VesselNameInfo.AddMessageError(Res.GetString("5F13659A-7CB4-4766-BEBD-019B92D6A9CA", "Lloyds Number must be set for this vessel."));
					}
				}
			}
		}

		protected override void CheckAMA_RN_NKConveyanceNationality()
		{
			base.CheckAMA_RN_NKConveyanceNationality();
			if (Parent.IsRoad && Parent.Consol == null)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_RN_NKConveyanceNationalityInfo);
			}
		}

		protected override void CheckSpecificCircumstanceIndicator()
		{
			base.CheckSpecificCircumstanceIndicator();

			var specificCircumstanceIndicator = Parent.SpecificCircumstanceIndicator;
			if (Parent.Consol == null)
			{
				if (specificCircumstanceIndicator == SpecificCircumstanceList.Codes.E)
				{
					if (!ValidationHelper.HasEORIOrTCUCustomsCode(GlbCompany.CurrentCompany.OrgProxy, Parent.AMA_RN_NKCountry))
					{
						Parent.SpecificCircumstanceIndicatorInfo.AddMessageError(Res.GetString("440EBBAA-E4D9-4744-8EA3-E26137B130B6", "Organization proxy must have a valid EORI or TCUIN number setup against its organization."));
					}
					if (Parent.AMA_OA_Carrier != GlbCompany.CurrentCompany.PK
						&& (!ValidationHelper.HasEORIOrTCUCustomsCode(Parent.Carrier?.Header, Parent.AMA_RN_NKCountry)))
					{
						Parent.SpecificCircumstanceIndicatorInfo.AddMessageError(Res.GetString("F94EDDD9-9D6F-4435-AE68-DE2E29D2E8B9", "Carrier must have a valid EORI or TCUIN number setup against its organization."));
					}
				}
				if (specificCircumstanceIndicator == SpecificCircumstanceList.Codes.A)
				{
				}
			}
		}
	}
}
