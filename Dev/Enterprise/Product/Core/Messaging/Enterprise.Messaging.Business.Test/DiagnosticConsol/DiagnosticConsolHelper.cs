using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Messaging.Business.Testing
{
	public sealed class DiagnosticConsolHelper : DiagnosticConsol
	{
		public DiagnosticConsolHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZGuid CreateTestMessage(BusinessObjectFactory testMessageFactory, ZString consolKey)
		{
			throw new NotImplementedException();
		}

		public override List<RegistryAndCertificateCheckResult> RegistryAndCertificateChecks()
		{
			throw new NotImplementedException();
		}

		public override ZString ProcessTimerTick(out ZString diagnosticConsolAction)
		{
			throw new NotImplementedException();
		}
	}
}
