using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	public class LocalPartNumberPivotFinder
	{
		public LocalPartNumberPivotFinder(LocalPartNumber localPartNumber)
		{
			this.localPartNumber = localPartNumber;
			factory = localPartNumber.Factory;
			goodsCatalog = localPartNumber.GoodsCatalog;
		}

		readonly BusinessObjectFactory factory;
		readonly LocalPartNumber localPartNumber;
		readonly CusGoodsCatalog goodsCatalog;

		public ZString WarningMessage => MatchingPivots.Count == 0 ? warningMessage : ZString.Empty;
		MultilingualString warningMessage;

		public IReadOnlyList<CusClassPartPivot> MatchingPivots => matchingPivots ??= FindMatchingPivots();
		CusClassPartPivot[] matchingPivots;

		CusClassPartPivot[] FindMatchingPivots()
		{
			CusClassPartPivot[] matchingPivots = null;
			warningMessage = null;

			if (CheckGoodsCatalog())
			{
				var catalogRootCnpj = goodsCatalog.Owner.GetRootCNPJ();
				if (CheckOwnerRootCNPJ(catalogRootCnpj))
				{
					var parts = FindProductsByPartNumber();
					if (parts.Length > 0)
					{
						parts = FilterActiveProductsByRootCNPJ(parts, catalogRootCnpj);
						if (parts.Length > 0)
						{
							matchingPivots = FindMatchingPivots(parts, catalogRootCnpj);
						}
					}
				}
			}
			return matchingPivots ?? Array.Empty<CusClassPartPivot>();
		}

		bool CheckGoodsCatalog()
		{
			if (goodsCatalog.CGC_Type.IsEmpty || !goodsCatalog.CGC_OH_Owner.IsValid)
			{
				warningMessage = MustEnterCatalogOwnerAndTypeMessage;
			}
			return warningMessage == null;
		}

		bool CheckOwnerRootCNPJ(ZString catalogRootCnpj)
		{
			if (catalogRootCnpj.IsEmpty)
			{
				warningMessage = CatalogOwnerDoesNotHaveRootCnpjMessage;
			}
			return warningMessage == null;
		}

		OrgSupplierPart[] FindProductsByPartNumber()
		{
			var query = new ZQuery(OrgSupplierPartSchema.OP_PartNum, localPartNumber.CGI_Reference);
			query.IgnoreActiveFilter = true;

			var parts = factory.Load<OrgSupplierPart>(query);
			if (parts.Length == 0)
			{
				warningMessage = NoProductsFoundMessage;
			}
			return parts;
		}

		OrgSupplierPart[] FilterActiveProductsByRootCNPJ(OrgSupplierPart[] candidates, ZString catalogRootCnpj)
		{
			var parts = candidates.Where(x => x.OP_IsActive && HasEquivalentOrganisation(x, catalogRootCnpj)).ToArray();
			if (parts.Length == 0)
			{
				warningMessage = goodsCatalog.IsImport ? InactiveOrNoProductMatchsOwnerMessage : InactiveOrNoProductMatchsSupplierMessage;
			}
			return parts;
		}

		CusClassPartPivot[] FindMatchingPivots(OrgSupplierPart[] candidates, ZString catalogRootCnpj)
		{
			var matchingPivots = candidates.SelectMany(s => s.GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.Brazil)).Where(w => IsMatchingPivot(w, catalogRootCnpj)).Distinct().ToArray();
			if (matchingPivots.Length == 0)
			{
				warningMessage = GetNoMatchingClassificationMessage(localPartNumber.CGI_Reference, goodsCatalog.CGC_Tariff, goodsCatalog.Owner.OH_Code);
			}
			return matchingPivots;
		}

		bool HasEquivalentOrganisation(OrgSupplierPart part, string catalogRootCnpj)
		{
			return part.RelatedOrganisations.Where(w => (w.IsOwner && goodsCatalog.IsImport) || (w.IsSupplier && goodsCatalog.IsExport))
				.Any(w => w.Header.GetRootCNPJFromCNPJ() == catalogRootCnpj);
		}

		bool IsMatchingPivot(CusClassPartPivot pivot, string catalogRootCnpj)
		{
			return ((pivot.IsImportClassification && goodsCatalog.IsImport) || (pivot.IsExportClassification && goodsCatalog.IsExport))
				&& (pivot.CI_CGC_Catalog == goodsCatalog.PK || pivot.CI_TariffNum == goodsCatalog.CGC_Tariff)
				&& (pivot.CI_OH.IsEmpty || pivot.Header.GetRootCNPJFromCNPJ() == catalogRootCnpj);
		}

		public static MultilingualString MustEnterCatalogOwnerAndTypeMessage => ResString.GetMultilingualString("5DB1E964-F831-496B-8FA7-D0646CE67D44", "You must enter a Catalog Owner and Type before product file be validated.");

		public static MultilingualString CatalogOwnerDoesNotHaveRootCnpjMessage = ResString.GetMultilingualString("5402517E-F9AE-428B-9E3B-F8A053CB95B3", "The entered Owner does not have Root CNPJ. Add the Root CNPJ to current Owner or entered a different Owner that contains Root CNPJ.");

		public static MultilingualString NoProductsFoundMessage = ResString.GetMultilingualString("D2008AC2-5FBD-4A46-B56D-F7EBF86BE015", "No product(s) found with this code.");

		public static MultilingualString InactiveOrNoProductMatchsOwnerMessage = ResString.GetMultilingualString("5B8F3744-A913-4B63-B891-8CEEF3591105", "A product with this code exists but it is inactive or the Importer (Owner) Relationship, or the relationship of any other that have same Root CNPJ , on that Product does not match with this catalog. Either add new Product (F3), make it active (F4, add a filter to show inactive records, search and edit), or change the Supplier (Exporter)/Owner (Importer) relationship on the existing Product (F4 then edit a Product).");

		public static MultilingualString InactiveOrNoProductMatchsSupplierMessage = ResString.GetMultilingualString("B36D4801-BA3F-40AB-B84F-09A255EB8C66", "A product with this code exists but it is inactive or the Exporter (Supplier) Relationship, or the relationship of any other that have same Root CNPJ , on that Product does not match with this catalog. Either add new Product (F3), make it active (F4, add a filter to show inactive records, search and edit), or change the Supplier (Exporter)/Owner (Importer) relationship on the existing Product (F4 then edit a Product).");

		public static MultilingualString GetNoMatchingClassificationMessage(string partNumber, string tariff, string ownerCode) => ResString.GetMultilingualString("5AB2ED9D-9FBD-474E-A16A-F68FE686A6EE", "Cannot match classification for Product ({0}), Tariff Code ({1}) and Owner ({2}).", partNumber, tariff, ownerCode);
	}
}
