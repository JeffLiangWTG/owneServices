using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public sealed class JobComInvoiceHeaderLookups : EU.Business.Declaration.JobComInvoiceHeaderLookups
	{
		public JobComInvoiceHeaderLookups(JobComInvoiceHeader parent)
		: base(parent)
		{
		}

		public override ICodeDescriptionPairList ValuationCodeList
		{
			get
			{
				var attributeNameValuePairs = new Dictionary<ZString, ZString> { { UniversalReferenceConstants.RefCusCodeListAttributes.Name.A1150, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes } };
				if (Parent.IsExport)
				{
					attributeNameValuePairs.Add(UniversalReferenceConstants.RefCusCodeListAttributes.Name.C0091, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
				}

				return RefCusCodeListTypes.GetCachedListMatchAnyAttributes(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature,
					ZDateTime.Today, attributeNameValuePairs.ToArray(), includeParentDataGrouping: false);
			}
		}

		public override CodeDescriptionPairList JZ_IncoTerm_List => Parent.IsImport ? GetIncoTermListImport() : base.JZ_IncoTerm_List;

		CodeDescriptionPairList GetIncoTermListImport()
		{
			return Factory.GetCachedValue("DE_JZ_IncoTerm_List_Import", () =>
			{
				var codeDescriptionList = new IncotermA1840CodeList();
				codeDescriptionList.AddPair(Core.Constants.IncoTerms.Other, Core.Constants.IncoTerms.Descriptions.Other);
				return codeDescriptionList;
			});
		}

		public RefUNLOCOCollection IncoTermPlaceList => Factory.GetCachedValue((NoResString)"DE JobComInvoiceHeaderLookups.IncoTermPlaceList", () => // Caching-key
		{
			var result = new RefUNLOCOCollection(Factory);
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Code", "Property", Parent.JZ_IncoTermPlace)); // identifier
			return result;
		});
	}
}
