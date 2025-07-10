using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class NonGADetailLookups : Customs.Business.CusSupportingInfoLookups
	{
		public NonGADetailLookups(NonGADetail parent)
			: base(parent)
		{
		}

		public NonGADetail GAApproval
		{
			get { return Parent; }
		}

		protected new NonGADetail Parent
		{
			get { return (NonGADetail)base.Parent; }
		}

		ZDateTime EffectiveAssessmentDate => Parent.Parent.DeclarationDate;

		public override ICollection CodeList => Factory.GetCachedValue<NonRequirementTypeCodeList>();

		public ZZRefCusCodeListCombinedCollection NonGAReasonTypeList
		{
			get
			{
				return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory,
										new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, SQLComparisonOperator.StartsWith, Parent.CSI_Procedure + Parent.CSI_Code),
										Core.Constants.CountryCodes.KoreaSouth,
										new ZString[] { Constants.ZZ.NKCodeType.INGAR },
										EffectiveAssessmentDate,
										null);
			}
		}

		public ZZRefCusCodeListCombinedCollection OGARegulationCategoryList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.OGARegulationCategory, EffectiveAssessmentDate);
	}
}
