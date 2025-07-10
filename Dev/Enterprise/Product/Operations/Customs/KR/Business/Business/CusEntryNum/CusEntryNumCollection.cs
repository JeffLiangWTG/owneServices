using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class CusEntryNumCollection : BaseCusEntryNumCollection<CusEntryHeader>
	{
		public CusEntryNumCollection(CusEntryHeader entry)
			: base(entry)
		{
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			base.SetCollectionRelationships(dependent);
			((CusEntryNumber)dependent).CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			((CusEntryNumber)bizOAdded).Parent = Master;
			if (Master is CusEntryHeader header && header.Declaration != null)
			{
				((CusEntryNumber)bizOAdded).CE_IssueDateInfo.ValueChanged += new EventHandler((sender, e) => header.Declaration.RefreshEntryIssueDates());
			}
		}
	}
}
