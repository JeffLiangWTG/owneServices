using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSAddInfoJobComInvoiceLineValidation : EUEMCSAddInfoValidation
	{
		public EMCSAddInfoJobComInvoiceLineValidation(EMCSAddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected new EMCSAddInfoJobComInvoiceLine Parent => (EMCSAddInfoJobComInvoiceLine)base.Parent;

		protected EMCSJobComInvoiceLine ParentInvoiceLine => Parent.Parent;

		protected override void CheckZG_GrowingZone()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.ZG_GrowingZoneInfo);
		}

		protected override void CheckZG_WineCategory()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.ZG_WineCategoryInfo);
		}

		protected override void CheckZG_ExciseProductCode()
		{
			var info = Parent.ZG_ExciseProductCodeInfo;
			var invoiceLine = ParentInvoiceLine;
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(info);

			var exciseProductCode = Parent.ZG_ExciseProductCode;
			if (invoiceLine.JI_Tariff == "10000000" && exciseProductCode != "S500")
			{
				info.AddMessageError(Res.GetString("CF50CC9E-65D2-43E1-A577-63AE8802A06B", "{0} must be S500 when tariff is {1}.", invoiceLine.ZG_ExciseProductCodeInfo.HumanReadableName, invoiceLine.JI_Tariff));
			}

			var declaration = invoiceLine.Declaration;
			if (declaration != null)
			{
				if (!invoiceLine.HasExciseProductAttribute(ExciseProductCodeAttribute.ExciseProductCategory, "E"))
				{
					if (declaration.JE_MessageSubType == EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown)
					{
						info.AddMessageError(Res.GetString("73525B52-27ED-4885-BA2C-6A0D6F306F21", "{0} must start with 'E' (Energy Products) when {1} is {2}.", invoiceLine.ZG_ExciseProductCodeInfo.HumanReadableName, declaration.JE_MessageSubTypeInfo.HumanReadableName, EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown));
					}

					if (declaration.ZG_GuarantorType == EMCSGuarantorTypeList.Codes.GuarantorNotRequiredQualifyingMemberStateToEuMovements)
					{
						info.AddMessageError(Res.GetString("E99A709C-6729-4B42-931F-105AA1552DA5", "{0} must start with 'E' (Energy Products) when {1} is {2}.", invoiceLine.ZG_ExciseProductCodeInfo.HumanReadableName, declaration.ZG_GuarantorTypeInfo.HumanReadableName, EMCSGuarantorTypeList.Codes.GuarantorNotRequiredQualifyingMemberStateToEuMovements));
					}
				}
			}

			CheckExciseProductCodeIsMappedForCorrectTariff();
		}

		protected virtual void CheckExciseProductCodeIsMappedForCorrectTariff()
		{
			var exciseProductCode = Parent.ZG_ExciseProductCode;
			var tariff = ParentInvoiceLine.JI_Tariff;
			if (!tariff.IsEmpty && !exciseProductCode.IsEmpty && !ParentInvoiceLine.HasExciseProductAttribute(ExciseProductCodeAttribute.RelTrf, tariff))
			{
				var tariffCodes = ParentInvoiceLine.GetExciseProductAttributeValues(ExciseProductCodeAttribute.RelTrf).OrderBy(x => x);
				Parent.ZG_ExciseProductCodeInfo.AddWarning(Res.GetString("0F38A6B9-99CF-44C6-BB54-66D00E5C440D", "The Excise Product code {0} is only mapped for these tariffs.\r\n{1}", exciseProductCode, string.Join(", ", tariffCodes)));
			}
		}

		protected override void CheckZG_Density()
		{
			if (ParentInvoiceLine.HasExciseProductAttribute(ExciseProductCodeAttribute.Density, Customs.Business.YesNoList.Codes.Yes))
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.ZG_DensityInfo);
			}
		}

		protected override void CheckZG_AlcoholicStrength()
		{
			MandatoryValidation.MessageErrorIfIsNegative(Parent.ZG_AlcoholicStrengthInfo);

			if (ParentInvoiceLine.HasExciseProductAttribute(ExciseProductCodeAttribute.AlcoholicStrength, Customs.Business.YesNoList.Codes.Yes))
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.ZG_AlcoholicStrengthInfo);

				var alcoholicStrength = Parent.ZG_AlcoholicStrength;
				if (alcoholicStrength > 0 && !alcoholicStrength.IsInRange(0.5m, 100m))
				{
					Parent.ZG_AlcoholicStrengthInfo.AddMessageError(Res.GetString("e1009155-9dcf-40f3-8051-bfa208a44590", "Must be greater than or equal to 0.5 and less than or equal to 100."));
				}
			}
		}

		protected override void CheckZG_DegreePlato()
		{
			if (ParentInvoiceLine.HasExciseProductAttribute(ExciseProductCodeAttribute.DegreePlato, Customs.Business.YesNoList.Codes.Yes))
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.ZG_DegreePlatoInfo);
			}
		}

		protected override void CheckZG_WineCountryOrigin()
		{
			base.CheckZG_WineCountryOrigin();
			if (ParentInvoiceLine.IsWine && ParentInvoiceLine.ZG_WineCategory == EMCSWineCategoryList.Codes.ImportedWine)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ZG_WineCountryOriginInfo);
			}
		}

		protected override void CheckZG_IsMainPack()
		{
			base.CheckZG_IsMainPack();

			if (Parent.ZG_IsMainPack)
			{
				var declaration = Parent.Declaration;
				var currentMainPacks = ParentInvoiceLine.EMCSPackagePivots.Cast<NonPersistentPackagePivot>().Where(c => c.IsForInvoiceLine).Select(c => c.Package.PK.ToString());
				var existsOtherLineLinkedToThisPackageIsMainPack = declaration.InvoiceLines.Cast<EMCSJobComInvoiceLine>()
					.FirstOrDefault(line => line.ZG_IsMainPack && line.PK != ParentInvoiceLine.PK && line.EMCSPackagePivots.Cast<NonPersistentPackagePivot>().Any(c => c.IsForInvoiceLine && currentMainPacks.Contains(c.Package?.PK.ToString())));
				if (existsOtherLineLinkedToThisPackageIsMainPack != null)
				{
					Parent.ZG_IsMainPackInfo.AddMessageError(Res.GetString("C6F17B6B-6C4E-4748-8A1B-908665C4007E", "Line #{0} is already marked as Main Pack for one or more selected Packages on this Line.", existsOtherLineLinkedToThisPackageIsMainPack.JI_LineNo));
				}
			}
		}
	}
}
