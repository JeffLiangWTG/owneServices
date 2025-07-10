using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.GB.CDS.Declaration.MultiLineAddInfos
{
	public class CDSAdditionalInfoLookups : AdditionalInfoLookups
	{
		public CDSAdditionalInfoLookups(AdditionalInfo parent) : base(parent)
		{
			ignoreLevel = parent.CSI_ParentTableCode != JobComInvoiceHeaderSchema.Constants.Prefix && parent.CSI_ParentTableCode != JobComInvoiceLineSchema.Constants.Prefix;
		}

		public override CodeDescriptionPairList GetAdditionalInformationList(ICanBeImportOrExport importExportParent, ZString direction)
		{
			return Factory.GetAdditionalInformationList(importExportParent.DataGroupingCode, UniversalReferenceConstants.RefCusCodeListLevelType.Item, direction, ignoreLevel, IncludeParentDataGroupingOptions.ChildFirstThenParent);
		}

		readonly bool ignoreLevel;
	}
}
