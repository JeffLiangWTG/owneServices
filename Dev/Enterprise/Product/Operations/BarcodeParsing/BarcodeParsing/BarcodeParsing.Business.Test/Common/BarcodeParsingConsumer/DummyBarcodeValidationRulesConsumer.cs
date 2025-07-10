using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	public class DummyBarcodeValidationRulesConsumer : DummyBarcodeParsingConsumer, IBarcodeValidationRulesConsumer
	{
		public DummyBarcodeValidationRulesConsumer(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public string ValidateTargetFieldForValidationRules(ZString targetField) => TargetFieldValidationErrorForTest;
		public string TargetFieldValidationErrorForTest { get; set; }
	}
}
