using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ProcessManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class IncidentClosureDisposition : CodeDescriptionBoolTreeNode
	{
		#region Schema

		protected new abstract class Schema : CodeDescriptionBoolTreeNode.Schema
		{
			public const string IsResolution = "IsResolution";
		}

		#endregion

		#region Properties

		public ZBool IsResolution
		{
			get { return isResolution; }
			set
			{
				SetNonPersistentPropertyValue(IsResolutionInfo, ref isResolution, value);
			}
		}
		ZBool isResolution;

		public ZPropertyInfo IsResolutionInfo
		{
			get { return GetZPropertyInfo(Schema.IsResolution); }
		}

		#endregion

		#region Xml Serialisation

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			IsResolution = new ZBool(reader.ReadElementString(Schema.IsResolution));
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.IsResolution, IsResolution.ToString());
		}

		#endregion

		#region Validation

		public const string ResolvedCode = "SLV";
		public const string ResolvedAndClosedCode = "CLS";

		protected override void ValidateCodeCore()
		{
			base.ValidateCodeCore();

			if (Code.EqualsIgnoringCase(ResolvedCode))
			{
				CodeInfo.AddError("This code cannot be used because it is reserved for the Resolved disposition.");
			}
			else if (Code.EqualsIgnoringCase(ResolvedAndClosedCode))
			{
				CodeInfo.AddError("This code cannot be used because it is reserved for the Closed disposition.");
			}
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new IncidentClosureDisposition();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var node = (IncidentClosureDisposition)clone;
			node.IsResolution = IsResolution;
		}

		#endregion
	}
}

