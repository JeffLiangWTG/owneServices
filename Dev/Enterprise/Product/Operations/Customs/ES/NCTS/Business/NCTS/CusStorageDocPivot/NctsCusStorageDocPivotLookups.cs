using System.Collections.Immutable;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsCusStorageDocPivotLookups : CusStorageDocPivotLookups
	{
		public NctsCusStorageDocPivotLookups(Customs.Business.AutoCusStorageDocPivot parent) : base(parent)
		{
		}

		protected override ImmutableArray<IStorageDocsBaseCollection> GetEDocCollections => (Parent.Parent as INctsCusStorageDocPivotParent)?.EDocCollections.ToImmutableArray() ?? ImmutableArray<IStorageDocsBaseCollection>.Empty;
	}
}
