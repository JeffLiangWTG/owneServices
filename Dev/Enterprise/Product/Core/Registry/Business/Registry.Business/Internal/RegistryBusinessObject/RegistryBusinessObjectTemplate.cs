using System;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public abstract class RegistryBusinessObjectTemplate<TValidation> : RegistryBusinessObjectTemplate
		where TValidation : ZValidation
	{
		protected RegistryBusinessObjectTemplate()
		{
		}

		protected RegistryBusinessObjectTemplate(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected RegistryBusinessObjectTemplate(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		protected RegistryBusinessObjectTemplate(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public TValidation Validation => GetNewValidation();

		protected abstract TValidation GetNewValidation();
	}

	public abstract class RegistryBusinessObjectTemplate : NonPersistentBusinessObject, IObsoleteValidation, IRegistryBusiness, IXmlSerializable
	{
		protected RegistryBusinessObjectTemplate()
			: this(null, null)
		{
		}

		protected RegistryBusinessObjectTemplate(BusinessObjectFactory factory)
			: this(null, factory)
		{
		}

		protected RegistryBusinessObjectTemplate(FallbackLevel fallbackLevel)
			: this(fallbackLevel, null)
		{
		}

		protected RegistryBusinessObjectTemplate(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			CurrentFallbackLevel = fallbackLevel;
			factoryOverride = factory;
			SetCustomDefaultValues();
		}

		protected RegistryBusinessObjectCollectionTemplate GetParentCollection(RegistryBusinessObjectTemplate businessObject, Type parentCollectionType)
		{
			RegistryBusinessObjectCollectionTemplate result = null;

			if (businessObject.ParentCollections.Count == 1 && parentCollectionType.IsAssignableFrom(businessObject.ParentCollections.First().GetType()))
			{
				result = ((RegistryBusinessObjectCollectionTemplate)ParentCollections.First());
			}

			return result;
		}

		#region Set Default Values

		/// <summary>
		/// This is sealed because SetDefaultValues is called in the base BusinessObject class constructor,
		/// where CurrentFallbackLevel and CurrentFactory will be null. 
		/// </summary>
		protected sealed override void SetDefaultValues()
		{
			base.SetDefaultValues();
		}

		void SetCustomDefaultValues()
		{
			using (SuspendSettingHasChanges())
			using (GetValidationSuspender())
			{
				SetCustomDefaultValuesCore();
			}
		}

		protected virtual void SetCustomDefaultValuesCore()
		{
		}

		#endregion

		#region Current Factory

		protected BusinessObjectFactory CurrentFactory
		{
			get
			{
				return factoryOverride ?? RegistryFactory.Instance;
			}
		}

		readonly BusinessObjectFactory factoryOverride;

		#endregion

		#region Clone

		public IRegistryBusiness Clone(FallbackLevel currentFallbackLevel, BusinessObjectFactory factory)
		{
			var result = GetClone(currentFallbackLevel, factory);
			using (result.GetValidationSuspender())
			{
				CopyValuesToClone(result);
				CopyCollectionsToClone(result, currentFallbackLevel, factory);
			}
			return result;
		}

		protected virtual void CopyCollectionsToClone(RegistryBusinessObjectTemplate clone, FallbackLevel currentFallbackLevel, BusinessObjectFactory factory)
		{
		}

		protected virtual void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			using (!clone.IsValidationSuspended ? clone.GetValidationSuspender() : null)
			{
				clone.CopyValuesFrom(this);
			}
		}

		protected abstract RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory);

		#endregion

		#region IBusinessWithFallback Members

		public FallbackLevel CurrentFallbackLevel
		{
			get { return CurrentFallbackLevelCore; }
			set { CurrentFallbackLevelCore = value; }
		}

		protected virtual FallbackLevel CurrentFallbackLevelCore
		{
			get { return fCurrentFallbackLevel; }
			set { fCurrentFallbackLevel = value; }
		}

		FallbackLevel fCurrentFallbackLevel;

		#endregion

		#region IXmlSerializable Members

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			WriteElements(writer);
		}

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			reader.Read();
			using (GetValidationSuspender())
			{
				ReadElements(new XmlReaderWrapper(reader));
			}
			if (!reader.EOF)
			{
				reader.ReadEndElement();
			}
		}

		XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		protected virtual void WriteElements(XmlWriter writer)
		{
		}

		protected abstract void ReadElements(XmlReaderWrapper reader);

		#endregion

		#region Validation

		protected void ThrowValidationExceptionToPreventSave()
		{
			throw new RegistryValidationException(Res.GetString("f4348144-9b7f-4676-b502-505fb0923670", "There are errors in one or more of items specified. Please correct any errors before saving."));
		}

		#endregion
	}
}
