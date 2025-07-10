using System;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CountryExportStatementSetting : RegistryBusinessObjectTemplate, IDisposable
	{
		public CountryExportStatementSetting()
		{
		}

		#region Schema

		public static class Schema
		{
			public const string CountryCode = "CountryCode";
			public const string Statements = "Statements";
		}

		#endregion

		#region CountryCode

		ZString fCountryCode;
		[MaxLength(2)]
		public ZString CountryCode
		{
			get { return fCountryCode; }
			set
			{
				value = value.TrimEnd(' ');
				if (value != fCountryCode)
				{
					CheckMaximumLength(CountryCodeInfo, value);
					SetNonPersistentPropertyValue<ZString>(CountryCodeInfo, ref fCountryCode, value);
					if (!IsValidationSuspended && !((IBusinessObjectInternals)this).IsCopying)
					{
						ValidateCountryCode();
					}
				}
			}
		}

		public ZPropertyInfo CountryCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CountryCode); }
		}

		void ValidateCountryCode()
		{
			CountryCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CountryCodeInfo, (IMultilingualString)ResString.GetMultilingualString("651577b5-6c76-4c8f-b58d-18c86576dbdc", "Country/Region Code"));
			ListValidation.ErrorIfInvalidCode(CountryCodeInfo, CountryCodes);
			if (!CountryCodeInfo.HasErrors())
			{
				CheckDuplicateCodes();
			}
		}

		public IBusinessObjectCollection CountryCodes
		{
			get
			{
				if (fCountryCodes == null)
				{
					fCountryCodes = (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IRefCountryCollection>(), CurrentFactory);
				}
				return fCountryCodes;
			}
		}
		IBusinessObjectCollection fCountryCodes;

		#endregion

		#region Statements
		public ExportStatementSettingCollection Statements
		{
			get
			{
				if (fStatements == null)
				{
					fStatements = GetNewStatements();
					RegisterEditableChildObject(fStatements);
				}
				return fStatements;
			}
		}
		ExportStatementSettingCollection fStatements;

		protected virtual ExportStatementSettingCollection GetNewStatements()
		{
			return new ExportStatementSettingCollection(this);
		}
		#endregion

		public override bool HasChanges
		{
			get { return base.HasChanges; }
			set
			{
				bool oldHasChange = HasChanges;
				base.HasChanges = value;
				if (!IsCopying)
				{
					if (oldHasChange != value && oldHasChange)
					{
						SetAllChildHasChange(value);
					}
				}
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (fStatements != null)
			{
				UnRegisterEditableChildObject(fStatements);
				fStatements = null;
			}
		}

		#endregion

		#region Implementation

		#region Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			CountryExportStatementSetting result = new CountryExportStatementSetting();
			try
			{
				((IBusinessObjectInternals)result).IsCopying = true;
				result.CountryCode = CountryCode;
				result.Statements.AddRange((BusinessObjectCollection)Statements.Clone(fallbackLevel, factory));
			}
			finally
			{
				result.HasChanges = false;
				((IBusinessObjectInternals)result).IsCopying = false;
			}
			return result;
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.CountryCode, CountryCode);
			ExportStatementSettingCollectionSerializer.Serialize(writer, Statements);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			try
			{
				((IBusinessObjectInternals)this).IsCopying = true;
				CountryCode = reader.ReadElementString(Schema.CountryCode);
				Statements.RemoveAndDeleteAll();
				Statements.AddRange((ExportStatementSettingCollection)ExportStatementSettingCollectionSerializer.Deserialize(reader));
			}
			finally
			{
				HasChanges = false;
				((IBusinessObjectInternals)this).IsCopying = false;
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			ClearAllNotifications();
			base.RunPreSaveValidationCore();
			ValidateCountryCode();
		}

		protected void SetAllChildHasChange(bool changed)
		{
			if (fStatements != null)
			{
				foreach (ExportStatementSetting statement in fStatements)
				{
					statement.HasChanges = changed;
				}
			}
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			CountryCode = Core.Constants.CountryCodes.Australia;
			ExportStatementSetting statement = Statements.AddNew();
			statement.FillWithValidTestData(kind, propertyPath);
		}
#endif

		#endregion

		#region CheckDuplicateCodes

		void CheckDuplicateCodes()
		{
			if (ParentCollections.Count > 0 && ((CountryExportStatementSettingCollection)ParentCollections.First()).IsDuplicateSetting(this))
			{
				CountryCodeInfo.AddError(Res.GetString("753be869-23e3-4b30-aa9b-59d261afa19a", "Duplicate Codes are entered."));
			}
		}

		#endregion

		ZXmlSerializer ExportStatementSettingCollectionSerializer
		{
			get
			{
				if (fExportStatementSettingCollectionSerializer == null)
				{
					fExportStatementSettingCollectionSerializer = ZXmlSerializer.New(typeof(ExportStatementSettingCollection));
				}

				return fExportStatementSettingCollectionSerializer;
			}
		}
		ZXmlSerializer fExportStatementSettingCollectionSerializer;

		#endregion
	}
}
