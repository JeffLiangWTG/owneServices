using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class CusClassPartPivot : BaseCusClassPartPivot,
		ILineOrProduct
	{
		public CusClassPartPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public CusKRClassification KRClassification
		{
			get
			{
				if (krClassification == null)
				{
					krClassification = Factory.LoadTop1<CusKRClassification>(new ZQuery(CusKRClassificationSchema.CKR_CI, PK));
					RegisterEditableChildObject(krClassification);
				}
				return krClassification;
			}
		}
		CusKRClassification krClassification;

		[ChildEditable(true)]
		public HSExtensionCodeCollection HSExtensionCodeCollection
		{
			get
			{
				if (hsExtensionCodeCollection == null)
				{
					hsExtensionCodeCollection = new HSExtensionCodeCollection(this);
					hsExtensionCodeCollection.Load();
					RegisterEditableChildObject(hsExtensionCodeCollection);
				}
				return hsExtensionCodeCollection;
			}
		}
		HSExtensionCodeCollection hsExtensionCodeCollection;

		[ChildEditable(true)]
		public GAApprovalCollection GAApprovalDataCollection
		{
			get
			{
				if (gaApprovalDataCollection == null)
				{
					gaApprovalDataCollection = new GAApprovalCollection(this);
					gaApprovalDataCollection.Load();
					RegisterEditableChildObject(gaApprovalDataCollection);
				}
				return gaApprovalDataCollection;
			}
		}
		GAApprovalCollection gaApprovalDataCollection;

		[ChildEditable(true)]
		public NonGADetailCollection NonGADetailCollection
		{
			get
			{
				if (nonGADetailCollection == null)
				{
					nonGADetailCollection = new NonGADetailCollection(this);
					nonGADetailCollection.Load();
					RegisterEditableChildObject(nonGADetailCollection);
				}
				return nonGADetailCollection;
			}
		}
		NonGADetailCollection nonGADetailCollection;

		public bool IsExport => CI_ChildType == ClassificationTypeList.Codes.HTE;

		public bool IsImport => CI_ChildType == ClassificationTypeList.Codes.HTI;

		ZDateTime ILineOrProduct.DeclarationDate => ZDateTime.Today;
		bool ILineOrProduct.IsExport => IsExport;
		bool ILineOrProduct.IsImport => IsImport;
		bool ILineOrProduct.IsIssueDateRelevant => false;
		bool ILineOrProduct.IsReferenceNumberRelevant => false;
		ZString ILineOrProduct.Tariff => CI_TariffNum;
		GAApprovalCollection ILineOrProduct.GAApprovalDataCollection => GAApprovalDataCollection;
		HSExtensionCodeCollection ILineOrProduct.HSExtensionCodeCollection => HSExtensionCodeCollection;
		bool ILineOrProduct.IsValidationEnabled => true;
		TariffView ILineOrProduct.UniversalTariff => UniversalTariff;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			if (krClassification == null)
			{
				krClassification = Factory.New<CusKRClassification>();
				krClassification.CKR_CI = PK;
			}
		}

		public override void Delete()
		{
			base.Delete();
			KRClassification.Delete();
		}

		public override ZString CI_TariffNum
		{
			get => base.CI_TariffNum;
			set
			{
				var oldValue = CI_TariffNum;
				base.CI_TariffNum = value;
				if (oldValue != CI_TariffNum && !IsCopying)
				{
					RenewGAApprovalDataCollection();
					RenewHSExtensionCodeCollectionByTariff();
				}
			}
		}

		public override ZString CI_ChildType
		{
			get => base.CI_ChildType;
			set
			{
				var oldValue = CI_ChildType;
				base.CI_ChildType = value;
				if (oldValue != CI_ChildType && !IsCopying && UniversalTariff != null)
				{
					RenewGAApprovalDataCollection();
					RenewHSExtensionCodeCollectionByTariff();
				}
			}
		}

		public void RenewGAApprovalDataCollection()
		{
			GAApprovalDataCollection.UpdateExportConditions(UniversalTariff, ZDateTime.Today);
		}

		public void RenewHSExtensionCodeCollectionByTariff()
		{
			HSExtensionCodeCollection.UpdateMainAdditionalCode(UniversalTariff);
		}
	}
}
