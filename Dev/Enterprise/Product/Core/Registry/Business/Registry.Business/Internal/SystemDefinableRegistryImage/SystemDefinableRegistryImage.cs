using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business;

[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
public class SystemDefinableRegistryImage : RegistryImage
{
	#region Schema

	protected new abstract class Schema : RegistryImage.Schema
	{
		public const string SystemDefined = "SystemDefined";
	}

	#endregion

	#region Override

	[ReadOnlyMember(nameof(SystemDefined))]
	public override ZString Code
	{
		get { return base.Code; }
		set { base.Code = value; }
	}

	[ReadOnlyMember(nameof(SystemDefined))]
	public override ZString EnglishDescription
	{
		get { return base.EnglishDescription; }
		set { base.EnglishDescription = value; }
	}

	protected override bool IsCodeUniqueInCollection => false;

	protected override void ValidateCodeCore()
	{
		ClearRowNotifications();

		base.ValidateCodeCore();

		var collection = GetParentCollection(this, typeof(SystemDefinableRegistryImageCollection));

		if (collection != null)
		{
			var duplicates = collection.Cast<SystemDefinableRegistryImage>()
				.Where(x => x != this && x.Code.EqualsIgnoringCase(Code))
				.Take(2)
				.ToList();
			var duplicatesCount = duplicates.Count;

			if (duplicatesCount == 1 && SystemDefined == duplicates[0].SystemDefined
				|| duplicatesCount >= 2)
			{
				CodeInfo.AddError(Res.GetString("1D07F639-8AF8-4E96-B948-407A171C441F", "The code has been duplicated and must be unique. A maximum of one system image and one custom image is allowed."));
			}
		}
	}

	#endregion

	#region Clone

	protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
	{
		return new SystemDefinableRegistryImage();
	}

	protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
	{
		base.CopyValuesToClone(clone);
		SystemDefinableRegistryImage typeCastClone = (SystemDefinableRegistryImage)clone;
		typeCastClone.SystemDefined = SystemDefined;
	}

	#endregion

	#region SystemDefined

	public virtual ZBool SystemDefined { get; set; }

	#endregion

	#region Xml Serialisation

	protected override void WriteMoreElements(XmlWriter writer)
	{
		base.WriteMoreElements(writer);
		writer.WriteElementString(Schema.SystemDefined, SystemDefined.ToString());
	}

	protected override void ReadMoreElements(XmlReader reader)
	{
		base.ReadMoreElements(reader);
		SystemDefined = reader.IsStartElement(Schema.SystemDefined)
			? new ZBool(reader.ReadElementString(Schema.SystemDefined))
			: ZBool.False;
	}

	#endregion

	#region Implementation

	public override bool Equals(object obj)
	{
		var other = obj as SystemDefinableRegistryImage;

		return other != null
				&& other.SystemDefined == SystemDefined
				&& base.Equals(other);
	}

	public override int GetHashCode()
	{
		var hashCode = base.GetHashCode();
		return HashCodeHelper.GetCompositeHashCode(new object[] { hashCode, SystemDefined });
	}

	#endregion
}
