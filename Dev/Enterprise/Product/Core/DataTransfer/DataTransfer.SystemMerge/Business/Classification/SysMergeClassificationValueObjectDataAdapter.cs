using System;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Master = Enterprise.MasterFiles.Business;
using Res = Enterprise.DataTransfer.SystemMerge.Business.Res;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters
{
	public class SysMergeClassificationValueObjectDataAdapter : ValueObjectDataAdapter<BaseCusClassification, Xsd.CusClassification>
	{
		public SysMergeClassificationValueObjectDataAdapter()
		{ }

		public SysMergeClassificationValueObjectDataAdapter(Master.OrgSupplierPart product)
		{
			Product = product;
		}

		readonly Master.OrgSupplierPart Product;

		#region Implementation

		public override string RootCollectionElementName
		{
			get { return "CusClassifications"; }
		}

		public override string RootElementName
		{
			get { return "CusClassification"; }
		}

		public override XmlSchema Schema
		{
			get { return null; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return null; }
		}

		protected override BaseCusClassification NewBusinessObject(Xsd.CusClassification value, IValueObjectImportContext context)
		{
			var country = (value.RN_Code.IsEmpty) ?
					null :
					context.GetCountryByCodeThrowingErrorIfNotFound(context.Factory, value.RN_Code);

			var classificationPk = new ZGuid(value.PK);
			if (classificationPk.IsEmpty)
			{
				var classification = context.Factory.New<BaseCusClassification>();
				classification.Delete();
				return classification;
			}

			return !value.RN_Code.IsEmpty
				? (BaseCusClassification)context.Factory.New(new BaseCusClassificationTypeDecider().GetTypeForCountryCode(value.RN_Code), classificationPk.ToGuid())
				: context.Factory.NewWithPrimaryKey<BaseCusClassification>(classificationPk.ToGuid());
		}

		protected override void OnUserDeclinedImport(BaseCusClassification bizObj, Xsd.CusClassification value, IValueObjectImportContext context)
		{
			string message = Res.GetString("9AE332EE-0079-4521-9257-0BBFB73A1144", "Import of classification [({0}) - {1} - {2}] skipped. Reason: Classification already exists.", bizObj.PK.ToString(), bizObj.CC_LookupCode, bizObj.CC_Description) + "\r\n";
			context.Notify(new InfoNotification(message));
		}

		protected override bool ShouldUpdateExistingObject(BaseCusClassification bizObj, INotifications notifications)
		{
			return Product != null && ClassificationIsNotLinkToProduct(bizObj);
		}

		bool ClassificationIsNotLinkToProduct(BaseCusClassification classification)
		{
			var query = new ZQuery(CusClassPartPivotSchema.CI_RN_NKCountry, classification.CC_RN_NKCountryCode);
			query.AddToFilter(CusClassPartPivotSchema.CI_CC, classification.PK);
			query.AddToFilter(CusClassPartPivotSchema.CI_OP, Product.PK);

			var matchedPivots = classification.Factory.Load<BaseCusClassPartPivot>(query);

			return matchedPivots.Length == 0;
		}

		protected override BaseCusClassification FindBusinessObject(Xsd.CusClassification value, IValueObjectImportContext context)
		{
			var pkTofind = new ZGuid(value.PK);
			return context.Factory.Load<BaseCusClassification>(pkTofind);
		}

		#endregion

		#region Import

		protected override void ImportFromValueObjectCore(BaseCusClassification bizObj, Xsd.CusClassification value, IValueObjectImportContext context)
		{
			if (!bizObj.IsInDatabase && !bizObj.IsDeleted)
			{
				var country = (value.RN_Code.IsEmpty) ?
						null :
						context.GetCountryByCodeThrowingErrorIfNotFound(context.Factory, value.RN_Code);

				bizObj.CC_LookupCode = GetUniqueLookupCode(context.Factory, value, country);
				bizObj.CC_AddInfo = value.AddInfo;
				bizObj.CC_ClassificationType = value.ClassificationType;
				bizObj.CC_Description = value.Description.ExcludeChars(char.ConvertFromUtf32(30)).Replace("\r\n", "\r").SubstringSafe(0, CusClassificationSchema.CC_Description.MaxLength);
				bizObj.CC_IsActive = true;
				bizObj.CC_IsUnpublished = value.IsUnpublished;
				bizObj.CC_LastAuditedDate = value.LastAuditedDate;
				bizObj.CC_LastAuditedUser = value.LastAuditedUser;
				bizObj.CC_TariffChangePending = value.TariffChangePending;
				bizObj.CC_TariffNum = value.TariffNum;

				if (country != null)
				{
					bizObj.CC_RN_NKCountryCode = country.Code;
				}

				SysMergeValueObjectHelper.ImportCPDecAnswers(context.Factory, value.DefaultCPDecAnswers, bizObj.PK, CusClassificationSchema.Constants.Prefix);
			}

			if (Product != null)
			{
				CreateAndPopulateClassPartPivot(Product, bizObj, value, context);
			}
		}

		protected override void NotifyBizObjCreatedOrUpdated(INotifications notifications, BusinessObject bizObj)
		{
			// Don't notify here. It will be notified if save succeeds.
		}

		void CreateAndPopulateClassPartPivot(Master.OrgSupplierPart product, BaseCusClassification classification, Xsd.CusClassification xsdClassification, IValueObjectImportContext context)
		{
			var xsdPivot = xsdClassification.CusClassPartPivot;
			if (xsdPivot == null || XsdPivotIsEmpty(xsdPivot))
			{
				return;
			}

			var pivotCountry = !xsdPivot.RN_NKCountry.IsEmpty || classification.IsDeleted ? xsdPivot.RN_NKCountry : classification.CC_RN_NKCountryCode;
			if (!pivotCountry.IsEmpty && !classification.IsDeleted && pivotCountry != classification.CC_RN_NKCountryCode)
			{
				throw new InvalidOperationException(string.Format("Country ({0}) specified on pivot for part {1} does not match country of classification ({2}).", pivotCountry, product.OP_PartNum, classification.CC_RN_NKCountryCode));
			}
			var classPivot = !pivotCountry.IsEmpty
				? (BaseCusClassPartPivot)context.Factory.New(new BaseCusClassPartPivotTypeDecider().GetTypeForCountryCode(pivotCountry))
				: context.Factory.New<BaseCusClassPartPivot>();

			classPivot.CI_OP = product.PK;
			if (!pivotCountry.IsEmpty)
			{
				classPivot.CI_RN_NKCountry = pivotCountry;
			}

			if (!classification.IsDeleted)
			{
				classPivot.CI_CC = classification.PK;
			}

			classPivot.CI_AddInfo = xsdPivot.AddInfo;
			classPivot.CI_LastAuditedDate = xsdPivot.LastAuditedDate;
			classPivot.CI_LastAuditedUser = xsdPivot.LastAuditedUser;
			classPivot.CI_TariffChangePending = xsdPivot.TariffChangePending;
			classPivot.CI_TariffNum = xsdPivot.TariffNum;
		}

#if DEBUG
		internal
#endif
		bool XsdPivotIsEmpty(Xsd.CusClassificationCusClassPartPivot xsdPivot)
		{
			return !xsdPivot.RN_NKCountrySpecified
				&& !xsdPivot.AddInfoSpecified
				&& !xsdPivot.LastAuditedDateSpecified
				&& !xsdPivot.LastAuditedUserSpecified
				&& !xsdPivot.TariffChangePendingSpecified
				&& !xsdPivot.TariffNumSpecified;
		}

		ZString GetUniqueLookupCode(BusinessObjectFactory importFactory, Xsd.CusClassification xsdClassification, Master.RefCountry country)
		{
			string countryCode = (country == null) ? null : country.Code.ToString();

			var query = new ZQuery();
			query.AddToFilter(CusClassificationSchema.CC_ClassificationType, xsdClassification.ClassificationType);
			query.AddToFilter(CusClassificationSchema.CC_LookupCode, xsdClassification.LookupCode);
			query.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, countryCode);
			var sameCodeClassif = importFactory.LoadTop1<BaseCusClassification>(query);

			ZString result = xsdClassification.LookupCode;

			if (sameCodeClassif != null)
			{
				var codeLookup = new ClassificationCodeLookup();
				result = codeLookup.GetUniqueLookupCode(importFactory, xsdClassification.LookupCode, xsdClassification.ClassificationType, countryCode);
			}

			return result;
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(BaseCusClassification bizObj, Xsd.CusClassification constructedValueObject, IValueObjectExportContext context)
		{
			SetClassificationXsdValues(constructedValueObject, bizObj);
			SysMergeValueObjectHelper.ExportCPDecAnswers(bizObj.Factory, constructedValueObject.DefaultCPDecAnswers, bizObj.PK, CusClassificationSchema.Constants.Prefix);
		}

		// for exporting classification from product 's part pivot
		public void ExportToValueObject(BaseCusClassPartPivot classPartPivot, Xsd.CusClassification xsdClassification, IValueObjectExportContext context)
		{
			if (classPartPivot != null)
			{
				if (classPartPivot.Classification != null)
				{
					ExportToValueObjectCore(classPartPivot.Classification, xsdClassification, context);
				}

				SetClassPartPivotXsdValues(xsdClassification.CusClassPartPivot, classPartPivot);
			}
		}

		void SetClassPartPivotXsdValues(Xsd.CusClassificationCusClassPartPivot xsdClassPivot, BaseCusClassPartPivot classPivot)
		{
			xsdClassPivot.AddInfo = classPivot.CI_AddInfo;
			xsdClassPivot.LastAuditedUser = classPivot.CI_LastAuditedUser;
			xsdClassPivot.RN_NKCountry = classPivot.CI_RN_NKCountry;
			xsdClassPivot.TariffChangePending = classPivot.CI_TariffChangePending;
			xsdClassPivot.TariffNum = classPivot.CI_TariffNum;

			if (!classPivot.CI_LastAuditedDate.IsEmpty)
			{
				xsdClassPivot.LastAuditedDate = classPivot.CI_LastAuditedDate.ToDateTime();
				xsdClassPivot.LastAuditedDateSpecified = true;
			}

			// Set Specified flag for boolean/numeric fields so they get serialised to XML
			// Boolean
			xsdClassPivot.TariffChangePendingSpecified = true;
		}

		void SetClassificationXsdValues(Xsd.CusClassification xsdClassification, BaseCusClassification classification)
		{
			xsdClassification.PK = classification.PK.ToString();
			xsdClassification.AddInfo = classification.CC_AddInfo;
			xsdClassification.LastAuditedUser = classification.CC_LastAuditedUser;
			xsdClassification.TariffChangePending = classification.CC_TariffChangePending;
			xsdClassification.TariffNum = classification.CC_TariffNum;
			xsdClassification.ClassificationType = classification.CC_ClassificationType;

			xsdClassification.Description = classification.CC_Description.ExcludeChars(char.ConvertFromUtf32(30));

			xsdClassification.IsActive = classification.CC_IsActive;
			xsdClassification.IsUnpublished = classification.CC_IsUnpublished;
			xsdClassification.LookupCode = classification.CC_LookupCode;

			if (!classification.CC_LastAuditedDate.IsEmpty)
			{
				xsdClassification.LastAuditedDate = classification.CC_LastAuditedDate.ToDateTime();
				xsdClassification.LastAuditedDateSpecified = true;
			}

			if (!classification.CC_RN_NKCountryCode.IsEmpty)
			{
				xsdClassification.RN_Code = classification.CC_RN_NKCountryCode;
			}

			// Set Specified flag for boolean/numeric fields so they get serialised to XML
			// Boolean
			xsdClassification.TariffChangePendingSpecified = true;
			xsdClassification.IsActiveSpecified = true;
			xsdClassification.IsUnpublishedSpecified = true;
		}

		#endregion

	}
}
