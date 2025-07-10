using System;
using System.Net.Http;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GlowInterop;
using GlowIndexQueryService.Common;
using Moq;
using NUnit.Framework;

namespace GlowIndexQueryService.Tests.Common
{
	internal class GlowWebRequestTest : TransactionedTestCase
	{
		public void TestGet()
		{
			var webRequest = new GlowWebRequest();

			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			var clientMock = new Mock<IGlowServiceClient>();
			clientFactoryMock.Setup(cf => cf.Create(It.IsAny<Uri>())).Returns(clientMock.Object);

			clientMock.Setup(c => c.GetAsync("test/endpoint"))
				.ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
				{
					Content = new StringContent("Success")
				});

			using (ObjectFactory.Substitute(clientFactoryMock.Object))
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://GlowServiceUri.com"))
			{
				var result = webRequest.Get("test/endpoint");
				AssertEquals("Success", result);
			}
		}

		public void TestPost()
		{
			var webRequest = new GlowWebRequest();

			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			var clientMock = new Mock<IGlowServiceClient>();
			clientFactoryMock.Setup(cf => cf.Create(It.IsAny<Uri>())).Returns(clientMock.Object);

			clientMock.Setup(c => c.PostAsync("test/endpoint", It.IsAny<StringContent>()))
				.ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
				{
					Content = new StringContent("Success")
				});

			using (ObjectFactory.Substitute(clientFactoryMock.Object))
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://GlowServiceUri.com"))
			{
				var result = webRequest.Post("test/endpoint", "json");
				AssertEquals("Success", result);
			}
		}

		public void TestGet_HandlesContextSwitchSafely()
		{
			var factory = new BusinessObjectFactory();
			var newStaff = factory.NewWithValidTestData<GlbStaff>();
			var newBranch = factory.NewWithValidTestData<GlbBranch>();
			var newDepartment = factory.NewWithValidTestData<GlbDepartment>();
			factory.Save();

			AssertNotEquals(newStaff.PK.ToGuid(), Env.Instance.CurrentUserPK);
			AssertNotEquals(newBranch.PK.ToGuid(), Env.Instance.CurrentBranchPK);
			AssertNotEquals(newDepartment.PK.ToGuid(), Env.Instance.CurrentDepartmentPK);

			var webRequest = new GlowWebRequest();

			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			var clientMock = new Mock<IGlowServiceClient>();
			clientFactoryMock.Setup(cf => cf.Create(It.IsAny<Uri>())).Returns(clientMock.Object);

			clientMock.Setup(c => c.GetAsync("test/endpoint"))
				.Callback(() =>
				{
					AssertEquals(newStaff.PK.ToGuid(), Env.Instance.CurrentUserPK);
					AssertEquals(newBranch.PK.ToGuid(), Env.Instance.CurrentBranchPK);
					AssertEquals(newDepartment.PK.ToGuid(), Env.Instance.CurrentDepartmentPK);
				})
				.ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
				{
					Content = new StringContent("Success")
				});

			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), newBranch.PK.ToGuid(), newDepartment.PK.ToGuid()))
			using (ObjectFactory.Substitute(clientFactoryMock.Object))
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://GlowServiceUri.com"))
			{
				var result = webRequest.Get("test/endpoint");
				AssertEquals("Success", result);
			}

			AssertNotEquals(newStaff.PK.ToGuid(), Env.Instance.CurrentUserPK);
			AssertNotEquals(newBranch.PK.ToGuid(), Env.Instance.CurrentBranchPK);
			AssertNotEquals(newDepartment.PK.ToGuid(), Env.Instance.CurrentDepartmentPK);
		}

		public void TestPost_HandlesContextSwitchSafely()
		{
			var factory = new BusinessObjectFactory();
			var newStaff = factory.NewWithValidTestData<GlbStaff>();
			var newBranch = factory.NewWithValidTestData<GlbBranch>();
			var newDepartment = factory.NewWithValidTestData<GlbDepartment>();
			factory.Save();

			AssertNotEquals(newStaff.PK.ToGuid(), Env.Instance.CurrentUserPK);
			AssertNotEquals(newBranch.PK.ToGuid(), Env.Instance.CurrentBranchPK);
			AssertNotEquals(newDepartment.PK.ToGuid(), Env.Instance.CurrentDepartmentPK);

			var webRequest = new GlowWebRequest();

			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			var clientMock = new Mock<IGlowServiceClient>();
			clientFactoryMock.Setup(cf => cf.Create(It.IsAny<Uri>())).Returns(clientMock.Object);

			clientMock.Setup(c => c.PostAsync("test/endpoint", It.IsAny<StringContent>()))
				.Callback(() =>
				{
					AssertEquals(newStaff.PK.ToGuid(), Env.Instance.CurrentUserPK);
					AssertEquals(newBranch.PK.ToGuid(), Env.Instance.CurrentBranchPK);
					AssertEquals(newDepartment.PK.ToGuid(), Env.Instance.CurrentDepartmentPK);
				})
				.ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
				{
					Content = new StringContent("Success")
				});

			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), newBranch.PK.ToGuid(), newDepartment.PK.ToGuid()))
			using (ObjectFactory.Substitute(clientFactoryMock.Object))
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://GlowServiceUri.com"))
			{
				var result = webRequest.Post("test/endpoint", "json");
				AssertEquals("Success", result);
			}

			AssertNotEquals(newStaff.PK.ToGuid(), Env.Instance.CurrentUserPK);
			AssertNotEquals(newBranch.PK.ToGuid(), Env.Instance.CurrentBranchPK);
			AssertNotEquals(newDepartment.PK.ToGuid(), Env.Instance.CurrentDepartmentPK);
		}
	}
}
