using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Telematics.ServiceTasks
{
	class EnrolmentAccessor : IEnrolmentAccessor
	{
		public IEnumerable<ClientTelRimRegistration> GetEnrollments(IFactory factory, DateTimeOffset enrollmentPeriodStart, DateTimeOffset enrollmentPeriodEnd)
		{
			_ = factory ?? throw new ArgumentNullException(nameof(factory));

			var query = new ZQuery(ClientTelRimRegistrationSchema.TRR_EndTime, null);
			query.AddToFilter(
				new ZQuery(ClientTelRimRegistrationSchema.TRR_EndTime, SQLComparisonOperator.GreaterThan, enrollmentPeriodEnd),
				JoinCondition.Or);
			query.AddToFilter(
				new ZQuery(
					new ZQuery(ClientTelRimRegistrationSchema.TRR_EndTime, SQLComparisonOperator.GreaterThanOrEqualTo, enrollmentPeriodStart),
					JoinCondition.And,
					new ZQuery(ClientTelRimRegistrationSchema.TRR_EndTime, SQLComparisonOperator.LessThan, enrollmentPeriodEnd)),
				JoinCondition.Or);
			query.AddToFilter(new ZQuery(ClientTelRimRegistrationSchema.TRR_StartTime, SQLComparisonOperator.LessThan, enrollmentPeriodEnd));

			return factory.Load<ClientTelRimRegistration>(query);
		}
	}
}
