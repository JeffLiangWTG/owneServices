using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class DefaultNumberOfSupportingDocumentsCollection : RegistryBusinessObjectCollectionTemplate
	{
		public DefaultNumberOfSupportingDocumentsCollection()
		{
		}

		public DefaultNumberOfSupportingDocumentsCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public new DefaultNumberOfSupportingDocuments this[int index]
		{
			get { return (DefaultNumberOfSupportingDocuments)Elements[index]; }
		}

		public new DefaultNumberOfSupportingDocuments AddNew()
		{
			return (DefaultNumberOfSupportingDocuments)base.AddNew();
		}

		public void AddDefaultValues(ZString code, MultilingualString defaultDescription, MultilingualString description, ZByte defaultValue)
		{
			DefaultNumberOfSupportingDocuments element = new DefaultNumberOfSupportingDocuments();
			element.Code = code.Trim().Left(5).ToUpper();
			element.DefaultDescription = defaultDescription;
			element.Description = description;
			element.NumberOfDefault = defaultValue;
			Add(element);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{			return new DefaultNumberOfSupportingDocumentsCollection(fallbackLevel, factory);		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{			return new DefaultNumberOfSupportingDocuments(CurrentFallbackLevel, CurrentFactory);		}

		/// <summary>
		/// Returns 0 if a match not found.
		/// </summary>
		public ZByte GetValueFromCode(string code)
		{
			string trimmedCode = code.Trim();
			foreach (DefaultNumberOfSupportingDocuments element in this)
			{
				if (String.Equals(element.Code.Trim(), trimmedCode, StringComparison.OrdinalIgnoreCase))
				{
					return element.NumberOfDefault;
				}
			}
			return 0;
		}

		/// <summary>
		/// Returns null if a match not found.
		/// </summary>
		public string GetDescriptionFromCode(string code)
		{
			string trimmedCode = code.Trim();
			foreach (DefaultNumberOfSupportingDocuments element in this)
			{
				if (String.Equals(element.Code.Trim(), trimmedCode, StringComparison.OrdinalIgnoreCase))
				{
					return element.Description;
				}
			}
			return null;
		}
	}
}