using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class GlowTempOrgRequiredField : CodeDescriptionBoolDisallowNew
	{
		#region Schema

		new abstract class Schema : CodeDescriptionBool.Schema
		{
			public const string IsMandatory = "IsMandatory";
		}

		#endregion

		#region ReadOnlyRegistryItems

		readonly string[] BoolReadOnlyCodes = { (NoResString)"Name" };
		readonly string[] IsMandatoryReadOnlyCodes = { (NoResString)"Name", "Address2" };

		protected override bool BoolReadOnly
		{
			get { return BoolReadOnlyCodes.Contains(Code.ToString()); }
		}

		protected bool IsMandatoryReadOnly
		{
			get { return IsMandatoryReadOnlyCodes.Contains(Code.ToString()) || !Bool; }
		}

		#endregion

		[ReadOnlyMember(nameof(BoolReadOnly))]
		public override ZBool Bool
		{
			get { return base.Bool; }
			set
			{
				base.Bool = value;
				if (!value)
				{
					IsMandatory = false;
				}
			}
		}

		#region IsMandatory

		[ReadOnlyMember(nameof(IsMandatoryReadOnly))]
		public ZBool IsMandatory
		{
			get { return isMandatory; }
			set { SetNonPersistentPropertyValue(IsMandatoryInfo, ref isMandatory, value); }
		}

		public ZPropertyInfo IsMandatoryInfo => GetZPropertyInfo(Schema.IsMandatory);

		ZBool isMandatory;

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new GlowTempOrgRequiredField();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var glowTempOrgRequiredField = (GlowTempOrgRequiredField)clone;
			glowTempOrgRequiredField.IsMandatory = IsMandatory;
		}

		#region XML Serialisation

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			IsMandatory = new ZBool(reader.ReadElementString(Schema.IsMandatory));
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			writer.WriteElementString(Schema.IsMandatory, IsMandatory.ToString());
		}

		#endregion
	}
}
