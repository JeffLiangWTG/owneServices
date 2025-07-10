using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public abstract class VATDeferStrategy : EU.Business.Declaration.VATDeferStrategy
	{
		public VATDeferStrategy(EU.Business.Declaration.JobDeclaration declaration) : base(declaration)
		{
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override OrgHeader GetSourceForVAT(ZString deferType) => Declaration.Importer;

		protected override void DefaultVATDeferType()
		{
			base.DefaultVATDeferType();

			if (Declaration.IsImport)
			{
				var source = Declaration.Importer;
				if (source != null)
				{
					var vatDeferImpAddInfo = FROrgImpAddInfo.Get(source);
					vatDeferImpAddInfo.Deserialise();
					var vatDeferType = vatDeferImpAddInfo.ZO_VATDeferType;
					if (!vatDeferType.IsEmpty)
					{
						Declaration.ZG_VATDeferType = vatDeferType;
					}
				}
			}
		}

		public override void OnVATDeferTypeChanged()
		{
			base.OnVATDeferTypeChanged();
			SetVatCana();
		}

		protected void SetVatCana()
		{
			var type = Declaration.ZG_VATDeferType;

			if (type == VATProcedureList.Codes._2)
			{
				var guarantee = Declaration.Ai2Permit;
				if (guarantee != null)
				{
					var rules = guarantee.CusGuaranteeRules.FirstOrDefault(x => x.CPR_RuleCode == PermitRuleCodeList.Codes.CAN);
					if (rules != null)
					{
						Declaration.ZG_VATCANACode = rules.CPR_ValueFrom.Left(Declaration.ZG_VATCANACodeInfo.MaxLength);
					}
				}
			}
			else
			{
				if (!Declaration.ZG_VATCANACode.IsEmpty)
				{
					Declaration.ZG_VATCANACode = ZString.Empty;
				}
			}
		}

		protected override void DefaultVATDeferNumber()
		{
			base.DefaultVATDeferNumber();
			if (Declaration.ZG_VATDeferType == VATProcedureList.Codes.S)
			{
				Declaration.ZG_VATDeferNumberInfo.ClearValue();
			}
		}

		public virtual void OnDeclarantTypeChanged()
		{
		}

		protected override ZString GetVATDeferNumber(OrgHeader header)
		{
			var vatDeferNumber = ZString.Empty;
			var type = Declaration.ZG_VATDeferType;
			if (type == VATProcedureList.Codes.L)
			{
				vatDeferNumber = Declaration.VATNumberSupporter.GetVATDeferNumberForAutoliquidation(header);
			}
			else if (type == VATProcedureList.Codes._2)
			{
				var query = Declaration.GetAi2PermitQuery(header?.PK ?? ZGuid.Empty);
				vatDeferNumber = Declaration.Factory.LoadTop1<CusGuaranteeHeader>(query)?.CPH_Number ?? ZString.Empty;
			}
			return vatDeferNumber;
		}
	}
}
