using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public class AdditionalInfoLookups : BaseAdditionalInfoLookups
	{
		public AdditionalInfoLookups(AdditionalInfo parent)
			: base(parent)
		{
		}
		new protected AdditionalInfo Parent => (AdditionalInfo)base.Parent;

		ZBool UseUCCAdditionalInfos
		{
			get
			{
				var parent = Parent.Parent;
				return UseUCCAdditionalInfosCore(parent);
			}
		}

		protected virtual ZBool UseUCCAdditionalInfosCore(BusinessObject parent)
		{
			var support = false;
			if (parent is JobDeclaration declaration)
			{
				support = declaration.Configuration.UCCAdditionalInfosSupport(declaration);
			}
			else if (parent is JobComInvoiceHeader invoice)
			{
				support = invoice.JobDeclaration?.Configuration.UCCAdditionalInfosSupport(invoice.JobDeclaration) ?? false;
			}
			else if (parent is JobComInvoiceLine invoiceLine)
			{
				support = invoiceLine.Declaration?.Configuration.UCCAdditionalInfosSupport(invoiceLine.Declaration) ?? false;
			}
			else if (parent is CusEntryInstruction entryInstruction)
			{
				support = entryInstruction.JobDeclaration?.Configuration.UCCAdditionalInfosSupport(entryInstruction.JobDeclaration) ?? false;
			}
			else if (parent is CusClassPartPivot pivot)
			{
				support = pivot.Configuration.UCCAdditionalInfosSupport(pivot);
			}
			return support;
		}

		protected virtual ZBool OmitLevelAttribute => false;

		public override ICollection CodeList
		{
			get
			{
				var importExportParent = Parent.ImportExportParent;

				if (importExportParent is BusinessObject bo && !bo.IsDeleted)
				{
					var direction = GetDirection(importExportParent);
					return UseUCCAdditionalInfos ? GetUCCAdditionalInformationList(importExportParent, direction) : GetAdditionalInformationList(importExportParent, direction);
				}

				return new CodeDescriptionPairList();
			}
		}

		protected virtual ICollection GetUCCAdditionalInformationList(ICanBeImportOrExport importExportParent, ZString direction)
		{
			var subType = Parent.CSI_SubType;
			var parentObject = Parent.Parent;
			string attributeValue = null;

			if (parentObject is JobDeclaration || parentObject is JobComInvoiceHeader)
			{
				attributeValue = UniversalReferenceConstants.RefCusCodeListLevelType.Header;
			}
			else if (parentObject is JobComInvoiceLine || parentObject is CusClassPartPivot)
			{
				attributeValue = UniversalReferenceConstants.RefCusCodeListLevelType.Item;
			}

			var codeTypes = GetRefCusCodeListTypes(importExportParent, subType);

			if (codeTypes.Length != 0)
			{
				var codeList = EUUniversalLookupsHelper.GetUCCAdditionalInformationList(Factory, GetAdditionalFilterForUCCAdditionalInformation(), GetDataGroupingForUCCAdditionalInformation(importExportParent), codeTypes, attributeValue, ignoreLevel: OmitLevelAttribute);
				return codeList;
			}
			return new ZZRefCusCodeListCombinedCollection(Factory);
		}

		protected virtual ZQuery GetAdditionalFilterForUCCAdditionalInformation() => null;

		protected virtual string GetDataGroupingForUCCAdditionalInformation(ICanBeImportOrExport importExportParent) => importExportParent.DataGroupingCode;

		protected virtual ZString[] GetRefCusCodeListTypes(ICanBeImportOrExport importExportParent, ZString csiSubType)
		{
			var codeType = ZString.Empty;
			if (importExportParent.IsImport)
			{
				codeType = AdditionalInfoLookupsHelper.GetImportRefCusCodeListType(csiSubType);
			}
			else if (importExportParent.IsExport)
			{
				codeType = AdditionalInfoLookupsHelper.GetUCCExportRefCusCodeListType(csiSubType);
			}
			return codeType.IsEmpty ? Array.Empty<ZString>() : new ZString[] { codeType };
		}

		public override CodeDescriptionPairList StatusList => Factory.GetCachedValue<AdditionalInfoIssuerList>();

		public CodeDescriptionPairList List63ExportFromCountries
		{
			get
			{
				return Factory.GetCachedValue("EU.NCTS.List63", delegate
				{
					var list = new CodeDescriptionPairList();
					foreach (ZString code in Core.Constants.CountryCodes.EuCommonTransitCountries)
					{
						var countryName = new RefCountry.Loader(Factory).LoadForCountry(code);  // yeah yeah, I know that this is a DB fetch inside a loop.... but there's no such method as RefCountry.Loader.LoadForCountries(string[] codesssss).  So shhhh.
						list.AddPairIfNotExist(code, countryName != null ? countryName.Description : code);
					}
					return list;
				});
			}
		}
	}
}
