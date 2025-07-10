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
	public class IncidentClosureDispositionCollection : CodeDescriptionBoolTreeNodeCollection
	{
		public IncidentClosureDispositionCollection()
			: this(true, 3, 4)
		{
		}

		public IncidentClosureDispositionCollection(bool defaultBoolForNewChild, int codeMaxLength = 3, int maxDepth = 4, MultilingualString[] allDescriptions = null, CodeDescriptionPairList[] codeLists = null)
			: base(defaultBoolForNewChild, codeMaxLength, maxDepth, allDescriptions, codeLists)
		{
		}

		IncidentClosureDispositionCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public override bool ReadOnly => false;

		protected override bool AllowNewCore => true;

		public new IncidentClosureDisposition this[int i]
		{
			get { return (IncidentClosureDisposition)base[i]; }
		}

		public new IncidentClosureDisposition FindParent(string parentCode)
		{
			return (IncidentClosureDisposition)base.FindParent(parentCode);
		}

		public new IncidentClosureDisposition Find(params string[] codes)
		{
			return (IncidentClosureDisposition)base.Find(codes);
		}

		public IncidentClosureDisposition Add(ZString code, MultilingualString desc, bool boolValue, bool isSystemDefined, CodeDescriptionBoolTreeNode parent, ZBool isResolution)
		{
			var result = (IncidentClosureDisposition)base.Add(code, desc, boolValue, isSystemDefined, parent);
			result.IsResolution = isResolution;
			return result;
		}

		public IncidentClosureDisposition Add(ZString code, MultilingualString desc, CodeDescriptionBoolTreeNode parent, ZBool isResolution)
		{
			var result = (IncidentClosureDisposition)base.Add(code, desc, parent);
			result.IsResolution = isResolution;
			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new IncidentClosureDisposition();
		}

		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			return new IncidentClosureDispositionCollection(CurrentFallbackLevel);
		}

		public new IncidentClosureDisposition AddNew()
		{
			return (IncidentClosureDisposition)base.AddNew();
		}
	}
}

