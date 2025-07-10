using System.Collections;
using Enterprise.Customs.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using CusEntryInstruction = Enterprise.Customs.NL.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.NL.Business;

public sealed class CusGoodsLocationLookups : EU.Business.CusGoodsLocationLookups
{
	public CusGoodsLocationLookups(EU.Business.CusGoodsLocation parent) : base(parent)
	{
	}

	public override ICollection UnlocodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, CargoWise.Types.ZDateTime.Today);

	public override CodeDescriptionPairList QualifierList => Factory.GetCachedValue("NL.CusGoodsLocationLookups.QualifierList", () =>
	{
		var parent = (CusGoodsLocation)Parent;
		JobDeclaration dec = null;

		switch (parent.Parent)
		{
			case JobDeclaration declaration:
				dec = declaration;
				break;
			case CusEntryInstruction instruction:
				dec = instruction.JobDeclaration;
				break;
		}

		if (dec != null && (dec.IsImport || dec.IsExport))
		{
			var qualifierList = new CodeDescriptionPairList();
			qualifierList.AddPair(CusGoodsLocationQualifierList.Codes.PostcodeAddress, CusGoodsLocationQualifierList.Descriptions.PostcodeAddress);
			return qualifierList;
		}

		return base.QualifierList;
	});
}
