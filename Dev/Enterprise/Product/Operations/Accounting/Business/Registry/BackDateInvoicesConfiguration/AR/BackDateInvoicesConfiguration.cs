using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class BackDateInvoicesConfiguration : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string OverridePostDate = "OverridePostDate";
			public const string DefaultPostDateFromInvoiceDate = "DefaultPostDateFromInvoiceDate";
		}

		#endregion

		public BackDateInvoicesConfiguration()
		{
		}

		public BackDateInvoicesConfiguration(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			if (InvoiceDateConfigurationCollection.Count == 0)
			{
				InvoiceDateConfigurationCollection.AddNew();
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BackDateInvoicesConfiguration(fallbackLevel);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			BackDateInvoicesConfiguration castedClone = (BackDateInvoicesConfiguration)clone;
			if (invoiceDateConfigurationCollection != null)
			{
				castedClone.invoiceDateConfigurationCollection = (InvoiceDateConfigurationCollection)InvoiceDateConfigurationCollection.Clone(castedClone.CurrentFallbackLevel, castedClone.Factory);
				castedClone.RegisterEditableChildObject(castedClone.InvoiceDateConfigurationCollection);
			}
		}

		#region Bound Properties

		#region OverridePostDate

		public ZBool OverridePostDate
		{
			get { return OverridePostDate_innerValue; }
			set
			{
				if (SetNonPersistentPropertyValue(OverridePostDateInfo, ref OverridePostDate_innerValue, value))
				{
					ValidateOverridePostDate();
				}
			}
		}

		public ZPropertyInfo OverridePostDateInfo
		{
			get { return GetZPropertyInfo(Schema.OverridePostDate); }
		}

		public void ValidateOverridePostDate()
		{
			OverridePostDateInfo.ClearAllNotifications();

			if (OverridePostDate)
			{
				string error = CheckBackPostingError();
				if (!string.IsNullOrEmpty(error))
				{
					OverridePostDateInfo.AddError(error);
				}
			}
		}

		ZBool OverridePostDate_innerValue;

		#endregion

		#region DefaultPostDateFromInvoiceDate

		public ZBool DefaultPostDateFromInvoiceDate
		{
			get { return DefaultPostDateFromInvoiceDate_innerValue; }
			set
			{
				if (SetNonPersistentPropertyValue(DefaultPostDateFromInvoiceDateInfo, ref DefaultPostDateFromInvoiceDate_innerValue, value))
				{
					ValidateDefaultPostDateFromInvoiceDate();
				}
			}
		}

		public ZPropertyInfo DefaultPostDateFromInvoiceDateInfo
		{
			get { return GetZPropertyInfo(Schema.DefaultPostDateFromInvoiceDate); }
		}

		public void ValidateDefaultPostDateFromInvoiceDate()
		{
			DefaultPostDateFromInvoiceDateInfo.ClearAllNotifications();

			if (DefaultPostDateFromInvoiceDate)
			{
				string error = CheckBackPostingError();
				if (!string.IsNullOrEmpty(error))
				{
					DefaultPostDateFromInvoiceDateInfo.AddError(error);
				}
			}
		}

		ZBool DefaultPostDateFromInvoiceDate_innerValue;

		#endregion

		#region InvoiceDateConfigurationCollection

		public InvoiceDateConfigurationCollection InvoiceDateConfigurationCollection
		{
			get
			{
				if (invoiceDateConfigurationCollection == null)
				{
					invoiceDateConfigurationCollection = new InvoiceDateConfigurationCollection();
					RegisterEditableChildObject(invoiceDateConfigurationCollection);
				}
				return invoiceDateConfigurationCollection;
			}
		}
		InvoiceDateConfigurationCollection invoiceDateConfigurationCollection;

		ZXmlSerializer fInvoiceDateConfigurationCollectionSerialiser;
		ZXmlSerializer InvoiceDateConfigurationCollectionSerialiser
		{
			get
			{
				return fInvoiceDateConfigurationCollectionSerialiser ?? (fInvoiceDateConfigurationCollectionSerialiser = ZXmlSerializer.New(typeof(InvoiceDateConfigurationCollection)));
			}
		}

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateOverridePostDate();
			ValidateDefaultPostDateFromInvoiceDate();
		}

		string CheckBackPostingError()
		{
			if (CurrentFallbackLevel != null && !AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.GetFallBackValueAtAllLevels(CurrentFallbackLevel.CompanyPK(false), Guid.Empty, Guid.Empty))
			{
				return Res.GetString("C21F88D0-3237-4ea0-9A6D-EBB77A8AE664", "You cannot configure back posting options here because 'Back Posting' has not been enabled for this company under the 'Allow Back Posting Sub Ledger Transaction' registry.");
			}
			return string.Empty;
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.OverridePostDate, OverridePostDate.ToString());
			writer.WriteElementString(Schema.DefaultPostDateFromInvoiceDate, DefaultPostDateFromInvoiceDate.ToString());
			InvoiceDateConfigurationCollectionSerialiser.Serialize(writer, InvoiceDateConfigurationCollection);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			OverridePostDate = new ZBool(reader.ReadElementString(Schema.OverridePostDate));
			DefaultPostDateFromInvoiceDate = new ZBool(reader.ReadElementString(Schema.DefaultPostDateFromInvoiceDate));
			invoiceDateConfigurationCollection = (InvoiceDateConfigurationCollection)InvoiceDateConfigurationCollectionSerialiser.Deserialize(reader);
			RegisterEditableChildObject(invoiceDateConfigurationCollection);
		}

		#endregion
	}
}