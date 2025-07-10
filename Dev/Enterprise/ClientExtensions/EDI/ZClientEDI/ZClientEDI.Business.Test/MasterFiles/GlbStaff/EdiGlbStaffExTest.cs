using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EdiGlbStaffEx))]
	public class EdiGlbStaffExTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPopulateNamesFromFullName()
		{
			var staffEx = Factory.New<EdiGlbStaffEx>();
			AssertSplit(staffEx, "Anupama Jinka Rajendraprasad", "", "Anupama", "Jinka", "Rajendraprasad", "");
			AssertSplit(staffEx, "Joe X", "", "Joe", "", "X", "");
			AssertSplit(staffEx, "Liesbeth Van Aerschot", "Liesbeth.VanAerschot", "Liesbeth", "", "Van Aerschot", "");
			AssertSplit(staffEx, "José Roberto Nascimento Buéno", "jose.bueno", "José", "Roberto Nascimento", "Buéno", "");
			AssertSplit(staffEx, "Gary O'Dea", "Gary.Odea", "Gary", "", "O'Dea", "");
			AssertSplit(staffEx, "Patrick Van De Looverbosch", "", "Patrick", "", "Van De Looverbosch", "");
			AssertSplit(staffEx, "Robert Paul Lammerts van Bueren", "robert.lammertsvanbueren", "Robert", "Paul", "Lammerts van Bueren", "");
			AssertSplit(staffEx, "Sofie Van De Looverbosch", "S.VanDeLooverbosch", "Sofie", "", "Van De Looverbosch", "");
			AssertSplit(staffEx, "Antonio Di Stefano", "antonio.distefano", "Antonio", "", "Di Stefano", "");
			AssertSplit(staffEx, "Benjamin Anthony Sutas", "Benjamin.Sutas", "Benjamin", "Anthony", "Sutas", "");
			AssertSplit(staffEx, "Este De La Hunt", "Este.Niemandt", "Este", "", "De La Hunt", "");
			AssertSplit(staffEx, "Elaine Cristina da Silva Vieira", "elaine.vieira", "Elaine", "Cristina", "da Silva Vieira", "");
			AssertSplit(staffEx, "Geoffrey F. Eid", "geoffrey.eid", "Geoffrey", "F.", "Eid", "");
			AssertSplit(staffEx, "Gampala L.N.V.Pitcheswara rao", "Gampala.LNVPitcheswaraRao", "Gampala", "", "L.N.V.Pitcheswara rao", "");
			AssertSplit(staffEx, "Jishnuraj K R", "Jishnuraj.KR", "Jishnuraj", "", "K R", "");
			AssertSplit(staffEx, "STEVE, KANG KEAT SIANG", "Steve.Kang", "STEVE", "KANG KEAT", "SIANG", "");
			AssertSplit(staffEx, "Sumesh Subbaraya reddy", "SumeshSubbaraya.reddy", "Sumesh", "Subbaraya", "reddy", "");
			AssertSplit(staffEx, "Penghao (Steven) Zhang", "Steven.Zhang", "Penghao", "", "Zhang", "");
			AssertSplit(staffEx, "Ben Gorringe (ViAGO)", "", "Ben", "", "Gorringe", "");
			AssertSplit(staffEx, "Xin Wang (Michelle)", "", "Xin", "", "Wang", "");
			AssertSplit(staffEx, "Ma. Aime M. Jumao-as", "Aime.Jumao", "Ma.", "Aime M.", "Jumao-as", "");
			AssertSplit(staffEx, "Zhongjiao Ye叶钟娇", "", "Zhongjiao", "", "Ye", "叶钟娇");
			AssertSplit(staffEx, "Joe", "", "", "", "", "", false);
			AssertSplit(staffEx, "叶钟娇", "", "", "", "", "", false);
			AssertSplit(staffEx, "WTGSYDPAVE02", "", "", "", "", "", false);
		}

		void AssertSplit(EdiGlbStaffEx staffEx, string fullName, string loginName, string first, string middle, string last, string domestic, bool expectSuccess = true)
		{
			var success = staffEx.PopulateNamesFromFullName(fullName, loginName);
			if (expectSuccess)
			{
				CombineAssertions(() =>
				{
					AssertEquals("FirstName", first, staffEx.GS9_FirstName);
					AssertEquals("MiddleName", middle, staffEx.GS9_MiddleName);
					AssertEquals("LastName", last, staffEx.GS9_LastName);
					AssertEquals("DomesticName", domestic, staffEx.GS9_DomesticName);
					AssertEquals("ok", true, success);
				});
			}
			else
			{
				AssertEquals("OK", false, success);
			}
		}

		public void TestUniqueIndexHandler()
		{
			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			Factory.RefreshEnabled = false;
			Factory.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = true };
			var staffInFactory2 = factory2.Load<EDIGlbStaff>(staff.PK);
			AssertNull("PRE", staff.ReadonlyStaffEx);
			AssertNull("PRE", staffInFactory2.ReadonlyStaffEx);

			AssertNotNull(staff.StaffEx);
			AssertNotNull(staffInFactory2.StaffEx);
			Factory.Save();

			try
			{
				factory2.Save();
				Fail("Should throw");
			}
			catch (ZSaveException ex)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZExceptionReporting.HandleSaveException(ex);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("While you have been working, another user has made changes. Please close and reopen the form again.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var bizo = Factory.NewWithValidTestData<EdiGlbStaffEx>();
			bizo.GS9_GS = Env.CurrentUserPK;
			return bizo;
		}

		#endregion
	}
}
