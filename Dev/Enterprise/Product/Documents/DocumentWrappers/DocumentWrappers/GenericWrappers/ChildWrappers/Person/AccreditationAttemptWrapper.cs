using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Recruiter.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class AccreditationAttemptWrapper : GenericWrapper
	{
		public AccreditationAttemptWrapper(GlbAccreditationAttempt attempt, BusinessObjectFactory factory)
			: base(attempt, factory)
		{
			AttemptInfo = attempt ?? throw new ArgumentNullException(nameof(attempt));
		}

		public GlbAccreditationAttempt AttemptInfo { get; }
	}
}
