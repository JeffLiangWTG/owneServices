using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.SDF
{
	public class StmSystemDefinedFieldDependentCollection : BusinessObjectCollection<StmSystemDefinedField>
	{
		public StmSystemDefinedFieldDependentCollection(IDocumentSupportable documentSupportable, BusinessObjectFactory factory)
			: base(factory)
		{
			this.DocumentSupportable = documentSupportable;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			StmSystemDefinedField newField = (StmSystemDefinedField)child;
			newField.S1_BusinessContext = BusinessContext.ToString();
			ZShort largestOrder = 0;

			foreach (StmSystemDefinedField otherField in this)
			{
				if (otherField.S1_Order > largestOrder)
				{
					largestOrder = otherField.S1_Order;
				}
			}

			newField.S1_Order = largestOrder + 1;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(StmSystemDefinedFieldSchema.S1_BusinessContext, BusinessContext);
			result.AddToFilter(StmSystemDefinedFieldSchema.S1_RN_NKCntrySpecific, ZString.Empty);
			result.AddToFilter(StmSystemDefinedFieldSchema.S1_OrderColumn, ZShort.Zero);
			return result;
		}

		BusinessContext BusinessContext
		{
			get { return DocumentSupportable.DocumentSupporter.BusinessContext; }
		}

		internal readonly IDocumentSupportable DocumentSupportable;
	}
}
