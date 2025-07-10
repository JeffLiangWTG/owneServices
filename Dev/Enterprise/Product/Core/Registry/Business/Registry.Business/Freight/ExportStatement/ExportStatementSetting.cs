using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ExportStatementSetting : RegistryBusinessObjectTemplate, IDisposable
	{
		public ExportStatementSetting()
		{
		}

		public ExportStatementSetting(CountryExportStatementSetting parent)
		{
			this.Parent = parent;
		}

		public ExportStatementSetting(CountryExportStatementSetting parent, string code,
			string statement,
			string statementDescription,
			string field1,
			string field2,
			string visibility,
			bool useOnHawb,
			bool useOnDirectIATAMawb,
			bool useOnConsolidationMawb,
			bool useOnHouseBillOfLading,
			bool useOnDirectMasterBillOfLading,
			bool useOnConsolidationMasterBillOfLading)
			: this(parent)
		{
			using (GetValidationSuspender())
			{
				this.Code = code;
				this.Statement = statement;
				this.StatementDescription = statementDescription;
				this.Field1 = field1;
				this.Field2 = field2;
				this.Visibility = visibility;
				this.UseOnHawb = useOnHawb;
				this.UseOnDirectIATAMawb = useOnDirectIATAMawb;
				this.UseOnConsolidationMawb = useOnConsolidationMawb;
				this.UseOnHouseBillOfLading = useOnHouseBillOfLading;
				this.UseOnDirectMasterBillOfLading = useOnDirectMasterBillOfLading;
				this.UseOnConsolidationMasterBillOfLading = useOnConsolidationMasterBillOfLading;
			}
		}

		#region Schema

		public static class Schema
		{
			public const string Code = "Code";
			public const string Statement = "Statement";
			public const string Field1 = "Field1";
			public const string Field1Description = "Field1Description";
			public const string Field2 = "Field2";
			public const string Field2Description = "Field2Description";
			public const string Visibility = "Visibility";
			public const string VisibilityDescription = "VisibilityDescription";
			public const string UseOnHawb = "UseOnHawb";
			public const string UseOnDirectIATAMawb = "UseOnDirectIATAMawb";
			public const string UseOnConsolidationMawb = "UseOnConsolidationMawb";
			public const string UseOnHouseBillOfLading = "UseOnHouseBillOfLading";
			public const string UseOnDirectMasterBillOfLading = "UseOnDirectMasterBillOfLading";
			public const string UseOnConsolidationMasterBillOfLading = "UseOnConsolidationMasterBillOfLading";
			public const string StatementDescription = "StatementDescription";

			public const string TableName = "ExportStatementSetting";
		}

		#endregion

		#region Parent
		public CountryExportStatementSetting Parent
		{
			get { return fParent; }
			internal set
			{
				if (fParent != value)
				{
					if (fParent != null)
					{
						fParent.CountryCodeInfo.ValueChanged -= new EventHandler(CountryCodeInfo_ValueChanged);
					}
					fParent = value;
					fStatementFieldTypeList = null;
					if (fParent != null)
					{
						fParent.CountryCodeInfo.ValueChanged += new EventHandler(CountryCodeInfo_ValueChanged);
					}
				}
			}
		}
		CountryExportStatementSetting fParent;
		#endregion

		#region Bound Properties

		#region Code

		[MaxLength(3)]
		public ZString Code
		{
			get { return fCode; }
			set
			{
				CheckMaximumLength(CodeInfo, value);
				SetNonPersistentPropertyValue<ZString>(CodeInfo, ref fCode, value);
				if (!IsValidationSuspended)
				{
					ValidateCode();
					ValidateRecord();
				}
			}
		}
		ZString fCode;

		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(Schema.Code); }
		}

		public void ValidateCode()
		{
			CodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CodeInfo, (IMultilingualString)ResString.GetMultilingualString("D9322016-F397-4FB7-9E32-AA47DF2CA44D", "Code"));
			if (!CodeInfo.HasErrors())
			{
				CheckDuplicateCodes();
			}
		}

		public bool Code_ReadOnly => IsUsaOrTerritory;

		#endregion

		#region Statement

		[MaxLength(StatementMaxLength)]
		public ZString Statement
		{
			get { return fStatement; }
			set
			{
				CheckMaximumLength(StatementInfo, value);
				SetNonPersistentPropertyValue<ZString>(StatementInfo, ref fStatement, value);
				if (!IsValidationSuspended)
				{
					ValidateStatement();
					ValidateRecord();
				}
			}
		}
		ZString fStatement;

		public ZPropertyInfo StatementInfo
		{
			get { return GetZPropertyInfo(Schema.Statement); }
		}

		public void ValidateStatement()
		{
			StatementInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(StatementInfo, (IMultilingualString)ResString.GetMultilingualString("f1b61a0e-32be-424f-aea1-62d78e642582", "Statement"));
		}

		const int StatementMaxLength = 300;

		public bool Statement_ReadOnly => IsUsaOrTerritory;

		#endregion

		#region StatementDescription

		[BusinessObjectMaxLengthTestExclude]
		[MaxLength(StatementMaxLength)]
		public ZString StatementDescription
		{
			get
			{
				ZString result = fStatementDescription;
				if (result.IsEmpty)
				{
					result = Statement;
				}
				return result;
			}
			set
			{
				ZString newValue = value;
				if (newValue == Statement)
				{
					newValue = ZString.Empty;
				}
				CheckMaximumLength(StatementDescriptionInfo, newValue);
				SetNonPersistentPropertyValue<ZString>(StatementDescriptionInfo, ref fStatementDescription, newValue);
				if (!IsValidationSuspended)
				{
					ValidateRecord();
				}
			}
		}
		ZString fStatementDescription;

		public ZPropertyInfo StatementDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.StatementDescription); }
		}

		public bool StatementDescription_ReadOnly => IsUsaOrTerritory;

		#endregion

		#region Visibility

		[MaxLength(3)]
		public ZString Visibility
		{
			get { return fVisibility; }
			set
			{
				CheckMaximumLength(VisibilityInfo, value);
				SetNonPersistentPropertyValue<ZString>(VisibilityInfo, ref fVisibility, value);
				if (!IsValidationSuspended)
				{
					ValidateVisibility();
				}
				RedefaultOtherSettings();
			}
		}
		ZString fVisibility;

		public ZPropertyInfo VisibilityInfo
		{
			get { return GetZPropertyInfo(Schema.Visibility); }
		}

		public void ValidateVisibility()
		{
			VisibilityInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(VisibilityInfo, VisibilityTypeList);
			MandatoryValidation.CheckEntered(VisibilityInfo, (IMultilingualString)ResString.GetMultilingualString("fc0cac59-b412-4aea-8942-123baeed12ca", "Visibility"));
		}

		void RedefaultOtherSettings()
		{
			if (Visibility == VisibilityList.Mandatory && IsUsaOrTerritory && IsInUsReferenceSet)
			{
				UseOnHawb = true;
				UseOnDirectIATAMawb = true;
				UseOnConsolidationMawb = true;
				UseOnHouseBillOfLading = true;
				UseOnDirectMasterBillOfLading = true;
				UseOnConsolidationMasterBillOfLading = true;
			}
		}

		#endregion

		#region VisibilityDescription

		public ZString VisibilityDescription
		{
			get { return StatementFieldTypeList.GetDescriptionFromCode(Visibility); }
		}

		public ZPropertyInfo VisibilityDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.VisibilityDescription); }
		}

		#endregion

		#region Field1

		[MaxLength(3)]
		public ZString Field1
		{
			get { return fField1; }
			set
			{
				CheckMaximumLength(Field1Info, value);
				SetNonPersistentPropertyValue<ZString>(Field1Info, ref fField1, value);
				if (!IsValidationSuspended)
				{
					ValidateField1();
				}
			}
		}
		ZString fField1;

		public ZPropertyInfo Field1Info
		{
			get { return GetZPropertyInfo(Schema.Field1); }
		}

		public void ValidateField1()
		{
			Field1Info.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(Field1Info, StatementFieldTypeList);
		}

		#endregion

		#region Field1Description

		public ZString Field1Description
		{
			get { return StatementFieldTypeList.GetDescriptionFromCode(Field1); }
		}

		public ZPropertyInfo Field1DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Field1Description); }
		}

		#endregion

		#region Field2

		[MaxLength(3)]
		public ZString Field2
		{
			get { return fField2; }
			set
			{
				CheckMaximumLength(Field2Info, value);
				SetNonPersistentPropertyValue<ZString>(Field2Info, ref fField2, value);
				if (!IsValidationSuspended)
				{
					ValidateField2();
				}
			}
		}
		ZString fField2;

		public ZPropertyInfo Field2Info
		{
			get { return GetZPropertyInfo(Schema.Field2); }
		}

		public void ValidateField2()
		{
			Field2Info.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(Field2Info, StatementFieldTypeList);
		}

		#endregion

		#region Field2Description

		public ZString Field2Description
		{
			get { return StatementFieldTypeList.GetDescriptionFromCode(Field2); }
		}

		public ZPropertyInfo Field2DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Field2Description); }
		}

		#endregion

		#region UseOnHawb

		public ZBool UseOnHawb
		{
			get { return fUseOnHawb; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(UseOnHawbInfo, ref fUseOnHawb, value);
				if (!IsValidationSuspended)
				{
					ValidateUseOnHawb();
				}
			}
		}
		ZBool fUseOnHawb;

		public ZPropertyInfo UseOnHawbInfo
		{
			get { return GetZPropertyInfo(Schema.UseOnHawb); }
		}

		public void ValidateUseOnHawb()
		{
			UseOnHawbInfo.ClearAllNotifications();
		}

		#endregion

		#region UseOnDirectIATAMawb

		public ZBool UseOnDirectIATAMawb
		{
			get { return fUseOnDirectIATAMawb; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(UseOnDirectIATAMawbInfo, ref fUseOnDirectIATAMawb, value);
				if (!IsValidationSuspended)
				{
					ValidateUseOnDirectIATAMawb();
				}
			}
		}
		ZBool fUseOnDirectIATAMawb;

		public ZPropertyInfo UseOnDirectIATAMawbInfo
		{
			get { return GetZPropertyInfo(Schema.UseOnDirectIATAMawb); }
		}

		public void ValidateUseOnDirectIATAMawb()
		{
			UseOnDirectIATAMawbInfo.ClearAllNotifications();
		}

		#endregion

		#region UseOnConsolidationMawb

		public ZBool UseOnConsolidationMawb
		{
			get { return fUseOnConsolidationMawb; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(UseOnConsolidationMawbInfo, ref fUseOnConsolidationMawb, value);
				if (!IsValidationSuspended)
				{
					ValidateUseOnConsolidationMawb();
				}
			}
		}
		ZBool fUseOnConsolidationMawb;

		public ZPropertyInfo UseOnConsolidationMawbInfo
		{
			get { return GetZPropertyInfo(Schema.UseOnConsolidationMawb); }
		}

		public void ValidateUseOnConsolidationMawb()
		{
			UseOnConsolidationMawbInfo.ClearAllNotifications();
		}

		#endregion

		#region UseOnHouseBillOfLading

		public ZBool UseOnHouseBillOfLading
		{
			get { return fUseOnHouseBillOfLading; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(UseOnHouseBillOfLadingInfo, ref fUseOnHouseBillOfLading, value);
				if (!IsValidationSuspended)
				{
					ValidateUseOnHouseBillOfLading();
				}
			}
		}
		ZBool fUseOnHouseBillOfLading;

		public ZPropertyInfo UseOnHouseBillOfLadingInfo
		{
			get { return GetZPropertyInfo(Schema.UseOnHouseBillOfLading); }
		}

		public void ValidateUseOnHouseBillOfLading()
		{
			UseOnHouseBillOfLadingInfo.ClearAllNotifications();
		}

		#endregion

		#region UseOnDirectMasterBillOfLading

		public ZBool UseOnDirectMasterBillOfLading
		{
			get { return fUseOnDirectMasterBillOfLading; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(UseOnDirectMasterBillOfLadingInfo, ref fUseOnDirectMasterBillOfLading, value);
				if (!IsValidationSuspended)
				{
					ValidateUseOnDirectMasterBillOfLading();
				}
			}
		}
		ZBool fUseOnDirectMasterBillOfLading;

		public ZPropertyInfo UseOnDirectMasterBillOfLadingInfo
		{
			get { return GetZPropertyInfo(Schema.UseOnDirectMasterBillOfLading); }
		}

		public void ValidateUseOnDirectMasterBillOfLading()
		{
			UseOnDirectMasterBillOfLadingInfo.ClearAllNotifications();
		}

		#endregion

		#region UseOnConsolidationMasterBillOfLading

		public ZBool UseOnConsolidationMasterBillOfLading
		{
			get { return fUseOnConsolidationMasterBillOfLading; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(UseOnConsolidationMasterBillOfLadingInfo, ref fUseOnConsolidationMasterBillOfLading, value);
				if (!IsValidationSuspended)
				{
					ValidateUseOnConsolidationMasterBillOfLading();
				}
			}
		}
		ZBool fUseOnConsolidationMasterBillOfLading;

		public ZPropertyInfo UseOnConsolidationMasterBillOfLadingInfo
		{
			get { return GetZPropertyInfo(Schema.UseOnConsolidationMasterBillOfLading); }
		}

		public void ValidateUseOnConsolidationMasterBillOfLading()
		{
			UseOnConsolidationMasterBillOfLadingInfo.ClearAllNotifications();
		}

		#endregion

		#endregion

		public CodeDescriptionPairList StatementFieldTypeList
		{
			get
			{
				if (fStatementFieldTypeList == null)
				{
					if (Parent != null && UsaAndTerritoriesList.Contains(Parent.CountryCode))
					{
						fStatementFieldTypeList = new SEDStatementFieldType();
					}
					else
					{
						fStatementFieldTypeList = new CodeDescriptionPairList();
					}
				}
				return fStatementFieldTypeList;
			}
		}
		CodeDescriptionPairList fStatementFieldTypeList;

		public CodeDescriptionPairList VisibilityTypeList
		{
			get
			{
				if (fVisibilityTypeList == null)
				{
					fVisibilityTypeList = new CodeDescriptionPairList();
					fVisibilityTypeList.AddPair(VisibilityList.Mandatory, ResString.GetMultilingualString("fea64baa-62ca-4e48-85e1-9e7be8345c84", "Mandatory"));
					fVisibilityTypeList.AddPair(VisibilityList.UserDefined, ResString.GetMultilingualString("5f03dad2-394e-45e3-ad40-c1a7f1135d9d", "User Defined"));
				}
				return fVisibilityTypeList;
			}
		}
		CodeDescriptionPairList fVisibilityTypeList;

		public void Dispose()
		{
			if (Parent != null)
			{
				Parent.CountryCodeInfo.ValueChanged -= new EventHandler(CountryCodeInfo_ValueChanged);
			}
		}

		#region Constants

		public static class VisibilityList
		{
			public const string Mandatory = "MAN";
			public const string UserDefined = "UDF";
		}

		readonly List<string> UsaAndTerritoriesList = new List<string>()
		{
			Constants.CountryCodes.PuertoRico,
			Constants.CountryCodes.UnitedStates,
			Constants.CountryCodes.Guam,
			Constants.CountryCodes.NorthernMarianaIslands,
			Constants.CountryCodes.VirginIslands,
			Constants.CountryCodes.AmericanSamoa
		};

		#endregion

		#region Overrides

		public override string TableName
		{
			get { return Schema.TableName; }
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			ExportStatementSetting cloneObj = new ExportStatementSetting(Parent);
			try
			{
				((IBusinessObjectInternals)cloneObj).IsCopying = true;
				cloneObj.Code = Code;
				cloneObj.Statement = Statement;
				cloneObj.StatementDescription = StatementDescription;
				cloneObj.Field1 = Field1;
				cloneObj.Field2 = Field2;
				cloneObj.Visibility = Visibility;
				cloneObj.UseOnHawb = UseOnHawb;
				cloneObj.UseOnDirectIATAMawb = UseOnDirectIATAMawb;
				cloneObj.UseOnConsolidationMawb = UseOnConsolidationMawb;
				cloneObj.UseOnHouseBillOfLading = UseOnHouseBillOfLading;
				cloneObj.UseOnDirectMasterBillOfLading = UseOnDirectMasterBillOfLading;
				cloneObj.UseOnConsolidationMasterBillOfLading = UseOnConsolidationMasterBillOfLading;
			}
			finally
			{
				cloneObj.HasChanges = false;
				((IBusinessObjectInternals)cloneObj).IsCopying = false;
			}
			return cloneObj;
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			ExportStatementSetting charge = (ExportStatementSetting)clone;
			charge.Code = Code;
			charge.Statement = Statement;
			charge.StatementDescription = StatementDescription;
			charge.Field1 = Field1;
			charge.Field2 = Field2;
			charge.Visibility = Visibility;
			charge.UseOnHawb = UseOnHawb;
			charge.UseOnDirectIATAMawb = UseOnDirectIATAMawb;
			charge.UseOnConsolidationMawb = UseOnConsolidationMawb;
			charge.UseOnHouseBillOfLading = UseOnHouseBillOfLading;
			charge.UseOnDirectMasterBillOfLading = UseOnDirectMasterBillOfLading;
			charge.UseOnConsolidationMasterBillOfLading = UseOnConsolidationMasterBillOfLading;
		}

		protected override void RunPreSaveValidationCore()
		{
			ClearAllNotifications();
			base.RunPreSaveValidationCore();
			ValidateCode();
			ValidateStatement();
			ValidateField1();
			ValidateField2();
			ValidateVisibility();
			ValidateUseOnHawb();
			ValidateUseOnDirectIATAMawb();
			ValidateUseOnConsolidationMawb();
			ValidateUseOnHouseBillOfLading();
			ValidateUseOnDirectMasterBillOfLading();
			ValidateUseOnConsolidationMasterBillOfLading();
			ValidateRecord();
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Code, Code);
			writer.WriteElementString(Schema.Statement, Statement);
			writer.WriteElementString(Schema.Field1, Field1);
			writer.WriteElementString(Schema.Field2, Field2);
			writer.WriteElementString(Schema.Visibility, Visibility);
			writer.WriteElementString(Schema.UseOnHawb, UseOnHawb.ToString());
			writer.WriteElementString(Schema.UseOnDirectIATAMawb, UseOnDirectIATAMawb.ToString());
			writer.WriteElementString(Schema.UseOnConsolidationMawb, UseOnConsolidationMawb.ToString());
			writer.WriteElementString(Schema.UseOnHouseBillOfLading, UseOnHouseBillOfLading.ToString());
			writer.WriteElementString(Schema.UseOnDirectMasterBillOfLading, UseOnDirectMasterBillOfLading.ToString());
			writer.WriteElementString(Schema.UseOnConsolidationMasterBillOfLading, UseOnConsolidationMasterBillOfLading.ToString());
			writer.WriteElementString(Schema.StatementDescription, StatementDescription);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Code = reader.ReadElementString(Schema.Code);
			Statement = reader.ReadElementString(Schema.Statement);
			Field1 = reader.ReadElementString(Schema.Field1);
			Field2 = reader.ReadElementString(Schema.Field2);
			Visibility = reader.ReadElementString(Schema.Visibility);
			UseOnHawb = new ZBool(reader.ReadElementString(Schema.UseOnHawb));
			UseOnDirectIATAMawb = new ZBool(reader.ReadElementString(Schema.UseOnDirectIATAMawb));
			UseOnConsolidationMawb = new ZBool(reader.ReadElementString(Schema.UseOnConsolidationMawb));
			UseOnHouseBillOfLading = new ZBool(reader.ReadElementString(Schema.UseOnHouseBillOfLading));
			UseOnDirectMasterBillOfLading = new ZBool(reader.ReadElementString(Schema.UseOnDirectMasterBillOfLading));
			UseOnConsolidationMasterBillOfLading = new ZBool(reader.ReadElementString(Schema.UseOnConsolidationMasterBillOfLading));
			StatementDescription = reader.ReadElementString(Schema.StatementDescription);
		}

		public override bool CanDelete => !IsUsaOrTerritory || !IsInUsReferenceSet;

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				return ResString.GetMultilingualString(
					"22F9C120-6ABE-452B-8537-37D88FF69BB8",
					"Valid US Export Statements cannot be removed.");
			}
		}

		void CountryCodeInfo_ValueChanged(object sender, EventArgs e)
		{
			fStatementFieldTypeList = null;
		}

		#region CheckDuplicateCodes

		void CheckDuplicateCodes()
		{
			if (IsDuplicate)
			{
				CodeInfo.AddError(Res.GetString("779F5316-AE68-407C-9872-53C5313D6730", "Duplicate Codes are entered."));
			}
		}

		void ValidateRecord()
		{
			var errorMessage = Res.GetString("DB5F1EC4-0099-4019-8750-05D26944BB27",
				"This entry is not a valid US Export Statement and must be removed from the list.");
			ClearRowNotifications();
			if (IsUsaOrTerritory && !IsInUsReferenceSet)
			{
				AddRowError(errorMessage);
			}
		}

		bool IsDuplicate => ParentCollections.Count > 0 && ((ExportStatementSettingCollection)ParentCollections.Last()).IsDuplicateSetting(this);

		bool IsUsaOrTerritory => Parent != null && Constants.CountryCodes.IsUsaOrTerritory(Parent.CountryCode);

		bool IsInUsReferenceSet => UsExportStatementSettings.ReferenceSet.Contains(this);

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			Code = "NAM";
			Statement = "STATEMENT FOR TESTING";
			Field1 = ZString.Empty;
			Field2 = ZString.Empty;
			Visibility = VisibilityList.UserDefined;
			UseOnHawb = true;
			UseOnDirectIATAMawb = false;
			UseOnConsolidationMawb = false;
			UseOnHouseBillOfLading = true;
			UseOnDirectMasterBillOfLading = true;
			UseOnConsolidationMasterBillOfLading = false;
		}
#endif

		#endregion
	}
}
