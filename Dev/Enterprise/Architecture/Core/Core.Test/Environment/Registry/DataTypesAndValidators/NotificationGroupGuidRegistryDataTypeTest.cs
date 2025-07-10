using System;
using System.Text;
using CargoWise.Data;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(NotificationGroupGuidRegistryDataType))]
	sealed class NotificationGroupGuidRegistryDataTypeTest : RegistryDataTypeTestCase<NotificationGroupGuidRegistryDataType>
	{
		[ExpectException(typeof(RegistryValidationException))]
		public void TestValidateyWithInvalidGuid()
		{
			NotificationGroupDataType.Validate(NotificationGroup, Guid.NewGuid(), Guid.Empty, Guid.Empty, Guid.Empty);
		}

		[ExpectException(typeof(RegistryValidationException))]
		public void TestValidateyWithValidGuidThatHasNoUserEmailAddress()
		{
			Guid groupPK = CreateLinkedStaffAndGroup("TZ", "DUMMY", "", "TZ$");
			NotificationGroupDataType.Validate(NotificationGroup, groupPK, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		protected override NotificationGroupGuidRegistryDataType GetNewDataType()
		{
			return NotificationGroupDataType;
		}

		GuidRegistryItem NotificationGroup
		{
			get
			{
				if (notificationGroup == null)
				{
					notificationGroup = new GuidRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.IsValueOptional);
					notificationGroup.DataType = NotificationGroupDataType;
				}
				return notificationGroup;
			}
		}
		GuidRegistryItem notificationGroup;

		NotificationGroupGuidRegistryDataType NotificationGroupDataType
		{
			get
			{
				if (fNotificationGroupDataType == null)
				{
					fNotificationGroupDataType = new NotificationGroupGuidRegistryDataType();
				}
				return fNotificationGroupDataType;
			}
		}
		NotificationGroupGuidRegistryDataType fNotificationGroupDataType;

		Guid CreateLinkedStaffAndGroup(string staffCode, string staffFullName, string staffEmail, string groupCode)
		{
			Guid staffPK = Guid.NewGuid();
			Guid groupPK = Guid.NewGuid();

			Db.Connection.ExecuteNonQuery("insert into dbo.GlbStaff(GS_PK, GS_Code, GS_LoginName, GS_FullName, GS_EmailAddress, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES ('" + staffPK + "', '" + staffCode + "', '" + "Test" + staffCode + "', '" + staffFullName + "', '" + staffEmail + "', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			Db.Connection.ExecuteNonQuery("insert into dbo.GlbGroup(GG_PK, GG_Code, GG_Desc, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser) VALUES ('" + groupPK + "', '" + groupCode + "', 'Test', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			Db.Connection.ExecuteNonQuery("insert into dbo.GlbGroupLink(GK_PK, GK_GG, GK_GS) VALUES (NEWID(), '" + groupPK + "', '" + staffPK + "')");

			return groupPK;
		}

		char codePostfix = 'A';
		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			Guid groupPK = CreateLinkedStaffAndGroup("TZ" + codePostfix++, "DUMMY", "dummy@what.com", "TZ$" + codePostfix++);
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(Guid.Empty, Encoding.Unicode.GetBytes(Guid.Empty.ToString())),
				new ValidSampleAndBinaryValueInDB(groupPK, Encoding.Unicode.GetBytes(groupPK.ToString()))
			};
		}
	}
}
