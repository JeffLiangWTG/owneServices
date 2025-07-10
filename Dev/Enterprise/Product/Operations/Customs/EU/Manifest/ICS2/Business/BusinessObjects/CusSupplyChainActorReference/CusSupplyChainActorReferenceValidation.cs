using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class CusSupplyChainActorReferenceValidation : EU.Business.Declaration.CusSupplyChainActorReferenceValidation
	{
		public CusSupplyChainActorReferenceValidation(CusSupplyChainActorReference parent)
			: base(parent)
		{
		}

		protected override void CheckOwnerOrgPK()
		{
		}

		protected override void CheckCFR_ReferenceIsNotEmpty()
		{
			var parent = Parent;
			var targetInfo = parent.CFR_ReferenceInfo;
			if (parent.CFR_Reference.IsEmpty)
			{
				if (!targetInfo.ReadOnly)
				{
					targetInfo.AddError(Res.GetString("3DCB0D8B-73C2-4577-BBB1-15F45D466E98", "You have not entered an Identification Number."));
				}
				else
				{
					targetInfo.AddError(Res.GetString("46748C01-88D4-415D-90B5-9D7AEEBDAED8",
						"The selected Organization ({0}) does not contain an {1} Customs Code.",
						parent.Owner?.Header?.OH_Code,
						OrgCusCode.EuropeanUnionSharedCodeTypes.Eori));
				}
			}
		}
	}
}
