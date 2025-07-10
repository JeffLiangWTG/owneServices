using System.Collections;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using RefCusCodeListAttributes = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes;
using RefCusCodeListType = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListType;

namespace Enterprise.Customs.IT.Business;

public sealed class CusAuthorizationUsageLookups : EU.Business.CusAuthorizationUsageLookups
{
	public CusAuthorizationUsageLookups(CusAuthorizationUsage parent) : base(parent)
	{
	}

	public override ICollection CodeList
	{
		get
		{
			return JobDeclaration?.IsExport ?? false
				? GetExportAuthorizationCodes()
				: base.CodeList;
		}
	}

	#region Implementation

	ICollection GetExportAuthorizationCodes()
	{
		if (Parent.Instruction != null)
		{
			return Factory.GetCachedValue("IT.CusAuthorizationUsageLookups.Export.EntryInstruction.CodeList", () => GetAuthorisationCodeList());
		}
		else if (Parent.InvoiceLine != null)
		{
			return Factory.GetCachedValue("IT.CusAuthorizationUsageLookups.Export.InvoiceLine.CodeList", () => GetAuthorisationCodeListWithItemLevelAttribute());
		}
		return new CodeDescriptionPairList();
	}

	ICollection GetAuthorisationCodeListWithItemLevelAttribute()
	{
		var attributeFilter = new RefCusCodeListAttributeFilter(RefCusCodeListAttributes.Names.Level, JoinCondition.And, RefCusCodeListAttributes.Values.Item);
		return GetAuthorisationCodeList(attributeFilter);
	}

	ICollection GetAuthorisationCodeList(params RefCusCodeListAttributeFilter[] attributeFilters)
	{
		var refCusCodeListCollection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Constants.DataGrouping.EuropeanUnion, new ZString[] { RefCusCodeListType.Code.Code_AUTH }, ZDateTime.Today, attributeFilters);
		refCusCodeListCollection.Load();
		refCusCodeListCollection.Sort(AutoRefCusCodeList.Schema.ZZD_Code, ListSortDirection.Ascending);
		return refCusCodeListCollection;
	}

	#endregion
}
