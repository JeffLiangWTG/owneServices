using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.Integration.Warehouse;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Business.Update.ProductEntityMatching
{
	class ProductEntityMatchingProductStyleHelper
	{
		public ProductEntityMatchingProductStyleHelper(IEntity xmlProduct)
		{
			XmlProduct = Argument.NotNull(xmlProduct, nameof(xmlProduct));
			ProductStyleColourEntityCache = new Lazy<IEntity>(() => GetProductStyle("ProductStyleColour"));
			ProductStyleClassificationEntityCache = new Lazy<IEntity>(() => GetProductStyle("ProductStyleClassification"));
			ProductStyleSizeEntityCache = new Lazy<IEntity>(() => GetProductStyle("ProductStyleSize"));
		}
		IEntity XmlProduct { get; }
		Lazy<IEntity> ProductStyleColourEntityCache { get; }
		Lazy<IEntity> ProductStyleClassificationEntityCache { get; }
		Lazy<IEntity> ProductStyleSizeEntityCache { get; }

		IEntity GetProductStyle(string productStyleEntityName)
		{
			IEntity productStyleToReturn = null;

			var productStyles = XmlProduct.Parents.Where(entity => entity.EntityName == productStyleEntityName).ToArray();
			if (productStyles.Length > 1)
			{
				throw new NativeXMLUserVisibleException($"Only 1 {productStyleEntityName} can be specified.");
			}

			var productStyle = productStyles.SingleOrDefault();
			if (productStyle != null)
			{
				if (productStyle.Action == EntityAction.DELETE)
				{
					throw new NativeXMLUserVisibleException("Delete action for product style entities is not allowed.");
				}

				if (productStyle.Properties.Any())
				{
					productStyleToReturn = productStyle;
				}
				else
				{
					productStyle.Action = EntityAction.IGNORE;
				}
			}

			return productStyleToReturn;
		}

		#region FailIfInvalidProductColourAndSizeStyles

		public void FailIfInvalidProductColourAndSizeStyles(OrgSupplierPart partLoadedFromEnterprise)
		{
			if ((ProductStyleColourEntity != null) ^ (ProductStyleSizeEntity != null))
			{
				throw new NativeXMLUserVisibleException("Product style colour and product style size must be both specified or both unspecified.");
			}

			if ((ProductStyleClassificationEntity != null) && (ProductStyleColourEntity == null))
			{
				throw new NativeXMLUserVisibleException("Product style classification cannot be set without a product style colour and product style size.");
			}

			if (ProductStyleColourEntity != null)
			{
				FailIfInvalidProductStyleParents();
				FailIfInvalidPropertyInEntity();
				FailIfColourEntityDoesNotMatchColourForProductInDB(partLoadedFromEnterprise);
				FailIfSizeEntityDoesNotMatchSizeForProductInDB(partLoadedFromEnterprise);
				FailIfClassificationEntityDoesNotMatchClassificationForProductInDB(partLoadedFromEnterprise);
			}
		}

		#region FailIfInvalidProductStyleParents

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void FailIfInvalidProductStyleParents()
		{
			var productStyleCodeForColour = ColourParentProductStyleEntity.GetPropertyOrBlankString("Code");
			var productStyleCodeForSize = SizeParentProductStyleEntity.GetPropertyOrBlankString("Code");
			var productStyleCodeForClassification = ClassificationParentProductStyleEntity?.GetPropertyOrBlankString("Code");

			if (productStyleCodeForColour != productStyleCodeForSize
				|| (productStyleCodeForClassification != null && productStyleCodeForClassification != productStyleCodeForColour))
			{
				throw new NativeXMLUserVisibleException("Product style for both style colour, style classification and style size should be the same.");
			}

			FailIfProductStyleParentsHaveIncorrectOwner();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void FailIfProductStyleParentsHaveIncorrectOwner()
		{
			var productStyleOwnerCodeForColour = GetProductStyleParentOwner(ColourParentProductStyleEntity).GetPropertyOrBlankString("Code");
			var productStyleOwnerCodeForSize = GetProductStyleParentOwner(SizeParentProductStyleEntity).GetPropertyOrBlankString("Code");
			var productStyleOwnerCodeForClassification = ClassificationParentProductStyleEntity != null ? GetProductStyleParentOwner(ClassificationParentProductStyleEntity).GetPropertyOrBlankString("Code") : null;

			if (productStyleOwnerCodeForColour != productStyleOwnerCodeForSize
				|| (productStyleOwnerCodeForClassification != null && productStyleOwnerCodeForClassification != productStyleOwnerCodeForColour))
			{
				throw new NativeXMLUserVisibleException("Product style owner for style colour, style classification and style size should be the same.");
			}

			if (SingleProductOwnerCode != productStyleOwnerCodeForColour)
			{
				throw new NativeXMLUserVisibleException("Product style owner is not the product owner.");
			}
		}

		static IEntity GetProductStyleParentOwner(IEntity productStyleParent)
		{
			var productStyleOwner = productStyleParent.Parents.FirstOrDefault(parent => parent.EntityName == "Owner") ?? throw new NativeXMLUserVisibleException("Product style should have an owner.");

			return productStyleOwner;
		}

		#endregion

		#region FailIfInvalidPropertyInEntity

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void FailIfInvalidPropertyInEntity()
		{
			if (!string.IsNullOrEmpty(ProductStyleSizeEntity.GetPropertyOrBlankString("Sequence")))
			{
				throw new NativeXMLUserVisibleException("Sequence can not be assigned to the product style size.");
			}
		}

		#endregion

		#region FailIfColourEntityDoesNotMatchColourForProductInDB

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void FailIfColourEntityDoesNotMatchColourForProductInDB(OrgSupplierPart partLoadedFromEnterprise)
		{
			if (partLoadedFromEnterprise != null && !partLoadedFromEnterprise.OP_WSC_WhsProductStyleColour.IsEmpty && ProductStyleColourEntity.Action != EntityAction.IGNORE)
			{
				var productStyleColourInDB = partLoadedFromEnterprise.Factory.Load<IWhsProductStyleColour>(partLoadedFromEnterprise.OP_WSC_WhsProductStyleColour);
				if (productStyleColourInDB != null)
				{
					if (productStyleColourInDB.WSC_Code != ProductStyleColourEntity.GetPropertyOrBlankString("Code"))
					{
						throw new NativeXMLUserVisibleException(ProductStyleColourError);
					}

					var productStyleInDB = partLoadedFromEnterprise.Factory.Load<IWhsProductStyle>(productStyleColourInDB.WSC_WST_ProductStyle);
					if (productStyleInDB.WST_Code != ColourParentProductStyleEntity.GetPropertyOrBlankString("Code"))
					{
						throw new NativeXMLUserVisibleException(ProductStyleColourError);
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		string ProductStyleColourError => "A product style colour has already been assigned to the product.";

		#endregion

		#region FailIfSizeEntityDoesNotMatchSizeForProductInDB

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void FailIfSizeEntityDoesNotMatchSizeForProductInDB(OrgSupplierPart partLoadedFromEnterprise)
		{
			if (partLoadedFromEnterprise != null && !partLoadedFromEnterprise.OP_WSZ_WhsProductStyleSize.IsEmpty && ProductStyleSizeEntity.Action != EntityAction.IGNORE)
			{
				var productStyleSizeInDB = partLoadedFromEnterprise.Factory.Load<IWhsProductStyleSize>(partLoadedFromEnterprise.OP_WSZ_WhsProductStyleSize);
				if (productStyleSizeInDB != null)
				{
					if (productStyleSizeInDB.WSZ_Size != ProductStyleSizeEntity.GetPropertyOrBlankString("Size"))
					{
						throw new NativeXMLUserVisibleException(ProductStyleSizeError);
					}

					var productStyleInDB = partLoadedFromEnterprise.Factory.Load<IWhsProductStyle>(productStyleSizeInDB.WSZ_WST_ProductStyle);
					if (productStyleInDB.WST_Code != SizeParentProductStyleEntity.GetPropertyOrBlankString("Code"))
					{
						throw new NativeXMLUserVisibleException(ProductStyleSizeError);
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const string")]
		const string ProductStyleSizeError = "A product style size has already been assigned to the product.";

		#endregion

		#region FailIfClassificationEntityDoesNotMatchClassificationForProductInDB

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void FailIfClassificationEntityDoesNotMatchClassificationForProductInDB(OrgSupplierPart partLoadedFromEnterprise)
		{
			if (partLoadedFromEnterprise != null && !partLoadedFromEnterprise.OP_WSS_WhsProductStyleClassification.IsEmpty && ProductStyleClassificationEntity.Action != EntityAction.IGNORE)
			{
				var productStyleClassificationInDB = partLoadedFromEnterprise.Factory.Load<IWhsProductStyleClassification>(partLoadedFromEnterprise.OP_WSS_WhsProductStyleClassification);
				if (productStyleClassificationInDB != null)
				{
					if (productStyleClassificationInDB.WSS_Code != ProductStyleClassificationEntity.GetPropertyOrBlankString("Code"))
					{
						throw new NativeXMLUserVisibleException(ProductStyleClassificationError);
					}

					var productStyleInDB = partLoadedFromEnterprise.Factory.Load<IWhsProductStyle>(productStyleClassificationInDB.WSS_WST_ProductStyle);
					if (productStyleInDB.WST_Code != ClassificationParentProductStyleEntity.GetPropertyOrBlankString("Code"))
					{
						throw new NativeXMLUserVisibleException(ProductStyleClassificationError);
					}
				}
			}
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const string")]
		const string ProductStyleClassificationError = "A product style classification has already been assigned to the product.";

		#endregion

		#endregion

		#region AddSequenceNumberToProductStyleSizeIfNotSpecified

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public void AddSequenceNumberToProductStyleSizeIfNotSpecified(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, nameof(factory));

			if (ProductStyleSizeEntity != null && string.IsNullOrEmpty(ProductStyleSizeEntity.GetPropertyOrBlankString("Sequence")))
			{
				var productStyleSizesInDB = factory.Load<IWhsProductStyleSize>(GetProductStyleSizeQuery());
				var size = ProductStyleSizeEntity.GetPropertyOrBlankString("Size");

				byte sequenceNumber;

				if (productStyleSizesInDB.Length > 0)
				{
					var productStyleSizeInDB = productStyleSizesInDB.FirstOrDefault(s => s.PK == ProductStyleSizeEntity.InternalPK || size.Equals(s.WSZ_Size, StringComparison.OrdinalIgnoreCase));
					sequenceNumber = productStyleSizeInDB?.WSZ_Sequence	?? (byte)(productStyleSizesInDB[0].WSZ_Sequence + 1);
				}
				else
				{
					sequenceNumber = 1;
				}

				ProductStyleSizeEntity["Sequence"] = sequenceNumber;
			}
		}

		ZQuery GetProductStyleSizeQuery()
		{
			var parentProductStyleSubQuery = GetParentProductStyleQuery();
			var existingProductStyleSizeQuery = new ZDBOnlyQuery(typeof(IWhsProductStyleSize));
			existingProductStyleSizeQuery.AddSubQuery(WhsProductStyleSizeSchema.WSZ_WST_ProductStyle, parentProductStyleSubQuery, JoinCondition.And);
			existingProductStyleSizeQuery.OrderBy = WhsProductStyleSizeSchema.WSZ_Sequence.Name + OrderByClause.Descending;

			return existingProductStyleSizeQuery;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		ZDBOnlySubQuery GetParentProductStyleQuery()
		{
			var orgOwnerSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			orgOwnerSubQuery.AddToFilter(OrgHeaderSchema.OH_Code, SingleProductOwnerCode);

			var parentProductStyleCode = SizeParentProductStyleEntity.GetPropertyOrBlankString("Code");
			var parentProductStyleSubQuery = new ZDBOnlySubQuery(typeof(IWhsProductStyle), WhsProductStyleSchema.PK);
			parentProductStyleSubQuery.AddToFilter(WhsProductStyleSchema.WST_Code, parentProductStyleCode);
			parentProductStyleSubQuery.AddSubQuery(WhsProductStyleSchema.WST_OH_Owner, orgOwnerSubQuery, JoinCondition.And);
			return parentProductStyleSubQuery;
		}

		#endregion

		#region SingleProductOwnerCode

		string SingleProductOwnerCode => singleProductOwnerCode ?? (singleProductOwnerCode = GetSingleProductOwnerCode());
		string singleProductOwnerCode;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		string GetSingleProductOwnerCode()
		{
			var ownerRelationships = new[] { OrgPartRelation.RelationshipTypes.Both, OrgPartRelation.RelationshipTypes.Owner };
			var productOwners = XmlProduct.Children.Where(e => e.EntityName == "OrgPartRelation" && ownerRelationships.Contains(e.GetPropertyOrBlankString("Relationship"))).ToArray();
			if (productOwners.Length > 1)
			{
				throw new NativeXMLUserVisibleException("Product style should not be specified if the product has multiple owners.");
			}
			else if (productOwners.Length < 1)
			{
				throw new NativeXMLUserVisibleException("Product style should not be specified if the product has no owners.");
			}

			return productOwners.Single().Parents.First(p => p.EntityName == "OrgHeader").GetPropertyOrBlankString("Code");
		}

		#endregion

		#region ProductStyleEntities

		IEntity ProductStyleColourEntity => ProductStyleColourEntityCache.Value;
		IEntity ProductStyleSizeEntity => ProductStyleSizeEntityCache.Value;
		IEntity ProductStyleClassificationEntity => ProductStyleClassificationEntityCache.Value;

		#endregion

		#region ParentProductStyleEntities

		IEntity ColourParentProductStyleEntity => colourParentProductStyleEntity ?? (colourParentProductStyleEntity = GetParentProductStyle(ProductStyleColourEntity, "ColourProductStyle"));
		IEntity colourParentProductStyleEntity;

		IEntity SizeParentProductStyleEntity => sizeParentProductStyleEntity ?? (sizeParentProductStyleEntity = GetParentProductStyle(ProductStyleSizeEntity, "SizeProductStyle"));
		IEntity sizeParentProductStyleEntity;

		IEntity ClassificationParentProductStyleEntity
		{
			get
			{
				IEntity result = null;
				if (ProductStyleClassificationEntity != null)
				{
					result = classificationParentProductStyleEntity ?? (classificationParentProductStyleEntity = GetParentProductStyle(ProductStyleClassificationEntity, "ClassificationProductStyle"));
				}
				return result;
			}
		}
		IEntity classificationParentProductStyleEntity;

		static IEntity GetParentProductStyle(IEntity productStyleChild, string parentEntityName)
		{
			var parentProductStyle = productStyleChild.Parents.FirstOrDefault(parent => parent.EntityName == parentEntityName) ?? throw new NativeXMLUserVisibleException("Product style colour/size should have a product style.");

			if (parentProductStyle.Action == EntityAction.DELETE)
			{
				throw new NativeXMLUserVisibleException("Delete action for product style entity is not allowed.");
			}

			return parentProductStyle;
		}

		#endregion
	}
}
