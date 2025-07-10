using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentVisualizer.Business
{
	/// <summary>
	/// This class was created to be used for Universal Copy
	/// </summary>
	public sealed class DocumentaryOverrides : NonPersistentBusinessObject
	{
		public DocumentaryOverrides(BusinessObject master)
			: base(master.Factory)
		{
			Parent = master;
		}

		public BusinessObject Parent { get; }

		public VisualizerDocumentData[] GetOverrides() => GetOverrides(Parent);

		public void ImportOverrides(BusinessObject bizObj)
		{
			if (bizObj == null
				|| bizObj == Parent)
			{
				return;
			}

			var bizObjDocumentaryOverrides = GetOverrides(bizObj);

			foreach (var documentData in bizObjDocumentaryOverrides)
			{
				var copy = Factory.New<VisualizerDocumentData>();
				copy.JDD_ParentID = Parent.PK;
				copy.JDD_ParentTableCode = documentData.JDD_ParentTableCode;
				copy.JDD_Name = documentData.JDD_Name;
				copy.JDD_OverriddenData = documentData.JDD_OverriddenData;
			}
		}

		VisualizerDocumentData[] GetOverrides(BusinessObject parent)
		{
			var filter = new ZQuery(JobDocumentDataSchema.JDD_ParentID, parent.PK);
			filter.AddToFilter(JobDocumentDataSchema.JDD_ParentTableCode, parent.TablePrefix);

			return Factory.Load<VisualizerDocumentData>(filter);
		}
	}
}