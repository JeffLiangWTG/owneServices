using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using EUGuaranteeTypeList = Enterprise.Customs.EU.Business.CodeDescriptionPairLists.EUGuaranteeTypeList;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class GuaranteeLookups : NctsGuaranteeLookups
	{
		public GuaranteeLookups(Guarantee parent) : base(parent)
		{
		}

		protected new Guarantee Parent => (Guarantee)base.Parent;

		protected override CodeDescriptionPairList BondTypeListCore
		{
			get
			{
				var nctsHeader = Parent.NctsHeader;
				var movementHeader = nctsHeader?.MovementHeader;
				var isSimplifiedProcedure = movementHeader?.IsSimplifiedNctsProcedure ?? false;
				var isTIRDeclarantType = (movementHeader?.BM_InBondEntryType ?? ZString.Empty) == NctsDeclarationTypeList.Codes.TIR;

				return Factory.GetCachedValue(string.Format(Culture.Invariant, "DEGuaranteeTypeList_{0}_{1}", isSimplifiedProcedure, isTIRDeclarantType), () =>
				{
					var result = new CodeDescriptionPairList();

					if (isTIRDeclarantType)
					{
						result.AddPair(NctsGuaranteeTypeList.Codes.B, NctsGuaranteeTypeList.Descriptions.B);
					}
					else if (isSimplifiedProcedure)
					{
						result.AddPair(NctsGuaranteeTypeList.Codes._0, NctsGuaranteeTypeList.Descriptions._0);
						result.AddPair(NctsGuaranteeTypeList.Codes._1, NctsGuaranteeTypeList.Descriptions._1);
					}
					else
					{
						result = new NctsGuaranteeTypeList();
					}

					return result;
				});
			}
		}

		public override CusGuaranteeHeaderCollection ReferenceNumbers
		{
			get
			{
				var bondType = Parent.PW_BondType;
				return Factory.GetCachedValue($"DE.NCTS.Business.GuaranteeLookups.ReferenceNumbers_{bondType}_{PrimaryGuaranteeHolderAddress}", () =>
				{
					var guarantees = new CusGuaranteeHeaderCollection(Factory, new ZString[] { Core.Constants.CountryCodes.Germany }, new ZString[] { EUGuaranteeTypeList.Codes.TRA });
					guarantees.AdditionalFilter.AddToFilter(CusPermitHeaderSchema.CPH_SubType, bondType);
					guarantees.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusGuaranteeHeaderCollection.FilterConstants.GuaranteeSubType, "Property", bondType));
					guarantees.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusGuaranteeHeaderCollection.FilterConstants.GuaranteeHolders, "Property1", PrimaryGuaranteeHolderAddress));
					return guarantees;
				});
			}
		}

		public BaseCusGuaranteeHeader SingleReferenceNumber => Factory.GetCachedValue("DE.NCTS.Business.GuaranteeLookups.SingleReferenceNumber_" + Parent.PW_BondType, () => ReferenceNumbers.Count == 1 ? ReferenceNumbers[0] : null);

		protected override CodeDescriptionPairList AccessCodeListCore
		{
			get
			{
				var guaranteeHeader = ReferenceNumbers.FirstOrDefault(x => x.CPH_Number == Parent.PW_BondNumber);
				if (guaranteeHeader != null)
				{
					return Factory.GetCachedValue(string.Format(Culture.Invariant, "DEAccessCodeListCore_{0}_{1}", guaranteeHeader.CPH_Number, guaranteeHeader.AdditionalAccessCodes.Count), () =>
					{
						var result = new CodeDescriptionPairList();
						var cusGuaranteeRules = guaranteeHeader.AdditionalAccessCodes;
						foreach (var rule in cusGuaranteeRules)
						{
							result.AddPair(rule.CPR_ValueFrom, rule.CPR_Description);
						}
						return result;
					});
				}
				else
				{
					return base.AccessCodeListCore;
				}
			}
		}
	}
}
