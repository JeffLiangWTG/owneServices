using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class HVLVEnablePartyScreening : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string EnableHVLVPartyScreening = "EnableHVLVPartyScreening";
			public const string EnableNewDPSResultForm = "EnableNewDPSResultForm";
		}

		#endregion

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			EnableHVLVPartyScreening = false;
			EnableNewDPSResultForm = true;
		}

		#region Bound Properties

		#region EnableHVLVPartyScreening

		public ZBool EnableHVLVPartyScreening
		{
			get { return enableHVLVPartyScreening; }
			set
			{
				SetNonPersistentPropertyValue(EnableHVLVPartyScreeningInfo, ref enableHVLVPartyScreening, value);
			}
		}

		public ZPropertyInfo EnableHVLVPartyScreeningInfo
		{
			get { return GetZPropertyInfo(Schema.EnableHVLVPartyScreening); }
		}

		ZBool enableHVLVPartyScreening;

		#endregion

		#region EnableNewDPSResultForm

		public ZBool EnableNewDPSResultForm
		{
			get { return enableNewDPSResultForm; }
			set
			{
				SetNonPersistentPropertyValue(EnableNewDPSResultFormInfo, ref enableNewDPSResultForm, value);
			}
		}

		public ZPropertyInfo EnableNewDPSResultFormInfo
		{
			get { return GetZPropertyInfo(Schema.EnableNewDPSResultForm); }
		}

		ZBool enableNewDPSResultForm;

		#endregion

		#endregion

		#region Cloning

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new HVLVEnablePartyScreening();
		}

		#endregion

		#region XML Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.EnableHVLVPartyScreening, EnableHVLVPartyScreening.ToString());
			writer.WriteElementString(Schema.EnableNewDPSResultForm, EnableNewDPSResultForm.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			EnableHVLVPartyScreening = new ZBool(reader.ReadElementString(Schema.EnableHVLVPartyScreening));
			EnableNewDPSResultForm = new ZBool(reader.ReadElementString(Schema.EnableNewDPSResultForm));
		}

		#endregion
	}
}
