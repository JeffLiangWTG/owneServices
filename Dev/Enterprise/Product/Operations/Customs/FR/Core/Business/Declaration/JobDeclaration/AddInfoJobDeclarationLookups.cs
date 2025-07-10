using System.Collections;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public partial class JobDeclarationLookups
	{
		public CodeDescriptionPairList ValuationBypassCodeList => Factory.GetCachedValue<ValuationBypassCodeList>();

		public override CodeDescriptionPairList DeferTypeList => Factory.GetCachedValue<VATProcedureList>();

		public override ICollection AgreedPlaceCodeList => Declaration.IsUCC6 ? (Declaration.ZG_AgreedPlaceCode.Length == 2 ? new RefCountryCollection(Factory) : new RefUNLOCOCollection(Factory)) : base.AgreedPlaceCodeList;

		protected override EU.Business.Declaration.InlandTransportCodeDescriptionPairListBuilder GetImportInlandTransportCodeDescriptionPairListBuilder(EU.Business.Declaration.JobDeclaration declaration) => new InlandTransportCodeDescriptionPairListBuilder(declaration);

		public CodeDescriptionPairList VatCanaList
		{
			get
			{
				var deferTypeCode = Parent.ZG_VATDeferType;
				return Factory.GetCachedValue(key: "FR.JobeclarationLookups.VatCanaList" + deferTypeCode, getValueDelegate: () =>
				{
					var vatCanaList = new CodeDescriptionPairList();

					switch (deferTypeCode)
					{
						case VATProcedureList.Codes._2:
							vatCanaList = new VatCanaForAI2List(Factory);
							break;
						case VATProcedureList.Codes.L:
							vatCanaList = new VatCanaForALTList(Factory);
							break;
						default:
							break;
					}
					vatCanaList.Sort();
					return vatCanaList;
				});
			}
		}
	}
}
