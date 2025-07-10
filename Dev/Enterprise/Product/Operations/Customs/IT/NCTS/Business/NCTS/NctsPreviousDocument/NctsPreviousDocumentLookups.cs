using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsPreviousDocumentLookups :
	EU.NCTS.Business.NctsPreviousDocumentPhase4Lookups
{
	public NctsPreviousDocumentLookups(NctsPreviousDocument parent) : base(parent)
	{
	}

	protected new NctsPreviousDocument Parent => (NctsPreviousDocument)base.Parent;

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
			var lookupsAddOn = GetPreviousDocumentCodeSubTypeListProvider();
			return lookupsAddOn.GetPreviousDocumentCodeList();
		}
	}

	public new Customs.Business.CustomsOfficeCodeCollection CustomsOfficeList => new Customs.Business.CustomsOfficeCodeCollection(Factory, Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);

	public override CodeDescriptionPairList UnitOfQuantityList => PreviousDocumentLookupsHelper.GetKilogramUomList(Factory);

	public override CodeDescriptionPairList UnitOfQuantity2List => PreviousDocumentLookupsHelper.GetKilogramUomList(Factory);

	public override CodeDescriptionPairList UnitOfQuantity3List => PreviousDocumentLookupsHelper.GetKilogramUomList(Factory);

	public IEnumerable<ZString> SupplementaryQuantityUOMs => Parent.SupplementaryQuantityHandler.SupplementaryQuantityUOMs;

	PreviousDocumentCodeSubTypeListProvider GetPreviousDocumentCodeSubTypeListProvider()
	{
		var combinationsProvider = new PreviousDocumentCombinationsProvider(Parent);
		return new PreviousDocumentCodeSubTypeListProvider(combinationsProvider);
	}
}
