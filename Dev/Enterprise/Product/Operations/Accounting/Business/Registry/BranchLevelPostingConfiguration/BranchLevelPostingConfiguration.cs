using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class BranchLevelPostingConfiguration : RegistryBusinessObjectTemplate
	{
		public BranchLevelPostingConfiguration()
		{
		}

		public BranchLevelPostingConfiguration(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region Properties
		[ResourceStringData("BranchLevelPostingConfiguration|EnableBranchLevelPosting", Caption = "Enable Branch Level Posting")]
		public ZBool EnableBranchLevelPosting
		{
			get { return enableBranchLevelPosting; }
			set
			{
				SetNonPersistentPropertyValue(EnableBranchLevelPostingInfo, ref enableBranchLevelPosting, value);
			}
		}

		public ZPropertyInfo EnableBranchLevelPostingInfo
		{
			get { return GetZPropertyInfo(nameof(EnableBranchLevelPosting)); }
		}

		ZBool enableBranchLevelPosting;

		public BranchGroupSettingsCollection BranchGroupSettingsCollection
		{
			get
			{
				if (branchGroupSettingsCollection == null)
				{
					branchGroupSettingsCollection = new BranchGroupSettingsCollection();
					RegisterEditableChildObject(branchGroupSettingsCollection);
				}

				return branchGroupSettingsCollection;
			}
		}
		BranchGroupSettingsCollection branchGroupSettingsCollection;

		#endregion

		ZXmlSerializer BranchGroupSettingsCollectionSerializer => branchGroupSettingsCollectionSerializer ?? (branchGroupSettingsCollectionSerializer = ZXmlSerializer.New(typeof(BranchGroupSettingsCollection)));
		ZXmlSerializer branchGroupSettingsCollectionSerializer;

		#region Overrides
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BranchLevelPostingConfiguration(fallbackLevel, factory);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			BranchLevelPostingConfiguration castedClone = (BranchLevelPostingConfiguration)clone;
			if (branchGroupSettingsCollection != null)
			{
				castedClone.enableBranchLevelPosting = castedClone.EnableBranchLevelPosting;
				castedClone.branchGroupSettingsCollection = (BranchGroupSettingsCollection)BranchGroupSettingsCollection.Clone(castedClone.CurrentFallbackLevel, castedClone.Factory);
				castedClone.RegisterEditableChildObject(castedClone.BranchGroupSettingsCollection);
			}
		}

		#endregion

		#region Validation

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(nameof(EnableBranchLevelPosting), EnableBranchLevelPosting.ToString());
			BranchGroupSettingsCollectionSerializer.Serialize(writer, BranchGroupSettingsCollection);
		}
		protected override void ReadElements(XmlReaderWrapper reader)
		{
			EnableBranchLevelPosting = new ZBool(reader.ReadElementString(nameof(EnableBranchLevelPosting)));
			branchGroupSettingsCollection = (BranchGroupSettingsCollection)BranchGroupSettingsCollectionSerializer.Deserialize(reader);
			RegisterEditableChildObject(branchGroupSettingsCollection);
		}

		#endregion
	}
}
