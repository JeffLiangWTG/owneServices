using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class ESGuaranteeLookups : EU.Business.Declaration.GuaranteeForDeclarationLookups
	{
		public ESGuaranteeLookups(ESGuarantee guarantee)
		: base(guarantee)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public const string Declarant = "Declarant";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public const string Importer = "Importer";

		public override CodeDescriptionPairList HolderIdentificationList => CommonLookups.EORILookup(Parent?.Declaration, new string[] { Declarant, Importer }, (org) => GetEoriOrNif(org));

		public ZString GetEoriOrNif(OrgHeader org)
		{
			var code = org.GetEuIdentificationNumber(EconomicGroupList.Codes.EuropeanUnion);
			if (code.IsEmpty)
			{
				code = org.GetEuIdentificationNumber();
			}
			return code.IsEmpty ? org.GetRegoCodeOfThisOrg(OrgCusCode.SpainCodeTypes.NIF) : code;
		}

		protected override CustomsOfficeCodeCollection OfficeCodeListCore => EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRoles(Factory, Array.Empty<ZString>());

		protected new ESGuarantee Parent => (ESGuarantee)base.Parent;
	}
}
