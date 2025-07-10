using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class CognosAccGLAccountDescriptor : AccGLAccountDescriptor
	{
		public CognosAccGLAccountDescriptor(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region ExtraInfo

		public bool HasExtraInfoBeenCreated
		{
			get { return fExtraInfo != null || LoadExtraInfo() != null; }
		}

		[ChildEditable(false)]
		public CognosAccGLAccountDescriptorExtraInfoCollection ExtraInfoForBinding
		{
			get
			{
				if (fExtraInfoForBinding == null)
				{
					fExtraInfoForBinding = new CognosAccGLAccountDescriptorExtraInfoCollection(Factory);
					RegisterEditableChildObject(fExtraInfoForBinding);
					AddToCollectionIfEmpty(ExtraInfoForBinding, ExtraInfo);
				}
				return fExtraInfoForBinding;
			}
		}

		public CognosAccGLAccountDescriptorExtraInfo ExtraInfo
		{
			get
			{
				if (fExtraInfo == null)
				{
					fExtraInfo = LoadOrCreateNewExtraInfo();
					fExtraInfo.HasChanges = false;
				}
				return fExtraInfo;
			}
		}

		CognosAccGLAccountDescriptorExtraInfo LoadOrCreateNewExtraInfo()
		{
			CognosAccGLAccountDescriptorExtraInfo result = LoadExtraInfo();

			if (result == null)
			{
				result = Factory.New<CognosAccGLAccountDescriptorExtraInfo>();
				result.T9_AJ = PK;
			}

			return result;
		}

		CognosAccGLAccountDescriptorExtraInfo LoadExtraInfo()
		{
			ZQuery filter = new ZQuery(ClientCognosAccGLAccountDescriptorExtraInfoSchema.T9_AJ, PK);
			return Factory.LoadTop1<CognosAccGLAccountDescriptorExtraInfo>(filter);
		}

		CognosAccGLAccountDescriptorExtraInfoCollection fExtraInfoForBinding;
		CognosAccGLAccountDescriptorExtraInfo fExtraInfo;

		#endregion

		#region Grouping Flags

		public bool HasGroupingFlagsBeenCreated
		{
			get { return fGroupingFlags != null || LoadGroupingFlags() != null; }
		}

		[ChildEditable(false)]
		public CognosGroupingFlagsCollection GroupingFlagsForBinding
		{
			get
			{
				if (fGroupingFlagsForBinding == null)
				{
					fGroupingFlagsForBinding = new CognosGroupingFlagsCollection(Factory);
					RegisterEditableChildObject(fGroupingFlagsForBinding);
					AddToCollectionIfEmpty(fGroupingFlagsForBinding, GroupingFlags);
				}
				return fGroupingFlagsForBinding;
			}
		}

		public CognosGroupingFlags GroupingFlags
		{
			get
			{
				if (fGroupingFlags == null)
				{
					fGroupingFlags = LoadOrCreateNewGroupingFlags();
					fGroupingFlags.HasChanges = false;
				}
				return fGroupingFlags;
			}
		}

		CognosGroupingFlags LoadOrCreateNewGroupingFlags()
		{
			CognosGroupingFlags result = LoadGroupingFlags();

			if (result == null)
			{
				result = Factory.New<CognosGroupingFlags>();
				result.T4_AJ = PK;
			}

			return result;
		}

		CognosGroupingFlags LoadGroupingFlags()
		{
			ZQuery filter = new ZQuery(ClientCognosGroupingFlagsSchema.T4_AJ, PK);
			return Factory.LoadTop1<CognosGroupingFlags>(filter);
		}

		CognosGroupingFlagsCollection fGroupingFlagsForBinding;
		CognosGroupingFlags fGroupingFlags;

		#endregion

		#region New Properties

		public ZBool IsCognosSubClassificationAccount
		{
			get { return IsCognosGLLanguage && AJ_ReportCategory == CognosSubClassificationAccountType; }
		}

		public ZPropertyInfo IsCognosSubClassificationAccountInfo
		{
			get { return GetZPropertyInfo(nameof(IsCognosSubClassificationAccount)); }
		}

		public bool RequiresCognosConsolidationAccount
		{
			get { return IsCognosGLLanguage && AJ_LocalAccountNumber.Contains('.'); }
		}

		public ZBool IsCognosGLLanguage
		{
			get { return (AJ_Language == Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger); }
		}

		public ZPropertyInfo IsCognosGLLanguageInfo
		{
			get { return GetZPropertyInfo(nameof(IsCognosGLLanguage)); }
		}

		#endregion

		#region Overrides

		protected override bool ParentGLHeaderPK_ReadOnly
		{
			get { return IsCognosSubClassificationAccount || base.ParentGLHeaderPK_ReadOnly; }
		}

		protected override bool AJ_AJ_AlternativeNum_ReadOnly
		{
			get { return IsCognosSubClassificationAccount || base.AJ_AJ_AlternativeNum_ReadOnly; }
		}

		protected override bool AJ_AJ_CarriedForwardAccount_ReadOnly
		{
			get { return IsCognosSubClassificationAccount || base.AJ_AJ_CarriedForwardAccount_ReadOnly; }
		}

		protected override bool AJ_AJ_ConsolidationNum_ReadOnly
		{
			get { return IsCognosSubClassificationAccount || base.AJ_AJ_ConsolidationNum_ReadOnly; }
		}

		protected override bool AJ_AJ_HeaderDependsOnTotal_ReadOnly
		{
			get { return IsCognosSubClassificationAccount || base.AJ_AJ_HeaderDependsOnTotal_ReadOnly; }
		}

		protected override bool AJ_AJ_PercentNum_ReadOnly
		{
			get { return IsCognosSubClassificationAccount || base.AJ_AJ_PercentNum_ReadOnly; }
		}

		protected bool AJ_PrintSequence_ReadOnly
		{
			get { return IsCognosSubClassificationAccount; }
		}

		protected override bool AJ_TotalLevel_ReadOnly
		{
			get { return IsCognosSubClassificationAccount || base.AJ_TotalLevel_ReadOnly; }
		}

		public override CodeDescriptionPairList AJ_GLAccountType_List
		{
			get
			{
				CodeDescriptionPairList result = base.AJ_GLAccountType_List;

				if (!IsCognosGLLanguage)
				{
					result.RemoveCode(CognosSubClassificationAccountType);
				}
				else if (!result.ContainsCode(CognosSubClassificationAccountType))
				{
					result.AddPair(CognosSubClassificationAccountType, CognosSubClassificationAccountTypeDescription);
				}

				return result;
			}
		}

		public override void Delete()
		{
			if (HasExtraInfoBeenCreated)
			{
				ExtraInfo.Delete();
			}
			if (HasGroupingFlagsBeenCreated)
			{
				GroupingFlags.Delete();
			}
			base.Delete();
		}

		protected override void OnFactorySaving()
		{
			if (fExtraInfo != null && !ExtraInfo.IsInDatabase && !ExtraInfo.HasChanges)
			{
				ExtraInfo.Delete();
				fExtraInfo = null;
			}

			if (fGroupingFlags != null && !GroupingFlags.IsInDatabase && !GroupingFlags.HasChanges)
			{
				GroupingFlags.Delete();
				fGroupingFlags = null;
			}

			base.OnFactorySaving();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			AddToCollectionIfEmpty(ExtraInfoForBinding, ExtraInfo);
			AddToCollectionIfEmpty(GroupingFlagsForBinding, GroupingFlags);
		}

		public const string CognosSubClassificationAccountType = "CCC";
		public const string CognosSubClassificationAccountTypeDescription = "Cognos Sub-Classification";

		void AddToCollectionIfEmpty(BusinessObjectCollection collection, BusinessObject bizO)
		{
			if (collection.Count == 0)
			{
				collection.Add(bizO);
			}
		}

		#endregion

		#region Validation

		protected override AccGLAccountDescriptorValidation GetNewValidation()
		{
			return new CognosAccGLAccountDescriptorValidation(this);
		}

		#endregion
	}
}

#region ExtraInfo
#endregion
#region GroupingFlags
#endregion
