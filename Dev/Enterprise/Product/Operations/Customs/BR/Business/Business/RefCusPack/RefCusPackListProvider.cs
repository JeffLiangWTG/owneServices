using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Integration.Customs.BR;
using ECC = Enterprise.Core.Constants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public sealed class RefCusPackListProvider : MasterFiles.Business.RefCusPackListProvider, IRefCusPackListProvider
	{
		public override CodeDescriptionPairList GetCustomsPackListForAllAreas(BusinessObjectFactory factory, ZString country)
		{
			return base.GetCIPCustomsPackList(factory, country);
		}

		public override CodeDescriptionPairList GetCIPCustomsPackList(BusinessObjectFactory factory, ZString country)
		{
			return RefCusCodeListTypes.GetCachedList(factory, country, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, ZDateTime.Today);
		}

		public override CodeDescriptionPairList GetDeclarationPackTypeList(BusinessObjectFactory factory)
		{
			var declarationImport = factory.GetNull<JobDeclaration>();
			declarationImport.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var importPackTypeList = declarationImport.Lookups.PackingUnitTypesList;

			var declarationExport = factory.GetNull<JobDeclaration>();
			declarationImport.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			var exportPackTypeList = declarationExport.Lookups.PackingUnitTypesList;

			var result = new CodeDescriptionPairList();
			result.AddRange(importPackTypeList);

			foreach (var type in exportPackTypeList.Cast<CodeDescriptionPair>())
			{
				result.AddPairIfNotExist(type.Code, type.Description);
			}

			return result;
		}
	}
}
