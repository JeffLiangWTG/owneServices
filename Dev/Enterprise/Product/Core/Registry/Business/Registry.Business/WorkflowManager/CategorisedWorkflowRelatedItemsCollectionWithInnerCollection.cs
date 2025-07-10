using System.Linq;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public abstract class CategorisedWorkflowRelatedItemsCollectionWithInnerCollection<TParent, TInnerCollection, TElement> : CategorisedWorkflowRelatedItemsCollection<TParent>, ICategorisedWorkflowRelatedItemsCollection<TParent>
		where TParent : RegistryBusinessObject, ICategorisedRegistryBusinessObjectCollection
		where TInnerCollection : RegistryBusinessObjectCollection
		where TElement : RegistryBusinessObject
	{
		protected CategorisedWorkflowRelatedItemsCollectionWithInnerCollection()
		{
		}

		protected CategorisedWorkflowRelatedItemsCollectionWithInnerCollection(bool initialiseWithWorkflowDescriptorList)
			: base(initialiseWithWorkflowDescriptorList)
		{
			InitialiseDefaultValues();
		}

		protected CategorisedWorkflowRelatedItemsCollectionWithInnerCollection(ReadOnlyCodeDescriptionPairList list)
			: base(list)
		{
			InitialiseDefaultValues();
		}

		void InitialiseDefaultValues()
		{
			foreach (var parent in this.Cast<TParent>())
			{
				AddDefaultElement(parent.InnerCollection);
			}
		}

		protected override void CreateItemFromWorkflowCodeDescriptionPairCore(CodeDescriptionPair pair)
		{
			GetOrCreateInnerCollectionFromWorkflowCode(pair.Code, pair.MultilingualDescription);
		}

		protected TInnerCollection GetOrCreateInnerCollectionFromWorkflowCode(string code, MultilingualString description = null)
		{
			if (string.IsNullOrEmpty(code))
			{
				return null;
			}

			TInnerCollection result;

			lock (this)
			{
				var parent = (TParent)FindByCode(code);

				if (parent != null && parent.InnerCollection.Count > 0)
				{
					if (!string.IsNullOrEmpty(description))
					{
						parent.Description = description;
					}
					result = (TInnerCollection)parent.InnerCollection;
				}
				else
				{
					var newTypes = parent ?? AddNew();
					newTypes.Code = code;
					newTypes.Description = description;
					result = (TInnerCollection)newTypes.InnerCollection;
					AddDefaultElement(result);
				}
			}

			return result;
		}

		void AddDefaultElement(RegistryBusinessObjectCollection innerCollection)
		{
			var element = innerCollection.AddNew();
			AddDefaultElementCore(element);
		}

		protected abstract void AddDefaultElementCore(RegistryBusinessObject addedElement);

		protected TElement GetElement(string workflowType, string elementCode)
		{
			var taskTypes = GetOrCreateInnerCollectionFromWorkflowCode(workflowType);
			return taskTypes.Cast<TElement>().FirstOrDefault(t => t.Code == elementCode);
		}

		#region For Testing
#if DEBUG

		public RegistryBusinessObjectCollection GetOrCreateInnerCollectionFromWorkflowCode_ForTest(string code) => GetOrCreateInnerCollectionFromWorkflowCode(code);

#endif
		#endregion
	}
}
