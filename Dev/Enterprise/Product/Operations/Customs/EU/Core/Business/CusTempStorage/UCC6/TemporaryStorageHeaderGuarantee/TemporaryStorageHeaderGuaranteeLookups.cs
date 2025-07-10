using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageHeaderGuaranteeLookups : CommonGuaranteeLookups
	{
		public TemporaryStorageHeaderGuaranteeLookups(TemporaryStorageHeaderGuarantee parent) : base(parent)
		{
		}

		protected new TemporaryStorageHeaderGuarantee Parent => (TemporaryStorageHeaderGuarantee)base.Parent;

		protected override IReadOnlyList<ZString> GuaranteeTypeFilter => new ZString[] { Parent.TemporaryStorageHeader?.GuaranteeBondType ?? EUGuaranteeTypeList.Codes.COD };
	}
}
