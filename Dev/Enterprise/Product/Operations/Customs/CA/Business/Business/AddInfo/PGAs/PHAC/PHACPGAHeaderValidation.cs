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
	public class PHACPGAHeaderValidation : CusAddInfoValidation
	{
		public PHACPGAHeaderValidation(AutoCusAddInfo parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateDangerousGoodsDGSubs();
		}

		PHACPGAHeader PGAHeader => (PHACPGAHeader)Parent;

		public void ValidateDangerousGoodsDGSubs()
		{
			ValidateCalculatedProperty(PGAHeader.DangerousGoodsDGSubsInfo);
		}

		protected void CheckDangerousGoodsDGSubs()
		{
			ListValidation.ErrorIfInvalidPK(PGAHeader.DangerousGoodsDGSubsInfo);

			if ((!PGAHeader.DangerousGoodsDGSubs.IsEmpty && PGAHeader.DangerousGoods?.UNDGSubstance == null) || (PGAHeader.DangerousGoods?.UNDGSubstance != null && !PHACUNDGSubstanceCollection.Contains(PGAHeader.DangerousGoods.UNDGSubstance)))
			{
				PGAHeader.DangerousGoodsDGSubsInfo.AddMessageError(ResString.GetMultilingualString("1FE0518A-6654-479E-B3AA-7818CE2B4715", "This UNDG code is not valid for Public Health Agency of Canada lines."));
			}
		}

		UNDGSubstanceCollection PHACUNDGSubstanceCollection
		{
			get
			{
				return PGAHeader.Factory.GetCachedValue("PHACUNDGSubstanceCollection", () =>
				{
					var query = new ZQuery(UNDGSubstanceSchema.DG_Code, new PHACApplicableUNDGs().GetAllCodes());
					query.AddToFilter(UNDGSubstanceSchema.DG_Standard, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
					return new UNDGSubstanceCollection(PGAHeader.Factory, query);
				});
			}
		}
	}
}
