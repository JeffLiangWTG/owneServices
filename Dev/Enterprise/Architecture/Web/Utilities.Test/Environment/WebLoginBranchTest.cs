using System;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Utilities.Environment.Testing
{
	internal class WebLoginBranchTest : TestCase
	{
		IBranch SomeOtherBranch;

		public void TestConstructDispose()
		{
			var mockBranch = new Mock<IBranch>();
			Guid currentBranch = Guid.Empty;
			if (Db.Connection.ExecuteScalar(string.Format("select top 1 GB_PK from dbo.GlbBranch where GB_PK <> '{0}'", Env.CurrentBranch.PK)) is object o
				&& Guid.TryParse(o.ToString(), out var someOtherBranchPK))
			{
				mockBranch.Setup(m => m.PK)
						  .Returns(someOtherBranchPK);
				SomeOtherBranch = mockBranch.Object;

				currentBranch = Env.CurrentBranch.PK;
			}

			try
			{
				using (WebLoginBranch tempBranch = new WebLoginBranch(SomeOtherBranch))
				{
					AssertEquals("Environment should be logged into temp branch", SomeOtherBranch.PK, Env.CurrentBranch.PK);
				}
			}
			finally
			{
				AssertEquals("Environment should revert to original branch", currentBranch, Env.CurrentBranch.PK);
			}

			mockBranch.VerifyAll();
		}
	}
}
