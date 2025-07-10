using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public abstract class CategorisedWorkflowRelatedItemsCollection<TParent> : RegistryBusinessObjectCollection, ICodeDescriptionPairList
		where TParent : RegistryBusinessObject
	{
		protected CategorisedWorkflowRelatedItemsCollection()
		{
		}

		protected CategorisedWorkflowRelatedItemsCollection(bool initialiseWithWorkflowDescriptorList)
			: this(initialiseWithWorkflowDescriptorList ? WorkflowDataRegistryHelper.GetWorkflowDescriptorList() : new ReadOnlyCodeDescriptionPairList())
		{
			if (initialiseWithWorkflowDescriptorList)
			{
				isSynchronisedWithDescriptorList = true;
			}
		}

		protected CategorisedWorkflowRelatedItemsCollection(ReadOnlyCodeDescriptionPairList list)
			: base(list)
		{
		}

		#region Syncronisation with WorkflowDescriptorList

		bool isSynchronisedWithDescriptorList;

		public void SynchroniseWithWorkflowDescriptorList()
		{
			if (!isSynchronisedWithDescriptorList)
			{
				foreach (CodeDescriptionPair pair in WorkflowDataRegistryHelper.GetWorkflowDescriptorList())
				{
					CreateItemFromWorkflowCodeDescriptionPair(pair);
				}
				isSynchronisedWithDescriptorList = true;
			}
		}

		void CreateItemFromWorkflowCodeDescriptionPair(CodeDescriptionPair pair)
		{
			if (string.IsNullOrEmpty(pair.Code))
			{
				return;
			}

			CreateItemFromWorkflowCodeDescriptionPairCore(pair);
		}

		protected virtual void CreateItemFromWorkflowCodeDescriptionPairCore(CodeDescriptionPair pair)
		{
			var parent = FindByCode(pair.Code) ?? AddNew();
			parent.Code = pair.Code;
			parent.Description = pair.MultilingualDescription;
		}

		#endregion

		public new TParent this[int i]
		{
			get { return (TParent)base[i]; }
		}

		public new TParent AddNew()
		{
			return (TParent)base.AddNew();
		}

		protected override bool IgnoreCaseInCodes => true;

		#region ICodeDescriptionPairList Members

		bool ICodeDescriptionPairList.ContainsCode(object code)
		{
			return ContainsCode(code.ToString());
		}

		string ICodeDescriptionPairList.GetDescriptionFromCode(string code)
		{
			RegistryBusinessObject parent = FindByCode(code);
			return (parent != null) ? parent.Description : ZString.Empty;
		}

		#endregion

		#region For Testing
#if DEBUG

		public void MarkAsRequiringSyncronisationWithWorkflowDescriptorList_ForTest()
		{
			isSynchronisedWithDescriptorList = false;
		}

#endif
		#endregion
	}
}
