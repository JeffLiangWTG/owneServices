using System.Collections;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class FECChallengeLookups : CusCodeDataLookups
	{
		public FECChallengeLookups(AutoCusCodeData parent)
			: base(parent)
		{
		}

		public FECChallenge fecChallenge => (FECChallenge)Parent;

		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
		public IList NewValueList
		{
			get
			{
				IList list = new CodeDescriptionPairList();
				if (fecChallenge.IsParentEntryHeader)
				{
					var parentBO = fecChallenge.Parent as CusEntryHeader;
					if (parentBO != null)
					{
						switch (fecChallenge.CY_Code)
						{
							case FECChallengeFields.Codes.JE_FLG:
								list = (IList)parentBO.Declaration.Lookups.TransportCountryList;
								break;
							case FECChallengeFields.Codes.JE_DSP:
								list = parentBO.Declaration.Lookups.Origins;
								break;
							case FECChallengeFields.Codes.JE_DST:
								list = parentBO.Declaration.Lookups.FinalDestinations;
								break;
						}
					}
				}
				else if (fecChallenge.IsParentEntryLine)
				{
					var parentBO = fecChallenge.Parent as CusEntryLine;
					if (parentBO != null && parentBO.InvoiceLines.Count > 0)
					{
						switch (fecChallenge.CY_Code)
						{
							case FECChallengeFields.Codes.JI_ORG:
								list = parentBO.InvoiceLines[0].Lookups.CountryList as IList;
								break;
							case FECChallengeFields.Codes.JI_NettMassUQ:
								list = parentBO.InvoiceLines[0].Lookups.WeightUQList;
								break;
							case FECChallengeFields.Codes.JI_SuppUQ:
								list = parentBO.InvoiceLines[0].Lookups.CustomsUQList;
								break;
						}
					}
				}
				return list;
			}
		}

		public CodeDescriptionPairList CusCodeDataTypes => Factory.GetCachedValue<CusCodeDataTypeList>();
	}
}
