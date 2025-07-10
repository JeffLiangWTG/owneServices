using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.DataTransfer.Business.ProductMatching
{
	public class ProductMatcher
	{
		#region ReasonsForNotMached

		public enum ReasonsForNotMatched
		{
			None = 0,
			EmptyBuyer,
			EmptySupplier,
			DisallowCreate,
			UnmatchedBuyer,
			DuplicateProduct
		}

		#endregion

		#region Constructor

		public ProductMatcher(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		#endregion

		#region Match

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		public void Match(IPart part, out OrgSupplierPart match, out ReasonsForNotMatched reasonForNotMatched)
		{
			var createMissingProductsInfo = SystemDataRegistry.Instance.CreateMissingProductWithRelationship.Value;
			var allowCreate = createMissingProductsInfo.IsOverrideToYes;
			var defaultRalationship = createMissingProductsInfo.DefaultRelationship;
			Match(part, allowCreate, false, out match, out reasonForNotMatched, defaultRalationship);
		}

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		public void MatchAllowEmptySupplier(IPart part, bool allowCreate, out OrgSupplierPart match, out ReasonsForNotMatched reasonForNotMatched)
		{
			Match(part, allowCreate, true, out match, out reasonForNotMatched);
		}

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		void Match(IPart part, bool allowCreate, bool allowEmptySupplier, out OrgSupplierPart match, out ReasonsForNotMatched reasonForNotMatched, string defaultRalationship = "")
		{
			var buyer = part.Buyer;
			var supplier = part.Supplier;
			if (buyer == null)
			{
				match = null;
				reasonForNotMatched = ReasonsForNotMatched.EmptyBuyer;
				return;
			}

			if (supplier == null && !allowEmptySupplier)
			{
				match = null;
				reasonForNotMatched = ReasonsForNotMatched.EmptySupplier;
				return;
			}

			var product = FindByCode(part.PartNum, supplier, buyer);

			if (product != null)
			{
				match = product;
				reasonForNotMatched = ReasonsForNotMatched.None;
				return;
			}

			if (!allowCreate)
			{
				match = null;
				reasonForNotMatched = ReasonsForNotMatched.DisallowCreate;
				return;
			}

			if (buyer.PK == OrgHeader.UnmatchedOrganisationPK)
			{
				match = null;
				reasonForNotMatched = ReasonsForNotMatched.UnmatchedBuyer;
				return;
			}

			if (IsDuplicate(part))
			{
				match = null;
				reasonForNotMatched = ReasonsForNotMatched.DuplicateProduct;
				return;
			}

			product = factory.New<OrgSupplierPart>();
			product.OP_PartNum = part.PartNum;
			product.OP_Desc = part.Description;
			if (defaultRalationship != DefaultRalationshipCodes.Supplier)
			{
				product.RelatedOrganisations.AddOwner(part.Buyer);
			}
			if (supplier != null && defaultRalationship != DefaultRalationshipCodes.Owner)
			{
				product.RelatedOrganisations.AddSupplier(part.Supplier);
			}

			if (!part.StockKeepingUnit.IsEmpty)
			{
				product.OP_StockKeepingUnit = part.StockKeepingUnit;
			}

			factory.Save();

			match = product;
			reasonForNotMatched = ReasonsForNotMatched.None;
		}

		#endregion

		#region FindByCode

		OrgSupplierPart FindByCode(ZString code, IOrgHeader supplier, IOrgHeader buyer)
		{
			return (supplier != null)
					? new OrgSupplierPart.Loader(factory).Load(code, buyer.PK, supplier.PK)
					: new OrgSupplierPart.Loader(factory).Load(code, buyer.PK, ZGuid.Empty);
		}

		#endregion

		#region IsDuplicate

		bool IsDuplicate(IPart part)
		{
			var partiesOnPart = new List<RelatedPartyWithCode>();
			if (part.Buyer != null)
			{
				partiesOnPart.Add(new RelatedPartyWithCode(OrgPartRelation.RelationshipTypes.Owner, part.Buyer.OH_Code));
			}

			if (part.Supplier != null)
			{
				partiesOnPart.Add(new RelatedPartyWithCode(OrgPartRelation.RelationshipTypes.Supplier, part.Supplier.OH_Code));
			}

			var duplicateDetector = new DuplicateProductDetectorNoBizO(ZGuid.Empty, part.PartNum, true, partiesOnPart, null);
			duplicateDetector.Validate();
			return duplicateDetector.HasError;
		}

		#endregion
	}
}


