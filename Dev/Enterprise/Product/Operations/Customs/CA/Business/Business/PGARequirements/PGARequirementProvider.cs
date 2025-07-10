using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	internal interface IPGARequirementSupporter
	{
		BusinessObjectFactory Factory { get; }

		ZBool IsDeleted { get; }
		ZString Tariff { get; }
		ZDateTime EffectiveDate { get; }
		ZBool IsPGARequirementEffective { get; }

		ZPropertyInfo HCIndInfo { get; }
		ZPropertyInfo PHACIndInfo { get; }
		ZPropertyInfo NRCanIndInfo { get; }
		ZPropertyInfo DFOIndInfo { get; }
		ZPropertyInfo GACIndInfo { get; }
		ZPropertyInfo ECCCIndInfo { get; }
		ZPropertyInfo CNSCIndInfo { get; }
		ZPropertyInfo TCIndInfo { get; }
		ZPropertyInfo CFIAIndInfo { get; }

		IPGAProgramRequirementProvider CFIARequirementProvider { get; }
		IPGAProgramRequirementProvider CNSCRequirementProvider { get; }
		IPGAProgramRequirementProvider DFORequirementProvider { get; }
		IPGAProgramRequirementProvider ECCCRequirementProvider { get; }
		IPGAProgramRequirementProvider GACRequirementProvider { get; }
		IPGAProgramRequirementProvider HCRequirementProvider { get; }
		IPGAProgramRequirementProvider NRCanRequirementProvider { get; }
		IPGAProgramRequirementProvider PHACRequirementProvider { get; }
		IPGAProgramRequirementProvider TCRequirementProvider { get; }
		SetterSuspender SetterSuspender { get; }
	}

	public class PGARequirementProvider
	{
		internal PGARequirementProvider(IPGARequirementSupporter supporter)
		{
			this.supporter = Argument.NotNull(supporter, "PGARequirementSupporter");
		}
		readonly IPGARequirementSupporter supporter;

		internal IPGARequirementSupporter Supporter => supporter;

		public BusinessObjectFactory Factory => supporter.Factory;

		public ZBool IsPGARequirementEffective => supporter.IsPGARequirementEffective;

		public ZPropertyInfo GetIndicatorInfo(ZString agencyCode)
		{
			switch (agencyCode)
			{
				case PGACodes.Codes.HC:
					return supporter.HCIndInfo;
				case PGACodes.Codes.PHAC:
					return supporter.PHACIndInfo;
				case PGACodes.Codes.NRCan:
					return supporter.NRCanIndInfo;
				case PGACodes.Codes.DFO:
					return supporter.DFOIndInfo;
				case PGACodes.Codes.GAC:
					return supporter.GACIndInfo;
				case PGACodes.Codes.ECCC:
					return supporter.ECCCIndInfo;
				case PGACodes.Codes.CNSC:
					return supporter.CNSCIndInfo;
				case PGACodes.Codes.TC:
					return supporter.TCIndInfo;
				case PGACodes.Codes.CFIA:
					return supporter.CFIAIndInfo;
			}

			return null;
		}

		public IPGAProgramRequirementProvider GetProgramRequirementProvider(ZString agencyCode)
		{
			switch (agencyCode)
			{
				case PGACodes.Codes.CFIA:
					return supporter.CFIARequirementProvider;
				case PGACodes.Codes.CNSC:
					return supporter.CNSCRequirementProvider;
				case PGACodes.Codes.DFO:
					return supporter.DFORequirementProvider;
				case PGACodes.Codes.ECCC:
					return supporter.ECCCRequirementProvider;
				case PGACodes.Codes.GAC:
					return supporter.GACRequirementProvider;
				case PGACodes.Codes.HC:
					return supporter.HCRequirementProvider;
				case PGACodes.Codes.NRCan:
					return supporter.NRCanRequirementProvider;
				case PGACodes.Codes.PHAC:
					return supporter.PHACRequirementProvider;
				case PGACodes.Codes.TC:
					return supporter.TCRequirementProvider;
			}

			return null;
		}

		public CodeDescriptionPairList GetGovernmentAgencyCodesList()
		{
			return PGACodes.GetGovernmentAgencyCodesList(Factory);
		}

		public ZString TariffNo => supporter.Tariff;

		public ZDateTime TariffEffectiveDate => supporter.EffectiveDate;
	}
}
