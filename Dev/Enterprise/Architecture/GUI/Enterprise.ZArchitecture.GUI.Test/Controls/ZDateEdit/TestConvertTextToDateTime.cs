using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class TestConvertTextToDateTime : TransactionedTestCase
	{
		public void TestConvertTextToDateTimeForJapaneseCompany()
		{
			var factory = new BusinessObjectFactory();

			var japaneseCompany = factory.New<IGlbCompany>();
			japaneseCompany.SetCountry("JP");
			(japaneseCompany as BusinessObject)[GlbCompanySchema.GC_Code] = "XXX";

			var branch = factory.New<IGlbBranch>();
			branch.GB_GC = japaneseCompany.PK;
			(branch as BusinessObject)[GlbBranchSchema.GB_Code] = "JAP";
			factory.Save();

			using (var dateEdit = new ZDateEdit())
			{
				dateEdit.DateTimeValue = new DateTime(2010, 11, 15);
				AssertEquals("ConvertTextToDateTime for AU", new DateTime(2010, 11, 15), new ZDateEditCore(dateEdit).ConvertTextToDateTime("15-NOV-10"));
				AssertEquals("ConvertTextToDateTime for AU", new DateTime(2010, 11, 15), new ZDateEditCore(dateEdit).ConvertTextToDateTime(dateEdit.Text));
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.LoginName, branch.PK.ToGuid(), EnvProxy.Instance.CurrentDepartment.PK))
			using (var dateEdit = new ZDateEdit())
			{
				dateEdit.DateTimeValue = new DateTime(2010, 11, 15);

				AssertEquals("ConvertTextToDateTime for JP", new DateTime(2010, 11, 15), new ZDateEditCore(dateEdit).ConvertTextToDateTime("2010-11-15"));
				AssertEquals("ConvertTextToDateTime for JP", new DateTime(2010, 11, 15), new ZDateEditCore(dateEdit).ConvertTextToDateTime("10-11-15"));
			}
		}
	}
}
