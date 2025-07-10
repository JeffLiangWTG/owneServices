using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class DeclarationLevelPackingGroupCollection : BaseDeclarationLevelPackingGroupCollection
	{
		public DeclarationLevelPackingGroupCollection(AutoEUJobDeclaration declaration)
			: base(declaration)
		{
		}

		public new PackingGroup this[int index]
		{
			get { return (PackingGroup)base[index]; }
		}

		public new PackingGroup AddNew()
		{
			return (PackingGroup)base.AddNew();
		}

		protected new PackingGroup AddNew(Type type)
		{
			return (PackingGroup)base.AddNew(type);
		}

		protected override BusinessObject AddNewCore()
		{
			return AddNew(typeof(PackingGroup));
		}
	}
}
