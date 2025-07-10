using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class CusFiscalReferenceProvider : EU.Business.Declaration.CusFiscalReferenceProvider
	{
		protected CusFiscalReferenceProvider(ZString dataGroupingCode) : base(dataGroupingCode)
		{
		}

		protected override EU.Business.Declaration.CusFiscalReferenceLookups GetNewLookupsCore(EU.Business.Declaration.CusFiscalReference reference)
		{
			return new CusFiscalReferenceLookups(reference);
		}

		protected override EU.Business.Declaration.CusFiscalReferenceValidation GetNewValidationCore(EU.Business.Declaration.CusFiscalReference reference)
		{
			return new CusFiscalReferenceValidation(reference);
		}

		protected override void RecalculateReferenceIfNeededCore(EU.Business.Declaration.CusFiscalReference reference)
		{
			if (!reference.CFR_OA_Owner.IsEmpty)
			{
				var factory = reference.Factory;
				var ownerAddress = factory.Load<OrgAddress>(reference.CFR_OA_Owner);
				if (ownerAddress != null)
				{
					var organisation = factory.Load<OrgHeader>(ownerAddress.OA_OH);
					if (organisation != null)
					{
						switch (reference.CFR_Code)
						{
							case FiscalReferenceCodeList.Codes.FR1:
							case FiscalReferenceCodeList.Codes.FR3:
								reference.CFR_Reference = organisation.GetVATRegistrationNumberWithCountryCodePrefix(Core.Constants.CountryCodes.Germany);
								break;
							case FiscalReferenceCodeList.Codes.FR2:
								reference.CFR_Reference = organisation.GetVATRegistrationNumberWithCountryCodePrefix(ownerAddress.OA_RN_NKCountryCode);
								break;
						}
					}
				}
			}
		}

		public override void RecalculateOwnerIfNeeded(EU.Business.Declaration.CusFiscalReference reference)
		{
			if (reference.CFR_Code != FiscalReferenceCodeList.Codes.FR5)
			{
				var entryInstruction = reference.Factory.Load<CusEntryInstruction>(reference.CFR_ParentID);
				if (entryInstruction != null && !entryInstruction.IsDeleted)
				{
					var declaration = entryInstruction.JobDeclaration;
					if (declaration != null)
					{
						switch (reference.CFR_Code)
						{
							case FiscalReferenceCodeList.Codes.FR1:
								reference.CFR_OA_Owner = declaration.JE_OA_DeclarantAddress;
								break;
							case FiscalReferenceCodeList.Codes.FR2:
								reference.CFR_OA_Owner = declaration.AcquirerDocAddress.E2_OA_Address;
								break;
							case FiscalReferenceCodeList.Codes.FR3:
								reference.CFR_OA_Owner = declaration.JE_OA_Representative;
								break;
						}
					}
				}
			}
		}

		public override bool ReferenceIsReadOnly(EU.Business.Declaration.CusFiscalReference reference) => reference.CFR_Code != FiscalReferenceCodeList.Codes.FR5;

		public override bool OwnerIsReadOnly(EU.Business.Declaration.CusFiscalReference reference) => reference.CFR_Code != FiscalReferenceCodeList.Codes.FR1 && reference.CFR_Code != FiscalReferenceCodeList.Codes.FR2 && reference.CFR_Code != FiscalReferenceCodeList.Codes.FR3;
	}
}
