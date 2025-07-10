using System;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	[TestedType(typeof(PkgUnitXmlCodeMappings))]
	class PkgUnitXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		public void TestErrorNotAddedForUnknownPackageType()
		{
			NotificationBuffer notifications = new NotificationBuffer();
			string result = PkgUnitXmlCodeMappings.Instance.GetExternalCode("XXX", "", notifications);
			AssertNull(result);
			AssertEquals(false, notifications.HasErrors);

			result = PkgUnitXmlCodeMappings.Instance.GetEnterpriseCode("XXX", "", notifications);
			AssertNull(result);
			AssertEquals(false, notifications.HasErrors);
		}

		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get { return new Type[] { typeof(Core.Constants.PkgUnit) }; }
		}

		protected override bool EnterpriseAndExternalCodeShouldBeSame
		{
			get { return true; }
		}
	}
}
