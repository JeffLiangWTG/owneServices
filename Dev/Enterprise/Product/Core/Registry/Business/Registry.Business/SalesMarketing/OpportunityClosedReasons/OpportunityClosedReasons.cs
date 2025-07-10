using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class OpportunityClosedReasons : CodeDescriptionBool
	{
		public OpportunityClosedReasons()
		{
		}

		public OpportunityClosedReasons(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		[ChildEditable]
		public CodeSelectionCollection StatusRules
		{
			get
			{
				if (statusRules == null)
				{
					statusRules = new CodeSelectionCollection(GetListProvider());
					RegisterEditableChildObject(statusRules);
				}
				return statusRules;
			}
		}

		CodeSelectionCollection statusRules;

		CodeDescriptionPairListProvider GetListProvider()
		{
			return new CodeDescriptionPairListProvider(() =>
			{
				var list = new CodeDescriptionPairList();
				list.AddRange(OrganisationsDataRegistry.Instance.OpportunityStatus.GetFallBackValueAtAllLevels(CurrentFallbackLevel != null ? CurrentFallbackLevel.CompanyPK(false) : Guid.Empty, Guid.Empty, Guid.Empty).OfType<OpportunityStatus>().Where(x => x.Enabled && x.Bool).ToList());
				return list;
			});
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			ZXmlSerializer.New(typeof(CodeSelectionCollection)).Serialize(writer, StatusRules);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			StatusRules.RemoveAll();
			StatusRules.AddRange(((CodeSelectionCollection)ZXmlSerializer.New(typeof(CodeSelectionCollection)).Deserialize(reader)).OfType<CodeSelection>().Select(x => x.Clone(CurrentFallbackLevel, Factory)));
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new OpportunityClosedReasons(fallbackLevel, factory);

		protected override void CopyCollectionsToClone(RegistryBusinessObjectTemplate clone, FallbackLevel currentFallbackLevel, BusinessObjectFactory factory)
		{
			var collection = ((OpportunityClosedReasons)clone).StatusRules;
			collection.RemoveAll();
			collection.AddRange(StatusRules.OfType<CodeSelection>().Select(x => x.Clone(currentFallbackLevel, factory)));
		}
	}
}
