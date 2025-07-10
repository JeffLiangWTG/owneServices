using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class GAApprovalLookups : Customs.Business.CusSupportingInfoLookups
	{
		public GAApprovalLookups(GAApproval parent)
			: base(parent)
		{
		}

		public GAApproval GAApproval
		{
			get { return Parent; }
		}

		protected new GAApproval Parent
		{
			get { return (GAApproval)base.Parent; }
		}

		bool IsImport => Parent.Parent?.IsImport ?? false;

		/// <summary>
		/// DO NOT USE AS A LIST
		/// </summary>
		public override CodeDescriptionPairList ProcedureList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				if (!Parent.CSI_Procedure.IsEmpty)
				{
					result.AddPair(Parent.CSI_Procedure, Parent.OGARegulationCategory?.ZZD_Description ?? ZString.Empty);
				}

				return result;
			}
		}

		public ZZRefCusCodeListCombinedCollection OGARegulationCategoryList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.OGARegulationCategory, Parent.Parent.DeclarationDate);

		public override CodeDescriptionPairList SubTypeList
		{
			get
			{
				CodeDescriptionPairList result = null;
				if (IsImport)
				{
					result = Factory.GetCachedValue<CommodityUsageCodeList>();
				}
				else
				{
					result = Factory.GetCachedValue<RequirementTypeCodeList>();
				}
				return result;
			}
		}
		public override ICollection CodeList
		{
			get
			{
				ICollection result = null;
				if (IsImport)
				{
					result = Factory.GetCachedValue<ImportRequirementTypeCodeList>();
				}
				else
				{
					result = Factory.GetCachedValue<RequirementDocumentTypeCodeList>();
				}
				return result;
			}
		}

		public ZZRefCusCodeListCombinedCollection NonGAReasonTypeList
		{
			get
			{
				var codeType = IsImport ? Constants.ZZ.NKCodeType.INGAR : Constants.ZZ.NKCodeType.ENGAR;
				return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory,
										new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, SQLComparisonOperator.StartsWith, Parent.CSI_Procedure + Parent.CSI_SubType),
										Core.Constants.CountryCodes.KoreaSouth,
										new ZString[] { codeType },
										Parent.Parent.DeclarationDate,
										null);
			}
		}
	}
}
