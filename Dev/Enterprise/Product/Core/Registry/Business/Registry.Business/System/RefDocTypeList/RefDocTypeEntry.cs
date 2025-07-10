using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class RefDocTypeEntry : RegistryBusinessObject, IObsoleteValidation
	{
		#region Constructors

		public RefDocTypeEntry()
			: base()
		{
		}

		public RefDocTypeEntry(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		#region Properties

		#region RefDocTypePK

		public ZGuid RefDocTypePK
		{
			get { return refDocTypePK; }
			set
			{
				SetNonPersistentPropertyValue<ZGuid>(RefDocTypePKInfo, ref refDocTypePK, value);
				if (!IsValidationSuspended)
				{
					ValidateRefDocTypePK();
				}
			}
		}

		public ZPropertyInfo RefDocTypePKInfo
		{
			get { return GetZPropertyInfo(nameof(RefDocTypePK), "Doc Type"); }
		}

		public void ValidateRefDocTypePK()
		{
			RefDocTypePKInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(RefDocTypePKInfo);
			TypeValidation.CheckValidGuid(RefDocTypePKInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(RefDocTypePKInfo);
		}

		ZGuid refDocTypePK;

		public IBusinessObjectCollection RefDocTypeCollection
		{
			get
			{
				if (refDocTypeCollection == null)
				{
					refDocTypeCollection = (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IRefDocTypeCollection>(), new object[] { CurrentFactory });
				}
				return refDocTypeCollection;
			}
		}
		IBusinessObjectCollection refDocTypeCollection;

		#endregion

		#region RefDocTypeName

		public ZString RefDocTypeName
		{
			get
			{
				var refDocType = CurrentFactory.Load<IRefDocType>(refDocTypePK);
				return refDocType != null ? refDocType.RT_DescMultilingual : ZString.Empty;
			}
		}

		#endregion

		#endregion

		#region Validation

		protected override bool IsCodeMandatory
		{
			get { return false; }
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateRefDocTypePK();
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			writer.WriteElementString(Schema.RefDocTypePK, RefDocTypePK.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			RefDocTypePK = new ZGuid(reader.ReadElementString(Schema.RefDocTypePK));
		}

		protected new class Schema : RegistryBusinessObject.Schema
		{
			public const string RefDocTypePK = "RefDocTypePK";
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new RefDocTypeEntry(factory);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			var entryClone = (RefDocTypeEntry)clone;
			entryClone.RefDocTypePK = RefDocTypePK;
		}

		#endregion
	}
}
