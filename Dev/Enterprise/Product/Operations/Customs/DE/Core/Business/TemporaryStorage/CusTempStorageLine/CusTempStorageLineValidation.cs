using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CusTempStorageLineValidation : EU.Business.CusTempStorage.CusTempStorageLineValidation
	{
		public CusTempStorageLineValidation(AutoCusTempStorageLine parent) : base(parent)
		{
		}

		protected new CusTempStorageLine Parent => (CusTempStorageLine)base.Parent;

		public static string OwnerReferenceNoHumanReadableName => Res.GetString("02f4b447-02bc-4acb-a339-8caf55279b2f", "Owner Reference No");
		public static string ReferenceHumanReadableName => Res.GetString("ae488f34-7d3c-47f2-aa7e-5ae4ffa7b2cb", "Reference");

		#region Custodian Eori

		protected override void CheckTSL_CustodianIdentifier()
		{
			base.CheckTSL_CustodianIdentifier();
			CheckTSL_CustodianIdentifierMandatory();

			if (!Parent.TSL_CustodianIdentifier.IsEmpty)
			{
				CheckTSL_CustodianIdentifierFormat();
			}
		}

		protected virtual void CheckTSL_CustodianIdentifierMandatory()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_CustodianIdentifierInfo);
		}

		protected virtual void CheckTSL_CustodianIdentifierFormat()
		{
			CheckEoriMismatchWithOrganisationRegoNo(Parent.Custodian?.Header, Parent.TSL_CustodianIdentifierInfo, Res.GetString("f6d02af5-d595-4d65-9a49-988bef3501d0", "Custodian"));
		}

		#endregion

		#region Custodian Branch

		protected override void CheckTSL_CustodianIdentifierBranchNo()
		{
			base.CheckTSL_CustodianIdentifierBranchNo();
			CheckTSL_CustodianIdentifierBranchNoMandatory();
			if (!Parent.TSL_CustodianIdentifierBranchNo.IsEmpty)
			{
				CheckTSL_CustodianIdentifierBranchNoFormat();
			}
		}

		protected virtual void CheckTSL_CustodianIdentifierBranchNoMandatory()
		{
			if (Parent.TSL_CustodianIdentifierBranchNo.IsEmpty && !Parent.TSL_CustodianIdentifier.IsEmpty)
			{
				Parent.TSL_CustodianIdentifierBranchNoInfo.AddWarning(MissingBranchMessage);
			}
		}

		protected virtual void CheckTSL_CustodianIdentifierBranchNoFormat()
		{
			GermanValidationHelper.ValidateEBS(Parent.TSL_CustodianIdentifierBranchNoInfo);
			CheckBranchNumberMismatchWithOrganisationRegoNo(Parent.Custodian, Parent.TSL_CustodianIdentifierBranchNoInfo);
		}

		#endregion

		#region Disposal Entitled Trader Eori

		protected override void CheckTSL_GoodsOwnerIdentifier()
		{
			base.CheckTSL_GoodsOwnerIdentifier();
			CheckTSL_GoodsOwnerIdentifierMandatory();
			if (!Parent.TSL_GoodsOwnerIdentifier.IsEmpty)
			{
				CheckTSL_GoodsOwnerIdentifierFormat();
			}
		}

		protected virtual void CheckTSL_GoodsOwnerIdentifierMandatory()
		{
			if (Parent.GoodsOwner != null)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_GoodsOwnerIdentifierInfo);
			}
		}

		protected virtual void CheckTSL_GoodsOwnerIdentifierFormat()
		{
			CheckEoriMismatchWithOrganisationRegoNo(Parent.GoodsOwner?.Header, Parent.TSL_GoodsOwnerIdentifierInfo, Res.GetString("fd0723a9-9158-46db-a4cf-09a4bf4228a3", "Disposal Entitled Trader"));
		}

		#endregion

		#region Disposal Entitled Trader Branch

		protected override void CheckTSL_GoodsOwnerIdentifierBranchNo()
		{
			base.CheckTSL_GoodsOwnerIdentifierBranchNo();
			CheckTSL_GoodsOwnerIdentifierBranchNoMandatory();
			if (!Parent.TSL_GoodsOwnerIdentifierBranchNo.IsEmpty)
			{
				CheckTSL_GoodsOwnerIdentifierBranchNoFormat();
			}
		}

		protected virtual void CheckTSL_GoodsOwnerIdentifierBranchNoMandatory()
		{
			if (Parent.TSL_GoodsOwnerIdentifierBranchNo.IsEmpty && !Parent.TSL_GoodsOwnerIdentifier.IsEmpty)
			{
				Parent.TSL_GoodsOwnerIdentifierBranchNoInfo.AddWarning(MissingBranchMessage);
			}
		}

		protected virtual void CheckTSL_GoodsOwnerIdentifierBranchNoFormat()
		{
			GermanValidationHelper.ValidateEBS(Parent.TSL_GoodsOwnerIdentifierBranchNoInfo);
			CheckBranchNumberMismatchWithOrganisationRegoNo(Parent.GoodsOwner, Parent.TSL_GoodsOwnerIdentifierBranchNoInfo);
		}

		#endregion

		protected override void CheckTSL_LocationOfGoods()
		{
			base.CheckTSL_LocationOfGoods();
			var locationOfGoods = Parent.TSL_LocationOfGoods;
			if (!locationOfGoods.IsEmpty)
			{
				if (ZInt.TryParse(locationOfGoods, out var locationOfGoodsZInt))
				{
					if (!locationOfGoodsZInt.IsInRange(1, 99))
					{
						Parent.TSL_LocationOfGoodsInfo.AddMessageError(Res.GetString("48A4BD99-D36E-40BB-94D7-B745D949F3FE", "Goods Location must be between 1 and 99"));
					}
				}
				else
				{
					Parent.TSL_LocationOfGoodsInfo.AddMessageError(Res.GetString("26EA34A1-555D-4847-922F-97DD51157B8F", "Goods Location must be numeric and between 1 and 99"));
				}
			}
			CheckTSL_LocationOfGoodsListValidation();
		}

		protected virtual void CheckTSL_LocationOfGoodsListValidation()
		{
			if (Parent.Custodian != null)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.TSL_LocationOfGoodsInfo);
			}
		}

		protected override void CheckTSL_GoodsType()
		{
			base.CheckTSL_GoodsType();
			ListValidation.MessageErrorIfInvalidCode(Parent.TSL_GoodsTypeInfo);
		}

		protected override void CheckTSL_OwnerReferenceType()
		{
			base.CheckTSL_OwnerReferenceType();
			ListValidation.MessageErrorIfInvalidCode(Parent.TSL_OwnerReferenceTypeInfo);
		}

		protected override void CheckTSL_PackageType()
		{
			base.CheckTSL_PackageType();
			ListValidation.MessageErrorIfInvalidCode(Parent.TSL_PackageTypeInfo);
		}

		protected override void CheckTSL_RN_NKDepartureCountry()
		{
			base.CheckTSL_RN_NKDepartureCountry();
			ListValidation.MessageErrorIfInvalidCode(Parent.TSL_RN_NKDepartureCountryInfo);
		}

		protected void CheckSinglePackageRequirements()
		{
			if (Parent.TSL_PackageQty != 1)
			{
				if (Parent.IsSingleCountPackgeType)
				{
					Parent.TSL_PackageQtyInfo.AddMessageError(Res.GetString("A881FA69-4DC1-4124-A269-0B52FA5170BD", "Package type of '{0}' requires package count to be 1.", Parent.TSL_PackageType));
				}
				else if (Parent.IsULD)
				{
					Parent.TSL_PackageQtyInfo.AddMessageError(Res.GetString("F23C8F0D-9B39-4855-9563-B2659EE35586", "Owner reference type of 'ULD' requires package count to be 1."));
				}
			}
		}

		protected bool IsUniqueLineNoAndATBNoCombination(Func<CusTempStorageLine, bool> lineNoAndATBNoComparator) => !Parent.Dec?.CusTempStorageLines.Cast<CusTempStorageLine>().Any(x => x.PK != Parent.PK && lineNoAndATBNoComparator(x)) ?? true;

		protected void CheckPackageQtyIsBetween1And99999()
		{
			if (!Parent.TSL_PackageQty.IsInRange(1, 99999))
			{
				Parent.TSL_PackageQtyInfo.AddMessageError(Res.GetString("AB1C47A4-056A-47F8-97EA-BF2E56A1F408", "Package count must be between 1 and 99999."));
			}
		}

		protected void CheckLineNoIsUnique()
		{
			if (!IsUniqueLineNoAndATBNoCombination((x) => x.TSL_LineNo == Parent.TSL_LineNo))
			{
				var ownerReferenceNumber = Parent.Dec?.FormattedOwnerReferenceNumber ?? ZString.Empty;
				Parent.TSL_LineNoInfo.AddMessageError(Res.GetString("A7918AAD-DD30-4E51-B88E-8C8E7B227897", "The combination of Line No. {0} and ATB No. {1} already exists for this declaration", Parent.TSL_LineNo, ownerReferenceNumber));
			}
		}

		void CheckBranchNumberMismatchWithOrganisationRegoNo(OrgAddress orgAddress, ZPropertyInfo propertyInfo)
		{
			if (orgAddress != null)
			{
				var orgBranchCode = orgAddress.CustomsCodes.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, Core.Constants.CountryCodes.Germany);
				var storageLineBranchCode = (ZString)propertyInfo.Value;
				if (orgBranchCode.IsEmpty)
				{
					propertyInfo.AddWarning(Res.GetString("AB599D5F-C9D6-4C29-8ABA-ED5F4144D123",
						"No branch code (EBS) for DE exists in the organization registration numbers for premise address {0}",
						orgAddress.OA_Code));
				}
				else if (orgBranchCode != storageLineBranchCode)
				{
					propertyInfo.AddWarning(Res.GetString("13B3888E-733C-4C4A-ACAF-BD9567F5DD9F",
						"The entered branch code {0}, does not match the organization registration number {1} of type EBS entered for address {2}",
						storageLineBranchCode, orgBranchCode, orgAddress.OA_Code));
				}
			}
		}

		void CheckEoriMismatchWithOrganisationRegoNo(OrgHeader orgHeader, ZPropertyInfo propertyInfo, ZString entityMessage)
		{
			if (orgHeader != null)
			{
				var orgEori = orgHeader.GetEUEoriDetails();
				var storageLineEori = (ZString)propertyInfo.Value;
				if (orgEori.IsEmpty)
				{
					propertyInfo.AddWarning(Res.GetString("C01FEAD3-6206-450A-90BB-D5F3453A3253",
						"No EORI code (EOR) exists in the organization registration numbers for organization {0}",
						orgHeader.OH_Code));
				}
				else if (orgEori != storageLineEori)
				{
					propertyInfo.AddWarning(Res.GetString("DB00EA79-2575-426F-8D7E-82D69D99639D",
						"EORI {0} does not match organization registration number {1} from {2} {3}",
						storageLineEori, orgEori, entityMessage, orgHeader.OH_Code));
				}
			}
		}

		string MissingBranchMessage => Res.GetString("a2a50408-b2d0-462c-b37f-320bf3912ab9", "Branch should be captured if known.");
	}
}
