using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	public class DuimpTaxRegime : Customs.Business.CusSupportingInfo
	{
		public new class Schema : Customs.Business.AutoCusSupportingInfo.Schema
		{
			public const string ProcedureDescription = "ProcedureDescription";
			public const string RateTypeDescription = "RateTypeDescription";
			public const string Mandatory = "Mandatory";
		}

		public DuimpTaxRegime(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool SupportsNotes => false;

		public new JobComInvoiceLine Parent => base.Parent as JobComInvoiceLine;

		JobDeclaration Declaration => Parent.Declaration;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.DuimpTaxRegime;
			CSI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
		}

		public override bool CanDelete => !IsDeleted && !IsMandatory;

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("4CF506EF-5872-4FCB-AA56-453680599CA1", "The legal base '{0}' cannot be deleted because is mandatory.", CSI_Procedure);

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.BR.Business.DuimpTaxRegime|CSI_Procedure", Caption = "Legal Basis")]
		public override ZString CSI_Procedure { get => base.CSI_Procedure; set => base.CSI_Procedure = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.DuimpTaxRegime|ProcedureDescription", Caption = "Legal Basis Description")]
		public ZString ProcedureDescription => (Declaration.GetDuimpLegalBaseListFromMessage(Parent.JI_Tariff, Parent.JI_CountryOfOrigin) ?? BRRefCusCodeListTypes.GetDuimpLegalBaseList(Factory)).GetDescriptionFromCode(CSI_Procedure);

		public ZPropertyInfo ProcedureDescriptionInfo => GetZPropertyInfo(Schema.ProcedureDescription);

		[ResourceStringData("Enterprise.Customs.BR.Business.DuimpTaxRegime|MandatoryDescription", Caption = "Type")]
		public ZString MandatoryDescription => IsMandatory ? IsMandatoryDescription : IsNotMandatoryDescription;

		public ZPropertyInfo MandatoryDescriptionInfo => GetZPropertyInfo(Schema.ProcedureDescription);

		public bool IsMandatory => Profiles.Any(x => x.IsMandatory);

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.BR.Business.DuimpTaxRegime|CSI_SubType", Caption = "Rate Type")]
		public override ZString CSI_SubType { get => base.CSI_SubType; set => base.CSI_SubType = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.DuimpTaxRegime|RateTypeDescription", Caption = "Rate Type Description")]
		public ZString RateTypeDescription => rateTypeDescription ??= RateTypes.GetDescriptionFromCode(CSI_SubType);
		string rateTypeDescription;

		public ZPropertyInfo RateTypeDescriptionInfo => GetZPropertyInfo(Schema.RateTypeDescription);

		CodeDescriptionPairList RateTypes => Factory.GetCachedValue("BRRateTypeDescriptions", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddRange(RefCusRateType.Loader.GetRateTypesByDataGrouping(Factory, Core.Constants.CountryCodes.Brazil));
			return result;
		});

		public void AddProfile(TariffProfile profile)
		{
			if (Profiles.Count == 0)
			{
				CSI_Procedure = profile.LegalCode;
				CSI_Code = profile.Regime;
				CSI_SubType = profile.TaxType;
				rateTypeDescription = null;
			}
			if (!Profiles.Any(a => a.PK == profile.PK))
			{
				Profiles.Add(profile);
			}
		}

		public List<TariffProfile> Profiles { get; private set; } = new();

		ZString IsMandatoryDescription => Res.GetString("D1DCB595-C2E2-4F75-A622-17D7D37BE2F2", "Normal");

		ZString IsNotMandatoryDescription => Res.GetString("6D6429BD-BCB0-47FD-982A-223BE0520C50", "Optional");
	}
}
