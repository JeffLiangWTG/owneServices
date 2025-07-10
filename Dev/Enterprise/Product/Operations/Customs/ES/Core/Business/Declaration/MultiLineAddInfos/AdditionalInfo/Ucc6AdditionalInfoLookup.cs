using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.Declaration
{
	sealed class Ucc6AdditionalInfoLookup : AdditionalInfoLookups
	{
		public Ucc6AdditionalInfoLookup(AdditionalInfo parent) : base(parent)
		{
		}

		public override ICollection CodeList
		{
			get
			{
				var isImport = (Parent.Parent as ICanBeImportOrExport)?.IsImport ?? false;
				var codeType = isImport ? AdditionalInfoLookupsHelper.GetImportRefCusCodeListType(Parent.CSI_SubType) : AdditionalInfoLookupsHelper.GetUCCExportRefCusCodeListType(Parent.CSI_SubType);

				if (codeType.IsEmpty)
				{
					return new CodeDescriptionPairList();
				}

				return Factory.GetCachedValue($"ES.{nameof(Ucc6AdditionalInfoLookup)}.{nameof(CodeList)}_{codeType}", () =>
					ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Spain, codeType, ZDateTime.Today));
			}
		}
	}
}
