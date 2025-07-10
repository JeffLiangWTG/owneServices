using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Accounting.Business.Res;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class EInvoicingReceivingFileTypeConfiguration : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string FileFormat = "FileFormat";
			public const string DebtorType = "DebtorType";
			public const string DebtorCode = "DebtorCode";
			public const string DebtorDescription = "DebtorDescription";
		}

		#endregion

		public string All => "ALL";

		#region FileFormat

		[List("FileFormatList")]
		public ZString FileFormat
		{
			get { return fileFormat; }
			set
			{
				SetNonPersistentPropertyValue(FileFormatInfo, ref fileFormat, value);

				if (!IsValidationSuspended)
				{
					ValidateFileFormat();
				}
			}
		}

		ZString fileFormat;

		public ZPropertyInfo FileFormatInfo
		{
			get { return GetZPropertyInfo(Schema.FileFormat); }
		}

		public CodeDescriptionPairList FileFormatList
		{
			get
			{
				if (fileFormatList == null)
				{
					fileFormatList = new CodeDescriptionPairList();
					fileFormatList.AddPair("OFD", Res.GetString("849FD5A3-4499-4E91-AE76-D4C64525A3C8", "OFD file"));
					fileFormatList.AddPair("XML", Res.GetString("F63A3436-0760-49DF-87C9-330FBBC11177", "XML file"));
				}

				return fileFormatList;
			}
		}

		CodeDescriptionPairList fileFormatList;

		public void ValidateFileFormat()
		{
			FileFormatInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(FileFormatInfo);
			ListValidation.ErrorIfInvalidCode(FileFormatInfo, FileFormatList);

			if (!FileFormatInfo.HasErrors())
			{
				var errorMessage = CheckHasAllConfigurations();
				if (!string.IsNullOrEmpty(errorMessage))
				{
					FileFormatInfo.AddError(errorMessage);
				}
			}
		}

		#endregion

		#region DebtorType

		[List("DebtorTypeList")]
		public virtual ZString DebtorType
		{
			get
			{
				return debtorType;
			}
			set
			{
				SetNonPersistentPropertyValue(DebtorTypeInfo, ref debtorType, value);

				if (!IsValidationSuspended)
				{
					ValidateDebtorType();
				}

				DebtorCode = ZGuid.Empty;
				DebtorCodeInfo.RefreshBinding();
			}
		}
		ZString debtorType;

		public ZPropertyInfo DebtorTypeInfo
		{
			get { return GetZPropertyInfo(Schema.DebtorType); }
		}

		public void ValidateDebtorType()
		{
			DebtorTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(DebtorTypeInfo);
			ListValidation.ErrorIfInvalidCode(DebtorTypeInfo, DebtorTypeList);

			if (!DebtorTypeInfo.HasErrors())
			{
				var errorMessage = CheckHasAllConfigurations();
				if (!string.IsNullOrEmpty(errorMessage))
				{
					DebtorTypeInfo.AddError(errorMessage);
				}
			}
		}

		public CodeDescriptionPairList DebtorTypeList
		{
			get
			{
				if (debtorTypeList == null)
				{
					debtorTypeList = new CodeDescriptionPairList(OLookUpEditType.DebtorTypes);
					debtorTypeList.Insert(0, new CodeDescriptionPair(All, All));
				}
				return debtorTypeList;
			}
		}
		CodeDescriptionPairList debtorTypeList;

		#endregion

		#region DebtorCode

		[List("DebtorCodes")]
		[ReadOnlyMember(nameof(DebtorCode_ReadOnly))]
		public ZGuid DebtorCode
		{
			get
			{
				return debtorCode;
			}
			set
			{
				var oldValue = DebtorCode;

				SetNonPersistentPropertyValue(DebtorCodeInfo, ref debtorCode, value);
				if (!IsValidationSuspended)
				{
					ValidateDebtorCode();
				}

				if (oldValue != DebtorCode)
				{
					debtor = null;
				}
			}
		}
		ZGuid debtorCode;

		public ZPropertyInfo DebtorCodeInfo
		{
			get { return GetZPropertyInfo(Schema.DebtorCode); }
		}

		public ZBool DebtorCode_ReadOnly => DebtorType == All;

		public void ValidateDebtorCode()
		{
			DebtorCodeInfo.ClearAllNotifications();

			if (debtorType != All)
			{
				MandatoryValidation.CheckEntered(DebtorCodeInfo);
				TypeValidation.CheckValidGuid(DebtorCodeInfo);

				if (!DebtorCodeInfo.HasErrors() && ParentCollection.Count > 0)
				{
					var orgHeader = Debtor as OrgHeader;
					if (ParentCollection.Cast<EInvoicingReceivingFileTypeConfiguration>()
						.Any(x => x.DebtorType == Core.Constants.DebtorTypes.Code.DebtorGroup && x.FileFormat == FileFormat && orgHeader?.CompanyData != null && x.DebtorCode == orgHeader.CompanyData.OB_OJ_ARDebtorGroup))
					{
						DebtorCodeInfo.AddError(ResString.GetMultilingualString("E3B4AE14-1052-4413-A78B-3CC7CC98F22C", "Debtor '{0}' is belong to the debtor group '{1}'.", orgHeader.OH_Code, orgHeader.CompanyData.ARDebtorGroup.OJ_Code));
					}
				}
			}
		}

		public BusinessObject Debtor
		{
			get
			{
				if (debtor == null)
				{
					if (DebtorType == Core.Constants.DebtorTypes.Code.DebtorOrganisation)
					{
						debtor = CurrentFactory.Load<OrgHeader>(DebtorCode);
					}
					else
					{
						debtor = CurrentFactory.Load<OrgDebtorGroup>(DebtorCode);
					}
				}

				return debtor;
			}
		}
		BusinessObject debtor;

		public BusinessObjectCollection DebtorCodes
		{
			get
			{
				BusinessObjectCollection debtorCodes;

				if (DebtorType == Core.Constants.DebtorTypes.Code.DebtorOrganisation)
				{
					debtorCodes = FindboxLookupCollections.GetDebtorCollection(CurrentFactory);
				}
				else
				{
					debtorCodes = FindboxLookupCollections.GetOrgDebtorGroupCollection(CurrentFactory);
				}

				return debtorCodes;
			}
		}

		#region DebtorDescription

		public ZString DebtorDescription
		{
			get
			{
				ZString debtorDescription = ZString.Empty;

				if (Debtor != null)
				{
					if (DebtorType == Core.Constants.DebtorTypes.Code.DebtorOrganisation)
					{
						debtorDescription = ((OrgHeader)Debtor).OH_FullName;
					}
					else
					{
						debtorDescription = ((OrgDebtorGroup)Debtor).OJ_Desc;
					}
				}

				return debtorDescription;
			}
		}

		#endregion

		void ValidateUniqueConfiguration()
		{
			ClearRowNotifications();
			if (ParentCollection.Count > 0)
			{
				if (ParentCollection.Cast<EInvoicingReceivingFileTypeConfiguration>().Count(x => x.DebtorType == DebtorType && x.FileFormat == FileFormat && x.DebtorCode == DebtorCode) > 1)
				{
					AddRowError(ResString.GetMultilingualString("F653A75F-9EC7-4ACF-AEF9-93004B4006CF", "Configuration must be unique."));
				}
			}
		}

		string CheckHasAllConfigurations()
		{
			var errorMessage = string.Empty;
			if (ParentCollection.Count > 0 && DebtorType != All)
			{
				if (ParentCollection.Cast<EInvoicingReceivingFileTypeConfiguration>().Any(x => x.DebtorType == All && x.FileFormat == FileFormat))
				{
					errorMessage = ResString.GetMultilingualString("68DEB7A7-1369-4848-8C70-71E5DFF35228", "A 'ALL' configuration already exists for file format '{0}'", FileFormat);
				}
			}

			return errorMessage;
		}

		#endregion

		public EInvoicingReceivingFileTypeConfigurationCollection ParentCollection
		{
			get
			{
				if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					return (EInvoicingReceivingFileTypeConfigurationCollection)((IBusinessObjectInternals)this).ParentCollections[0];
				}
				else
				{
					if (parentCollection == null)
					{
						parentCollection = new EInvoicingReceivingFileTypeConfigurationCollection();
					}
					return parentCollection;
				}
			}
		}

		EInvoicingReceivingFileTypeConfigurationCollection parentCollection;

		#region Override

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateFileFormat();
			ValidateDebtorType();
			ValidateDebtorCode();
			ValidateUniqueConfiguration();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EInvoicingReceivingFileTypeConfiguration();
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.FileFormat, FileFormat);
			writer.WriteElementString(Schema.DebtorType, DebtorType);
			writer.WriteElementString(Schema.DebtorCode, DebtorCode.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			FileFormat = reader.ReadElementString(Schema.FileFormat);
			DebtorType = reader.ReadElementString(Schema.DebtorType);
			DebtorCode = new ZGuid(reader.ReadElementString(Schema.DebtorCode));
		}

		#endregion
	}
}
