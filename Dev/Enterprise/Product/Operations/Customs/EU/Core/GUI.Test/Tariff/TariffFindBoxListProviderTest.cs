using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class TariffFindBoxListProviderTest : TestCaseWithFactory
	{
		public void TestNearestMatch()
		{
			AssertEquals("2203000201", listProvider.NearestMatch("2203.00.02 01", true, -1).Item1);
		}

		public void TestDescriptionFromCode()
		{
			AssertEquals("", listProvider.DescriptionFromCode("2203.00.02.01L"));
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestDescriptionPrimaryKey()
		{
			listProvider.DescriptionFromPrimaryKey(ZGuid.Empty);
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestCodeFromPrimaryKey()
		{
			listProvider.CodeFromPrimaryKey(ZGuid.Empty);
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestPrimaryKeyFromCode()
		{
			listProvider.PrimaryKeyFromCode(string.Empty);
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestGetBusinessObjectsFromCode()
		{
			listProvider.GetBusinessObjectsFromCode(string.Empty);
		}

		protected override void SetUp()
		{
			base.SetUp();
			listProvider = new TariffFindBoxListProvider();
		}
		IFindBoxListProvider listProvider;
	}
}
