using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ImportJobDeclarationLookups : JobDeclarationLookups
	{
		public ImportJobDeclarationLookups(JobDeclaration parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList MessageSubTypeList => Factory.GetCachedValue<EntryStyleListImport>();

		public override CodeDescriptionPairList ApplicationCodeList => Parent.GetInterfaceSubmissionType() == DeclarationApplicationCodeList.Codes.Builtin ? ApplicationCodeList_BLT : ApplicationCodeList_Other;

		CodeDescriptionPairList ApplicationCodeList_BLT
		{
			get
			{
				var isUCC6EnabledForImport = Parent.IsUCC6EnabledForImport;
				return Factory.GetCachedValue($"Enterprise.Customs.IE.Business.Declaration.ImportJobDeclarationLookups.ApplicationCodeList_BLT_IsUCC6EnabledForImport_{isUCC6EnabledForImport}", () =>
				{
					var result = new CodeDescriptionPairList();

					result.AddPair(ImportDeclarationApplicationCodeList.Codes.V1, ImportDeclarationApplicationCodeList.Descriptions.V1);
					if (isUCC6EnabledForImport)
					{
						result.AddPair(ImportDeclarationApplicationCodeList.Codes.V2, ImportDeclarationApplicationCodeList.Descriptions.V2);
					}
					return result;
				});
			}
		}

		public override CodeDescriptionPairList IncoTermList => Factory.GetCachedIncoTermListEU(true);

		CodeDescriptionPairList ApplicationCodeList_Other
		{
			get
			{
				var isUCC6EnabledForImport = Parent.IsUCC6EnabledForImport;
				return Factory.GetCachedValue($"Enterprise.Customs.IE.Business.Declaration.ImportJobDeclarationLookups.ApplicationCodeList_Other_IsUCC6EnabledForImport_{isUCC6EnabledForImport}", () =>
				{
					var result = new CodeDescriptionPairList();

					result.AddPair(ImportDeclarationApplicationCodeList.Codes.V1, ImportDeclarationApplicationCodeList.Descriptions.V1);
					if (isUCC6EnabledForImport)
					{
						result.AddPair(ImportDeclarationApplicationCodeList.Codes.V2, ImportDeclarationApplicationCodeList.Descriptions.V2);
					}
					result.AddPair(ImportDeclarationApplicationCodeList.Codes.Interfaced, ImportDeclarationApplicationCodeList.Descriptions.Interfaced);
					return result;
				});
			}
		}

		public override CodeDescriptionPairList EntryStatusList => Factory.GetCachedValue<AISEntryStatusList>();

		protected override ZString CustomsOfficeDataGrouping => Core.Constants.CountryCodes.Ireland;
	}
}
