using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ReleaseType : RegistryBusinessObject, ICanDelete
	{
		#region Schema

		abstract new class Schema : RegistryBusinessObject.Schema
		{
			public const string OriginalsNumber = "OriginalsNumber";
			public const string CopiesNumber = "CopiesNumber";
		}

		#endregion

		public ReleaseType()
		{
		}

		public ReleaseType(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ReleaseType(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		public ReleaseType(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region Method Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ReleaseType();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			((ReleaseType)clone).SystemDefined = SystemDefined;
		}

		#endregion

		#region Bound Properties

		#region Code / Description

		protected bool Code_ReadOnly
		{
			get { return SystemDefined; }
		}

		protected bool Description_ReadOnly
		{
			get { return SystemDefined; }
		}

		protected override int MaxDescriptionLength
		{
			get { return 256; }
		}

		public bool EnglishDescription_ReadOnly
		{
			get { return SystemDefined; }
		}

		#endregion

		#region System Defined

		public bool SystemDefined
		{
			get { return systemDefined; }
			set { systemDefined = value; }
		}

		bool systemDefined;

		#endregion

		#region OriginalsNumber

		public virtual ZInt OriginalsNumber
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fOriginalsNumber; }
			set
			{
				SetNonPersistentPropertyValue<ZInt>(OriginalsNumberInfo, ref fOriginalsNumber, value);
				if (!IsValidationSuspended)
				{
					ValidateOriginalsNumber();
				}
			}
		}

		ZInt fOriginalsNumber;

		public virtual ZPropertyInfo OriginalsNumberInfo
		{
			get { return GetZPropertyInfo(Schema.OriginalsNumber); }
		}

		public void ValidateOriginalsNumber()
		{
			OriginalsNumberInfo.ClearAllNotifications();
			CompareValidation.CheckWithinRange(OriginalsNumberInfo, 0, 255);
		}

		#endregion

		#region CopiesNumber

		public virtual ZInt CopiesNumber
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fCopiesNumber; }
			set
			{
				SetNonPersistentPropertyValue<ZInt>(CopiesNumberInfo, ref fCopiesNumber, value);
				if (!IsValidationSuspended)
				{
					ValidateCopiesNumber();
				}
			}
		}

		ZInt fCopiesNumber;

		public virtual ZPropertyInfo CopiesNumberInfo
		{
			get { return GetZPropertyInfo(Schema.CopiesNumber); }
		}

		public void ValidateCopiesNumber()
		{
			CopiesNumberInfo.ClearAllNotifications();
			CompareValidation.CheckWithinRange(CopiesNumberInfo, 0, 255);
		}

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteMoreElements(XmlWriter writer)
		{
			writer.WriteElementString(Schema.OriginalsNumber, OriginalsNumber.ToString());
			writer.WriteElementString(Schema.CopiesNumber, CopiesNumber.ToString());
		}

		protected override void ReadMoreElements(XmlReader reader)
		{
			OriginalsNumber = new ZInt(reader.ReadElementString(Schema.OriginalsNumber));
			CopiesNumber = new ZInt(reader.ReadElementString(Schema.CopiesNumber));
		}

		#endregion

		#region ICanDelete Members

		bool ICanDelete.CanDelete
		{
			get { return !SystemDefined; }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("9960d19d-5a1a-421c-9ecf-366175787573", "This is a system defined value and cannot be deleted."); }
		}

		#endregion
	}
}
