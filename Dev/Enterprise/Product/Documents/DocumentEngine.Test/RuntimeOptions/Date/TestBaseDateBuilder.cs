using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class TestBaseDateBuilder : BaseDateBuilder<ZDateTime>
	{
		public TestBaseDateBuilder(BusinessObjectFactory factory)
			: base(new ValidatorPack(), factory, (x) => "", ReportRunningType.Report)
		{
		}

		protected override IReportDocumenter GetDateFilterDocumentation(List<string> supportedProperties)
		{
			throw new NotImplementedException();
		}

		public ZDateTime GetMacroDateReplacement_Exposed(string value)
		{
			return GetMacroDateReplacement(value);
		}

		public override bool CanBuild(string filterType)
		{
			throw new NotImplementedException();
		}

		protected override FilterField GetFilterField()
		{
			throw new NotImplementedException();
		}

		public string GetDateDefaultedOptionsNameStringForTest()
		{
			return base.GetDateDefaultedOptionsNameString();
		}
	}
}
