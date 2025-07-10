using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.TrustedMessaging.Business.Testing
{
	[TestedType(typeof(EdiTrustedSystemCollection))]
	public class EdiTrustedSystemCollectionTest : ActiveBusinessObjectCollectionTestCase<EdiTrustedSystemCollection>
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(EdiTrustedSystemCollection);
		}
	}
}
