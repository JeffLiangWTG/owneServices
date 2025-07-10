using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.Business.Declaration
{
	class AddInfoCusEntryInstructionValidation : EU.Business.Declaration.AddInfoCusEntryInstructionValidation
	{
		public AddInfoCusEntryInstructionValidation(AddInfoCusEntryInstruction instr) : base(instr)
		{
		}

		protected override void CheckZG_HouseSplitReference()
		{
			base.CheckZG_HouseSplitReference();
			var cusEntryInstr = (CusEntryInstruction)Parent.Parent;
			var job = cusEntryInstr.JobDeclaration;

			if (null != job)
			{
				if (GatewayList.Codes.CCSUKviaNTMsgGW.Equals(job.ZG_Gateway) && JobMessageTypeList.Codes.Import.Equals(job.JE_MessageType))
				{
					var srf = cusEntryInstr.CEI_SplitReference;

					if (!srf.IsEmpty && (srf.Length != 2 || !srf.IsNumbersOnlyOrEmpty))
					{
						Parent.ZG_HouseSplitReferenceInfo.AddMessageError(Res.GetString("E7408B52-1AA3-4981-B853-ED2CC9CE2622", "Split Reference must be blank or 2 numeric digits."));
					}
				}
			}
		}

		protected override void CheckZG_PackageCount()
		{
			base.CheckZG_PackageCount();

			if (Parent.ZG_PackageCount == ZInt.Zero)
			{
				Parent.ZG_PackageCountInfo.AddMessageError(packageCountValidationWarning);
			}
		}

		const string packageCountValidationWarning = "Package Count must not be zero.";
	}
}
