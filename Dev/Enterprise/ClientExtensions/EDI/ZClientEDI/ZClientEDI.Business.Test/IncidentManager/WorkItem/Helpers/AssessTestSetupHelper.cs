using System;
using System.Linq;
using System.Net;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Moq;
using WTG.DevTools.ServiceClient.Assess;
using WTG.DevTools.ServiceClient.Common;

namespace ZClientEDI.Business.Test
{
	public static class AssessTestSetupHelper
	{
		public static IDisposable SetupForEnsureEnrolled(GlbStaff staff, params Guid[] aspectsToEnrol)
		{
			var serviceClientMock = new Mock<IAssessServiceClient>();
			var personPK = staff.GS_PER.ToGuid();
			foreach (var aspectPK in aspectsToEnrol)
			{
				serviceClientMock
					.Setup(c => c.EnsureEnrolledAsync(aspectPK, personPK, It.IsAny<EnrolmentParameters>(), It.IsAny<ServiceRequestOptions>()))
					.ReturnsAsync(ServiceResponse.Success(HttpStatusCode.OK));
			}
			return ObjectFactory.Substitute(serviceClientMock.Object);
		}

		public static IDisposable SetupHasCompletedLearningUnits(GlbStaff staff, params (Guid aspectPK, bool hasPassed)[] hasPassedLearningUnits)
		{
			return SetupHasCompletedLearningUnits(staff, hasPassedLearningUnits.Select(x => (x.aspectPK, ServiceResponse<bool>.Success(HttpStatusCode.OK, x.hasPassed))).ToArray());
		}

		public static IDisposable SetupHasCompletedLearningUnits(GlbStaff staff, params (Guid aspectPK, ServiceResponse<bool> serviceResponse)[] serviceResponses)
		{
			var serviceClientMock = new Mock<IAssessServiceClient>();
			var personPK = staff.GS_PER.ToGuid();
			foreach (var (aspectPK, response) in serviceResponses)
			{
				serviceClientMock
					.Setup(c => c.HasPassedLearningUnitAsync(aspectPK, personPK, It.IsAny<ServiceRequestOptions>()))
					.ReturnsAsync(response);
			}
			return ObjectFactory.Substitute(serviceClientMock.Object);
		}

		public static Mock<IAssessServiceClient> SetupHasCompletedLearningUnit(this Mock<IAssessServiceClient> serviceClientMock, GlbStaff staff, Guid aspectPK, bool hasPassed)
		{
			return SetupHasCompletedLearningUnit(serviceClientMock, staff, aspectPK, ServiceResponse<bool>.Success(HttpStatusCode.OK, hasPassed));
		}

		public static Mock<IAssessServiceClient> SetupHasCompletedLearningUnit(this Mock<IAssessServiceClient> serviceClientMock, GlbStaff staff, Guid aspectPK, ServiceResponse<bool> response)
		{
			var personPK = staff.GS_PER.ToGuid();
			serviceClientMock
				.Setup(c => c.HasPassedLearningUnitAsync(aspectPK, personPK, It.IsAny<ServiceRequestOptions>()))
				.ReturnsAsync(response);
			return serviceClientMock;
		}

		public static Mock<IAssessServiceClient> SetupGetLearningUnitName(this Mock<IAssessServiceClient> serviceClientMock, Guid aspectPK, string name)
		{
			return SetupGetLearningUnitName(serviceClientMock, aspectPK, ServiceResponse<string>.Success(HttpStatusCode.OK, name));
		}

		public static Mock<IAssessServiceClient> SetupGetLearningUnitName(this Mock<IAssessServiceClient> serviceClientMock, Guid aspectPK, ServiceResponse<string> response)
		{
			serviceClientMock
				.Setup(c => c.GetLearningUnitNameAsync(aspectPK, It.IsAny<ServiceRequestOptions>()))
				.ReturnsAsync(response);
			return serviceClientMock;
		}

		public static Mock<IAssessServiceClient> SetupGetLearningUnitUrl(this Mock<IAssessServiceClient> serviceClientMock, Guid aspectPK, string url)
		{
			return SetupGetLearningUnitUrl(serviceClientMock, aspectPK, ServiceResponse<string>.Success(HttpStatusCode.OK, url));
		}

		public static Mock<IAssessServiceClient> SetupGetLearningUnitUrl(this Mock<IAssessServiceClient> serviceClientMock, Guid aspectPK, ServiceResponse<string> response)
		{
			serviceClientMock
				.Setup(c => c.GetLearningUnitUrlAsync(aspectPK, It.IsAny<ServiceRequestOptions>()))
				.ReturnsAsync(response);
			return serviceClientMock;
		}

		public static Mock<IAssessServiceClient> SetupEnsureEnrolled(this Mock<IAssessServiceClient> serviceClientMock, GlbStaff staff, Guid aspectPK)
		{
			return SetupEnsureEnrolled(serviceClientMock, staff, aspectPK, ServiceResponse.Success(HttpStatusCode.OK));
		}

		public static Mock<IAssessServiceClient> SetupEnsureEnrolled(this Mock<IAssessServiceClient> serviceClientMock, GlbStaff staff, Guid aspectPK, ServiceResponse response)
		{
			var personPK = staff.GS_PER.ToGuid();
			serviceClientMock
				.Setup(c => c.EnsureEnrolledAsync(aspectPK, personPK, It.IsAny<EnrolmentParameters>(), It.IsAny<ServiceRequestOptions>()))
				.ReturnsAsync(response);
			return serviceClientMock;
		}
	}
}
