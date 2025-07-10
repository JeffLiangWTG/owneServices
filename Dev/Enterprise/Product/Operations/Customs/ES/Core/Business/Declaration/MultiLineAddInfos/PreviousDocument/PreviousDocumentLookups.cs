using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class PreviousDocumentLookups : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentLookups
	{
		public PreviousDocumentLookups(PreviousDocument parent) : base(parent)
		{
		}

		protected new PreviousDocument Parent => (PreviousDocument)base.Parent;

		public CodeDescriptionPairList CustomsUQList =>
			RefCusCodeListTypes.GetCachedList(
				Factory,
				Parent.Declaration?.GetDefaultDataGroupingCode() ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ,
				Parent.Declaration?.DateOfValuation ?? ZDateTime.Today);

		public override ICollection CodeList
		{
			get
			{
				if (IsT2LOrT2C())
				{
					var result = GetPreviousDocumentListForExportOrT2LPous([new RefCusCodeListAttributeFilter(UniversalReferenceConstants.RefCusCodeListAttributeNames.IsT2LPous, SQLComparisonOperator.StartsWith, "Y")]);

					if (!result.IsLoaded && !result.IsLoading)
					{
						result.Load();
					}
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.AttributeName, "Property", (ZString)UniversalReferenceConstants.RefCusCodeListAttributeNames.IsT2LPous, false));
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.AttributeValue, "ComparisonOperator", (ZString)ModuleTextFilter.ComparisonConstants.Exact, false));
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.AttributeValue, "Property", (ZString)"Y", false));
					return result;
				}
				else if (Parent.Declaration?.IsUCC6AndIsExport ?? false)
				{
					return GetPreviousDocumentListForExportOrT2LPous(System.Array.Empty<RefCusCodeListAttributeFilter>());
				}
				else
				{
					var codeType = ZString.Empty;
					var countryCode = ZString.Empty;
					if (Parent.ImportExportParent is ICanBeImportOrExport canBeImportOrExport)
					{
						countryCode = canBeImportOrExport.DataGroupingCode;
						if (canBeImportOrExport.IsImport)
						{
							var isH2 = false;
							if (Parent.ParentIsJobComInvoiceLine)
							{
								var invLine = (JobComInvoiceLine)Parent.Parent;
								isH2 = invLine.EntryInstruction?.IsH2 ?? false;
							}
							else if (Parent.ParentIsJobComInvoiceHeader)
							{
								var invoice = (JobComInvoiceHeader)Parent.Parent;
								isH2 = !invoice.InvoiceLines.Any(x => !((JobComInvoiceLine)x).EntryInstruction?.IsH2 ?? false);
							}
							else if (Parent.ParentIsDeclaration)
							{
								var dec = (JobDeclaration)Parent.Parent;
								isH2 = !dec.CustomsEntryInstructions.Any<CusEntryInstruction>(x => !x.IsH2);
							}

							codeType = isH2 ? UniversalReferenceConstants.RefCusCodeListTypes.DC40W
											: Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection;
						}
						else if (canBeImportOrExport.IsExport)
						{
							codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection;
						}
					}
					return codeType.IsEmpty || countryCode.IsEmpty ? new ZZRefCusCodeListCombinedCollection(Factory) : ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, countryCode, codeType, ZDateTime.Today);
				}
			}
		}

		public override CodeDescriptionPairList SubTypeList =>
			(Parent.Declaration?.IsUCC6AndIsImport ?? false) && !IsT2LOrT2C()
			? (Parent.CSI_Code == UniversalReferenceConstants.PreviousDocumentType.NMRN ? UCC6ImportSubTypeList : EmptySubTypeList)
			: base.SubTypeList;

		CodeDescriptionPairList UCC6ImportSubTypeList => Factory.GetCachedValue<PreviousDocumentNMRNClassList>();
		CodeDescriptionPairList EmptySubTypeList => [];

		ZZRefCusCodeListCombinedCollection GetPreviousDocumentListForExportOrT2LPous(IEnumerable<RefCusCodeListAttributeFilter> attributeFilters) =>
			ZZRefCusCodeListCombinedCollection.GetCachedCollection(
				Factory,
				Core.Constants.CountryCodes.Spain,
				[UniversalReferenceConstants.RefCusCodeListTypes.DC40A],
				ZDateTime.Today,
				attributeFilters,
				false);

		public CodeDescriptionPairList PackageCodeList =>
			RefCusCodeListTypes.GetCachedList(
				Factory,
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				ZDateTime.Today);

		bool IsT2LOrT2C()
		{
			var isT2LOrT2C = false;
			if (Parent.ParentIsJobComInvoiceLine)
			{
				isT2LOrT2C = isEntryInstructionT2LOrT2C(((JobComInvoiceLine)Parent.Parent).EntryInstruction);
			}
			else if (Parent.ParentIsJobComInvoiceHeader)
			{
				var invoice = (JobComInvoiceHeader)Parent.Parent;
				isT2LOrT2C = invoice.InvoiceLines.Count > 0 && invoice.InvoiceLines.All(x =>
				{
					return isEntryInstructionT2LOrT2C(((JobComInvoiceLine)x).EntryInstruction);
				});
			}
			else if (Parent.ParentIsDeclaration)
			{
				var dec = (JobDeclaration)Parent.Parent;
				isT2LOrT2C = dec.InvoiceLines.Count > 0 && dec.InvoiceLines.All(x =>
				{
					return isEntryInstructionT2LOrT2C(((JobComInvoiceLine)x).EntryInstruction);
				});
			}
			else if (Parent.ParentIsEntryInstruction)
			{
				return isEntryInstructionT2LOrT2C((CusEntryInstruction)Parent.Parent);
			}

			return isT2LOrT2C;

			bool isEntryInstructionT2LOrT2C(CusEntryInstruction entryInstruction) => entryInstruction != null && (entryInstruction.IsT2L || entryInstruction.IsT2C);
		}
	}
}
