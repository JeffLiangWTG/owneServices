using System;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusExit.Testing
{
	[TestedType(typeof(CusExitDetailCollection))]
	class CusExitDetailCollectionTest : EU.Business.Testing.CusExitDetailCollectionTest
	{
		protected override Type GetExpectedCollectionType() => typeof(CusExitDetailCollection);
	}
}
