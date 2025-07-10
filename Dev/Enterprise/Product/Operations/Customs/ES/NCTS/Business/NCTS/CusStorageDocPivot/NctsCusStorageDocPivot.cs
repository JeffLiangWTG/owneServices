using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsCusStorageDocPivot : CusStorageDocPivot
	{
		public NctsCusStorageDocPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get
			{
				var result = new TypeLoaderCollection();

				result.Add(ObjectFactory.GetType<Integration.Customs.ES.ICusInBondHeader>());

				return result;
			}
		}

		protected override Customs.Business.CusStorageDocPivotLookups GetNewLookups()
		{
			return new NctsCusStorageDocPivotLookups(this);
		}

		public new NctsCusStorageDocPivotLookups Lookups => (NctsCusStorageDocPivotLookups)base.Lookups;

		public NctsHeader NctsHeader => Parent as NctsHeader;

		public override CusStorageDocPivotEDIMessageCollection Messages => messages ?? (messages = NctsHeader != null ? new CusStorageDocPivotEDIMessageCollection(this, NctsHeader.Messages) : base.Messages);
		CusStorageDocPivotEDIMessageCollection messages;

		public override bool ReadOnly
		{
			get => NctsHeader != null && (IsAccepted || IsSentWithoutResponse);
			set => base.ReadOnly = value;
		}
	}
}
