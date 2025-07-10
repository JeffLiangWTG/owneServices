using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Organizations.CodeGeneration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class OrgCodeAlgorithm : RegistryBusinessObjectTemplate, IOrgCodeAlgorithm
	{
		OrgCodeAlgorithmType algorithmType;
		OrgCodeElementCollection elements;
		ZByte? currentOrgCodeLength;
		ZBool regenerateOrgCodeOnChanges;
		ZBool allowRecalculatedOrgCodeByUser;
		OrgCodeOrgTypeCollection selectableOrgTypes;

		public OrgCodeAlgorithm()
		{
		}

		public OrgCodeAlgorithmType AlgorithmType
		{
			get { return algorithmType; }
			set { algorithmType = value; }
		}

		public ZByte CurrentOrgCodeLength
		{
			get
			{
				if (currentOrgCodeLength == null)
				{
					currentOrgCodeLength = ZByte.Zero;
					foreach (OrgCodeElement element in Elements)
					{
						if (!element.Order.IsEmpty)
						{
							currentOrgCodeLength += element.Length;
						}
					}
					if (!IsValidationSuspended)
					{
						ValidateCurrentOrgCodeLength();
					}
				}
				return currentOrgCodeLength.Value;
			}
		}

		public ZPropertyInfo CurrentOrgCodeLengthInfo
		{
			get { return GetZPropertyInfo(Schema.CurrentOrgCodeLength); }
		}

		public bool NeedsUNLOCO()
		{
			var result = false;
			foreach (OrgCodeElement element in Elements)
			{
				if (!element.Empty && (element.Description == OrgCodeElementDescription.UnlocoCode || element.Description == OrgCodeElementDescription.IataCode))
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public bool NeedsName()
		{
			var result = false;
			foreach (OrgCodeElement element in Elements)
			{
				if (!element.Empty && (element.Description == OrgCodeElementDescription.FirstName || element.Description == OrgCodeElementDescription.SecondName || element.Description == OrgCodeElementDescription.LastName))
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public bool NeedsCountry()
		{
			var result = false;
			foreach (OrgCodeElement element in Elements)
			{
				if (!element.Empty && (element.Description == OrgCodeElementDescription.CountryCode))
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public OrgCodeElementCollection Elements
		{
			get
			{
				if (elements == null)
				{
					elements = new OrgCodeElementCollection(this);
					elements.Load();
					RegisterEditableChildObject(elements);
				}
				return elements;
			}
		}

		ZXmlSerializer ElementsSerializer
		{
			get { return ZXmlSerializer.New(typeof(OrgCodeElementCollection)); }
		}

		public ZByte MaxOrgCodeLength
		{
			get { return (ZByte)OrgHeaderSchema.OH_Code.MaxLength; }
		}

		public ZPropertyInfo MaxOrgCodeLengthInfo
		{
			get { return GetZPropertyInfo(Schema.MaxOrgCodeLength); }
		}

		public ZBool RegenerateOrgCodeOnChanges
		{
			get { return regenerateOrgCodeOnChanges; }
			set { SetNonPersistentPropertyValue<ZBool>(RegenerateOrgCodeOnChangesInfo, ref regenerateOrgCodeOnChanges, value); }
		}

		public ZPropertyInfo RegenerateOrgCodeOnChangesInfo
		{
			get { return GetZPropertyInfo(Schema.RegenerateOrgCodeOnChanges); }
		}

		public ZBool AllowRecalculatedOrgCodeByUser
		{
			get { return allowRecalculatedOrgCodeByUser; }
			set { SetNonPersistentPropertyValue<ZBool>(AllowRecalculatedOrgCodeByUserInfo, ref allowRecalculatedOrgCodeByUser, value); }
		}

		public ZPropertyInfo AllowRecalculatedOrgCodeByUserInfo
		{
			get { return GetZPropertyInfo(Schema.AllowRecalculatedOrgCodeByUser); }
		}

		public OrgCodeOrgTypeCollection SelectableOrgTypes
		{
			get
			{
				if (selectableOrgTypes == null)
				{
					selectableOrgTypes = new OrgCodeOrgTypeCollection(this);
					selectableOrgTypes.Load();
					RegisterEditableChildObject(selectableOrgTypes);
				}
				return selectableOrgTypes;
			}
		}

		ZXmlSerializer SelectableOrgTypesSerializer
		{
			get { return ZXmlSerializer.New(typeof(OrgCodeOrgTypeCollection)); }
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			OrgCodeAlgorithm cloneAlgorithm = (OrgCodeAlgorithm)clone;
			cloneAlgorithm.algorithmType = algorithmType;
			if (selectableOrgTypes != null)
			{
				cloneAlgorithm.SelectableOrgTypes.CopyElementValuesFrom((OrgCodeOrgTypeCollection)selectableOrgTypes.Clone(null, null));
			}
			if (elements != null)
			{
				cloneAlgorithm.Elements.CopyElementValuesFrom((OrgCodeElementCollection)elements.Clone(null, null));
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OrgCodeAlgorithm();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			regenerateOrgCodeOnChanges = new ZBool(reader.ReadElementString(Schema.RegenerateOrgCodeOnChanges));
			allowRecalculatedOrgCodeByUser = new ZBool(reader.ReadElementString(Schema.AllowRecalculatedOrgCodeByUser));
			Elements.CopyElementValuesFrom((OrgCodeElementCollection)ElementsSerializer.Deserialize(reader));
			SelectableOrgTypes.CopyElementValuesFrom((OrgCodeOrgTypeCollection)SelectableOrgTypesSerializer.Deserialize(reader));
		}

		public void RefreshCurrentOrgCodeLength()
		{
			currentOrgCodeLength = null;
			CurrentOrgCodeLengthInfo.RefreshBinding();
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateCurrentOrgCodeLength();
		}

		public void ValidateCurrentOrgCodeLength()
		{
			CurrentOrgCodeLengthInfo.ClearAllNotifications();
			if (CurrentOrgCodeLength > MaxOrgCodeLength)
			{
				CurrentOrgCodeLengthInfo.AddError(Res.GetString("58dd3e2d-a0b4-40b5-821e-674c974d1f41", "The total length has exceeded the maximum length allowed for organization codes. Please reduce the length of one or more elements."));
			}
			else if (CurrentOrgCodeLength <= 0)
			{
				CurrentOrgCodeLengthInfo.AddError(Res.GetString("8513a163-5fbb-4ad7-a0cf-ab834da4a9cf", "Organization codes cannot be empty, please select a non-zero order for one or more elements with non-zero length."));
			}
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.RegenerateOrgCodeOnChanges, RegenerateOrgCodeOnChanges.ToString());
			writer.WriteElementString(Schema.AllowRecalculatedOrgCodeByUser, AllowRecalculatedOrgCodeByUser.ToString());
			ElementsSerializer.Serialize(writer, Elements);
			SelectableOrgTypesSerializer.Serialize(writer, SelectableOrgTypes);
		}

		#region IOrgCodeAlgorithm Members

		IEnumerable<IOrgCodeElement> IOrgCodeAlgorithm.Elements
		{
			get { return Elements.Cast<IOrgCodeElement>(); }
		}

		bool IOrgCodeAlgorithm.RegenerateOrgCodeOnChanges
		{
			get { return RegenerateOrgCodeOnChanges; }
		}

		bool IOrgCodeAlgorithm.AllowRecalculatedOrgCodeByUser
		{
			get { return AllowRecalculatedOrgCodeByUser; }
		}

		IEnumerable<IOrgCodeOrgType> IOrgCodeAlgorithm.SelectableOrgTypes
		{
			get { return SelectableOrgTypes.Cast<IOrgCodeOrgType>(); }
		}

		#endregion

		#region Schema

		public static class Schema
		{
			public const string CurrentOrgCodeLength = "CurrentOrgCodeLength";
			public const string MaxOrgCodeLength = "MaxOrgCodeLength";
			public const string RegenerateOrgCodeOnChanges = "RegenerateOrgCodeOnChanges";
			public const string AllowRecalculatedOrgCodeByUser = "AllowRecalculatedOrgCodeByUser";
		}

		#endregion
	}
}
