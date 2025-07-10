
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public class GenPivotCollection : PivotBusinessObjectCollection<GenPivot>
	{
		public GenPivotCollection(BusinessObject master, string relationType, bool includeChildren = true, bool includeParents = true)
			: base(master, new GenPivotCollectionRelationship(master, relationType, includeChildren, includeParents), includeChildren, includeParents)
		{
			Argument.NotNull(relationType, "relationType");

			RelationType = relationType;
		}

		public string RelationType { get; }

		#region Add

		protected override void OnChildAdded(BusinessObject child, GenPivot pivot)
		{
			pivot.XX_Relation1TableCode = Master.TablePrefix;
			pivot.XX_Relation2TableCode = child.TablePrefix;
		}

		protected override void OnParentAdded(BusinessObject parent, GenPivot pivot)
		{
			pivot.XX_Relation1TableCode = parent.TablePrefix;
			pivot.XX_Relation2TableCode = Master.TablePrefix;
		}

		#endregion

		#region Implementation

		protected override void SetDefaultsForNewElementCore(GenPivot newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.XX_RelationType = RelationType;
		}

		protected override void SetAdditionalDefaultsForNewChild(GenPivot newElement)
		{
			base.SetAdditionalDefaultsForNewChild(newElement);
			newElement.XX_Relation1TableCode = Master.TablePrefix;
		}

		protected override void SetAdditionalDefaultsForNewParent(GenPivot newElement)
		{
			base.SetAdditionalDefaultsForNewParent(newElement);
			newElement.XX_Relation2TableCode = Master.TablePrefix;
		}

		#endregion
	}
}
