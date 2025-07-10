using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class CustomLabelsProviderAndBizOTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructorWithNullInterfaceThrowsException()
		{
			var provider = new CustomLabelsProviderAndBizO(null, null);
		}
	}
}
