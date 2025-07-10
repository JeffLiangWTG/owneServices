using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(DummyExporter))]
	public class DummyExporterTest : CMRDataExporterCSVTest
	{
		protected override CMRDataExporterCSV GetNewExporter()
		{
			return new DummyExporter(Factory.New<DummyBusinessObject>());
		}

		protected override string ExpectedCSVResult
		{
			get { return " ,Hello,Something with a comma and 'quotes'\r\n"; }
		}

		protected override string ExpectedFileName
		{
			get { return "ZUBIN"; }
		}

		protected override bool IsDocManagerSupported
		{
			get { return false; }
		}

		protected override string ExpectedMailSubject
		{
			get { return "Contingency Test Report"; }
		}

		protected override string ExpectedBodyText
		{ get { return "This is the body text"; } }
	}
}
