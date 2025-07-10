using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class PreviousDocumentLookups : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentLookups
	{
		public PreviousDocumentLookups(PreviousDocument parent) : base(parent)
		{
		}

		public override ICollection ReferenceList
		{
			get
			{
				var previousDoc = Parent;
				if (previousDoc.IsTemporaryStorageDocument)
				{
					var result = new CusTempStorageRegHeaderCollection(Factory);
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusTempStorageRegHeaderCollectionFilterConstants.Status, "Property", (ZString)TempStorageDeclarationStatusList.Codes.Open, false));

					var office = previousDoc.Declaration?.JE_CustomsOffice ?? ZString.Empty;
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusTempStorageRegHeaderCollectionFilterConstants.CustomsOffice, "Property", office));

					var packType = ZString.Empty;
					var parentIsInvoiceLine = previousDoc.ParentIsJobComInvoiceLine;
					if (parentIsInvoiceLine)
					{
						var invoiceLine = previousDoc.Parent as JobComInvoiceLine;
						packType = invoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().FirstOrDefault(x => x.IsLinked)?.Package.CW_PackType ?? ZString.Empty;
					}
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusTempStorageRegHeaderCollectionFilterConstants.PackingType, "Property", packType));

					if (previousDoc.CSI_Code == UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage)
					{
						var reference = previousDoc.CSI_ReferenceNumber;
						result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusTempStorageRegHeaderCollectionFilterConstants.TsdNumber, "Property", reference));

						result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusTempStorageRegHeaderCollectionFilterConstants.PreviousReferenceNumber, "Property", ZString.Empty));
					}
					return result;
				}
				else
				{
					return null;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public static class CusTempStorageRegHeaderCollectionFilterConstants
		{
			public const string Status = "Status";
			public const string PackingType = "Packing Type";
			public const string CustomsOffice = "Customs Office";
			public const string TsdNumber = "TSD Number";
			public const string PreviousReferenceNumber = "Previous Reference Number";
		}

		public override ICollection CodeList => (Parent.Declaration?.IsUCC6AndIsImport ?? false) ? RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfUCCImport, ZDateTime.Today, includeParentDataGrouping: true) : Factory.GetCachedValue<PreviousDocumentCodeList>();

		public CodeDescriptionPairList PackageTypeList => UniversalReferenceDataHelper.GetCachedPackageTypeList(Parent.Factory);

		public override CodeDescriptionPairList UnitOfQuantityList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, ZDateTime.Today);

		public new PreviousDocument Parent => (PreviousDocument)base.Parent;
	}
}
