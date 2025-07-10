using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ReleaseTypes : RegistryBusinessObjectTemplate
	{
		#region Schema

		protected abstract class Schema
		{
			public const string Types = "Types";
			public const string OriginalsNumber = "OriginalsNumber";
			public const string CopiesNumber = "CopiesNumber";
		}

		#endregion

		public ReleaseTypes()
		{
		}

		public ReleaseTypes(ReadOnlyCodeDescriptionPairList list)
		{
			Types = new ReleaseTypeCollection(list);
		}

		public CodeDescriptionPairList GetCodeDescriptionPairList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();

			foreach (RegistryBusinessObject element in this.Types)
			{
				result.AddPair(element.Code, element.Description);
			}

			return result;
		}

		public ZInt OriginalsNumberByType(ZString code)
		{
			ZInt result = OriginalsNumber;
			if (!code.IsEmpty)
			{
				ReleaseType relType = Types.FindByCode(code);
				if (relType != null)
				{
					result = relType.OriginalsNumber;
				}
			}

			return result;
		}

		public ZInt CopiesNumberByType(ZString code)
		{
			ZInt result = CopiesNumber;
			if (!code.IsEmpty)
			{
				ReleaseType relType = Types.FindByCode(code);
				if (relType != null)
				{
					result = relType.CopiesNumber;
				}
			}

			return result;
		}

		#region Cloning

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			ReleaseTypes result = new ReleaseTypes();
			result.Types = (ReleaseTypeCollection)Types.Clone(fallbackLevel, factory);
			return result;
		}

		#endregion

		#region OriginalsNumber

		public ZInt OriginalsNumber
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

		public ZPropertyInfo OriginalsNumberInfo
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

		public ZInt CopiesNumber
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

		public ZPropertyInfo CopiesNumberInfo
		{
			get { return GetZPropertyInfo(Schema.CopiesNumber); }
		}

		public void ValidateCopiesNumber()
		{
			CopiesNumberInfo.ClearAllNotifications();
			CompareValidation.CheckWithinRange(CopiesNumberInfo, 0, 255);
		}

		#endregion

		#region Types

		[BusinessObjectTestExclude]
		public ReleaseTypeCollection Types
		{
			get { return fTypes ?? (Types = new ReleaseTypeCollection()); }
			private set
			{
				fTypes = value;
				fTypes.Parent = this;
				RegisterEditableChildObject(fTypes);
			}
		}

		ZXmlSerializer ReleaseTypeCollectionSerialiser
		{
			get { return fReleaseTypeCollectionSerialiser ?? (fReleaseTypeCollectionSerialiser = ZXmlSerializer.New(typeof(ReleaseTypeCollection))); }
		}

		ReleaseTypeCollection fTypes;
		ZXmlSerializer fReleaseTypeCollectionSerialiser;

		#endregion

		#region XML Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.OriginalsNumber, OriginalsNumber.ToString());
			writer.WriteElementString(Schema.CopiesNumber, CopiesNumber.ToString());
			ReleaseTypeCollectionSerialiser.Serialize(writer, Types);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			OriginalsNumber = new ZInt(reader.ReadElementString(Schema.OriginalsNumber));
			CopiesNumber = new ZInt(reader.ReadElementString(Schema.CopiesNumber));
			Types = (ReleaseTypeCollection)ReleaseTypeCollectionSerialiser.Deserialize(reader);
		}

		#endregion
	}
}
