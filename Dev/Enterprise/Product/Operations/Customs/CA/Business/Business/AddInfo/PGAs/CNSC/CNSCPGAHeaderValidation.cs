using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CNSCPGAHeaderValidation : CusAddInfoValidation
	{
		public CNSCPGAHeaderValidation(AutoCusAddInfo parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateDangerousGoodsDGSubs();
		}

		CNSCPGAHeader PGAHeader => (CNSCPGAHeader)Parent;

		public void ValidateDangerousGoodsDGSubs()
		{
			ValidateCalculatedProperty(PGAHeader.DangerousGoodsDGSubsInfo);
		}

		protected void CheckDangerousGoodsDGSubs()
		{
			ListValidation.ErrorIfInvalidPK(PGAHeader.DangerousGoodsDGSubsInfo);

			if ((PGAHeader.IsRadiationDevice || PGAHeader.IsSubstance))
			{
				MandatoryValidation.MessageErrorIfNotEntered(PGAHeader.DangerousGoodsDGSubsInfo);
			}

			if ((!PGAHeader.DangerousGoodsDGSubs.IsEmpty && PGAHeader.DangerousGoods?.UNDGSubstance == null) || (PGAHeader.DangerousGoods?.UNDGSubstance != null && !CNSCUNDGSubstanceCollection.Contains(PGAHeader.DangerousGoods.UNDGSubstance)))
			{
				PGAHeader.DangerousGoodsDGSubsInfo.AddMessageError(ResString.GetMultilingualString("51EF21DC-0B1D-438D-B567-637450328B5E", "This UNDG code is not valid for Canadian Nuclear Safety Commission lines."));
			}
		}

		UNDGSubstanceCollection CNSCUNDGSubstanceCollection
		{
			get
			{
				return PGAHeader.Factory.GetCachedValue("CNSCUNDGSubstanceCollection", () =>
				{
					var query = new ZQuery(UNDGSubstanceSchema.DG_Code, new CNSCApplicableUNDGs().GetAllCodes());
					query.AddToFilter(UNDGSubstanceSchema.DG_Standard, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
					return new UNDGSubstanceCollection(PGAHeader.Factory, query);
				});
			}
		}
	}
}
