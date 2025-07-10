using System;
using System.Data;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public abstract class SeaCargoTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForBinding()
		{
			AssertEquals("Failed to return CMR Type", GetCMRType(), GetTypeDecider().GetTypeForBinding());
		}

		public void TestGetTypeForLoading()
		{
			AssertEquals("Failed to return CMR Type", GetCMRType(), GetTypeDecider().GetTypeForLoad(GetDataRowForCMRObject(), Factory));
		}

		public void TestGetTypeForNew()
		{
			AssertEquals("Failed to return CMR Type", GetCMRType(), GetTypeDecider().GetTypeForNew());
		}

		protected abstract DataRow GetDataRowForCMRObject();
		protected abstract CargoWise.EntityFramework.TypeDecider GetTypeDecider();
		protected abstract Type GetCMRType();
	}
}
