using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class VATDeferStrategy
	{
		public VATDeferStrategy(JobDeclaration declaration)
		{
			Declaration = declaration;
		}

		protected readonly JobDeclaration Declaration;

		public virtual void OnOrganisationChanged()
		{
			DefaultPaymentMethod();
			DefaultDefermentAccountNumber();
			DefaultVATDeferType();
			DefaultVATDeferNumber();
		}

		public virtual void OnPaymentMethodChanged()
		{
			DefaultDefermentAccountNumber();
		}

		public virtual void OnVATDeferTypeChanged()
		{
			DefaultVATDeferNumber();
		}

		protected virtual void DefaultPaymentMethod()
		{
			var source = GetSourceForDeferment(Declaration.JE_PaymentMethod);
			if (source != null)
			{
				var orgImpAddInfo = EUOrgImpAddInfo.Get(source, Declaration.CountryCode);
				if (orgImpAddInfo != null)
				{
					orgImpAddInfo.Deserialise();
					var paymentMethod = orgImpAddInfo.ZO_OtherDeferType;
					if (!paymentMethod.IsEmpty)
					{
						Declaration.JE_PaymentMethod = paymentMethod;
					}
				}
			}
		}

		protected virtual void DefaultDefermentAccountNumber()
		{
			var paymentMethod = Declaration.JE_PaymentMethod;
			if (paymentMethod.IsEmpty)
			{
				Declaration.JE_DefermentAccountNumberInfo.ClearValue();
			}
			else if (Declaration.Lookups.PaymentPartyList.ContainsCode(paymentMethod))
			{
				var source = GetSourceForDeferment(paymentMethod);
				if (source != null)
				{
					var defermentAccountNumber = GetDefermentAccountNumber(source);
					if (!defermentAccountNumber.IsEmpty)
					{
						var defermentAccountNumberMaxLength = Declaration.GetPossiblyCustomPropertyMaxLength(JobDeclaration.Schema.JE_DefermentAccountNumber);
						Declaration.JE_DefermentAccountNumber = defermentAccountNumber.Left(defermentAccountNumberMaxLength);
					}
				}
			}
		}

		public OrgHeader GetPaymentMethodSourceWrapper() => GetSourceForDeferment(Declaration.JE_PaymentMethod);

		protected virtual ZString GetDefermentAccountNumber(OrgHeader header)
		{
			return header.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, Declaration.CountryCode);
		}

		protected virtual void DefaultVATDeferType()
		{
		}

		protected virtual void DefaultVATDeferNumber()
		{
			if (Declaration.ZG_VATDeferType.IsEmpty)
			{
				Declaration.ZG_VATDeferNumberInfo.ClearValue();
			}
			else if (Declaration.AddInfoLookups.DeferTypeList.ContainsCode(Declaration.ZG_VATDeferType))
			{
				var source = GetSourceForVAT(Declaration.ZG_VATDeferType);
				if (source != null)
				{
					var vatDeferNumber = GetVATDeferNumber(source);
					if (!vatDeferNumber.IsEmpty)
					{
						var vatDeferNumberMaxLength = Declaration.GetPossiblyCustomPropertyMaxLength(JobDeclaration.Schema.ZG_VATDeferNumber);
						Declaration.ZG_VATDeferNumber = vatDeferNumber.Left(vatDeferNumberMaxLength);
					}
				}
			}
		}

		protected virtual ZString GetVATDeferNumber(OrgHeader header)
		{
			return header.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, Declaration.CountryCode);
		}

		protected virtual OrgHeader GetSourceForVAT(ZString deferType) => GetSource(deferType);

		protected virtual OrgHeader GetSourceForDeferment(ZString deferType) => GetSource(deferType);

		OrgHeader GetSource(ZString deferType)
		{
			if (deferType == DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14)
			{
				return Declaration.Declarant?.Header;
			}

			return Declaration.Importer;
		}
	}
}
