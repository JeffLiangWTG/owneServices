using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration;

public class PreviousDocumentLookups : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentLookups
{
	public PreviousDocumentLookups(PreviousDocument parent)
		: base(parent)
	{
	}

	protected new PreviousDocument Parent => (PreviousDocument)base.Parent;

	public override CodeDescriptionPairList ProcedureList
	{
		get
		{
			var procedureCodeListProvider = (IPreviousDocumentProcedureCodeListProvider)new PreviousDocumentProcedureCodeListProvider(Parent);
			return procedureCodeListProvider.GetProcedureCodeList();
		}
	}

	public override CodeDescriptionPairList SubTypeList
	{
		get
		{
			var lookupsAddOn = GetPreviousDocumentCodeSubTypeListProvider();
			return lookupsAddOn.GetPreviousDocumentSubTypeList();
		}
	}

	public override ICollection CodeList
	{
		get
		{
			if (IsUcc6Export)
			{
				return RefCusCodeListTypes.GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, ZDateTime.Today, attributeNameValuePairs: null, includeParentDataGrouping: RefCusCodeListTypes.IncludeParentDataGroupingOptions.ChildFirstThenParent);
			}

			var lookupsAddOn = GetPreviousDocumentCodeSubTypeListProvider();
			return lookupsAddOn.GetPreviousDocumentCodeList();
		}
	}

	public new Customs.Business.CustomsOfficeCodeCollection CustomsOfficeList => new Customs.Business.CustomsOfficeCodeCollection(Factory, Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);

	public override CodeDescriptionPairList UnitOfQuantityList => GetUnitOfQuantityList();

	public override CodeDescriptionPairList UnitOfQuantity2List => GetEuropeanUnionEUNCustomsUQCodeList();

	public override CodeDescriptionPairList UnitOfQuantity3List => GetWeigthCodeDescriptionList();

	public IEnumerable<ZString> SupplementaryQuantityUOMs => Parent.SupplementaryQuantityHandler.SupplementaryQuantityUOMs;

	public CodeDescriptionPairList PackageTypeList => PreviousDocumentLookupsHelper.GetPackageTypeList(Factory);

	CodeDescriptionPairList GetUnitOfQuantityList()
	{
		if (IsUcc6Export)
		{
			return GetEuropeanUnionEUNCustomsUQCodeList();
		}
		return GetWeigthCodeDescriptionList();
	}

	CodeDescriptionPairList GetEuropeanUnionEUNCustomsUQCodeList() => UniversalReferenceHelper.GetEuropeanUnionEUNCustomsUQCodeList(Factory);

	CodeDescriptionPairList GetWeigthCodeDescriptionList() => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);

	PreviousDocumentCodeSubTypeListProvider GetPreviousDocumentCodeSubTypeListProvider()
	{
		var combinationsProvider = new PreviousDocumentCombinationsProvider(Parent);
		return new PreviousDocumentCodeSubTypeListProvider(combinationsProvider);
	}

	ZBool IsUcc6Export => Parent?.Declaration?.IsUCC6AndIsExport ?? false;
}
