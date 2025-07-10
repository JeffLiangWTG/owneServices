using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class FeeChargeType : RegistryBusinessObjectTemplate
	{
		#region Schema

		protected abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string TypeCode = "Code";
			public const string TypeDescription = "Description";
			public const string FeeChargeLevels = "FeeChargeLevels";
			public const string EnglishDescription = "EnglishDescription";
		}

		#endregion

		#region Properties

		#region Fee Charge Type Code

		[MaxLength(3)]
		public ZString Code
		{
			get { return code; }
			set
			{
				CheckMaximumLength(CodeInfo, value);
				SetNonPersistentPropertyValue(CodeInfo, ref code, value);

				if (!IsValidationSuspended)
				{
					ValidateCodeCore();
				}
			}
		}
		ZString code;

		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(Schema.TypeCode); }
		}

		protected void ValidateCodeCore()
		{
			CodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CodeInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(CodeInfo);
		}

		#endregion

		#region Description

		[MaxLength(256)]
		public MultilingualString Description
		{
			get { return description ?? (NoResString)""; }
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}

				CheckMaximumLength(DescriptionInfo, value.GetUnresolvedString());
				SetNonPersistentPropertyValue(DescriptionInfo, ref description, value, false);
				EnglishDescriptionInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					ValidateDescriptionCore();
				}
			}
		}
		MultilingualString description;

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.TypeDescription); }
		}

		[MaxLength(256)]
		public ZString EnglishDescription
		{
			get { return Description.GetUnresolvedString(); }
			set { Description = (NoResString)value; }
		}

		public ZPropertyInfo EnglishDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.EnglishDescription); }
		}

		protected void ValidateDescriptionCore()
		{
			DescriptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(DescriptionInfo);
		}

		#endregion

		#region Fee Charge - Levels

		public FeeChargeLevelCollection FeeChargeLevels
		{
			get
			{
				if (feeChargeLevels == null)
				{
					feeChargeLevels = new FeeChargeLevelCollection();
					RegisterEditableChildObject(feeChargeLevels);
				}
				return feeChargeLevels;
			}
		}
		FeeChargeLevelCollection feeChargeLevels;

		#endregion

		#endregion

		#region Overriden

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateCodeCore();
			ValidateDescriptionCore();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = new FeeChargeType();
			var cloneLevels = (FeeChargeLevelCollection)FeeChargeLevels.Clone(fallbackLevel, factory);

			foreach (var level in cloneLevels)
			{
				result.FeeChargeLevels.Add(level);
			}

			return result;
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.TypeCode, Code);
			writer.WriteElementString(Schema.TypeDescription, EnglishDescription);
			LevelsSerialiser.Serialize(writer, FeeChargeLevels);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Code = reader.ReadElementString(Schema.TypeCode);
			EnglishDescription = reader.ReadElementString(Schema.TypeDescription);

			var levels = (FeeChargeLevelCollection)LevelsSerialiser.Deserialize(reader);

			foreach (var level in levels)
			{
				FeeChargeLevels.Add(level);
			}
		}

		ZXmlSerializer LevelsSerialiser
		{
			get { return levelsSerialiser ?? (levelsSerialiser = ZXmlSerializer.New(typeof(FeeChargeLevelCollection))); }
		}
		ZXmlSerializer levelsSerialiser;

		#endregion
	}
}
