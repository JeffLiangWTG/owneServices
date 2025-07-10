using System;
using System.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Public.Testing
{
	abstract class MoveSecurityRightsBetweenParentsTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.GlbSecurity");

			Db.Connection.Command(string.Format(insertCommonTestData,
				userPK1.ToString(), userPK2.ToString(), userPK3.ToString(), userPK4.ToString(),
				userPK5.ToString(), userPK6.ToString(), userPK7.ToString(), userPK8.ToString(),
				groupPK1.ToString(), groupPK2.ToString(), companyPK1.ToString(), companyPK2.ToString(),
				branchPK11.ToString(), branchPK12.ToString(), branchPK21.ToString(), branchPK22.ToString(),
				departmentPK1.ToString(), departmentPK2.ToString(), //{16} and {17}
				((MoveSecurityRightsBetweenParents)TransformationToTest).oldParents.First(), ((MoveSecurityRightsBetweenParents)TransformationToTest).securityRights.First(),
				((MoveSecurityRightsBetweenParents)TransformationToTest).oldParents.ElementAt(1), ((MoveSecurityRightsBetweenParents)TransformationToTest).newParents.First(),
				emptyUserPK1, emptyUserPK2))
				.ExecuteNonQuery();
		}

		class MoveSecurityRightsBetweenParentsForTest : MoveSecurityRightsBetweenParents
		{
			public MoveSecurityRightsBetweenParentsForTest(IUpgradeManager manager, string[] oldParents, string[] newParents, params string[] securityRightsToMove)
			: base(manager, oldParents, newParents, securityRightsToMove)
			{
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new MoveSecurityRightsBetweenParentsForTest(new DummyUpgradeManager(), new string[] { "Parent", "GrandParent", "GreatGrandParent" }, new string[] { "NewParent" }, "SecurityRight1", "SecurityRight2", "SecurityRight3");
		}

		protected override void AssertTransformationResults()
		{
			var right = ((MoveSecurityRightsBetweenParents)TransformationToTest).securityRights.First();

			//Commented out lines are because if the code was smart enough to look at all groups for the user and determine that there were no relevant rights in the security-parent
			//hierarchy then it could have a base case *-*-* Denied at the top, but it is not currently.
			//(There is also no equivalent for a group to this.)

			//User1 has fullhierarchy on parent for C1 and C2 and * on child

			AssertSecurityValue(right, userPK1, true, Guid.Empty, Guid.Empty, Guid.Empty, false);

			//User2 has fullhierarchy on parent and C1 on child for C1, fullhierarchy on parent and B21/B22 on child for C2

			AssertSecurityValue(right, userPK2, true, Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertSecurityValue(right, userPK2, true, companyPK1, Guid.Empty, Guid.Empty, false);

			AssertSecurityValue(right, userPK2, true, companyPK2, Guid.Empty, Guid.Empty, true);
			AssertSecurityValue(right, userPK2, true, Guid.Empty, branchPK21, Guid.Empty, false);
			AssertSecurityValue(right, userPK2, true, Guid.Empty, branchPK22, Guid.Empty, false);

			//User3 has fullhierarchy on parent and C1+D1 on child for C1, fullhierarchy on parent and C2+D2 on child for C2

			AssertSecurityValue(right, userPK3, true, Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertSecurityValue(right, userPK3, true, companyPK1, Guid.Empty, Guid.Empty, true);
			AssertSecurityValue(right, userPK3, true, Guid.Empty, branchPK11, Guid.Empty, true);
			AssertSecurityValue(right, userPK3, true, companyPK1, Guid.Empty, departmentPK1, false);

			AssertSecurityValue(right, userPK3, true, companyPK2, Guid.Empty, Guid.Empty, true);
			AssertSecurityValue(right, userPK3, true, Guid.Empty, branchPK21, Guid.Empty, true);
			AssertSecurityValue(right, userPK3, true, companyPK2, Guid.Empty, departmentPK1, true);
			AssertSecurityValue(right, userPK3, true, companyPK2, Guid.Empty, departmentPK2, false);

			//User4 has fullhierarchy on parent and B11+D1 on child for C1, fullhierarchy on parent B21+D2 on child for C2

			AssertSecurityValue(right, userPK4, true, Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertSecurityValue(right, userPK4, true, companyPK1, Guid.Empty, Guid.Empty, true);
			AssertSecurityValue(right, userPK4, true, Guid.Empty, branchPK11, Guid.Empty, true);
			AssertSecurityValue(right, userPK4, true, companyPK1, Guid.Empty, departmentPK1, true);
			AssertSecurityValue(right, userPK4, true, Guid.Empty, branchPK11, departmentPK1, false);

			AssertSecurityValue(right, userPK4, true, companyPK2, Guid.Empty, Guid.Empty, true);
			AssertSecurityValue(right, userPK4, true, Guid.Empty, branchPK21, Guid.Empty, true);
			AssertSecurityValue(right, userPK4, true, companyPK2, Guid.Empty, departmentPK1, true);
			AssertSecurityValue(right, userPK4, true, Guid.Empty, branchPK21, departmentPK1, true);
			AssertSecurityValue(right, userPK4, true, Guid.Empty, branchPK21, departmentPK2, false);

			//User5 has nothing on parent and * on child

			//AssertSecurityValue(right, userPK5, true, Guid.Empty, Guid.Empty, Guid.Empty, false);

			//User6 has * on parent and nothing on child

			AssertSecurityValue(right, userPK6, true, Guid.Empty, Guid.Empty, Guid.Empty, true);

			//User7 has C1 on parent and C2 on child

			//AssertSecurityValue(right, userPK7, true, Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertSecurityValue(right, userPK7, true, companyPK1, Guid.Empty, Guid.Empty, true);
			AssertSecurityValue(right, userPK7, true, companyPK2, Guid.Empty, Guid.Empty, false);

			//User8 has nothing on parent and C2 on child, and also nothing on newParent (sanity check kicks in)

			AssertSecurityValue(right, userPK8, true, Guid.Empty, Guid.Empty, Guid.Empty, null);
			AssertSecurityValue(right, userPK8, true, companyPK2, Guid.Empty, Guid.Empty, false);

			//Group1 has * on parent and * on child and also demonstrates that grandparent's entries are ignored if not visible

			AssertSecurityValue(right, groupPK1, false, Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertSecurityValue(right, groupPK1, false, companyPK1, Guid.Empty, Guid.Empty, null);
			AssertSecurityValue(right, groupPK1, false, companyPK2, Guid.Empty, Guid.Empty, null);
			AssertSecurityValue(right, groupPK1, false, Guid.Empty, branchPK11, Guid.Empty, null);

			//Group2 has nothing on parent or child and also demonstrates that grandparent's entries are used if visible

			AssertSecurityValue(right, groupPK2, false, Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertSecurityValue(right, groupPK2, false, companyPK1, Guid.Empty, Guid.Empty, false);
			AssertSecurityValue(right, groupPK2, false, companyPK2, Guid.Empty, Guid.Empty, true);
			AssertSecurityValue(right, groupPK2, false, Guid.Empty, branchPK11, Guid.Empty, true);

			//EmptyUser1 has nothing on parent or child and demonstrates that no security rights are made

			AssertSecurityValue(right, emptyUserPK1, true, Guid.Empty, Guid.Empty, Guid.Empty, null);

			//EmptyUser2 has nothing on parent or child but does on newParent and determines that new security rights are made regardless

			//AssertSecurityValue(right, emptyUserPK2, true, Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		void AssertSecurityValue(string securityRight, Guid owner, bool isUser, Guid fGU_GC, Guid fGU_GB, Guid fGU_GE, bool? expectedValue)
		{
			var assertSql = @"SELECT GU_SecurityItemIsAllowed FROM dbo.GlbSecurity WHERE GU_SecurityRight = '{0}' AND ";
			assertSql += fGU_GC == Guid.Empty ? "GU_GC is null AND " : "GU_GC = '{1}' AND ";
			assertSql += fGU_GB == Guid.Empty ? "GU_GB is null AND " : "GU_GB = '{2}' AND ";
			assertSql += fGU_GE == Guid.Empty ? "GU_GE is null AND " : "GU_GE = '{3}' AND ";
			assertSql += isUser ? "GU_GS = '{4}'" : "GU_GG = '{4}'";

			bool? result = (bool?)Db.Connection.Command(string.Format(assertSql, securityRight, fGU_GC, fGU_GB, fGU_GE, owner)).ExecuteScalar();

			if (expectedValue == null)
			{
				AssertNull(string.Format("New security records should not be created. {0} {1} {2} {3} {4} {5} {6}",
					securityRight, owner, isUser, fGU_GC, fGU_GB, fGU_GE, expectedValue
					), result);
			}
			else
			{
				AssertEquals(string.Format("New security record should have correct value. {0} {1} {2} {3} {4} {5} {6}",
					securityRight, owner, isUser, fGU_GC, fGU_GB, fGU_GE, expectedValue
					), expectedValue, result);
			}
		}

		#region Implementation

		Guid userPK1 = new Guid(1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		Guid userPK2 = new Guid(2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		Guid userPK3 = new Guid(3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		Guid userPK4 = new Guid(4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		Guid userPK5 = new Guid(5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		Guid userPK6 = new Guid(6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		Guid userPK7 = new Guid(7, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		Guid userPK8 = new Guid(8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		Guid groupPK1 = new Guid(9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		Guid groupPK2 = new Guid(10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		Guid companyPK1 = new Guid(11, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		Guid companyPK2 = new Guid(12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		Guid branchPK11 = new Guid(13, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		Guid branchPK12 = new Guid(14, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		Guid branchPK21 = new Guid(15, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		Guid branchPK22 = new Guid(16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		Guid departmentPK1 = new Guid(17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		Guid departmentPK2 = new Guid(18, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

		Guid emptyUserPK1 = new Guid(19, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		Guid emptyUserPK2 = new Guid(20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

		const string insertCommonTestData = @"
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES ('{0}', 'T01', 'User1', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES ('{1}', 'T02', 'User2', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES ('{2}', 'T03', 'User3', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES ('{3}', 'T04', 'User4', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES ('{4}', 'T05', 'User5', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES ('{5}', 'T06', 'User6', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES ('{6}', 'T07', 'User7', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES ('{7}', 'T08', 'User8', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.GlbGroup (GG_PK, GG_Code) VALUES ('{8}', 'G1')
INSERT INTO dbo.GlbGroup (GG_PK, GG_Code) VALUES ('{9}', 'G2')

insert into dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES ('{10}', 'C1', 'AU company1', 'AU', 'AUD')
insert into dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES ('{11}', 'C2', 'AU company2', 'AU', 'AUD')

insert into dbo.GlbBranch(GB_PK, GB_Code, GB_GC) VALUES ('{12}', 'B11', '{10}')
insert into dbo.GlbBranch(GB_PK, GB_Code, GB_GC) VALUES ('{13}', 'B12', '{10}')
insert into dbo.GlbBranch(GB_PK, GB_Code, GB_GC) VALUES ('{14}', 'B21', '{11}')
insert into dbo.GlbBranch(GB_PK, GB_Code, GB_GC) VALUES ('{15}', 'B22', '{11}')

insert into dbo.GlbDepartment(GE_PK, GE_Code) VALUES ('{16}', 'D1')
insert into dbo.GlbDepartment(GE_PK, GE_Code) VALUES ('{17}', 'D2')

INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES ('{22}', 'EM1', 'EmptyUser1', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES ('{23}', 'EM2', 'EmptyUser2', GetUtcDate(), 'E', GetUtcDate(), 'E')

--'{18}' shall be parent security name, '{19}' shall be child security name

--User1 has fullhierarchy on parent for C1 and C2 and * on child
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 0, '{19}', null, null, null, '{0}')

INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', null, null, null, '{0}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', '{10}', null, null, '{0}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', null, '{12}', null, '{0}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', '{10}', null, '{16}', '{0}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', null, '{12}', '{16}', '{0}')

INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', '{11}', null, null, '{0}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', null, '{14}', null, '{0}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', '{11}', null, '{16}', '{0}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', null, '{14}', '{16}', '{0}')

--User2 has fullhierarchy on parent and C1 on child for C1, fullhierarchy on parent and B21/B22 on child for C2
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 0, '{19}', '{10}', null, null, '{1}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 0, '{19}', null, '{14}', null, '{1}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 0, '{19}', null, '{15}', null, '{1}')

INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', null, null, null, '{1}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', '{10}', null, null, '{1}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', null, '{12}', null, '{1}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', '{10}', null, '{16}', '{1}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', null, '{12}', '{16}', '{1}')

INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', '{11}', null, null, '{1}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', null, '{14}', null, '{1}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', '{11}', null, '{16}', '{1}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', null, '{14}', '{16}', '{1}')

--User3 has fullhierarchy on parent and C1+D1 on child for C1, fullhierarchy on parent and C2+D2 on child for C2
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 0, '{19}', '{10}', null, '{16}', '{2}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 0, '{19}', '{11}', null, '{17}', '{2}')

INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', null, null, null, '{2}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', '{10}', null, null, '{2}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', null, '{12}', null, '{2}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', '{10}', null, '{16}', '{2}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', null, '{12}', '{16}', '{2}')

INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', '{11}', null, null, '{2}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', null, '{14}', null, '{2}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', '{11}', null, '{16}', '{2}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', null, '{14}', '{16}', '{2}')

--User4 has fullhierarchy on parent and B11+D1 on child for C1, fullhierarchy on parent B21+D2 on child for C2
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 0, '{19}', null, '{12}', '{16}', '{3}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 0, '{19}', null, '{14}', '{17}', '{3}')

INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', null, null, null, '{3}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', '{10}', null, null, '{3}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', null, '{12}', null, '{3}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', '{10}', null, '{16}', '{3}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', null, '{12}', '{16}', '{3}')

INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', '{11}', null, null, '{3}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', null, '{14}', null, '{3}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', '{11}', null, '{16}', '{3}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', null, '{14}', '{16}', '{3}')

--User5 has nothing on parent and * on child
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 0, '{19}', null, null, null, '{4}')

--User6 has * on parent and nothing on child
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', null, null, null, '{5}')

--User7 has C1 on parent and C2 on child
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{18}', '{10}', null, null, '{6}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 0, '{19}', '{11}', null, null, '{6}')

--User8 has nothing on parent and C2 on child, and also nothing on newParent (sanity check kicks in)
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 0, '{19}', '{11}', null, null, '{7}')

--Group1 has * on parent and * on child and also demonstrates that grandparent's entries are ignored if not visible
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GG)
VALUES (NEWID(), 1, '{18}', null, null, null, '{8}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GG)
VALUES (NEWID(), 0, '{19}', null, null, null, '{8}')

INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GG)
VALUES (NEWID(), 1, '{20}', null, null, null, '{8}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GG)
VALUES (NEWID(), 1, '{20}', '{10}', null, null, '{8}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GG)
VALUES (NEWID(), 1, '{20}', '{11}', null, null, '{8}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GG)
VALUES (NEWID(), 1, '{20}', null, '{12}', null, '{8}')

--Group2 has nothing on parent or child and also demonstrates that grandparent's entries are used if visible
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GG)
VALUES (NEWID(), 0, '{20}', '{10}', null, null, '{9}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GG)
VALUES (NEWID(), 1, '{20}', '{11}', null, null, '{9}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GG)
VALUES (NEWID(), 1, '{20}', null, '{12}', null, '{9}')

--EmptyUser1 has nothing on parent or child and demonstrates that no security rights are made

--EmptyUser2 has nothing on parent or child but does on newParent and determines that new security rights are made regardless
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 1, '{21}', null, null, null, '{23}')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, GU_GS)
VALUES (NEWID(), 0, '{21}', '{10}', null, null, '{23}')

";

		#endregion
	}
}
