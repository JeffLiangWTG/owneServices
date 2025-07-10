using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class RegistryUserAgreementType : RegistryBusinessObject
	{
		public new class Schema : RegistryBusinessObject.Schema
		{
			public const string Level = "Level";
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new RegistryUserAgreementType();
		}

		public override bool CanDelete
		{
			get
			{
				if (Code.IsEmpty)
				{
					return true;
				}

				var existingAgreementsQuery = new ZQuery(EdiUserAgreementSchema.ERA_Type, Code);
				var factory = Factory ?? new BusinessObjectFactory();
				return !factory.ExistsInDatabase(EdiUserAgreementSchema.Constants.TableName, existingAgreementsQuery);
			}
		}

		[List("AgreementLevels")]
		[MaxLength(3)]
		[ResourceStringData("RegistryUserAgreementType|Level", Caption = "Level", FullDescription = "Agreement Level")]
		public ZString Level
		{
			get { return level; }
			set
			{
				CheckMaximumLength(LevelInfo, value);
				SetNonPersistentPropertyValue(LevelInfo, ref level, value);
				if (!IsValidationSuspended)
				{
					ValidateLevel();
				}
			}
		}
		ZString level;

		public bool Level_ReadOnly => !Level.IsEmpty && !Code.IsEmpty && !CanDelete;

		public ZPropertyInfo LevelInfo => GetZPropertyInfo(Schema.Level);

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("ebc8f125-e483-491d-ae68-5ee64ff65a63", "Cannot delete Types which already have User Agreements.");

		protected override int MaxDescriptionLength => 50;

		public CodeDescriptionPairList AgreementLevels => new EdiUserAgreementLevelList();

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			if (reader.NodeType == XmlNodeType.Element && reader.Name == Schema.Level)
			{
				Level = new ZString(reader.ReadElementString(Schema.Level));
			}
			else
			{
				Level = EdiUserAgreementLevelList.Codes.User;
			}
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			writer.WriteElementString(Schema.Level, Level);
		}

		void ValidateLevel()
		{
			LevelInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(LevelInfo);
			ListValidation.ErrorIfInvalidCode(LevelInfo);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			if (!IsValidationSuspended)
			{
				ValidateLevel();
			}
		}
	}
}
