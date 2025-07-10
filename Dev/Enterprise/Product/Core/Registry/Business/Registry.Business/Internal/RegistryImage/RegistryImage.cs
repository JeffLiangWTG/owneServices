using System;
using System.Drawing;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class RegistryImage : RegistryBusinessObject
	{
		#region Schema

		protected new abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string ImagePK = "ImagePK";
			public const string FallbackKey = "FallbackKey";
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new RegistryImage();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			RegistryImage typeCastClone = (RegistryImage)clone;
			typeCastClone.DefaultImageResourceName = DefaultImageResourceName;
			typeCastClone.fallbackKeyInDb = fallbackKeyInDb;
			typeCastClone.imagePk = imagePk;

			if (image != null)
			{
				typeCastClone.image = (Image)image.Clone();
			}
		}

		#endregion

		#region Validation Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateImage();
		}

		protected void ValidateImage()
		{
			ClearRowNotifications();
			if (Image == null && !IsEmptyImageAllowed)
			{
				AddRowError(Res.GetString("24e2a448-8262-41f4-b68f-b19a05e86abb", "Please select an image."));
			}
		}

		protected virtual bool IsEmptyImageAllowed
		{
			get { return false; }
		}

		protected override void ValidateDescriptionCore()
		{
			MandatoryValidation.CheckEntered(DescriptionInfo, (IMultilingualString)ResString.GetMultilingualString("0d046884-46ac-49fb-93be-f239f4911af4", "Description"));
		}

		protected override int MaxDescriptionLength
		{
			get { return 50; }
		}

		#endregion

		#region Image

		public Image Image
		{
			get
			{
				if (!isImageSet && (image == null || image.IsDisposed()))
				{
					if (!defaultImageResourceName.IsEmpty)
					{
						Stream stream = GetType().Assembly.GetManifestResourceStream(DefaultImageResourceName);
						image = Image.FromStream(stream);
					}
					else if (!imagePk.IsEmpty)
					{
						image = FreightDataRegistry.Instance.RegistryImageContainer.GetValueWithoutFallback(imagePk.ToGuid(), Guid.Empty, Guid.Empty);
					}
				}

				return image;
			}
			set
			{
				if (image != value)
				{
					shouldSaveImage = true;
				}

				isImageSet = true;
				image = value;
				DefaultImageResourceName = "";
				RefreshBinding();

				if (!IsValidationSuspended)
				{
					ValidateImage();
				}
			}
		}

		internal void DeleteImage(string fallbackKey)
		{
			if (!imagePk.IsEmpty && (fallbackKey == fallbackKeyInDb))
			{
				((IRegistryItemInternals)FreightDataRegistry.Instance.RegistryImageContainer).DeleteValue(imagePk.ToGuid(), Guid.Empty, Guid.Empty);
			}
		}

		internal string FallbackKeyForSaving
		{
			get { return fallbackKeyForSaving; }
			set { fallbackKeyForSaving = value; }
		}

		internal ZString DefaultImageResourceName
		{
			get { return defaultImageResourceName; }
			set { defaultImageResourceName = value; }
		}

		bool isImageSet;
		bool shouldSaveImage;
		internal ZGuid imagePk;
		internal string fallbackKeyForSaving;
		internal string fallbackKeyInDb;
		ZString defaultImageResourceName;
		Image image;

		#endregion

		#region Xml Serialisation

		protected override void WriteMoreElements(XmlWriter writer)
		{
			bool hasFallbackKeyChanged = false;

			if (FallbackKeyForSaving != fallbackKeyInDb)
			{
				hasFallbackKeyChanged = true;
				fallbackKeyInDb = FallbackKeyForSaving;
			}

			if (shouldSaveImage || hasFallbackKeyChanged)
			{
				shouldSaveImage = false;

				if (Image == null)
				{
					if (!hasFallbackKeyChanged)
					{
						DeleteImage(FallbackKeyForSaving);
					}
					imagePk = Guid.Empty;
				}
				else
				{
					if (hasFallbackKeyChanged || imagePk.IsEmpty)
					{
						imagePk = Guid.NewGuid();
					}
					FreightDataRegistry.Instance.RegistryImageContainer.SetValue(imagePk.ToGuid(), Guid.Empty, Guid.Empty, Image);
				}
			}

			writer.WriteElementString(Schema.ImagePK, imagePk.ToString());
			writer.WriteElementString(Schema.FallbackKey, FallbackKeyForSaving);
		}

		protected override void ReadMoreElements(XmlReader reader)
		{
			imagePk = new ZGuid(reader.ReadElementString(Schema.ImagePK));
			fallbackKeyInDb = reader.ReadElementString(Schema.FallbackKey);
		}

		#endregion

		#region Implementation

		public override bool Equals(object obj)
		{
			var other = obj as RegistryImage;

			return other != null &&
					 other.Code == Code &&
					 other.Description.Equals(Description) &&
					 (other.fallbackKeyInDb == fallbackKeyInDb ||
					(string.IsNullOrEmpty(other.fallbackKeyInDb) && string.IsNullOrEmpty(fallbackKeyInDb))) &&
					 Utilities.IsImageEqual(other.Image, Image);
		}

		public override int GetHashCode()
		{
			var hashCode = HashCodeHelper.GetCompositeHashCode(new object[]
			{
				Code,
				Description,
				string.IsNullOrEmpty(fallbackKeyInDb) ? string.Empty : fallbackKeyInDb
			});

			return HashCodeHelper.GetCompositeHashCode(hashCode, GetImageHashCode());
		}

		int GetImageHashCode()
		{
			var img = Image;

			if (img != hashedImage)
			{
				imageHashCode = Utilities.GetImageContentsHashCode(img);
				hashedImage = img;
			}

			return imageHashCode;
		}

		int imageHashCode;
		Image hashedImage;

		#endregion

		#region Test
#if DEBUG
		internal ZGuid ImagePkForTest
		{
			get { return imagePk; }
			set { imagePk = value; }
		}

		internal string FallbackKeyInDbForTest
		{
			get { return fallbackKeyInDb; }
			set { fallbackKeyInDb = value; }
		}

#endif
		#endregion
	}
}
