using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class CusStorageDocPivotLookups : Customs.Business.CusStorageDocPivotLookups
	{
		public CusStorageDocPivotLookups(AutoCusStorageDocPivot parent) : base(parent)
		{
		}

		protected new CusStorageDocPivot Parent => (CusStorageDocPivot)base.Parent;

		public AvailableEDocList AvailableEDocs
		{
			get
			{
				var extensionFilter = new List<ZString>();
				var eDocCollections = GetEDocCollections;
				return new AvailableEDocList(extensionFilter, eDocCollections.ToArray());
			}
		}

		protected virtual ImmutableArray<IStorageDocsBaseCollection> GetEDocCollections => (Parent.Parent as ICusStorageDocPivotParent)?.EDocCollections.ToImmutableArray() ?? ImmutableArray<IStorageDocsBaseCollection>.Empty;
	}
}
