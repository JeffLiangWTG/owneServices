using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ProcessManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class ResolutionAndClosureBehaviourCollection : CodeDescriptionBoolTreeNodeCollection, ICodeDescriptionBoolTreeNodeCollectionExtension
	{
		public ResolutionAndClosureBehaviourCollection() : this(true, 3, 4) { }

		public ResolutionAndClosureBehaviourCollection(bool defaultBoolForNewChild, int codeMaxLength = 3, int maxDepth = 4, MultilingualString[] allDescriptions = null, CodeDescriptionPairList[] codeLists = null)
			: base(defaultBoolForNewChild, codeMaxLength, maxDepth, allDescriptions, codeLists)
		{
		}

		ResolutionAndClosureBehaviourCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public new ResolutionAndClosureBehaviour this[int i] => (ResolutionAndClosureBehaviour)Elements[i];

		public new ResolutionAndClosureBehaviour FindParent(string parentCode) => (ResolutionAndClosureBehaviour)base.FindParent(parentCode);

		public new ResolutionAndClosureBehaviour Find(params string[] codes) => (ResolutionAndClosureBehaviour)base.Find(codes);

		public new ResolutionAndClosureBehaviour AddNew() => (ResolutionAndClosureBehaviour)base.AddNew();

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ResolutionAndClosureBehaviour();
		}

		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			return new ResolutionAndClosureBehaviourCollection(CurrentFallbackLevel);
		}

		public CodeDescriptionBoolTreeView CreateView(ZGuid parentID)
		{
			return new ResolutionAndClosureBehaviourCollectionView(this, parentID);
		}
	}
}

