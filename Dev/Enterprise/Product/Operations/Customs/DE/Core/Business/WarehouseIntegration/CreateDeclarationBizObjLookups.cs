using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class CreateDeclarationBizObjLookups : ZLookups
	{
		public CreateDeclarationBizObjLookups(CreateDeclarationBizObj parent) : base(parent)
		{
		}

		public CustomsOfficeCodeCollection CustomsOffices =>
			CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles(Factory, Core.Constants.CountryCodes.Germany, Array.Empty<ZString>());

		public CodeDescriptionPairList CpcList
		{
			get
			{
				return Factory.GetCachedValue("CreateDeclarationBizObjLookups.CpcList", () =>
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(ImportMainProcedureCodeList.Codes._40, ImportMainProcedureCodeList.Descriptions._40);
					list.AddPair(ImportMainProcedureCodeList.Codes._42, ImportMainProcedureCodeList.Descriptions._42);
					return list;
				});
			}
		}

		public virtual CodeDescriptionPairList DeclarationTypeList => new CodeDescriptionPairList
		{
			new CodeDescriptionPair(ImportDeclarationTypeList.Codes.EZA, ImportDeclarationTypeList.Descriptions.EZA),
			new CodeDescriptionPair(ImportDeclarationTypeList.Codes.AZ, ImportDeclarationTypeList.Descriptions.AZ),
			new CodeDescriptionPair(ImportDeclarationTypeList.Codes.VZA, ImportDeclarationTypeList.Descriptions.VZA),
		};
	}
}
