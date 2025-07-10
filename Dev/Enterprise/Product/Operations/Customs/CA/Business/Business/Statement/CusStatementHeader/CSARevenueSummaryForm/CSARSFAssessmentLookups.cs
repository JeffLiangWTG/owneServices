using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CSARSFAssessmentLookups : ZLookups
	{
		public CSARSFAssessmentLookups(CSARSFAssessment parent) : base(parent)
		{
		}

		public CodeDescriptionPairList TypeList => Factory.GetCachedValue<CustomsAssessmentsCodes>();
	}
}
