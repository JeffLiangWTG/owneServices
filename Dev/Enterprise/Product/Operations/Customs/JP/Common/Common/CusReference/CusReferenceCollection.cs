using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Common
{
	public class CusReferenceCollection<T> : DependentBusinessObjectCollection<T, BusinessObject> where T : CusReference
	{
		public CusReferenceCollection(BusinessObject parent, ZString type) : base(parent, SetAdditionalFilter(Argument.NotNullOrEmpty(type, nameof(type))))
		{
			this.type = type;
		}

		readonly ZString type;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var reference = child as CusReference;
			if (reference != null)
			{
				reference.CFR_Type = type;
			}
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CusReferenceSchema.CFR_ParentID;

		static ZQuery SetAdditionalFilter(ZString type) => new (CusReferenceSchema.CFR_Type, type);
	}
}
