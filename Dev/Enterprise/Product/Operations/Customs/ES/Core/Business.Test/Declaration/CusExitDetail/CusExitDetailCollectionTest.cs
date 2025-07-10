using System;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(CusExitDetailCollection))]
	public class CusExitDetailCollectionTest : EU.Business.Testing.CusExitDetailCollectionTest
	{
		protected override Type GetExpectedCollectionType() => typeof(CusExitDetailCollection);
	}
}
