using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ResourceStrings.Business
{
	[CodeProperty("FullLanguageCode")]
	public class RefLocalLanguage : AutoRefLocalLanguage, IRefLocalLanguage, IDocManagerSupport, IObsoleteValidation
	{
		public RefLocalLanguage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[ReadOnlyMember(Schema.RA_IsSystem)]
		public override ZString RA_Code
		{
			get => base.RA_Code;
			set
			{
				if (base.RA_Code != value)
				{
					base.RA_Code = value;
					if (string.IsNullOrEmpty(ParentLanguageCode))
					{
						foreach (ICodeDescription language in Lookups.LocalLanguagesList)
						{
							if (language.Code.Split('-')[0] == value)
							{
								ParentLanguageCode = language.Code;
							}
						}
					}
					FullLanguageCodeInfo.RefreshBinding();
				}
			}
		}

		[ReadOnlyMember(Schema.RA_IsSystem)]
		public override ZGuid RA_RA_ParentLanguage
		{
			get => base.RA_RA_ParentLanguage;
			set
			{
				base.RA_RA_ParentLanguage = value;
				var parentLanguage = Lookups.LocalLanguagesList[value];
				parentLanguageCode = parentLanguage == null ? string.Empty : parentLanguage.Code;
			}
		}

		ZString parentLanguageCode;
		[List("Lookups.LocalLanguagesList")]
		[ReadOnlyMember(Schema.RA_IsSystem)]
		public ZString ParentLanguageCode
		{
			get
			{
				if (string.IsNullOrEmpty(parentLanguageCode))
				{
					var parentLanguage = Lookups.LocalLanguagesList[RA_RA_ParentLanguage];
					parentLanguageCode = parentLanguage == null ? string.Empty : parentLanguage.Code;
				}
				return parentLanguageCode;
			}
			set
			{
				SetNonPersistentPropertyValue(ParentLanguageCodeInfo, ref parentLanguageCode, value);
				var parentLanguage = Lookups.LocalLanguagesList[parentLanguageCode, StringComparison.OrdinalIgnoreCase];
				SetPropertyValue(RA_RA_ParentLanguageInfo, parentLanguage == null ? ZGuid.Empty : (ZGuid)parentLanguage.PK);
				ParentLanguageCodeInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateParentLanguageCode();
				}
			}
		}

		public ZPropertyInfo ParentLanguageCodeInfo => GetZPropertyInfo(nameof(ParentLanguageCode));

		[TranslatableDataField(RefLocalLanguageSchema.Constants.TableName, RefLocalLanguageSchema.Constants.RA_Description, @"Database\Odyssey\Data\Public\RefLocalLanguage\RefLocalLanguage.xml", Type = typeof(RefLocalLanguage), Asmid = Res.AssemblyId)]
		public override ZString RA_Description
		{
			get => base.RA_Description;
			set => base.RA_Description = value;
		}

		public bool RA_Description_ReadOnly
		{
			get { return RA_IsSystem && !GlbStaff.CurrentUser.GS_IsController; }
		}

		public MultilingualString RA_DescriptionMultilingual => GetMultilingual(RA_DescriptionInfo);

		[ReadOnlyMember(Schema.RA_IsSystem)]
		public override ZString RA_RN_NKCountryCode
		{
			get => base.RA_RN_NKCountryCode;
			set
			{
				base.RA_RN_NKCountryCode = value;
				FullLanguageCodeInfo.RefreshBinding();
			}
		}

		public bool RA_IsSystem_ReadOnly
		{
			get { return true; }
		}

		[ReadOnlyMember(Schema.RA_IsSystem)]
		public override ZBool RA_IsActive
		{
			get => base.RA_IsActive;
			set => base.RA_IsActive = value;
		}

		public ZString FullLanguageCode => RA_Code + (RA_RN_NKCountryCode.IsEmpty ? "" : "-" + RA_RN_NKCountryCode);

		public ZPropertyInfo FullLanguageCodeInfo => GetZPropertyInfo(nameof(FullLanguageCode));

		public IRefLocalLanguage ParentLanguage => Factory.Load<RefLocalLanguage>(RA_RA_ParentLanguage);

		public ResourceLanguage GetResourceLanguage()
		{
			return new ResourceLanguage(this.FullLanguageCode, this.ParentLanguage?.GetResourceLanguage());
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.LocalLanguage);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion
	}
}
