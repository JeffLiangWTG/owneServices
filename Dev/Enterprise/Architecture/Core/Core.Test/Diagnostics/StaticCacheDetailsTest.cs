using System;
using CargoWise.Common.MemoryManagement;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	[TestedType(typeof(StaticCacheDetails))]
	sealed class StaticCacheDetailsTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var mock = new Mock<IReclaimable>();
			mock.Setup(m => m.ConstructionTime).Returns(DateTime.Now);

			return new StaticCacheDetails(mock.Object);
		}
	}
}
