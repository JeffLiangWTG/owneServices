using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsPackingSlipLineCollection : DocumentWrapperCollection, IPackingSlipWrapperCollection<DocWhsPackingSlipLine>
	{
		public DocWhsPackingSlipLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocWhsPackingSlipLineCollection(IEnumerable<KeyValuePair<WhsReleaseLine, WhsPickableDocketLine>> collectionSource, BusinessObjectFactory factory)
			: base(collectionSource, factory)
		{
		}

		public new DocWhsPackingSlipLine this[int index]
		{
			get { return (DocWhsPackingSlipLine)base[index]; }
		}

		protected override DocumentWrapper WrapObject(object objectToWrap)
		{
			var releaseAndOrderLinePair = (KeyValuePair<WhsReleaseLine, WhsPickableDocketLine>)objectToWrap;
			return DocWhsPackingSlipLine.New(releaseAndOrderLinePair.Key, releaseAndOrderLinePair.Value, Factory);
		}

		// interfaces

		#region IPackingSlipWrapperCollection Members

		void IPackingSlipWrapperCollection<DocWhsPackingSlipLine>.Add(DocWhsPackingSlipLine packingSlipWrapper)
		{
			Add(packingSlipWrapper);
		}

		IEnumerable<DocWhsPackingSlipLine> IPackingSlipWrapperCollection<DocWhsPackingSlipLine>.Wrappers
		{
			get
			{
				foreach (DocWhsPackingSlipLine wrapper in this)
				{
					yield return wrapper;
				}
			}
		}

		#endregion
	}
}
