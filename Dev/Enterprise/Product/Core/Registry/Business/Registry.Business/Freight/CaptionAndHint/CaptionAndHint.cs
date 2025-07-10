using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CaptionAndHint : RegistryBusinessObjectTemplate, ICaptionAndHint
	{
		#region Schema

		public abstract class Schema
		{
			public const string Caption = "Caption";
			public const string Hint = "Hint";
		}

		#endregion

		public CaptionAndHint()
		{
		}

		public CaptionAndHint(string caption, string hint)
		{
			this.Caption = caption;
			this.Hint = hint;
		}

		public CaptionAndHint(string caption, string hint, int captionMaxLength)
		{
			this.Caption = caption;
			this.Hint = hint;
			this.CaptionMaxLength = captionMaxLength;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CaptionAndHint("", "", CaptionMaxLength);
		}

		#region Caption

		[CargoWise.ComponentModel.MaxLength(256)]
		public ZString Caption
		{
			get { return caption; }
			set
			{
				CheckMaximumLength(CaptionInfo, value);
				SetNonPersistentPropertyValue<ZString>(CaptionInfo, ref caption, value);
			}
		}

		public ZPropertyInfo CaptionInfo
		{
			get { return GetZPropertyInfo(Schema.Caption); }
		}

		ZString caption;

		public int CaptionMaxLength
		{
			get { return captionMaxLength; }
			set { captionMaxLength = value; }
		}
		int captionMaxLength = 256;
		#endregion

		#region Hint

		[CargoWise.ComponentModel.MaxLength(256)]
		public ZString Hint
		{
			get { return hint; }
			set
			{
				CheckMaximumLength(HintInfo, value);
				SetNonPersistentPropertyValue<ZString>(HintInfo, ref hint, value);
			}
		}

		public ZPropertyInfo HintInfo
		{
			get { return GetZPropertyInfo(Schema.Hint); }
		}

		ZString hint;

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Caption, Caption);
			writer.WriteElementString(Schema.Hint, Hint);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Caption = reader.ReadElementString(Schema.Caption);
			Hint = reader.ReadElementString(Schema.Hint);
		}

		#endregion
	}
}
