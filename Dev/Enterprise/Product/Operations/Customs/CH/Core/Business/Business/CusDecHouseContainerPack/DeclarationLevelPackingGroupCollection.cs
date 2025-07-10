using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class DeclarationLevelPackingGroupCollection : BaseDeclarationLevelPackingGroupCollection
{
	public DeclarationLevelPackingGroupCollection(BaseJobDeclaration declaration) : base(declaration)
	{
	}

	public new PackingGroup this[int index] => (PackingGroup)base[index];

	public new PackingGroup AddNew() => (PackingGroup)base.AddNew();

	protected new PackingGroup AddNew(Type type) => (PackingGroup)base.AddNew(type);

	protected override BusinessObject AddNewCore() => AddNew(typeof(PackingGroup));
}
