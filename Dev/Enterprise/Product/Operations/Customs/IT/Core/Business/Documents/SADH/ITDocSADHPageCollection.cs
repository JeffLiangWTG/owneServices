using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.EU;
using ITCusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.IT.Business.Declaration.CusEntryLine>;

namespace Enterprise.Customs.IT.Business;

public sealed class ITDocSADHPageCollection : DocSADHPageCollection
{
	public ITDocSADHPageCollection(ITCusEntryLineCollection collection, BusinessObjectFactory factory)
		: base(factory)
	{
		if (collection.Count > 0)
		{
			Add(ITDocSADHPage.New(factory, collection[0]));

			for (int index = 1; index < collection.Count; index += 3)
			{
				Add(ITDocSADHPage.New(factory, collection, index));
			}
		}
	}

	public new ITDocSADHPage AddNew() => (ITDocSADHPage)base.AddNew();

	public new ITDocSADHPage this[int index] => (ITDocSADHPage)base[index];
}
