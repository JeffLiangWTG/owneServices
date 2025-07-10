using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class GuaranteeForEntryInstruction : CommonGuarantee
	{
		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : CusBondDetail.Schema
		{
			public new const int PW_BondTypeMaxLength = 1;

			public new const int PW_BondNumberMaxLength = 24;

			public new const int PW_BondNumber2MaxLength = 35;

			public new const int PW_PasswordMaxLength = 4;

			public new const int PW_RX_NKCurrencyMaxLength = 3;

			public new const int PW_BondFiledPortMaxLength = 8;

			public new const int PW_SuretyCodeMaxLength = 3;
		}

		#endregion

		public GuaranteeForEntryInstruction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected sealed override TypeLoaderCollection ParentLoaders
		{
			get
			{
				var result = base.ParentLoaders;
				result.Add(typeof(CusEntryInstruction));
				return result;
			}
		}

		protected override ZString HumanReadableNameCore => Res.GetString("262CB728-C88C-4456-AB81-03DD53D14C5D", "Guarantee Reference");

		public CusEntryInstruction EntryInstruction => entryInstruction ?? (entryInstruction = Factory.Load<CusEntryInstruction>(PW_ParentID));
		CusEntryInstruction entryInstruction;

		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.CusEntryInstructionGuarantee|PW_BondType", Caption = "Type", FullDescription = "UCC 8/3 Guarantee Type")]
		[MaxLength(Schema.PW_BondTypeMaxLength)]
		public override ZString PW_BondType { get => base.PW_BondType; set => base.PW_BondType = value; }

		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.CusEntryInstructionGuarantee|PW_BondNumber", Caption = "Reference", FullDescription = "UCC 8/3 Guarantee Reference or GRN")]
		[MaxLength(Schema.PW_BondNumberMaxLength)]
		public override ZString PW_BondNumber { get => base.PW_BondNumber; set => base.PW_BondNumber = value; }

		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.CusEntryInstructionGuarantee|PW_BondNumber2", Caption = "Reference 2", FullDescription = "UCC 8/3 Other Guarantee Reference")]
		[MaxLength(Schema.PW_BondNumber2MaxLength)]
		public override ZString PW_BondNumber2 { get => base.PW_BondNumber2; set => base.PW_BondNumber2 = value; }

		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.CusEntryInstructionGuarantee|PW_Password", Caption = "Access Code (PIN)", FullDescription = "UCC 8/3 Guarantee Access Code or PIN")]
		[MaxLength(Schema.PW_PasswordMaxLength)]
		public override ZString PW_Password { get => base.PW_Password; set => base.PW_Password = value; }

		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.CusEntryInstructionGuarantee|PW_RX_NKCurrency", Caption = "Currency", FullDescription = "UCC 8/3 Guarantee Currency")]
		[MaxLength(Schema.PW_RX_NKCurrencyMaxLength)]
		public override ZString PW_RX_NKCurrency { get => base.PW_RX_NKCurrency; set => base.PW_RX_NKCurrency = value; }

		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.CusEntryInstructionGuarantee|PW_BondAmount", Caption = "Duty Amount", FullDescription = "UCC 8/3 Guaranteed Duty Amount")]
		[DecimalPlaces(nameof(GuaranteeBondAmountDecimalPlaces))]
		public override ZDecimal PW_BondAmount { get => base.PW_BondAmount; set => base.PW_BondAmount = value; }

		public ZInt GuaranteeBondAmountDecimalPlaces => GuaranteeBondAmountDecimalPlacesCore;
		protected virtual ZInt GuaranteeBondAmountDecimalPlacesCore => 2;

		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.CusEntryInstructionGuarantee|PW_BondFiledPort", Caption = "Customs Office", FullDescription = "UCC 8/3 Customs Office")]
		[MaxLength(Schema.PW_BondFiledPortMaxLength)]
		public override ZString PW_BondFiledPort { get => base.PW_BondFiledPort; set => base.PW_BondFiledPort = value; }

		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.CusEntryInstructionGuarantee|PW_SuretyCode", Caption = "Reduction Fraction", FullDescription = "Reduction percentage applied to the Guaranteed Duty Amount")]
		[MaxLength(Schema.PW_SuretyCodeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(GuaranteeForEntryInstructionLookups.LiabilityApplicablePercentageList))]
		public override ZString PW_SuretyCode { get => base.PW_SuretyCode; set => base.PW_SuretyCode = value; }

		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.CusEntryInstructionGuarantee|PW_HolderIdentification",
			Caption = "Holder Identification",
			FullDescription = "Guarantee Holder Identification",
			ShortCaption = "Holder ID",
			MediumCaption = "Holder Identification")]
		[MaxLength(Schema.PW_HolderIdentificationMaxLength)]
		public override ZString PW_HolderIdentification { get => base.PW_HolderIdentification; set => base.PW_HolderIdentification = value; }

		public new GuaranteeForEntryInstructionLookups Lookups => (GuaranteeForEntryInstructionLookups)base.Lookups;

		protected override CusBondDetailLookups GetNewLookups() => new GuaranteeForEntryInstructionLookups(this);

		protected override void SyncroniseWithGuaranteeHeader(BaseCusGuaranteeHeader guaranteeHeader)
		{
			base.SyncroniseWithGuaranteeHeader(guaranteeHeader);

			PW_RX_NKCurrency = guaranteeHeader.CPH_UnitOfMeasure.Left(Schema.PW_RX_NKCurrencyMaxLength);
			PW_SuretyCode = guaranteeHeader.CusGuaranteeRules
				.Cast<CusGuaranteeRule>()
				.FirstOrDefault(x => x.CPR_RuleCode == PermitRuleCodeList.Codes.LAP)
				?.CPR_ValueFrom ?? ZString.Empty;
		}

		protected override CusBondDetailValidation GetNewValidation() => new GuaranteeForEntryInstructionValidation(this);
	}
}
