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
	public class CriticalityStageMappingCollection : CodeDescriptionBoolTreeNodeCollection, ICodeDescriptionBoolTreeNodeCollectionExtension
	{
		public CriticalityStageMappingCollection()
			: this(true, 3, 4)
		{
		}

		public CriticalityStageMappingCollection(bool defaultBoolForNewChild, int codeMaxLength = 3, int maxDepth = 4, MultilingualString[] allDescriptions = null, CodeDescriptionPairList[] codeLists = null)
			: base(defaultBoolForNewChild, codeMaxLength, maxDepth, allDescriptions, codeLists)
		{
		}

		CriticalityStageMappingCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public new CriticalityStageMapping this[int i]
		{
			get { return (CriticalityStageMapping)base[i]; }
		}

		public new CriticalityStageMapping FindParent(string parentCode)
		{
			return (CriticalityStageMapping)base.FindParent(parentCode);
		}

		public new CriticalityStageMapping Find(params string[] codes)
		{
			return (CriticalityStageMapping)base.Find(codes);
		}

		public CriticalityStageMapping Add(ZString code, MultilingualString desc, bool boolValue, bool isDefault, bool isSystemDefined, CodeDescriptionBoolTreeNode parent)
		{
			var result = (CriticalityStageMapping)base.Add(code, desc, boolValue, isSystemDefined, parent);
			result.IsDefault = isDefault;
			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CriticalityStageMapping();
		}

		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			return new CriticalityStageMappingCollection(CurrentFallbackLevel);
		}

		public new CriticalityStageMapping AddNew()
		{
			return (CriticalityStageMapping)base.AddNew();
		}

		public CodeDescriptionBoolTreeView CreateView(ZGuid parentID)
		{
			return new CriticalityStageMappingCollectionView(this, parentID);
		}
	}
}

