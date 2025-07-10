using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.RemoteDesktopServices;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class DepotAddressColorSound : RegistryBusinessObjectTemplate
	{
		#region Schema

		abstract class Schema
		{
			public const string Organisation = "Organisation";
			public const string Address = "Address";
			public const string Color = "Color";
			public const string MP3FileContent = "MP3FileContent";
			public const string MP3FileName = "MP3FileName";
		}

		#endregion

		public DepotAddressColorSound()
			: base()
		{
		}

		public DepotAddressColorSound(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DepotAddressColorSound(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region Organisation

		[List(nameof(OrganisationList))]
		public ZGuid Organisation
		{
			get { return organisation; }
			set
			{
				SetNonPersistentPropertyValue(OrganisationInfo, ref organisation, value);
				addressList = null;
				orgHeader = null;
				Address = ZGuid.Empty;
				if (!IsValidationSuspended)
				{
					ValidateOrganisation();
				}
			}
		}

		public ZPropertyInfo OrganisationInfo
		{
			get { return GetZPropertyInfo(Schema.Organisation); }
		}

		public void ValidateOrganisation()
		{
			OrganisationInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(OrganisationInfo);
			TypeValidation.CheckValidGuid(OrganisationInfo);
			ListValidation.ErrorIfInvalidPK(OrganisationInfo, OrganisationList);
			if (OrgHeader != null)
			{
				DepotAddressColorSoundValidation.ValidateOrgHasDepot(OrganisationInfo, OrgHeader);
			}
		}

		ZGuid organisation;

		#endregion

		#region Address

		[List(nameof(AddressList))]
		public ZGuid Address
		{
			get { return address; }
			set
			{
				SetNonPersistentPropertyValue(AddressInfo, ref address, value);
				if (!IsValidationSuspended)
				{
					ValidateAddress();
				}
			}
		}

		public ZPropertyInfo AddressInfo
		{
			get { return GetZPropertyInfo(Schema.Address); }
		}

		public void ValidateAddress()
		{
			AddressInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AddressInfo);
			ListValidation.ErrorIfInvalidPK(AddressInfo, AddressList);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(AddressInfo);
		}

		ZGuid address;

		#endregion

		#region Color

		[List(nameof(List))]
		[MaxLength(11)]
		public ZString Color
		{
			get { return color; }
			set
			{
				if (value.Length == 9 && !value.Contains(','))
				{
					value = value.Insert(3, ",").Insert(7, ",");
				}
				SetNonPersistentPropertyValue(ColorInfo, ref color, value);
				if (!IsValidationSuspended)
				{
					ValidateColor();
				}
			}
		}

		public ZPropertyInfo ColorInfo
		{
			get { return GetZPropertyInfo(Schema.Color); }
		}

		public void ValidateColor()
		{
			ColorInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ColorInfo);
			DepotAddressColorSoundValidation.ValidateRGBValue(ColorInfo);
		}

		ZString color;

		#endregion

		#region Sound

		#region MP3

		[List(nameof(List))]
		public ZString MP3FileName
		{
			get { return mp3FileName; }
			set
			{
				invalidFilePath = false;
				SetNonPersistentPropertyValue(MP3FileNameInfo, ref mp3FileName, FileName(value));

				if (value == ZString.Empty || value != FileName(value))
				{
					var mappedValue = value;
					if (value != ZString.Empty && !File.Exists(value) && !File.Exists(mappedValue = new ZString(ObjectFactory.Get<IMappedClientPath>().GetMappedPath(value))))
					{
						SetNonPersistentPropertyValue(MP3FileNameInfo, ref mp3FileName, value);
						invalidFilePath = true;
					}
					else
					{
						var newMP3FileContent = LoadFileContentFromPath(mappedValue);
						if (newMP3FileContent != MP3FileContent)
						{
							MP3FileContent = newMP3FileContent;
						}

						else
						{
							MP3FileNameInfo.RefreshBinding();
						}
					}

					if (!IsValidationSuspended)
					{
						ValidateMP3FileName();
					}
				}
			}
		}
		static string InvalidFilePathMessage
		{
			get
			{
				return Res.GetString("9EB59865-7D5D-4CB6-97BB-AE24E0D978ED", "File selected is not accessible.");
			}
		}
		bool invalidFilePath;

		public ZPropertyInfo MP3FileNameInfo
		{
			get { return GetZPropertyInfo(Schema.MP3FileName); }
		}

		public void ValidateMP3FileName()
		{
			ValidateSoundFileName(MP3FileNameInfo, MP3FileName);
		}

		ZString mp3FileName;

		public ZBlob MP3FileContent
		{
			get { return mp3FileContent; }
			set
			{
				SetNonPersistentPropertyValue(MP3FileContentInfo, ref mp3FileContent, value);
				if (!IsValidationSuspended)
				{
					ValidateMP3FileContent();
				}
			}
		}

		public ZPropertyInfo MP3FileContentInfo
		{
			get { return GetZPropertyInfo(Schema.MP3FileContent); }
		}

		public void ValidateMP3FileContent()
		{
			ValidateSoundFileContent(MP3FileContentInfo, MP3FileName);
		}

		ZBlob mp3FileContent;
		#endregion

		ZString FileName(ZString path)
		{
			return path.Substring(FileNameIndex(path));
		}

		ZBlob LoadFileContentFromPath(ZString path)
		{
			if (path != ZString.Empty && FileNameIndex(path) > 0)
			{
				return File.ReadAllBytes(path);
			}
			return ZBlob.Empty;
		}

		int FileNameIndex(ZString path)
		{
			if (path == ZString.Empty)
			{
				return 0;
			}

			for (int i = path.Length - 1; i >= 0; i--)
			{
				if (path[i] == '\\')
				{
					return i + 1;
				}
			}
			return 0;
		}

		void ValidateSoundFileName(ZPropertyInfo soundFileInfo, ZString otherSoundFileName)
		{
			soundFileInfo.ClearAllNotifications();
			if (invalidFilePath)
			{
				soundFileInfo.AddError(InvalidFilePathMessage);
			}
			if (!string.IsNullOrEmpty(otherSoundFileName))
			{
				MandatoryValidation.CheckEntered(soundFileInfo);
			}
		}

		void ValidateSoundFileContent(ZPropertyInfo soundFileContentInfo, ZString soundFileName)
		{
			soundFileContentInfo.ClearAllNotifications();
			if (!string.IsNullOrEmpty(soundFileName))
			{
				DepotAddressColorSoundValidation.ValidateSoundFileContent(soundFileContentInfo);
			}
		}

		#endregion

		#region Lookups

		public BusinessObjectCollection OrganisationList
		{
			get
			{
				if (orgList == null)
				{
					orgList = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IOrgHeaderCollection>(), new object[] { CurrentFactory });
					orgList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", (ZString)OrgConstants.FilterControl.SecondaryOrgType.Depot, false));
				}
				return orgList;
			}
		}

		BusinessObjectCollection orgList;

		IOrgHeader OrgHeader
		{
			get
			{
				return orgHeader ?? (orgHeader = (Organisation != ZGuid.Empty) ? CurrentFactory.LoadTop1<IOrgHeader>(new ZQuery(OrgHeaderSchema.PK, Organisation)) : null);
			}
		}

		IOrgHeader orgHeader;

		public CodeDescriptionPairList AddressList
		{
			get
			{
				if (addressList == null)
				{
					addressList = new CodeDescriptionPairList();
					if (OrgHeader != null && OrgHeader.PK != ZGuid.Empty)
					{
						var addresses = CurrentFactory.Load<IOrgAddress>(new ZQuery(OrgAddressSchema.OA_OH, OrgHeader.PK));
						foreach (var address in addresses)
						{
							addressList.AddPair(address.PK, address.OA_Code, address.OA_Address1);
						}
					}
				}
				return addressList;
			}
		}

		CodeDescriptionPairList addressList;

		List<ZString> List
		{
			get { return new List<ZString>(); }
		}

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DepotAddressColorSound(factory);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateOrganisation();
			ValidateAddress();
			ValidateColor();
			ValidateMP3FileContent();
			ValidateMP3FileName();
		}
		#endregion

		#region XML Serialization

		protected override void WriteElements(XmlWriter writer)
		{
			writer.WriteElementString(Schema.Organisation, Organisation.ToString());
			writer.WriteElementString(Schema.Address, Address.ToString());
			writer.WriteElementString(Schema.Color, Color);
			writer.WriteElementString(Schema.MP3FileName, MP3FileName);
			writer.WriteElementString(Schema.MP3FileContent, Convert.ToBase64String(MP3FileContent));
			base.WriteElements(writer);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			XmlReader readerObject = reader.Reader;
			while (readerObject.NodeType != XmlNodeType.EndElement)
			{
				switch (readerObject.LocalName)
				{
					case Schema.Organisation:
						ZGuid orgGuid;
						if (ZGuid.TryParse(readerObject.ReadElementString(), out orgGuid))
						{
							Organisation = orgGuid;
						}
						break;

					case Schema.Address:
						ZGuid addressGuid;
						if (ZGuid.TryParse(readerObject.ReadElementString(), out addressGuid))
						{
							Address = addressGuid;
						}
						break;

					case Schema.Color:
						Color = readerObject.ReadElementString();
						break;

					case Schema.MP3FileContent:
						MP3FileContent = Convert.FromBase64String(readerObject.ReadElementString());
						break;

					case Schema.MP3FileName:
						MP3FileName = readerObject.ReadElementString();
						break;

					default:
						readerObject.Read();
						break;
				}
			}
		}

		#endregion
	}
}
