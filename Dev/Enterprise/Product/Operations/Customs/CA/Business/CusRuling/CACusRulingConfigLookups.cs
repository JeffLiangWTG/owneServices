using System.Collections;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;
using ConfigTypes = Enterprise.Customs.Universal.RefCusRulingConfigTypes;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CACusRulingConfigLookups : CusRulingConfigCombinedLookups
	{
		public CACusRulingConfigLookups(CACusRulingConfig parent) : base(parent)
		{
		}
		public override CodeDescriptionPairList RulingConfigTypeList
		{
			get
			{
				return Factory.GetCachedValue("CACusRulingConfigLookups|RulingConfigTypeList" + Parent.ZZY_Category, delegate()
				{
					var result = new CodeDescriptionPairList();

					if (Parent.IsCategoryValid)
					{
						if (Parent.IsDAT)
						{
							result.Add(new CodeDescriptionPair(ConfigTypes.Codes.DSD, ConfigTypes.Descriptions.DSD));
							result.Add(new CodeDescriptionPair(ConfigTypes.Codes.REL, ConfigTypes.Descriptions.REL));
						}
						else
						{
							result.AddPair(ConfigTypes.Codes.AcceptAmount, ConfigTypes.Descriptions.AcceptAmount);
							result.AddPair(ConfigTypes.Codes.AcceptRate, ConfigTypes.Descriptions.AcceptRate);
							result.AddPair(ConfigTypes.Codes.AdValorem, ConfigTypes.Descriptions.AdValorem);
							result.AddPair(ConfigTypes.Codes.Minimum, ConfigTypes.Descriptions.Minimum);
							result.AddPair(ConfigTypes.Codes.Maximum, ConfigTypes.Descriptions.Maximum);
							result.AddPair(ConfigTypes.Codes.Specific, ConfigTypes.Descriptions.Specific);
							if (Parent.IsDTY || Parent.IsEXD)
							{
								result.AddPair(ConfigTypes.Codes.TreatmentCode, ConfigTypes.Descriptions.TreatmentCode);
							}
							else if (Parent.IsGST || Parent.IsSIM || Parent.IsEXC)
							{
								result.AddPair(ConfigTypes.Codes.ExemptCode, ConfigTypes.Descriptions.ExemptCode);
							}
							result.AddPair(ConfigTypes.Codes.NoneFree, ConfigTypes.Descriptions.NoneFree);
						}
					}
					return result;
				});
			}
		}

		public override ICollection RulingConfigValueList
		{
			get
			{
				return Factory.GetCachedValue("CACusRulingConfigLookups|RulingConfigValueList" + Parent.ZZY_Category + Parent.ZZY_Type, delegate()
				{
					ICollection result = new CodeDescriptionPairList();
					if (Parent.IsCategoryValid && !Parent.ZZY_Type.IsEmpty && RulingConfigTypeList.ContainsCode(Parent.ZZY_Type))
					{
						if ((Parent.IsDTY || Parent.IsEXD) && Parent.ZZY_Type == ConfigTypes.Codes.TreatmentCode)
						{
							result = new TariffTreatmentCodes();
						}
						else if (Parent.ZZY_Type == ConfigTypes.Codes.ExemptCode)
						{
							if (Parent.IsGST)
							{
								result = new GSTStatusCodes();
							}
							else if (Parent.IsSIM)
							{
								result = new SIMACodes();
							}
							else if (Parent.IsEXC)
							{
								result = new ExciseTaxExemptionCodes();
							}
						}
					}
					return result;
				});
			}
		}
	}
}
