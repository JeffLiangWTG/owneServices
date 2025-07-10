using System.Collections;
using System.Collections.Generic;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Business
{
	public class SupportingDocumentLookups : CusSupportingInfoLookups
	{
		public SupportingDocumentLookups(AutoCusSupportingInfo parent) : base(parent)
		{
		}

		public ICodeDescriptionPairList EDocList => new AvailableEDocList(new List<ZString>(), Parent.EDocCollections());

		public override ICollection CodeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var list = ZZRefCusCodeListCombined.Loader.Load(
					Factory,
					Core.Constants.CountryCodes.Israel,
					codeType: Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILDocumentType,
					ZDateTime.Today);
				result.AddRange(list);
				result.Sort();
				return result;
			}
		}

		protected new SupportingDocument Parent => (SupportingDocument)base.Parent;
	}
}
