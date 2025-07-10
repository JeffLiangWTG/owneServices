using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class NRCanPGAHeaderValidation : CusAddInfoValidation
	{
		public NRCanPGAHeaderValidation(AutoCusAddInfo parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateDangerousGoodsDGSubs();
		}

		NRCanPGAHeader PGAHeader => (NRCanPGAHeader)Parent;

		public void ValidateDangerousGoodsDGSubs()
		{
			ValidateCalculatedProperty(PGAHeader.DangerousGoodsDGSubsInfo);
		}

		protected void CheckDangerousGoodsDGSubs()
		{
			ListValidation.ErrorIfInvalidPK(PGAHeader.DangerousGoodsDGSubsInfo);

			if (PGAHeader.CA_EXPProgramInd == YesNoList.Codes.Yes)
			{
				MandatoryValidation.MessageErrorIfNotEntered(PGAHeader.DangerousGoodsDGSubsInfo);
				if (!PGAHeader.DangerousGoodsDGSubs.IsEmpty)
				{
					var code = PGAHeader.DangerousGoods?.UNDGSubstance?.DG_UNNO;
					var isNumber = ZInt.TryParse(code, out var codeValue);
					if ((code.HasValue && code.Value.Length != 4) || !isNumber || (codeValue != 1442 && !(codeValue >= 1 && codeValue <= 999)))
					{
						PGAHeader.DangerousGoodsDGSubsInfo.AddMessageError(ResString.GetMultilingualString("54EE8EF6-32CC-4A4D-B9D6-81AA0F7D5631", "This UNDG code is not valid for Natural Resources Canada lines."));
					}
				}
			}
		}
	}
}
