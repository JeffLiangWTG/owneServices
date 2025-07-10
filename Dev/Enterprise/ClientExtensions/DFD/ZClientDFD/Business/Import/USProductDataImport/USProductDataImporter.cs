using System;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.DFD.Business.Import
{
	class USProductDataImporter : DataImporter
	{
		protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
		{
			additionalTransactionActions = Array.Empty<ITransactionParticipant>();
			int numberOfRowsToSave = 0;

			string line = dataReader.ReadLine();
			while ((line = dataReader.ReadLine()) != null)
			{
				if (ImportProductData(line, notifications))
				{
					numberOfRowsToSave++;
				}

				if (numberOfRowsToSave % 500 == 0)
				{
					FactoryProvider.SaveCurrentAndCreateNew();
				}
			}

			notifications.Notify(new InfoNotification(string.Format("{0} products  created.", numberOfRowsToSave)));

			return numberOfRowsToSave > 0;
		}

		#region Implementation

		bool ImportProductData(string line, INotifications notifications)
		{
			var dataRow = new USProductDataRow(new FlatFileDataRow(new OCsvLine(line).FieldValues));

			if (IsValidRecord(dataRow, notifications))
			{
				var ownerOrg = FindOrganisation(dataRow.Customer, OrgCusCode.CodeTypes.LegacySystemCode);
				var classification = FindClassification(dataRow.HTCode);

				var product = FactoryProvider.Current.New<Customs.US.Business.OrgSupplierPart>();
				product.OP_PartNum = dataRow.ProductCode;

				var relation = product.RelatedOrganisations.AddNew();
				relation.OU_OH = ownerOrg.PK;
				relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

				var descriptionBuilder = new ZStringBuilder();
				descriptionBuilder.AppendIfNotEmpty(dataRow.ProductDesc1);
				descriptionBuilder.AppendIfNotEmpty(dataRow.ProductDesc2);
				product.OP_Desc = descriptionBuilder.ToStringWithDelimiterBetweenAppends(" ");

				var hTIPivot = product.PivotsForBinding.AddNew();
				hTIPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				hTIPivot.CI_CC = classification != null ? classification.PK : Guid.Empty;

				return true;
			}
			return false;
		}

		#region Validation

		bool IsValidRecord(USProductDataRow dataRow, INotifications notifications)
		{
			var errorBuilder = new ZStringBuilder();

			var ownerOrg = FindOrganisation(dataRow.Customer, OrgCusCode.CodeTypes.LegacySystemCode);
			if (ownerOrg == null)
			{
				errorBuilder.Append(string.Format("Product {0} - Unable to find organisation with Legacy System Code: '{1}' - Product Owner", dataRow.ProductCode, dataRow.Customer));
			}
			else if (MatchProduct(dataRow.ProductCode, ownerOrg))
			{
				errorBuilder.Append(string.Format("Product {0} - Existing product found with product code: '{0}' and owner organisation: '{1}'", dataRow.ProductCode, dataRow.Customer));
			}
			if (!dataRow.ActualManufacturerID.IsEmpty && FindOrganisation(dataRow.ActualManufacturerID, OrgCusCode.USACodeTypes.ManufacturerID) == null)
			{
				errorBuilder.Append(string.Format("Product {0} - Unable to find organisation with manufacturer ID: '{1}'- FDA Manufacturer ID", dataRow.ProductCode, dataRow.ActualManufacturerID));
			}
			if (!dataRow.ActualSupplierShipper.IsEmpty && FindOrganisation(dataRow.ActualSupplierShipper, OrgCusCode.USACodeTypes.ManufacturerID) == null)
			{
				errorBuilder.Append(string.Format("Product {0} - Unable to find organisation with manufacturer ID: '{1}'- FDA Shipper", dataRow.ProductCode, dataRow.ActualSupplierShipper));
			}
			if (!dataRow.FDAConsigneeCode.IsEmpty && FindOrganisation(dataRow.FDAConsigneeCode, OrgCusCode.USACodeTypes.ManufacturerID) == null)
			{
				errorBuilder.Append(string.Format("Product {0} - Unable to find organisation with manufacturer ID: '{1}'- FDA Consignee", dataRow.ProductCode, dataRow.FDAConsigneeCode));
			}
			if (!dataRow.HTCode.IsEmpty && FindClassification(dataRow.HTCode) == null)
			{
				errorBuilder.Append(string.Format("Product {0} - Unable to find import classification code: '{1}'", dataRow.ProductCode, dataRow.HTCode));
			}
			if (!errorBuilder.IsEmpty)
			{
				notifications.Notify(new ErrorNotification(ErrorType.Error, errorBuilder.ToStringWithNewLineBetweenAppends()));
			}

			return errorBuilder.IsEmpty;
		}

		#endregion

		#region Find Classification

		CusClassification FindClassification(ZString classificationCode)
		{
			var query = new ZQuery(CusClassificationSchema.CC_LookupCode, classificationCode);
			query.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedStates);
			var typeQuery = new ZQuery(CusClassificationSchema.CC_ClassificationType, CusClassification.ClassificationType.IMP);
			typeQuery.AddToFilter(JoinCondition.Or, CusClassificationSchema.CC_ClassificationType, CusClassification.ClassificationType.Both);
			query.AddToFilter(typeQuery);

			var classifications = FactoryProvider.Current.Load<CusClassification>(query);
			if (classifications.Length > 0)
			{
				return classifications[0];
			}

			return null;
		}

		#endregion

		#region Find Organisation

		OrgHeader FindOrganisation(ZString cusCode, ZString codeType)
		{
			var query = new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, cusCode);
			query.AddToFilter(OrgCusCodeSchema.OK_CodeType, codeType);
			query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.UnitedStates);

			var cusCodes = FactoryProvider.Current.Load<OrgCusCode>(query);
			if (cusCodes.Length > 0)
			{
				return FactoryProvider.Current.Load<OrgHeader>(cusCodes[0].OK_OH);
			}

			return null;
		}

		#endregion

		#region Match Product

		bool MatchProduct(ZString productCode, OrgHeader ownerOrg)
		{
			var query = new ZDBOnlyQuery(typeof(Customs.US.Business.OrgSupplierPart));
			var relationSubQuery = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
			relationSubQuery.AddToFilter(OrgPartRelationSchema.OU_OH, ownerOrg.PK);
			relationSubQuery.AddToFilter(OrgPartRelationSchema.OU_Relationship, SQLComparisonOperator.NotEqual, OrgPartRelation.RelationshipTypes.Supplier);
			query.AddSubQuery(relationSubQuery, JoinCondition.And);
			query.AddToFilter(OrgSupplierPartSchema.OP_PartNum, productCode);

			return FactoryProvider.Current.Load<Customs.US.Business.OrgSupplierPart>(query).Length > 0;
		}

		#endregion

		#endregion
	}
}
