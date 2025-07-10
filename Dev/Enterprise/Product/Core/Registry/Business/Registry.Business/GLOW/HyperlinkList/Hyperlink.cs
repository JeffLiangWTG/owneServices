using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class Hyperlink : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string Caption = "Caption";
			public const string Url = "Url";
		}

		#endregion

		#region Properties

		#region Caption

		public ZString Caption
		{
			get => caption;
			set
			{
				SetNonPersistentPropertyValue<ZString>(CaptionInfo, ref caption, value);
				if (!IsValidationSuspended)
				{
					ValidateCaption();
				}
			}
		}

		public ZPropertyInfo CaptionInfo => GetZPropertyInfo(Hyperlink.Schema.Caption);

		ZString caption;

		#endregion

		#region Url

		public ZString Url
		{
			get => url;
			set
			{
				SetNonPersistentPropertyValue<ZString>(UrlInfo, ref url, value);
				if (!IsValidationSuspended)
				{
					ValidateUrl();
				}
			}
		}

		public ZPropertyInfo UrlInfo => GetZPropertyInfo(Hyperlink.Schema.Url);

		ZString url;

		#endregion

		#endregion

		#region Validation

		ZString UrlIsMalformed => Res.GetString("54b51730-7fca-4521-acf6-882ce9797b19", "Please enter a valid URL.");

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateCaption();
			ValidateUrl();
		}

		protected void ValidateCaption()
		{
			CaptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CaptionInfo);
		}

		protected void ValidateUrl()
		{
			UrlInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(UrlInfo);
			if (UrlInfo.HasErrors())
			{
				return;
			}

			if (!Uri.TryCreate(Url, UriKind.Absolute, out var uriResult))
			{
				UrlInfo.AddError(UrlIsMalformed);
			}
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new Hyperlink();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			var hyperlink = (Hyperlink)clone;
			hyperlink.Caption = Caption;
			hyperlink.Url = Url;
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Hyperlink.Schema.Caption, Caption);
			writer.WriteElementString(Hyperlink.Schema.Url, Url);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Caption = reader.ReadElementString(Hyperlink.Schema.Caption);
			Url = reader.ReadElementString(Hyperlink.Schema.Url);
		}

		#endregion
	}
}
